using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoShou.UI
{
    /// <summary>
    /// 伤害飘字组件
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [Header("设置")]
        [SerializeField] private Text damageText;
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float fadeSpeed = 1f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float randomOffsetX = 0.5f;

        [Header("颜色设置")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color critColor = Color.yellow;
        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color missColor = Color.gray;

        private float timer;
        private Color startColor;
        private Vector3 startScale;
        private bool isInitialized = false;

        /// <summary>
        /// 初始化飘字
        /// </summary>
        public void Initialize(int damage, DamageType type = DamageType.Normal)
        {
            if (damageText == null)
            {
                damageText = GetComponentInChildren<Text>();
                if (damageText == null)
                {
                    // 创建Text组件
                    damageText = gameObject.AddComponent<Text>();
                    damageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    damageText.alignment = TextAnchor.MiddleCenter;
                    damageText.fontSize = 24;
                }
            }

            // 设置文本
            switch (type)
            {
                case DamageType.Normal:
                    damageText.text = damage.ToString();
                    startColor = normalColor;
                    startScale = Vector3.one;
                    break;
                case DamageType.Critical:
                    damageText.text = $"{damage}!";
                    startColor = critColor;
                    startScale = Vector3.one * 1.5f;
                    break;
                case DamageType.Heal:
                    damageText.text = $"+{damage}";
                    startColor = healColor;
                    startScale = Vector3.one;
                    break;
                case DamageType.Miss:
                    damageText.text = "Miss";
                    startColor = missColor;
                    startScale = Vector3.one * 0.8f;
                    break;
            }

            damageText.color = startColor;
            transform.localScale = startScale;

            // 随机水平偏移
            float randomX = Random.Range(-randomOffsetX, randomOffsetX);
            transform.position += new Vector3(randomX, 0, 0);

            timer = 0;
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized) return;

            timer += Time.deltaTime;

            // 上浮
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // 淡出
            if (damageText != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
                damageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            // 缩小
            float scale = Mathf.Lerp(1f, 0.5f, timer / lifetime);
            transform.localScale = startScale * scale;

            // 销毁
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 静态方法 - 创建伤害飘字
        /// </summary>
        public static DamagePopup Create(Vector3 worldPosition, int damage, DamageType type = DamageType.Normal)
        {
            // 创建游戏对象
            GameObject popupObj = new GameObject("DamagePopup");

            // 转换为屏幕坐标
            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
                popupObj.transform.position = screenPos;
            }
            else
            {
                popupObj.transform.position = worldPosition;
            }

            // 添加Canvas组件使其在UI层显示
            Canvas canvas = popupObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // 添加DamagePopup组件
            DamagePopup popup = popupObj.AddComponent<DamagePopup>();
            popup.Initialize(damage, type);

            return popup;
        }

        /// <summary>
        /// 静态方法 - 在世界空间创建伤害飘字（跟随3D位置）
        /// </summary>
        public static DamagePopup CreateWorldSpace(Vector3 worldPosition, int damage, DamageType type = DamageType.Normal)
        {
            GameObject popupObj = new GameObject("DamagePopup_World");
            popupObj.transform.position = worldPosition + Vector3.up * 1.5f;

            // 使用3D文本或Billboard方式
            TextMesh textMesh = popupObj.AddComponent<TextMesh>();
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.1f;

            DamagePopup popup = popupObj.AddComponent<DamagePopup>();

            // 设置文本
            switch (type)
            {
                case DamageType.Normal:
                    textMesh.text = damage.ToString();
                    textMesh.color = popup.normalColor;
                    break;
                case DamageType.Critical:
                    textMesh.text = $"{damage}!";
                    textMesh.color = popup.critColor;
                    popupObj.transform.localScale = Vector3.one * 1.5f;
                    break;
                case DamageType.Heal:
                    textMesh.text = $"+{damage}";
                    textMesh.color = popup.healColor;
                    break;
                case DamageType.Miss:
                    textMesh.text = "Miss";
                    textMesh.color = popup.missColor;
                    break;
            }

            popup.isInitialized = true;
            popup.startColor = textMesh.color;
            popup.startScale = popupObj.transform.localScale;

            // 添加Billboard行为（始终面向摄像机）
            popupObj.AddComponent<BillboardBehavior>();

            return popup;
        }
    }

    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        Normal,
        Critical,
        Heal,
        Miss
    }

    /// <summary>
    /// Billboard行为 - 始终面向摄像机
    /// </summary>
    public class BillboardBehavior : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                transform.Rotate(0, 180, 0);
            }
        }
    }
}
