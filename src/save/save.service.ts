import { Inject, Injectable, forwardRef } from '@nestjs/common';
import { SaveData, PlayerState } from '../../interfaces/save';
import { ChatModule } from 'src/chat/chat.module';
import { Chat } from 'interfaces/chat';
import * as fs from 'fs';
import * as path from 'path';
// import { ChatService } from 'src/chat/chat.service';
import { HistoryService } from 'src/chat/history.service';
import { Console, log } from 'console';

const SAVE_PATH = "../../../saves/";

/*
  存档服务类，用于处理存档相关的业务逻辑
*/
@Injectable()
export class SaveService {
  constructor(
    private historyService: HistoryService,
  ) {}

  // 传入存档数据，保存存档
  // 这个saveData方法被session策略调用
  async saveData(saveData: SaveData): Promise<boolean> {
    try {
      const filePath = path.join(__dirname, SAVE_PATH, `${saveData.chatId}.json`);
      await fs.writeFileSync(filePath, JSON.stringify(saveData, null, 2), 'utf-8');
      console.log(`存档已保存到 ${filePath}`);
      return true;
    } catch (error) {
      console.log('保存存档时出错:', error);
      return false;
    }
  }

  // 读取存档，目前一个id只有一个存档
  async loadData(chatId: string): Promise<SaveData | null> {
    const filePath = path.join(__dirname, SAVE_PATH, `${chatId}.json`);
    if (!fs.existsSync(filePath)) {
      console.log(`存档文件 ${filePath} 不存在`);
      return null;
    }
    try {const data = await fs.readFileSync(filePath, 'utf-8'); 
    const saveData: SaveData = JSON.parse(data);
    console.log(`存档已从 ${filePath} 读取`);
    return saveData;
    } catch (error) {
      console.log('读取存档时出错:', error);
      return null;
    }
  }

  // 构造存档
  async constructSaveData(
    chatId: string, 
    completedTherapySession: number, 
    inChat: boolean, 
    chatHistory: Chat[] | null, 
    therapySummary: string[],
    problem: string | null,
    playerState: PlayerState,
    solution: string | null,
  ): Promise<SaveData> {
    const timestamp = Date.now();
    return {
      chatId,
      timestamp,
      completedTherapySession,
      inChat,
      chatHistory,
      therapySummary,
      problem,
      playerState,
      solution,
    };
  }

  // 获取构造存档的全部信息，返回构造好的存档，供控制器调用
  // 注意这个save功能没有被session策略调用
  async save(chatId: string, inChat: boolean = true, isNew: boolean = false): Promise<boolean> {
    // 读取当前存档
    const currentSave = isNew ? null : await this.loadData(chatId);

    let saveData: SaveData;

    if (currentSave) {
      // 更新现有存档
      const problem = currentSave.problem;
      const playerState = currentSave.playerState;
      const solution = currentSave.solution;
      let completedTherapySession = currentSave.completedTherapySession;
      let therapySummary = currentSave.therapySummary || [];
      let chatHistory = currentSave.chatHistory;

      if (inChat) {
        chatHistory = await this.historyService.getChatHistory(chatId);
      } else {
        completedTherapySession += 1;
        // const newSummary = await this.chatService.getTherapySummary(chatId, completedTherapySession);
        // therapySummary.push(newSummary);
        chatHistory = null; // 结束对话，清空记录
      }
      
      saveData = await this.constructSaveData(chatId, completedTherapySession, inChat, chatHistory, therapySummary, problem, playerState, solution);

    } else {
      // 创建新存档
      const playerState: PlayerState = {
        anxiety: 50, happiness: 50, stress: 50,
        energy: 50, trust: 0, resilience: 0
      };
      // For a new save, chat history should be null initially.
      const chatHistory = null;
      
      saveData = await this.constructSaveData(
        chatId,
        0, // completedTherapySession
        inChat, // Will be `false` when creating a new save from the controller
        chatHistory,
        [], // therapySummary
        null, // problem
        playerState,
        null, // solution
      );
    }
    
    return this.saveData(saveData);
  }

  // 供控制器调用，读取存档
  async load(chatId: string): Promise<SaveData | null> {
    return this.loadData(chatId);
  }

  // 检查存档是否存在
  async exists(chatId: string): Promise<boolean> {
    const filePath = path.join(__dirname, SAVE_PATH, `${chatId}.json`);
    return fs.existsSync(filePath);
  }

  // 重置存档到指定的疗程阶段
  async resetToSession(chatId: string, sessionNumber: number): Promise<SaveData | null> {
    const saveData = await this.loadData(chatId);
    if (!saveData) {
      return null;
    }

    // 根据要重置到的阶段，清除后续阶段的数据
    if (sessionNumber <= 1) {
      saveData.problem = null;
      saveData.solution = null;
    }
    if (sessionNumber <= 2) {
      saveData.solution = null;
    }

    saveData.completedTherapySession = sessionNumber - 1;
    saveData.inChat = false; // 标记为需要开启新对话
    saveData.chatHistory = null; // 清空历史

    await this.saveData(saveData);
    // 注意：重置存档时，也需要清理Redis中的聊天记录
    // 这个操作移动到 ChatService 中，因为它同时管理 SaveService 和 HistoryService
    // await this.historyService.deleteChatHistory(chatId);

    return saveData;
  }
}