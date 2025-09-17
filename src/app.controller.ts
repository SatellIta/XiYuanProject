import { Controller, Get, Post, Param, Body } from '@nestjs/common';
import { AppService } from './app.service';

/*
@Controller()
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get()
  getHello(): string {
    return this.appService.getHello();
  }
}
*/

// 测试用控制器
@Controller('test')
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get()
  findAllUser(): string {
    return `假装返回了所有用户的列表`;
  }

  @Get(':id')
  findSpecicalUser(@Param('id') id: string): string {
    return `假装返回了用户 ${id} 的信息`;
  }
}
