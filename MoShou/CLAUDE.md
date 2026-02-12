# MoShou (魔兽归来) - Claude Code 项目备忘

## 项目概述

竖屏 Roguelike 动作手游，Unity 2022.3 + URP。参考分辨率 1080x1920。
UI 采用 **Prefab + 代码混合方案**：核心面板（商店/选关/战斗HUD）已转为 Editor 生成 Prefab，运行时加载；小地图/角色详情等仍为代码动态创建。通过 `Resources.Load<Sprite>()` 加载散图资源，加载失败时 fallback 到程序化纯色/渐变。

## 技术栈

- **引擎:** Unity 2022.3, Universal Render Pipeline (URP)
- **语言:** C# (.NET Standard 2.1)
- **UI方案:** Prefab + 代码混合 (UGUI)，CanvasScaler: ScaleWithScreenSize, referenceResolution 1080x1920, matchWidthOrHeight 0.5
- **Canvas renderMode:** ScreenSpaceOverlay, sortingOrder=100 (GameCanvas)
- **存档:** SaveSystem 单例 (JSON 本地存档)
- **场景管理:** SceneManager.LoadScene
- **GitHub:** `https://github.com/skyofzhang/MS.git` (branch: main)

## 场景结构

| 场景 | 文件 | 说明 |
|------|------|------|
| MainMenu | `Assets/Scenes/MainMenu.unity` | 主菜单（背景+Logo+5个按钮+底部状态栏） |
| StageSelect | `Assets/Scenes/StageSelect.unity` | 关卡选择（Prefab竖排卡片列表+底部进度） |
| GameScene | `Assets/Scenes/GameScene.unity` | 战斗场景（BattleHUD Prefab + 动态面板） |

## 代码目录结构

```
Assets/Scripts/
  Core/           # 场景Setup + 管理器
    GameSceneSetup.cs      (~2600行) 战斗场景：加载BattleHUD Prefab + 动态创建面板(商店/背包/装备/技能/角色信息)
    MainMenuSceneSetup.cs  (~970行)  主菜单全部UI创建
    StageSelectSceneSetup.cs (~270行) 选关界面Prefab实例化+数据填充
    GameManager.cs          游戏状态管理
    LoadingManager.cs       加载界面
    BattleStats.cs          战斗统计
    GameInitializer.cs      初始化
    MainMenuManager.cs      主菜单逻辑
    StageSelectManager.cs   选关逻辑
  UI/             # UI面板 & 组件
    GameHUD.cs              战斗HUD控制器(Prefab) — 顶部状态栏/侧边按钮/技能/暂停/头像
    ShopPanel.cs            商店面板 (~370行)
    ShopItemCardUI.cs       商品卡片Prefab控制器
    SkillUpgradePanel.cs    技能升级面板 (~960行)
    MinimapSystem.cs        小地图系统(动态创建, 右上角, ~720行)
    SimpleHealthBar.cs      血条组件(从UIManager拆分)
    SimpleInventoryPanel.cs 简化版背包面板
    SimpleEquipmentPanel.cs 简化版装备面板
    ConfirmDialog.cs        确认弹窗
    UIStyleHelper.cs        静态样式辅助类（颜色/字体）
    UITween.cs              缓动动画
    StageCardUI.cs          关卡卡片Prefab控制器
    Screens/
      CharacterInfoScreen.cs  角色信息面板 (~835行)
      ResultScreen.cs         结算界面
      DefeatScreen.cs         失败界面
    Components/
      TopStatusBar.cs
      BottomNavigationBar.cs
  Combat/         # 战斗系统
  Data/           # 数据结构（Equipment, ItemData, PlayerStats, MonsterConfigData）
  Effects/        # 视觉效果（HitStop, ScreenShake, VFX, UIFeedbackSystem, GameFeedback）
  Gameplay/       # 游戏逻辑（PlayerController, MonsterController, TerrainGenerator）
  Systems/        # 系统管理（SaveSystem, InventoryManager, EquipmentManager, AudioManager, LootManager）
  Utils/          # 工具类
  Editor/         # 编辑器脚本
    BattleHUDPrefabCreator.cs   菜单: MoShou/创建战斗HUD Prefab/0.全部生成
    ShopPrefabCreator.cs        菜单: MoShou/创建商店Prefab
    StageSelectPrefabCreator.cs 菜单: MoShou/创建选关Prefab
  Test/           # 测试
```

## Prefab 资源

```
Assets/Resources/Prefabs/UI/
  BattleHUD.prefab         战斗HUD (⚠️ 用户已手动调整，勿覆盖)
  ShopPanel.prefab         商店面板 (⚠️ 用户已手动调整，勿覆盖)
  ShopItemCard.prefab      商品卡片 (⚠️ 用户已手动调整，勿覆盖)
  StageSelectCanvas.prefab 选关界面 (⚠️ 用户已手动调整，勿覆盖)
  StageCard.prefab         关卡卡片 (⚠️ 用户已手动调整，勿覆盖)
```

**⚠️ 重要**: 所有 Prefab 均已由用户在 Unity Inspector 中手动微调布局。禁止重新运行 Editor 菜单生成，否则会覆盖用户调整。如需修改，应在运行时代码中动态调整，或由用户手动编辑 Prefab。

## Sprite 资源目录

所有 sprite 放在 `Assets/Resources/Sprites/UI/` 下，按功能分子目录。
代码中加载路径格式: `"Sprites/UI/{子目录}/{文件名}"` (不带 .png 后缀)。

```
Sprites/UI/
  MainMenu/       # 主菜单 (BG, Logo, 5个按钮sprite, Frame)
  StageSelect/    # 选关界面 (BG, 10张区域缩略图)
  Common/         # 公用 (金色卷轴Banner, 9-slice金色帧, 盾牌badge, 金币icon, 关闭按钮等)
  HUD/            # 战斗HUD (TopBanner, 圆形按钮帧, 5个功能icon, 技能icon, 小地图背景, 头像帧, 血条帧等)
  Shop/           # 商店 (Tab_Active/Inactive)
  CharInfo/       # 角色信息 (BG, 肖像帧, 肖像, 属性icon x4, 装备槽, 分隔线)
  Skills/         # 技能 (Slot_BG, Slot_Locked, Cooldown_Mask, 3个技能icon)
  Result/         # 结算 (Victory/Defeat BG, Star_Filled/Empty, Reward_Slot)
  LevelUp/        # 升级 (Card_BG, Card_Selected, Panel_BG, Title_BG, 3个稀有度)
  Buttons/        # 通用按钮 (Primary/Secondary Normal/Pressed/Disabled, Close, Pause)
  Kenney/         # Kenney UI Kit 素材
  RPGKit/         # 装备槽icon, 金币icon, 背包icon 等
```

## 核心编码模式

### Sprite 安全加载模式
```csharp
Sprite sprite = Resources.Load<Sprite>("Sprites/UI/子目录/文件名");
if (sprite != null)
{
    img.sprite = sprite;
    img.type = Image.Type.Simple;  // 或 Image.Type.Sliced (9-slice)
    img.color = Color.white;
}
else
{
    // fallback: 程序化颜色
    img.color = new Color(r, g, b, a);
}
```

### 单例面板
ShopPanel、CharacterInfoScreen、ConfirmDialog、SkillUpgradePanel、SimpleInventoryPanel、SimpleEquipmentPanel、MinimapSystem 等均使用 `Instance` 单例 + `Show()/Hide()/Toggle()` 模式。

### GameHUD 按钮绑定架构
```
GameHUD.cs (挂载在BattleHUD Prefab根节点)
├── portraitButton → OnPortraitClick() → CharacterInfoScreen / MinimapSystem.ShowCharacterDetail()
├── shopButton     → OnSideButtonClick(0) → ShopPanel.Instance.Toggle()
├── bagButton      → OnSideButtonClick(1) → SimpleInventoryPanel.Instance.Toggle() (fallback: InventoryPanel)
├── skillButton    → OnSideButtonClick(2) → SkillUpgradePanel.Instance.Toggle()
├── equipSideButton→ OnSideButtonClick(3) → SimpleEquipmentPanel.Instance.Toggle()
├── mapButton      → OnSideButtonClick(4) → MinimapSystem.Instance.ToggleVisible()
├── attackButton   → OnAttackClick()      → Debug.Log (普攻由PlayerController处理)
├── skill1-3Button → OnSkill1-3Click()    → PlayerController.UseSkill1-3()
├── pauseButton    → OnPauseClick()       → GameManager.Instance.TogglePause()
└── WireUIManager() — 将Prefab引用传递给UIManager (由GameSceneSetup调用)
```

### 小地图 (MinimapSystem) 架构
- **位置**: 右上角 (anchor=1,1, pivot=1,1, pos=-10,-20)
- **背景**: `UI_HUD_Map_Bg.png` 图片
- **Canvas**: 挂载到 GameCanvas 下 (sortingOrder=100), `SetAsFirstSibling()` 确保层级最低
- **显隐**: `ToggleVisible()` 控制 minimapContainer 的 SetActive
- **点击**: 小地图点击为空操作；角色详情改由左上角头像按钮触发
- **保持动态创建**: ~720行自建UI，不适合Prefab化

## 已完成的 UI 重构历史

### Phase 1-8: 基础 UI Sprite 适配
- Phase 1: 主菜单布局调整 (Logo/按钮/状态栏)
- Phase 2: 选关界面 (Banner/卡片/缩略图/星星)
- Phase 3a: 商店 (Tab sprite/商品行/购买按钮)
- Phase 3b: 角色信息 (肖像帧/属性grid/装备grid)
- Phase 4a: 技能升级 (卷轴banner/3列grid/详情面板)
- Phase 4b: 战斗HUD (顶部banner/肖像帧/圆形按钮/技能栏)
- Phase 5: 加载界面 (BG+进度条)
- Phase 6: 编译修复
- Phase 8: 选关界面修复 (Sprite路径/Banner/9-slice)

### Phase 9-10: 选关界面转 Prefab
- `StageSelectSceneSetup.cs` 从 ~1175行 → ~270行
- 新建 `StageCardUI.cs` + `StageSelectPrefabCreator.cs`
- 生成 `StageSelectCanvas.prefab` + `StageCard.prefab`
- 用户手动布局微调 (**⚠️ 禁止重新生成**)

### Phase 11-12: 商店界面转 Prefab
- `GameSceneSetup.cs` 的 `CreateShopPanel()` 从 ~190行 → ~20行
- 新建 `ShopItemCardUI.cs` + `ShopPrefabCreator.cs`
- 生成 `ShopPanel.prefab` + `ShopItemCard.prefab`
- 用户手动布局微调 (**⚠️ 禁止重新生成**)

### Phase 13: 战斗HUD转Prefab (第一阶段 — 基础)
- `GameSceneSetup.cs` 删除 `CreateHUD()`(~240行) + `CreateLeftSidebarButtons()`(~75行) + `OnSideButtonClick()`(~20行) + `CreateSkillBar()`(~85行)
- 新增 `CreateBattleHUD()` 加载 `Prefabs/UI/BattleHUD.prefab`
- 新建 `BattleHUDPrefabCreator.cs` Editor脚本
- `GameHUD.cs` 新增: 侧边按钮字段 + `WireUIManager()` + `OnSideButtonClick()`
- 包含: 顶部状态栏 + 左侧4按钮 + 右下技能栏

### Phase 14: 战斗HUD转Prefab (第二阶段 — 完整)
- `GameSceneSetup.cs` 删除8个动态方法(~481行): `CreateVirtualJoystick`, `CreateCircleSprite`, `CreateSkillButtons`, `CreateSkillButtonWithIcon`, `CreateFunctionButtons`, `CreateSideFunctionButton`, `CreatePausePanel`, `CreatePauseButton`
- `BattleHUDPrefabCreator.cs` 新增: 虚拟摇杆 + 技能动作按钮(攻击+3技能) + 暂停按钮 + 暂停面板 + 装备侧边按钮
- `GameHUD.cs` 新增: `attackButton/skill1-3Button/pauseButton/pausePanel/equipSideButton` + 回调 + 侧边按钮扩展到5个(商店/背包/技能/装备/地图)
- 圆形Sprite资源自动生成: `Assets/Resources/Sprites/UI/HUD/UI_Circle_64.png`

### Phase 15: 小地图优化 + 角色详情入口 + BattleHUD用户调整
- `MinimapSystem.cs`:
  - 小地图位置从左上角→右上角 (anchor=1,1, pivot=1,1, pos=-10,-20)
  - 背景改用 `UI_HUD_Map_Bg.png` 图片替代纯色
  - 不再创建独立Canvas，挂载到GameCanvas下 + `SetAsFirstSibling()` 确保层级最低
  - 新增 `ToggleVisible()` / `ShowCharacterDetail()` 公开方法
  - 点击小地图不再弹出角色详情（改为空操作）
  - 地图区域填充色 alpha=0.25，网格线 alpha=0.2
- `GameHUD.cs`:
  - 地图按钮(case 4)改为调用 `MinimapSystem.Instance.ToggleVisible()`
  - 背包按钮(case 1)改为优先使用 `SimpleInventoryPanel.Instance`
  - 新增 `portraitButton` 字段 + `OnPortraitClick()` 回调（点击头像→角色详情）
- `BattleHUDPrefabCreator.cs`: PlayerIconFrame 添加 Button 组件 + 绑定 portraitButton
- **用户手动调整 BattleHUD.prefab** (**⚠️ 勿覆盖！**):
  - 精简 Prefab 层级结构，删除多余节点
  - 调整各 UI 元素位置和尺寸
  - 战斗主界面布局已由用户在 Unity Inspector 中手动优化完成

## BattleHUD Prefab 层级结构 (Phase 14+15, 用户已手动调整)

```
BattleHUD (全屏透明容器, 挂载GameHUD)
├── HUD (anchor顶部, Image半透明黑色)
│   ├── PlayerIconFrame (80x80, sprite=UI_HUD_Portrait_Frame, Button→角色详情)
│   ├── HealthBarBG (340x32, sprite=UI_HUD_HPBar_Frame, 含SimpleHealthBar)
│   │   ├── HealthFill (Image.Filled Horizontal)
│   │   └── HealthText ("100/100")
│   ├── WaveBG (居中, sprite=UI_HUD_TopBanner)
│   │   └── WaveText ("波次 1/3 击杀 0")
│   ├── GoldIcon + GoldText
│   ├── LevelText ("Lv.1")
│   ├── AttackText + DefenseText
├── SideBtn_商店/背包/技能/装备/地图 (左侧5个圆形按钮)
├── SkillBar (右下角4slot)
├── VirtualJoystick (左下, 挂载VirtualJoystick组件)
│   └── Background → Handle
├── SkillButtons (右下)
│   ├── AttackBtn (130x130)
│   ├── Skill1/2/3 (100x100)
├── PauseButton (右上, sprite=UI_Btn_Pause)
└── PausePanel (全屏遮罩, 默认隐藏)
    └── Content → Resume/Retry/Menu Buttons
```

## 当前项目状态

**编译状态:** ✅ 正常
**GitHub:** `https://github.com/skyofzhang/MS.git` main 分支已同步 (commit a732b89)

### 已完成 ✅
- [x] 主菜单 Sprite 适配
- [x] 选关界面转 Prefab + 用户手动布局
- [x] 商店界面转 Prefab + 用户手动布局
- [x] 战斗HUD转 Prefab (两阶段完成) + 用户手动布局
- [x] 小地图优化 (右上角 + 背景图 + 层级修复)
- [x] 角色详情入口改为头像按钮
- [x] 左侧5个按钮全部可用 (商店/背包/技能/装备/地图)
- [x] SimpleHealthBar 从 UIManager 拆分为独立脚本

### 仍为动态创建的 UI (不转Prefab)
- 小地图 (`MinimapSystem.cs`, ~720行)
- 角色详情面板 (`MinimapSystem.ShowCharacterDetail()` / `CharacterInfoScreen`)
- 背包面板 (`SimpleInventoryPanel`)
- 装备面板 (`SimpleEquipmentPanel`)
- 技能升级面板 (`SkillUpgradePanel`)
- 伤害数字/拾取反馈 (`UIFeedbackSystem`)

### 已知非阻塞警告 (不影响运行)
- `[SkillUpgradePanel] GetSkillLevelSaveData被调用但尚未初始化完成` — 初始化时序问题，有缓存兜底
- `[Spawner] Model not found for MON_SKELETON_001` — 骷髅怪模型缺失，使用fallback

## 注意事项

1. **所有 Prefab 禁止重新生成** — 用户已手动调整布局，重新运行 Editor 菜单会覆盖
2. **Sprite 路径必须与磁盘一致** — `Resources.Load` 路径不带 .png 后缀，大小写敏感
3. **9-slice sprite 必须在 Unity Inspector 中设置好 Border** — 否则 `Image.Type.Sliced` 不会正确拉伸
4. **GameSceneSetup ~2600行** — 改动前先定位具体方法和行号
5. **SimpleInventoryPanel 在全局命名空间** — 其余 UI 类在 `MoShou.UI` 命名空间，但 C# 允许跨命名空间访问全局类
6. **VirtualJoystick 的 background/handle/handleRange 是 public 字段** — 不是 SerializeField，BattleHUDPrefabCreator 中直接赋值
