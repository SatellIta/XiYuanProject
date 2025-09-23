import { Injectable, Inject } from '@nestjs/common';
import { Chat } from 'interfaces/chat';
import { REDIS_CLIENT } from 'src/redis/redis.module';
import { Redis } from 'ioredis';
import { get } from 'utils/cfg';

const EXPIRE_TIME = get('history_expire_time') || 3 * 24 * 60 * 60; // 默认三天

@Injectable()
export class HistoryService {
  // 注入 Redis 客户端实例
  constructor(
    @Inject(REDIS_CLIENT) private readonly redisClient: Redis,
  ) {}

  // 以覆写形式保存聊天记录到 Redis, 以列表形式存储, 每条消息为一个元素
  async saveChatHistory(messageId: string, messages: Chat[]): Promise<void> {
    const key = `chat_history:${messageId}`;
    // 创建一个pipeline，用于执行连续的redis命令，而不需要每一步都等待服务器的响应
    const pipeline = this.redisClient.pipeline();
    // 清空现有的聊天记录
    pipeline.del(key);
    // 将新的消息数组添加到列表中
    messages.forEach((message) => {
      pipeline.rpush(key, JSON.stringify(message));
    });
    // 设置过期时间, 这里是3天
    pipeline.expire(key, EXPIRE_TIME);
    await pipeline.exec();
  }

  // 以添加形式将消息添加到聊天记录中
  async appendChatHistory(messageId: string, message: Chat): Promise<void> {
    const key = `chat_history:${messageId}`;
    const pipeline = this.redisClient.pipeline();
    pipeline.rpush(key, JSON.stringify(message));
    // 每次添加消息时，重置过期时间
    pipeline.expire(key, EXPIRE_TIME);
    await pipeline.exec();
  }

  // 支持一次性传入多条消息的添加
  async appendChatHistories(messageId: string, ...messages: Chat[]): Promise<void> {
    const key = `chat_history:${messageId}`;
    const pipeline = this.redisClient.pipeline();
    messages.forEach((message) => {
      pipeline.rpush(key, JSON.stringify(message));
    });
    // 每次添加消息时，重置过期时间
    pipeline.expire(key, EXPIRE_TIME);
    await pipeline.exec();
  }

  // 获取聊天记录
  async getChatHistory(messageId: string): Promise<Chat[]> {
    const key = `chat_history:${messageId}`;
    const messages = await this.redisClient.lrange(key, 0, -1);
    return messages.map((msg) => JSON.parse(msg));
  }

  // 删除聊天记录
  async deleteChatHistory(messageId: string): Promise<void> {
    const key = `chat_history:${messageId}`;
    await this.redisClient.del(key);
  }

  // 以下都是调试函数
  // 测试对话是否存在
  async chatExists(messageId: string): Promise<boolean> {
    const key = `chat_history:${messageId}`;
    const exists = await this.redisClient.exists(key);
    return exists === 1;  // redis.exists返回1表示存在，0表示不存在
  }

  // 列出所有对话ID
  async listChats(): Promise<string[]> {
    const keys = await this.redisClient.keys('chat_history:*');
    const userHistory = keys.map((key) => {
      return key.replace('chat_history:', '');
    });
    return userHistory;
  }
}