import { Controller, Get, Post, Param, Header, Body, Redirect, Query } from '@nestjs/common';

@Controller('cats')
export class CatsController {
    
    @Post()
    @Header('Cache-Control', 'no-cache')  // 添加自定义响应头
    create(): string {
        console.log('create收到post请求\n');
        return `这个动作添加一个新的猫`;
    }

    @Get()
    findAll(): string {
        return `这个动作返回所有的猫`;
    }

    // 通过路径参数获取指定猫
    @Get('any/*path')
    findAny(@Param('path') path: string): string {
        return `这个动作返回任意指定${path}的猫`;
    }

    // 重定向
    @Get('trick')
    @Redirect('https://nestjs.com', 301)
    getDocs() {
        // 这里可以有一些逻辑
        return { url: 'https://docs.nestjs.com' };
    }


    @Get('ID/:anyId')  // 这里:anyId是一个占位符，可以是任意名称
    findOne(@Param('anyId') id: number): string {
        return `这个动作返回id为 ${id} 的猫`;
    }

    // 使用DTO创建猫
    @Post('dto')
    async createCat(@Body('body') createCatDto : any) {
        // 这里通常会调用一个服务来处理创建逻辑
        console.log('createCat收到post请求，内容为：', createCatDto);
        return `创建了一只名为 ${createCatDto.name?? '未知名称' }，年龄为 ${createCatDto.age?? 0}，品种为 ${createCatDto.breed?? '未知品种' } 的猫`;
    }

    // 查询特定年龄和品种的猫
    @Get('search')
    async findCats(@Query('age') age: number, @Query('breed') breed: string) {
        return `返回所有年龄为 ${age?? '任意年龄'}，品种为 ${breed?? '任意品种'} 的猫`;
    }


}