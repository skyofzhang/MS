using System;
using System.Collections.Generic;
using UnityEngine;
using MoShou.Data;

namespace MoShou.Systems
{
    /// <summary>
    /// 装备属性汇总结构
    /// </summary>
    public struct EquipmentStats
    {
        public int attack;
        public int defense;
        public int health;
        public float critRate;
    }

    /// <summary>
    /// 装备管理器 - 管理玩家装备的穿戴和卸下
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }

        // 装备槽位（6个）
        private Dictionary<EquipmentSlot, Equipment> equippedItems = new Dictionary<EquipmentSlot, Equipment>();

        // 装备配置表
        private Dictionary<string, Equipment> equipmentDatabase = new Dictionary<string, Equipment>();

        // 事件
        public event Action<EquipmentSlot, Equipment> OnEquipmentChanged;
        public event Action OnStatsChanged;

        // 当前装备提供的总属性加成
        public int TotalAttackBonus { get; private set; }
        public int TotalDefenseBonus { get; private set; }
        public int TotalHpBonus { get; private set; }
        public float TotalCritRateBonus { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSlots();
                LoadEquipmentDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化所有装备槽位
        /// </summary>
        private void InitializeSlots()
        {
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems[slot] = null;
            }
        }

        /// <summary>
        /// 加载装备配置表
        /// </summary>
        private void LoadEquipmentDatabase()
        {
            // 从 Resources 加载装备配置
            TextAsset configFile = Resources.Load<TextAsset>("Configs/EquipmentConfigs");
            if (configFile != null)
            {
                try
                {
                    EquipmentConfigTable table = JsonUtility.FromJson<EquipmentConfigTable>(configFile.text);
                    foreach (var equip in table.equipments)
                    {
                        equipmentDatabase[equip.id] = equip;
                    }
                    Debug.Log($"[EquipmentManager] 加载了 {equipmentDatabase.Count} 件装备配置");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EquipmentManager] 解析装备配置失败: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[EquipmentManager] 未找到装备配置文件，使用默认配置");
                CreateDefaultEquipments();
            }
        }

        /// <summary>
        /// 创建默认装备（调试用）
        /// </summary>
        private void CreateDefaultEquipments()
        {
            // 默认武器
            equipmentDatabase["WPN_001"] = new Equipment
            {
                id = "WPN_001",
                name = "新手木剑",
                slot = EquipmentSlot.Weapon,
                quality = EquipmentQuality.White,
                level = 1,
                attackBonus = 5,
                defenseBonus = 0,
                hpBonus = 0,
                critRateBonus = 0.01f,
                description = "一把简陋的木剑"
            };

            // 默认护甲
            equipmentDatabase["ARM_001"] = new Equipment
            {
                id = "ARM_001",
                name = "布甲",
                slot = EquipmentSlot.Armor,
                quality = EquipmentQuality.White,
                level = 1,
                attackBonus = 0,
                defenseBonus = 3,
                hpBonus = 10,
                critRateBonus = 0,
                description = "普通的布制护甲"
            };

            // ===== 掉落表引用的装备（补全） =====

            // 武器
            equipmentDatabase["WPN_002"] = new Equipment
            {
                id = "WPN_002",
                name = "铁剑",
                slot = EquipmentSlot.Weapon,
                quality = EquipmentQuality.Green,
                level = 5,
                attackBonus = 15,
                defenseBonus = 0,
                hpBonus = 0,
                critRateBonus = 0.03f,
                description = "普通铁剑，攻击+15"
            };

            equipmentDatabase["WPN_003"] = new Equipment
            {
                id = "WPN_003",
                name = "精钢剑",
                slot = EquipmentSlot.Weapon,
                quality = EquipmentQuality.Blue,
                level = 10,
                attackBonus = 30,
                defenseBonus = 0,
                hpBonus = 0,
                critRateBonus = 0.05f,
                description = "精钢锻造的利剑，攻击+30"
            };

            equipmentDatabase["WPN_004"] = new Equipment
            {
                id = "WPN_004",
                name = "猎人弓",
                slot = EquipmentSlot.Weapon,
                quality = EquipmentQuality.Green,
                level = 8,
                attackBonus = 20,
                defenseBonus = 0,
                hpBonus = 0,
                critRateBonus = 0.04f,
                description = "远程武器，攻击+20"
            };

            // 护甲
            equipmentDatabase["ARM_002"] = new Equipment
            {
                id = "ARM_002",
                name = "皮甲",
                slot = EquipmentSlot.Armor,
                quality = EquipmentQuality.Green,
                level = 5,
                attackBonus = 0,
                defenseBonus = 8,
                hpBonus = 25,
                critRateBonus = 0,
                description = "轻型护甲，防御+8"
            };

            equipmentDatabase["ARM_003"] = new Equipment
            {
                id = "ARM_003",
                name = "锁子甲",
                slot = EquipmentSlot.Armor,
                quality = EquipmentQuality.Blue,
                level = 10,
                attackBonus = 0,
                defenseBonus = 15,
                hpBonus = 50,
                critRateBonus = 0,
                description = "中型护甲，防御+15"
            };

            // 头盔
            equipmentDatabase["HLM_001"] = new Equipment
            {
                id = "HLM_001",
                name = "皮帽",
                slot = EquipmentSlot.Helmet,
                quality = EquipmentQuality.White,
                level = 1,
                attackBonus = 0,
                defenseBonus = 2,
                hpBonus = 5,
                critRateBonus = 0,
                description = "简单的皮制帽子"
            };

            equipmentDatabase["HLM_002"] = new Equipment
            {
                id = "HLM_002",
                name = "铁盔",
                slot = EquipmentSlot.Helmet,
                quality = EquipmentQuality.Green,
                level = 5,
                attackBonus = 0,
                defenseBonus = 5,
                hpBonus = 15,
                critRateBonus = 0.02f,
                description = "坚固的铁制头盔"
            };

            // 商店追加装备
            equipmentDatabase["ARM_004"] = new Equipment
            {
                id = "ARM_004",
                name = "板甲",
                slot = EquipmentSlot.Armor,
                quality = EquipmentQuality.Purple,
                level = 15,
                attackBonus = 0,
                defenseBonus = 25,
                hpBonus = 80,
                critRateBonus = 0,
                description = "重型板甲，防御力极高"
            };

            // ===== 高属性装备 =====

            // 武器 - 高级
            equipmentDatabase["WPN_005"] = new Equipment
            {
                id = "WPN_005", name = "暗影之刃", slot = EquipmentSlot.Weapon,
                quality = EquipmentQuality.Purple, level = 15,
                attackBonus = 50, defenseBonus = 0, hpBonus = 0, critRateBonus = 0.08f,
                description = "暗影锻造的魔剑，攻击+50，暴击+8%"
            };
            equipmentDatabase["WPN_006"] = new Equipment
            {
                id = "WPN_006", name = "龙牙大剑", slot = EquipmentSlot.Weapon,
                quality = EquipmentQuality.Orange, level = 20,
                attackBonus = 80, defenseBonus = 5, hpBonus = 50, critRateBonus = 0.12f,
                description = "传说中用龙牙打造的神器"
            };
            equipmentDatabase["WPN_007"] = new Equipment
            {
                id = "WPN_007", name = "雷霆战弓", slot = EquipmentSlot.Weapon,
                quality = EquipmentQuality.Purple, level = 15,
                attackBonus = 45, defenseBonus = 0, hpBonus = 20, critRateBonus = 0.10f,
                description = "蕴含雷电之力的战弓"
            };

            // 护甲 - 高级
            equipmentDatabase["ARM_005"] = new Equipment
            {
                id = "ARM_005", name = "秘银甲", slot = EquipmentSlot.Armor,
                quality = EquipmentQuality.Purple, level = 15,
                attackBonus = 0, defenseBonus = 35, hpBonus = 120, critRateBonus = 0,
                description = "秘银锻造的轻便重甲"
            };
            equipmentDatabase["ARM_006"] = new Equipment
            {
                id = "ARM_006", name = "龙鳞战甲", slot = EquipmentSlot.Armor,
                quality = EquipmentQuality.Orange, level = 20,
                attackBonus = 10, defenseBonus = 50, hpBonus = 200, critRateBonus = 0.03f,
                description = "龙鳞编织的传说战甲"
            };

            // 头盔 - 高级
            equipmentDatabase["HLM_003"] = new Equipment
            {
                id = "HLM_003", name = "狮鹫头盔", slot = EquipmentSlot.Helmet,
                quality = EquipmentQuality.Blue, level = 10,
                attackBonus = 5, defenseBonus = 10, hpBonus = 30, critRateBonus = 0.02f,
                description = "狮鹫骑士的头盔"
            };
            equipmentDatabase["HLM_004"] = new Equipment
            {
                id = "HLM_004", name = "暗金王冠", slot = EquipmentSlot.Helmet,
                quality = EquipmentQuality.Purple, level = 15,
                attackBonus = 10, defenseBonus = 15, hpBonus = 60, critRateBonus = 0.05f,
                description = "暗金打造的王冠"
            };

            // 护腿 - 补全
            equipmentDatabase["PNT_001"] = new Equipment
            {
                id = "PNT_001", name = "布裤", slot = EquipmentSlot.Pants,
                quality = EquipmentQuality.White, level = 1,
                attackBonus = 0, defenseBonus = 2, hpBonus = 5, critRateBonus = 0,
                description = "普通的布裤"
            };
            equipmentDatabase["PNT_002"] = new Equipment
            {
                id = "PNT_002", name = "皮裤", slot = EquipmentSlot.Pants,
                quality = EquipmentQuality.Green, level = 5,
                attackBonus = 0, defenseBonus = 5, hpBonus = 12, critRateBonus = 0,
                description = "结实的皮裤"
            };
            equipmentDatabase["PNT_003"] = new Equipment
            {
                id = "PNT_003", name = "铁甲护腿", slot = EquipmentSlot.Pants,
                quality = EquipmentQuality.Blue, level = 10,
                attackBonus = 0, defenseBonus = 10, hpBonus = 25, critRateBonus = 0,
                description = "铁甲护腿，防御+10"
            };
            equipmentDatabase["PNT_004"] = new Equipment
            {
                id = "PNT_004", name = "暗影行者", slot = EquipmentSlot.Pants,
                quality = EquipmentQuality.Purple, level = 15,
                attackBonus = 8, defenseBonus = 18, hpBonus = 50, critRateBonus = 0.04f,
                description = "暗影行者护腿"
            };

            // 戒指 - 补全
            equipmentDatabase["RNG_001"] = new Equipment
            {
                id = "RNG_001", name = "铜戒指", slot = EquipmentSlot.Ring,
                quality = EquipmentQuality.White, level = 1,
                attackBonus = 2, defenseBonus = 0, hpBonus = 0, critRateBonus = 0.01f,
                description = "普通的铜戒指"
            };
            equipmentDatabase["RNG_002"] = new Equipment
            {
                id = "RNG_002", name = "银戒指", slot = EquipmentSlot.Ring,
                quality = EquipmentQuality.Green, level = 5,
                attackBonus = 5, defenseBonus = 0, hpBonus = 0, critRateBonus = 0.03f,
                description = "精美的银戒指"
            };
            equipmentDatabase["RNG_003"] = new Equipment
            {
                id = "RNG_003", name = "烈焰戒指", slot = EquipmentSlot.Ring,
                quality = EquipmentQuality.Blue, level = 10,
                attackBonus = 12, defenseBonus = 0, hpBonus = 10, critRateBonus = 0.05f,
                description = "蕴含烈焰之力的戒指"
            };
            equipmentDatabase["RNG_004"] = new Equipment
            {
                id = "RNG_004", name = "霜龙之戒", slot = EquipmentSlot.Ring,
                quality = EquipmentQuality.Purple, level = 15,
                attackBonus = 20, defenseBonus = 5, hpBonus = 30, critRateBonus = 0.08f,
                description = "封印霜龙之力的神戒"
            };

            // 项链 - 补全
            equipmentDatabase["NCK_001"] = new Equipment
            {
                id = "NCK_001", name = "护身符", slot = EquipmentSlot.Necklace,
                quality = EquipmentQuality.White, level = 1,
                attackBonus = 0, defenseBonus = 1, hpBonus = 10, critRateBonus = 0,
                description = "带来好运的护身符"
            };
            equipmentDatabase["NCK_002"] = new Equipment
            {
                id = "NCK_002", name = "生命吊坠", slot = EquipmentSlot.Necklace,
                quality = EquipmentQuality.Green, level = 5,
                attackBonus = 0, defenseBonus = 2, hpBonus = 30, critRateBonus = 0,
                description = "增强生命力的神奇吊坠"
            };
            equipmentDatabase["NCK_003"] = new Equipment
            {
                id = "NCK_003", name = "守护者圣物", slot = EquipmentSlot.Necklace,
                quality = EquipmentQuality.Blue, level = 10,
                attackBonus = 3, defenseBonus = 8, hpBonus = 60, critRateBonus = 0.02f,
                description = "守护者的圣物"
            };
            equipmentDatabase["NCK_004"] = new Equipment
            {
                id = "NCK_004", name = "不灭心脏", slot = EquipmentSlot.Necklace,
                quality = EquipmentQuality.Purple, level = 15,
                attackBonus = 8, defenseBonus = 12, hpBonus = 120, critRateBonus = 0.05f,
                description = "传说之物，不灭心脏"
            };

            Debug.Log($"[EquipmentManager] 已创建 {equipmentDatabase.Count} 个默认装备配置");
        }

        /// <summary>
        /// 穿戴装备
        /// </summary>
        /// <param name="equipmentId">装备ID</param>
        /// <returns>被替换下来的装备（如果有）</returns>
        public Equipment Equip(string equipmentId)
        {
            if (!equipmentDatabase.TryGetValue(equipmentId, out Equipment newEquip))
            {
                Debug.LogWarning($"[EquipmentManager] 装备不存在: {equipmentId}");
                return null;
            }

            return Equip(newEquip.Clone());
        }

        /// <summary>
        /// 穿戴装备（直接传入装备对象）
        /// </summary>
        public Equipment Equip(Equipment equipment)
        {
            if (equipment == null) return null;

            EquipmentSlot slot = equipment.slot;
            Equipment oldEquip = equippedItems[slot];

            equippedItems[slot] = equipment;
            RecalculateStats();

            Debug.Log($"[EquipmentManager] 穿戴装备: {equipment.name} -> {slot}");
            OnEquipmentChanged?.Invoke(slot, equipment);
            OnStatsChanged?.Invoke();

            return oldEquip;
        }

        /// <summary>
        /// 卸下装备
        /// </summary>
        /// <param name="slot">装备槽位</param>
        /// <returns>卸下的装备</returns>
        public Equipment Unequip(EquipmentSlot slot)
        {
            Equipment oldEquip = equippedItems[slot];
            if (oldEquip == null)
            {
                Debug.Log($"[EquipmentManager] 槽位 {slot} 没有装备");
                return null;
            }

            equippedItems[slot] = null;
            RecalculateStats();

            Debug.Log($"[EquipmentManager] 卸下装备: {oldEquip.name} <- {slot}");
            OnEquipmentChanged?.Invoke(slot, null);
            OnStatsChanged?.Invoke();

            return oldEquip;
        }

        /// <summary>
        /// 获取指定槽位的装备
        /// </summary>
        public Equipment GetEquipment(EquipmentSlot slot)
        {
            return equippedItems.TryGetValue(slot, out Equipment equip) ? equip : null;
        }

        /// <summary>
        /// 获取所有已穿戴的装备
        /// </summary>
        public Dictionary<EquipmentSlot, Equipment> GetAllEquipments()
        {
            return new Dictionary<EquipmentSlot, Equipment>(equippedItems);
        }

        /// <summary>
        /// 根据ID获取装备配置
        /// </summary>
        public Equipment GetEquipmentConfig(string equipmentId)
        {
            return equipmentDatabase.TryGetValue(equipmentId, out Equipment equip) ? equip.Clone() : null;
        }

        /// <summary>
        /// 获取所有装备配置
        /// </summary>
        public List<Equipment> GetAllEquipmentConfigs()
        {
            return new List<Equipment>(equipmentDatabase.Values);
        }

        /// <summary>
        /// 获取装备总属性（供UI显示）
        /// </summary>
        public EquipmentStats GetTotalStats()
        {
            return new EquipmentStats
            {
                attack = TotalAttackBonus,
                defense = TotalDefenseBonus,
                health = TotalHpBonus,
                critRate = TotalCritRateBonus
            };
        }

        /// <summary>
        /// 重新计算装备总属性
        /// </summary>
        private void RecalculateStats()
        {
            TotalAttackBonus = 0;
            TotalDefenseBonus = 0;
            TotalHpBonus = 0;
            TotalCritRateBonus = 0;

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    TotalAttackBonus += kvp.Value.attackBonus;
                    TotalDefenseBonus += kvp.Value.defenseBonus;
                    TotalHpBonus += kvp.Value.hpBonus;
                    TotalCritRateBonus += kvp.Value.critRateBonus;
                }
            }

            Debug.Log($"[EquipmentManager] 属性重算: ATK+{TotalAttackBonus}, DEF+{TotalDefenseBonus}, HP+{TotalHpBonus}, CRIT+{TotalCritRateBonus:P1}");
        }

        /// <summary>
        /// 保存装备数据（返回可序列化的数据）
        /// </summary>
        public Dictionary<string, string> GetSaveData()
        {
            var saveData = new Dictionary<string, string>();
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    saveData[kvp.Key.ToString()] = kvp.Value.id;
                }
            }
            return saveData;
        }

        /// <summary>
        /// 加载装备数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, string> saveData)
        {
            InitializeSlots();
            foreach (var kvp in saveData)
            {
                if (Enum.TryParse(kvp.Key, out EquipmentSlot slot))
                {
                    Equip(kvp.Value);
                }
            }
        }
    }
}
