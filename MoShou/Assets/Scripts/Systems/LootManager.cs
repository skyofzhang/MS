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
                EnsureDropPickupPrefab();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 确保有掉落物预制体（运行时自动创建）
        /// </summary>
        private void EnsureDropPickupPrefab()
        {
            if (dropPickupPrefab != null) return;

            // 运行时创建一个掉落物模板对象
            dropPickupPrefab = new GameObject("DropPickup_RuntimePrefab");
            dropPickupPrefab.SetActive(false); // 模板不显示

            // 添加基础球体网格
            MeshFilter mf = dropPickupPrefab.AddComponent<MeshFilter>();
            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mf.sharedMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
            Destroy(tempSphere);

            // 添加渲染器
            MeshRenderer mr = dropPickupPrefab.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));

            // 添加触发碰撞器
            SphereCollider col = dropPickupPrefab.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;

            // 添加 DropPickup 组件
            dropPickupPrefab.AddComponent<DropPickup>();

            // 隐藏在场景中，不会被销毁
            DontDestroyOnLoad(dropPickupPrefab);

            Debug.Log("[LootManager] 运行时创建了掉落物预制体");
        }

        /// <summary>
        /// 加载掉落表配置
        /// </summary>
        private void LoadDropTables()
        {
            TextAsset configFile = Resources.Load<TextAsset>("Configs/LootConfigs");
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
                    new DropItem { itemId = "POTION_HP_SMALL", dropRate = 0.10f, minCount = 1, maxCount = 1 }
                },
                equipmentChance = 0.08f,  // 8%装备掉落概率
                equipmentPool = new string[] { "WPN_001", "ARM_001" }
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
                    new DropItem { itemId = "POTION_HP_MEDIUM", dropRate = 0.15f, minCount = 1, maxCount = 1 }
                },
                equipmentChance = 0.20f,  // 20%装备掉落概率
                equipmentPool = new string[] { "WPN_001", "WPN_002", "ARM_001", "ARM_002", "HLM_002" }
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
                    new DropItem { itemId = "POTION_HP_LARGE", dropRate = 0.50f, minCount = 1, maxCount = 2 }
                },
                equipmentChance = 0.60f,  // 60%装备掉落概率
                equipmentPool = new string[] { "WPN_002", "WPN_003", "WPN_004", "ARM_002", "ARM_003" }
            };

            Debug.Log("[LootManager] 已创建默认掉落表（含装备掉落配置）");
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

            // 掉落装备
            string equipmentId = table.RollEquipment();
            if (!string.IsNullOrEmpty(equipmentId))
            {
                SpawnDropPickup(position, DropPickupType.Equipment, 1, equipmentId);
                Debug.Log($"[LootManager] 掉落装备: {equipmentId}");
            }
        }

        /// <summary>
        /// 生成掉落物
        /// </summary>
        private void SpawnDropPickup(Vector3 basePosition, DropPickupType type, int amount, string itemId)
        {
            // 物品和装备需要检查背包空间
            if ((type == DropPickupType.Item || type == DropPickupType.Equipment)
                && InventoryManager.Instance != null && InventoryManager.Instance.IsFull())
            {
                Debug.Log($"[LootManager] 背包已满，丢弃掉落: {type} {itemId} x{amount}");
                return;
            }

            // 随机散布位置
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * dropSpreadRadius;
            Vector3 spawnPos = basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);

            if (dropPickupPrefab != null)
            {
                GameObject dropObj = Instantiate(dropPickupPrefab, spawnPos, Quaternion.identity);
                dropObj.SetActive(true);
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

                case DropPickupType.Equipment:
                    if (InventoryManager.Instance != null)
                    {
                        int added = InventoryManager.Instance.AddItem(itemId, 1);
                        if (added > 0)
                        {
                            OnItemPickup?.Invoke(itemId, 1);
                            Debug.Log($"[LootManager] 装备已添加到背包: {itemId}");
                        }
                        else
                        {
                            Debug.LogWarning($"[LootManager] 背包已满，无法拾取装备: {itemId}");
                        }
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
        Item,
        Equipment  // 装备掉落
    }
}
