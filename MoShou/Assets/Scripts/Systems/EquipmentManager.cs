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
