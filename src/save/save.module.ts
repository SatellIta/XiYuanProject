import { Module } from '@nestjs/common';
import { SaveController } from './save.controller';
import { ChatModule } from 'src/chat/chat.module';
import { ChatService } from '../chat/chat.service';
import { HistoryService } from 'src/chat/history.service';
import { SaveService } from './save.service';

@Module({
  imports: [ChatModule],
  controllers: [SaveController],
  providers: [ChatService, SaveService, HistoryService],
})

export class SaveModule {}