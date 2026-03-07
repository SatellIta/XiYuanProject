import { Injectable } from '@nestjs/common';
import { ISessionStrategy } from 'interfaces/session-strategy.interface';
import { Chat } from 'interfaces/chat';
import { SaveData } from 'interfaces/save';
import { sendMessage } from 'utils/openai';
import { HistoryService } from '../history.service';
import { get } from 'utils/cfg';
import { removeRole } from 'utils/message';
import { forEach, List } from 'lodash';

@Injectable()
export class Session3Strategy implements ISessionStrategy {
  constructor(private historyService: HistoryService) {}

  // 开启新疗程时，由AI发送第一条消息
  async startSession(saveData: SaveData): Promise<string> {
    // 对于第三疗程，开始逻辑与用户发送第一条消息类似，
    // 因此我们可以复用 handleMessage 方法，并传入一个通用的开场白。
    const initialMessage = "我们已经确认了解决方案，接下来我该做什么呢？";
    // 注意：此时历史记录可能是空的，所以我们从Redis获取
    const history = await this.historyService.getChatHistory(saveData.chatId);
    return this.handleMessage(history, initialMessage, saveData);
  }

  async handleMessage(history: Chat[], userMessage: string, saveData: SaveData): Promise<string> {
    // 判断用户状态
    const timeSinceLastSave = Date.now() - saveData.timestamp;
    const isReturningAfterBreak = timeSinceLastSave > 1000 * 60 * 60; // 超过1小时
    const isFirstMessageInSession = history.length <= 1;

    const isReturning = isReturningAfterBreak && isFirstMessageInSession;

    // 根据状态选择提示词的键
    const promptKey = (isReturningAfterBreak && isFirstMessageInSession)
      ? 'system_prompt.session_3_returning'
      : 'system_prompt.session_3';

    // 获取并格式化提示词
    let systemPrompt = get(promptKey);
    const levels: string[] = get('levels', []); // 从配置中读取关卡列表
    const levelDescriptions: string[] = get('level_descriptions', []); // 读取关卡描述
    let levelsString: string[] = levels.map((level, index) => {
      const description = levelDescriptions[index] ?? "";
      return `${level}，关卡描述${description}；`;
    });
  
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

    // 返回添加了 isReturning 字段的 JSON 
    const aiJson = JSON.parse(response.content);
    aiJson.is_returning = isReturning;
    return aiJson;
  }
}
