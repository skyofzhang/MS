using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 战败屏幕
    /// 显示战斗统计和重试选项
    /// 对应效果图: UI_Defeat.png
    /// </summary>
    public class DefeatScreen : MonoBehaviour
    {
        public static DefeatScreen Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text defeatTitleText;
        [SerializeField] private Text stageInfoText;
        [SerializeField] private Text waveReachedText;

        [Header("Stats Display")]
        [SerializeField] private Text killCountText;
        [SerializeField] private Text survivalTimeText;

        [Header("Partial Rewards")]
        [SerializeField] private Text partialGoldText;
        [SerializeField] private Text partialExpText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button watchAdButton;

        [Header("Visual Effects")]
        [SerializeField] private Image skullIcon;
        [SerializeField] private Image darkOverlay;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private bool isInitialized = false;
        private DefeatData currentData;

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

            // 暗红色遮罩
            GameObject overlay = CreatePanel("DarkOverlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            darkOverlay = overlay.AddComponent<Image>();
            darkOverlay.color = new Color(0.15f, 0.05f, 0.05f, 0.85f);

            // 主面板
            GameObject mainPanel = CreatePanel("MainPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 1200));
            backgroundImage = mainPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.08f, 0.08f, 0.95f);

            // 尝试加载效果图
            Sprite defeatBg = Resources.Load<Sprite>("UI_Mockups/Screens/UI_Defeat");
            if (defeatBg != null)
            {
                backgroundImage.sprite = defeatBg;
                backgroundImage.type = Image.Type.Sliced;
            }

            // 骷髅图标
            GameObject skullContainer = CreatePanel("SkullIcon", new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, new Vector2(150, 150));
            skullContainer.transform.SetParent(mainPanel.transform, false);
            skullIcon = skullContainer.AddComponent<Image>();
            skullIcon.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

            // 骷髅文字图标
            Text skullText = CreateText(skullContainer.transform, "Skull", "\u2620", 100, TextAnchor.MiddleCenter, new Color(0.9f, 0.2f, 0.2f, 1f));

            // DEFEAT横幅
            GameObject defeatBanner = CreatePanel("DefeatBanner", new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(600, 100));
            defeatBanner.transform.SetParent(mainPanel.transform, false);
            Image bannerBg = defeatBanner.AddComponent<Image>();
            bannerBg.color = new Color(0.6f, 0.15f, 0.15f, 0.9f);

            defeatTitleText = CreateText(defeatBanner.transform, "Title", "DEFEAT", 52, TextAnchor.MiddleCenter, new Color(1f, 0.3f, 0.3f, 1f));
            defeatTitleText.fontStyle = FontStyle.Bold;

            // 关卡信息
            stageInfoText = CreateText(mainPanel.transform, "StageInfo", "Stage 1-1", 26, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f, 1f));
            RectTransform stageRect = stageInfoText.GetComponent<RectTransform>();
            stageRect.anchorMin = new Vector2(0.5f, 0.6f);
            stageRect.anchorMax = new Vector2(0.5f, 0.6f);
            stageRect.sizeDelta = new Vector2(400, 40);

            // 统计面板
            GameObject statsPanel = CreatePanel("StatsPanel", new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(700, 280));
            statsPanel.transform.SetParent(mainPanel.transform, false);
            Image statsBg = statsPanel.AddComponent<Image>();
            statsBg.color = new Color(0.12f, 0.1f, 0.1f, 0.9f);

            // 波次到达
            GameObject waveRow = CreateStatRow(statsPanel.transform, "WaveReached", "\uD83C\uDF0A", "Wave Reached:", "0", 0.78f);
            waveReachedText = waveRow.GetComponentsInChildren<Text>()[2];

            // 击杀数
            GameObject killRow = CreateStatRow(statsPanel.transform, "Kills", "\u2694", "Enemies Killed:", "0", 0.52f);
            killCountText = killRow.GetComponentsInChildren<Text>()[2];

            // 存活时间
            GameObject timeRow = CreateStatRow(statsPanel.transform, "Time", "\u23F1", "Survival Time:", "00:00", 0.26f);
            survivalTimeText = timeRow.GetComponentsInChildren<Text>()[2];

            // 部分奖励面板
            GameObject rewardPanel = CreatePanel("PartialRewards", new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(600, 120));
            rewardPanel.transform.SetParent(mainPanel.transform, false);
            Image rewardBg = rewardPanel.AddComponent<Image>();
            rewardBg.color = new Color(0.15f, 0.12f, 0.1f, 0.85f);

            // 标题
            Text rewardTitle = CreateText(rewardPanel.transform, "Title", "Partial Rewards", 20, TextAnchor.MiddleCenter, new Color(0.6f, 0.6f, 0.6f, 1f));
            RectTransform titleRect = rewardTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.7f);
            titleRect.anchorMax = new Vector2(1, 1);

            // 金币
            partialGoldText = CreateText(rewardPanel.transform, "Gold", "+0", 24, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            RectTransform goldRect = partialGoldText.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0, 0);
            goldRect.anchorMax = new Vector2(0.5f, 0.7f);

            // 经验
            partialExpText = CreateText(rewardPanel.transform, "Exp", "+0", 24, TextAnchor.MiddleCenter, new Color(0.4f, 0.8f, 1f, 1f));
            RectTransform expRect = partialExpText.GetComponent<RectTransform>();
            expRect.anchorMin = new Vector2(0.5f, 0);
            expRect.anchorMax = new Vector2(1, 0.7f);

            // 按钮容器
            GameObject buttonContainer = CreatePanel("Buttons", new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(750, 80));
            buttonContainer.transform.SetParent(mainPanel.transform, false);
            HorizontalLayoutGroup btnLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 25;
            btnLayout.childControlWidth = true;
            btnLayout.childForceExpandWidth = true;

            // 重试按钮 (橙色，醒目)
            retryButton = CreateButton(buttonContainer.transform, "Retry", "RETRY", new Color(0.9f, 0.5f, 0.2f, 1f));

            // 观看广告按钮 (可选)
            // watchAdButton = CreateButton(buttonContainer.transform, "WatchAd", "REVIVE (AD)", new Color(0.3f, 0.6f, 0.3f, 1f));

            // 返回按钮
            returnButton = CreateButton(buttonContainer.transform, "Return", "QUIT", new Color(0.4f, 0.35f, 0.35f, 1f));
        }

        private GameObject CreateStatRow(Transform parent, string name, string icon, string label, string value, float yAnchor)
        {
            GameObject row = CreatePanel(name, new Vector2(0.5f, yAnchor), new Vector2(0.5f, yAnchor), Vector2.zero, new Vector2(600, 60));
            row.transform.SetParent(parent, false);

            // 图标
            Text iconText = CreateText(row.transform, "Icon", icon, 28, TextAnchor.MiddleLeft, new Color(0.7f, 0.4f, 0.4f, 1f));
            RectTransform iconRect = iconText.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0.1f, 1);
            iconRect.offsetMin = new Vector2(15, 0);

            // 标签
            Text labelText = CreateText(row.transform, "Label", label, 22, TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.8f, 1f));
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.12f, 0);
            labelRect.anchorMax = new Vector2(0.65f, 1);

            // 数值
            Text valueText = CreateText(row.transform, "Value", value, 26, TextAnchor.MiddleRight, Color.white);
            valueText.fontStyle = FontStyle.Bold;
            RectTransform valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.65f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMax = new Vector2(-15, 0);

            return row;
        }

        private Button CreateButton(Transform parent, string name, string text, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            LayoutElement layout = btnObj.AddComponent<LayoutElement>();
            layout.minWidth = 200;
            layout.preferredHeight = 70;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            Text btnText = CreateText(btnObj.transform, "Text", text, 26, TextAnchor.MiddleCenter, Color.white);
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
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClick);

            if (returnButton != null)
                returnButton.onClick.AddListener(OnReturnClick);

            if (watchAdButton != null)
                watchAdButton.onClick.AddListener(OnWatchAdClick);
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

        private void OnWatchAdClick()
        {
            PlayButtonFeedback(watchAdButton);

            // TODO: 实现观看广告复活逻辑
            Debug.Log("[DefeatScreen] Watch Ad for revive - not implemented");
        }

        private void PlayButtonFeedback(Button button)
        {
            if (button != null && UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(button.transform);
            }
        }

        #endregion

        #region Display

        public void Show(DefeatData data)
        {
            InitializeUI();

            currentData = data;
            gameObject.SetActive(true);

            DisplayDefeatInfo(data);

            // 播放动画
            StartCoroutine(PlayShowAnimation());
        }

        private void DisplayDefeatInfo(DefeatData data)
        {
            if (stageInfoText != null)
                stageInfoText.text = data.stageName ?? $"Stage {data.stageId}";

            if (waveReachedText != null)
                waveReachedText.text = $"{data.waveReached}/{data.totalWaves}";

            if (killCountText != null)
                killCountText.text = data.killCount.ToString();

            if (survivalTimeText != null)
            {
                int minutes = Mathf.FloorToInt(data.survivalTime / 60f);
                int seconds = Mathf.FloorToInt(data.survivalTime % 60f);
                survivalTimeText.text = $"{minutes:00}:{seconds:00}";
            }

            if (partialGoldText != null)
                partialGoldText.text = $"+{data.partialGold}";

            if (partialExpText != null)
                partialExpText.text = $"+{data.partialExp}";

            // 保存部分奖励
            SavePartialRewards(data);
        }

        private IEnumerator PlayShowAnimation()
        {
            // 淡入
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                if (UITween.Instance != null)
                {
                    UITween.Instance.FadeTo(canvasGroup, 1f, 0.4f, null);
                }
                else
                {
                    canvasGroup.alpha = 1f;
                }
            }

            yield return new WaitForSeconds(0.2f);

            // 骷髅图标动画
            if (skullIcon != null && UITween.Instance != null)
            {
                skullIcon.transform.localScale = Vector3.zero;
                UITween.Instance.ScaleTo(skullIcon.transform, Vector3.one, 0.4f, null);
            }

            yield return new WaitForSeconds(0.3f);

            // DEFEAT横幅震动
            if (defeatTitleText != null && UITween.Instance != null)
            {
                UITween.Instance.ScalePunch(defeatTitleText.transform, Vector3.one * 0.2f, 0.3f, null);
            }

            // 播放失败音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_Defeat");
            }
        }

        private void SavePartialRewards(DefeatData data)
        {
            if (SaveSystem.Instance == null) return;

            // 只给部分奖励
            if (SaveSystem.Instance.CurrentPlayerStats != null)
            {
                if (data.partialGold > 0)
                {
                    SaveSystem.Instance.CurrentPlayerStats.AddGold(data.partialGold);
                }

                if (data.partialExp > 0)
                {
                    SaveSystem.Instance.CurrentPlayerStats.AddExperience(data.partialExp);
                }
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

    #region Data Class

    [Serializable]
    public class DefeatData
    {
        public int stageId;
        public string stageName;
        public int waveReached;
        public int totalWaves;
        public int killCount;
        public float survivalTime;
        public int partialGold;
        public int partialExp;
    }

    #endregion
}
