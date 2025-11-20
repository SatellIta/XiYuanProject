import { Controller, Get, Post, Body, Param } from '@nestjs/common';
import { SaveService } from './save.service';
import { SaveData } from '../../interfaces/save';

@Controller('saves')
export class SaveController {
  constructor(private saveService: SaveService) {}

  // 保存存档的POST方法
  // 用户需要提供chatId和inChat
  // inChat表示当前是否在对话中
  // 其他数据由服务端构造
  @Post('save')
  async save(
    @Body('chatId') chatId: string,
    @Body('inChat') inChat: boolean = true,
  ): Promise<string> {
    const success = await this.saveService.save(chatId, inChat);
    if (success) {
      return `存档已成功保存！`;
    }
    return `存档保存失败！`;
  }

  // 读取存档的GET方法
  // 用户需要提供chatId
  @Get('load/:chatId')
  async load(
    @Param('chatId') chatId: string,
  ): Promise<SaveData | string> {
    const saveData = await this.saveService.load(chatId);
    if (saveData) {
      return saveData;
    }
    return `未找到对应的存档！`;
  }

  // 创建新存档的POST方法
  @Post('new')
  async newSave(@Body('chatId') chatId: string): Promise<string> {
    const success = await this.saveService.save(chatId, false, true); // isNew = true
    if (success) {
      return `新存档创建成功！`;
    }
    return `存档创建失败！`;
  }

  // 以下是测试方法
  // 保存存档的GET方法
  @Get('testsave/:chatId')
  async testSave(
    @Param('chatId') chatId:string,
  ): Promise<string> {
    const dummySaveData: SaveData = {
      chatId,
      timestamp: Date.now(),
      completedTherapySession: 1,
      inChat: true,
      chatHistory:
        [{
          role: 'sysetem',
          content: '你是一家心理理疗机构的心理咨询师，你现在需要引导用户进行心理咨询，并尝试解决用户的心理问题。请不要急着结束诊疗，或者让用户下周再来见你。你需要在这次诊疗中尽可能多地了解用户的心理状态，并给出专业的建议。注意不要使用括号内的表示动作和神态的文字。',
        }, {
          role: 'user',
          content: '我最近总是感到很焦虑，不知道怎么办。',
        }],
      therapySummary: ['用户主要问题是焦虑情绪，需要通过认知行为疗法进行干预。'],
      problem: '焦虑症',
      solution: null,
      playerState: {
        anxiety: 80,
        happiness: 20,
        stress: 70,
        energy: 40,
        trust: 10,
        resilience: 5
      }
    };
    const success = await this.saveService.saveData(dummySaveData);
    if (success) {
      return `测试存档已成功保存！`;
    }
    return `测试存档保存失败！`;
  }

  // 读取存档的测试GET方法
  @Get('testload/:chatId')
  async testLoad(
    @Param('chatId') chatId: string,
  ): Promise<SaveData | string> {
    const saveData = await this.saveService.loadData(chatId);
    if (saveData) {
      return saveData;
    }
    return `未找到对应的测试存档！`;
  }
}