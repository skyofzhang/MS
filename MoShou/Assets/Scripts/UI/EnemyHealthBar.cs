using UnityEngine;
using UnityEngine.UI;

namespace MoShou.UI
{
    /// <summary>
    /// 怪物头顶血条组件
    /// 使用World Space Canvas显示在怪物头顶
    /// 显示血条和血量数字
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("设置")]
        public float heightOffset = 2.0f;   // 头顶偏移高度
        public float barWidth = 1.2f;       // 世界空间中的宽度
        public float barHeight = 0.15f;     // 世界空间中的高度
        public bool hideWhenFull = false;
        public float hideDelay = 2f;
        public bool showHealthText = true;  // 是否显示血量数字

        private Canvas canvas;
        private RectTransform canvasRect;
        private Image bgImage;
        private Image fillImage;
        private Text healthText;            // 血量数字
        private Transform target;
        private Camera mainCamera;
        private float maxHealth;
        private float currentHealth;
        private float lastDamageTime;

        /// <summary>
        /// 初始化血条
        /// </summary>
        public void Initialize(Transform targetTransform, float maxHP)
        {
            target = targetTransform;
            maxHealth = maxHP;
            currentHealth = maxHP;
            mainCamera = Camera.main;

            // 自动根据目标尺寸调整血条参数
            AutoAdjustToTarget();

            CreateHealthBarUI();
            UpdateFill();

            Debug.Log($"[EnemyHealthBar] 初始化完成 - 最大血量: {maxHP}, 血条宽度: {barWidth:F2}m");
        }

        /// <summary>
        /// 根据目标尺寸自动调整血条参数
        /// </summary>
        private void AutoAdjustToTarget()
        {
            if (target == null) return;

            // 尝试获取Renderer来估算目标尺寸
            Renderer rend = target.GetComponentInChildren<Renderer>();
            float targetHeight = 1f; // 默认1米

            if (rend != null)
            {
                targetHeight = rend.bounds.size.y;
            }
            else
            {
                // 尝试从Collider估算
                Collider col = target.GetComponentInChildren<Collider>();
                if (col != null)
                {
                    targetHeight = col.bounds.size.y;
                }
                else
                {
                    Collider2D col2d = target.GetComponentInChildren<Collider2D>();
                    if (col2d != null)
                    {
                        targetHeight = col2d.bounds.size.y;
                    }
                }
            }

            // 根据目标高度调整血条参数（适配1倍模型大小）
            // 血条宽度约为目标高度的120%，确保清晰可见
            barWidth = Mathf.Clamp(targetHeight * 1.2f, 0.8f, 2.5f);
            // 血条高度约为宽度的12%
            barHeight = barWidth * 0.12f;
            // 头顶偏移：高于目标顶部0.3米
            heightOffset = targetHeight + 0.3f;

            Debug.Log($"[EnemyHealthBar] AutoAdjust: targetHeight={targetHeight:F2}, barWidth={barWidth:F2}, barHeight={barHeight:F2}, heightOffset={heightOffset:F2}");
        }

        void CreateHealthBarUI()
        {
            // 创建Canvas
            GameObject canvasGO = new GameObject("HealthBarCanvas");
            canvasGO.transform.SetParent(transform, false);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;

            canvasRect = canvas.GetComponent<RectTransform>();
            // 设置Canvas像素大小（用于UI布局）
            canvasRect.sizeDelta = new Vector2(100, 10);  // 100x10像素的内部尺寸
            // 缩放到世界空间大小（关键！）
            canvasRect.localScale = new Vector3(barWidth / 100f, barHeight / 10f, 1f);

            // 添加CanvasScaler
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            // 背景
            GameObject bgGO = new GameObject("BG");
            bgGO.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 填充
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(bgGO.transform, false);
            RectTransform fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;
            fillImage = fillGO.AddComponent<Image>();
            fillImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // 红色血条
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;

            // 边框（可选）
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(bgGO.transform, false);
            RectTransform borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-1f, -1f);  // 更小边框
            borderRect.offsetMax = new Vector2(1f, 1f);
            var borderImage = borderGO.AddComponent<Image>();
            borderImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            borderGO.transform.SetAsFirstSibling(); // 放到最底层

            // 血量数字
            if (showHealthText)
            {
                GameObject textGO = new GameObject("HealthText");
                textGO.transform.SetParent(canvasGO.transform, false);
                RectTransform textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 1f);
                textRect.anchorMax = new Vector2(0.5f, 1f);
                textRect.pivot = new Vector2(0.5f, 0f);
                textRect.anchoredPosition = new Vector2(0, 2f);  // 血条上方
                textRect.sizeDelta = new Vector2(100, 12);

                healthText = textGO.AddComponent<Text>();
                healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                healthText.fontSize = 10;
                healthText.alignment = TextAnchor.MiddleCenter;
                healthText.color = Color.white;
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";

                // 添加描边
                var outline = textGO.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(0.5f, -0.5f);
            }
        }

        void LateUpdate()
        {
            if (target == null || mainCamera == null)
            {
                if (canvas != null)
                    canvas.gameObject.SetActive(false);
                return;
            }

            // 更新位置
            canvas.transform.position = target.position + Vector3.up * heightOffset;

            // 始终面向摄像机
            canvas.transform.rotation = mainCamera.transform.rotation;

            // 隐藏满血时的血条
            if (hideWhenFull && currentHealth >= maxHealth)
            {
                if (Time.time - lastDamageTime > hideDelay)
                {
                    canvas.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 更新血量
        /// </summary>
        public void SetHealth(float current, float max = -1)
        {
            if (max > 0) maxHealth = max;
            currentHealth = Mathf.Clamp(current, 0, maxHealth);
            lastDamageTime = Time.time;

            if (canvas != null)
                canvas.gameObject.SetActive(true);

            UpdateFill();
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            SetHealth(currentHealth - damage);
        }

        void UpdateFill()
        {
            if (fillImage == null) return;

            float ratio = maxHealth > 0 ? currentHealth / maxHealth : 0;
            fillImage.fillAmount = ratio;

            // 根据血量比例改变颜色
            if (ratio > 0.5f)
                fillImage.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2);
            else
                fillImage.color = Color.Lerp(Color.red, Color.yellow, ratio * 2);

            // 更新血量数字
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
            }
        }

        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        void OnDestroy()
        {
            if (canvas != null)
                Destroy(canvas.gameObject);
        }

        /// <summary>
        /// 静态工厂方法 - 为敌人创建血条
        /// </summary>
        public static EnemyHealthBar CreateForEnemy(GameObject enemy, float maxHP)
        {
            var healthBar = enemy.AddComponent<EnemyHealthBar>();
            healthBar.Initialize(enemy.transform, maxHP);
            return healthBar;
        }
    }
}
