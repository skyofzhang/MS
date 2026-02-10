using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MoShou.UI
{
    /// <summary>
    /// 物品标签/徽章系统
    /// 用于显示"新"、"可升级"、"热卖"等物品状态标签
    /// 符合策划案UI规范
    /// </summary>
    public class ItemBadge : MonoBehaviour
    {
        [Header("配置")]
        public BadgeType badgeType = BadgeType.New;
        public bool animate = true;
        public float pulseSpeed = 2f;
        public float pulseScale = 0.1f;

        private Image badgeImage;
        private Text badgeText;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private float animTime;

        /// <summary>
        /// 标签类型
        /// </summary>
        public enum BadgeType
        {
            New,        // 新物品 - 红色
            Upgrade,    // 可升级 - 绿色
            Hot,        // 热卖 - 橙色
            Sale,       // 特价 - 黄色
            Locked,     // 锁定 - 灰色
            Limited     // 限时 - 紫色
        }

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            originalScale = transform.localScale;
        }

        void Update()
        {
            // 脉冲动画
            if (animate && badgeType != BadgeType.Locked)
            {
                animTime += Time.deltaTime * pulseSpeed;
                float scale = 1f + Mathf.Sin(animTime) * pulseScale;
                transform.localScale = originalScale * scale;
            }
        }

        /// <summary>
        /// 设置标签类型
        /// </summary>
        public void SetType(BadgeType type)
        {
            badgeType = type;
            UpdateVisuals();
        }

        /// <summary>
        /// 更新视觉效果
        /// </summary>
        void UpdateVisuals()
        {
            if (badgeImage == null)
            {
                badgeImage = GetComponent<Image>();
            }
            if (badgeText == null)
            {
                badgeText = GetComponentInChildren<Text>();
            }

            Color bgColor;
            string text;

            switch (badgeType)
            {
                case BadgeType.New:
                    bgColor = new Color(0.9f, 0.2f, 0.2f); // 红色
                    text = "新";
                    break;
                case BadgeType.Upgrade:
                    bgColor = new Color(0.2f, 0.7f, 0.2f); // 绿色
                    text = "升";
                    break;
                case BadgeType.Hot:
                    bgColor = new Color(1f, 0.5f, 0.1f); // 橙色
                    text = "热";
                    break;
                case BadgeType.Sale:
                    bgColor = new Color(1f, 0.8f, 0.2f); // 黄色
                    text = "惠";
                    break;
                case BadgeType.Locked:
                    bgColor = new Color(0.5f, 0.5f, 0.5f); // 灰色
                    text = "锁";
                    animate = false;
                    break;
                case BadgeType.Limited:
                    bgColor = new Color(0.6f, 0.3f, 0.8f); // 紫色
                    text = "限";
                    break;
                default:
                    bgColor = Color.gray;
                    text = "?";
                    break;
            }

            if (badgeImage != null)
            {
                badgeImage.color = bgColor;
            }
            if (badgeText != null)
            {
                badgeText.text = text;
            }
        }

        /// <summary>
        /// 创建标签（静态工厂方法）
        /// </summary>
        public static ItemBadge Create(Transform parent, BadgeType type, Vector2 position)
        {
            GameObject badgeGO = new GameObject($"Badge_{type}");
            badgeGO.transform.SetParent(parent, false);

            RectTransform rect = badgeGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(36, 36);

            // 背景
            Image bg = badgeGO.AddComponent<Image>();

            // 文字
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(badgeGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textGO.AddComponent<Text>();
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = GetDefaultFont();

            // 描边
            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            // 添加组件
            ItemBadge badge = badgeGO.AddComponent<ItemBadge>();
            badge.badgeImage = bg;
            badge.badgeText = text;
            badge.SetType(type);

            return badge;
        }

        /// <summary>
        /// 创建"新"标签
        /// </summary>
        public static ItemBadge CreateNewBadge(Transform parent)
        {
            return Create(parent, BadgeType.New, new Vector2(-5, -5));
        }

        /// <summary>
        /// 创建"可升级"标签
        /// </summary>
        public static ItemBadge CreateUpgradeBadge(Transform parent)
        {
            return Create(parent, BadgeType.Upgrade, new Vector2(-5, -5));
        }

        /// <summary>
        /// 显示/隐藏标签
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        static Font GetDefaultFont()
        {
            string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
            foreach (string fontName in fontNames)
            {
                Font font = Resources.GetBuiltinResource<Font>(fontName);
                if (font != null) return font;
            }
            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }
    }

    /// <summary>
    /// 物品标签管理器
    /// 追踪玩家已查看的物品，管理"新"标签显示
    /// </summary>
    public static class ItemBadgeManager
    {
        private const string VIEWED_ITEMS_KEY = "ViewedItems";
        private static System.Collections.Generic.HashSet<string> viewedItems;

        /// <summary>
        /// 初始化（从PlayerPrefs加载）
        /// </summary>
        public static void Initialize()
        {
            viewedItems = new System.Collections.Generic.HashSet<string>();

            string savedData = PlayerPrefs.GetString(VIEWED_ITEMS_KEY, "");
            if (!string.IsNullOrEmpty(savedData))
            {
                string[] items = savedData.Split(',');
                foreach (string item in items)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        viewedItems.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// 标记物品为已查看
        /// </summary>
        public static void MarkAsViewed(string itemId)
        {
            if (viewedItems == null) Initialize();

            if (!viewedItems.Contains(itemId))
            {
                viewedItems.Add(itemId);
                SaveViewedItems();
            }
        }

        /// <summary>
        /// 检查物品是否是新的（未查看）
        /// </summary>
        public static bool IsNewItem(string itemId)
        {
            if (viewedItems == null) Initialize();
            return !viewedItems.Contains(itemId);
        }

        /// <summary>
        /// 保存已查看物品列表
        /// </summary>
        private static void SaveViewedItems()
        {
            string data = string.Join(",", viewedItems);
            PlayerPrefs.SetString(VIEWED_ITEMS_KEY, data);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 清除所有已查看记录（所有物品变为"新"）
        /// </summary>
        public static void ClearAllViewed()
        {
            viewedItems?.Clear();
            PlayerPrefs.DeleteKey(VIEWED_ITEMS_KEY);
            PlayerPrefs.Save();
        }
    }
}
