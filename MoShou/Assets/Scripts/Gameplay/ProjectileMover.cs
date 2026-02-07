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

    private float startTime;
    private Vector3 startPos;
    private ProjectileTrail trail;

    void Start()
    {
        startTime = Time.time;
        startPos = transform.position;

        // 添加拖尾效果
        if (enableTrail)
        {
            trail = ProjectileTrail.AddToProjectile(gameObject, ProjectileTrail.TrailPreset.Arrow);
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

        // 缩放效果
        float elapsed = Time.time - startTime;
        float scale = Mathf.Lerp(0.3f, 0.1f, elapsed / destroyDelay);
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

        // 简单的命中闪光
        GameObject hit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hit.name = "HitEffect";
        hit.transform.position = target;
        hit.transform.localScale = Vector3.one * 0.5f;

        Destroy(hit.GetComponent<Collider>());

        var renderer = hit.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.5f, 0f, 0.6f); // 橙色
        renderer.material = mat;

        // 添加淡出效果
        hit.AddComponent<FadeAndDestroy>();
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
        transform.localScale = Vector3.one * (0.5f + t * 0.5f);

        if (mat != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(0.6f, 0f, t);
            mat.color = c;
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
