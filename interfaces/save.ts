import { Chat } from '../interfaces/chat.js';

// 定义一个标准的存档格式
export interface SaveData {
  chatId: string;                     // 对话ID
  timestamp: number;                  // 存档时间戳
  completedTherapySession: number;    // 已完成的疗程数
  inChat: boolean;                    // 是否在对话中
  chatHistory: Chat[] | null;         // 聊天记录，如果在对话中则包含当前对话记录
  therapySummary: string[] | null;    // 疗愈阶段总结，一个阶段都没有结束则为空
}

