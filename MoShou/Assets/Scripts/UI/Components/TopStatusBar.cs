using UnityEngine;
using UnityEngine.UI;
using System;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 顶部状态栏组件
    /// 显示玩家头像、等级、金币、宝石等信息
    /// 对应效果图: UI_TopBar.png
    /// </summary>
    public class TopStatusBar : MonoBehaviour
    {
        public static TopStatusBar Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Text levelText;
        [SerializeField] private Slider expSlider;
        [SerializeField] private Text goldText;
        [SerializeField] private Image goldIcon;
        [SerializeField] private Text gemText;
        [SerializeField] private Image gemIcon;
        [SerializeField] private Button settingsButton;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.5f;

        // Cached values for animations
        private int lastGold = 0;
        private int lastExp = 0;
        private int lastLevel = 1;

        // Events
        public event Action OnSettingsClicked;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private bool isInitialized = false;

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
            SubscribeEvents();
            RefreshAll();
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
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

            // 绑定设置按钮
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClick);
            }

            isInitialized = true;
        }

        private void CreateDynamicUI()
        {
            // 设置RectTransform
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0, 150);

            // 背景
            backgroundImage = CreateChildImage("Background", new Vector2(0, 0), new Vector2(1, 1));
            backgroundImage.color = new Color(0.1f, 0.12f, 0.15f, 0.95f);

            // 尝试加载效果图作为背景
            Sprite topBarSprite = Resources.Load<Sprite>("UI_Mockups/Components/UI_TopBar");
            if (topBarSprite != null)
            {
                backgroundImage.sprite = topBarSprite;
                backgroundImage.type = Image.Type.Sliced;
            }

            // 头像容器 (左侧)
            GameObject avatarContainer = CreateChildPanel("AvatarContainer", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(100, 100));

            // 头像背景
            Image avatarBg = avatarContainer.AddComponent<Image>();
            avatarBg.color = new Color(0.2f, 0.25f, 0.3f, 1f);

            // 头像图片
            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(avatarContainer.transform);
            avatarImage = avatarObj.AddComponent<Image>();
            avatarImage.color = Color.white;
            RectTransform avatarRect = avatarObj.GetComponent<RectTransform>();
            avatarRect.anchorMin = Vector2.zero;
            avatarRect.anchorMax = Vector2.one;
            avatarRect.offsetMin = new Vector2(5, 5);
            avatarRect.offsetMax = new Vector2(-5, -5);

            // 等级徽章
            GameObject levelBadge = CreateChildPanel("LevelBadge", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 10), new Vector2(40, 25));
            levelBadge.transform.SetParent(avatarContainer.transform);
            Image levelBadgeBg = levelBadge.AddComponent<Image>();
            levelBadgeBg.color = UIStyleHelper.Colors.Gold;

            levelText = CreateText(levelBadge.transform, "LevelText", "Lv.1", 14, TextAnchor.MiddleCenter, Color.white);

            // 经验条 (头像下方)
            GameObject expBarContainer = CreateChildPanel("ExpBarContainer", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, 0), new Vector2(150, 15));

            // 创建经验条
            GameObject expBarObj = new GameObject("ExpSlider");
            expBarObj.transform.SetParent(expBarContainer.transform);
            expSlider = expBarObj.AddComponent<Slider>();
            RectTransform expRect = expBarObj.GetComponent<RectTransform>();
            expRect.anchorMin = Vector2.zero;
            expRect.anchorMax = Vector2.one;
            expRect.offsetMin = Vector2.zero;
            expRect.offsetMax = Vector2.zero;

            // 经验条背景
            GameObject expBg = new GameObject("Background");
            expBg.transform.SetParent(expBarObj.transform);
            Image expBgImg = expBg.AddComponent<Image>();
            expBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            RectTransform expBgRect = expBg.GetComponent<RectTransform>();
            expBgRect.anchorMin = Vector2.zero;
            expBgRect.anchorMax = Vector2.one;
            expBgRect.offsetMin = Vector2.zero;
            expBgRect.offsetMax = Vector2.zero;

            // 经验条填充
            GameObject expFill = new GameObject("Fill");
            expFill.transform.SetParent(expBarObj.transform);
            Image expFillImg = expFill.AddComponent<Image>();
            expFillImg.color = new Color(0.3f, 0.7f, 1f, 1f); // 蓝色经验条
            RectTransform expFillRect = expFill.GetComponent<RectTransform>();
            expFillRect.anchorMin = Vector2.zero;
            expFillRect.anchorMax = new Vector2(0.5f, 1f);
            expFillRect.offsetMin = Vector2.zero;
            expFillRect.offsetMax = Vector2.zero;

            expSlider.fillRect = expFillRect;
            expSlider.targetGraphic = expFillImg;
            expSlider.interactable = false;

            // 金币显示 (中间偏右)
            GameObject goldContainer = CreateChildPanel("GoldContainer", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-250, 0), new Vector2(120, 40));

            goldIcon = CreateChildImage("GoldIcon", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(30, 30), goldContainer.transform);
            goldIcon.color = UIStyleHelper.Colors.Gold;

            goldText = CreateText(goldContainer.transform, "GoldText", "0", 20, TextAnchor.MiddleLeft, UIStyleHelper.Colors.Gold);
            RectTransform goldTextRect = goldText.GetComponent<RectTransform>();
            goldTextRect.anchorMin = new Vector2(0, 0);
            goldTextRect.anchorMax = new Vector2(1, 1);
            goldTextRect.offsetMin = new Vector2(40, 0);
            goldTextRect.offsetMax = Vector2.zero;

            // 宝石显示 (右侧)
            GameObject gemContainer = CreateChildPanel("GemContainer", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-120, 0), new Vector2(100, 40));

            gemIcon = CreateChildImage("GemIcon", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(15, 0), new Vector2(25, 25), gemContainer.transform);
            gemIcon.color = new Color(0.4f, 0.8f, 1f, 1f); // 蓝色宝石

            gemText = CreateText(gemContainer.transform, "GemText", "0", 18, TextAnchor.MiddleLeft, Color.white);
            RectTransform gemTextRect = gemText.GetComponent<RectTransform>();
            gemTextRect.anchorMin = new Vector2(0, 0);
            gemTextRect.anchorMax = new Vector2(1, 1);
            gemTextRect.offsetMin = new Vector2(35, 0);
            gemTextRect.offsetMax = Vector2.zero;

            // 设置按钮 (最右侧)
            GameObject settingsBtnObj = CreateChildPanel("SettingsButton", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(50, 50));
            settingsButton = settingsBtnObj.AddComponent<Button>();
            Image settingsBtnImg = settingsBtnObj.AddComponent<Image>();
            settingsBtnImg.color = new Color(0.3f, 0.35f, 0.4f, 0.8f);

            // 设置图标文字
            Text settingsIcon = CreateText(settingsBtnObj.transform, "Icon", "\u2699", 28, TextAnchor.MiddleCenter, Color.white);
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }

        #region Helper Methods

        private Image CreateChildImage(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos = default, Vector2 size = default, Transform parent = null)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent ?? transform);
            Image img = obj.AddComponent<Image>();
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            if (size != default) rect.sizeDelta = size;
            return img;
        }

        private GameObject CreateChildPanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return obj;
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

        #endregion

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            // 订阅存档系统事件
            if (SaveSystem.Instance != null)
            {
                // SaveSystem事件订阅
            }
        }

        private void UnsubscribeEvents()
        {
            // 取消订阅
        }

        #endregion

        #region Refresh Methods

        public void RefreshAll()
        {
            RefreshLevel();
            RefreshCurrency();
        }

        private void RefreshLevel()
        {
            if (SaveSystem.Instance?.CurrentPlayerStats != null)
            {
                var stats = SaveSystem.Instance.CurrentPlayerStats;
                int level = stats.level;
                int currentExp = stats.experience;
                int maxExp = CalculateMaxExp(level);

                if (levelText != null)
                {
                    levelText.text = $"Lv.{level}";
                }

                if (expSlider != null)
                {
                    float expProgress = maxExp > 0 ? (float)currentExp / maxExp : 0f;
                    expSlider.value = expProgress;
                }

                lastLevel = level;
                lastExp = currentExp;
            }
            else
            {
                // 默认值
                if (levelText != null) levelText.text = "Lv.1";
                if (expSlider != null) expSlider.value = 0f;
            }
        }

        private void RefreshCurrency()
        {
            if (SaveSystem.Instance?.CurrentPlayerStats != null)
            {
                var stats = SaveSystem.Instance.CurrentPlayerStats;
                int gold = stats.gold;
                int gems = stats.gems;

                if (goldText != null)
                {
                    goldText.text = FormatNumber(gold);
                }

                if (gemText != null)
                {
                    gemText.text = FormatNumber(gems);
                }

                lastGold = gold;
            }
            else
            {
                if (goldText != null) goldText.text = "0";
                if (gemText != null) gemText.text = "0";
            }
        }

        private int CalculateMaxExp(int level)
        {
            // 简单的经验公式: 100 * level^1.5
            return Mathf.RoundToInt(100f * Mathf.Pow(level, 1.5f));
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000)
            {
                return (number / 1000000f).ToString("0.#") + "M";
            }
            else if (number >= 1000)
            {
                return (number / 1000f).ToString("0.#") + "K";
            }
            return number.ToString();
        }

        #endregion

        #region Animated Updates

        public void AnimateGoldChange(int newValue)
        {
            if (goldText != null && UITween.Instance != null)
            {
                UITween.Instance.NumberTo(goldText, lastGold, newValue, animationDuration, "{0}", null);

                // 金币图标闪烁效果
                if (goldIcon != null)
                {
                    UITween.Instance.Flash(goldIcon, Color.white, 3f, 0.3f, null);
                }
            }
            lastGold = newValue;
        }

        public void AnimateExpGain(int amount)
        {
            if (expSlider != null && UITween.Instance != null)
            {
                int newExp = lastExp + amount;
                int maxExp = CalculateMaxExp(lastLevel);
                float targetValue = (float)newExp / maxExp;

                UITween.Instance.FillTo(expSlider.fillRect.GetComponent<Image>(), targetValue, 1.5f, null);
                lastExp = newExp;
            }
        }

        public void AnimateLevelUp(int newLevel)
        {
            if (levelText != null && UIFeedbackSystem.Instance != null)
            {
                lastLevel = newLevel;
                levelText.text = $"Lv.{newLevel}";

                // 等级文字放大效果
                if (UITween.Instance != null)
                {
                    UITween.Instance.ScalePunch(levelText.transform, Vector3.one * 0.3f, 0.5f, null);
                }
            }
        }

        #endregion

        #region Button Handlers

        private void OnSettingsButtonClick()
        {
            // 按钮反馈
            if (UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(settingsButton.transform);
            }

            OnSettingsClicked?.Invoke();

            // 打开设置面板
            if (SettingsPanel.Instance != null)
            {
                SettingsPanel.Instance.Show();
            }
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

            RefreshAll();
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
    }
}
