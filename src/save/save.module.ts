import { Module, forwardRef } from '@nestjs/common';
import { SaveController } from './save.controller';
import { ChatModule } from 'src/chat/chat.module';
import { HistoryModule } from 'src/chat/history.module';
import { SaveService } from './save.service';

@Module({
  imports: [HistoryModule],
  controllers: [SaveController],
  providers: [SaveService],
  exports: [SaveService],
})

export class SaveModule {}