import { Controller, Get, Post, Body, Param } from '@nestjs/common';
import { Chat } from 'interfaces/chat';
import { ChatService } from './chat.service';
import { get } from 'utils/cfg';

@Controller('chats')
export class ChatController {
  constructor(private chatsService: ChatService) {}

  // 默认页面展示帮助信息
  @Get()
  async help(): Promise<string[] | string> {
    const helpText = get('chat_help') as string[];
    return helpText.join('\n');  // 暂时先这样
  }

  // 用于初始化一个新对话的POST方法
  // 客户端需要提供对话ID
  // 客户端可以提供提示词名称，否则使用默认提示词
  @Post('new')
  async newChat( 
    @Body('chatId') chatId: string,
    @Body('systemPromptName') systemPromptName: string,
  ): Promise<string> {
    if (!systemPromptName) systemPromptName = 'prompt_default';
    const systemPromt = get(`system_prompt.${systemPromptName}`);
    const response = await this.chatsService.newChatOnlySystem(chatId, systemPromt); 

    return response;
  }

  // 用于连接游戏，系统给出第一条消息
  // 客户端需要提供对话ID，第一条用户消息
  // 客户端可以指定提示词名称，否则使用默认提示词
  @Post('newgame')
  async newGameChat(
    @Body('chatId') chatId: string,
    @Body('body') message: string,
    @Body('systemPromptName') systemPromptName: string,
  ): Promise<string> {
    if (!systemPromptName) systemPromptName = 'prompt_default';
    const systemPromt = get(`system_prompt.${systemPromptName}`);
    const response = await this.chatsService.newChat(chatId, systemPromt, message);

    return response;
  }

  // 用于继续一个已有对话的POST方法
  // 客户端需要提供对话ID, 一条用户消息
  @Post('continue')
  async continueChat(
    @Body('chatId') chatId: string,
    @Body('body') message: string,
  ): Promise<string> {
    const response = await this.chatsService.continueChat(chatId, message);
    
    return response;
  }

  // 用于诊断一个已有对话的POST方法
  @Get('diagnose/:chatId')
  async diagnoseChat(
    @Param('chatId') chatId: string,
  ): Promise<string> {
    const systemPrompt = get('system_prompt.prompt_2');
    const response = await this.chatsService.diagnoseChat(chatId, systemPrompt);

    // 如果对话不存在，response将是null
    if (response === null) {
      return `${chatId} 对话不存在！`;
    }

    return response;
  }

  // 用于查询对话是否存在的GET方法
  @Get('exists/:chatId')
  async chatExists(
    @Param('chatId') chatId: string,
  ): Promise<boolean> {
    const exists = await this.chatsService.chatExists(chatId);

    return exists;
  }

  // 用于获取对话的聊天记录的GET方法
  @Get('history/:chatId')
  async getChatHistory(
    @Param('chatId') chatId: string,
  ): Promise<Chat[] | string> {
    const history = await this.chatsService.getChatHistory(chatId);

    // 如果对话不存在，history将是null
    if (history === null) {
      return `${chatId} 对话不存在！`;
    }

    return history;
  }

  // 和列出所有对话的GET方法一样
  @Get('history')
  async getHistory(): Promise<string[]> {
    const response = await this.chatsService.listChats();

    return response;
  }

    // 以下是用于调试的接口
    // 用于接收测试请求的POST方法
  @Post('test')
  async test(@Body('length') length: number): Promise<Chat[]> {
    const response = await this.chatsService.testChat(length);

    return response;
  }

  // 列出所有对话的GET方法
  @Get('list')
  async listChats(): Promise<string[]> {
    const response = await this.chatsService.listChats();

    return response;
  }

  // 删除一个对话的POST方法
  @Post('end')
  async endChat(
    @Body('chatId') chatId: string,
  ): Promise<string> {
    const response = await this.chatsService.endChat(chatId);

    return response;
  }
}