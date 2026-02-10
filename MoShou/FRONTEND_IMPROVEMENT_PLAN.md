# 魔兽重生 - 前端表现改善计划

## 执行日期: 2026-02-07
## 状态: 进行中

---

## 一、当前问题诊断

### 1.1 资源应用问题
- **已生成资源**: 56个高质量AI生成的UI/VFX/角色贴图（全部完成）
- **未应用问题**: 生成的资源未正确应用到Unity组件中
  - UI组件仍使用Unity默认白色方块
  - VFX效果未使用生成的粒子贴图
  - 地面材质虽已更新，但其他材质未统一风格

### 1.2 视觉效果缺失
- 缺少粒子系统（攻击、技能、升级、受击）
- 缺少屏幕特效（受击闪红、升级闪光）
- 缺少UI动画（弹窗弹性动画、按钮悬停效果）
- 缺少镜头震动反馈

### 1.3 游戏手感不足
- 攻击无打击感
- 移动无惯性/加速度曲线
- 技能释放无预告/后摇
- 伤害数字无动态效果

---

## 二、解决方案架构

### 2.1 创建 Editor 自动化工具

#### UIArtApplier (Editor工具)
```
MoShou > Apply UI Art Assets
```
功能:
- 扫描所有UI组件的Image/RawImage
- 根据命名规则自动匹配Generated目录下的贴图
- 批量应用Sprite到UI组件
- 自动设置Sprite Import Settings (Sprite Mode, Pixels Per Unit等)

#### VFXPrefabGenerator (Editor工具)
```
MoShou > Generate VFX Prefabs
```
功能:
- 为每个VFX贴图创建粒子系统Prefab
- 配置粒子参数（生命周期、大小曲线、颜色渐变）
- 创建常用VFX组合（连击、升级光柱等）

### 2.2 运行时效果系统

#### GameFeedbackSystem
- 屏幕震动
- 时间缩放（顿帧）
- 屏幕闪光
- 后处理效果切换

#### UIAnimationSystem
- DOTween风格的UI动画
- 弹窗弹性出现/消失
- 按钮交互动效
- 数字滚动效果

---

## 三、执行步骤

### Phase 1: Editor工具创建 (本次执行)

| 步骤 | 文件 | 说明 |
|------|------|------|
| 1.1 | `Editor/UIArtApplier.cs` | 自动匹配并应用UI贴图 |
| 1.2 | `Editor/VFXPrefabGenerator.cs` | 生成VFX预制体 |
| 1.3 | `Editor/SpriteImportFixer.cs` | 修复Sprite导入设置 |

### Phase 2: 运行时系统 (本次执行)

| 步骤 | 文件 | 说明 |
|------|------|------|
| 2.1 | `Scripts/Effects/GameFeedback.cs` | 游戏反馈系统 |
| 2.2 | `Scripts/Effects/ScreenShake.cs` | 镜头震动 |
| 2.3 | `Scripts/Effects/HitStop.cs` | 顿帧效果 |
| 2.4 | `Scripts/UI/UIAnimation.cs` | UI动画工具 |

### Phase 3: 集成与优化

| 步骤 | 文件 | 说明 |
|------|------|------|
| 3.1 | 修改 `PlayerController.cs` | 添加攻击反馈调用 |
| 3.2 | 修改 `EnemyBase.cs` | 添加受击反馈调用 |
| 3.3 | 创建 `VFXManager.cs` | VFX对象池管理 |

---

## 四、资源映射规则

### UI组件命名 → 贴图映射

| UI组件名称模式 | 对应贴图 |
|----------------|----------|
| `*Panel*Background*` | `UI_Panel_Background.png` |
| `*Button*Normal*` | `UI_Button_Normal.png` |
| `*Button*Highlight*` | `UI_Button_Highlight.png` |
| `*Slot*Empty*` | `UI_Slot_Empty.png` |
| `*Slot*Selected*` | `UI_Slot_Selected.png` |
| `*HealthBar*Background*` | `UI_HealthBar_Background.png` |
| `*HealthBar*Fill*` | `UI_HealthBar_Fill.png` |
| `*ManaBar*Fill*` | `UI_ManaBar_Fill.png` |
| `*ExpBar*Fill*` | `UI_ExpBar_Fill.png` |
| `*Joystick*Base*` | `UI_Joystick_Base.png` |
| `*Joystick*Knob*` | `UI_Joystick_Knob.png` |
| `*Icon*Coin*` | `UI_Icon_Coin.png` |
| `*Icon*Gem*` | `UI_Icon_Gem.png` |
| `*Icon*Settings*` | `UI_Icon_Settings.png` |
| `*Icon*Close*` | `UI_Icon_Close.png` |
| `*Dialog*` | `UI_Dialog_Background.png` |
| `*Tooltip*` | `UI_Tooltip_Background.png` |

### VFX贴图 → 粒子系统配置

| VFX贴图 | 粒子系统参数 |
|---------|-------------|
| `VFX_Hit_Spark.png` | burst=8, lifetime=0.3s, size=0.5→0, color=white→yellow |
| `VFX_Arrow_Trail.png` | rate=30, lifetime=0.5s, trail=true, color=golden |
| `VFX_LevelUp.png` | burst=20, lifetime=1.5s, size=1→2, gravity=-0.5 |
| `VFX_Heal.png` | rate=15, lifetime=1s, size=0.3, color=green, gravity=-1 |

---

## 五、是否需要构建 SKILL？

### 分析

当前工作流已具备:
- ✅ LiblibAI API 资源生成能力
- ✅ Unity Editor 脚本执行能力
- ✅ n8n 自动化工作流

### 建议创建的 SKILL

| SKILL名称 | 触发方式 | 功能 |
|-----------|----------|------|
| `/apply-art` | 手动触发 | 执行 UIArtApplier + SpriteImportFixer |
| `/generate-vfx` | 手动触发 | 执行 VFXPrefabGenerator |
| `/visual-check` | PlayMode后自动 | 截图对比UI是否正确渲染 |

### 结论
**暂不需要构建独立SKILL**，因为:
1. Editor工具可直接在Unity菜单执行
2. 现有n8n工作流可以通过 `[NODE_COMPLETE]` 标签触发后续验证
3. 复杂度不高，不需要额外抽象层

---

## 六、其他AI工具需求

### 已使用
- ✅ **LiblibAI**: 生成2D美术资源
- ✅ **DeepSeek**: 代码审核和问题分析

### 建议增加
- ❓ **Stable Diffusion XL (本地)**: 更快的迭代生成，无API限制
- ❓ **ControlNet**: 基于现有UI布局生成一致风格的变体
- ❓ **AudioGen**: AI生成音效（攻击音效、UI音效）

### 当前优先级
**不建议增加新AI工具**，先把现有资源应用完毕，验证效果后再决定是否需要更多生成能力。

---

## 七、立即执行任务

1. ✅ 创建 `SpriteImportFixer.cs` - 修复所有生成贴图的导入设置
2. ✅ 创建 `UIArtApplier.cs` - 批量应用UI贴图
3. ✅ 创建 `VFXPrefabGenerator.cs` - 生成VFX预制体
4. ✅ 创建 `GameFeedback.cs` - 游戏反馈系统
5. ✅ 创建 `ScreenShake.cs` - 镜头震动组件
6. ✅ 创建 `VFXManager.cs` - VFX对象池
7. ✅ 修改 `PlayerController.cs` - 添加反馈调用
8. ✅ 运行 Unity Editor 菜单工具应用资源

---

## 八、验收标准

### 视觉验收 (VC-xxx)
- [ ] VC-010: 所有UI面板使用生成的背景贴图
- [ ] VC-011: 所有按钮使用生成的按钮贴图
- [ ] VC-012: 血条/蓝条/经验条使用生成的填充贴图
- [ ] VC-013: 摇杆使用生成的底盘和摇杆贴图
- [ ] VC-014: 攻击时有火花VFX
- [ ] VC-015: 升级时有光柱VFX
- [ ] VC-016: 攻击时有镜头微震
- [ ] VC-017: 受击时有屏幕闪红

### 手感验收 (GF-xxx)
- [ ] GF-001: 攻击命中有0.05s顿帧
- [ ] GF-002: UI弹窗有弹性动画
- [ ] GF-003: 伤害数字有弹出动画
- [ ] GF-004: 技能图标有冷却遮罩动画
