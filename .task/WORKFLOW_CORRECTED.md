# MS Project - 修正后的自动化工作流 V1.4

## 反推分析结论

### 原设计的错误
1. **假设Cursor可以被远程触发** → 错误，Cursor AI需要人类输入
2. **假设Cursor能控制Unity** → 错误，Cursor只是代码编辑器
3. **把Cursor放在自动化流程中** → 根本性错误

### 正确的理解
- Cursor是人类使用的工具，不是自动化节点
- Unity的自动化通过CLI + Editor脚本实现
- Claude Code可以直接执行所有AI任务

---

## 修正后的架构

```
┌─────────────────────────────────────────────────────────┐
│                    人类 (Human)                          │
│  输入需求 → Notion                                       │
│  测试APK → 反馈                                          │
└─────────────────────────────────────────────────────────┘
                          ↓ ↑
┌─────────────────────────────────────────────────────────┐
│                 n8n (云端协调器)                          │
│  NWF-01: Notion监听 → 触发流程                           │
│  NWF-05: 进度同步 → Notion更新                           │
│  NWF-07: 质检 → DeepSeek API                             │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│              Claude Code (AI大脑+执行者)                  │
│                                                          │
│  职责:                                                    │
│  1. 策划案生成                                           │
│  2. 代码生成 (C# Scripts)                                │
│  3. Editor脚本生成 (场景修改)                            │
│  4. 调用Unity CLI                                        │
│  5. 分析日志/截图                                        │
│  6. 验收判断                                             │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                Unity CLI (执行引擎)                       │
│                                                          │
│  -executeMethod SceneModifier.XXX  → 修改场景            │
│  -executeMethod TestRunner.XXX     → 运行测试            │
│  -executeMethod BuildScript.XXX    → 打包APK             │
│                                                          │
│  所有操作无需打开Unity GUI!                               │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    输出 (Outputs)                        │
│  - APK文件                                               │
│  - 测试报告                                              │
│  - 截图 (可选: PlayMode截图脚本)                         │
└─────────────────────────────────────────────────────────┘

---

## Cursor的正确定位

Cursor **不在**自动化流程中。

Cursor的用途:
- 人类想手动编辑代码时
- 人类想用AI辅助调试时
- 非自动化的开发场景

---

## 自动化流程步骤

### Phase 1: 需求获取
```
人类写需求到Notion
  ↓
n8n NWF-01检测
  ↓
触发Claude Code
```

### Phase 2: 策划+代码生成
```
Claude Code:
  1. 分析需求
  2. 生成策划案 (.task/plan_draft.json)
  3. 调用DeepSeek质检 (通过n8n NWF-07)
  4. 生成C#代码 (Gameplay/UI/etc)
  5. 生成Editor脚本 (Assets/Editor/AutoBuild/)
```

### Phase 3: Unity执行
```
Claude Code调用PowerShell:

# Step 1: 关闭Unity (如果在运行)
Get-Process -Name "Unity" | Stop-Process -Force

# Step 2: 执行场景修改
Unity.exe -batchmode -executeMethod SceneModifier.XXX -quit

# Step 3: 执行测试 (可选)
Unity.exe -batchmode -executeMethod TestRunner.RunAllTests -quit

# Step 4: 打包APK
Unity.exe -batchmode -executeMethod BuildScript.BuildAndroid -quit
```

### Phase 4: 验收
```
Claude Code:
  1. 读取build.log检查是否成功
  2. 检查APK是否生成
  3. (可选) 运行截图脚本获取UI预览
  4. 更新Notion状态
  5. 通知人类测试
```

### Phase 5: 人类测试
```
人类:
  1. 安装APK到手机
  2. 测试功能
  3. 在Notion反馈问题或确认通过
```

---

## 实施清单

### 已完成
- [x] n8n工作流 (NWF-01, NWF-05, NWF-07)
- [x] 策划案生成
- [x] BuildScript.cs (打包脚本)

### 需要完善
- [ ] SceneModifier.cs (场景修改脚本) - 已创建框架
- [ ] 完整的Unity CLI调用封装
- [ ] 测试报告生成

### 不需要
- [x] Cursor触发机制 - **移除，不需要**
- [x] .cursorrules中的状态机检查 - **移除，不需要**

---

## 与人类的约定

1. 人类只需要:
   - 在Notion写需求
   - 在Claude Code说"开始"
   - 等待APK
   - 测试并反馈

2. 人类不需要:
   - 打开Unity
   - 打开Cursor
   - 手动拖拽组件
   - 手动打包

3. 如果需要人类介入:
   - Claude Code会明确说明原因
   - 通常是: 需要创意决策 / 需要测试真机 / 遇到技术限制
