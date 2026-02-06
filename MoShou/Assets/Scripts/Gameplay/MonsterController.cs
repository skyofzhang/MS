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
    
    private CharacterController characterController;
    private float groundCheckDistance = 0.5f;

    void Start()
    {
        currentHealth = maxHealth;
        FindPlayer();

        // 添加CharacterController用于移动
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 1f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 0.5f, 0);
        }

        Debug.Log($"[Monster] {monsterName} 初始化完成, Player: {(player != null ? player.name : "未找到")}");
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // 检查游戏状态
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (player == null)
        {
            FindPlayer();
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
        else
        {
            // 待机/巡逻
            Idle();
        }

        // 应用重力
        ApplyGravity();
    }

    void Idle()
    {
        // 可以添加巡逻逻辑
    }

    void ApplyGravity()
    {
        if (characterController != null && !characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.8f * Time.deltaTime);
        }
    }
    
    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        // 使用CharacterController移动
        if (characterController != null)
        {
            Vector3 move = direction * moveSpeed * Time.deltaTime;
            characterController.Move(move);
        }
        else
        {
            // 后备：直接移动
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // 面向玩家
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }
    
    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        Debug.Log($"[{monsterName}] Attacking player!");

        // 面向玩家
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // 创建攻击特效
        CreateAttackVFX();

        // 造成伤害
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            CombatSystem.DealDamage(this.gameObject, player.gameObject, attackDamage);
        }
    }

    void CreateAttackVFX()
    {
        // 简单的攻击特效
        GameObject vfx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vfx.name = "MonsterAttackVFX";
        vfx.transform.position = transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;
        vfx.transform.localScale = Vector3.one * 0.4f;

        Destroy(vfx.GetComponent<Collider>());

        var renderer = vfx.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.2f, 0.2f, 0.7f); // 红色
        renderer.material = mat;

        vfx.AddComponent<FadeAndDestroy>();
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
            GameManager.Instance.AddKill(goldReward, expReward);
        }

        // 创建死亡特效
        CreateDeathVFX();

        // 通知MonsterSpawner
        if (MonsterSpawner.Instance != null)
        {
            MonsterSpawner.Instance.OnMonsterKilled();
        }

        // 播放死亡效果后销毁
        Destroy(gameObject, 0.5f);
    }

    void CreateDeathVFX()
    {
        // 死亡爆炸特效
        for (int i = 0; i < 5; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "DeathParticle";
            particle.transform.position = transform.position + Vector3.up * 0.5f;
            particle.transform.localScale = Vector3.one * 0.2f;

            Destroy(particle.GetComponent<Collider>());

            var renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            renderer.material = mat;

            // 添加随机飞散
            var rb = particle.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.velocity = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(2f, 5f),
                Random.Range(-3f, 3f)
            );

            Destroy(particle, 1f);
        }
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
