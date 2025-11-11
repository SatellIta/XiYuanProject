import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { RedisModule } from './redis/redis.module';
import { ChatModule } from './chat/chat.module';
import { SaveModule } from './save/save.module';

@Module({
  imports: [RedisModule, ChatModule, SaveModule],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}
