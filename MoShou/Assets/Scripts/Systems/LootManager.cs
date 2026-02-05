using System;
using System.Collections.Generic;
using UnityEngine;
using MoShou.Data;
using MoShou.Gameplay;

namespace MoShou.Systems
{
    /// <summary>
    /// 掉落管理器 - 处理怪物死亡掉落
    /// </summary>
    public class LootManager : MonoBehaviour
    {
        public static LootManager Instance { get; private set; }

        [Header("掉落设置")]
        [SerializeField] private float pickupRadius = 2f;       // 自动拾取半径
        [SerializeField] private float dropSpreadRadius = 1f;   // 掉落物散布半径
        [SerializeField] private GameObject dropPickupPrefab;   // 掉落物预制体

        // 掉落表配置
        private Dictionary<string, DropTable> dropTables = new Dictionary<string, DropTable>();

        // 事件
        public event Action<int> OnGoldPickup;
        public event Action<int> OnExpPickup;
        public event Action<string, int> OnItemPickup;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadDropTables();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 加载掉落表配置
        /// </summary>
        private void LoadDropTables()
        {
            TextAsset configFile = Resources.Load<TextAsset>("Configs/DropTableConfigs");
            if (configFile != null)
            {
                try
                {
                    DropTableConfigTable table = JsonUtility.FromJson<DropTableConfigTable>(configFile.text);
                    foreach (var dt in table.dropTables)
                    {
                        dropTables[dt.id] = dt;
                    }
                    Debug.Log($"[LootManager] 加载了 {dropTables.Count} 个掉落表");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LootManager] 解析掉落表失败: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[LootManager] 未找到掉落表配置，使用默认配置");
                CreateDefaultDropTables();
            }
        }

        /// <summary>
        /// 创建默认掉落表
        /// </summary>
        private void CreateDefaultDropTables()
        {
            // 普通怪物掉落表
            dropTables["DROP_NORMAL"] = new DropTable
            {
                id = "DROP_NORMAL",
                goldMin = 5,
                goldMax = 15,
                expMin = 10,
                expMax = 20,
                drops = new DropItem[]
                {
                    new DropItem { itemId = "WPN_001", dropRate = 0.05f, minCount = 1, maxCount = 1 },
                    new DropItem { itemId = "ARM_001", dropRate = 0.05f, minCount = 1, maxCount = 1 }
                }
            };

            // 精英怪物掉落表
            dropTables["DROP_ELITE"] = new DropTable
            {
                id = "DROP_ELITE",
                goldMin = 30,
                goldMax = 50,
                expMin = 50,
                expMax = 80,
                drops = new DropItem[]
                {
                    new DropItem { itemId = "WPN_001", dropRate = 0.15f, minCount = 1, maxCount = 1 },
                    new DropItem { itemId = "ARM_001", dropRate = 0.15f, minCount = 1, maxCount = 1 }
                }
            };

            // BOSS掉落表
            dropTables["DROP_BOSS"] = new DropTable
            {
                id = "DROP_BOSS",
                goldMin = 100,
                goldMax = 200,
                expMin = 200,
                expMax = 300,
                drops = new DropItem[]
                {
                    new DropItem { itemId = "WPN_001", dropRate = 0.5f, minCount = 1, maxCount = 1 },
                    new DropItem { itemId = "ARM_001", dropRate = 0.5f, minCount = 1, maxCount = 1 }
                }
            };
        }

        /// <summary>
        /// 处理怪物死亡掉落
        /// </summary>
        /// <param name="position">死亡位置</param>
        /// <param name="dropTableId">掉落表ID</param>
        public void ProcessDrop(Vector3 position, string dropTableId)
        {
            if (!dropTables.TryGetValue(dropTableId, out DropTable table))
            {
                table = dropTables.GetValueOrDefault("DROP_NORMAL");
                if (table == null)
                {
                    Debug.LogWarning($"[LootManager] 掉落表不存在: {dropTableId}");
                    return;
                }
            }

            // 掉落金币
            int gold = table.RollGold();
            if (gold > 0)
            {
                SpawnDropPickup(position, DropPickupType.Gold, gold, "");
            }

            // 掉落经验
            int exp = table.RollExp();
            if (exp > 0)
            {
                SpawnDropPickup(position, DropPickupType.Exp, exp, "");
            }

            // 掉落物品
            if (table.drops != null)
            {
                foreach (var drop in table.drops)
                {
                    if (drop.RollDrop())
                    {
                        int count = drop.RollCount();
                        SpawnDropPickup(position, DropPickupType.Item, count, drop.itemId);
                    }
                }
            }
        }

        /// <summary>
        /// 生成掉落物
        /// </summary>
        private void SpawnDropPickup(Vector3 basePosition, DropPickupType type, int amount, string itemId)
        {
            // 随机散布位置
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * dropSpreadRadius;
            Vector3 spawnPos = basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);

            if (dropPickupPrefab != null)
            {
                GameObject dropObj = Instantiate(dropPickupPrefab, spawnPos, Quaternion.identity);
                DropPickup pickup = dropObj.GetComponent<DropPickup>();
                if (pickup != null)
                {
                    pickup.Initialize(type, amount, itemId);
                }
            }
            else
            {
                // 没有预制体时直接拾取
                DirectPickup(type, amount, itemId);
            }
        }

        /// <summary>
        /// 直接拾取（无掉落物表现）
        /// </summary>
        public void DirectPickup(DropPickupType type, int amount, string itemId)
        {
            switch (type)
            {
                case DropPickupType.Gold:
                    if (SaveSystem.Instance != null)
                    {
                        SaveSystem.Instance.CurrentPlayerStats.AddGold(amount);
                        OnGoldPickup?.Invoke(amount);
                    }
                    break;

                case DropPickupType.Exp:
                    if (SaveSystem.Instance != null)
                    {
                        SaveSystem.Instance.CurrentPlayerStats.AddExperience(amount);
                        OnExpPickup?.Invoke(amount);
                    }
                    break;

                case DropPickupType.Item:
                    if (InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.AddItem(itemId, amount);
                        OnItemPickup?.Invoke(itemId, amount);
                    }
                    break;
            }
        }

        /// <summary>
        /// 获取自动拾取半径
        /// </summary>
        public float GetPickupRadius() => pickupRadius;

        /// <summary>
        /// 设置掉落物预制体
        /// </summary>
        public void SetDropPickupPrefab(GameObject prefab)
        {
            dropPickupPrefab = prefab;
        }
    }

    /// <summary>
    /// 掉落物类型
    /// </summary>
    public enum DropPickupType
    {
        Gold,
        Exp,
        Item
    }
}
