using UnityEngine;
using MoShou.UI;

/// <summary>
/// 简单掉落物行为组件
/// 实现：浮动效果、自动吸附玩家、拾取效果
/// </summary>
public class SimpleDropBehavior : MonoBehaviour
{
    [Header("设置")]
    public float bobSpeed = 3f;          // 上下浮动速度
    public float bobHeight = 0.15f;      // 浮动高度
    public float rotateSpeed = 90f;      // 旋转速度
    public float pickupRadius = 1.5f;    // 拾取距离
    public float magnetRadius = 3f;      // 吸附距离
    public float magnetSpeed = 8f;       // 吸附速度
    public float lifetime = 30f;         // 存在时间

    private string dropType;
    private int amount;
    private string itemId;  // 物品/装备ID
    private Transform player;
    private Vector3 startPosition;
    private float spawnTime;
    private bool isBeingPickedUp = false;
    private Renderer rend;

    public void Initialize(string type, int amt, string id = "")
    {
        dropType = type;
        amount = amt;
        itemId = id;  // 存储物品ID
        startPosition = transform.position;
        spawnTime = Time.time;

        rend = GetComponent<Renderer>();

        // 初始弹跳效果
        StartCoroutine(InitialBounce());
    }

    void Start()
    {
        // 查找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (startPosition == Vector3.zero)
        {
            startPosition = transform.position;
            spawnTime = Time.time;
        }

        // 性能优化：作为备份，30秒后强制销毁（即使Update没有被调用）
        Destroy(gameObject, lifetime + 1f);
    }

    System.Collections.IEnumerator InitialBounce()
    {
        // 初始向上弹跳
        Vector3 velocity = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(2f, 3.5f),
            Random.Range(-1f, 1f)
        );

        float bounceTime = 0f;
        float bounceDuration = 0.4f;

        while (bounceTime < bounceDuration)
        {
            bounceTime += Time.deltaTime;
            velocity.y -= 15f * Time.deltaTime; // 重力
            transform.position += velocity * Time.deltaTime;
            yield return null;
        }

        // 更新起始位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 生命周期检查
        if (Time.time - spawnTime > lifetime)
        {
            // 淡出销毁
            StartCoroutine(FadeOut());
            enabled = false;
            return;
        }

        // 旋转效果
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        if (!isBeingPickedUp)
        {
            // 上下浮动效果
            float bobOffset = Mathf.Sin((Time.time - spawnTime) * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * bobOffset;

            // 检查玩家距离
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);

                // 在吸附范围内开始吸附
                if (distance <= magnetRadius)
                {
                    isBeingPickedUp = true;
                }
            }
        }
        else
        {
            // 吸附到玩家
            if (player != null)
            {
                Vector3 targetPos = player.position + Vector3.up * 0.8f;
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    magnetSpeed * Time.deltaTime
                );

                // 加速效果（越近越快）
                float distance = Vector3.Distance(transform.position, targetPos);
                if (distance < pickupRadius)
                {
                    Pickup();
                }
            }
        }
    }

    void Pickup()
    {
        // 根据类型处理拾取
        switch (dropType)
        {
            case "Gold":
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddGold(amount);
                }
                CreatePickupEffect(new Color(1f, 0.84f, 0f));
                ShowPickupText($"+{amount} 金币", new Color(1f, 0.84f, 0f));
                break;

            case "Exp":
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddExp(amount);
                }
                CreatePickupEffect(new Color(0.5f, 0.8f, 1f));
                ShowPickupText($"+{amount} 经验", new Color(0.5f, 0.8f, 1f));
                break;

            case "Equipment":
                // 装备拾取 - 添加到背包
                if (MoShou.Systems.InventoryManager.Instance != null && !string.IsNullOrEmpty(itemId))
                {
                    int added = MoShou.Systems.InventoryManager.Instance.AddItem(itemId, 1);
                    if (added > 0)
                    {
                        // 获取装备名称用于显示
                        string equipName = GetEquipmentName(itemId);
                        CreatePickupEffect(new Color(0.8f, 0.4f, 1f));
                        ShowPickupText($"获得 {equipName}!", new Color(0.8f, 0.4f, 1f));
                        Debug.Log($"[Drop] 装备已添加到背包: {itemId}");
                    }
                    else
                    {
                        ShowPickupText("背包已满!", Color.red);
                        Debug.LogWarning($"[Drop] 背包已满，无法拾取装备: {itemId}");
                    }
                }
                else
                {
                    CreatePickupEffect(new Color(0.8f, 0.4f, 1f));
                    ShowPickupText("获得装备!", new Color(0.8f, 0.4f, 1f));
                    Debug.LogWarning($"[Drop] InventoryManager不存在或itemId为空: {itemId}");
                }
                break;
        }

        // 播放拾取音效
        if (MoShou.Systems.AudioManager.Instance != null)
        {
            MoShou.Systems.AudioManager.Instance.PlaySFX(MoShou.Systems.AudioManager.SFX.CoinPickup);
        }

        Destroy(gameObject);
    }

    void CreatePickupEffect(Color color)
    {
        // 创建拾取粒子效果
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "PickupParticle";
            particle.transform.position = transform.position;
            particle.transform.localScale = Vector3.one * 0.08f;

            Destroy(particle.GetComponent<Collider>());

            var pRend = particle.GetComponent<Renderer>();
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            Material mat = new Material(shader);
            mat.color = new Color(color.r, color.g, color.b, 0.9f);
            pRend.material = mat;

            // 向上飞散
            var scatter = particle.AddComponent<PickupParticle>();
            scatter.velocity = new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(2f, 4f),
                Random.Range(-1.5f, 1.5f)
            );
        }
    }

    void ShowPickupText(string text, Color color)
    {
        // 创建拾取飘字
        if (player != null)
        {
            DamagePopup.CreateWorldSpace(
                player.position + Vector3.up * 2.5f + Random.insideUnitSphere * 0.3f,
                0, // 使用文本模式时amount不重要
                DamageType.Heal, // 用Heal类型的绿色效果
                text
            );
        }
    }

    /// <summary>
    /// 获取装备名称
    /// </summary>
    string GetEquipmentName(string equipId)
    {
        if (MoShou.Systems.EquipmentManager.Instance != null)
        {
            var config = MoShou.Systems.EquipmentManager.Instance.GetEquipmentConfig(equipId);
            if (config != null)
            {
                return config.name;
            }
        }
        return equipId;  // 如果找不到配置，返回ID
    }

    System.Collections.IEnumerator FadeOut()
    {
        float fadeDuration = 0.5f;
        float elapsed = 0f;
        Color startColor = rend != null ? rend.material.color : Color.white;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (rend != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                rend.material.color = c;
            }

            transform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}

/// <summary>
/// 拾取粒子效果
/// </summary>
public class PickupParticle : MonoBehaviour
{
    public Vector3 velocity;
    public float lifetime = 0.5f;
    public float gravity = 5f;

    private float startTime;
    private Material mat;
    private Color startColor;

    void Start()
    {
        startTime = Time.time;
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
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
        transform.localScale = Vector3.one * Mathf.Lerp(0.08f, 0.02f, t);

        if (mat != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(0.9f, 0f, t);
            mat.color = c;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
