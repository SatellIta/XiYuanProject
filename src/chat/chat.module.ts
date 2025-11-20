import { Module, forwardRef } from '@nestjs/common';
import { ChatController } from './chat.controller';
import { PromptController } from './prompt.controller';
import { ChatService } from './chat.service';
import { PromptService } from './prompt.service';
import { RedisModule } from 'src/redis/redis.module';
import { HistoryService } from './history.service';
import { SaveModule } from 'src/save/save.module';
import { StrategyFactory } from './strategies/strategy.factory';
import { Session1Strategy } from './strategies/session-1.strategy';
import { Session2Strategy } from './strategies/session-2.strategy';
import { Session3Strategy } from './strategies/session-3.strategy';

@Module({
  imports: [RedisModule, forwardRef(() => SaveModule)],
  controllers: [ChatController, PromptController],
  providers: [
    ChatService, 
    HistoryService, 
    PromptService,
    StrategyFactory,
    Session1Strategy,
    Session2Strategy,
    Session3Strategy
  ],
  exports: [ChatService, HistoryService, RedisModule],
})

export class ChatModule {}
