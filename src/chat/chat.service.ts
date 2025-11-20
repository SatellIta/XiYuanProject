import { Injectable, Inject, forwardRef } from '@nestjs/common';
import { Chat } from 'interfaces/chat';
import { SaveData } from 'interfaces/save';
import { sendMessage } from 'utils/openai';
import { removeRole } from 'utils/message';
import { get } from 'utils/cfg';
import { HistoryService } from './history.service';
import { SaveService } from 'src/save/save.service';
import { StrategyFactory } from './strategies/strategy.factory';

// 聊天服务类，负责处理聊天相关的业务逻辑
// 需要在这里处理与ai通信的逻辑
@Injectable()
export class ChatService {
  // 导入聊天记录服务
  constructor(
    private historyService: HistoryService,
    @Inject(forwardRef(() => SaveService))
    private saveService: SaveService,
    private strategyFactory: StrategyFactory,
  ) {}

  // 初始化一个新对话, 需要指定系统提示词, 和第一条用户发言, 返回第一条回复
  async newChat(chatId: string, systemPromt: string, userMsg: string): Promise<string> {
    const messages: Chat[] = [];
    // 构建系统消息
    const systemMessage: Chat = {
      role: 'system',
      content: systemPromt,
    }
    // 构建用户消息
    const userMessage: Chat = {
      role: 'user',
      content: userMsg,
    }
    messages.push(systemMessage, userMessage);
    const inialResponse = await sendMessage(messages);
    messages.push(inialResponse);
    // 保存初始的系统消息和AI回复到聊天记录中, saveChatHistory会覆写原有的聊天记录
    await this.historyService.saveChatHistory(chatId, messages);

    return removeRole(inialResponse);
  }

  // 另一个新建对话的方法，区别是不附带第一条用户信息
  async newChatOnlySystem(chatId: string, systemPrompt: string): Promise<string> {
    const messages: Chat[] = [];
    const systemMessage: Chat = {
      role: 'system',
      content: systemPrompt,
    };
    messages.push(systemMessage);
    const initialResponse = await sendMessage(messages);
    messages.push(initialResponse);
    await this.historyService.saveChatHistory(chatId, messages);

    return removeRole(initialResponse);
  }

  // 继续一个已有的对话, 需要指定对话ID和用户消息, 返回AI的回复
  // 根据存档中的疗程阶段选择不同的策略处理消息
  async continueChat (chatId: string, userMessage: string): Promise<string> {
    const saveData = await this.saveService.load(chatId);
    if (!saveData) return '存档不存在。';

    const history = await this.historyService.getChatHistory(chatId);
    const sessionNumber = saveData.completedTherapySession + 1;
    const strategy = this.strategyFactory.getStrategy(sessionNumber);

    if (!strategy) {
      return '当前疗程阶段的逻辑未实现。';
    }

    const responseContent = await strategy.handleMessage(history, userMessage, saveData);


    return responseContent;
  }

  // 当用户确认问题或选择解决方案后，调用此方法
  async confirmStage(chatId: string, payload: { problem?: string; solution?: string }): Promise<{ success: boolean; message: string }> {
    const saveData = await this.saveService.load(chatId);
    if (!saveData) return { success: false, message: '存档不存在。' };

    const sessionNumber = saveData.completedTherapySession + 1;
    const strategy = this.strategyFactory.getStrategy(sessionNumber);

    if (strategy && strategy.processStageTransition) {
      return strategy.processStageTransition(payload, saveData);
    }

    return { success: false, message: '当前阶段不支持此操作。' };
  }

  // 诊断一个已有的对话, 需要指定对话ID和系统提示词, 返回AI的回复
  // 如果找不到聊天记录，则返回null
  async diagnoseChat(chatId: string, systemPrompt: string): Promise<string | null>{
    if (await this.historyService.chatExists(chatId)) {
      const history = await this.historyService.getChatHistory(chatId);
      // 构建系统消息
      const systemMessage: Chat = {
        role: 'system',
        content: systemPrompt,
      };
      // 组织聊天记录
      const messages: Chat[] = [systemMessage, ...history.slice(1)];  // slice(1)去掉最初的系统提示词
      const response = await sendMessage(messages);
      return removeRole(response);
    }

    return null;
  }

  // 结束一个对话
  async endChat(chatId: string): Promise<string> {
    this.historyService.deleteChatHistory(chatId);
    return `对话${chatId}已结束, 聊天记录已删除`;
  }

  // 以下都是调试函数
  // 查询对话是否存在
  async chatExists(chatId: string): Promise<boolean> {
    return this.historyService.chatExists(chatId);
  }

  // 查询对话的聊天记录
  async getChatHistory(chatId: string): Promise<Chat[] | null>  {
    if (await this.historyService.chatExists(chatId)) {
      return this.historyService.getChatHistory(chatId);
    }
  
    return null;
  }

  // 测试聊天功能，接收聊天轮数并返回Ai间的聊天记录
  async testChat( length: number ) : Promise<Chat[]> {
    // 分别获取用户和医生的提示词
    const userPrompt = get('user_prompt.prompt_3');
    const doctorPrompt = get('system_prompt.prompt_default');
    // 医生先说话
    let doctorMessage: string = await this.newChatOnlySystem('test_chat_doctor', doctorPrompt);
    let userMessage: string = await this.newChat('test_chat_user', userPrompt, doctorMessage);
    // 开始对话循环
    for (let i = 0; i < length; i++) {
      console.log(`第${i+1}轮对话`);
      doctorMessage = await this.continueChat('test_chat_doctor', userMessage);
      userMessage = await this.continueChat('test_chat_user', doctorMessage);
    }
    // 获取最后的聊天记录
    const userHistory = await this.getChatHistory('test_chat_user') as Chat[];  // 使用断言，因为这里返回的类型必须为Chat[]
    const diagnose = await this.diagnoseChat('test_chat_user', get('system_prompt.prompt_2'));
    // 清理测试对话
    await this.endChat('test_chat_user');
    await this.endChat('test_chat_doctor');
    console.log('测试对话已清理');
    // 将聊天记录和诊断结果合并返回
    userHistory.push({
      role: 'diagnose',
      content : diagnose as string,
    });

    return userHistory;
  }

  // 列出所有对话
  async listChats(): Promise<string[]> {
    const chats = await this.historyService.listChats();
    return chats;
  }

  // 根据疗程总结存档对话
  // 总结后会自动删除聊天记录
  // 返回总结消息
  async getTherapySummary(chatId: string, therapyState: number): Promise<string> {
    const history = await this.getChatHistory(chatId);
    if (history === null) {
      return `${chatId} 对话不存在，无法生成总结！`;
    }
    // 构建系统消息
    let systemPrompt = '';
    systemPrompt = get(`system_prompt.therapy_summary_${therapyState}`);
    const systemMessage: Chat = {
      role: 'system',
      content: systemPrompt
    };
    // 组织聊天记录
    const messages: Chat[] = [systemMessage, ...history.slice(1)];  // slice(1)去掉最初的系统提示词
    const response = await sendMessage(messages);
    return removeRole(response);
  }

  // 从存档加载游戏
  // 根据存档状态恢复对话或开启新疗程
  async loadGame(saveData: SaveData, userMessage?: string): Promise<string> {
    const { chatId, inChat, completedTherapySession, chatHistory } = saveData;

    if (inChat) {
      // 恢复聊天记录
      if (chatHistory) {
        await this.historyService.saveChatHistory(chatId, chatHistory);
      }
      
      // 如果有用户消息，继续对话
      if (userMessage) {
        return this.continueChat(chatId, userMessage);
      }
      
      // 否则返回最后一条 AI 消消息（如果有）
      if (chatHistory && chatHistory.length > 0) {
        const lastMsg : Chat = chatHistory[chatHistory.length - 1];
        return removeRole(lastMsg);
      }
      return "存档已加载，等待用户输入...";
    } else {
      // 不在对话中，开启新疗程
      const session = completedTherapySession + 1;
      // 尝试获取对应疗程的 prompt
      // 假设命名规则是 session_1, session_2...
      let promptKey = `system_prompt.session_${session}`;
      let systemPrompt = get(promptKey);
      
      if (!systemPrompt) {
          // 如果没有特定疗程的 prompt，使用默认的
          systemPrompt = get('system_prompt.prompt_default');
      }
      
      if (userMessage) {
        return this.newChat(chatId, systemPrompt, userMessage);
      } else {
        return this.newChatOnlySystem(chatId, systemPrompt);
      }
    }
  }

  // 重置会话到指定阶段
  async resetSession(chatId: string, sessionNumber: number): Promise<boolean> {
    const newSaveData = await this.saveService.resetToSession(chatId, sessionNumber);
    if (newSaveData) {
      // 清理 Redis 记录
      await this.historyService.deleteChatHistory(chatId);
      return true;
    }
    return false;
  }
}