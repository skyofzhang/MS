using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoShou.Utils;
using MoShou.UI;
using MoShou.Data;

/// <summary>
/// 怪物生成器 - 符合AI开发知识库§2 RULE-RES-002, §4怪物属性表
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float spawnInterval = 1.0f;    // 性能优化：0.5s -> 1.0s
    public int maxMonsters = 15;          // 性能优化：30 -> 15

    [Header("Wave Settings")]
    public int currentWave = 1;
    public int monstersPerWave = 10;      // 性能优化：20 -> 10
    public int wavesPerLevel = 3;         // 波次：2 -> 3

    // 怪物ID到Resources路径的映射 (知识库§2 RULE-RES-002)
    // 优先使用Prefab路径，其次使用FBX路径
    private static readonly Dictionary<string, string[]> MonsterPaths = new Dictionary<string, string[]>
    {
        { "MON_SLIME_001", new[] { "Prefabs/Characters/Monster_Slime", "Models/Monsters/Slime/Monster_Slime" } },
        { "MON_GOBLIN_001", new[] { "Prefabs/Characters/Monster_Goblin", "Models/Monsters/Goblin/Monster_Goblin" } },
        { "MON_WOLF_001", new[] { "Prefabs/Characters/Monster_Wolf", "Models/Monsters/Wolf/Monster_Wolf" } },
        { "MON_GOBLIN_ELITE_001", new[] { "Prefabs/Characters/Monster_GoblinElite", "Models/Monsters/GoblinElite/Monster_GoblinElite" } },
        { "BOSS_GOBLIN_KING", new[] { "Prefabs/Characters/Boss_GoblinKing", "Models/Monsters/GoblinKing/Boss_GoblinKing" } }
    };

    // 怪物属性表 (知识库§4) - 调整血量：约4次攻击击杀
    private static readonly Dictionary<string, MonsterData> MonsterStats = new Dictionary<string, MonsterData>
    {
        { "MON_SLIME_001", new MonsterData("史莱姆", 50, 5, 1, 2.0f, 10, 5) },           // HP: 15->50 (~4次攻击)
        { "MON_GOBLIN_001", new MonsterData("哥布林", 60, 8, 2, 1.5f, 15, 8) },          // HP: 25->60 (~4次攻击)
        { "MON_WOLF_001", new MonsterData("灰狼", 70, 12, 3, 3.0f, 20, 12) },            // HP: 35->70 (~5次攻击)
        { "MON_GOBLIN_ELITE_001", new MonsterData("精英哥布林", 120, 18, 5, 2.0f, 50, 30) }, // HP: 60->120 (~8次攻击)
        { "BOSS_GOBLIN_KING", new MonsterData("哥布林王", 200, 25, 8, 1.8f, 300, 200) }     // HP: 100->200 (~13次攻击)
    };

    private List<GameObject> activeMonsters = new List<GameObject>();
    private int monstersKilledThisWave = 0;
    private bool isSpawning = false;
    private Dictionary<string, GameObject> cachedPrefabs = new Dictionary<string, GameObject>();

    // 配置驱动的怪物数据（优先于硬编码 MonsterStats）
    private Dictionary<string, MonsterConfigEntry> configMonsterStats;
    // 当前关卡的配置数据（驱动波次 enemyIds）
    private StageConfigEntry currentStageConfig;

    /// <summary>
    /// 是否正在生成怪物
    /// </summary>
    public bool IsSpawning => isSpawning;

    public struct MonsterData
    {
        public string name;
        public float hp, atk, def, speed;
        public int exp, gold;

        public MonsterData(string n, float h, float a, float d, float s, int e, int g)
        {
            name = n; hp = h; atk = a; def = d; speed = s; exp = e; gold = g;
        }
    }

    void Awake()
    {
        Instance = this;

        // 强制覆盖序列化值，确保使用最新配置
        // Unity场景中保存的旧值会覆盖代码默认值，所以必须在Awake中强制设置
        spawnInterval = 1.0f;      // 性能优化：降低生成频率
        maxMonsters = 30;          // 同屏怪物上限
        monstersPerWave = 20;      // 每波20只
        wavesPerLevel = 3;         // 3波

        Debug.Log($"[Spawner] 强制配置: monstersPerWave={monstersPerWave}, maxMonsters={maxMonsters}, spawnInterval={spawnInterval}");

        // 加载配置文件（怪物属性 + 关卡波次）
        LoadConfigs();
    }

    /// <summary>
    /// 从配置文件加载怪物属性和关卡波次数据
    /// 失败时静默降级到硬编码数据
    /// </summary>
    void LoadConfigs()
    {
        // 加载怪物配置
        TextAsset monsterConfigFile = Resources.Load<TextAsset>("Configs/MonsterConfigs");
        if (monsterConfigFile != null)
        {
            try
            {
                MonsterConfigTable table = JsonUtility.FromJson<MonsterConfigTable>(monsterConfigFile.text);
                if (table?.monsters != null && table.monsters.Length > 0)
                {
                    configMonsterStats = new Dictionary<string, MonsterConfigEntry>();
                    foreach (var m in table.monsters)
                    {
                        configMonsterStats[m.id] = m;
                    }
                    Debug.Log($"[Spawner] 从MonsterConfigs加载了 {configMonsterStats.Count} 个怪物配置");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Spawner] 解析MonsterConfigs失败: {e.Message}，使用硬编码数据");
            }
        }

        // 加载关卡配置
        TextAsset stageConfigFile = Resources.Load<TextAsset>("Configs/StageConfigs");
        if (stageConfigFile != null)
        {
            try
            {
                StageConfigTable stageTable = JsonUtility.FromJson<StageConfigTable>(stageConfigFile.text);
                if (stageTable?.stages != null)
                {
                    int currentLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;
                    foreach (var stage in stageTable.stages)
                    {
                        if (stage.id == currentLevel)
                        {
                            currentStageConfig = stage;
                            wavesPerLevel = stage.waveCount;
                            Debug.Log($"[Spawner] 从StageConfigs加载关卡{currentLevel}配置: {stage.name}, {stage.waveCount}波");
                            break;
                        }
                    }
                    if (currentStageConfig == null)
                    {
                        Debug.LogWarning($"[Spawner] StageConfigs中未找到关卡{currentLevel}，使用默认波次配置");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Spawner] 解析StageConfigs失败: {e.Message}，使用默认波次配置");
            }
        }
    }

    void Start()
    {
        // Auto-start disabled - GameManager will call StartWave
        // StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// Start a specific wave (called by GameManager)
    /// </summary>
    public void StartWave(int wave)
    {
        currentWave = wave;
        StopAllCoroutines();
        StartCoroutine(SpawnWave());
    }
    
    IEnumerator SpawnWave()
    {
        isSpawning = true;
        monstersKilledThisWave = 0;
        
        Debug.Log($"[Spawner] Starting Wave {currentWave}");
        
        int toSpawn = monstersPerWave + (currentWave - 1);  // 每波增加怪物数量
        
        for (int i = 0; i < toSpawn && activeMonsters.Count < maxMonsters; i++)
        {
            SpawnMonster();
            yield return new WaitForSeconds(spawnInterval);
        }
        
        isSpawning = false;
        
        // 等待当前波次怪物全部死亡
        while (activeMonsters.Count > 0)
        {
            // 清理已死亡的怪物引用
            activeMonsters.RemoveAll(m => m == null);
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"[Spawner] 波次 {currentWave} 全部击杀完成!");

        // 检查是否最后一波
        if (currentWave >= wavesPerLevel)
        {
            // 关卡完成 - 通过GameManager处理
            Debug.Log("[Spawner] 所有波次完成!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveCleared();
            }
        }
        else
        {
            // 进入下一波
            currentWave++;
            yield return new WaitForSeconds(2f);
            StartCoroutine(SpawnWave());
        }
    }
    
    void SpawnMonster()
    {
        // 根据波次决定生成什么怪物
        string monsterId = GetMonsterIdByWave(currentWave);

        // 尝试加载模型
        GameObject prefab = GetCachedPrefab(monsterId);
        GameObject monster;

        if (prefab != null)
        {
            monster = Instantiate(prefab, GetRandomSpawnPosition(), Quaternion.identity);
        }
        else
        {
            // 降级：创建占位胶囊体
            Debug.LogWarning($"[Spawner] Model not found for {monsterId}, using fallback");
            monster = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            monster.GetComponent<Renderer>().material.color = Color.red;
        }

        // 恢复模型原始大小
        monster.transform.localScale = Vector3.one * 1f;

        monster.name = $"Monster_{monsterId}_{activeMonsters.Count}";
        monster.tag = "Enemy";
        monster.layer = 9; // 知识库§5: Layer 9 = Enemy

        // 添加或获取MonsterController
        var mc = monster.GetComponent<MonsterController>();
        if (mc == null)
        {
            mc = monster.AddComponent<MonsterController>();
        }

        // 配置怪物属性
        SetupMonsterById(mc, monsterId);

        // 设置Animator Controller
        SetupMonsterAnimator(monster, monsterId);

        // 添加血条显示
        {
            float waveMultiplier = 1f + (currentWave - 1) * 0.15f;
            float maxHP = 0f;

            // 优先使用配置数据
            if (configMonsterStats != null && configMonsterStats.TryGetValue(monsterId, out MonsterConfigEntry cfgEntry))
            {
                maxHP = cfgEntry.baseHp * waveMultiplier;
            }
            else if (MonsterStats.TryGetValue(monsterId, out MonsterData statData))
            {
                maxHP = statData.hp * waveMultiplier;
            }

            if (maxHP > 0)
            {
                EnemyHealthBar.CreateForEnemy(monster, maxHP);
                Debug.Log($"[Spawner] 为 {monster.name} 添加血条, HP: {maxHP}");
            }
        }

        activeMonsters.Add(monster);
    }

    string GetMonsterIdByWave(int wave)
    {
        // 优先从关卡配置读取波次敌人
        if (currentStageConfig?.waves != null)
        {
            int waveIndex = wave - 1; // waves 从1开始，数组从0开始
            if (waveIndex >= 0 && waveIndex < currentStageConfig.waves.Length)
            {
                var waveData = currentStageConfig.waves[waveIndex];
                if (waveData.enemyIds != null && waveData.enemyIds.Length > 0)
                {
                    // 从该波次的敌人列表中随机选一个
                    return waveData.enemyIds[Random.Range(0, waveData.enemyIds.Length)];
                }
            }
        }

        // Fallback: 硬编码的波次逻辑
        // 普通波次：史莱姆、哥布林、狼轮换
        // 每5波出一次精英
        // BOSS波单独处理
        if (wave >= wavesPerLevel)
        {
            return "BOSS_GOBLIN_KING";
        }

        if (wave % 5 == 0)
        {
            return "MON_GOBLIN_ELITE_001";
        }

        switch (wave % 3)
        {
            case 1: return "MON_SLIME_001";
            case 2: return "MON_GOBLIN_001";
            default: return "MON_WOLF_001";
        }
    }

    GameObject GetCachedPrefab(string monsterId)
    {
        if (cachedPrefabs.ContainsKey(monsterId) && cachedPrefabs[monsterId] != null)
        {
            return cachedPrefabs[monsterId];
        }

        if (MonsterPaths.TryGetValue(monsterId, out string[] paths))
        {
            // 尝试每个路径，优先Prefab
            foreach (string path in paths)
            {
                var prefab = Resources.Load<GameObject>(path);
                if (prefab != null)
                {
                    cachedPrefabs[monsterId] = prefab;
                    Debug.Log($"[Spawner] Loaded and cached: {path}");
                    return prefab;
                }
            }
        }

        return null;
    }

    void SetupMonsterById(MonsterController mc, string monsterId)
    {
        float waveMultiplier = 1f + (currentWave - 1) * 0.15f;

        // 优先使用配置文件数据
        if (configMonsterStats != null && configMonsterStats.TryGetValue(monsterId, out MonsterConfigEntry configData))
        {
            mc.SetupMonster(
                monsterId,
                configData.name,
                configData.baseHp * waveMultiplier,
                configData.baseAttack * waveMultiplier,
                configData.baseDefense,
                configData.moveSpeed,
                configData.expDrop,
                configData.goldDrop
            );
            return;
        }

        // Fallback: 硬编码怪物属性表
        if (MonsterStats.TryGetValue(monsterId, out MonsterData data))
        {
            mc.SetupMonster(
                monsterId,
                data.name,
                data.hp * waveMultiplier,
                data.atk * waveMultiplier,
                data.def,
                data.speed,
                data.exp,
                data.gold
            );
        }
        else
        {
            Debug.LogWarning($"[Spawner] No stats found for {monsterId} in config or hardcoded data");
        }
    }

    /// <summary>
    /// 为怪物设置Animator Controller
    /// </summary>
    void SetupMonsterAnimator(GameObject monster, string monsterId)
    {
        // 获取怪物名称用于查找Animator Controller
        string monsterTypeName = GetMonsterTypeName(monsterId);
        string animatorPath = $"Animations/Monsters/{monsterTypeName}_Animator";

        // 加载Animator Controller
        RuntimeAnimatorController animController = Resources.Load<RuntimeAnimatorController>(animatorPath);

        if (animController == null)
        {
            Debug.LogWarning($"[Spawner] 未找到Animator Controller: {animatorPath}");
            return;
        }

        // 获取或添加Animator组件
        Animator animator = monster.GetComponent<Animator>();
        if (animator == null)
        {
            animator = monster.AddComponent<Animator>();
        }

        // 设置Animator Controller
        animator.runtimeAnimatorController = animController;
        Debug.Log($"[Spawner] 为 {monster.name} 设置Animator: {animatorPath}");
    }

    /// <summary>
    /// 从怪物ID获取类型名称
    /// </summary>
    string GetMonsterTypeName(string monsterId)
    {
        switch (monsterId)
        {
            case "MON_SLIME_001": return "Slime";
            case "MON_GOBLIN_001": return "Goblin";
            case "MON_WOLF_001": return "Wolf";
            case "MON_GOBLIN_ELITE_001": return "GoblinElite";
            case "BOSS_GOBLIN_KING": return "GoblinKing";
            default: return "Slime";
        }
    }

    [Header("Spawn Area Settings")]
    public float spawnRadius = 15f;        // 生成区域半径
    public float minSpawnDistance = 5f;    // 距离玩家最小距离
    public float maxSpawnDistance = 12f;   // 距离玩家最大距离（减小以避免刷到边界外）

    [Header("Terrain Bounds - 地形边界约束")]
    public float terrainMinX = -23f;       // 扩大到MAP实际大小（留2单位边距）
    public float terrainMaxX = 23f;        // 地形X轴最大值
    public float terrainMinZ = -23f;       // 地形Z轴最小值
    public float terrainMaxZ = 23f;        // 地形Z轴最大值

    /// <summary>
    /// 设置地形边界（由GameSceneSetup根据MAP尺寸调用）
    /// </summary>
    public void SetTerrainBounds(float minX, float maxX, float minZ, float maxZ)
    {
        terrainMinX = minX;
        terrainMaxX = maxX;
        terrainMinZ = minZ;
        terrainMaxZ = maxZ;
        Debug.Log($"[MonsterSpawner] 地形边界已更新: X({minX:F1} to {maxX:F1}), Z({minZ:F1} to {maxZ:F1})");
    }

    Vector3 GetRandomSpawnPosition()
    {
        // 优先围绕玩家生成
        Transform player = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        Vector3 basePosition = Vector3.zero;
        int maxAttempts = 10;  // 最大尝试次数

        // 如果有玩家，围绕玩家生成
        if (player != null)
        {
            float playerX = player.position.x;
            float playerZ = player.position.z;

            // 计算玩家到边界的可用距离
            float maxDistToMinX = Mathf.Abs(playerX - terrainMinX);
            float maxDistToMaxX = Mathf.Abs(terrainMaxX - playerX);
            float maxDistToMinZ = Mathf.Abs(playerZ - terrainMinZ);
            float maxDistToMaxZ = Mathf.Abs(terrainMaxZ - playerZ);

            // 有效最大距离（取最小值确保不超出边界）
            float effectiveMaxDist = Mathf.Min(maxSpawnDistance,
                Mathf.Max(Mathf.Min(maxDistToMinX, maxDistToMaxX),
                          Mathf.Min(maxDistToMinZ, maxDistToMaxZ)));
            effectiveMaxDist = Mathf.Max(effectiveMaxDist, minSpawnDistance + 1f);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 随机角度
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                // 随机距离（使用有效最大距离）
                float distance = Random.Range(minSpawnDistance, effectiveMaxDist);

                float x = playerX + Mathf.Cos(angle) * distance;
                float z = playerZ + Mathf.Sin(angle) * distance;

                // 检查是否在有效范围内
                if (x >= terrainMinX && x <= terrainMaxX &&
                    z >= terrainMinZ && z <= terrainMaxZ)
                {
                    basePosition = new Vector3(x, 0.5f, z);
                    break;
                }

                // 最后一次尝试：强制Clamp
                if (attempt == maxAttempts - 1)
                {
                    x = Mathf.Clamp(x, terrainMinX, terrainMaxX);
                    z = Mathf.Clamp(z, terrainMinZ, terrainMaxZ);
                    basePosition = new Vector3(x, 0.5f, z);
                }
            }
        }
        // 如果有预设生成点，在生成点周围随机
        else if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            basePosition = spawnPoint.position;
            basePosition.x = Mathf.Clamp(basePosition.x, terrainMinX, terrainMaxX);
            basePosition.z = Mathf.Clamp(basePosition.z, terrainMinZ, terrainMaxZ);
        }
        // 否则使用纯随机位置
        else
        {
            basePosition = new Vector3(
                Random.Range(terrainMinX, terrainMaxX),
                0f,
                Random.Range(terrainMinZ, terrainMaxZ)
            );
        }

        // 添加小范围随机偏移，避免怪物完全重叠
        Vector2 randomOffset = Random.insideUnitCircle * 1.5f;  // 减小偏移量
        basePosition.x += randomOffset.x;
        basePosition.z += randomOffset.y;

        // ★ 最终钳制位置到地形范围内 ★
        basePosition.x = Mathf.Clamp(basePosition.x, terrainMinX, terrainMaxX);
        basePosition.z = Mathf.Clamp(basePosition.z, terrainMinZ, terrainMaxZ);

        // 使用Raycast获取实际地面高度
        float groundY = GetGroundHeight(basePosition.x, basePosition.z);
        basePosition.y = groundY;  // 直接放在地面上

        return basePosition;
    }

    /// <summary>
    /// 获取指定XZ位置的地面高度
    /// </summary>
    float GetGroundHeight(float x, float z)
    {
        // 从高处向下发射射线
        Vector3 rayStart = new Vector3(x, 200f, z);
        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 400f))
        {
            return hit.point.y;
        }

        // 尝试使用Terrain.activeTerrain
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            return terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
        }

        // 默认返回0
        return 0f;
    }

    /// <summary>
    /// 当怪物被杀死时调用
    /// </summary>
    public void OnMonsterKilled()
    {
        monstersKilledThisWave++;

        // 清理空引用
        activeMonsters.RemoveAll(m => m == null);

        Debug.Log($"[Spawner] 怪物被击杀! 本波已击杀: {monstersKilledThisWave}, 剩余: {activeMonsters.Count}");

        // 注意: 波次完成的逻辑在SpawnWave协程中处理
        // OnMonsterKilled只负责更新计数和清理引用
    }

    /// <summary>
    /// 获取当前活着的怪物数量
    /// </summary>
    public int GetAliveMonsterCount()
    {
        activeMonsters.RemoveAll(m => m == null);
        return activeMonsters.Count;
    }
}
