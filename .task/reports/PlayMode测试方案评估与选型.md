# PlayMode 测试方案评估与选型

**文档版本**: V1.0  
**日期**: 2026-02-06  
**目的**: 在「先不实施」的前提下，评估你提供的 PlayMode 说明与当前 MoShou 现状，选出适合本地自动化测试的方案。  
**关联**: PlayMode 测试环节说明与快速尝试指南、知识库 §6 RULE-DONE、本地 Agent（Cursor）执行测试。

---

## 一、现状对照

### 1.1 你的说明文档 vs 本仓库（MoShou）

| 项目 | 说明文档中的描述 | MoShou 当前状态 |
|------|------------------|-----------------|
| 项目路径 | `D:\AI\AI_project\AI_project` | `E:\AI_Project\MS\MoShou` |
| Unity 路径 | `D:\soft\unity2022\2022.3.47f1c1\Editor\Unity.exe` | 你本机可能是 `D:\program\2022.3.47f1c1\Editor\Unity.exe`（以 current_task 为准） |
| Unity 版本 | 2022.3.47f1c1 | ✅ 一致（ProjectVersion.txt） |
| Unity Test Framework | ✅ 已安装 1.1.31 | ❌ **未安装**（Packages/manifest.json 无 com.unity.test-framework） |
| 测试目录 Assets/Tests/PlayMode/ | 需创建 | ❌ 不存在 |
| PlayMode 测试脚本 | SamplePlayModeTest.cs 示例 | ❌ 不存在 |
| 命令行 -runTests -testPlatform PlayMode | 支持 | ✅ 支持（需先安装 Test Framework） |

说明：文档里的路径与「卡皮巴拉对决」等其它项目可能一致；本评估以 **MoShou（我叫MT之魔兽归来）** 为准。

### 1.2 MoShou 已有测试相关能力

| 能力 | 位置 | 说明 |
|------|------|------|
| **VisualSelfTest** | Assets/Editor/VisualSelfTest.cs | 菜单「MoShou/Visual Self Test」「MoShou/Run All Visual Tests (Console)」。EditMode：VC-E01～E09（场景、Build、脚本、资源、占位符）。PlayMode：VC-001～008（玩家、血条、怪物、技能、地面、Canvas 等）。**依赖 Application.isPlaying**：未进 Play 时只跑 EditMode。 |
| **TestSceneSetup** | Assets/Scripts/Test/TestSceneSetup.cs | 运行时挂到场景上，进 Play 后跑 SaveSystem/Equipment/Inventory/Loot/UI 等逻辑测试；**非** NUnit，无 TestResults.xml。 |
| **知识库 §6** | RULE-DONE-002～004 | 要求：PlayMode 可进、Visual Self Test 8/8 通过、核心循环可体验。完成报告格式含「PlayMode ✓」「视觉测试 8/8」。 |

结论：**知识库要求的「视觉测试 8/8」已由 VisualSelfTest 实现，但当前只能在编辑器里手动/菜单跑，且 PlayMode 部分必须已进入播放模式**；**没有** Unity Test Framework 的 NUnit 用例，也**没有**命令行可出的 TestResults.xml。

---

## 二、方案对比

### 方案 A：仅引入 Unity Test Framework（按说明文档最小化）

- **做法**：安装 `com.unity.test-framework`，创建 `Assets/Tests/PlayMode/SamplePlayModeTest.cs`，仅 2 个简单用例（Application.isPlaying、Time 推进），命令行用 `-runTests -testPlatform PlayMode -batchmode -testResults xxx.xml`。
- **优点**：实现快、与文档一致、有标准 XML、便于本地 Agent 解析和上报 N8N。
- **缺点**：与知识库 VC-001～008 无对应关系，无法替代「视觉自测 8/8」；只验证「能进 PlayMode、时间在走」。
- **适用**：先打通「本地 Agent 能跑 PlayMode 并拿到结果文件」，再逐步加用例。

### 方案 B：UTF PlayMode + 逐步对齐 VC-001～008

- **做法**：在方案 A 基础上，新增若干 `[UnityTest]`，内容上对齐 VisualSelfTest 的 VC-001～008（如加载 GameScene、FindWithTag("Player")、血条/怪物/技能等存在性），输出仍为 TestResults.xml。
- **优点**：与知识库 §6、RULE-DONE-003 一致；一份命令行即可得到「8/8」级别的可解析结果。
- **缺点**：要写 8 个以上用例，且需处理场景加载顺序、等待生成怪物等时序问题；实现量和稳定性风险都更高。
- **适用**：希望「本地全自动测试」直接覆盖 §6 的 8 项视觉检查时采用。

### 方案 C：保留 VisualSelfTest，用 -executeMethod 在 batchmode 跑并写文件

- **做法**：在 VisualSelfTest 或新 Editor 脚本中增加一个 **static** 方法，在 batchmode 下可被 `-executeMethod` 调用：先进入 PlayMode（若 Unity 支持在 batchmode 下执行一段 Play），跑 RunAllTests()，把结果写入 `.task/visual_test_result.json`（或固定路径）。本地 Agent 执行 Unity -batchmode -executeMethod xxx -quit，再读该 JSON。
- **优点**：直接复用现有 VC-E01～E09 与 VC-001～008 逻辑，与知识库一字不差。
- **缺点**：Unity 在 **-batchmode** 下默认**不进入** PlayMode；`-executeMethod` 在 Editor 启动时执行，此时 `Application.isPlaying == false`，只会跑 EditMode 部分，**拿不到 VC-001～008**。若要通过脚本「自动进 Play」再跑测试，需要依赖 Test Framework 的 PlayMode 或额外设计，和方案 A/B 会重合。
- **适用**：仅当「只跑 EditMode 检查（VC-E01～E09）」就满足当前阶段时可用；若要 8/8 视觉，仍需 PlayMode，建议用 A 或 B。

### 方案 D：双轨——UTF 做「可自动化基础」，VisualSelfTest 做「人工/半自动增强」

- **做法**：方案 A 的 UTF PlayMode 只负责「编译 + 进 PlayMode + 1～2 个基础用例」，产出 TestResults.xml 供 Agent 解析上报；VC-001～008 仍由人在合适时机在编辑器里跑 Visual Self Test，或后续再做成 UTF 用例（即过渡到方案 B）。
- **优点**：落地最快、风险最小；先有「可自动跑、可上报」的基线，再迭代。
- **缺点**：8/8 视觉检查未完全自动化，与「全自动化测试」有差距。
- **适用**：当前阶段优先「打通闭环」而非「一次到位 8/8 全自动」时选用。

### 方案 E：仅编译 + 构建，不做 PlayMode

- **做法**：本地 Agent 只执行：编译（或 Unity -batchmode -quit）+ 可选 BuildScript.BuildAndroid，根据 log 判断成功/失败并上报。
- **优点**：实现量最小，无需 Test Framework、无需新脚本。
- **缺点**：不符合知识库 §6「PlayMode 可进、视觉测试 8/8」的要求，只能算「构建验证」，不是「全自动化测试」。
- **适用**：仅作「最快验证闭环」的临时方案，不作为长期选型。

---

## 三、选型建议

### 推荐：**方案 D（双轨）** 作为当前阶段首选

| 理由 | 说明 |
|------|------|
| 与「先不着急做、先评估选型」一致 | 不一次性上满 8 个 VC 用例，先确立「本地可跑、可出结果、可上报」的路径。 |
| 与知识库可兼容 | §6 的「视觉测试 8/8」仍以 VisualSelfTest 为权威定义；UTF 先覆盖「PlayMode 能进、基础可用」，后续再逐步把 VC-001～008 迁到 UTF（演进到方案 B）。 |
| 实现成本与风险可控 | 只需安装 Test Framework、建一个 PlayMode 目录、写 1～2 个简单 [UnityTest]、本地 Agent 跑一条命令行并解析 XML；不立刻改 VisualSelfTest，也不强求 batchmode 下跑满 8 项。 |
| 为后续扩展留口子 | 一旦 D 跑通，加用例即可向 B 靠拢；N8N/报告格式可先按「TestResults.xml + 可选 visual_test_result」设计。 |

### 备选

- 若你**明确要求「本地全自动」必须包含 8/8 视觉且不想保留双轨**，则选 **方案 B**，并接受较多实现与调试量。
- 若当前**只求「能跑通一次命令行 PlayMode」做验证**，可先做 **方案 A**（最小 UTF），再在下一阶段决定是走 D 还是 B。

### 不推荐

- **方案 C** 作为「自动跑满 VC-001～008」的手段：batchmode 下不进入 Play，无法直接复用现有 PlayMode 分支。
- **方案 E** 作为长期方案：无法满足 §6 的 PlayMode/视觉要求。

---

## 四、若采用方案 D 的后续步骤（仅列出，不实施）

1. **依赖**：在 MoShou 的 `Packages/manifest.json` 中增加 `com.unity.test-framework`（版本与 2022.3 兼容，如 1.1.31）。
2. **目录与脚本**：创建 `Assets/Tests/PlayMode/`，新增一个最小 PlayMode 测试类（如 2 个 [UnityTest]：Application.isPlaying、Time 推进），与说明文档中的示例一致，路径改为本仓库（E:\AI_Project\MS\MoShou）。
3. **命令行**：本地 Agent（Cursor 或脚本）使用你本机 Unity 路径与项目路径，执行 `-runTests -testPlatform PlayMode -batchmode -testResults <path> -logFile <path> -projectPath <MoShou>`。
4. **结果**：解析 TestResults.xml，写入 `.task/local_test_report.json`（或你与 N8N 约定的格式），失败时上报 N8N。
5. **VisualSelfTest**：暂不改为自动调用；仍由人在需要时在编辑器跑，或后续再增加 UTF 用例对齐 VC-001～008。

---

## 五、小结

| 项目 | 结论 |
|------|------|
| 说明文档与 MoShou 差异 | 文档路径/项目可能对应其它工程；MoShou 需单独安装 Test Framework 并创建 Tests/PlayMode。 |
| 现有资产 | VisualSelfTest 已实现 §6 的 8 项视觉检查，但依赖 Editor/Play，无命令行 XML；TestSceneSetup 为运行时逻辑测试，非 UTF。 |
| 推荐方案 | **方案 D（双轨）**：UTF PlayMode 做可自动化基础并产出 XML，VisualSelfTest 保留为 8/8 权威，后续再按需把 VC 迁入 UTF。 |
| 备选 | 方案 B（一次到位 8/8 全自动）或方案 A（最小 UTF 先跑通）。 |
| 下一步 | 确认选型后，再实施「依赖安装 + 目录与脚本 + 命令行与结果解析 + 上报格式」；本次仅评估与选型，不实施。 |
