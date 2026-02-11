using System;
using UnityEngine;
using MoShou.Systems;

namespace MoShou.Data
{
    /// <summary>
    /// 玩家属性数据类
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        // 基础属性
        public int level = 1;
        public int experience = 0;
        public int gold = 0;
        public int gems = 0;  // 宝石货币

        // 战斗属性（基础值）
        public int baseMaxHp = 100;
        public int baseAttack = 10;
        public int baseDefense = 5;
        public float baseCritRate = 0.05f;      // 5% 基础暴击率
        public float baseCritDamage = 1.5f;     // 150% 暴击伤害

        // 当前状态
        public int currentHp;

        // 升级经验表
        private static readonly int[] ExpTable = { 0, 100, 250, 500, 850, 1300, 1900, 2600, 3500, 4600, 6000 };

        /// <summary>
        /// 获取当前等级升级所需经验
        /// </summary>
        public int GetExpToNextLevel()
        {
            if (level >= ExpTable.Length) return int.MaxValue;
            return ExpTable[level];
        }

        /// <summary>
        /// 获取总最大生命值（基础+装备）
        /// </summary>
        public int GetTotalMaxHp()
        {
            int equipBonus = EquipmentManager.Instance != null ? EquipmentManager.Instance.TotalHpBonus : 0;
            return baseMaxHp + equipBonus + (level - 1) * 10; // 每级+10HP
        }

        /// <summary>
        /// 获取总攻击力（基础+装备）
        /// </summary>
        public int GetTotalAttack()
        {
            int equipBonus = EquipmentManager.Instance != null ? EquipmentManager.Instance.TotalAttackBonus : 0;
            return baseAttack + equipBonus + (level - 1) * 2; // 每级+2ATK
        }

        /// <summary>
        /// 获取总防御力（基础+装备）
        /// </summary>
        public int GetTotalDefense()
        {
            int equipBonus = EquipmentManager.Instance != null ? EquipmentManager.Instance.TotalDefenseBonus : 0;
            return baseDefense + equipBonus + (level - 1) * 1; // 每级+1DEF
        }

        /// <summary>
        /// 获取总暴击率（基础+装备）
        /// </summary>
        public float GetTotalCritRate()
        {
            float equipBonus = EquipmentManager.Instance != null ? EquipmentManager.Instance.TotalCritRateBonus : 0;
            return Mathf.Clamp01(baseCritRate + equipBonus); // 暴击率上限100%
        }

        /// <summary>
        /// 增加经验值，自动处理升级
        /// </summary>
        /// <returns>是否升级</returns>
        public bool AddExperience(int amount)
        {
            experience += amount;
            bool leveledUp = false;

            while (experience >= GetExpToNextLevel() && level < ExpTable.Length)
            {
                experience -= GetExpToNextLevel();
                level++;
                leveledUp = true;
                Debug.Log($"[PlayerStats] 升级! 当前等级: {level}");
            }

            return leveledUp;
        }

        /// <summary>
        /// 增加金币
        /// </summary>
        public void AddGold(int amount)
        {
            gold += amount;
            Debug.Log($"[PlayerStats] 金币+{amount}, 当前: {gold}");
        }

        /// <summary>
        /// 消耗金币
        /// </summary>
        /// <returns>是否成功消耗</returns>
        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHp = Mathf.Max(0, currentHp - damage);
        }

        /// <summary>
        /// 恢复生命
        /// </summary>
        public void Heal(int amount)
        {
            currentHp = Mathf.Min(GetTotalMaxHp(), currentHp + amount);
        }

        /// <summary>
        /// 完全恢复
        /// </summary>
        public void FullRestore()
        {
            currentHp = GetTotalMaxHp();
        }

        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool IsDead => currentHp <= 0;

        /// <summary>
        /// 创建默认玩家属性
        /// </summary>
        public static PlayerStats CreateDefault()
        {
            var stats = new PlayerStats();

            // 从 GameSettings.json 读取基础属性（有配置用配置，无配置用字段默认值）
            if (ConfigManager.Instance?.IsLoaded == true)
            {
                var cfg = ConfigManager.Instance.Settings.playerSettings;
                stats.baseMaxHp = cfg.baseMaxHp;
                stats.baseAttack = cfg.baseAttack;
                stats.baseDefense = cfg.baseDefense;
                stats.baseCritRate = cfg.baseCritRate;
                stats.baseCritDamage = cfg.baseCritDamage;
                Debug.Log($"[PlayerStats] 从GameSettings加载基础属性: HP={cfg.baseMaxHp}, ATK={cfg.baseAttack}, DEF={cfg.baseDefense}");
            }

            stats.currentHp = stats.GetTotalMaxHp();
            return stats;
        }

        /// <summary>
        /// 克隆
        /// </summary>
        public PlayerStats Clone()
        {
            return new PlayerStats
            {
                level = this.level,
                experience = this.experience,
                gold = this.gold,
                gems = this.gems,
                baseMaxHp = this.baseMaxHp,
                baseAttack = this.baseAttack,
                baseDefense = this.baseDefense,
                baseCritRate = this.baseCritRate,
                baseCritDamage = this.baseCritDamage,
                currentHp = this.currentHp
            };
        }
    }
}
