using System;

namespace MoShou.Data
{
    /// <summary>
    /// 怪物配置条目（对应 MonsterConfigs.json）
    /// </summary>
    [Serializable]
    public class MonsterConfigEntry
    {
        public string id;
        public string name;
        public string type;        // Normal/Elite/Boss
        public float baseHp;
        public float baseAttack;
        public float baseDefense;
        public float moveSpeed;
        public int goldDrop;
        public int expDrop;
    }

    /// <summary>
    /// 怪物配置表
    /// </summary>
    [Serializable]
    public class MonsterConfigTable
    {
        public MonsterConfigEntry[] monsters;
    }

    /// <summary>
    /// 关卡波次条目（对应 StageConfigs.json 中的 waves[]）
    /// </summary>
    [Serializable]
    public class StageWaveEntry
    {
        public string[] enemyIds;
        public float spawnDelay;
    }

    /// <summary>
    /// 关卡配置条目（对应 StageConfigs.json 中的 stages[]）
    /// </summary>
    [Serializable]
    public class StageConfigEntry
    {
        public int id;
        public string name;
        public int chapter;
        public string description;
        public int difficulty;
        public int recommendedLevel;
        public int waveCount;
        public StageWaveEntry[] waves;
        public int goldReward;
        public int expReward;
        public string backgroundMusic;
    }

    /// <summary>
    /// 关卡配置表
    /// </summary>
    [Serializable]
    public class StageConfigTable
    {
        public StageConfigEntry[] stages;
    }
}
