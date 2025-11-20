import { Injectable } from '@nestjs/common';
import { ISessionStrategy } from 'interfaces/session-strategy.interface';
import { Chat } from 'interfaces/chat';
import { SaveData } from 'interfaces/save';
import { sendMessage } from 'utils/openai';
import { HistoryService } from '../history.service';
import { get } from 'utils/cfg';
import { removeRole } from 'utils/message';

@Injectable()
export class Session3Strategy implements ISessionStrategy {
  constructor(private historyService: HistoryService) {}

  async handleMessage(history: Chat[], userMessage: string, saveData: SaveData): Promise<string> {
    // 判断用户状态
    const timeSinceLastSave = Date.now() - saveData.timestamp;
    const isReturningAfterBreak = timeSinceLastSave > 1000 * 60 * 60; // 超过1小时
    const isFirstMessageInSession = history.length <= 1;

    // 根据状态选择提示词的键
    const promptKey = (isReturningAfterBreak && isFirstMessageInSession)
      ? 'system_prompt.session_3_returning'
      : 'system_prompt.session_3';

    // 获取并格式化提示词
    let systemPrompt = get(promptKey);
    const levels = get('levels', []); // 从配置中读取关卡列表
    const levelsString = JSON.stringify(levels);

    systemPrompt = systemPrompt.replace('{problem}', saveData.problem || '未定义');
    systemPrompt = systemPrompt.replace('{solution}', saveData.solution || '未定义');
    systemPrompt = systemPrompt.replace('{levels}', levelsString); // 注入关卡列表

    // 构建消息并与 AI 交互
    const systemMsg: Chat = { role: 'system', content: systemPrompt };
    const userMsg: Chat = { role: 'user', content: userMessage };
    
    const messages = [systemMsg, ...history.slice(1), userMsg];
    const response = await sendMessage(messages);
    
    // 更新对话历史
    await this.historyService.appendChatHistories(saveData.chatId, userMsg, response);

    // 直接返回 AI 生成的 JSON 字符串
    return response.content;
  }
}
