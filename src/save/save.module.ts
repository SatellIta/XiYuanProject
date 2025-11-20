import { Module, forwardRef } from '@nestjs/common';
import { SaveController } from './save.controller';
import { ChatModule } from 'src/chat/chat.module';
import { SaveService } from './save.service';

@Module({
  imports: [forwardRef(() => ChatModule)],
  controllers: [SaveController],
  providers: [SaveService],
  exports: [SaveService],
})

export class SaveModule {}