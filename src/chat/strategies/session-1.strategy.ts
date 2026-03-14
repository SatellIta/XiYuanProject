import { Injectable, Inject, forwardRef } from '@nestjs/common';
import { ISessionStrategy } from 'interfaces/session-strategy.interface';
import { Chat } from 'interfaces/chat';
import { SaveData } from 'interfaces/save';
import { sendMessage } from 'utils/openai';
import { SaveService } from 'src/save/save.service';
import { HistoryService } from '../history.service';
import { removeRole } from 'utils/message';
import { get } from 'utils/cfg';

@Injectable()
export class Session1Strategy implements ISessionStrategy {
  constructor(
    private saveService: SaveService,
    private historyService: HistoryService,
  ) {}

  systemPrompt = get('system_prompt.session_1');
  prefix = get('system_prompt.prompt_prefix');
  finalSystemPrompt = this.prefix + this.systemPrompt;

  // 开启新疗程时，由AI发送第一条消息
  async startSession(saveData: SaveData): Promise<string> {
    const messages: Chat[] = [{ role: 'system', content: this.finalSystemPrompt }];
    
    const response = await sendMessage(messages);
    
    // 保存初始的系统消息和AI回复到历史记录
    await this.historyService.saveChatHistory(saveData.chatId, [messages[0], response]);

    return response.content;
  }

  async handleMessage(history: Chat[], userMessage: string, saveData: SaveData): Promise<string> {
    const userMsg: Chat = { role: 'user', content: userMessage };
    const messages = [{ role: 'system', content: this.finalSystemPrompt }, ...history, userMsg];
    
    const response = await sendMessage(messages);
    
    // 更新聊天记录
    await this.historyService.appendChatHistories(saveData.chatId, userMsg, response);

    // 由于提示词已强制 AI 返回 JSON，此处直接返回其内容
    return response.content;
  }

  async processStageTransition(response: { problem: string }, saveData: SaveData): Promise<{ success: boolean; message: string }> {
    if (!response.problem) {
      return { success: false, message: '缺少 problem 字段' };
    }

    // 更新存档
    saveData.problem = response.problem;
    saveData.inChat = false;
    saveData.completedTherapySession += 1;
    saveData.chatHistory = null;

    const success = await this.saveService.saveData(saveData);
    if (success) {
      await this.historyService.deleteChatHistory(saveData.chatId);
      return { success: true, message: '问题已确认，第一疗程结束。' };
    }
    
    return { success: false, message: '存档失败' };
  }
}
