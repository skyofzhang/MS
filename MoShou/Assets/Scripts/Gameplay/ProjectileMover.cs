using UnityEngine;
using MoShou.Combat;

/// <summary>
/// 简单的投射物移动组件
/// </summary>
public class ProjectileMover : MonoBehaviour
{
    public Vector3 target;
    public float speed = 10f;
    public float destroyDelay = 1f;
    public bool enableTrail = true;
    public float projectileScale = 0.5f;  // 弹道大小（外部可设置）
    public ProjectileTrail.TrailPreset trailPreset = ProjectileTrail.TrailPreset.Arrow;

    private float startTime;
    private Vector3 startPos;
    private float initialScale;
    private ProjectileTrail trail;

    void Start()
    {
        startTime = Time.time;
        startPos = transform.position;
        initialScale = projectileScale;

        // 添加拖尾效果
        if (enableTrail)
        {
            trail = ProjectileTrail.AddToProjectile(gameObject, trailPreset);
        }

        // 播放射击音效
        if (MoShou.Systems.AudioManager.Instance != null)
        {
            MoShou.Systems.AudioManager.Instance.PlaySFX(MoShou.Systems.AudioManager.SFX.ArrowShoot);
        }
    }

    void Update()
    {
        // 移向目标
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 缩放效果 - 从初始大小缓慢缩小（保持可见）
        float elapsed = Time.time - startTime;
        float t = Mathf.Clamp01(elapsed / destroyDelay);
        float scale = Mathf.Lerp(initialScale, initialScale * 0.5f, t);
        transform.localScale = Vector3.one * scale;

        // 到达目标或超时销毁
        if (Vector3.Distance(transform.position, target) < 0.5f || elapsed > destroyDelay)
        {
            // 创建命中特效
            CreateHitEffect();
            Destroy(gameObject);
        }
    }

    void CreateHitEffect()
    {
        // 播放命中音效
        if (MoShou.Systems.AudioManager.Instance != null)
        {
            MoShou.Systems.AudioManager.Instance.PlaySFXAtPosition(
                MoShou.Systems.AudioManager.SFX.ArrowHit,
                target
            );
        }

        // 命中闪光
        GameObject hit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hit.name = "HitEffect";
        hit.transform.position = target;
        hit.transform.localScale = Vector3.one * 0.2f;

        Destroy(hit.GetComponent<Collider>());

        var renderer = hit.GetComponent<Renderer>();
        Shader hitShader = Shader.Find("Universal Render Pipeline/Unlit")
                        ?? Shader.Find("Universal Render Pipeline/Lit")
                        ?? Shader.Find("Sprites/Default")
                        ?? Shader.Find("Standard");
        Material mat = new Material(hitShader);
        mat.color = new Color(1f, 0.7f, 0.2f, 0.8f); // 明亮橙色
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", mat.color);
        renderer.material = mat;

        // 添加淡出效果
        var fade = hit.AddComponent<FadeAndDestroy>();
        fade.duration = 0.4f;
    }
}

/// <summary>
/// 淡出并销毁
/// </summary>
public class FadeAndDestroy : MonoBehaviour
{
    public float duration = 0.3f;
    private float startTime;
    private Material mat;
    private Color startColor;

    void Start()
    {
        startTime = Time.time;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;
            startColor = mat.color;
        }
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        float t = elapsed / duration;

        // 放大并淡出
        transform.localScale = Vector3.one * (0.2f + t * 0.3f);

        if (mat != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(0.8f, 0f, t);
            mat.color = c;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", c);
            }
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// 带弧度的投射物移动组件
/// 弹道会有随机弧度，更加生动
/// </summary>
public class ArcProjectileMover : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private float speed;
    private float arcOffset;        // 弧度偏移量
    private float startTime;
    private float journeyLength;
    private Vector3 arcDirection;   // 弧度方向（垂直于飞行方向）

    public void Initialize(Vector3 target, float moveSpeed, float arc)
    {
        startPos = transform.position;
        targetPos = target;
        speed = moveSpeed;
        arcOffset = arc;
        startTime = Time.time;
        journeyLength = Vector3.Distance(startPos, targetPos);

        // 计算弧度方向（水平垂直于飞行方向）
        Vector3 forward = (targetPos - startPos).normalized;
        arcDirection = Vector3.Cross(forward, Vector3.up).normalized;

        // 播放射击音效
        if (MoShou.Systems.AudioManager.Instance != null)
        {
            MoShou.Systems.AudioManager.Instance.PlaySFX(MoShou.Systems.AudioManager.SFX.ArrowShoot);
        }
    }

    void Update()
    {
        if (journeyLength <= 0)
        {
            Destroy(gameObject);
            return;
        }

        float elapsed = Time.time - startTime;
        float t = (speed * elapsed) / journeyLength;

        if (t >= 1f)
        {
            // 到达目标
            CreateHitEffect();
            Destroy(gameObject);
            return;
        }

        // 线性插值基础位置
        Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);

        // 添加弧形偏移 (使用sin曲线，中间最大)
        float arcAmount = Mathf.Sin(t * Mathf.PI) * arcOffset;
        Vector3 arcPos = linearPos + arcDirection * arcAmount;

        // 添加轻微的上下弧度
        float verticalArc = Mathf.Sin(t * Mathf.PI) * Mathf.Abs(arcOffset) * 0.3f;
        arcPos.y += verticalArc;

        transform.position = arcPos;

        // 面向飞行方向
        Vector3 nextPos = Vector3.Lerp(startPos, targetPos, Mathf.Min(t + 0.05f, 1f));
        float nextArc = Mathf.Sin(Mathf.Min(t + 0.05f, 1f) * Mathf.PI) * arcOffset;
        Vector3 nextArcPos = nextPos + arcDirection * nextArc;
        nextArcPos.y += Mathf.Sin(Mathf.Min(t + 0.05f, 1f) * Mathf.PI) * Mathf.Abs(arcOffset) * 0.3f;

        Vector3 flyDir = (nextArcPos - arcPos).normalized;
        if (flyDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(flyDir);
        }

        // 缩放效果
        transform.localScale = Vector3.one * 0.25f;
    }

    void CreateHitEffect()
    {
        // 播放命中音效
        if (MoShou.Systems.AudioManager.Instance != null)
        {
            MoShou.Systems.AudioManager.Instance.PlaySFXAtPosition(
                MoShou.Systems.AudioManager.SFX.ArrowHit,
                targetPos
            );
        }

        // 创建多个小粒子爆发效果
        for (int i = 0; i < 6; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "HitParticle";
            particle.transform.position = targetPos;
            particle.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);

            Destroy(particle.GetComponent<Collider>());

            var renderer = particle.GetComponent<Renderer>();
            Shader pShader = Shader.Find("Universal Render Pipeline/Unlit")
                          ?? Shader.Find("Universal Render Pipeline/Lit")
                          ?? Shader.Find("Sprites/Default")
                          ?? Shader.Find("Standard");
            Material mat = new Material(pShader);
            Color pColor = new Color(1f, Random.Range(0.6f, 0.9f), Random.Range(0.1f, 0.4f), 0.9f);
            mat.color = pColor;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", pColor);
            renderer.material = mat;

            // 添加随机飞散
            var scatter = particle.AddComponent<ParticleScatter>();
            scatter.velocity = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(1f, 4f),
                Random.Range(-3f, 3f)
            );
        }

        // 创建主命中闪光
        GameObject hit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hit.name = "HitFlash";
        hit.transform.position = targetPos;
        hit.transform.localScale = Vector3.one * 0.15f;

        Destroy(hit.GetComponent<Collider>());

        var hitRenderer = hit.GetComponent<Renderer>();
        Shader flashShader = Shader.Find("Universal Render Pipeline/Unlit")
                          ?? Shader.Find("Universal Render Pipeline/Lit")
                          ?? Shader.Find("Sprites/Default")
                          ?? Shader.Find("Standard");
        Material hitMat = new Material(flashShader);
        hitMat.color = new Color(1f, 0.9f, 0.5f, 0.8f);
        if (hitMat.HasProperty("_BaseColor")) hitMat.SetColor("_BaseColor", hitMat.color);
        hitRenderer.material = hitMat;

        hit.AddComponent<FadeAndDestroy>();
    }
}

/// <summary>
/// 粒子飞散效果
/// </summary>
public class ParticleScatter : MonoBehaviour
{
    public Vector3 velocity;
    public float lifetime = 0.4f;
    public float gravity = 8f;

    private float startTime;
    private Material mat;
    private Color startColor;

    void Start()
    {
        startTime = Time.time;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;
            startColor = mat.color;
        }
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        float t = elapsed / lifetime;

        // 应用速度和重力
        velocity.y -= gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        // 缩小并淡出
        transform.localScale = Vector3.one * Mathf.Lerp(0.05f, 0.01f, t);

        if (mat != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(0.9f, 0f, t);
            mat.color = c;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
