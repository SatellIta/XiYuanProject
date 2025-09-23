import { Controller, Post, Body, Get, Param } from '@nestjs/common';
import { PromptService } from './prompt.service';
import { get } from 'utils/cfg';

@Controller('prompts')
export class PromptController {
  constructor(private promptService: PromptService) {}

  // 默认页面展示帮助信息
  @Get()
  async help(): Promise<string[] | string> {
    const helpText = get('prompt_help') as string[];
    return helpText.join('\n');  // 暂时先这样
  }

  // 获取所有提示词
  @Get('list')
  async getAllPrompts(): Promise<Record<string, string>> {
    return this.promptService.getAllPrompts();
  }

  // 获取指定名称的提示词
  @Get(':name')
  async getPromptByName(
    @Param('name') name: string
  ): Promise<string> {
    const prompt = this.promptService.getPromptByName(name);
    if (prompt) {
      return prompt;
    }
    return `名称为 ${name} 的系统提示词不存在！`;
  }

  // 设置或更新指定名称的提示词
  @Post('set')
  async setPromptByName(
    @Body('systemPromptName') name: string,
    @Body('prompt') prompt: string,
  ): Promise<string> {
    const success = this.promptService.setPromptByName(name, prompt);
    if (success) {
      return `名称为 ${name} 的系统提示词已成功设置/更新！`;
    }
    return `名称为 ${name} 的系统提示词设置/更新失败！`;
  }

  // 删除指定名称的提示词
  @Post('delete')
  async deletePromptByName(
    @Body('systemPromptName') name: string,
  ): Promise<string> {
    const success = this.promptService.deletePromptByName(name);
    if (success) {
      return `名称为 ${name} 的系统提示词已成功删除！`;
    }
    return `名称为 ${name} 的系统提示词删除失败！`;
  }
}