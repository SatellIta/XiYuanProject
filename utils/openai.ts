import OpenAI from "openai";
import { get } from "./cfg";

const openai = new OpenAI({
  // 读取配置文件中的 baseURL 和 apiKey
  baseURL: get('baseURL'),
  apiKey: get('apiKey')
});

// 发送消息到OpenAI并获取响应
// 没有作类型检查是因为OpenAI的类型定义不完善
/*
formattedMessages 的格式是：
[
  { role: 'system', content: '你是一个翻译助手' },
  { role: 'user', content: '你好' },
  { role: 'assistant', content: '你好！我能帮你什么吗？' },
  { role: 'user', content: '把这句话翻译成英文：今天天气真好' }
  // 可以继续添加聊天记录
]
*/
export async function sendMessage(formattedMessages: any[]) : Promise<any> {
  const response = await openai.chat.completions.create({
    messages: formattedMessages,
    model: get('model'),
    temperature: get('temperature'),
    max_tokens: get('max_tokens'),
  });

  return response.choices[0].message;
}