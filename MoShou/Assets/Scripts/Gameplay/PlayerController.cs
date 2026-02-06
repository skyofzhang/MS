using UnityEngine;

/// <summary>
/// 玩家控制器 - 符合AI开发知识库§4属性规范
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement - 知识库§4")]
    public float moveSpeed = 4.0f;      // MoveSpeed: 4.0
    public float rotationSpeed = 10f;

    [Header("Combat - 知识库§4")]
    public float attackRange = 8.0f;     // AttackRange: 8.0
    public float attackDamage = 15f;     // Attack: 15
    public float attackCooldown = 1.0f;  // AttackSpeed: 1.0
    public float autoAttackInterval = 1.0f;
    public float critChance = 0.10f;     // CritChance: 0.10
    public float critDamage = 0.50f;     // CritDamage: 0.50

    [Header("Stats - 知识库§4")]
    public float maxHealth = 150f;       // HP: 150
    public float defense = 10f;          // Defense: 10
    public float currentHealth;

    [Header("Skills - 知识库§4技能数据表")]
    // SK001 多重箭: CD=8s, 倍率=0.8, 箭数=5, 范围=10m, 角度=60°
    public float skill1Cooldown = 8f;
    public float skill1Multiplier = 0.8f;
    public int skill1ArrowCount = 5;
    public float skill1Range = 10f;
    public float skill1Angle = 60f;

    // SK002 穿透箭: CD=10s, 倍率=2.0, 穿透=true, 范围=15m
    public float skill2Cooldown = 10f;
    public float skill2Multiplier = 2.0f;
    public float skill2Range = 15f;
    public float skill2Width = 1f;
    
    private CharacterController controller;
    private Vector3 moveDirection;
    private float lastAttackTime;
    private float lastSkill1Time;
    private float lastSkill2Time;
    private Transform currentTarget;
    
    // 虚拟摇杆输入
    public Vector2 JoystickInput { get; set; }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();
        
        currentHealth = maxHealth;
    }
    
    void Update()
    {
        // 检查游戏状态（允许在没有GameManager时也能移动，用于测试）
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                // 调试信息：显示为什么不能移动
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    Debug.Log($"[PlayerController] 无法移动，当前状态: {GameManager.Instance.CurrentState}");
                }
                return;
            }
        }

        HandleMovement();
        HandleAutoAttack();
    }
    
    void HandleMovement()
    {
        // 从虚拟摇杆获取输入
        float horizontal = JoystickInput.x;
        float vertical = JoystickInput.y;
        
        // 也支持键盘输入(调试用)
        if (JoystickInput == Vector2.zero)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        
        if (moveDirection.magnitude > 0.1f)
        {
            // 移动
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            
            // 旋转面向移动方向
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 应用重力
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 9.8f * Time.deltaTime);
        }
    }
    
    void HandleAutoAttack()
    {
        // 寻找最近的敌人
        FindNearestEnemy();
        
        if (currentTarget != null && Time.time - lastAttackTime >= autoAttackInterval)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange)
            {
                Attack(currentTarget.gameObject);
                lastAttackTime = Time.time;
            }
        }
    }
    
    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearestDistance = float.MaxValue;
        Transform nearest = null;
        
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance && distance <= attackRange * 2)
            {
                nearestDistance = distance;
                nearest = enemy.transform;
            }
        }
        
        currentTarget = nearest;
    }
    
    void Attack(GameObject target)
    {
        Debug.Log($"[Player] Attacking {target.name}");
        
        // 面向目标
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // 造成伤害
        var monster = target.GetComponent<MonsterController>();
        if (monster != null)
        {
            CombatSystem.DealDamage(this.gameObject, target, attackDamage);
        }
    }
    
    // 技能1: 多重箭 (锥形范围) - 知识库§4 SK001
    public void UseSkill1()
    {
        if (Time.time - lastSkill1Time < skill1Cooldown) return;
        lastSkill1Time = Time.time;

        Debug.Log($"[Player] Skill1: 多重箭! 范围={skill1Range}m, 角度={skill1Angle}°, 箭数={skill1ArrowCount}");

        // 播放特效 (知识库§2 RULE-RES-011)
        PlaySkillVFX("VFX_MultiShot");

        // 锥形范围攻击
        int hitCount = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (hitCount >= skill1ArrowCount) break;

            Vector3 toEnemy = enemy.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toEnemy);
            float distance = toEnemy.magnitude;

            if (angle <= skill1Angle / 2 && distance <= skill1Range)
            {
                CombatSystem.DealDamage(this.gameObject, enemy, attackDamage * skill1Multiplier);
                hitCount++;
            }
        }

        Debug.Log($"[Player] 多重箭命中 {hitCount} 个目标");
    }

    // 技能2: 穿透箭 (直线范围) - 知识库§4 SK002
    public void UseSkill2()
    {
        if (Time.time - lastSkill2Time < skill2Cooldown) return;
        lastSkill2Time = Time.time;

        Debug.Log($"[Player] Skill2: 穿透箭! 范围={skill2Range}m, 倍率={skill2Multiplier}x");

        // 播放特效 (知识库§2 RULE-RES-011)
        PlaySkillVFX("VFX_PierceShot");

        // 直线范围攻击（穿透所有敌人）
        int hitCount = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Vector3 toEnemy = enemy.transform.position - transform.position;
            float distance = toEnemy.magnitude;

            // 检查是否在直线范围内
            Vector3 projected = Vector3.Project(toEnemy, transform.forward);
            float perpDistance = (toEnemy - projected).magnitude;

            if (distance <= skill2Range && perpDistance <= skill2Width && Vector3.Dot(toEnemy, transform.forward) > 0)
            {
                CombatSystem.DealDamage(this.gameObject, enemy, attackDamage * skill2Multiplier);
                hitCount++;
            }
        }

        Debug.Log($"[Player] 穿透箭穿透 {hitCount} 个目标");
    }

    void PlaySkillVFX(string vfxName)
    {
        var vfxPrefab = Resources.Load<GameObject>($"Prefabs/VFX/{vfxName}");
        if (vfxPrefab != null)
        {
            var vfx = Instantiate(vfxPrefab, transform.position, transform.rotation);
            Destroy(vfx, 2f);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"[Player] Took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("[Player] Died!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Defeat);
        }
    }
}
