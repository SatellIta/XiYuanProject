import { Chat } from './chat';
import { SaveData } from './save';

export interface ISessionStrategy {
  /**
   * 处理用户的消息并返回 AI 的回复。
   * @param history 当前的聊天记录
   * @param userMessage 用户的最新消息
   * @param saveData 用户的存档数据
   * @returns AI 的回复或一个表示阶段结束的特殊信号（如 JSON 字符串）
   */
  handleMessage(history: Chat[], userMessage: string, saveData: SaveData): Promise<string>;

  /**
   * (可选) 当 AI 的回复触发了阶段转换时，调用此方法处理后续逻辑。
   * @param response AI 的回复
   * @param saveData 用户的存档数据
   * @returns 返回一个包含是否成功和消息的对象
   */
  processStageTransition?(response: any, saveData: SaveData): Promise<{ success: boolean; message: string }>;
}
