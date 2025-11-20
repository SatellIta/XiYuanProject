import { Injectable } from '@nestjs/common';
import { ISessionStrategy } from 'interfaces/session-strategy.interface';
import { Session1Strategy } from './session-1.strategy';
import { Session2Strategy } from './session-2.strategy';
import { Session3Strategy } from './session-3.strategy';

// 策略工厂，根据疗程阶段返回对应的策略实例
@Injectable()
export class StrategyFactory {
  private strategies: Map<number, ISessionStrategy> = new Map();

  constructor(
    private readonly session1: Session1Strategy,
    private readonly session2: Session2Strategy,
    private readonly session3: Session3Strategy,
  ) {
    this.strategies.set(1, this.session1);
    this.strategies.set(2, this.session2);
    this.strategies.set(3, this.session3);
  }

  getStrategy(sessionNumber: number): ISessionStrategy | undefined {
    return this.strategies.get(sessionNumber);
  }
}
