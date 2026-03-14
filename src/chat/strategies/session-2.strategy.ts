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
    private saveService: SaveService,
    private historyService: HistoryService,
  ) {}

  // 注意这里获取的提示词会在startSession中被替换掉{problem}
  // 之后在handleMessage中就不需要每次获取了，直接使用this.systemPrompt即可
  systemPrompt = get('system_prompt.session_2');
  prefix = get('system_prompt.prompt_prefix');
  finalSystemPrompt;

  // 开启新疗程时，由AI发送第一条消息
  async startSession(saveData: SaveData): Promise<string> {

    if (this.finalSystemPrompt == null) {
      this.finalSystemPrompt = this.systemPrompt.replace('{problem}', saveData.problem || '未定义');
      this.finalSystemPrompt = this.prefix + this.finalSystemPrompt;
    }
    
    console.log('Session 2 final system prompt:', this.finalSystemPrompt);
    
    const messages: Chat[] = [{ role: 'system', content: this.finalSystemPrompt }];
    const response = await sendMessage(messages);

    // 保存初始的系统消息和AI回复到历史记录
    await this.historyService.saveChatHistory(saveData.chatId, [messages[0], response]);

    return response.content;
  }

  async handleMessage(history: Chat[], userMessage: string, saveData: SaveData): Promise<string> {
    // 获取并格式化 session_2 的基础提示词
    if (this.finalSystemPrompt == null) {
      this.finalSystemPrompt = this.systemPrompt.replace('{problem}', saveData.problem || '未定义');
      this.finalSystemPrompt = this.prefix + this.finalSystemPrompt;
    }

    // 构建发送给 AI 的消息
    const systemMsg: Chat = { role: 'system', content: this.finalSystemPrompt };
    const userMsg: Chat = { role: 'user', content: userMessage };
    
    const messages = [systemMsg, ...history.slice(1), userMsg];

    // 发送消息并获取 AI 的 JSON 回复
    const response = await sendMessage(messages);
    
    // 更新完整的对话历史
    await this.historyService.appendChatHistories(saveData.chatId, userMsg, response);

    // 直接返回 AI 的 JSON 字符串
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
