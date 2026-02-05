# MoShou - 魔兽小游戏

一款基于Unity开发的移动端RPG游戏，采用全自动化AI开发工作流。

## 项目概述

- **项目名称**: MoShou (魔兽小游戏)
- **开发引擎**: Unity 2022.3 LTS
- **目标平台**: Android / WebGL
- **开发模式**: AI驱动的全自动化开发 (V1.2架构)

## 功能特性

### 核心战斗系统
- 虚拟摇杆控制
- 自动普通攻击
- 技能系统 (3个技能槽位)
- 怪物AI (追击/攻击)
- 伤害计算 (暴击/防御)

### 装备系统
- 6个装备槽位 (武器/护甲/头盔/靴子/戒指/项链)
- 装备品质 (白/绿/蓝/紫/橙)
- 装备属性加成

### 背包系统
- 30格背包容量
- 物品堆叠
- 物品使用/穿戴

### 掉落系统
- 怪物死亡掉落
- 金币/经验/物品
- 自动拾取

### 存档系统
- PlayerPrefs本地存档
- 自动保存
- 备份恢复

### UI系统
- 主界面HUD
- 背包界面
- 装备界面
- 伤害飘字

## 项目结构

```
MoShou/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # 核心系统
│   │   ├── Data/           # 数据类
│   │   ├── Gameplay/       # 游戏逻辑
│   │   ├── Combat/         # 战斗系统
│   │   ├── Systems/        # 管理器系统
│   │   ├── UI/             # UI组件
│   │   └── Test/           # 测试脚本
│   ├── Editor/             # 编辑器扩展
│   ├── Scenes/             # 场景文件
│   └── Resources/          # 资源文件
├── .task/                  # 任务追踪
└── Builds/                 # 构建输出
```

## 快速开始

### 环境要求
- Unity 2022.3 LTS 或更高版本
- Android Build Support (打包APK)
- WebGL Build Support (可选)

### 打开项目
1. 克隆仓库
2. 使用Unity Hub打开 `MoShou` 文件夹
3. 等待Unity导入所有资源

### 构建APK
在Unity编辑器中:
- 菜单 → MoShou → Build → Android APK

或使用命令行:
```bash
Unity -batchmode -projectPath ./MoShou -executeMethod MoShou.Editor.BuildScript.BuildAndroid
```

## 开发工作流

本项目采用V1.2自动化工作流架构:

1. **Claude Code** - AI开发者，负责代码生成和提交
2. **n8n** - 事件监听和状态记录
3. **Notion** - 知识库和任务管理

### Git提交规范
- `[MS-XXX]` - 任务开发提交
- `[TASK_COMPLETE]` - 任务完成标记
- `[FIX]` - 修复提交

## 任务进度

| 任务ID | 标题 | 状态 |
|--------|------|------|
| MS-001 | Unity项目初始化 | ✅ 完成 |
| MS-002 | 核心战斗系统 | ✅ 完成 |
| MS-003 | P1功能开发 | ✅ 完成 |
| MS-004 | UI完善与集成 | ✅ 完成 |
| MS-005 | 测试验证 | ✅ 完成 |
| MS-006 | APK打包配置 | ✅ 完成 |

## 测试

在Unity中运行测试场景:
1. 打开 TestScene
2. 挂载 TestSceneSetup 组件
3. 运行后查看Console输出

## 许可证

私有项目 - 仅供学习和研究使用

## 联系方式

- GitHub: https://github.com/skyofzhang/MS
