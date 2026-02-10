using System;
using UnityEngine;

namespace MoShou.Data
{
    /// <summary>
    /// 物品类型枚举
    /// </summary>
    public enum ItemType
    {
        Equipment = 0,      // 装备
        Consumable = 1,     // 消耗品
        Material = 2,       // 材料
        Currency = 3        // 货币
    }

    /// <summary>
    /// 物品数据基类
    /// </summary>
    [Serializable]
    public class ItemData
    {
        public string id;               // 物品唯一ID
        public string name;             // 物品名称
        public ItemType type;           // 物品类型
        public string description;      // 物品描述
        public string iconPath;         // 图标路径
        public int maxStack;            // 最大堆叠数量（装备为1）
        public int sellPrice;           // 出售价格

        /// <summary>
        /// 是否可堆叠
        /// </summary>
        public bool IsStackable => maxStack > 1;

        /// <summary>
        /// 克隆
        /// </summary>
        public virtual ItemData Clone()
        {
            return new ItemData
            {
                id = this.id,
                name = this.name,
                type = this.type,
                description = this.description,
                iconPath = this.iconPath,
                maxStack = this.maxStack,
                sellPrice = this.sellPrice
            };
        }
    }

    /// <summary>
    /// 背包中的物品实例
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string itemId;           // 物品ID
        public int count;               // 数量
        public int slotIndex;           // 所在格子索引

        // 如果是装备，存储装备数据
        public Equipment equipmentData;

        public InventoryItem(string id, int amount, int slot)
        {
            itemId = id;
            count = amount;
            slotIndex = slot;
        }
    }

    /// <summary>
    /// 掉落物配置
    /// </summary>
    [Serializable]
    public class DropItem
    {
        public string itemId;           // 物品ID
        public float dropRate;          // 掉落概率 (0-1)
        public int minCount;            // 最小数量
        public int maxCount;            // 最大数量

        /// <summary>
        /// 计算实际掉落数量
        /// </summary>
        public int RollCount()
        {
            return UnityEngine.Random.Range(minCount, maxCount + 1);
        }

        /// <summary>
        /// 是否掉落
        /// </summary>
        public bool RollDrop()
        {
            return UnityEngine.Random.value <= dropRate;
        }
    }

    /// <summary>
    /// 掉落表配置
    /// </summary>
    [Serializable]
    public class DropTable
    {
        public string id;               // 掉落表ID
        public DropItem[] drops;        // 掉落物列表

        // 固定掉落（金币、经验）
        public int goldMin;
        public int goldMax;
        public int expMin;
        public int expMax;

        // 装备掉落配置
        public float equipmentChance;   // 装备掉落概率 (0-1)
        public string[] equipmentPool;  // 可掉落装备ID池

        /// <summary>
        /// 计算金币掉落
        /// </summary>
        public int RollGold()
        {
            return UnityEngine.Random.Range(goldMin, goldMax + 1);
        }

        /// <summary>
        /// 计算经验掉落
        /// </summary>
        public int RollExp()
        {
            return UnityEngine.Random.Range(expMin, expMax + 1);
        }

        /// <summary>
        /// 计算装备掉落
        /// </summary>
        /// <returns>掉落的装备ID，如果没有掉落返回null</returns>
        public string RollEquipment()
        {
            if (equipmentPool == null || equipmentPool.Length == 0)
                return null;

            if (UnityEngine.Random.value > equipmentChance)
                return null;

            // 从装备池中随机选择一件
            int index = UnityEngine.Random.Range(0, equipmentPool.Length);
            return equipmentPool[index];
        }
    }

    /// <summary>
    /// 掉落表配置表
    /// </summary>
    [Serializable]
    public class DropTableConfigTable
    {
        public DropTable[] dropTables;
    }
}
