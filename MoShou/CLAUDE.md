# MoShou (魔兽归来) - Claude Code 项目备忘

## 项目概述

竖屏 Roguelike 动作手游，Unity 2022.3 + URP。参考分辨率 1080x1920。
所有 UI 均由 C# 代码动态创建（无 Prefab），通过 `Resources.Load<Sprite>()` 加载散图资源，加载失败时 fallback 到程序化纯色/渐变。

## 技术栈

- **引擎:** Unity 2022.3, Universal Render Pipeline (URP)
- **语言:** C# (.NET Standard 2.1)
- **UI方案:** 纯代码创建 (UGUI)，CanvasScaler: ScaleWithScreenSize, referenceResolution 1080x1920, matchWidthOrHeight 0.5
- **Canvas renderMode:** ScreenSpaceOverlay
- **存档:** SaveSystem 单例 (JSON 本地存档)
- **场景管理:** SceneManager.LoadScene

## 场景结构

| 场景 | 文件 | 说明 |
|------|------|------|
| MainMenu | `Assets/Scenes/MainMenu.unity` | 主菜单（背景+Logo+5个按钮+底部状态栏） |
| StageSelect | `Assets/Scenes/StageSelect.unity` | 关卡选择（竖排卡片列表+底部进度） |
| GameScene | `Assets/Scenes/GameScene.unity` | 战斗场景（含HUD/商店/背包/角色/技能等所有面板） |

## 代码目录结构

```
Assets/Scripts/
  Core/           # 场景Setup + 管理器（最重的几个文件在这）
    GameSceneSetup.cs      (~2600行) 战斗场景UI创建(HUD+摇杆+技能+暂停+功能按钮已转Prefab)
    MainMenuSceneSetup.cs  (~970行)  主菜单全部UI创建
    StageSelectSceneSetup.cs (~270行) 选关界面Prefab实例化+数据填充
    GameManager.cs          游戏状态管理
    LoadingManager.cs       加载界面
    BattleStats.cs          战斗统计
    GameInitializer.cs      初始化
    MainMenuManager.cs      主菜单逻辑
    StageSelectManager.cs   选关逻辑
  UI/             # UI面板 & 组件
    ShopPanel.cs            商店面板 (~370行)
    ShopItemCardUI.cs       商品卡片Prefab控制器
    SkillUpgradePanel.cs    技能升级面板 (~960行)
    MinimapSystem.cs        小地图系统
    GameHUD.cs              战斗HUD
    ConfirmDialog.cs        确认弹窗
    UIStyleHelper.cs        静态样式辅助类（颜色/字体）
    UITween.cs              缓动动画
    Screens/
      CharacterInfoScreen.cs  角色信息面板 (~835行)
      ResultScreen.cs         结算界面
      DefeatScreen.cs         失败界面
    StageCardUI.cs              关卡卡片Prefab控制器
    Components/
      TopStatusBar.cs
      BottomNavigationBar.cs
  Combat/         # 战斗系统
  Data/           # 数据结构（Equipment, ItemData, PlayerStats, MonsterConfigData）
  Effects/        # 视觉效果（HitStop, ScreenShake, VFX）
  Gameplay/       # 游戏逻辑（PlayerController, MonsterController, TerrainGenerator）
  Systems/        # 系统管理（SaveSystem, InventoryManager, EquipmentManager, AudioManager, LootManager）
  Utils/          # 工具类
  Editor/         # 编辑器脚本
  Test/           # 测试
```

## Sprite 资源目录

所有 sprite 放在 `Assets/Resources/Sprites/UI/` 下，按功能分子目录。
代码中加载路径格式: `"Sprites/UI/{子目录}/{文件名}"` (不带 .png 后缀)。

```
Sprites/UI/
  MainMenu/       # 主菜单 (BG, Logo, 5个按钮sprite, Frame)
  StageSelect/    # 选关界面 (BG, 10张区域缩略图)
  Common/         # 公用 (金色卷轴Banner, 9-slice金色帧, 盾牌badge, 金币icon, 关闭按钮等)
  HUD/            # 战斗HUD (TopBanner, 圆形按钮帧, 4个功能icon, 技能icon, 小地图帧, 金币显示)
  Shop/           # 商店 (Tab_Active/Inactive)
  CharInfo/       # 角色信息 (BG, 肖像帧, 肖像, 属性icon x4, 装备槽, 分隔线)
  Skills/         # 技能 (Slot_BG, Slot_Locked, Cooldown_Mask, 3个技能icon)
  Result/         # 结算 (Victory/Defeat BG, Star_Filled/Empty, Reward_Slot)
  LevelUp/        # 升级 (Card_BG, Card_Selected, Panel_BG, Title_BG, 3个稀有度)
  Buttons/        # 通用按钮 (Primary/Secondary Normal/Pressed/Disabled, Close, Pause)
  Kenney/         # Kenney UI Kit 素材
  RPGKit/         # 装备槽icon, 金币icon, 背包icon 等
```

### 已到位的关键资源清单

**MainMenu (8)**
`UI_MainMenu_BG`, `UI_MainMenu_Logo`, `UI_MainMenu_Frame`,
`UI_Btn_Start_Normal/Pressed`, `UI_Btn_Continue_Normal/Disabled`,
`UI_Btn_Role`, `UI_Btn_Settings_Normal`, `UI_Btn_Quit_Normal`

**StageSelect (11)**
`UI_StageSelect_BG`,
`UI_Stage_Thumb_Forest/Desert/Element/Lava/Ice/Giant/Swamp/Shadow/Undead/Final`

**Common (8)**
`UI_Banner_Scroll_Gold`, `UI_Frame_Gold_9slice`, `UI_Badge_Shield_Lv`,
`UI_Icon_Coin_Stack`, `UI_Icon_Coin_Small`, `UI_Panel_Dark_Ornate`,
`UI_ProgressBar_Frame`, `UI_Btn_Close_X`

**HUD (12)**
`UI_HUD_TopBanner`, `UI_HUD_Btn_Circle_Frame`, `UI_HUD_Minimap_Frame`,
`UI_HUD_Btn_Shop/Bag/Skill/Map`, `UI_HUD_Btn_Attack_Frame`,
`UI_HUD_Skill_Attack/Heal/IceSword/Potion`, `UI_HUD_Gold_Display_BG`

**CharInfo (10)**
`UI_CharInfo_BG`, `UI_CharInfo_Portrait_Frame`, `UI_CharInfo_Portrait_Warrior`,
`UI_CharInfo_Section_Divider`, `UI_Icon_Stat_HP/ATK/DEF/CRIT`,
`UI_Equip_Slot_Empty`, `UI_Equip_Slot_Filled`

**Shop (2):** `UI_Shop_Tab_Active`, `UI_Shop_Tab_Inactive`

**Result (5):** `UI_Result_Victory_BG`, `UI_Result_Defeat_BG`, `UI_Result_Star_Filled`, `UI_Result_Star_Empty`, `UI_Result_Reward_Slot`

**Skills (6):** `UI_Skill_Slot_BG`, `UI_Skill_Slot_Locked`, `UI_Skill_Cooldown_Mask`, `UI_Skill_Icon_MultiShot/Pierce/BattleShout`

**LevelUp (6):** `UI_LevelUp_Panel_BG`, `UI_LevelUp_Title_BG`, `UI_LevelUp_Card_BG/Selected`, `UI_Rarity_Common/Epic/Rare`

### 尚未到位的资源 (代码中有加载但磁盘无对应文件)

以下路径在代码中用 `Resources.Load<Sprite>()` 尝试加载，但目前磁盘上无对应 PNG：
- `StageSelect/UI_StageCard_Thumb_Frame` — 缩略图外框
- `StageSelect/UI_Icon_Lock` — 锁定icon
- `StageSelect/UI_Btn_Stage_Go` — "前往"按钮
- `Shop/UI_Shop_ItemRow_Frame` — 商品行帧
- `Shop/UI_Shop_ItemIcon_Frame` — 商品icon帧
- `Shop/UI_Btn_Buy` — 购买按钮

这些加载失败时代码会 fallback 到程序化绘制，不影响运行。

## UI 效果图

7 张效果图位于 `Assets/Res/WoW_Cartoon_UI_Mockups/`：
- 开始游戏界面.png (主菜单)
- 选择关卡界面.png (选关)
- 战斗主界面.png (HUD)
- 商店界面.png
- 玩家信息界面.png (角色信息)
- 技能升级界面.png
- 常用加载界面.png

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

### 9-slice vs 原尺寸
- **9-slice 帧类** (`UI_Frame_Gold_9slice` 等): 用 `Image.Type.Sliced`，可拉伸
- **完整图片** (BG, Banner, Logo, 按钮): 用 `Image.Type.Simple` + `SetNativeSize()`，保持原始比例

### 单例面板
ShopPanel、CharacterInfoScreen、ConfirmDialog、SkillUpgradePanel 等均使用 `Instance` 单例 + `Show()/Hide()/Toggle()` 模式。

### UI 风格统一
`UIStyleHelper` (namespace `MoShou.UI`) 提供：
- `Colors.Gold` / `Colors.DarkBg` / `Colors.TextPrimary` 等
- `GetDefaultFont()` 返回系统字体

## 已完成的 UI 重构 (Phase 1-8)

### Phase 1: 主菜单 `MainMenuSceneSetup.cs`
- 删除版本号文字
- 按钮面板位置/尺寸调整 (居中偏上)
- 按钮宽度扩大(560)，有sprite时不叠加Text子物体
- 底部状态栏减高+半透明，新增金币icon
- Logo 文字改为"我是MT\n魔兽归来"，位置上移至锚点0.9
- 等级/金币显示位置微调

### Phase 2: 选关界面 `StageSelectSceneSetup.cs`
- 标题栏: 删除返回按钮，使用卷轴Banner sprite
- 卡片: 新增左侧区域缩略图、右侧"前往"按钮、星星图标(3星)
- 移除难度badge，星星移到右侧
- 底部进度改为水平进度条

### Phase 3a: 商店 `ShopPanel.cs`
- Tab 从 7类重构为 7类: 全部/武器/头盔/护甲/护腿/饰品(戒指+项链)/药水
- Tab 支持 sprite (Active/Inactive)
- 商品行增加 icon 帧、购买按钮 sprite、金币icon
- icon fallback 改为半透明色 + 首字母

### Phase 3b: 角色信息 `CharacterInfoScreen.cs`
- 从径向装备槽布局改为纵向布局
- 新增: 肖像帧 sprite + 属性区 2列grid + 装备区 3列grid
- 移除 UPGRADE 按钮

### Phase 4a: 技能升级 `SkillUpgradePanel.cs`
- 新增卷轴 banner
- 左侧: 技能列表改为 3列 grid
- 右侧: 详情面板背景 + 更大的 icon + 升级按钮

### Phase 4b: 战斗HUD `GameSceneSetup.cs`
- 顶部中央 banner、左上角肖像帧
- 左侧 4个圆形快捷按钮 (商店/背包/技能/地图)
- 右下技能栏 4slot

### Phase 5: 加载界面 `LoadingManager.cs`
- 动态创建 Loading UI (BG + 极细进度条)

### Phase 6: 编译修复
- `GameSceneSetup.cs` ShopPanel tab 从旧7-tab同步到新6-tab
- `LoadingManager.cs` 删除未使用的 `uiCreated` 字段

### Phase 8: 选关界面彻底修复 `StageSelectSceneSetup.cs`
- 背景图路径修正: `"UI_Mockups/Screens/UI_StageSelect"` → `"Sprites/UI/StageSelect/UI_StageSelect_BG"`
- 删除 Vignette 暗角蒙版
- Banner 改为 `SetNativeSize()` + `Image.Type.Simple` (不拉伸)
- 卡片帧改为 `Sprites/UI/Common/UI_Frame_Gold_9slice` (9-slice)，删除程序化金色边框/发光
- fallback 颜色统一为协调深色调

### Phase 9: 选关界面转Prefab方案
- `StageSelectSceneSetup.cs` 从 ~1175行 精简到 ~270行，删除所有Create***()方法
- 新建 `StageCardUI.cs` 卡片控制器，暴露所有子元素引用给Inspector
- UI布局改由两个Prefab控制（在Unity编辑器中手动搭建）：
  - `Assets/Resources/Prefabs/UI/StageSelectCanvas.prefab` — 整体布局
  - `Assets/Resources/Prefabs/UI/StageCard.prefab` — 单张关卡卡片模板
- 代码只负责：加载关卡配置 → 实例化100张卡片 → 填充数据 → 绑定事件
- Sprite资源直接在Prefab Inspector中拖入，无需代码Resources.Load

### Phase 10: 用户手动布局微调 + 返回按钮
- 用户已在Unity编辑器中手动调整了 StageSelectCanvas.prefab 和 StageCard.prefab 的布局
- **⚠️ 禁止重新运行 "MoShou/创建选关Prefab" 菜单，否则会覆盖用户调整**
- 后续新增UI元素应在运行时代码 `StageSelectSceneSetup.cs` 中动态创建，或由用户手动编辑Prefab
- 右上角关闭按钮（`UI_Btn_Close_X`）由运行时代码动态创建，不在Prefab中

### Phase 11: 商店界面转Prefab方案
- `GameSceneSetup.cs` 的 `CreateShopPanel()` 从 ~190行 精简到 ~20行（加载Prefab）
- 删除 `CreateShopTabButton()` 方法（已移入Editor脚本）
- `ShopPanel.cs` 删除 `ShopItemUI` 内部类（~250行），改用 `ShopItemCardUI` + Prefab实例化
- 新建 `ShopItemCardUI.cs` 商品卡片控制器，暴露所有子元素引用给Inspector
- 新建 `ShopPrefabCreator.cs` Editor脚本（菜单: MoShou/创建商店Prefab）
- 7个Tab: 全部→武器→头盔→护甲→护腿→饰品(戒指+项链)→药水
- 生成两个Prefab：
  - `Assets/Resources/Prefabs/UI/ShopPanel.prefab` — 商店面板整体
  - `Assets/Resources/Prefabs/UI/ShopItemCard.prefab` — 单个商品行模板

### Phase 12: 用户手动布局微调（商店界面）
- 用户已在Unity编辑器中手动调整了 ShopPanel.prefab 和 ShopItemCard.prefab 的布局
- **⚠️ 禁止重新运行 "MoShou/创建商店Prefab" 菜单，否则会覆盖用户调整**
- 后续新增UI元素应在运行时代码 `ShopPanel.cs` 或 `GameSceneSetup.cs` 中动态创建，或由用户手动编辑Prefab

### Phase 13: 战斗HUD转Prefab方案 (第一阶段)
- `GameSceneSetup.cs` 删除 `CreateHUD()`(~240行) + `CreateLeftSidebarButtons()`(~75行) + `OnSideButtonClick()`(~20行) + `CreateSkillBar()`(~85行)
- 新增 `CreateBattleHUD()` 加载 `Prefabs/UI/BattleHUD.prefab`
- 新建 `BattleHUDPrefabCreator.cs` Editor脚本（菜单: MoShou/创建战斗HUD Prefab/0. 全部生成）
- `GameHUD.cs` 新增: 侧边按钮字段 + `WireUIManager()` + `OnSideButtonClick()`
- 包含: 顶部状态栏 + 左侧4按钮 + 右下技能栏

### Phase 14: 战斗HUD转Prefab方案 (第二阶段 — 完整)
- `GameSceneSetup.cs` 删除8个动态方法(~481行): `CreateVirtualJoystick`, `CreateCircleSprite`, `CreateSkillButtons`, `CreateSkillButtonWithIcon`, `CreateFunctionButtons`, `CreateSideFunctionButton`, `CreatePausePanel`, `CreatePauseButton`
- 移除 `CreateGameUI()` 中的对应4行调用，只保留 `CreateBattleHUD()`
- `CreateBattleHUD()` 新增 `BindPausePanelButtons()` 绑定暂停面板按钮回调
- `BattleHUDPrefabCreator.cs` 新增: 虚拟摇杆(含圆形sprite资源生成) + 技能动作按钮(攻击+3技能) + 暂停按钮 + 暂停面板 + 装备侧边按钮
- `GameHUD.cs` 新增: `attackButton/skill1-3Button/pauseButton/pausePanel/equipSideButton` 字段 + 回调 + 侧边按钮扩展到5个(商店/背包/技能/装备/地图)
- 圆形Sprite资源自动生成: `Assets/Resources/Sprites/UI/HUD/UI_Circle_64.png`
- 小地图保持动态创建（MinimapSystem 720行自建UI，不适合Prefab化）

### Phase 15: 小地图优化 + 角色详情入口调整
- `MinimapSystem.cs`: 小地图位置从左上角→右上角 (anchor=1,1, pivot=1,1, pos=-10,-20)
- `MinimapSystem.cs`: 背景改用 `UI_HUD_Map_Bg.png` 图片替代纯色
- `MinimapSystem.cs`: 不再创建独立Canvas，挂载到GameCanvas下（与战斗HUD同层级）
- `MinimapSystem.cs`: 新增 `ToggleVisible()` 方法控制minimapContainer显隐
- `MinimapSystem.cs`: 点击小地图不再弹出角色详情，改为空操作
- `MinimapSystem.cs`: 新增 `ShowCharacterDetail()` 公开方法供外部调用
- `GameHUD.cs`: 地图按钮(case 4)改为调用 `MinimapSystem.Instance.ToggleVisible()`
- `GameHUD.cs`: 背包按钮(case 1)改为优先使用 `SimpleInventoryPanel.Instance`
- `GameHUD.cs`: 新增 `portraitButton` 字段 + `OnPortraitClick()` 回调（点击头像→角色详情）
- `BattleHUDPrefabCreator.cs`: PlayerIconFrame 添加 Button 组件 + 绑定 portraitButton
- `MinimapSystem.cs`: minimapContainer 创建后调用 `SetAsFirstSibling()` 确保层级最低
- `MinimapSystem.cs`: 地图区域填充色 alpha 降至 0.25，网格线 alpha 降至 0.2
- **用户手动调整 BattleHUD.prefab**（⚠️ 勿覆盖！重新生成Prefab前需备份）:
  - 精简了 Prefab 层级结构，删除多余节点
  - 调整了各 UI 元素的位置和尺寸
  - 战斗主界面布局已由用户在 Unity Inspector 中手动优化完成

### StageSelectCanvas Prefab 层级结构（需手动创建）
```
StageSelectCanvas (Canvas, CanvasScaler 1080x1920 matchWH=0.5, ScreenSpaceOverlay, sortOrder=100)
├── Background (Image, 全屏, sprite=UI_StageSelect_BG)
├── TopTitleBar (Image, sprite=UI_Banner_Scroll_Gold, 顶部居中)
│   └── Title (Text, "关卡选择", 44号粗体深棕色)
└── StageListScrollView (ScrollRect, 垂直滚动, sensitivity=40)
    └── Viewport (Image+Mask, 填满ScrollView)
        └── Content (VerticalLayoutGroup spacing=12 padding=20,20,20,40 + ContentSizeFitter)
```

### StageCard Prefab 层级结构（需手动创建，挂载StageCardUI）
```
StageCard (920x140, Image=UI_Frame_Gold_9slice Sliced, Button)
├── Thumbnail (100x100, Image, 左侧 x=65)
├── StageName (Text, 左上, 26号粗体, offsetLeft=130)
├── StageInfo (Text, 左下, 18号, offsetLeft=130)
├── Stars (HorizontalLayoutGroup spacing=2, 右侧 x=-70, 默认隐藏)
│   ├── Star_0 (Image 22x22)
│   ├── Star_1 (Image 22x22)
│   └── Star_2 (Image 22x22)
├── GoButton (Button 95x44, 绿色背景, 右侧 x=-55, 默认隐藏)
│   └── Text ("激活", 22号白色粗体)
└── LockIcon (50x50, 右侧 x=-60, 默认隐藏)
```

### ShopPanel Prefab 层级结构（用户已手动调整，勿覆盖）
```
ShopPanel (全屏, Image 半透明黑色遮罩 rgba(0,0,0,0.8))
├── Content (锚点 0.05~0.95 x 0.1~0.9, Image=UI_Shop_BG Sliced)
│   ├── Title (Text, "商店", 36号粗体, 顶部居中)
│   ├── CloseButton (Button 50x50, sprite=UI_Btn_Close_X, 右上角)
│   ├── GoldDisplay (Text, "金币: 0", 22号黄色, 左上角)
│   ├── Tabs (HorizontalLayoutGroup spacing=5, 顶部下方 y=-85)
│   │   ├── Tab_全部 (Button 65x40, sprite=Tab_Active/Inactive)
│   │   ├── Tab_武器
│   │   ├── Tab_头盔
│   │   ├── Tab_护甲
│   │   ├── Tab_护腿
│   │   ├── Tab_饰品
│   │   └── Tab_药水
│   ├── ItemsContainer (ScrollRect 垂直滚动)
│   │   └── Viewport (Image+Mask)
│   │       └── Content (VerticalLayoutGroup spacing=10 + ContentSizeFitter)
│   └── ToastContainer (默认隐藏, 400x60, 居中偏上)
│       └── ToastText (Text, 24号白色)
```

### ShopItemCard Prefab 层级结构（用户已手动调整，勿覆盖）
```
ShopItemCard (高110, Image=UI_Shop_ItemRow_Frame Sliced, LayoutElement flexW=1)
├── IconContainer (80x80, Image=UI_Shop_ItemIcon_Frame, 左侧 x=55)
│   └── Icon (Image, 占IconContainer 80%区域)
├── Name (Text, 24号粗体白色, 左上方 x=175 y=22)
├── Description (Text, 17号灰色, 左下方 x=175 y=-14)
├── PriceArea (右侧 x=-130)
│   ├── CoinIcon (Image 22x22, sprite=UI_Icon_Coin_Stack)
│   └── Price (Text, 20号黄色, 右对齐)
└── BuyButton (Button 90x40, sprite=UI_Btn_Buy, 右下方 x=-55 y=-14)
    └── Text ("购买", 18号白色粗体)
```

### BattleHUD Prefab 层级结构
```
BattleHUD (全屏透明容器, 挂载GameHUD + VirtualJoystick在子对象)
├── HUD (anchor 0,0.88~1,1, Image 半透明黑色 alpha=0.5)
│   ├── PlayerIconFrame (80x80, sprite=UI_HUD_Portrait_Frame)
│   ├── HealthBarBG (340x32, sprite=UI_HUD_HPBar_Frame Sliced, 含SimpleHealthBar)
│   │   ├── HealthFill (Image.Filled Horizontal)
│   │   └── HealthText (Text "100/100", 20号粗体白色+黑描边)
│   ├── WaveBG (280x48, sprite=UI_HUD_TopBanner Sliced, 居中)
│   │   └── WaveText (Text "波次 1/3 击杀 0", 22号粗体)
│   ├── GoldIcon (36x36, sprite=UI_Icon_Coin_Small)
│   ├── GoldText (Text "0", 30号粗体金色)
│   ├── LevelText (Text "Lv.1", 20号粗体, 头像下方)
│   ├── AttackText (Text "攻击: 15", 20号橙色)
│   └── DefenseText (Text "防御: 5", 20号蓝色)
├── SideBtn_商店 (70x70, sprite=UI_HUD_Btn_Circle_Frame + Icon, y=-280)
├── SideBtn_背包 (y=-370)
├── SideBtn_技能 (y=-460)
├── SideBtn_装备 (y=-550, icon=Slot_Equip_Weapon)
├── SideBtn_地图 (y=-640)
├── SkillBar (HorizontalLayoutGroup spacing=8, 右下角)
│   ├── SkillSlot_0 ~ SkillSlot_3 (80x80, SkillIcon + LvBadge)
├── VirtualJoystick (200x200, anchor左下 50,50, 挂载VirtualJoystick组件)
│   └── Background (180x180, sprite=UI_Circle_64, alpha=0.3)
│       └── Handle (80x80, sprite=UI_Circle_64, alpha=0.7)
├── SkillButtons (400x300, anchor右下 -30,50)
│   ├── AttackBtn (130x130, Button, sprite=UI_Skill_Slot_BG)
│   ├── Skill1 (100x100, Button, icon=UI_Skill_Icon_MultiShot)
│   ├── Skill2 (100x100, Button, icon=UI_Skill_Icon_Pierce)
│   └── Skill3 (100x100, Button, icon=UI_Skill_Icon_BattleShout)
├── PauseButton (60x60, anchor右上 -20,-130, sprite=UI_Btn_Pause)
└── PausePanel (全屏, 默认隐藏, Image黑色alpha=0.65)
    └── Content (340x380, 居中)
        ├── TopBar (装饰条5px)
        ├── Title ("游戏暂停", 36号粗体金色)
        ├── SepLine (分隔线)
        ├── ResumeButton ("继续游戏", Button)
        ├── RetryButton ("重试", Button)
        └── MenuButton ("返回主菜单", Button)
```

## 当前状态与待验证项

**编译状态:** 待验证（需要创建Prefab后才能运行）

**需要在Unity中手动创建：**
- [ ] `Assets/Resources/Prefabs/UI/StageSelectCanvas.prefab` — 按上方层级结构搭建
- [ ] `Assets/Resources/Prefabs/UI/StageCard.prefab` — 按上方层级结构搭建，挂载StageCardUI脚本并拖入引用

**需要 Play Mode 逐屏验证的界面:**
- [ ] MainMenu — 背景图/Logo/按钮sprite/金币icon
- [ ] StageSelect — Prefab加载/100卡片生成/滚动/点击/状态颜色
- [ ] GameScene HUD — 顶部banner/肖像帧/圆形按钮/技能栏
- [ ] 商店面板 — Tab sprite/商品行帧/购买按钮
- [ ] 角色信息 — 面板BG/肖像帧/属性icon/装备槽
- [ ] 技能升级 — 技能slot grid
- [ ] 加载界面 — BG + 进度条

**已知未提供但代码中有 fallback 的资源:** 见上方"尚未到位的资源"列表。

## 注意事项

1. **只改视觉/布局，不改游戏逻辑** — 所有按钮 onClick 回调保持原样
2. **Sprite 路径必须与磁盘一致** — `Resources.Load` 路径不带 .png 后缀，大小写敏感
3. **9-slice sprite 必须在 Unity Inspector 中设置好 Border** — 否则 `Image.Type.Sliced` 不会正确拉伸
4. **SceneSetup 文件很大** — GameSceneSetup 约3500行，改动前先定位具体方法和行号
5. **GameSceneSetup 负责创建 GameScene 中所有面板** — ShopPanel、CharacterInfoScreen、SkillUpgradePanel 等的 GameObject 都由它创建，面板类自身只负责内容填充
