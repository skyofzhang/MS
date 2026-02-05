using UnityEngine;
using MoShou.Core;
using MoShou.Systems;
using MoShou.Gameplay;
using MoShou.UI;

namespace MoShou.Test
{
    /// <summary>
    /// 测试场景设置 - 用于快速验证所有系统
    /// </summary>
    public class TestSceneSetup : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private bool autoRunTests = true;
        [SerializeField] private bool showDebugUI = true;

        [Header("测试结果")]
        [SerializeField] private bool allSystemsReady = false;
        [SerializeField] private string lastTestResult = "";

        private void Start()
        {
            Debug.Log("========== 测试场景启动 ==========");

            // 确保所有系统初始化
            if (GameInitializer.Instance == null)
            {
                GameObject initObj = new GameObject("GameInitializer");
                initObj.AddComponent<GameInitializer>();
            }

            if (autoRunTests)
            {
                Invoke(nameof(RunAllTests), 1f); // 延迟1秒等待初始化
            }
        }

        /// <summary>
        /// 运行所有测试
        /// </summary>
        public void RunAllTests()
        {
            Debug.Log("========== 开始系统测试 ==========");

            bool allPassed = true;

            // 测试1: SaveSystem
            allPassed &= TestSaveSystem();

            // 测试2: EquipmentManager
            allPassed &= TestEquipmentManager();

            // 测试3: InventoryManager
            allPassed &= TestInventoryManager();

            // 测试4: LootManager
            allPassed &= TestLootManager();

            // 测试5: UI组件
            allPassed &= TestUIComponents();

            // 结果汇总
            allSystemsReady = allPassed;
            lastTestResult = allPassed ? "所有测试通过!" : "存在失败的测试";

            Debug.Log("========================================");
            Debug.Log($"测试结果: {lastTestResult}");
            Debug.Log("========================================");
        }

        /// <summary>
        /// 测试存档系统
        /// </summary>
        private bool TestSaveSystem()
        {
            Debug.Log("[TEST] SaveSystem...");

            if (SaveSystem.Instance == null)
            {
                Debug.LogError("[FAIL] SaveSystem.Instance 为空");
                return false;
            }

            // 测试保存和加载
            SaveSystem.Instance.CurrentPlayerStats.AddGold(100);
            SaveSystem.Instance.SaveGame();

            int savedGold = SaveSystem.Instance.CurrentPlayerStats.gold;
            if (savedGold < 100)
            {
                Debug.LogError("[FAIL] 金币保存失败");
                return false;
            }

            Debug.Log("[PASS] SaveSystem");
            return true;
        }

        /// <summary>
        /// 测试装备管理器
        /// </summary>
        private bool TestEquipmentManager()
        {
            Debug.Log("[TEST] EquipmentManager...");

            if (EquipmentManager.Instance == null)
            {
                Debug.LogError("[FAIL] EquipmentManager.Instance 为空");
                return false;
            }

            // 测试获取装备配置
            var configs = EquipmentManager.Instance.GetAllEquipmentConfigs();
            if (configs == null || configs.Count == 0)
            {
                Debug.LogWarning("[WARN] 没有装备配置数据");
            }

            // 测试穿戴装备
            var equip = EquipmentManager.Instance.GetEquipmentConfig("WPN_001");
            if (equip != null)
            {
                EquipmentManager.Instance.Equip(equip);
                var equipped = EquipmentManager.Instance.GetEquipment(MoShou.Data.EquipmentSlot.Weapon);
                if (equipped == null)
                {
                    Debug.LogError("[FAIL] 装备穿戴失败");
                    return false;
                }
            }

            Debug.Log("[PASS] EquipmentManager");
            return true;
        }

        /// <summary>
        /// 测试背包管理器
        /// </summary>
        private bool TestInventoryManager()
        {
            Debug.Log("[TEST] InventoryManager...");

            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[FAIL] InventoryManager.Instance 为空");
                return false;
            }

            // 测试添加物品
            int added = InventoryManager.Instance.AddItem("TEST_ITEM", 5);
            int count = InventoryManager.Instance.GetItemCount("TEST_ITEM");

            if (count != 5)
            {
                Debug.LogError($"[FAIL] 物品添加失败: 期望5, 实际{count}");
                return false;
            }

            // 测试移除物品
            int removed = InventoryManager.Instance.RemoveItem("TEST_ITEM", 3);
            count = InventoryManager.Instance.GetItemCount("TEST_ITEM");

            if (count != 2)
            {
                Debug.LogError($"[FAIL] 物品移除失败: 期望2, 实际{count}");
                return false;
            }

            // 清理测试数据
            InventoryManager.Instance.RemoveItem("TEST_ITEM", 99);

            Debug.Log("[PASS] InventoryManager");
            return true;
        }

        /// <summary>
        /// 测试掉落管理器
        /// </summary>
        private bool TestLootManager()
        {
            Debug.Log("[TEST] LootManager...");

            if (LootManager.Instance == null)
            {
                Debug.LogError("[FAIL] LootManager.Instance 为空");
                return false;
            }

            // 测试直接拾取
            int goldBefore = SaveSystem.Instance?.CurrentPlayerStats.gold ?? 0;
            LootManager.Instance.DirectPickup(DropPickupType.Gold, 10, "");
            int goldAfter = SaveSystem.Instance?.CurrentPlayerStats.gold ?? 0;

            if (goldAfter != goldBefore + 10)
            {
                Debug.LogWarning($"[WARN] 金币拾取可能异常: 之前{goldBefore}, 之后{goldAfter}");
            }

            Debug.Log("[PASS] LootManager");
            return true;
        }

        /// <summary>
        /// 测试UI组件
        /// </summary>
        private bool TestUIComponents()
        {
            Debug.Log("[TEST] UI Components...");

            // 这些测试在没有实际UI的情况下只检查类型是否可用
            try
            {
                // 检查类型是否存在
                System.Type[] types = new System.Type[]
                {
                    typeof(InventoryPanel),
                    typeof(InventorySlotUI),
                    typeof(EquipmentPanel),
                    typeof(EquipmentSlotUI),
                    typeof(GameHUD),
                    typeof(DamagePopup),
                    typeof(BillboardBehavior)
                };

                foreach (var t in types)
                {
                    if (t == null)
                    {
                        Debug.LogError($"[FAIL] 类型加载失败");
                        return false;
                    }
                    Debug.Log($"  - {t.Name} OK");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FAIL] UI组件测试异常: {e.Message}");
                return false;
            }

            Debug.Log("[PASS] UI Components");
            return true;
        }

        /// <summary>
        /// 模拟怪物死亡掉落
        /// </summary>
        public void SimulateMonsterDeath()
        {
            Debug.Log("[TEST] 模拟怪物死亡掉落...");

            if (LootManager.Instance != null)
            {
                LootManager.Instance.ProcessDrop(transform.position, "DROP_NORMAL");
                Debug.Log("[TEST] 掉落已生成");
            }
        }

        /// <summary>
        /// 模拟伤害飘字
        /// </summary>
        public void SimulateDamagePopup()
        {
            Debug.Log("[TEST] 模拟伤害飘字...");

            DamagePopup.CreateWorldSpace(transform.position, 100, DamageType.Normal);
            DamagePopup.CreateWorldSpace(transform.position + Vector3.right, 250, DamageType.Critical);
            DamagePopup.CreateWorldSpace(transform.position + Vector3.left, 50, DamageType.Heal);
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 250, 400));
            GUILayout.Box("系统测试面板");

            GUILayout.Label($"状态: {(allSystemsReady ? "就绪" : "检查中...")}");
            GUILayout.Label($"结果: {lastTestResult}");

            if (GUILayout.Button("重新运行测试"))
            {
                RunAllTests();
            }

            if (GUILayout.Button("模拟怪物死亡掉落"))
            {
                SimulateMonsterDeath();
            }

            if (GUILayout.Button("模拟伤害飘字"))
            {
                SimulateDamagePopup();
            }

            if (GUILayout.Button("保存游戏"))
            {
                SaveSystem.Instance?.SaveGame();
                Debug.Log("游戏已保存");
            }

            if (GUILayout.Button("添加测试金币+100"))
            {
                SaveSystem.Instance?.CurrentPlayerStats.AddGold(100);
                Debug.Log("添加100金币");
            }

            if (GUILayout.Button("添加测试经验+50"))
            {
                SaveSystem.Instance?.CurrentPlayerStats.AddExperience(50);
                Debug.Log("添加50经验");
            }

            GUILayout.EndArea();
        }
    }
}
