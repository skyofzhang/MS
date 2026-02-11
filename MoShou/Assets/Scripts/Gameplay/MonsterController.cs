using UnityEngine;
using MoShou.UI;
using MoShou.Systems;
using MoShou.Effects;

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
    // groundCheckDistance - 保留用于未来地面检测功能

    // 动画系统
    private Animator animator;
    private bool hasAnimator = false;

    // 简单动画（后备方案，当没有Animator时使用）
    private Transform modelTransform;
    private Vector3 originalScale;
    private float bobTime = 0f;
    private bool isAttacking = false;
    private float attackAnimTime = 0f;

    // 血条组件
    private EnemyHealthBar healthBar;

    // 动画参数名
    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");
    private static readonly int ANIM_ATTACK = Animator.StringToHash("Attack");
    private static readonly int ANIM_SKILL = Animator.StringToHash("Skill");
    private static readonly int ANIM_HIT = Animator.StringToHash("Hit");
    private static readonly int ANIM_DEATH = Animator.StringToHash("Death");

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
            characterController.radius = 0.3f;
            characterController.center = new Vector3(0, 0.5f, 0);
            characterController.skinWidth = 0.08f;
            characterController.minMoveDistance = 0f;
        }

        // 确保怪物贴地：向下做一次Raycast校正位置
        SnapToGround();

        // 初始化动画系统
        InitializeAnimator();

        // 获取模型Transform（用于后备动画）
        modelTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;
        originalScale = modelTransform.localScale;

        Debug.Log($"[Monster] {monsterName} 初始化完成, Player: {(player != null ? player.name : "未找到")}, HasAnimator: {hasAnimator}");
    }

    /// <summary>
    /// 初始化动画系统
    /// </summary>
    void InitializeAnimator()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            hasAnimator = true;
            Debug.Log($"[Monster] {monsterName} 使用Animator动画系统");
        }
        else
        {
            hasAnimator = false;
            Debug.Log($"[Monster] {monsterName} 使用后备程序化动画");
        }
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

        // 更新攻击动画（始终更新，不受游戏状态影响）
        if (isAttacking)
        {
            UpdateAttackAnimation();
        }

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
        else
        {
            // 始终追击：只要猎人活着，怪物就朝猎人方向行走
            ChasePlayer();
        }

        // 应用重力
        ApplyGravity();
    }

    void Idle()
    {
        // 使用Animator或后备动画
        if (hasAnimator && animator != null)
        {
            animator.SetFloat(ANIM_SPEED, 0f);
        }
        else
        {
            // 后备：待机动画 - 轻微呼吸效果
            UpdateIdleAnimation();
        }
    }

    private float verticalVelocity = 0f;

    void ApplyGravity()
    {
        if (characterController == null) return;

        if (characterController.isGrounded)
        {
            // 贴地时保持一个小的向下速度，确保isGrounded持续检测
            verticalVelocity = -2f;
        }
        else
        {
            // 累积重力加速度，让怪物快速落地
            verticalVelocity -= 20f * Time.deltaTime;
        }

        characterController.Move(new Vector3(0, verticalVelocity * Time.deltaTime, 0));
    }

    /// <summary>
    /// 将怪物吸附到地面上
    /// </summary>
    void SnapToGround()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 50f))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            Debug.Log($"[Monster] {monsterName} SnapToGround: Y={hit.point.y}");
        }
    }

    void UpdateIdleAnimation()
    {
        if (modelTransform == null || isAttacking) return;

        bobTime += Time.deltaTime * 2f;
        float breathe = 1f + Mathf.Sin(bobTime) * 0.02f;
        modelTransform.localScale = originalScale * breathe;
    }

    void UpdateMoveAnimation()
    {
        if (modelTransform == null || isAttacking) return;

        bobTime += Time.deltaTime * 8f;
        float bob = Mathf.Sin(bobTime) * 0.05f;
        modelTransform.localPosition = new Vector3(0, Mathf.Abs(bob), 0);
    }

    void UpdateAttackAnimation()
    {
        if (modelTransform == null || !isAttacking) return;

        attackAnimTime += Time.deltaTime * 6f;
        float scale = 1f + Mathf.Sin(attackAnimTime * Mathf.PI) * 0.2f;
        modelTransform.localScale = originalScale * scale;

        if (attackAnimTime >= 1f)
        {
            isAttacking = false;
            attackAnimTime = 0f;
            modelTransform.localScale = originalScale;
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

        // 移动动画
        if (hasAnimator && animator != null)
        {
            animator.SetFloat(ANIM_SPEED, 1f);
        }
        else
        {
            UpdateMoveAnimation();
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

        // 触发攻击动画
        if (hasAnimator && animator != null)
        {
            animator.SetTrigger(ANIM_ATTACK);
        }
        else
        {
            isAttacking = true;
            attackAnimTime = 0f;
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
        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
        Material mat = new Material(shader);
        mat.color = new Color(1f, 0.2f, 0.2f, 0.7f); // 红色
        renderer.material = mat;

        vfx.AddComponent<FadeAndDestroy>();
    }
    
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, false);
    }

    /// <summary>
    /// 受到伤害（支持暴击参数）
    /// </summary>
    public void TakeDamage(float damage, bool isCritical)
    {
        if (isDead) return;

        // 应用防御减伤
        float actualDamage = CombatSystem.CalculateDamage(damage, defense);
        currentHealth -= actualDamage;

        // 更新血条
        if (healthBar == null)
        {
            healthBar = GetComponent<EnemyHealthBar>();
        }
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        // 播放受击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAtPosition(AudioManager.SFX.EnemyHit, transform.position);
        }

        // 显示伤害飘字
        DamagePopup.CreateWorldSpace(
            transform.position + Vector3.up * 1.5f,
            Mathf.RoundToInt(actualDamage),
            isCritical ? DamageType.Critical : DamageType.Normal
        );

        // 播放受击动画
        if (hasAnimator && animator != null)
        {
            animator.SetTrigger(ANIM_HIT);
        }

        Debug.Log($"[{monsterName}] Took {actualDamage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 僵直效果 (策划案3.1.1)
    /// </summary>
    public void Stun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    private System.Collections.IEnumerator StunCoroutine(float duration)
    {
        // 暂时禁用AI
        enabled = false;
        yield return new WaitForSeconds(duration);
        if (!isDead)
        {
            enabled = true;
        }
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log($"[{monsterName}] Died! Dropping {expReward} EXP, {goldReward} Gold");

        // 播放死亡动画
        if (hasAnimator && animator != null)
        {
            animator.SetTrigger(ANIM_DEATH);
        }

        // 播放死亡音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAtPosition(AudioManager.SFX.EnemyDeath, transform.position);
        }

        // 给玩家奖励
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill(goldReward, expReward);
        }

        // === 策划案3.1.1死亡反馈 ===
        if (GameFeedback.Instance != null)
        {
            GameFeedback.Instance.TriggerDeathFeedback(gameObject);
        }
        else
        {
            // 备用：创建死亡特效
            CreateDeathVFX();
        }

        // 使用VFXManager播放死亡VFX
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayVFX("VFX_Death_Dissolve", transform.position);
        }

        // 通知MonsterSpawner
        if (MonsterSpawner.Instance != null)
        {
            MonsterSpawner.Instance.OnMonsterKilled();
        }

        // === 掉落系统 ===
        SpawnDrops();

        // 播放死亡效果后销毁
        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// 生成掉落物（金币、经验、装备）
    /// </summary>
    void SpawnDrops()
    {
        Vector3 dropPos = transform.position + Vector3.up * 0.3f;

        // 使用LootManager处理掉落
        if (LootManager.Instance != null)
        {
            // 根据怪物类型选择掉落表
            string dropTableId = GetDropTableId();
            LootManager.Instance.ProcessDrop(dropPos, dropTableId);
            Debug.Log($"[{monsterName}] 触发掉落表: {dropTableId}");
        }
        else
        {
            // 备用：直接创建简单掉落物
            CreateSimpleDrops(dropPos);
        }
    }

    /// <summary>
    /// 获取掉落表ID
    /// </summary>
    string GetDropTableId()
    {
        if (monsterId.Contains("BOSS"))
        {
            return "DROP_BOSS";
        }
        else if (monsterId.Contains("ELITE"))
        {
            return "DROP_ELITE";
        }
        return "DROP_NORMAL";
    }

    /// <summary>
    /// 创建简单掉落物（备用方案，当LootManager不可用时）
    /// </summary>
    void CreateSimpleDrops(Vector3 position)
    {
        // 掉落金币
        if (goldReward > 0)
        {
            CreateDropItem(position + Random.insideUnitSphere * 0.3f, DropType.Gold, goldReward, "");
        }

        // 掉落经验
        if (expReward > 0)
        {
            CreateDropItem(position + Random.insideUnitSphere * 0.3f, DropType.Exp, expReward, "");
        }

        // 随机掉落装备（10%概率）
        if (Random.value < 0.1f)
        {
            string equipmentId = GetRandomEquipmentId();
            CreateDropItem(position + Random.insideUnitSphere * 0.3f, DropType.Equipment, 1, equipmentId);
            Debug.Log($"[{monsterName}] 备用掉落装备: {equipmentId}");
        }
    }

    /// <summary>
    /// 获取随机装备ID（用于备用掉落）
    /// </summary>
    string GetRandomEquipmentId()
    {
        // 根据怪物类型返回可能掉落的装备
        string[] commonEquipments = { "WPN_001", "ARM_001" };
        string[] eliteEquipments = { "WPN_001", "WPN_002", "ARM_001", "ARM_002", "HLM_002" };
        string[] bossEquipments = { "WPN_002", "WPN_003", "WPN_004", "ARM_002", "ARM_003" };

        string[] pool;
        if (monsterId.Contains("BOSS"))
            pool = bossEquipments;
        else if (monsterId.Contains("ELITE"))
            pool = eliteEquipments;
        else
            pool = commonEquipments;

        return pool[Random.Range(0, pool.Length)];
    }

    enum DropType { Gold, Exp, Equipment }

    void CreateDropItem(Vector3 pos, DropType type, int amount, string itemId)
    {
        GameObject drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        drop.name = $"Drop_{type}";
        drop.transform.position = pos;

        // 设置大小和颜色
        Color dropColor;
        float scale;
        switch (type)
        {
            case DropType.Gold:
                dropColor = new Color(1f, 0.84f, 0f, 1f); // 金色
                scale = 0.25f;
                break;
            case DropType.Exp:
                dropColor = new Color(0.5f, 0.8f, 1f, 1f); // 蓝色
                scale = 0.2f;
                break;
            case DropType.Equipment:
                dropColor = new Color(0.8f, 0.4f, 1f, 1f); // 紫色
                scale = 0.35f;
                break;
            default:
                dropColor = Color.white;
                scale = 0.2f;
                break;
        }

        drop.transform.localScale = Vector3.one * scale;

        // 设置材质 (URP兼容)
        var renderer = drop.GetComponent<Renderer>();
        // 优先使用URP Lit shader, 回退到Sprites/Default
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        Material mat = new Material(shader);
        mat.color = dropColor;
        // URP shader使用不同的属性名
        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", 0.8f);
        }
        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", 0.9f);
        }
        else if (mat.HasProperty("_Glossiness"))
        {
            mat.SetFloat("_Glossiness", 0.9f);
        }
        renderer.material = mat;

        // 设置碰撞为触发器
        var col = drop.GetComponent<Collider>();
        col.isTrigger = true;

        // 添加掉落物行为组件
        var dropBehavior = drop.AddComponent<SimpleDropBehavior>();
        dropBehavior.Initialize(type.ToString(), amount, itemId);
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
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            Material mat = new Material(shader);
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
