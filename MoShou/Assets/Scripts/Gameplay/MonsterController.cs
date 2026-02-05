using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Stats")]
    public string monsterId = "MON_SLIME_001";
    public string monsterName = "史莱姆";
    public float maxHealth = 30f;
    public float currentHealth;
    public float attackDamage = 5f;
    public float defense = 2f;
    public float moveSpeed = 2f;
    public int expReward = 10;
    public int goldReward = 5;
    
    [Header("AI")]
    public float detectRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    
    private Transform player;
    private float lastAttackTime;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        if (isDead) return;
        if (player == null) 
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // AI行为
        if (distanceToPlayer <= attackRange)
        {
            // 攻击玩家
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectRange)
        {
            // 追踪玩家
            ChasePlayer();
        }
    }
    
    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        
        // 移动
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // 面向玩家
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        
        Debug.Log($"[{monsterName}] Attacking player!");
        
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            CombatSystem.DealDamage(this.gameObject, player.gameObject, attackDamage);
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // 应用防御减伤
        float actualDamage = CombatSystem.CalculateDamage(damage, defense);
        currentHealth -= actualDamage;
        
        Debug.Log($"[{monsterName}] Took {actualDamage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log($"[{monsterName}] Died! Dropping {expReward} EXP, {goldReward} Gold");
        
        // 给玩家奖励
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExp(expReward);
            GameManager.Instance.AddGold(goldReward);
        }
        
        // 播放死亡效果后销毁
        Destroy(gameObject, 0.5f);
    }
    
    // 配置怪物属性(从配置加载)
    public void SetupMonster(string id, string name, float hp, float atk, float def, float speed, int exp, int gold)
    {
        monsterId = id;
        monsterName = name;
        maxHealth = hp;
        currentHealth = hp;
        attackDamage = atk;
        defense = def;
        moveSpeed = speed;
        expReward = exp;
        goldReward = gold;
    }
}
