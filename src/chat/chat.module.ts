import { Module, forwardRef } from '@nestjs/common';
import { ChatController } from './chat.controller';
import { ChatService } from './chat.service';
import { SaveModule } from 'src/save/save.module';
import { HistoryModule } from './history.module';
import { StrategyFactory } from './strategies/strategy.factory';
import { Session1Strategy } from './strategies/session-1.strategy';
import { Session2Strategy } from './strategies/session-2.strategy';
import { Session3Strategy } from './strategies/session-3.strategy';

@Module({
  imports: [SaveModule, HistoryModule],
  controllers: [ChatController],
  providers: [
    ChatService,  
    StrategyFactory,
    Session1Strategy,
    Session2Strategy,
    Session3Strategy
  ],
  exports: [ChatModule],
})

export class ChatModule {}
