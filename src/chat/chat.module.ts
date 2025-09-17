import { Module } from '@nestjs/common';
import { ChatController } from './chat.controller';
import { ChatService } from './chat.service';
import { RedisModule } from 'src/redis/redis.module';
import { HistoryService } from './history.service';

@Module({
  imports: [RedisModule],
  controllers: [ChatController],
  providers: [ChatService, HistoryService],
})

export class ChatModule {}
