using UnityEngine;
using MoShou.UI;
using MoShou.Effects;

/// <summary>
/// 玩家控制器 - 符合AI开发知识库§4属性规范
/// 集成策划案3.1.1打击感反馈系统
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
    public float skill2Width = 3f;  // 穿透箭宽度（适配1倍模型大小）

    // SK003 战吼: CD=20s, 护盾=20%最大生命, 攻击提升=15%, 持续8s
    public float skill3Cooldown = 20f;
    public float skill3ShieldPercent = 0.2f;
    public float skill3AttackBuff = 0.15f;
    public float skill3Duration = 8f;

    // 技能冷却状态（供UI读取）
    public float Skill1CooldownRemaining => Mathf.Max(0, skill1Cooldown - (Time.time - lastSkill1Time));
    public float Skill2CooldownRemaining => Mathf.Max(0, skill2Cooldown - (Time.time - lastSkill2Time));
    public float Skill3CooldownRemaining => Mathf.Max(0, skill3Cooldown - (Time.time - lastSkill3Time));

    // BUFF状态
    private float shieldAmount = 0f;
    private float attackBuffEndTime = 0f;
    private float attackBuffMultiplier = 1f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private float lastAttackTime;
    private float lastSkill1Time;
    private float lastSkill2Time;
    private float lastSkill3Time;
    private Transform currentTarget;

    // 动画参数名（直接使用字符串，因为FBX内嵌动画）
    private const string ANIM_IDLE = "Idle";
    private const string ANIM_RUN = "Run";
    private const string ANIM_ATTACK01 = "Attack01";
    private const string ANIM_ATTACK02 = "Attack02";
    private const string ANIM_SKILL01 = "Skill01";
    private const string ANIM_SKILL02 = "Skill02";
    private const string ANIM_SKILL03 = "Skill03";
    private const string ANIM_HIT = "Hit";
    private const string ANIM_DEATH = "Death";
    private const string ANIM_VICTORY = "Victory";

    // 当前动画状态
    private string currentAnim = "";
    private bool isPlayingOneShot = false;
    private float oneShotEndTime = 0f;

    // 简单动画效果
    private Transform modelTransform;
    private Vector3 originalScale;
    private float bobTime = 0f;
    private bool isAttacking = false;
    private float attackAnimTime = 0f;

    // 虚拟摇杆输入
    public Vector2 JoystickInput { get; set; }

    // 性能优化：敌人搜索缓存
    private float lastEnemySearchTime;
    private float enemySearchInterval = 0.3f;  // 每0.3秒搜索一次敌人

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0, 0.9f, 0);
        }

        // 获取动画控制器
        animator = GetComponentInChildren<Animator>();

        // 获取模型Transform用于简单动画
        modelTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;
        originalScale = modelTransform.localScale;

        currentHealth = maxHealth;

        // 初始化动画
        if (animator != null)
        {
            // 确保Animator有RuntimeAnimatorController
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("[PlayerController] Animator没有AnimatorController，尝试创建运行时动画控制");
            }
            PlayAnimation(ANIM_IDLE);
        }

        Debug.Log($"[PlayerController] 初始化完成, Animator: {(animator != null ? "有" : "无")}");
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

        // 更新动画（使用FBX内嵌动画）
        UpdateMovementAnimation(moveDirection.magnitude > 0.1f);

        // 简单的移动动画（上下摆动）- 备用
        UpdateSimpleAnimation(moveDirection.magnitude > 0.1f);

        // 应用重力
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 9.8f * Time.deltaTime);
        }
    }

    /// <summary>
    /// 更新移动动画
    /// </summary>
    void UpdateMovementAnimation(bool isMoving)
    {
        // 检查是否有动画系统可用
        bool hasAnimator = animator != null && animator.runtimeAnimatorController != null;
        bool hasLegacy = GetComponentInChildren<Animation>() != null;

        if (!hasAnimator && !hasLegacy) return;

        // 如果正在播放一次性动画，检查是否结束
        if (isPlayingOneShot)
        {
            if (Time.time >= oneShotEndTime)
            {
                isPlayingOneShot = false;
            }
            else
            {
                return; // 等待一次性动画播放完毕
            }
        }

        // 使用Speed参数驱动动画切换（Animator Controller使用Speed参数作为条件）
        if (hasAnimator)
        {
            float speed = isMoving ? 1f : 0f;
            animator.SetFloat("Speed", speed);
            currentAnim = isMoving ? ANIM_RUN : ANIM_IDLE;
            return;
        }

        // Legacy模式：直接切换动画状态
        string targetAnim = isMoving ? ANIM_RUN : ANIM_IDLE;
        if (currentAnim != targetAnim)
        {
            PlayAnimation(targetAnim, true);
        }
    }

    /// <summary>
    /// 播放动画（支持Mecanim和Legacy两种模式）
    /// </summary>
    void PlayAnimation(string animName, bool loop = false, float duration = 0f)
    {
        // 尝试Mecanim Animator
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            try
            {
                animator.CrossFade(animName, 0.1f);
                currentAnim = animName;

                if (!loop && duration > 0)
                {
                    isPlayingOneShot = true;
                    oneShotEndTime = Time.time + duration;
                }
                else if (!loop)
                {
                    isPlayingOneShot = true;
                    oneShotEndTime = Time.time + 0.5f;
                }

                Debug.Log($"[PlayerController] 播放Mecanim动画: {animName}");
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PlayerController] Mecanim播放失败: {e.Message}");
            }
        }

        // 尝试Legacy动画
        var legacyAnim = GetComponentInChildren<Animation>();
        if (legacyAnim != null && legacyAnim.GetClip(animName) != null)
        {
            legacyAnim[animName].wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            legacyAnim.CrossFade(animName, 0.1f);
            currentAnim = animName;

            if (!loop)
            {
                isPlayingOneShot = true;
                oneShotEndTime = Time.time + (duration > 0 ? duration : legacyAnim[animName].length);
            }

            Debug.Log($"[PlayerController] 播放Legacy动画: {animName}");
        }
    }

    /// <summary>
    /// 播放攻击动画
    /// </summary>
    void PlayAttackAnimation()
    {
        // 随机选择Attack01或Attack02
        string attackAnim = Random.value > 0.5f ? ANIM_ATTACK01 : ANIM_ATTACK02;
        PlayAnimation(attackAnim, false, 0.5f);
    }

    /// <summary>
    /// 播放技能动画
    /// </summary>
    void PlaySkillAnimation(int skillIndex)
    {
        string skillAnim = skillIndex switch
        {
            1 => ANIM_SKILL01,
            2 => ANIM_SKILL02,
            3 => ANIM_SKILL03,
            _ => ANIM_ATTACK01
        };
        PlayAnimation(skillAnim, false, 0.8f);
    }

    /// <summary>
    /// 播放受击动画
    /// </summary>
    void PlayHitAnimation()
    {
        PlayAnimation(ANIM_HIT, false, 0.3f);
    }

    /// <summary>
    /// 播放死亡动画
    /// </summary>
    void PlayDeathAnimation()
    {
        PlayAnimation(ANIM_DEATH, false, 2f);
    }

    /// <summary>
    /// 播放胜利动画
    /// </summary>
    public void PlayVictoryAnimation()
    {
        PlayAnimation(ANIM_VICTORY, false, 2f);
    }

    /// <summary>
    /// 简单的视觉动画效果
    /// </summary>
    void UpdateSimpleAnimation(bool isMoving)
    {
        if (modelTransform == null) return;

        // 攻击动画
        if (isAttacking)
        {
            attackAnimTime += Time.deltaTime * 8f;
            float attackScale = 1f + Mathf.Sin(attackAnimTime * Mathf.PI) * 0.15f;
            modelTransform.localScale = originalScale * attackScale;

            if (attackAnimTime >= 1f)
            {
                isAttacking = false;
                attackAnimTime = 0f;
                modelTransform.localScale = originalScale;
            }
            return;
        }

        // 移动动画（轻微上下摆动）
        if (isMoving)
        {
            bobTime += Time.deltaTime * 10f;
            float bobAmount = Mathf.Sin(bobTime) * 0.03f;
            modelTransform.localPosition = new Vector3(0, bobAmount, 0);
        }
        else
        {
            bobTime = 0f;
            modelTransform.localPosition = Vector3.zero;
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
        // 性能优化：如果当前目标仍然有效且在搜索间隔内，跳过搜索
        if (Time.time - lastEnemySearchTime < enemySearchInterval && currentTarget != null)
        {
            // 检查当前目标是否仍然存活
            if (currentTarget.gameObject.activeInHierarchy)
            {
                return;
            }
        }

        lastEnemySearchTime = Time.time;

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

        // 播放攻击动画
        PlayAttackAnimation();

        // 触发简单攻击动画（备用）
        isAttacking = true;
        attackAnimTime = 0f;

        // 面向目标
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 判定暴击
        bool isCritical = Random.value < critChance;
        float finalDamage = GetCurrentAttackDamage();
        if (isCritical)
        {
            finalDamage *= (1f + critDamage);
        }

        // 造成伤害（应用BUFF）
        var monster = target.GetComponent<MonsterController>();
        if (monster != null)
        {
            CombatSystem.DealDamage(this.gameObject, target, finalDamage, isCritical);

            // === 策划案3.1.1打击感反馈 ===
            Vector3 hitPosition = target.transform.position + Vector3.up;

            // 1. 触发命中反馈 (屏幕震动+顿帧+VFX)
            if (GameFeedback.Instance != null)
            {
                GameFeedback.Instance.TriggerHitFeedback(hitPosition, isCritical);
                GameFeedback.Instance.TriggerEnemyHitFeedback(target, direction,
                    Mathf.RoundToInt(finalDamage), isCritical);
            }

            // 2. 始终创建弓箭弹道特效（带粒子拖尾和随机弧度）
            CreateAttackVFX(target.transform.position);
        }
    }
    
    // 技能1: 多重箭 (锥形范围) - 知识库§4 SK001
    public void UseSkill1()
    {
        if (Time.time - lastSkill1Time < skill1Cooldown) return;
        lastSkill1Time = Time.time;

        Debug.Log($"[Player] Skill1: 多重箭! 范围={skill1Range}m, 角度={skill1Angle}°, 箭数={skill1ArrowCount}");

        // 播放技能动画
        PlaySkillAnimation(1);

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
                CombatSystem.DealDamage(this.gameObject, enemy, GetCurrentAttackDamage() * skill1Multiplier);
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

        // 播放技能动画
        PlaySkillAnimation(2);

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
                CombatSystem.DealDamage(this.gameObject, enemy, GetCurrentAttackDamage() * skill2Multiplier);
                hitCount++;
            }
        }

        Debug.Log($"[Player] 穿透箭穿透 {hitCount} 个目标");
    }

    // 技能3: 战吼 (护盾+攻击增益) - 知识库§4 SK003
    public void UseSkill3()
    {
        if (Time.time - lastSkill3Time < skill3Cooldown) return;
        lastSkill3Time = Time.time;

        Debug.Log($"[Player] Skill3: 战吼! 护盾={skill3ShieldPercent * 100}%, 攻击增益={skill3AttackBuff * 100}%");

        // 播放技能动画
        PlaySkillAnimation(3);

        // 播放特效
        PlaySkillVFX("VFX_BattleShout");

        // 应用护盾
        shieldAmount = maxHealth * skill3ShieldPercent;

        // 应用攻击增益
        attackBuffMultiplier = 1f + skill3AttackBuff;
        attackBuffEndTime = Time.time + skill3Duration;

        // 显示增益飘字
        DamagePopup.CreateWorldSpace(
            transform.position + Vector3.up * 2.5f,
            Mathf.RoundToInt(shieldAmount),
            DamageType.Heal
        );
    }

    /// <summary>
    /// 获取当前实际攻击力（含BUFF）
    /// </summary>
    public float GetCurrentAttackDamage()
    {
        // 检查BUFF是否过期
        if (Time.time > attackBuffEndTime)
        {
            attackBuffMultiplier = 1f;
        }
        return attackDamage * attackBuffMultiplier;
    }

    void PlaySkillVFX(string vfxName)
    {
        // 多重箭和穿透箭始终使用代码弹道特效（飞行箭矢+拖尾），效果更好
        if (vfxName.Contains("MultiShot") || vfxName.Contains("PierceShot"))
        {
            CreateFallbackSkillVFX(vfxName);
            return;
        }

        var vfxPrefab = Resources.Load<GameObject>($"Prefabs/VFX/{vfxName}");
        if (vfxPrefab != null)
        {
            var vfx = Instantiate(vfxPrefab, transform.position, transform.rotation);
            Destroy(vfx, 2f);
        }
        else
        {
            CreateFallbackSkillVFX(vfxName);
        }
    }

    /// <summary>
    /// 创建备用技能特效
    /// </summary>
    void CreateFallbackSkillVFX(string skillName)
    {
        if (skillName.Contains("BattleShout"))
        {
            // 战吼 - 环形扩散效果
            CreateShoutVFX();
            return;
        }

        Vector3 firePos = transform.position + Vector3.up * 0.8f;

        if (skillName.Contains("Multi"))
        {
            // 多重箭 - 扇形发射5支箭（橙色）
            Color effectColor = new Color(1f, 0.5f, 0f, 0.9f);
            for (int i = 0; i < 5; i++)
            {
                float angle = -30f + i * 15f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
                CreateSkillProjectile(firePos, dir, effectColor, skill1Range, 0.12f,
                    MoShou.Combat.ProjectileTrail.TrailPreset.MultiShot);
            }
        }
        else
        {
            // 穿透箭 - 蓝色光束直线穿透
            Color effectColor = new Color(0.3f, 0.8f, 1f, 0.9f);
            // 主弹道
            CreateSkillProjectile(firePos, transform.forward, effectColor, skill2Range, 0.2f,
                MoShou.Combat.ProjectileTrail.TrailPreset.Pierce);
            // 两侧辅助弹道
            Vector3 right = Quaternion.Euler(0, 5f, 0) * transform.forward;
            Vector3 left = Quaternion.Euler(0, -5f, 0) * transform.forward;
            CreateSkillProjectile(firePos, right, new Color(0.5f, 0.9f, 1f, 0.7f), skill2Range, 0.1f,
                MoShou.Combat.ProjectileTrail.TrailPreset.Pierce);
            CreateSkillProjectile(firePos, left, new Color(0.5f, 0.9f, 1f, 0.7f), skill2Range, 0.1f,
                MoShou.Combat.ProjectileTrail.TrailPreset.Pierce);
        }
    }

    /// <summary>
    /// 创建技能弹道
    /// </summary>
    void CreateSkillProjectile(Vector3 startPos, Vector3 direction, Color color, float range,
        float scale = 0.6f, MoShou.Combat.ProjectileTrail.TrailPreset trailPreset = MoShou.Combat.ProjectileTrail.TrailPreset.Arrow)
    {
        GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        proj.name = "SkillProjectile";
        proj.transform.position = startPos;
        proj.transform.localScale = Vector3.one * scale;

        Destroy(proj.GetComponent<Collider>());

        var renderer = proj.GetComponent<Renderer>();
        // 使用URP兼容Shader，fallback到Standard
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Sprites/Default")
                     ?? Shader.Find("Standard");
        Material mat = new Material(shader);
        mat.color = color;
        // URP Unlit需要设置_BaseColor
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        renderer.material = mat;

        // 添加移动
        var mover = proj.AddComponent<ProjectileMover>();
        mover.target = startPos + direction * range;
        mover.speed = 20f;
        mover.destroyDelay = range / 20f;
        mover.enableTrail = true;
        mover.projectileScale = scale;
        mover.trailPreset = trailPreset;

        Debug.Log($"[Player] 技能弹道已创建: pos={startPos}, scale={scale}, shader={shader?.name}");
    }

    /// <summary>
    /// 创建战吼特效（扩散环）
    /// </summary>
    void CreateShoutVFX()
    {
        // 创建多个扩散的圆环
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ShoutVFX";
            ring.transform.position = transform.position + Vector3.up * 0.1f;
            ring.transform.localScale = new Vector3(0.2f, 0.02f, 0.2f);

            Destroy(ring.GetComponent<Collider>());

            var renderer = ring.GetComponent<Renderer>();
            Shader ringShader = Shader.Find("Universal Render Pipeline/Unlit")
                             ?? Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Sprites/Default")
                             ?? Shader.Find("Standard");
            Material mat = new Material(ringShader);
            Color shoutColor = new Color(1f, 0.8f, 0.2f, 0.8f);
            mat.color = shoutColor;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", shoutColor);
            renderer.material = mat;

            // 添加扩散动画组件
            var expander = ring.AddComponent<RingExpander>();
            expander.delay = i * 0.15f;
            expander.expandSpeed = 3f;
            expander.maxScale = 2.5f;
            expander.fadeSpeed = 2f;
        }
    }

    /// <summary>
    /// 创建攻击特效（带粒子拖尾和随机弧度的弓箭）
    /// </summary>
    void CreateAttackVFX(Vector3 targetPos)
    {
        // 获取发射点（优先使用武器挂点，其次使用右手，最后使用默认偏移）
        Vector3 firePoint = GetFirePoint();

        // 调整目标点到怪物模型中心（而非脚底）
        Vector3 adjustedTarget = targetPos + Vector3.up * 0.8f;

        // 创建主弹体
        GameObject vfx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vfx.name = "AttackVFX_Arrow";
        vfx.transform.position = firePoint;
        vfx.transform.localScale = Vector3.one * 0.15f;

        // 移除碰撞
        Destroy(vfx.GetComponent<Collider>());

        // 设置材质 - 亮黄色核心（URP兼容）
        var renderer = vfx.GetComponent<Renderer>();
        Shader atkShader = Shader.Find("Universal Render Pipeline/Unlit")
                        ?? Shader.Find("Universal Render Pipeline/Lit")
                        ?? Shader.Find("Sprites/Default")
                        ?? Shader.Find("Standard");
        Material mat = new Material(atkShader);
        Color atkColor = new Color(1f, 0.9f, 0.3f, 1f); // 亮金黄色
        mat.color = atkColor;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", atkColor);
        renderer.material = mat;

        // 添加粒子拖尾系统
        var trailPS = CreateTrailParticleSystem(vfx);

        // 添加弧形移动逻辑（使用调整后的目标点）
        var mover = vfx.AddComponent<ArcProjectileMover>();
        mover.Initialize(adjustedTarget, Random.Range(12f, 18f), Random.Range(-0.8f, 0.8f));
    }

    /// <summary>
    /// 获取发射点位置（武器挂点 > 右手 > 默认偏移）
    /// </summary>
    Vector3 GetFirePoint()
    {
        // 优先查找武器挂点
        Transform weaponPoint = FindChildRecursive(transform, "WeaponPoint");
        if (weaponPoint == null) weaponPoint = FindChildRecursive(transform, "Weapon_Point");
        if (weaponPoint == null) weaponPoint = FindChildRecursive(transform, "FirePoint");
        if (weaponPoint == null) weaponPoint = FindChildRecursive(transform, "Fire_Point");

        if (weaponPoint != null)
        {
            return weaponPoint.position;
        }

        // 其次查找右手骨骼
        Transform rightHand = FindChildRecursive(transform, "RightHand");
        if (rightHand == null) rightHand = FindChildRecursive(transform, "Right_Hand");
        if (rightHand == null) rightHand = FindChildRecursive(transform, "Hand_R");
        if (rightHand == null) rightHand = FindChildRecursive(transform, "Bip001 R Hand");
        if (rightHand == null) rightHand = FindChildRecursive(transform, "mixamorig:RightHand");

        if (rightHand != null)
        {
            return rightHand.position;
        }

        // 默认：角色前方略高的位置（模拟弓箭手射击位置）
        return transform.position + Vector3.up * 1.3f + transform.forward * 0.3f + transform.right * 0.3f;
    }

    /// <summary>
    /// 递归查找子物体
    /// </summary>
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(name.ToLower()))
            {
                return child;
            }
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// 创建粒子拖尾系统
    /// </summary>
    ParticleSystem CreateTrailParticleSystem(GameObject parent)
    {
        GameObject trailGO = new GameObject("TrailParticles");
        trailGO.transform.SetParent(parent.transform, false);
        trailGO.transform.localPosition = Vector3.zero;

        ParticleSystem ps = trailGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.9f, 0.4f, 1f),
            new Color(1f, 0.6f, 0.1f, 0.8f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 50;

        var emission = ps.emission;
        emission.rateOverTime = 60f;

        var shape = ps.shape;
        shape.enabled = false;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.5f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // 使用默认粒子材质
        var psRenderer = trailGO.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psRenderer.material.SetColor("_Color", Color.white);

        return ps;
    }
    
    public void TakeDamage(float damage)
    {
        // 应用防御减伤
        float actualDamage = CombatSystem.CalculateDamage(damage, defense);

        // 先消耗护盾
        if (shieldAmount > 0)
        {
            if (shieldAmount >= actualDamage)
            {
                shieldAmount -= actualDamage;
                // 显示护盾吸收飘字
                DamagePopup.CreateWorldSpace(
                    transform.position + Vector3.up * 2f,
                    Mathf.RoundToInt(actualDamage),
                    DamageType.Miss // 用Miss类型表示护盾吸收
                );
                Debug.Log($"[Player] Shield absorbed {actualDamage} damage. Shield: {shieldAmount}");
                return;
            }
            else
            {
                actualDamage -= shieldAmount;
                Debug.Log($"[Player] Shield absorbed {shieldAmount} damage, breaking shield");
                shieldAmount = 0;
            }
        }

        currentHealth -= actualDamage;

        // 播放受击动画
        PlayHitAnimation();

        // 显示伤害飘字
        DamagePopup.CreateWorldSpace(
            transform.position + Vector3.up * 2f,
            Mathf.RoundToInt(actualDamage),
            DamageType.Normal
        );

        Debug.Log($"[Player] Took {actualDamage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("[Player] Died!");

        // 播放死亡动画
        PlayDeathAnimation();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Defeat);
        }
    }
}

/// <summary>
/// 环形扩散特效组件
/// </summary>
public class RingExpander : MonoBehaviour
{
    public float delay = 0f;
    public float expandSpeed = 5f;
    public float maxScale = 5f;
    public float fadeSpeed = 2f;

    private float timer = 0f;
    private bool started = false;
    private Renderer rend;
    private Color startColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            startColor = rend.material.color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < delay) return;

        if (!started)
        {
            started = true;
            timer = 0f;
        }

        // 扩展
        float currentScale = transform.localScale.x + expandSpeed * Time.deltaTime;
        if (currentScale < maxScale)
        {
            transform.localScale = new Vector3(currentScale, 0.02f, currentScale);
        }

        // 淡出
        if (rend != null)
        {
            float alpha = Mathf.Max(0, startColor.a - fadeSpeed * timer);
            rend.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            if (alpha <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
