using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 关卡选择屏幕
    /// 显示章节标签和地图路径关卡节点
    /// 对应效果图: UI_StageSelect.png
    /// </summary>
    public class StageSelectScreen : MonoBehaviour
    {
        public static StageSelectScreen Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Transform chapterTabContainer;
        [SerializeField] private Transform stageContainer;
        [SerializeField] private ScrollRect stageScrollRect;
        [SerializeField] private Button backButton;

        [Header("Stage Button Prefab")]
        [SerializeField] private GameObject stageButtonPrefab;

        [Header("Visual Settings")]
        [SerializeField] private Color unlockedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color completedColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color currentChapterColor = new Color(1f, 0.8f, 0.2f, 1f);

        // State
        private int currentChapter = 1;
        private int maxChapters = 3;
        private int stagesPerChapter = 10;
        private List<StageButtonData> stageButtons = new List<StageButtonData>();
        private List<Button> chapterTabs = new List<Button>();

        // Events
        public event Action<int> OnStageSelected;
        public event Action OnBackClicked;

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

            // 绑定返回按钮
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClick);
            }

            isInitialized = true;
        }

        private void CreateDynamicUI()
        {
            // 设置RectTransform为全屏
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // 背景图片
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform);
            backgroundImage = bgObj.AddComponent<Image>();
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 尝试加载效果图作为背景
            Sprite bgSprite = Resources.Load<Sprite>("UI_Mockups/Screens/UI_StageSelect");
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                backgroundImage.type = Image.Type.Simple;
                backgroundImage.preserveAspect = false;
            }
            else
            {
                // 后备渐变背景
                backgroundImage.color = new Color(0.08f, 0.1f, 0.15f, 1f);
            }

            // 标题
            CreateTitleBar();

            // 章节标签容器
            CreateChapterTabs();

            // 关卡滚动区域
            CreateStageScrollArea();

            // 返回按钮
            CreateBackButton();

            // 装饰边框
            CreateDecorations();
        }

        private void CreateTitleBar()
        {
            GameObject titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(transform);
            RectTransform titleRect = titleBar.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(0, 80);

            // 标题背景
            Image titleBg = titleBar.AddComponent<Image>();
            titleBg.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);

            // 标题文字
            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(titleBar.transform);
            Text titleText = titleTextObj.AddComponent<Text>();
            titleText.text = "SELECT STAGE";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = UIStyleHelper.Colors.Gold;

            RectTransform textRect = titleTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // 标题装饰线
            CreateHorizontalLine(titleBar.transform, new Vector2(0.1f, 0), new Vector2(0.9f, 0), 3, UIStyleHelper.Colors.Gold);
        }

        private void CreateChapterTabs()
        {
            GameObject tabContainer = new GameObject("ChapterTabs");
            tabContainer.transform.SetParent(transform);
            chapterTabContainer = tabContainer.transform;

            RectTransform containerRect = tabContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0.5f, 1);
            containerRect.anchoredPosition = new Vector2(0, -100);
            containerRect.sizeDelta = new Vector2(0, 60);

            // 水平布局
            HorizontalLayoutGroup layout = tabContainer.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.spacing = 15;
            layout.padding = new RectOffset(50, 50, 5, 5);

            // 创建章节标签
            string[] chapterNames = { "Chapter I", "Chapter II", "Chapter III" };
            for (int i = 0; i < maxChapters; i++)
            {
                CreateChapterTab(tabContainer.transform, i + 1, chapterNames[i]);
            }
        }

        private void CreateChapterTab(Transform parent, int chapter, string name)
        {
            GameObject tabObj = new GameObject($"ChapterTab_{chapter}");
            tabObj.transform.SetParent(parent);

            RectTransform tabRect = tabObj.AddComponent<RectTransform>();
            tabRect.sizeDelta = new Vector2(180, 50);

            // 按钮背景
            Image tabBg = tabObj.AddComponent<Image>();
            tabBg.color = chapter == currentChapter ? currentChapterColor : new Color(0.2f, 0.22f, 0.28f, 0.9f);

            // 圆角效果尝试
            Sprite tabSprite = Resources.Load<Sprite>("Sprites/UI/Common/UI_Button_Rounded");
            if (tabSprite != null)
            {
                tabBg.sprite = tabSprite;
                tabBg.type = Image.Type.Sliced;
            }

            // 按钮组件
            Button tabButton = tabObj.AddComponent<Button>();
            int chapterIndex = chapter;
            tabButton.onClick.AddListener(() => ShowChapter(chapterIndex));
            chapterTabs.Add(tabButton);

            // 标签文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tabObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.text = name;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = chapter == currentChapter ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private void CreateStageScrollArea()
        {
            // 滚动区域
            GameObject scrollObj = new GameObject("StageScrollView");
            scrollObj.transform.SetParent(transform);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0.12f);
            scrollRect.anchorMax = new Vector2(1, 0.78f);
            scrollRect.offsetMin = new Vector2(30, 0);
            scrollRect.offsetMax = new Vector2(-30, 0);

            // 滚动视图背景
            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.1f, 0.12f, 0.15f, 0.5f);

            // Mask组件
            Mask mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // ScrollRect组件
            stageScrollRect = scrollObj.AddComponent<ScrollRect>();
            stageScrollRect.horizontal = false;
            stageScrollRect.vertical = true;
            stageScrollRect.movementType = ScrollRect.MovementType.Elastic;
            stageScrollRect.elasticity = 0.1f;

            // 内容容器
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform);
            stageContainer = contentObj.transform;

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 1200); // 动态调整

            stageScrollRect.content = contentRect;

            // 网格布局
            GridLayoutGroup grid = contentObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(180, 200);
            grid.spacing = new Vector2(25, 30);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.padding = new RectOffset(30, 30, 20, 20);

            // ContentSizeFitter
            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void CreateBackButton()
        {
            GameObject btnObj = new GameObject("BackButton");
            btnObj.transform.SetParent(transform);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(0, 1);
            btnRect.pivot = new Vector2(0, 1);
            btnRect.anchoredPosition = new Vector2(20, -15);
            btnRect.sizeDelta = new Vector2(90, 50);

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.3f, 0.32f, 0.38f, 0.9f);

            backButton = btnObj.AddComponent<Button>();
            backButton.onClick.AddListener(OnBackButtonClick);

            // 返回箭头
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(btnObj.transform);
            Text arrow = arrowObj.AddComponent<Text>();
            arrow.text = "◀ BACK";
            arrow.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            arrow.fontSize = 18;
            arrow.fontStyle = FontStyle.Bold;
            arrow.alignment = TextAnchor.MiddleCenter;
            arrow.color = Color.white;

            RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
            arrowRect.anchorMin = Vector2.zero;
            arrowRect.anchorMax = Vector2.one;
            arrowRect.offsetMin = Vector2.zero;
            arrowRect.offsetMax = Vector2.zero;
        }

        private void CreateDecorations()
        {
            // 左侧装饰
            CreateVerticalDecoration(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(15, 0), true);
            // 右侧装饰
            CreateVerticalDecoration(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-15, 0), false);
        }

        private void CreateVerticalDecoration(Vector2 anchorMin, Vector2 anchorMax, Vector2 position, bool isLeft)
        {
            GameObject decoObj = new GameObject(isLeft ? "LeftDecoration" : "RightDecoration");
            decoObj.transform.SetParent(transform);

            RectTransform decoRect = decoObj.AddComponent<RectTransform>();
            decoRect.anchorMin = anchorMin;
            decoRect.anchorMax = anchorMax;
            decoRect.pivot = new Vector2(isLeft ? 0 : 1, 0.5f);
            decoRect.anchoredPosition = position;
            decoRect.sizeDelta = new Vector2(8, 600);

            Image decoImg = decoObj.AddComponent<Image>();
            decoImg.color = UIStyleHelper.Colors.Gold;
        }

        private void CreateHorizontalLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float height, Color color)
        {
            GameObject lineObj = new GameObject("Line");
            lineObj.transform.SetParent(parent);

            RectTransform lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.anchorMin = anchorMin;
            lineRect.anchorMax = anchorMax;
            lineRect.pivot = new Vector2(0.5f, 0);
            lineRect.anchoredPosition = Vector2.zero;
            lineRect.sizeDelta = new Vector2(0, height);

            Image lineImg = lineObj.AddComponent<Image>();
            lineImg.color = color;
        }

        #region Stage Management

        public void ShowChapter(int chapter)
        {
            if (chapter < 1 || chapter > maxChapters) return;

            currentChapter = chapter;

            // 更新章节标签视觉
            UpdateChapterTabVisuals();

            // 生成关卡按钮
            GenerateStageButtons();

            // 按钮反馈
            if (UIFeedbackSystem.Instance != null && chapterTabs.Count >= chapter)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(chapterTabs[chapter - 1].transform);
            }
        }

        private void UpdateChapterTabVisuals()
        {
            for (int i = 0; i < chapterTabs.Count; i++)
            {
                Image tabBg = chapterTabs[i].GetComponent<Image>();
                Text tabText = chapterTabs[i].GetComponentInChildren<Text>();

                bool isSelected = (i + 1) == currentChapter;

                if (tabBg != null)
                {
                    tabBg.color = isSelected ? currentChapterColor : new Color(0.2f, 0.22f, 0.28f, 0.9f);
                }

                if (tabText != null)
                {
                    tabText.color = isSelected ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
                }
            }
        }

        private void GenerateStageButtons()
        {
            // 清除现有按钮
            foreach (var data in stageButtons)
            {
                if (data.buttonObject != null)
                {
                    Destroy(data.buttonObject);
                }
            }
            stageButtons.Clear();

            // 获取玩家进度
            int unlockedStage = GetUnlockedStage();

            // 创建关卡按钮
            for (int i = 0; i < stagesPerChapter; i++)
            {
                int stageIndex = (currentChapter - 1) * stagesPerChapter + i + 1;
                CreateStageButton(stageIndex, i + 1, unlockedStage);
            }
        }

        private void CreateStageButton(int stageId, int displayNumber, int unlockedStage)
        {
            GameObject btnObj = new GameObject($"Stage_{stageId}");
            btnObj.transform.SetParent(stageContainer);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(180, 200);

            // 确定关卡状态
            StageStatus status = GetStageStatus(stageId, unlockedStage);

            // 按钮背景
            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = GetStageColor(status);

            // 尝试加载关卡图标
            Sprite stageSprite = null;
            if (status == StageStatus.Locked)
            {
                stageSprite = Resources.Load<Sprite>("Sprites/UI/StageSelect/UI_Stage_Locked");
            }
            else
            {
                stageSprite = Resources.Load<Sprite>("Sprites/UI/StageSelect/UI_Stage_Unlocked");
            }

            if (stageSprite != null)
            {
                btnBg.sprite = stageSprite;
                btnBg.type = Image.Type.Sliced;
            }

            // 关卡编号
            GameObject numObj = new GameObject("Number");
            numObj.transform.SetParent(btnObj.transform);
            Text numText = numObj.AddComponent<Text>();
            numText.text = displayNumber.ToString();
            numText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            numText.fontSize = 48;
            numText.fontStyle = FontStyle.Bold;
            numText.alignment = TextAnchor.MiddleCenter;
            numText.color = status == StageStatus.Locked ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;

            RectTransform numRect = numObj.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0, 0.3f);
            numRect.anchorMax = new Vector2(1, 0.8f);
            numRect.offsetMin = Vector2.zero;
            numRect.offsetMax = Vector2.zero;

            // 锁定图标
            if (status == StageStatus.Locked)
            {
                GameObject lockObj = new GameObject("Lock");
                lockObj.transform.SetParent(btnObj.transform);
                Text lockText = lockObj.AddComponent<Text>();
                lockText.text = "🔒";
                lockText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lockText.fontSize = 36;
                lockText.alignment = TextAnchor.MiddleCenter;

                RectTransform lockRect = lockObj.GetComponent<RectTransform>();
                lockRect.anchorMin = new Vector2(0.5f, 0.5f);
                lockRect.anchorMax = new Vector2(0.5f, 0.5f);
                lockRect.sizeDelta = new Vector2(50, 50);
                lockRect.anchoredPosition = Vector2.zero;

                numText.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            }

            // 星级显示（已完成的关卡）
            if (status == StageStatus.Completed)
            {
                int stars = GetStageStars(stageId);
                CreateStarsDisplay(btnObj.transform, stars);
            }

            // 按钮交互
            Button btn = btnObj.AddComponent<Button>();
            if (status != StageStatus.Locked)
            {
                int capturedStageId = stageId;
                btn.onClick.AddListener(() => OnStageButtonClick(capturedStageId));
            }
            else
            {
                btn.interactable = false;
            }

            // 存储数据
            stageButtons.Add(new StageButtonData
            {
                stageId = stageId,
                status = status,
                buttonObject = btnObj,
                button = btn
            });
        }

        private void CreateStarsDisplay(Transform parent, int stars)
        {
            GameObject starsObj = new GameObject("Stars");
            starsObj.transform.SetParent(parent);

            RectTransform starsRect = starsObj.AddComponent<RectTransform>();
            starsRect.anchorMin = new Vector2(0, 0);
            starsRect.anchorMax = new Vector2(1, 0.25f);
            starsRect.offsetMin = Vector2.zero;
            starsRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = starsObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.spacing = 5;

            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject($"Star_{i + 1}");
                starObj.transform.SetParent(starsObj.transform);

                RectTransform starRect = starObj.AddComponent<RectTransform>();
                starRect.sizeDelta = new Vector2(30, 30);

                Text starText = starObj.AddComponent<Text>();
                starText.text = "★";
                starText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                starText.fontSize = 24;
                starText.alignment = TextAnchor.MiddleCenter;
                starText.color = i < stars ? UIStyleHelper.Colors.Gold : new Color(0.3f, 0.3f, 0.3f, 1f);
            }
        }

        private void OnStageButtonClick(int stageId)
        {
            // 按钮反馈
            var stageData = stageButtons.Find(s => s.stageId == stageId);
            if (stageData != null && UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(stageData.button.transform);
            }

            // 显示确认弹窗
            ShowStageConfirmPopup(stageId);
        }

        private void ShowStageConfirmPopup(int stageId)
        {
            // 使用ConfirmDialog显示关卡确认
            if (ConfirmDialog.Instance != null)
            {
                ConfirmDialog.Instance.Show(
                    $"Stage {stageId}",
                    $"Start Stage {stageId}?\n\nDifficulty: ★★☆\nRecommended Level: {stageId + 5}",
                    () => StartStage(stageId),
                    null
                );
            }
            else
            {
                // 直接开始关卡
                StartStage(stageId);
            }
        }

        private void StartStage(int stageId)
        {
            // 设置当前关卡
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentLevel = stageId;
            }

            OnStageSelected?.Invoke(stageId);

            // 加载游戏场景
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }

        #endregion

        #region Helper Methods

        private int GetUnlockedStage()
        {
            // 使用GameManager获取最高解锁关卡
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.CurrentLevel;
            }
            return 1;
        }

        private StageStatus GetStageStatus(int stageId, int unlockedStage)
        {
            if (stageId > unlockedStage)
            {
                return StageStatus.Locked;
            }
            else if (stageId < unlockedStage)
            {
                return StageStatus.Completed;
            }
            else
            {
                return StageStatus.Unlocked;
            }
        }

        private Color GetStageColor(StageStatus status)
        {
            switch (status)
            {
                case StageStatus.Completed:
                    return completedColor;
                case StageStatus.Unlocked:
                    return unlockedColor;
                case StageStatus.Locked:
                default:
                    return lockedColor;
            }
        }

        private int GetStageStars(int stageId)
        {
            // 从SaveSystem获取关卡星级
            if (MoShou.Systems.SaveSystem.Instance != null)
            {
                int stars = MoShou.Systems.SaveSystem.Instance.GetStageStars(stageId);
                return stars;
            }
            return 0;
        }

        #endregion

        #region Button Handlers

        private void OnBackButtonClick()
        {
            if (UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(backButton.transform);
            }

            OnBackClicked?.Invoke();

            // 返回主菜单
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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

            // 刷新关卡显示
            ShowChapter(currentChapter);
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

        #region Data Classes

        private class StageButtonData
        {
            public int stageId;
            public StageStatus status;
            public GameObject buttonObject;
            public Button button;
        }

        private enum StageStatus
        {
            Locked,
            Unlocked,
            Completed
        }

        #endregion
    }
}
