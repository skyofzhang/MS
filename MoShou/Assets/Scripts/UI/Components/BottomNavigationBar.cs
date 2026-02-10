using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace MoShou.UI
{
    /// <summary>
    /// 底部导航栏组件
    /// 提供主页、背包、战斗、商店、设置的快速导航
    /// 对应效果图: UI_BottomNav.png
    /// </summary>
    public class BottomNavigationBar : MonoBehaviour
    {
        public static BottomNavigationBar Instance { get; private set; }

        [Header("Tab Buttons")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button characterButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button settingsButton;

        [Header("Tab Icons")]
        [SerializeField] private Image homeIcon;
        [SerializeField] private Image characterIcon;
        [SerializeField] private Image inventoryIcon;
        [SerializeField] private Image shopIcon;
        [SerializeField] private Image settingsIcon;

        [Header("Tab Labels")]
        [SerializeField] private Text homeLabel;
        [SerializeField] private Text characterLabel;
        [SerializeField] private Text inventoryLabel;
        [SerializeField] private Text shopLabel;
        [SerializeField] private Text settingsLabel;

        [Header("Notification Badges")]
        [SerializeField] private GameObject inventoryBadge;
        [SerializeField] private Text inventoryBadgeText;
        [SerializeField] private GameObject shopBadge;
        [SerializeField] private Text shopBadgeText;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image highlightIndicator;

        // State
        private NavTab currentTab = NavTab.Home;
        private Dictionary<NavTab, TabData> tabDataMap = new Dictionary<NavTab, TabData>();

        // Events
        public event Action<NavTab> OnTabChanged;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private bool isInitialized = false;

        // Tab icons (Unicode/Emoji fallback)
        private readonly string[] tabIcons = { "\u2302", "\u263A", "\uD83C\uDF92", "\uD83D\uDED2", "\u2699" };
        private readonly string[] tabLabels = { "Home", "Hero", "Bag", "Shop", "Settings" };

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        void Start()
        {
            InitializeUI();
            SelectTab(NavTab.Home);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeUI()
        {
            if (isInitialized) return;

            // 如果没有预设UI引用，动态创建
            if (backgroundImage == null)
            {
                CreateDynamicUI();
            }

            // 绑定按钮事件
            BindButtonEvents();

            isInitialized = true;
        }

        private void CreateDynamicUI()
        {
            // 设置RectTransform
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0, 200);

            // 背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform);
            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.1f, 0.12f, 0.98f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 尝试加载效果图作为背景
            Sprite bottomNavSprite = Resources.Load<Sprite>("UI_Mockups/Components/UI_BottomNav");
            if (bottomNavSprite != null)
            {
                backgroundImage.sprite = bottomNavSprite;
                backgroundImage.type = Image.Type.Sliced;
            }

            // 顶部金色边框线
            GameObject topBorder = new GameObject("TopBorder");
            topBorder.transform.SetParent(transform);
            Image topBorderImg = topBorder.AddComponent<Image>();
            topBorderImg.color = UIStyleHelper.Colors.Gold;
            RectTransform borderRect = topBorder.GetComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0, 1);
            borderRect.anchorMax = new Vector2(1, 1);
            borderRect.pivot = new Vector2(0.5f, 1);
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.sizeDelta = new Vector2(0, 3);

            // Tab容器
            GameObject tabContainer = new GameObject("TabContainer");
            tabContainer.transform.SetParent(transform);
            RectTransform containerRect = tabContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = new Vector2(10, 10);
            containerRect.offsetMax = new Vector2(-10, -10);

            HorizontalLayoutGroup layout = tabContainer.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 5, 5);

            // 创建5个Tab
            CreateTab(tabContainer.transform, NavTab.Home, 0, out homeButton, out homeIcon, out homeLabel);
            CreateTab(tabContainer.transform, NavTab.Character, 1, out characterButton, out characterIcon, out characterLabel);
            CreateTab(tabContainer.transform, NavTab.Inventory, 2, out inventoryButton, out inventoryIcon, out inventoryLabel);
            CreateTab(tabContainer.transform, NavTab.Shop, 3, out shopButton, out shopIcon, out shopLabel);
            CreateTab(tabContainer.transform, NavTab.Settings, 4, out settingsButton, out settingsIcon, out settingsLabel);

            // 创建背包徽章
            CreateBadge(inventoryButton.transform, out inventoryBadge, out inventoryBadgeText);
            // 创建商店徽章
            CreateBadge(shopButton.transform, out shopBadge, out shopBadgeText);

            // 高亮指示器
            GameObject highlight = new GameObject("Highlight");
            highlight.transform.SetParent(transform);
            highlightIndicator = highlight.AddComponent<Image>();
            highlightIndicator.color = new Color(1f, 0.8f, 0.2f, 0.3f);
            RectTransform highlightRect = highlight.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0, 0);
            highlightRect.anchorMax = new Vector2(0.2f, 1);
            highlightRect.offsetMin = new Vector2(10, 10);
            highlightRect.offsetMax = new Vector2(-5, -10);
            highlight.transform.SetAsFirstSibling();
        }

        private void CreateTab(Transform parent, NavTab tab, int index, out Button button, out Image icon, out Text label)
        {
            GameObject tabObj = new GameObject($"Tab_{tab}");
            tabObj.transform.SetParent(parent);

            // 按钮
            button = tabObj.AddComponent<Button>();
            Image btnBg = tabObj.AddComponent<Image>();
            btnBg.color = new Color(0.15f, 0.18f, 0.22f, 0.8f);

            // 设置按钮颜色过渡
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            button.colors = colors;

            // 垂直布局
            VerticalLayoutGroup vLayout = tabObj.AddComponent<VerticalLayoutGroup>();
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = false;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = false;
            vLayout.childForceExpandHeight = false;
            vLayout.spacing = 5;
            vLayout.padding = new RectOffset(5, 5, 15, 10);

            // 图标
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(tabObj.transform);
            icon = iconObj.AddComponent<Image>();
            icon.color = normalColor;
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(50, 50);

            // 图标文字 (使用Unicode作为占位符)
            Text iconText = CreateText(iconObj.transform, "IconText", tabIcons[index], 32, TextAnchor.MiddleCenter, normalColor);

            // 标签
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(tabObj.transform);
            label = labelObj.AddComponent<Text>();
            label.text = tabLabels[index];
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = normalColor;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(80, 25);

            // 存储Tab数据
            tabDataMap[tab] = new TabData
            {
                button = button,
                icon = icon,
                label = label,
                iconText = iconText,
                index = index
            };
        }

        private void CreateBadge(Transform parent, out GameObject badge, out Text badgeText)
        {
            badge = new GameObject("Badge");
            badge.transform.SetParent(parent);
            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.color = new Color(1f, 0.2f, 0.2f, 1f); // 红色徽章
            RectTransform badgeRect = badge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1, 1);
            badgeRect.anchorMax = new Vector2(1, 1);
            badgeRect.pivot = new Vector2(1, 1);
            badgeRect.anchoredPosition = new Vector2(5, 5);
            badgeRect.sizeDelta = new Vector2(30, 25);

            badgeText = CreateText(badge.transform, "Count", "0", 14, TextAnchor.MiddleCenter, Color.white);

            badge.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        private void BindButtonEvents()
        {
            if (homeButton != null) homeButton.onClick.AddListener(() => OnTabClick(NavTab.Home));
            if (characterButton != null) characterButton.onClick.AddListener(() => OnTabClick(NavTab.Character));
            if (inventoryButton != null) inventoryButton.onClick.AddListener(() => OnTabClick(NavTab.Inventory));
            if (shopButton != null) shopButton.onClick.AddListener(() => OnTabClick(NavTab.Shop));
            if (settingsButton != null) settingsButton.onClick.AddListener(() => OnTabClick(NavTab.Settings));
        }

        #region Tab Management

        private void OnTabClick(NavTab tab)
        {
            // 按钮反馈
            if (UIFeedbackSystem.Instance != null && tabDataMap.ContainsKey(tab))
            {
                UIFeedbackSystem.Instance.PlayButtonClick(tabDataMap[tab].button.transform);
            }

            SelectTab(tab);
        }

        public void SelectTab(NavTab tab)
        {
            if (currentTab == tab && isInitialized) return;

            NavTab previousTab = currentTab;
            currentTab = tab;

            UpdateTabVisuals();
            MoveHighlight(tab);

            OnTabChanged?.Invoke(tab);

            // 执行Tab对应的操作
            ExecuteTabAction(tab);
        }

        private void UpdateTabVisuals()
        {
            foreach (var kvp in tabDataMap)
            {
                bool isSelected = kvp.Key == currentTab;
                TabData data = kvp.Value;

                Color targetColor = isSelected ? selectedColor : normalColor;

                if (data.icon != null) data.icon.color = targetColor;
                if (data.label != null) data.label.color = targetColor;
                if (data.iconText != null) data.iconText.color = targetColor;

                // 选中时放大效果
                if (data.button != null)
                {
                    float scale = isSelected ? 1.1f : 1.0f;
                    data.button.transform.localScale = Vector3.one * scale;
                }
            }
        }

        private void MoveHighlight(NavTab tab)
        {
            if (highlightIndicator == null || !tabDataMap.ContainsKey(tab)) return;

            int index = tabDataMap[tab].index;
            float tabWidth = 1f / 5f;
            float startX = index * tabWidth;

            RectTransform highlightRect = highlightIndicator.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(startX, 0);
            highlightRect.anchorMax = new Vector2(startX + tabWidth, 1);
            highlightRect.offsetMin = new Vector2(5, 10);
            highlightRect.offsetMax = new Vector2(-5, -10);
        }

        private void ExecuteTabAction(NavTab tab)
        {
            switch (tab)
            {
                case NavTab.Home:
                    // 返回主菜单或主界面
                    break;

                case NavTab.Character:
                    // 打开角色信息界面
                    if (CharacterInfoScreen.Instance != null)
                    {
                        CharacterInfoScreen.Instance.Show();
                    }
                    break;

                case NavTab.Inventory:
                    // 打开背包
                    if (InventoryPanel.Instance != null)
                    {
                        InventoryPanel.Instance.Toggle();
                    }
                    break;

                case NavTab.Shop:
                    // 打开商店
                    if (ShopPanel.Instance != null)
                    {
                        ShopPanel.Instance.Toggle();
                    }
                    break;

                case NavTab.Settings:
                    // 打开设置
                    if (SettingsPanel.Instance != null)
                    {
                        SettingsPanel.Instance.Toggle();
                    }
                    break;
            }
        }

        #endregion

        #region Badge Management

        public void SetInventoryBadge(int count)
        {
            if (inventoryBadge == null) return;

            if (count > 0)
            {
                inventoryBadge.SetActive(true);
                if (inventoryBadgeText != null)
                {
                    inventoryBadgeText.text = count > 99 ? "99+" : count.ToString();
                }
            }
            else
            {
                inventoryBadge.SetActive(false);
            }
        }

        public void SetShopBadge(bool hasNew)
        {
            if (shopBadge == null) return;

            shopBadge.SetActive(hasNew);
            if (hasNew && shopBadgeText != null)
            {
                shopBadgeText.text = "!";
            }
        }

        public void ClearAllBadges()
        {
            SetInventoryBadge(0);
            SetShopBadge(false);
        }

        #endregion

        #region Visibility

        public void Show()
        {
            gameObject.SetActive(true);

            if (canvasGroup != null && UITween.Instance != null)
            {
                canvasGroup.alpha = 0f;
                UITween.Instance.FadeTo(canvasGroup, 1f, 0.3f, null);
            }
        }

        public void Hide()
        {
            if (canvasGroup != null && UITween.Instance != null)
            {
                UITween.Instance.FadeTo(canvasGroup, 0f, 0.2f, () => {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region Helper Classes

        private class TabData
        {
            public Button button;
            public Image icon;
            public Text label;
            public Text iconText;
            public int index;
        }

        #endregion
    }

    public enum NavTab
    {
        Home,
        Character,
        Inventory,
        Shop,
        Settings
    }
}
