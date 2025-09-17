import { Module } from '@nestjs/common';
import { Redis } from 'ioredis';

// 创建一个 Injection Token
export const REDIS_CLIENT = 'REDIS_CLIENT';

@Module({
  providers: [
    {
      provide: REDIS_CLIENT,
      useFactory: () => {
        // 配置 Redis 连接信息
        const redisInstance = new Redis({
          host: 'localhost',
          port: 6379,
        });

        redisInstance.on('error', (err) => {
          console.error('Redis connection error:', err);
        });

        redisInstance.on('connect', () => {
          console.log('Successfully connected to Redis!');
        });

        return redisInstance;
      },
    },
  ],
  // 导出 Provider，以便其他模块可以使用
  exports: [REDIS_CLIENT],
})
export class RedisModule {}