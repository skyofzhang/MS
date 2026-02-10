using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 战斗胜利结算屏幕
    /// 显示战斗奖励、星级评价等
    /// 对应效果图: UI_Result.png
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        public static ResultScreen Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text victoryTitleText;
        [SerializeField] private Text stageNameText;
        [SerializeField] private Image[] starImages;

        [Header("Rewards Display")]
        [SerializeField] private Text goldRewardText;
        [SerializeField] private Text expRewardText;
        [SerializeField] private Transform itemRewardsContainer;
        [SerializeField] private Text killCountText;
        [SerializeField] private Text waveInfoText;

        [Header("Buttons")]
        [SerializeField] private Button nextStageButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button returnButton;

        [Header("Animation Settings")]
        [SerializeField] private float starAnimationDelay = 0.3f;
        [SerializeField] private float numberAnimationDuration = 0.5f;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private bool isInitialized = false;
        private ResultData currentData;

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

            canvas = GetComponent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            gameObject.SetActive(false);
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

            if (backgroundImage == null)
            {
                CreateDynamicUI();
            }

            BindButtonEvents();
            isInitialized = true;
        }

        private void CreateDynamicUI()
        {
            // 确保有Canvas
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                gameObject.AddComponent<GraphicRaycaster>();
            }

            // 半透明背景遮罩
            GameObject overlay = CreatePanel("Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.7f);

            // 主面板
            GameObject mainPanel = CreatePanel("MainPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 1400));
            backgroundImage = mainPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            // 尝试加载效果图
            Sprite resultBg = Resources.Load<Sprite>("UI_Mockups/Screens/UI_Result");
            if (resultBg != null)
            {
                backgroundImage.sprite = resultBg;
                backgroundImage.type = Image.Type.Sliced;
            }

            // VICTORY横幅
            GameObject victoryBanner = CreatePanel("VictoryBanner", new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(600, 100));
            victoryBanner.transform.SetParent(mainPanel.transform, false);
            Image bannerBg = victoryBanner.AddComponent<Image>();
            bannerBg.color = new Color(1f, 0.8f, 0.2f, 0.9f);

            victoryTitleText = CreateText(victoryBanner.transform, "Title", "VICTORY!", 48, TextAnchor.MiddleCenter, Color.white);
            victoryTitleText.fontStyle = FontStyle.Bold;

            // 关卡名称
            stageNameText = CreateText(mainPanel.transform, "StageName", "Stage 1-1", 28, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.8f, 1f));
            RectTransform stageNameRect = stageNameText.GetComponent<RectTransform>();
            stageNameRect.anchorMin = new Vector2(0.5f, 0.82f);
            stageNameRect.anchorMax = new Vector2(0.5f, 0.82f);
            stageNameRect.sizeDelta = new Vector2(400, 40);

            // 星星容器
            GameObject starContainer = CreatePanel("StarContainer", new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(400, 100));
            starContainer.transform.SetParent(mainPanel.transform, false);
            HorizontalLayoutGroup starLayout = starContainer.AddComponent<HorizontalLayoutGroup>();
            starLayout.childAlignment = TextAnchor.MiddleCenter;
            starLayout.spacing = 30;
            starLayout.childControlWidth = false;
            starLayout.childControlHeight = false;

            starImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject($"Star{i + 1}");
                starObj.transform.SetParent(starContainer.transform, false);
                starImages[i] = starObj.AddComponent<Image>();
                starImages[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 灰色未激活
                RectTransform starRect = starObj.GetComponent<RectTransform>();
                starRect.sizeDelta = new Vector2(80, 80);

                // 星星文字
                Text starText = CreateText(starObj.transform, "StarIcon", "\u2605", 60, TextAnchor.MiddleCenter, starImages[i].color);
            }

            // 奖励面板
            GameObject rewardPanel = CreatePanel("RewardPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 350));
            rewardPanel.transform.SetParent(mainPanel.transform, false);
            Image rewardBg = rewardPanel.AddComponent<Image>();
            rewardBg.color = new Color(0.15f, 0.18f, 0.22f, 0.9f);

            // 金币奖励
            GameObject goldRow = CreateRewardRow(rewardPanel.transform, "GoldReward", "\uD83D\uDCB0", "Gold:", "0", UIStyleHelper.Colors.Gold, 0.75f);
            goldRewardText = goldRow.GetComponentsInChildren<Text>()[1];

            // 经验奖励
            GameObject expRow = CreateRewardRow(rewardPanel.transform, "ExpReward", "\u2728", "EXP:", "0", new Color(0.4f, 0.8f, 1f, 1f), 0.55f);
            expRewardText = expRow.GetComponentsInChildren<Text>()[1];

            // 击杀数
            GameObject killRow = CreateRewardRow(rewardPanel.transform, "KillCount", "\u2694", "Kills:", "0", Color.white, 0.35f);
            killCountText = killRow.GetComponentsInChildren<Text>()[1];

            // 波次信息
            GameObject waveRow = CreateRewardRow(rewardPanel.transform, "WaveInfo", "\uD83C\uDF0A", "Waves:", "0/0", new Color(0.6f, 0.8f, 0.6f, 1f), 0.15f);
            waveInfoText = waveRow.GetComponentsInChildren<Text>()[1];

            // 物品奖励容器
            itemRewardsContainer = CreatePanel("ItemRewards", new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(700, 150)).transform;
            itemRewardsContainer.SetParent(mainPanel.transform, false);
            Image itemsBg = itemRewardsContainer.gameObject.AddComponent<Image>();
            itemsBg.color = new Color(0.1f, 0.12f, 0.15f, 0.8f);

            HorizontalLayoutGroup itemsLayout = itemRewardsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            itemsLayout.childAlignment = TextAnchor.MiddleCenter;
            itemsLayout.spacing = 15;
            itemsLayout.padding = new RectOffset(20, 20, 10, 10);

            // 按钮容器
            GameObject buttonContainer = CreatePanel("Buttons", new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(800, 80));
            buttonContainer.transform.SetParent(mainPanel.transform, false);
            HorizontalLayoutGroup btnLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 30;
            btnLayout.childControlWidth = true;
            btnLayout.childForceExpandWidth = true;

            // 重试按钮
            retryButton = CreateButton(buttonContainer.transform, "Retry", "RETRY", new Color(0.7f, 0.5f, 0.2f, 1f));

            // 下一关按钮
            nextStageButton = CreateButton(buttonContainer.transform, "Next", "NEXT STAGE", new Color(0.3f, 0.7f, 0.3f, 1f));

            // 返回按钮
            returnButton = CreateButton(buttonContainer.transform, "Return", "RETURN", new Color(0.4f, 0.4f, 0.45f, 1f));
        }

        private GameObject CreateRewardRow(Transform parent, string name, string icon, string label, string value, Color color, float yAnchor)
        {
            GameObject row = CreatePanel(name, new Vector2(0.5f, yAnchor), new Vector2(0.5f, yAnchor), Vector2.zero, new Vector2(600, 60));
            row.transform.SetParent(parent, false);

            // 图标
            Text iconText = CreateText(row.transform, "Icon", icon, 32, TextAnchor.MiddleLeft, color);
            RectTransform iconRect = iconText.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0.15f, 1);
            iconRect.offsetMin = new Vector2(20, 0);
            iconRect.offsetMax = Vector2.zero;

            // 标签
            Text labelText = CreateText(row.transform, "Label", label, 26, TextAnchor.MiddleLeft, Color.white);
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.15f, 0);
            labelRect.anchorMax = new Vector2(0.5f, 1);

            // 数值
            Text valueText = CreateText(row.transform, "Value", value, 28, TextAnchor.MiddleRight, color);
            valueText.fontStyle = FontStyle.Bold;
            RectTransform valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.5f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMax = new Vector2(-20, 0);

            return row;
        }

        private Button CreateButton(Transform parent, string name, string text, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            LayoutElement layout = btnObj.AddComponent<LayoutElement>();
            layout.minWidth = 180;
            layout.preferredHeight = 70;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            Text btnText = CreateText(btnObj.transform, "Text", text, 24, TextAnchor.MiddleCenter, Color.white);
            btnText.fontStyle = FontStyle.Bold;

            return btn;
        }

        #region Helper Methods

        private GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return obj;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
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

        #region Button Events

        private void BindButtonEvents()
        {
            if (nextStageButton != null)
                nextStageButton.onClick.AddListener(OnNextStageClick);

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClick);

            if (returnButton != null)
                returnButton.onClick.AddListener(OnReturnClick);
        }

        private void OnNextStageClick()
        {
            PlayButtonFeedback(nextStageButton);

            Hide();

            // 加载下一关
            int nextStage = (currentData?.stageId ?? 0) + 1;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentLevel = nextStage;
            }
            SceneManager.LoadScene("GameScene");
        }

        private void OnRetryClick()
        {
            PlayButtonFeedback(retryButton);

            Hide();
            SceneManager.LoadScene("GameScene");
        }

        private void OnReturnClick()
        {
            PlayButtonFeedback(returnButton);

            Hide();
            SceneManager.LoadScene("StageSelect");
        }

        private void PlayButtonFeedback(Button button)
        {
            if (UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(button.transform);
            }
        }

        #endregion

        #region Display

        public void Show(ResultData data)
        {
            InitializeUI();

            currentData = data;
            gameObject.SetActive(true);

            // 显示数据
            if (stageNameText != null)
                stageNameText.text = data.stageName ?? $"Stage {data.stageId}";

            if (killCountText != null)
                killCountText.text = data.killCount.ToString();

            if (waveInfoText != null)
                waveInfoText.text = $"{data.wavesCompleted}/{data.totalWaves}";

            // 重置星星
            ResetStars();

            // 重置数值显示
            if (goldRewardText != null) goldRewardText.text = "0";
            if (expRewardText != null) expRewardText.text = "0";

            // 清除物品奖励
            ClearItemRewards();

            // 播放动画
            StartCoroutine(PlayShowAnimation(data));
        }

        private IEnumerator PlayShowAnimation(ResultData data)
        {
            // 淡入
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                if (UITween.Instance != null)
                {
                    UITween.Instance.FadeTo(canvasGroup, 1f, 0.3f, null);
                }
                else
                {
                    canvasGroup.alpha = 1f;
                }
            }

            yield return new WaitForSeconds(0.5f);

            // 星星动画
            yield return StartCoroutine(AnimateStars(data.starsEarned));

            yield return new WaitForSeconds(0.3f);

            // 数字滚动动画
            AnimateGoldCounter(data.goldReward);
            AnimateExpCounter(data.expReward);

            yield return new WaitForSeconds(0.5f);

            // 显示物品奖励
            if (data.itemRewards != null)
            {
                DisplayItemRewards(data.itemRewards);
            }

            // 保存奖励
            SaveRewards(data);
        }

        private void ResetStars()
        {
            if (starImages == null) return;

            foreach (var star in starImages)
            {
                if (star != null)
                {
                    star.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                    star.transform.localScale = Vector3.one;

                    // 更新星星文字颜色
                    Text starText = star.GetComponentInChildren<Text>();
                    if (starText != null)
                    {
                        starText.color = star.color;
                    }
                }
            }
        }

        private IEnumerator AnimateStars(int count)
        {
            if (starImages == null) yield break;

            Color starColor = UIStyleHelper.Colors.Gold;

            for (int i = 0; i < Mathf.Min(count, starImages.Length); i++)
            {
                yield return new WaitForSeconds(starAnimationDelay);

                if (starImages[i] != null)
                {
                    // 激活星星
                    starImages[i].color = starColor;

                    Text starText = starImages[i].GetComponentInChildren<Text>();
                    if (starText != null)
                    {
                        starText.color = starColor;
                    }

                    // 缩放动画
                    if (UITween.Instance != null)
                    {
                        UITween.Instance.ScalePunch(starImages[i].transform, Vector3.one * 0.3f, 0.3f, null);
                    }

                    // 播放音效
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX("SFX_Star");
                    }
                }
            }
        }

        private void AnimateGoldCounter(int targetValue)
        {
            if (goldRewardText != null && UITween.Instance != null)
            {
                UITween.Instance.NumberTo(goldRewardText, 0, targetValue, numberAnimationDuration, "+{0}", null);
            }
            else if (goldRewardText != null)
            {
                goldRewardText.text = $"+{targetValue}";
            }
        }

        private void AnimateExpCounter(int targetValue)
        {
            if (expRewardText != null && UITween.Instance != null)
            {
                UITween.Instance.NumberTo(expRewardText, 0, targetValue, numberAnimationDuration, "+{0}", null);
            }
            else if (expRewardText != null)
            {
                expRewardText.text = $"+{targetValue}";
            }
        }

        private void ClearItemRewards()
        {
            if (itemRewardsContainer == null) return;

            foreach (Transform child in itemRewardsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void DisplayItemRewards(List<ItemReward> items)
        {
            if (itemRewardsContainer == null || items == null) return;

            foreach (var item in items)
            {
                GameObject itemObj = new GameObject(item.itemName);
                itemObj.transform.SetParent(itemRewardsContainer, false);

                Image itemBg = itemObj.AddComponent<Image>();
                itemBg.color = UIStyleHelper.GetQualityColor(item.quality);

                RectTransform itemRect = itemObj.GetComponent<RectTransform>();
                itemRect.sizeDelta = new Vector2(100, 100);

                // 数量
                if (item.quantity > 1)
                {
                    Text quantityText = CreateText(itemObj.transform, "Quantity", $"x{item.quantity}", 16, TextAnchor.LowerRight, Color.white);
                    RectTransform qRect = quantityText.GetComponent<RectTransform>();
                    qRect.anchorMin = new Vector2(0.5f, 0);
                    qRect.anchorMax = new Vector2(1, 0.3f);
                }
            }
        }

        private void SaveRewards(ResultData data)
        {
            if (SaveSystem.Instance == null) return;

            // 添加金币和经验
            if (SaveSystem.Instance.CurrentPlayerStats != null)
            {
                SaveSystem.Instance.CurrentPlayerStats.AddGold(data.goldReward);
                SaveSystem.Instance.CurrentPlayerStats.AddExperience(data.expReward);
            }

            // 更新关卡进度
            SaveSystem.Instance.MarkStageCleared(data.stageId);

            // 更新星级
            string stageKey = $"stage_{data.stageId}_stars";
            int currentStars = PlayerPrefs.GetInt(stageKey, 0);
            if (data.starsEarned > currentStars)
            {
                PlayerPrefs.SetInt(stageKey, data.starsEarned);
            }

            SaveSystem.Instance.SaveGame();
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

    #region Data Classes

    [Serializable]
    public class ResultData
    {
        public int stageId;
        public string stageName;
        public int goldReward;
        public int expReward;
        public int killCount;
        public int wavesCompleted;
        public int totalWaves;
        public int starsEarned;
        public List<ItemReward> itemRewards;
    }

    [Serializable]
    public class ItemReward
    {
        public string itemId;
        public string itemName;
        public int quantity;
        public int quality; // 0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary
    }

    #endregion
}
