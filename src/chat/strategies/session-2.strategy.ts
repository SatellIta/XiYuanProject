import { Injectable, Inject, forwardRef } from '@nestjs/common';
import { ISessionStrategy } from 'interfaces/session-strategy.interface';
import { Chat } from 'interfaces/chat';
import { SaveData } from 'interfaces/save';
import { sendMessage } from 'utils/openai';
import { SaveService } from 'src/save/save.service';
import { HistoryService } from '../history.service';
import { get } from 'utils/cfg';

@Injectable()
export class Session2Strategy implements ISessionStrategy {
  constructor(
    // @Inject(forwardRef(() => SaveService))
    private saveService: SaveService,
    private historyService: HistoryService,
  ) {}

  async handleMessage(history: Chat[], userMessage: string, saveData: SaveData): Promise<string> {
    // 获取并格式化 session_2 的基础提示词
    let systemPrompt = get('system_prompt.session_2');
    if (saveData.problem) {
      systemPrompt = systemPrompt.replace('{problem}', saveData.problem);
    }

    // 构建发送给 AI 的消息
    const systemMsg: Chat = { role: 'system', content: systemPrompt };
    const userMsg: Chat = { role: 'user', content: userMessage };
    
    const messages = [systemMsg, ...history.slice(1), userMsg];

    // 发送消息并获取 AI 的 JSON 回复
    const response = await sendMessage(messages);
    
    // 更新完整的对话历史
    await this.historyService.appendChatHistories(saveData.chatId, userMsg, response);

    // 5. 直接返回 AI 的 JSON 字符串
    return response.content;
  }

  async processStageTransition(response: { solution: string }, saveData: SaveData): Promise<{ success: boolean; message: string }> {
    if (!response.solution) {
      return { success: false, message: '缺少 solution 字段' };
    }

    // 更新存档
    saveData.solution = response.solution;
    saveData.inChat = false;
    saveData.completedTherapySession += 1;
    saveData.chatHistory = null;

    const success = await this.saveService.saveData(saveData);
    if (success) {
      await this.historyService.deleteChatHistory(saveData.chatId);
      return { success: true, message: '解决方案已选定，第二疗程结束。' };
    }
    
    return { success: false, message: '存档失败' };
  }
}
