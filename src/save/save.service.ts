import { Inject, Injectable } from '@nestjs/common';
import { SaveData } from '../../interfaces/save';
// import { ChatService } from 'src/chat/chat.service';
import { ChatModule } from 'src/chat/chat.module';
import { Chat } from 'interfaces/chat';
import * as fs from 'fs';
import * as path from 'path';
import { ChatService } from 'src/chat/chat.service';
import { Console, log } from 'console';

const SAVE_PATH = "../../../saves/";

/*
  存档服务类，用于处理存档相关的业务逻辑
*/
@Injectable()
export class SaveService {
  // 注入 ChatService 以获取聊天记录
  constructor(
    private chatService: ChatService,
  ) {}

  // 传入存档数据，保存存档
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
  async constructSaveData(chatId: string, completedTherapySession: number, inChat: boolean, chatHistory: Chat[] | null, therapySummary: string[]): Promise<SaveData> {
    const timestamp = Date.now();
    return {
      chatId,
      timestamp,
      completedTherapySession,
      inChat,
      chatHistory,
      therapySummary,
    };
  }

  // 获取构造存档的全部信息，返回构造好的存档，供控制器调用
  async save(chatId: string, inChat: boolean = true): Promise<boolean> {
    // 读取当前存档
    const currentSave = await this.loadData(chatId);
    if (inChat == true) {
      // 如果在对话中，则保留当前对话记录
      const chatHistory = await this.chatService.getChatHistory(chatId);
      const completedTherapySession = currentSave ? currentSave.completedTherapySession : 0;
      const therapySummary = currentSave ? currentSave.therapySummary || [] : [];
      const saveData = await this.constructSaveData(chatId, completedTherapySession, inChat, chatHistory, therapySummary);
      return this.saveData(saveData);
    } else {
      // 如果不在对话中，则清空对话记录
      const completedTherapySession = currentSave ? currentSave.completedTherapySession + 1 : 1;
      let therapySummary: string[] = [];
      if (currentSave && currentSave.therapySummary) {
        therapySummary = [...currentSave.therapySummary, await this.chatService.getTherapySummary(chatId, completedTherapySession)];
      } else {
        therapySummary = [await this.chatService.getTherapySummary(chatId, completedTherapySession)];
      }
      const chatHistory: any[] = [];
      const saveData = await this.constructSaveData(chatId, completedTherapySession, inChat, chatHistory, therapySummary);
      return this.saveData(saveData);
    }
  }

  // 供控制器调用，读取存档
  async load(chatId: string): Promise<SaveData | null> {
    return this.loadData(chatId);
  }
}