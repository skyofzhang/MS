# 工作流状态机控制协议

## 状态流转图
```
idle (等待需求)
  ↓ NWF-01触发
planning (Claude写策划)
  ↓ 完成
quality_check (DeepSeek质检)
  ↓ 通过          ↓ 不通过
resource_pull    planning (返回修改)
  ↓
task_assign (分配任务)
  ↓
cursor_execute (Cursor执行)
  ↓
screenshot_review (Claude验收)
  ↓ 通过          ↓ 不通过
check_completion  task_assign (返回重做)
  ↓ 还有任务      ↓ 全部完成
task_assign      build_apk
                   ↓
                 human_review
                   ↓ 通过
                 completed → idle
```

## 状态转换规则

### Claude负责的转换
| 当前状态 | 动作 | 下一状态 |
|----------|------|----------|
| idle | 收到NWF-01通知 | planning |
| planning | 写完plan_draft.json | quality_check |
| quality_check | 读取通过的plan_review.json | resource_pull |
| quality_check | 读取失败的plan_review.json | planning |
| resource_pull | 资源拉取完成 | task_assign |
| task_assign | 写入current_task.json | cursor_execute |
| screenshot_review | 验收通过 | check_completion |
| screenshot_review | 验收失败 | task_assign |
| check_completion | 队列还有任务 | task_assign |
| check_completion | 队列为空 | build_apk |
| human_review | 人类说OK | completed |
| human_review | 人类说不OK | task_assign |
| completed | 自动 | idle |

### n8n负责的转换
| 触发事件 | 当前状态 | 动作 |
|----------|----------|------|
| Notion新需求 | idle | 更新状态为planning，通知Claude |
| Git push [PLAN_READY] | planning | 触发NWF-07质检 |
| 质检完成 | quality_check | 写入plan_review.json |
| Git push [TASK_RESULT] | cursor_execute | 通知Claude验收 |

### Cursor负责的转换
| 当前状态 | 动作 | 触发 |
|----------|------|------|
| cursor_execute | 完成任务，写task_result.json | Git push [TASK_RESULT] |
| build_apk | 打包完成 | Git push [APK_READY] |

## 状态更新方法

### Claude更新状态
```python
# 读取当前状态
state = read_json(".task/workflow_state.json")

# 更新状态
state["current_phase"] = "new_phase"
state["updated_at"] = now()
state["history"].append({
    "timestamp": now(),
    "from": old_phase,
    "to": "new_phase",
    "reason": "完成xxx"
})

# 写回
write_json(".task/workflow_state.json", state)

# Git提交
git commit -m "[STATE] planning → quality_check"
```

### n8n检测状态变化
监听Git push，检查commit message是否包含[STATE]标签

## 新增Git标签

| 标签 | 触发者 | 说明 |
|------|--------|------|
| [STATE] | Claude | 状态机状态变更 |
| [PLAN_READY] | Claude | 策划案写完，待质检 |
| [TASK_RESULT] | Cursor | 任务完成，待验收 |
| [APK_READY] | Cursor | APK打包完成 |

## Claude启动时检查

每次Claude启动，执行以下步骤：
1. 读取 workflow_state.json
2. 检查 current_phase
3. 根据当前阶段执行对应动作：
   - idle: 等待（或检查Notion是否有新需求）
   - planning: 继续写策划案
   - quality_check: 等待质检结果
   - resource_pull: 继续拉取资源
   - task_assign: 分配下一个任务
   - cursor_execute: 等待Cursor完成
   - screenshot_review: 验收截图
   - check_completion: 检查任务队列
   - build_apk: 等待APK
   - human_review: 等待人类反馈
   - completed: 重置为idle
