using UnityEngine;
using UnityEngine.UI;

namespace MoShou.UI
{
    /// <summary>
    /// 怪物头顶血条组件
    /// 使用World Space Canvas显示在怪物头顶
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("设置")]
        public float heightOffset = 2.0f;
        public Vector2 barSize = new Vector2(1.0f, 0.12f);
        public bool hideWhenFull = false;
        public float hideDelay = 2f;

        private Canvas canvas;
        private RectTransform canvasRect;
        private Image bgImage;
        private Image fillImage;
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

            CreateHealthBarUI();
            UpdateFill();

            Debug.Log($"[EnemyHealthBar] 初始化完成 - 最大血量: {maxHP}");
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
            canvasRect.sizeDelta = barSize;
            canvasRect.localScale = Vector3.one;

            // 添加CanvasScaler（可选）
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
            borderRect.offsetMin = new Vector2(-0.02f, -0.02f);
            borderRect.offsetMax = new Vector2(0.02f, 0.02f);
            var borderImage = borderGO.AddComponent<Image>();
            borderImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            borderGO.transform.SetAsFirstSibling(); // 放到最底层
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
