

---

## API 文档

本文档概述了用于与后端服务交互的主要 API 端点。

### 对话控制器 (`/chats`)

处理所有与对话和疗程相关的操作。

| 方法   | 端点                      | 描述                                                     | 请求体 / 参数                                                              |
| :----- | :------------------------ | :------------------------------------------------------- | :------------------------------------------------------------------------- |
| `POST` | `/chats/load`             | 从存档文件加载游戏，并获取当前疗程的 AI 初始消息。       | `{ "chatId": "string", "message"?: "string" }`                             |
| `POST` | `/chats/continue`         | 使用新的用户消息继续现有对话。                           | `{ "chatId": "string", "body": "string" }`                                 |
| `POST` | `/chats/confirm-stage`    | 确认问题或解决方案，以过渡到下一个治疗阶段。             | `{ "chatId": "string", "problem"?: "string", "solution"?: "string" }`      |
| `POST` | `/chats/reset-session`    | 将用户进度重置到指定疗程的开始。                         | `{ "chatId": "string", "sessionNumber": "number" }`                        |
| `GET`  | `/chats/history/:chatId`  | 检索指定对话的完整聊天记录。                             | `chatId` (URL 参数)                                                        |
| `GET`  | `/chats/history`          | 列出 Redis 中所有活跃对话的 ID。                         | -                                                                          |
| `POST` | `/chats/end`              | 从 Redis 中删除指定对话的聊天记录。                      | `{ "chatId": "string" }`                                                   |

### 存档控制器 (`/saves`)

管理用户进度的创建、保存和加载。

| 方法   | 端点                  | 描述                               | 请求体 / 参数            |
| :----- | :-------------------- | :--------------------------------- | :----------------------- |
| `POST` | `/saves/new`          | 为用户创建一个新的空存档文件。     | `{ "chatId": "string" }` |
| `POST` | `/saves/save`         | 保存用户会话的当前状态。           | `{ "chatId": "string" }` |
| `GET`  | `/saves/load/:chatId` | 检索指定用户的完整存档数据。       | `chatId` (URL 参数)      |

### 提示词控制器 (`/prompts`)

管理 AI 使用的系统提示词。

| 方法   | 端点               | 描述                               | 请求体 / 参数                                        |
| :----- | :----------------- | :--------------------------------- | :--------------------------------------------------- |
| `GET`  | `/prompts/list`    | 检索所有可用的系统提示词。         | -                                                    |
| `GET`  | `/prompts/:name`   | 按名称检索特定的提示词。           | `name` (URL 参数)                                    |
| `POST` | `/prompts/set`     | 设置或更新一个系统提示词。         | `{ "systemPromptName": "string", "prompt": "string" }` |
| `POST` | `/prompts/delete`  | 按名称删除一个系统提示词。         | `{ "systemPromptName": "string" }`                   |
