using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject monsterPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;
    public int maxMonsters = 5;
    
    [Header("Wave Settings")]
    public int currentWave = 1;
    public int monstersPerWave = 3;
    public int wavesPerLevel = 3;
    
    private List<GameObject> activeMonsters = new List<GameObject>();
    private int monstersKilledThisWave = 0;
    private bool isSpawning = false;
    
    void Start()
    {
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
        if (monsterPrefab == null)
        {
            // 如果没有预制体,创建一个简单的怪物
            var monster = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            monster.name = "Monster_" + activeMonsters.Count;
            monster.tag = "Enemy";
            monster.transform.position = GetRandomSpawnPosition();
            monster.GetComponent<Renderer>().material.color = Color.red;
            
            var controller = monster.AddComponent<MonsterController>();
            SetupMonsterByWave(controller);
            
            activeMonsters.Add(monster);
            return;
        }
        
        // 使用预制体生成
        var spawnedMonster = Instantiate(monsterPrefab, GetRandomSpawnPosition(), Quaternion.identity);
        spawnedMonster.tag = "Enemy";
        
        var mc = spawnedMonster.GetComponent<MonsterController>();
        if (mc != null) SetupMonsterByWave(mc);
        
        activeMonsters.Add(spawnedMonster);
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
    
    void SetupMonsterByWave(MonsterController mc)
    {
        // 根据波次设置怪物属性
        float waveMultiplier = 1f + (currentWave - 1) * 0.2f;
        
        switch (currentWave % 3)
        {
            case 1:
                mc.SetupMonster("MON_SLIME_001", "史莱姆", 
                    30 * waveMultiplier, 5 * waveMultiplier, 2, 2f, 10, 5);
                break;
            case 2:
                mc.SetupMonster("MON_GOBLIN_001", "哥布林", 
                    50 * waveMultiplier, 8 * waveMultiplier, 4, 1.5f, 15, 8);
                break;
            case 0:
                mc.SetupMonster("MON_WOLF_001", "灰狼", 
                    70 * waveMultiplier, 12 * waveMultiplier, 5, 3f, 20, 12);
                break;
        }
    }
}
