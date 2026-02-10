using UnityEngine;
using System;

namespace MoShou.Core
{
    /// <summary>
    /// 战斗统计 - 追踪单场战斗的各项数据
    /// </summary>
    [Serializable]
    public class BattleStats
    {
        public int killCount = 0;
        public int totalDamageDealt = 0;
        public int totalDamageTaken = 0;
        public int criticalHits = 0;
        public int skillsUsed = 0;
        public float survivalTime = 0f;
        public int wavesCompleted = 0;
        public int goldCollected = 0;
        public int expCollected = 0;
        public int itemsDropped = 0;
        public int bossesKilled = 0;
        public int maxCombo = 0;
        public int currentCombo = 0;
        private float lastHitTime = 0f;
        private const float COMBO_TIMEOUT = 2f;

        /// <summary>
        /// 重置所有统计数据
        /// </summary>
        public void Reset()
        {
            killCount = 0;
            totalDamageDealt = 0;
            totalDamageTaken = 0;
            criticalHits = 0;
            skillsUsed = 0;
            survivalTime = 0f;
            wavesCompleted = 0;
            goldCollected = 0;
            expCollected = 0;
            itemsDropped = 0;
            bossesKilled = 0;
            maxCombo = 0;
            currentCombo = 0;
            lastHitTime = 0f;
        }

        /// <summary>
        /// 记录击杀
        /// </summary>
        public void RecordKill(bool isBoss = false)
        {
            killCount++;
            if (isBoss)
            {
                bossesKilled++;
            }
        }

        /// <summary>
        /// 记录伤害输出
        /// </summary>
        public void RecordDamageDealt(int damage, bool isCritical = false)
        {
            totalDamageDealt += damage;

            if (isCritical)
            {
                criticalHits++;
            }

            // 更新连击
            UpdateCombo();
        }

        /// <summary>
        /// 记录受到伤害
        /// </summary>
        public void RecordDamageTaken(int damage)
        {
            totalDamageTaken += damage;
            // 受伤重置连击
            currentCombo = 0;
        }

        /// <summary>
        /// 记录技能使用
        /// </summary>
        public void RecordSkillUse()
        {
            skillsUsed++;
        }

        /// <summary>
        /// 记录收集金币
        /// </summary>
        public void RecordGoldCollected(int amount)
        {
            goldCollected += amount;
        }

        /// <summary>
        /// 记录收集经验
        /// </summary>
        public void RecordExpCollected(int amount)
        {
            expCollected += amount;
        }

        /// <summary>
        /// 记录掉落物品
        /// </summary>
        public void RecordItemDropped()
        {
            itemsDropped++;
        }

        /// <summary>
        /// 记录波次完成
        /// </summary>
        public void RecordWaveCompleted()
        {
            wavesCompleted++;
        }

        /// <summary>
        /// 更新连击计数
        /// </summary>
        private void UpdateCombo()
        {
            float currentTime = Time.time;

            if (currentTime - lastHitTime < COMBO_TIMEOUT)
            {
                currentCombo++;
            }
            else
            {
                currentCombo = 1;
            }

            lastHitTime = currentTime;

            if (currentCombo > maxCombo)
            {
                maxCombo = currentCombo;
            }
        }

        /// <summary>
        /// 更新存活时间
        /// </summary>
        public void UpdateSurvivalTime(float deltaTime)
        {
            survivalTime += deltaTime;
        }

        /// <summary>
        /// 获取DPS (每秒伤害)
        /// </summary>
        public float GetDPS()
        {
            if (survivalTime <= 0) return 0;
            return totalDamageDealt / survivalTime;
        }

        /// <summary>
        /// 获取暴击率
        /// </summary>
        public float GetCriticalRate()
        {
            int totalHits = criticalHits + (killCount * 3); // 假设每个击杀3次攻击
            if (totalHits <= 0) return 0;
            return (float)criticalHits / totalHits * 100f;
        }

        /// <summary>
        /// 获取格式化的存活时间
        /// </summary>
        public string GetFormattedSurvivalTime()
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// 获取战斗评分 (0-100)
        /// </summary>
        public int GetBattleScore()
        {
            int score = 0;

            // 击杀分 (每杀1个怪物+2分, 最高40分)
            score += Mathf.Min(killCount * 2, 40);

            // DPS分 (每100DPS +5分, 最高20分)
            score += Mathf.Min(Mathf.FloorToInt(GetDPS() / 100f) * 5, 20);

            // 连击分 (每10连击+5分, 最高15分)
            score += Mathf.Min(maxCombo / 10 * 5, 15);

            // 存活分 (每分钟+5分, 最高15分)
            score += Mathf.Min(Mathf.FloorToInt(survivalTime / 60f) * 5, 15);

            // Boss击杀加成 (每Boss +10分)
            score += bossesKilled * 10;

            return Mathf.Clamp(score, 0, 100);
        }

        /// <summary>
        /// 获取评级 (S/A/B/C/D)
        /// </summary>
        public string GetGrade()
        {
            int score = GetBattleScore();

            if (score >= 90) return "S";
            if (score >= 75) return "A";
            if (score >= 55) return "B";
            if (score >= 35) return "C";
            return "D";
        }

        /// <summary>
        /// 转换为字符串（用于调试）
        /// </summary>
        public override string ToString()
        {
            return $"BattleStats: Kills={killCount}, DmgDealt={totalDamageDealt}, DmgTaken={totalDamageTaken}, " +
                   $"Crits={criticalHits}, Skills={skillsUsed}, Time={GetFormattedSurvivalTime()}, " +
                   $"Waves={wavesCompleted}, MaxCombo={maxCombo}, Score={GetBattleScore()}({GetGrade()})";
        }
    }

    /// <summary>
    /// 战斗统计管理器 - 单例模式
    /// </summary>
    public class BattleStatsManager : MonoBehaviour
    {
        public static BattleStatsManager Instance { get; private set; }

        public BattleStats CurrentStats { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                CurrentStats = new BattleStats();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            // 更新存活时间（仅在游戏进行中）
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                CurrentStats.UpdateSurvivalTime(Time.deltaTime);
            }
        }

        /// <summary>
        /// 开始新战斗时调用
        /// </summary>
        public void StartNewBattle()
        {
            CurrentStats.Reset();
            Debug.Log("[BattleStatsManager] New battle started, stats reset");
        }

        /// <summary>
        /// 结束战斗时调用
        /// </summary>
        public BattleStats EndBattle()
        {
            Debug.Log($"[BattleStatsManager] Battle ended: {CurrentStats}");
            return CurrentStats;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
