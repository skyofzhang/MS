# Claude 上下文记忆文件
> 这个文件是 Claude Code 的持久化记忆，每次新对话开始时请先读取此文件恢复上下文。
> 最后更新: 2026-02-05 19:05

## 项目基本信息
- **项目名称**: MS (魔兽小游戏)
- **项目路径**: E:\AI_Project\MS
- **Unity项目**: E:\AI_Project\MS\MoShou
- **GitHub仓库**: https://github.com/skyofzhang/MS
- **n8n服务器**: http://43.161.249.54:5678
- **参考知识库**: 小游戏知识库V3.0 (Notion)

## 工作流架构 (V1.2)
**核心变化**: 移除Cursor，Claude Code统一承担策划+代码生成
**原因**: Cursor需要人工触发，无法实现全自动化

### Claude Code 的角色 (V1.2)
- **策划**: 读取Notion知识库，理解需求
- **开发**: 直接生成C#代码
- **质检**: 代码自检，确保可编译
- **提交**: git push 触发 n8n 记录进度

### n8n 工作流 (3个)
1. **NWF-01 Notion需求监听器** - 监听 Notion 需求池变更，通知 Claude
2. **NWF-02 Git事件分发器** - 监听 GitHub push，根据 commit tag 分发
3. **NWF-03 状态记录器** - 记录状态到 Notion

### Claude 的职责
- 理解需求 → 生成代码 → git push → n8n 自动记录
- 通过 `.task/` 目录与其他系统通信

## 开发进度

### 🎉 基础开发已完成!

| 任务ID | 标题 | 完成时间 | 说明 |
|--------|------|----------|------|
| MS-001 | Unity项目初始化 | 2026-02-04 | 创建 MoShou 项目，配置 Android/WebGL |
| MS-002 | 核心战斗系统 | 2026-02-05 | 虚拟摇杆、自动普攻、技能、怪物AI、伤害计算 |
| MS-003 | P1功能开发 | 2026-02-05 | 装备系统、存档系统、掉落系统、背包系统 |
| MS-004 | UI完善与集成 | 2026-02-05 | 背包UI、装备UI、HUD、伤害飘字、GameInitializer |
| MS-005 | 测试验证 | 2026-02-05 | TestSceneSetup测试脚本，系统集成测试 |
| MS-006 | APK打包配置 | 2026-02-05 | BuildScript构建脚本，README文档 |

### 当前状态
**等待用户指示** - 基础功能开发完成，可进行以下操作：
1. 在Unity中打开项目并测试
2. 构建APK进行真机测试
3. 继续开发新功能

## 已创建的代码文件
```
MoShou/Assets/Scripts/
├── Core/
│   ├── GameManager.cs      # 游戏状态管理
│   ├── GameSceneSetup.cs   # 场景初始化
│   └── GameInitializer.cs  # 系统初始化器
├── Data/
│   ├── Equipment.cs        # 装备数据类
│   ├── PlayerStats.cs      # 玩家属性数据
│   └── ItemData.cs         # 物品/掉落数据
├── Gameplay/
│   ├── PlayerController.cs # 玩家控制(移动/攻击/技能)
│   ├── MonsterController.cs # 怪物AI
│   └── DropPickup.cs       # 掉落物拾取
├── Combat/
│   └── CombatSystem.cs     # 伤害计算(暴击/防御)
├── Systems/
│   ├── MonsterSpawner.cs   # 怪物生成器
│   ├── EquipmentManager.cs # 装备管理
│   ├── SaveSystem.cs       # 存档系统
│   ├── LootManager.cs      # 掉落管理
│   └── InventoryManager.cs # 背包管理
├── UI/
│   ├── VirtualJoystick.cs  # 虚拟摇杆
│   ├── UIManager.cs        # UI管理
│   ├── HealthBar.cs        # 血条组件
│   ├── InventoryPanel.cs   # 背包面板
│   ├── InventorySlotUI.cs  # 背包格子UI
│   ├── EquipmentPanel.cs   # 装备面板(含EquipmentSlotUI)
│   ├── GameHUD.cs          # 主界面HUD
│   └── DamagePopup.cs      # 伤害飘字(含BillboardBehavior)
├── Test/
│   └── TestSceneSetup.cs   # 测试场景设置
└── Editor/
    └── BuildScript.cs      # 构建脚本
```

## 关键凭证
> 凭证存储在本地 `.env` 文件或对话上下文中，不提交到 Git
> Claude 在需要时会从用户处获取或使用已知凭证

## 知识库参考
- **Notion AI工作流文档**: https://www.notion.so/AI-2fdba9f99f5880369d98f3d69868b500
- **小游戏知识库V3.0**: 包含PRD、技术规范、验收标准

## 新对话启动指南
每次新对话开始时，Claude 应该：
1. 读取此文件 (`CLAUDE_CONTEXT.md`)
2. 读取 `.task/current_task.json` 获取当前任务状态
3. 如果有未完成任务，继续执行
4. 如果任务已完成，询问用户下一步计划

## 变更日志
- 2026-02-05 19:05: MS-001 ~ MS-006 全部完成
- 2026-02-05 17:30: 创建此文件，记录 MS-001/MS-002 完成状态
