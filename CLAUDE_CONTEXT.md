# Claude 上下文记忆文件
> 这个文件是 Claude Code 的持久化记忆，每次新对话开始时请先读取此文件恢复上下文。
> 最后更新: 2026-02-06 00:00

## 项目基本信息
- **项目名称**: MS (魔兽小游戏)
- **项目路径**: E:\AI_Project\MS
- **Unity项目**: E:\AI_Project\MS\MoShou
- **GitHub仓库**: https://github.com/skyofzhang/MS
- **n8n服务器**: http://43.161.249.54:5678
- **参考知识库**: 小游戏知识库V3.0 (Notion)
- **美术资源库**: https://github.com/skyofzhang/ai-resources (待创建)

## 工作流架构 (V1.3)
**核心变化**: Claude + Cursor 双AI协作，通过.task/目录通信
**原因**: 解决"Unity验证断层"问题 - Claude写代码但无法验证Unity运行

### V1.3 角色分工
| 角色 | 职责 | 代码边界 |
|------|------|----------|
| Claude Code | 策划 + 主程序 + 验收 | Core/, Data/, Configs/ |
| Cursor | Unity执行者 | Gameplay/, Combat/, UI/, Systems/ |
| n8n | 事件监听 + 消息转发 | 工作流调度 |
| DeepSeek | 外部质检 | 防止Claude自检偷懒 |

### 通信协议 (.task/ 目录)
```
Claude写入 → current_task.json → Cursor读取执行
Cursor写入 → task_result.json → Claude读取验收
Cursor写入 → ask_claude.json → n8n转发 → claude_response.json
```

### Git Commit 标签
- `[NODE_COMPLETE]` - 任务完成，触发质检
- `[PROGRESS]` - 进度更新
- `[ASK_CLAUDE]` - Cursor有问题
- `[FIX]` - 修复提交
- `[WORKFLOW_UPDATE]` - 工作流变更

### n8n 工作流 (V1.3)
1. **NWF-01 Notion需求监听器** - 每5分钟检查需求池
2. **NWF-02 Git事件分发器** - GitHub Webhook入口
3. **NWF-03 状态记录器** - 记录到Notion
4. **NWF-05 Build Error Handler** - 编译错误处理
5. **NWF-06 工作流同步器** - [WORKFLOW_UPDATE]触发 (待部署)
6. **NWF-07 策划质检器** - DeepSeek外部质检 (待部署)

### 人类介入点 (仅3处)
1. **提需求** - 写入Notion需求池
2. **资源准备** - 提交美术资源到ai-resources
3. **体验验收** - 在手机上测试APK

## 开发进度

### 已完成任务
| 任务ID | 标题 | 完成时间 | 说明 |
|--------|------|----------|------|
| MS-001 | Unity项目初始化 | 2026-02-04 | 创建 MoShou 项目，配置 Android/WebGL |
| MS-002 | 核心战斗系统 | 2026-02-05 | 虚拟摇杆、自动普攻、技能、怪物AI |
| MS-003 | P1功能开发 | 2026-02-05 | 装备、存档、掉落、背包系统 |
| MS-004 | UI完善与集成 | 2026-02-05 | 背包UI、装备UI、HUD、伤害飘字 |
| MS-005 | 测试验证 | 2026-02-05 | TestSceneSetup测试脚本 |
| MS-006 | APK打包配置 | 2026-02-05 | BuildScript构建脚本 |
| MS-007 | 关卡选择场景 | 2026-02-05 | MainMenu、StageSelect、Loading |
| MS-008 | 游戏配置JSON | 2026-02-05 | 装备/物品/怪物/关卡配置 |

### 当前状态
**MS-009 Unity打包测试** - 需要用户在Unity中操作
- 创建场景 (MainMenu/StageSelect/GameScene)
- 打包APK进行测试

## 已创建的代码文件
```
MoShou/Assets/Scripts/
├── Core/
│   ├── GameManager.cs       # 游戏状态管理
│   ├── GameSceneSetup.cs    # 场景初始化
│   ├── GameInitializer.cs   # 系统初始化器
│   ├── MainMenuManager.cs   # 主菜单管理
│   ├── StageSelectManager.cs # 关卡选择
│   └── LoadingManager.cs    # 异步加载
├── Data/
│   ├── Equipment.cs         # 装备数据类
│   ├── PlayerStats.cs       # 玩家属性数据
│   └── ItemData.cs          # 物品/掉落数据
├── Gameplay/
│   ├── PlayerController.cs  # 玩家控制
│   ├── MonsterController.cs # 怪物AI
│   └── DropPickup.cs        # 掉落物拾取
├── Combat/
│   └── CombatSystem.cs      # 伤害计算
├── Systems/
│   ├── MonsterSpawner.cs    # 怪物生成器
│   ├── EquipmentManager.cs  # 装备管理
│   ├── SaveSystem.cs        # 存档系统
│   ├── LootManager.cs       # 掉落管理
│   └── InventoryManager.cs  # 背包管理
├── UI/
│   ├── VirtualJoystick.cs   # 虚拟摇杆
│   ├── UIManager.cs         # UI管理
│   ├── HealthBar.cs         # 血条组件
│   ├── InventoryPanel.cs    # 背包面板
│   ├── InventorySlotUI.cs   # 背包格子UI
│   ├── EquipmentPanel.cs    # 装备面板
│   ├── GameHUD.cs           # 主界面HUD
│   └── DamagePopup.cs       # 伤害飘字
├── Test/
│   └── TestSceneSetup.cs    # 测试场景设置
└── Editor/
    ├── BuildScript.cs       # 构建脚本
    └── BuildErrorReporter.cs # 编译错误上报

MoShou/Assets/Resources/Configs/
├── EquipmentConfigs.json    # 15件装备配置
├── ItemConfigs.json         # 11种物品配置
├── MonsterConfigs.json      # 8种怪物配置
├── StageConfigs.json        # 10个关卡配置
├── LootConfigs.json         # 掉落表配置
└── GameSettings.json        # 游戏设置
```

## 知识库参考
- **AI工作流文档**: https://www.notion.so/AI-2fdba9f99f5880369d98f3d69868b500
- **n8n工作流V1.2**: https://www.notion.so/2fdba9f99f5881c4872bfcea734bb8d4
- **R01美术资源清单**: https://www.notion.so/2ffba9f99f5881539bdbf83b75e85712

## 新对话启动指南
每次新对话开始时，Claude 应该：
1. 读取此文件 (`CLAUDE_CONTEXT.md`)
2. 读取 `.task/current_task.json` 获取当前任务状态
3. 读取 `.task/PROTOCOL.json` 了解通信协议
4. 如果有未完成任务，继续执行
5. 如果任务已完成，询问用户下一步计划

## 变更日志
- 2026-02-06 00:00: 升级到V1.3架构，添加Cursor协作协议
- 2026-02-05 20:30: MS-007/MS-008 完成，配置JSON文件创建
- 2026-02-05 19:15: 添加 NWF-05 编译错误处理器
- 2026-02-05 19:05: MS-001 ~ MS-006 全部完成
- 2026-02-05 17:30: 创建此文件
