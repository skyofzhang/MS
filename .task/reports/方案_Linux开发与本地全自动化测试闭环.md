# 方案：Linux 开发 + 本地全自动化测试闭环

**你的设想**：  
- 开发与开发自测在 **Linux 服务器**上完成，由 **Claude** 做到自认为 100% 完成。  
- **N8N** 触发**本地 Agent** 拉取 Git 仓库，在**本地**执行**全自动化测试**。  
- 若出现问题，本地通过 **API 通知 N8N**，提交**测试报告**；N8N 将报告交给 **Claude**，由 Claude 完成修复。  

**结论**：该方案**可行且与知识库目标高度一致**，是「云端开发 + 本地真实验证」的清晰分工。下面做可行性拆解与落地要点。

---

## 一、方案结构（你描述的流程）

```
┌─────────────────────────────────────────────────────────────────┐
│  Linux 服务器（腾讯云等）                                          │
│  Claude 开发 + 开发自测 → 自认为 100% 完成 → Git push              │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  N8N                                                              │
│  监听 Git push（或 Claude 提交的 [READY_FOR_TEST]）                 │
│  → 触发「本地 Agent 拉取并执行全自动化测试」                         │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  本地（Windows / 你本机）                                           │
│  Agent: git pull → 运行 Unity 全自动化测试（PlayMode/视觉/APK 等）   │
│  → 成功：可选通知 N8N「通过」                                        │
│  → 失败：调用 N8N API，提交测试报告                                 │
└─────────────────────────────────────────────────────────────────┘
                                    │
                         失败时 POST 测试报告
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  N8N                                                              │
│  收到测试报告 → 写入 .task 或调用 Linux 上的 Claude                 │
│  → Claude 根据报告修复 → push → 再次触发「本地测试」                │
└─────────────────────────────────────────────────────────────────┘
```

---

## 二、可行性评估

### 2.1 优势（为什么这个方案好）

| 点 | 说明 |
|----|------|
| **环境一致** | Claude 始终在 Linux 上开发，同一套路径、同一套工具，无「有时在本地、有时在云」的漂移，符合「稳定开发环境」。 |
| **职责清晰** | Linux = 代码 + 自测（编译/静态/可做的批处理测试）；本地 = 只有本地才能做的「真机级」验证（Unity Editor PlayMode、视觉自测、真机/模拟器）。 |
| **闭环自动** | 测试失败 → API 报 N8N → Claude 修 → push → N8N 再触发本地，形成**自动修复循环**，人类只需在「多次修不过」或「发布前验收」时介入。 |
| **与知识库一致** | 「AI 为唯一大脑」在 Linux 上实现；「人类是物理限制突破助手」体现在本地只跑自动化 + 必要时人工验收；§6 完成检查中的 PlayMode/视觉/APK 在本地执行，符合「验证在真实环境」。 |

### 2.2 需要说清的两层「测试」

- **Linux 上 Claude 的「开发自测」（他自认为 100%）**  
  在**无 GUI、无真机**的前提下，只能做：  
  - 编译 0 Error（脚本/Editor 编译）；  
  - 可选：Unity **headless** 下的批量测试（如 `-runTests -testPlatform PlayMode` 等，若你们已配）；  
  - 可选：静态检查、配置校验。  
  因此「100%」应明确定义为：**在 Linux 可执行范围内的 100%**，而不是「包含 PlayMode 画面、真机安装」的 100%。  

- **本地的「全自动化测试」**  
  建议包含知识库 §6 中需要**真实运行环境**的部分，例如：  
  - 编译 + 进入 PlayMode（或打开指定场景）；  
  - VisualSelfTest（VC-001～008）；  
  - 核心循环脚本/场景跑通；  
  - 可选：APK 构建 + 安装到模拟器/真机 + 冒烟测试。  
  本地 Agent 用**同一套标准**（如 .task 中的完成检查清单）生成「通过/失败 + 报告」，再决定是否调用 N8N API。

### 2.3 风险与应对

| 风险 | 应对 |
|------|------|
| Claude「自认 100%」与本地结果不一致 | 在协议里约定：**只有本地测试通过才算通过**；Claude 的 100% 仅表示「Linux 自测通过，请本地验证」。N8N 触发的语义用「待本地验证」而不是「已发布」。 |
| 本地 Agent 与 N8N 的衔接不稳定 | 约定**单一 Webhook**：本地只调一个 N8N 地址（如 `/webhook/local-test-result`），payload 固定为「成功/失败 + 报告内容 + commit/branch」。N8N 内部分支处理。 |
| 报告格式 Claude 读不懂 | 报告**结构化**：JSON，含 `passed: bool`、`failed_steps: string[]`、`log_excerpt: string`、`screenshots_paths: string[]`（可选）、`commit`。Claude 读 .task 或 n8n 转写后的内容，按步骤修复。 |

---

## 三、落地要点（如何接上你现有体系）

### 3.1 Linux 侧：Claude 的「完成」与 push 约定

- Claude 在 Linux 上完成开发与自测后：  
  - **必须**：`git push`，并建议带固定 tag，例如 `[READY_FOR_TEST]`，方便 n8n 只对「待测」提交触发本地。  
  - **可选**：写 `.task/ready_for_test.json`，内容例如：`{"commit": "abc123", "branch": "main", "self_test_summary": "compile ok, headless tests ok"}`。  
- 这样 N8N 的触发条件可以是：**GitHub Webhook（push）** 且 commit message 含 `[READY_FOR_TEST]`，或 n8n 轮询 `.task/ready_for_test.json` 的更新（由 Claude push 带来）。

### 3.2 N8N：触发「本地 Agent 拉取并测试」

- N8N 收到「Claude 已 push [READY_FOR_TEST]」后：  
  - **方式一**：N8N 调用**本地暴露的 HTTP 接口**（本地 Agent 起一个简单 HTTP 服务），例如 `POST http://你的本机:端口/run-full-test`，body 可带 `repo、branch、commit`。  
  - **方式二**：本地 Agent **轮询** N8N 或 GitHub（例如「最近一次带 [READY_FOR_TEST] 的 commit」），拉取后执行测试；失败再报 N8N。  
  - 更推荐**方式一**：N8N 主动调本地，逻辑简单；本地需有固定 IP 或内网穿透/反向代理，以便 n8n 云服务器能访问。  
- 若 n8n 在**本机**运行，则 N8N 直接调 `localhost` 即可，无需公网。

### 3.3 本地 Agent：拉取 + 全自动化测试 + 上报

- **拉取**：`git pull`（或按 N8N 传入的 branch/commit 拉取）。  
- **测试**：  
  - 调用 Unity 命令行（如 `Unity -batchmode -runTests -testResults ...`），或你们已有的 **VisualSelfTest / PlayMode 脚本**；  
  - 收集结果：通过/失败、日志、截图路径（若有）。  
- **上报**：  
  - **成功**：可选 `POST /webhook/local-test-result`，body 如 `{"passed": true, "commit": "..."}`，便于 N8N 更新状态或通知人类。  
  - **失败**：**必须** `POST /webhook/local-test-result`，body 如：

```json
{
  "passed": false,
  "commit": "abc123",
  "branch": "main",
  "failed_steps": ["PlayMode Scene GameScene", "VC-003 血条显示"],
  "log_excerpt": "NullReferenceException in GameHUD.cs:42 ...",
  "screenshots": [".task/screenshots/run_xxx/01.png"],
  "timestamp": "2026-02-06T12:00:00Z"
}
```

- 本地 Agent 可以是：**PowerShell 脚本 + 定时/被 N8N 触发**，或你们已有的 `auto_executor.ps1` 的扩展；不一定要「常驻进程」，只要能在被调时执行 pull → test → POST 即可。

### 3.4 N8N：收测试报告 → 交给 Claude 修复

- N8N 收到 `/webhook/local-test-result` 且 `passed === false` 时：  
  1. **写 .task**：将上述 JSON 写入 `.task/local_test_report.json`（通过 GitHub API 或「能写 repo 的 runner」写回仓库），或写入 n8n 可读的存储。  
  2. **触发 Claude**：  
     - **若 Claude 在 Linux 上可由 n8n 间接调用**：n8n 调用 Linux 上的 runner，执行例如：  
       `claude -p "根据 .task/local_test_report.json 修复 MoShou 项目，修复后 push 并打 [READY_FOR_TEST]。" --allowedTools Read,Edit,Bash`  
     - 或 n8n 将报告内容通过 HTTP 发给「Linux 上跑着的 Claude Agent 服务」，由该服务再调 Claude。  
  3. Claude 修完 push 后，可再次触发「本地测试」（同上 3.2），形成闭环。

- 若**暂时无法**在 n8n 里直接触发 Linux 上的 Claude：  
  - 可先退化为：n8n 将 `local_test_report.json` 写回 repo，并更新 `workflow_state.json` 为「local_test_failed」；  
  - 人类或定时任务在 Linux 上跑 Claude，Claude 启动时读 `workflow_state.json` 与 `local_test_report.json`，执行修复后再 push，再由 n8n 触发本地重测。

---

## 四、与现有 PROTOCOL / 状态机的关系

- 在 **PROTOCOL.json** 或 **STATE_MACHINE.md** 中可增加阶段，例如：  
  - **cursor_execute** 之后（或改为 **claude_develop** 在 Linux 上完成）→ **ready_for_test**（Claude push [READY_FOR_TEST]）→ **local_test**（N8N 触发本地）→ 成功则 **screenshot_review / build_apk**，失败则 **local_test_failed** → Claude 修复 → 再 **ready_for_test**。  
- **current_task.json** 在「本地测试失败」时，可由 N8N 或 Claude 写为「修复 local_test_report 中的问题」，Claude 在 Linux 上只认 .task 与报告，不依赖 Cursor。

这样既保留你们现有「状态机 + .task」的写法，又把你说的「Linux 开发 + 本地全自动化测试 + N8N 拿报告给 Claude 修」嵌进去。

---

## 五、总结

| 问题 | 回答 |
|------|------|
| 这个方案可行吗？ | **可行**，且与「稳定 Linux 开发环境 + 本地真实验证」的目标一致。 |
| Claude 的「100%」指什么？ | 建议约定为「Linux 上可做的 100%」（编译 + 可选 headless 测试），不包含 PlayMode 画面/真机；最终通过以**本地全自动化测试**为准。 |
| N8N 的角色？ | 监听 Claude push → 触发本地 Agent；接收本地测试报告 API → 写 .task 并触发 Claude 修复；必要时更新 workflow_state。 |
| 本地要做什么？ | 提供「被 N8N 触发」的入口（HTTP 或轮询）；执行 git pull + 全自动化测试；失败时 POST 结构化报告到 N8N。 |

按上述约定实现后，即可形成「**Linux 开发与自测 → N8N 触发本地 → 本地全自动化测试 → 失败则 API 报 N8N → Claude 修复 → 再测**」的闭环，且与知识库中「AI 为主、人类仅关键点介入、§6 完成检查」一致。
