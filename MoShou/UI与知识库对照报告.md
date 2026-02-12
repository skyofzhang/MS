# UI 与知识库对照报告

**依据文档**  
- 04 UI系统策划案（执行版）：https://www.notion.so/04-UI-2fdba9f99f588130b830f7f93a8db371  
- T05 UI原型图（执行层详细版）：https://www.notion.so/T05-UI-2faba9f99f5881ae84c3e15d4934ce28  

**工程路径**：`e:\AI_Project\MS\MoShou`（Unity 工程）  
**报告目的**：对比知识库与项目工程在 UI 切换、布局、散图资源等方面差异，便于决定下一步执行。

---

## 一、界面统一映射表：知识库 vs 工程

| 知识库 ID | 知识库类名 | 知识库说明 | 工程对应 | 工程场景/类 | 备注 |
|-----------|------------|------------|----------|-------------|------|
| UI_001 | UISplash | 启动画面，Logo+进度+自动跳主界面 | ❌ 无 | - | 工程直接进 MainMenu，无独立 Splash |
| UI_002 | (已合并) | 登录界面 MVP 不做 | - | - | 一致 |
| UI_003 | UIMainMenu | 主界面：关卡入口+底部导航+状态栏 | ⚠️ 部分 | MainMenu 场景 + MainMenuSceneSetup / MainMenuScreen | 工程主界面为 5 按钮(开始/继续/角色/设置/退出)，**无关卡卡片**；关卡在 StageSelect |
| UI_004 | UISettings | 设置界面 弹窗 | ✅ 有 | SettingsPanel, MainMenu 内设置弹窗 | 存在 |
| UI_005 | UILevelSelect | 关卡选择 全屏 | ✅ 有 | StageSelect 场景, StageSelectScreen, StageSelectSceneSetup | 知识库写「合并在 UIMainMenu」，工程为**独立场景** |
| UI_006 | UIGameplay | 战斗 HUD 全屏 | ✅ 有 | GameScene + UIManager + GameHUD 等 | 存在，实现方式不同 |
| UI_007 | UIPause | 暂停界面 弹窗 | ✅ 有 | UIManager.ShowPausePanel, 战斗内暂停面板 | 存在 |
| UI_008 | UIBattleResult | 胜利结算 弹窗 | ✅ 有 | ResultScreen, GameManager.ShowVictoryPanel | 存在 |
| UI_009 | UIBattleResult | 失败结算 弹窗 | ✅ 有 | DefeatScreen, GameManager.ShowDefeatPanel | 存在 |
| UI_010 | UIRevive | 复活确认 弹窗 | ❌ 无 | - | 工程无「看广告复活」流程 |
| UI_011 | UITeam | 角色信息/队伍 全屏 | ✅ 有 | CharacterInfoScreen | 工程为「角色」入口，功能近似 |
| UI_012 | UIInventory | 背包界面 全屏 | ✅ 有 | SimpleInventoryPanel, InventoryPanel | 存在，战斗内/主菜单等多处使用 |
| UI_013 | UIEquip | 装备界面 全屏 | ✅ 有 | SimpleEquipmentPanel, EquipmentPanel | 存在 |
| UI_014 | UISkillUpgrade | 技能界面 全屏 | ✅ 有 | SkillUpgradePanel | 存在 |
| UI_015 | UIEquipDetail | 装备详情弹窗 | ✅ 有 | SimpleEquipmentPanel 内详情弹窗 | 存在 |
| UI_016 | UISkillDetail | 技能升级弹窗 | ✅ 有 | SkillUpgradePanel 内详情 | 存在 |
| UI_017 | UIShop | 商店界面 全屏 | ✅ 有 | ShopPanel | 存在 |
| UI_018 | UIConfirmDialog | 通用确认弹窗 | ✅ 有 | ConfirmDialog (MoShou.UI) | 存在，DontDestroyOnLoad |
| UI_019 | UILoading | 加载界面 全屏 | ⚠️ 部分 | LoadingManager | 有异步加载与进度条逻辑，但**无独立 Loading 场景/预制体**，且未被主流程统一使用 |
| UI_020 | UIToast | 浮动提示 HUD | ⚠️ 部分 | ShopPanel 内局部 Toast | **无全局 UIManager.ShowToast**，仅商店等单处自建 Toast |

---

## 二、UI 切换与流程差异

### 2.1 知识库规范（04 策划案）

- **UIManager 单例**：全局唯一，挂载在 DontDestroyOnLoad 的 Canvas 上。  
- **打开/关闭**：统一通过 `UIManager.Open<T>()` / `Close<T>()`。  
- **全屏规则**：同一时刻只允许一个全屏界面活跃；弹窗可叠加，最多 3 层。  
- **全屏切换**：打开新全屏时自动关闭当前全屏（或压栈），关闭时恢复上一个。  
- **转场动画**：  
  - 全屏→全屏：FadeOut 0.3s → FadeIn 0.3s  
  - 全屏→子界面：SlideInFromRight 0.3s OutCubic  
  - 弹窗打开：ScaleIn 0.25s easeOutBack + 蒙版 FadeIn 0.2s  
  - 弹窗关闭：ScaleOut 0.15s easeInBack + 蒙版 FadeOut 0.15s  
- **Toast**：`UIManager.ShowToast(string msg, float duration=2f)`，屏幕上方中央，同时只 1 条。  
- **红点**：底部导航背包/技能/商店有条件红点（8×8 红点，无数字）。  
- **返回键**：Android 返回键优先关弹窗，无弹窗则退出确认；战斗中打开暂停。  
- **安全区**：全屏内容在 SafeArea 内，用 `Screen.safeArea` 动态获取。

### 2.2 工程现状

- **无 `Open<T>()/Close<T>()`**：UIManager 仅有 `ShowPausePanel()`、`ShowVictoryPanel()`、`ShowDefeatPanel()`、`HideAllPanels()`，无泛型界面栈与统一打开/关闭接口。  
- **场景切换**：主流程为 `SceneManager.LoadScene("MainMenu"|"StageSelect"|"GameScene")`，无统一「全屏压栈/恢复」逻辑。  
- **转场**：无统一 Fade/Slide/Scale 规范；部分面板有 UIFeedbackSystem 的 ShowPanelWithBounce 等，未与 04 规范对齐。  
- **Toast**：无全局 `ShowToast`；ShopPanel 等自建局部 Toast，无法复用于「金币不足/背包已满」等全局提示。  
- **红点**：BottomNavigationBar 有 inventoryBadge/shopBadge，但未按 04 的「新装备未查看/可升级技能/可购买新物品」条件驱动，且知识库为 4 图标(背包/技能/商店/设置)，工程底栏为 5 项(Home/Hero/Bag/Shop/Settings)。  
- **返回键**：未在工程中检索到 Android 返回键统一处理（优先关弹窗、战斗中打开暂停）。  
- **安全区**：未在工程中检索到基于 `Screen.safeArea` 的布局适配。  
- **Loading**：LoadingManager 存在且支持异步加载与进度条，但进入关卡等仍多处直接 `LoadScene`，未统一经 Loading 界面。

---

## 三、布局与 T05 施工图差异

### 3.1 主界面 (UI_003 UIMainMenu)

| T05 元素 | T05 锚点/坐标/尺寸 | 工程情况 |
|----------|---------------------|----------|
| Logo_GameTitle | TopCenter (0,800) 600×200，logo_mt_title.png | 工程用 UI_MainMenu_Logo，CreateLogoArea，尺寸与 T05 可能不一致 |
| Panel_PlayerInfo | TopCenter (0,500) 300×150，panel_player_info_bg.png | 工程主菜单无「玩家信息条」；StageSelect 等另有状态展示 |
| Button_StartGame | Center (0,100) 400×120，btn_stone_normal.png | 工程有开始游戏按钮，路径 Sprites/UI/MainMenu/UI_Btn_Play 等，非 T05 资源名 |
| Button_Continue | Center (0,-50) 400×120 | 工程有继续游戏按钮 |
| Button_Inventory | BottomLeft (-350,-850) 200×100 | 工程主界面为 5 按钮竖排(开始/继续/角色/设置/退出)，无单独「背包」入口在底左；背包在 StageSelect/战斗内 |
| Button_Settings | BottomRight (350,-850) 200×100 | 工程有设置按钮 |
| Panel_GoldDisplay | BottomLeft (-480,-780) 200×64 | 工程主菜单有 Lv 等，金币显示在 TopStatusBar 或它处，布局非 T05 坐标 |
| Panel_LevelCards | anchorMin [0,0.12] anchorMax [1,0.88]，VerticalLayoutGroup | **工程主界面无关卡卡片**；关卡列表在 StageSelect 场景 |
| Panel_BottomNav | anchorMin [0,0] anchorMax [1,0.1]，4 项：队伍/战斗/装备/商店 | 工程 BottomNavigationBar 为 5 项(Home/Hero/Bag/Shop/Settings)，且主菜单场景不一定使用同一底栏组件 |

结论：主界面**结构不一致**——知识库是「主界面=关卡列表+底部 4 导航」；工程是「主界面=5 按钮(开始/继续/角色/设置/退出)」，关卡在独立 StageSelect 场景，底栏项数与命名也不同。

### 3.2 关卡选择 (UI_005)

- T05：Button_Back、Panel_LevelList、Button_Level_N、Panel_LevelDetail、Button_StartChallenge 等，有精确 RectTransform。  
- 工程：StageSelect 场景 + StageSelectScreen/StageSelectManager，有关卡列表与开始挑战，**未按 T05 的锚点/尺寸逐项对齐**，资源路径为 Sprites/UI/StageSelect/*、UI_Mockups/Screens/UI_StageSelect。

### 3.3 战斗 HUD (UI_006)

- T05：Panel_PlayerInfo TopLeft (-380,850) 300×120，Button_Pause TopRight (380,850) 80×80，Joystick BottomLeft (-380,-680) 200×200，Button_Skill1/2 BottomRight。  
- 工程：GameSceneSetup 中 CreateHUD、技能按钮、暂停等，布局为自建坐标，**未使用 T05 的施工规格表**；资源为 Sprites/UI/HUD/*、Sprites/UI/Skills/*、Sprites/UI/Buttons/*。

### 3.4 战斗结算 (UI_008/009)

- T05：Panel_Overlay、Panel_ResultCard、Txt_ResultTitle、Panel_Stars、Panel_Rewards、Panel_Buttons（胜利/失败不同按钮组），有详细 RectTransform 与动画说明。  
- 工程：ResultScreen、DefeatScreen，使用 Resources 的 UI_Result_Victory_BG、UI_Result_Defeat_BG、UI_Result_Star_*、UI_Btn_Primary_* 等，**布局与动画未按 T05 规范逐项实现**。

### 3.5 其余界面 (UIPause/UIEquip/UISkillUpgrade/UIShop/UITeam/UISettings/UIConfirmDialog/UILoading/UIToast)

- T05 对各界面均有 RectTransform、颜色、字体、间距等施工规格（含 1080×1920 参考分辨率、Scale With Screen Size）。  
- 工程：功能上多数存在，但**未系统按 T05 的 JSON/表结构生成或校验布局**，坐标与命名（Panel_*/Txt_*/Btn_*）为代码内手写，与 T05 不完全一致。

---

## 四、知识库有而工程没有的内容

1. **UI_001 UISplash**：启动画面（Logo+版本号+初始化进度条），Addressables 预加载完成后自动跳 UIMainMenu；工程无此界面。  
2. **UI_010 UIRevive**：死亡后复活确认弹窗（看广告复活/放弃、10 秒倒计时）；工程无复活流程。  
3. **全局 UIToast**：`UIManager.ShowToast(msg, duration)`，屏幕上方中央、同时 1 条、用于金币不足/背包已满等；工程无全局 Toast，仅局部自建。  
4. **UIManager 统一接口**：`Open<T>()/Close<T>()`、全屏栈、弹窗叠层上限 3；工程无。  
5. **转场动画规范**：全屏 Fade、子界面 SlideInFromRight、弹窗 ScaleIn/ScaleOut+蒙版；工程未统一实现。  
6. **红点系统**：按「新装备未查看/可升级技能/可购买新物品」驱动，8×8 红点；工程有 Badge 但未按该逻辑。  
7. **Android 返回键与安全区**：返回键优先关弹窗、战斗中打开暂停；安全区适配；工程未体现。  
8. **UILoading 作为统一入口**：关卡切换经 UILoading 显示进度与 Tips，最小显示 1s；工程有 LoadingManager 但未作为唯一关卡进入路径。  
9. **T05 级布局施工**：按 1080×1920、锚点/尺寸/颜色/间距的施工图逐项实现；工程布局为手写，未与 T05 表一一对应。

---

## 五、工程有而知识库没有或不一致的内容

1. **主界面 5 按钮**：开始游戏、继续游戏、角色、设置、退出（竖排），知识库主界面为「关卡入口+底部 4 导航」，无「退出」单独按钮。  
2. **主菜单与关卡选择拆成两场景**：MainMenu 与 StageSelect；知识库 04 写 UI_005 合并在 UIMainMenu，T05 又单独给出 UILevelSelect 施工图，工程采用独立场景。  
3. **BottomNav 5 项**：Home / Hero / Bag / Shop / Settings；知识库 04 为 4 项(背包/技能/商店/设置)，T05 为 队伍/战斗/装备/商店。  
4. **战斗内侧边按钮**：背包、商店、技能、装备等入口在战斗 HUD 侧边；知识库战斗 HUD 为血条/摇杆/技能/暂停，未写侧边多入口。  
5. **MinimapSystem**：小地图；知识库未列。  
6. **GameHUD 与 UIManager 并存**：GameScene 中 HUD 与面板由 GameSceneSetup 创建，UIManager 只负责暂停/胜利/失败面板；与知识库「单一 UIManager 管所有界面」不一致。  
7. **多处 SceneSetup 内建 UI**：MainMenuSceneSetup、StageSelectSceneSetup、GameSceneSetup 中大量用代码创建 UI，而非预制体+UIManager.Open；与知识库「界面 Prefab + 统一打开」不一致。  
8. **资源路径与 T05 资源名**：工程大量使用 `Sprites/UI/MainMenu/*`、`Sprites/UI/HUD/*`、`UI_Mockups/Screens/UI_*`；T05 出现 logo_mt_title.png、btn_stone_normal.png、panel_player_info_bg.png 等，工程中多为 UI_MainMenu_Logo、UI_Btn_Play、UI_HUD_HPBar_BG 等，**命名与层级不完全一致**。

---

## 六、散图资源差异

### 6.1 工程已引用但可能缺失或路径不一的资源

- **UI_Mockups/Screens/**：  
  - 代码引用：UI_MainMenu, UI_StageSelect, UI_Result, UI_Defeat, UI_CharacterInfo, UI_Inventory, UI_Settings。  
  - 工程 Resources 下已有：UI_Mockups/Screens/ 与 UI_Mockups/Components/ 部分文件（如 UI_MainMenu.png、UI_StageSelect.png、UI_BottomNav、UI_TopBar 等）；若代码还引用 UI_Result、UI_Defeat 等，需确认是否与 Screens 下文件名一致。  
- **Sprites/UI/MainMenu/**：  
  - 工程存在：UI_MainMenu_BG、UI_MainMenu_Logo、UI_MainMenu_Frame、UI_Btn_Play、UI_Btn_Continue、UI_Btn_Role、UI_Btn_Settings、UI_Btn_Quit 及 _Normal/_Pressed/_Disabled 等；与当前代码引用一致。  
- **Sprites/UI/HUD/**：  
  - 代码引用：UI_HUD_HPBar_BG、UI_HUD_HPBar_Fill、UI_HUD_Gold_Icon、UI_HUD_Wave_BG、UI_HUD_PlayerIcon_Frame；工程 HUD 目录存在部分（如 HPBar、Gold_Icon 等），需核对是否缺 EXPBar、Level_BG 等。  
- **Sprites/UI/Result/**：  
  - 代码引用：UI_Result_Victory_BG、UI_Result_Defeat_BG、UI_Result_Star_Filled、UI_Result_Star_Empty；工程 Result 目录存在，需确认文件名完全一致。  
- **Sprites/UI/Skills/**：  
  - 代码引用：UI_Skill_Slot_BG、UI_Skill_Icon_MultiShot/Pierce/BattleShout 等；工程有；SkillUpgradePanel 还引用 Precision/Vitality/Swift/Lifesteal 等图标，需确认 Sprites/UI/Skills 或 Sprites/Items 下是否存在。  
- **Sprites/UI/Buttons/**：  
  - 工程有 UI_Btn_Pause、UI_Btn_Close、UI_Btn_Primary_*、UI_Btn_Secondary_Normal 等；与 UIManager/GameSceneSetup 引用一致。  
- **Configs**：  
  - 代码引用 Configs/ShopConfigs、Configs/SkillConfigs、Configs/LootConfigs 等；工程仅有 LootConfigs、EquipmentConfigs、GameSettings、StageConfigs、MonsterConfigs、ItemConfigs 等，**ShopConfigs/SkillConfigs 缺失或名不一致**（与《项目工程意见报告》一致）。

### 6.2 T05 提及但工程未统一使用的资源名

- logo_mt_title.png、panel_player_info_bg.png、btn_stone_normal.png、btn_icon_inventory.png、btn_icon_settings.png、btn_icon_back.png、btn_icon_pause.png、panel_list_bg.png、btn_level_normal.png、panel_detail_bg.png、game_logo_large.png、spinner_ring.png 等。  
- 工程以 UI_MainMenu_Logo、UI_Btn_Play、UI_HUD_* 等命名为主，**与 T05 资源表不完全对应**，若要对齐需做映射或重命名/替换资源。

### 6.3 工程有而知识库未列的资源

- Sprites/UI/Kenney/*、RPGKit/*、LevelUp/*、StageSelect/*（如 UI_Stage_Locked、UI_Stage_Unlocked、UI_Star_Filled 等）、Equipment 相关、Common（Dialog_BG、Tooltip_BG、Progress 等）。  
- UI_Mockups/Popups/*（UI_EquipDetail、UI_ItemDetail、UI_PauseMenu、UI_SellConfirm、UI_SkillUpgrade、UI_StageConfirm 等）。  
- 以上为工程实际使用的散图与 mockup，知识库未逐项列出。

---

## 七、其他不符汇总

1. **命名与类名**：知识库界面类名 UIMainMenu、UILevelSelect、UIGameplay 等；工程为 MainMenuSceneSetup、StageSelectScreen、GameSceneSetup、UIManager、ResultScreen、DefeatScreen、CharacterInfoScreen 等，**无统一 UIMainMenu/UILevelSelect 等类名**。  
2. **Canvas 与 DontDestroyOnLoad**：知识库要求 UIManager 在 DontDestroyOnLoad 的 Canvas 上；工程 UIManager 在 GameScene 的 Canvas 上，**随场景销毁**；仅 ConfirmDialog 使用 DontDestroyOnLoad。  
3. **战斗内面板**：背包/商店/技能/装备面板在 GameScene 内由 GameSceneSetup 创建并 SetActive 控制，**非通过 UIManager.Open<T>() 打开**。  
4. **结算与失败**：胜利/失败由 GameManager 调 UIManager.ShowVictoryPanel/ShowDefeatPanel，与知识库「UIBattleResult isVictory」一致，但界面层级与动画未按 04/T05 实现。  
5. **设置界面**：知识库要求 BGM/SFX 滑块、振动、画质、登出；工程 SettingsPanel 有音量等，**登出/画质等需逐项对照**。  
6. **技能数量与 ID**：知识库 04 为 6 技能（3 主动+3 被动），SkillUpgradePanel 默认多技能；工程与知识库技能 ID/数量需核对一致。  
7. **商店 Tab**：知识库为 装备/道具/充值；工程 ShopPanel 有分类，需确认是否完全一致。  
8. **背包**：知识库为 5 列 4 行 20 格/页、筛选全部/装备/消耗品、容量 50；工程 SimpleInventoryPanel/InventoryManager 需核对格数、筛选与容量。

---

## 八、建议的下一步（供你决定）

1. **补齐缺失界面**：实现 UISplash、UIRevive；统一 UILoading 为关卡进入必经路径。  
2. **统一 UI 切换**：引入 UIManager.Open<T>()/Close<T>() 与全屏栈，逐步将现有面板改为通过该接口打开/关闭，并统一转场动画。  
3. **全局 Toast**：实现 UIManager.ShowToast，并替换 ShopPanel 等局部 Toast 为全局调用。  
4. **主界面与关卡**：决定采用「知识库方案（主界面含关卡列表+底栏 4 项）」还是「保持工程方案（主菜单 5 按钮+独立 StageSelect）」；若选知识库，需合并主界面与关卡选择布局与流程。  
5. **底栏统一**：统一底部导航为 4 项(背包/技能/商店/设置) 或保留 5 项并与 04/T05 文档对齐命名与红点逻辑。  
6. **布局按 T05 施工**：选 1～2 个核心界面（如主界面、战斗 HUD）按 T05 的 RectTransform/颜色/字体表逐项实现或校验，再推广到其余界面。  
7. **资源命名与配置**：建立 T05 资源名与工程路径的映射表，或替换为 T05 命名；补齐 ShopConfigs、SkillConfigs 等配置与加载逻辑。  
8. **安全区与返回键**：实现 Android 返回键逻辑与 SafeArea 适配。

以上为 UI 与知识库在切换、布局、散图资源及行为上的差异与建议，可按优先级分步执行。
