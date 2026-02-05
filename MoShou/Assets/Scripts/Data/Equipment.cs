using System;
using UnityEngine;

namespace MoShou.Data
{
    /// <summary>
    /// 装备品质枚举
    /// </summary>
    public enum EquipmentQuality
    {
        White = 0,      // 白色 - 普通
        Green = 1,      // 绿色 - 优秀
        Blue = 2,       // 蓝色 - 精良
        Purple = 3,     // 紫色 - 史诗
        Orange = 4      // 橙色 - 传说
    }

    /// <summary>
    /// 装备槽位枚举
    /// </summary>
    public enum EquipmentSlot
    {
        Weapon = 0,     // 武器
        Helmet = 1,     // 头盔
        Armor = 2,      // 护甲
        Pants = 3,      // 护腿
        Ring = 4,       // 戒指
        Necklace = 5    // 项链
    }

    /// <summary>
    /// 装备数据类
    /// </summary>
    [Serializable]
    public class Equipment
    {
        public string id;               // 装备唯一ID
        public string name;             // 装备名称
        public EquipmentSlot slot;      // 装备槽位
        public EquipmentQuality quality;// 装备品质
        public int level;               // 装备等级要求

        // 属性加成
        public int attackBonus;         // 攻击力加成
        public int defenseBonus;        // 防御力加成
        public int hpBonus;             // 生命值加成
        public float critRateBonus;     // 暴击率加成 (0-1)

        public string iconPath;         // 图标路径
        public string description;      // 装备描述

        /// <summary>
        /// 获取品质对应的颜色
        /// </summary>
        public Color GetQualityColor()
        {
            switch (quality)
            {
                case EquipmentQuality.White:  return Color.white;
                case EquipmentQuality.Green:  return new Color(0.2f, 0.8f, 0.2f);
                case EquipmentQuality.Blue:   return new Color(0.2f, 0.4f, 1f);
                case EquipmentQuality.Purple: return new Color(0.6f, 0.2f, 0.8f);
                case EquipmentQuality.Orange: return new Color(1f, 0.5f, 0f);
                default: return Color.white;
            }
        }

        /// <summary>
        /// 获取槽位名称
        /// </summary>
        public string GetSlotName()
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:   return "武器";
                case EquipmentSlot.Helmet:   return "头盔";
                case EquipmentSlot.Armor:    return "护甲";
                case EquipmentSlot.Pants:    return "护腿";
                case EquipmentSlot.Ring:     return "戒指";
                case EquipmentSlot.Necklace: return "项链";
                default: return "未知";
            }
        }

        /// <summary>
        /// 克隆装备数据
        /// </summary>
        public Equipment Clone()
        {
            return new Equipment
            {
                id = this.id,
                name = this.name,
                slot = this.slot,
                quality = this.quality,
                level = this.level,
                attackBonus = this.attackBonus,
                defenseBonus = this.defenseBonus,
                hpBonus = this.hpBonus,
                critRateBonus = this.critRateBonus,
                iconPath = this.iconPath,
                description = this.description
            };
        }
    }

    /// <summary>
    /// 装备配置表（用于JSON反序列化）
    /// </summary>
    [Serializable]
    public class EquipmentConfigTable
    {
        public Equipment[] equipments;
    }
}
