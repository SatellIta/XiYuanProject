import { Module } from '@nestjs/common';
import { HistoryService } from './history.service';
import { RedisModule } from 'src/redis/redis.module';

@Module({
  imports: [RedisModule],
  providers: [HistoryService],
  exports: [HistoryService, RedisModule],
})
export class HistoryModule {}