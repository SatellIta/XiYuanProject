import { Injectable, Inject, forwardRef } from '@nestjs/common';
import { ISessionStrategy } from 'interfaces/session-strategy.interface';
import { Chat } from 'interfaces/chat';
import { SaveData } from 'interfaces/save';
import { sendMessage } from 'utils/openai';
import { SaveService } from 'src/save/save.service';
import { HistoryService } from '../history.service';
import { removeRole } from 'utils/message';

@Injectable()
export class Session1Strategy implements ISessionStrategy {
  constructor(
    @Inject(forwardRef(() => SaveService))
    private saveService: SaveService,
    private historyService: HistoryService,
  ) {}

  async handleMessage(history: Chat[], userMessage: string, saveData: SaveData): Promise<string> {
    const userMsg: Chat = { role: 'user', content: userMessage };
    const messages = [...history, userMsg];
    
    const response = await sendMessage(messages);
    
    // 更新聊天记录
    await this.historyService.appendChatHistories(saveData.chatId, userMsg, response);

    // 检查 AI 是否返回了问题总结 JSON
    try {
      const content = response.content.trim();
      if (content.startsWith('{') && content.endsWith('}')) {
        const jsonResponse = JSON.parse(content);
        if (jsonResponse.problem) {
          // 发现问题，直接返回 JSON 字符串给 ChatService，由它传递给前端
          return content;
        }
      }
    } catch (e) {
      // 不是 JSON，忽略
    }

    return removeRole(response);
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
