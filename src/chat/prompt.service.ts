import { Injectable } from '@nestjs/common';
import { set, get, remove } from 'utils/cfg';

// 提示词服务类，用于处理提示词相关的逻辑
@Injectable()
export class PromptService {

  // 获取所有提示词的键值对
  getAllPrompts(): Record<string, string> {
    const prompts = get('system_prompt', {}) as Record<string, string>;
    return prompts;
  }
  
  // 获取指定名称的提示词
  // 作为通用方法，应该返回能让其他方法判断是否成功的值
  // 因此失败会返回null
  getPromptByName(name: string): string | null {
    const prompt = get(`system_prompt.${name}`);
    if (prompt) {
      return prompt;
    }
    return null;
  }

  // 设置或更新指定名称的提示词
  // 成功返回true，失败返回false
  // 这里使用try-catch来触发失败，是因为调用get获取文件中的写入操作成功与否，会消耗大量系统资源
  // 不如直接尝试写入，失败则捕获异常
  setPromptByName(name: string, prompt: string): boolean {
    try{
      const prompts = get('system_prompt', {}) as Record<string, string>;
      prompts[name] = prompt;
      set('system_prompt', prompts);
      return true;
    } catch (error) {
      return false;
    }
  }

  // 删除指定名称的提示词
  // 成功返回true，失败返回false
  deletePromptByName(name: string): boolean {
    try {
      remove(`system_prompt.${name}`);
      return true;
    } catch (error) {
      return false;
    }
  }
}