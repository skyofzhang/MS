using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoShou.Utils;

/// <summary>
/// 怪物生成器 - 符合AI开发知识库§2 RULE-RES-002, §4怪物属性表
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;
    public int maxMonsters = 8;

    [Header("Wave Settings")]
    public int currentWave = 1;
    public int monstersPerWave = 3;
    public int wavesPerLevel = 3;

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

    // 怪物属性表 (知识库§4)
    private static readonly Dictionary<string, MonsterData> MonsterStats = new Dictionary<string, MonsterData>
    {
        { "MON_SLIME_001", new MonsterData("史莱姆", 30, 5, 2, 2.0f, 10, 5) },
        { "MON_GOBLIN_001", new MonsterData("哥布林", 50, 8, 4, 1.5f, 15, 8) },
        { "MON_WOLF_001", new MonsterData("灰狼", 70, 12, 5, 3.0f, 20, 12) },
        { "MON_GOBLIN_ELITE_001", new MonsterData("精英哥布林", 150, 18, 10, 2.0f, 50, 30) },
        { "BOSS_GOBLIN_KING", new MonsterData("哥布林王", 800, 25, 15, 1.8f, 300, 200) }
    };

    private List<GameObject> activeMonsters = new List<GameObject>();
    private int monstersKilledThisWave = 0;
    private bool isSpawning = false;
    private Dictionary<string, GameObject> cachedPrefabs = new Dictionary<string, GameObject>();

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
        
        // 进入下一波
        currentWave++;
        
        if (currentWave > wavesPerLevel)
        {
            // 关卡完成
            Debug.Log("[Spawner] Level Complete!");
            if (GameManager.Instance != null)
                GameManager.Instance.ChangeState(GameManager.GameState.Victory);
        }
        else
        {
            // 下一波
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

        activeMonsters.Add(monster);
    }

    string GetMonsterIdByWave(int wave)
    {
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
        if (MonsterStats.TryGetValue(monsterId, out MonsterData data))
        {
            // 应用波次加成
            float waveMultiplier = 1f + (currentWave - 1) * 0.15f;

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
            Debug.LogWarning($"[Spawner] No stats found for {monsterId}");
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }
        
        // 随机位置
        return new Vector3(
            Random.Range(-10f, 10f),
            1f,
            Random.Range(-10f, 10f)
        );
    }
    
}
