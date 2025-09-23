import { Module } from '@nestjs/common';
import { ChatController } from './chat.controller';
import { PromptController } from './prompt.controller';
import { ChatService } from './chat.service';
import { PromptService } from './prompt.service';
import { RedisModule } from 'src/redis/redis.module';
import { HistoryService } from './history.service';

@Module({
  imports: [RedisModule],
  controllers: [ChatController, PromptController],
  providers: [ChatService, HistoryService, PromptService],
})

export class ChatModule {}
