import { Chat } from '../interfaces/chat.js';

// 定义一个标准的存档格式
export interface SaveData {
  chatId: string;                     // 对话ID
  timestamp: number;                  // 存档时间戳
  completedTherapySession: number;    // 已完成的疗程数，默认为0
  inChat: boolean;                    // 是否在对话中
  chatHistory: Chat[] | null;         // 聊天记录，如果在对话中则包含当前对话记录
  therapySummary: string[] | null;    // 疗愈阶段总结，一个阶段都没有结束则为空
  playerState: PlayerState;           // 玩家心理状态
  problem: string | null;             // 诊断出的用户问题
  solution: string | null;            // 用户选择的解决方案
}

// 玩家心理状态接口
export interface PlayerState {
  anxiety: number;      // 焦虑值 (0-100)
  happiness: number;    // 愉悦值 (0-100)
  stress: number;       // 压力值 (0-100)
  energy: number;       // 活力/精神状态 (0-100)
  trust: number;        // 对AI疗愈师的信任度 (0-100)
  resilience: number;  // 心理韧性，长期成长指标 (0-100)
}