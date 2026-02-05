using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Combat")]
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float autoAttackInterval = 1.5f;
    
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Skills")]
    public float skill1Cooldown = 8f;  // 多重箭
    public float skill2Cooldown = 10f; // 穿透箭
    
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
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;
        
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
    
    // 技能1: 多重箭 (锥形范围)
    public void UseSkill1()
    {
        if (Time.time - lastSkill1Time < skill1Cooldown) return;
        lastSkill1Time = Time.time;
        
        Debug.Log("[Player] Skill1: 多重箭!");
        
        // 锥形范围攻击
        float coneAngle = 60f;
        float coneRange = 10f;
        int arrowCount = 5;
        float damageMultiplier = 0.8f;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Vector3 toEnemy = enemy.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toEnemy);
            float distance = toEnemy.magnitude;
            
            if (angle <= coneAngle / 2 && distance <= coneRange)
            {
                CombatSystem.DealDamage(this.gameObject, enemy, attackDamage * damageMultiplier);
            }
        }
    }
    
    // 技能2: 穿透箭 (直线范围)
    public void UseSkill2()
    {
        if (Time.time - lastSkill2Time < skill2Cooldown) return;
        lastSkill2Time = Time.time;
        
        Debug.Log("[Player] Skill2: 穿透箭!");
        
        // 直线范围攻击
        float lineRange = 15f;
        float lineWidth = 1f;
        float damageMultiplier = 2.0f;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Vector3 toEnemy = enemy.transform.position - transform.position;
            float distance = toEnemy.magnitude;
            
            // 检查是否在直线范围内
            Vector3 projected = Vector3.Project(toEnemy, transform.forward);
            float perpDistance = (toEnemy - projected).magnitude;
            
            if (distance <= lineRange && perpDistance <= lineWidth && Vector3.Dot(toEnemy, transform.forward) > 0)
            {
                CombatSystem.DealDamage(this.gameObject, enemy, attackDamage * damageMultiplier);
            }
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
