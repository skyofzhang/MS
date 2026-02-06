# MS Project Architecture V1.4
# Claude Code 单一执行者模式

## 核心变更

V1.3的Cursor作为执行者方案有致命缺陷：
**无法远程触发Cursor AI开始工作**

## V1.4 新架构

### 执行者角色
- **Claude Code**: 唯一AI执行者
  - 策划案生成
  - Unity场景修改（通过脚本）
  - APK打包
  - 验收

### Cursor的新角色
- **辅助开发工具**: 人类手动使用时的AI助手
- **不再是自动化流程的一部分**

## 自动化流程

```
人类写需求(Notion)
  ↓
n8n检测 (NWF-01)
  ↓
Claude Code 执行全流程:
  1. 生成策划案
  2. 质检 (调用DeepSeek)
  3. 生成Unity脚本
  4. 调用Unity CLI执行脚本修改场景
  5. 打包APK
  6. 上传到手机/通知人类测试
  ↓
人类测试APK
  ↓
反馈 → 循环
```

## Unity自动化方式

### 场景修改
不再依赖手动在Unity Editor操作，而是：
1. 创建Editor脚本 (Assets/Editor/AutoBuild/)
2. 脚本包含场景修改逻辑
3. 调用Unity -executeMethod 执行

### 示例：添加UI按钮
```csharp
// Assets/Editor/AutoBuild/SceneModifier.cs
public static void AddStartButton()
{
    // 打开场景
    EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

    // 创建Canvas
    var canvas = new GameObject("Canvas");
    canvas.AddComponent<Canvas>();
    // ...

    // 保存场景
    EditorSceneManager.SaveScene(scene);
}
```

### 调用方式
```powershell
& "D:\program\2022.3.47f1c1\Editor\Unity.exe" `
  -batchmode -nographics `
  -projectPath "E:\AI_Project\MS\MoShou" `
  -executeMethod SceneModifier.AddStartButton `
  -quit -logFile "E:\AI_Project\MS\modify.log"
```

## 文件结构

```
.task/
  workflow_state.json    # 状态机
  current_task.json      # 当前任务（给人类看的进度）
  plan_draft.json        # 策划案
  ARCHITECTURE_V1.4.md   # 本文档

MoShou/Assets/Editor/AutoBuild/
  SceneModifier.cs       # 场景修改脚本
  BuildScript.cs         # 打包脚本（已有）
```

## 触发方式

1. 人类在Notion写需求
2. 人类在Claude Code对话中说"开始"
3. Claude Code自动完成全流程
4. 人类收到APK测试通知

## 与Cursor的关系

Cursor仍然可用于：
- 人类想手动调试代码时
- 需要交互式IDE功能时
- 非自动化的开发任务

但不再是自动化流水线的一部分。
