import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { ValidationPipe } from '@nestjs/common';

// 创建应用的主循环
async function bootstrap() {
  const app = await NestFactory.create(AppModule, { abortOnError: false }); // 禁用abortOnError选项允许抛出应用初始化的错误

  app.useGlobalPipes(new ValidationPipe()); // 启用全局验证管道

  await app.listen(process.env.PORT ?? 3000);  // 开放端口3000或环境变量中指定的端口
}
bootstrap();
