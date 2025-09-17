import { Chat } from "../interfaces/chat";

// 去掉消息中的 role 字段，返回纯文本内容
export function removeRole(message: Chat): string {
  return message.content;
}

