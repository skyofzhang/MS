using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using MoShou.Systems;
using MoShou.UI;
using MoShou.Data;

/// <summary>
/// 选关场景初始化 - 竖排卡片列表（100关）
/// 依据策划案: 竖屏 1080x1920
/// 风格: 卡通魔兽风格，竖排关卡卡片列表
/// </summary>
public class StageSelectSceneSetup : MonoBehaviour
{
    private static bool isInitialized = false;

    // 关卡配置缓存
    private StageConfigTable stageConfigTable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
    }

    static void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StageSelect")
        {
            var temp = new GameObject("_StageSelectLoader");
            temp.AddComponent<StageSelectDelayedSetup>();
        }
    }

    private class StageSelectDelayedSetup : MonoBehaviour
    {
        void Start()
        {
            if (FindObjectOfType<StageSelectSceneSetup>() == null)
            {
                var go = new GameObject("StageSelectSceneSetup");
                go.AddComponent<StageSelectSceneSetup>();
            }
            Destroy(gameObject);
        }
    }

    void Awake()
    {
        if (isInitialized)
        {
            Destroy(gameObject);
            return;
        }
        isInitialized = true;
        SetupStageSelect();
    }

    void OnDestroy()
    {
        isInitialized = false;
    }

    void SetupStageSelect()
    {
        Debug.Log("[StageSelectSetup] 开始创建选关UI（竖排卡片列表）...");

        // 加载关卡配置
        LoadStageConfigs();

        // 确保有EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 销毁所有现有的低优先级Canvas
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in existingCanvases)
        {
            if (canvas.sortingOrder < 500)
            {
                Debug.Log($"[StageSelectSetup] 立即销毁现有Canvas: {canvas.gameObject.name}");
                DestroyImmediate(canvas.gameObject);
            }
        }

        // 销毁旧的StageSelectManager
        var oldManager = FindObjectOfType<MoShou.Core.StageSelectManager>();
        if (oldManager != null)
        {
            Debug.Log("[StageSelectSetup] 立即销毁旧的StageSelectManager");
            DestroyImmediate(oldManager.gameObject);
        }

        // 销毁旧的StageSelectScreen
        var oldScreen = FindObjectOfType<MoShou.UI.StageSelectScreen>();
        if (oldScreen != null)
        {
            Debug.Log("[StageSelectSetup] 立即销毁旧的StageSelectScreen");
            DestroyImmediate(oldScreen.gameObject);
        }

        // 清理残留UI
        var stageButtonsParent = GameObject.Find("StageButtonsParent");
        if (stageButtonsParent != null)
        {
            Debug.Log("[StageSelectSetup] 销毁残留的StageButtonsParent");
            DestroyImmediate(stageButtonsParent);
        }

        // 创建新UI
        CreateStageSelectUI();
    }

    /// <summary>
    /// 加载关卡配置表
    /// </summary>
    void LoadStageConfigs()
    {
        TextAsset json = Resources.Load<TextAsset>("Configs/StageConfigs");
        if (json != null)
        {
            stageConfigTable = JsonUtility.FromJson<StageConfigTable>(json.text);
            Debug.Log($"[StageSelectSetup] 加载了 {stageConfigTable.stages.Length} 个关卡配置");
        }
        else
        {
            Debug.LogWarning("[StageSelectSetup] 无法加载 StageConfigs.json");
            stageConfigTable = new StageConfigTable { stages = new StageConfigEntry[0] };
        }
    }

    /// <summary>
    /// 获取关卡配置（优先JSON，否则算法生成）
    /// </summary>
    StageConfigEntry GetStageConfig(int stageNum)
    {
        // 先从JSON查找
        if (stageConfigTable != null && stageConfigTable.stages != null)
        {
            foreach (var entry in stageConfigTable.stages)
            {
                if (entry.id == stageNum)
                    return entry;
            }
        }

        // 算法生成 fallback
        string[] themeNames = { "未知区域", "荒野", "山脉", "沙漠", "冰原", "火山", "深渊", "天空", "混沌", "终末" };
        int regionIdx = Mathf.Clamp((stageNum - 1) / 10, 0, themeNames.Length - 1);

        return new StageConfigEntry
        {
            id = stageNum,
            name = $"{themeNames[regionIdx]}·关卡{stageNum}",
            chapter = regionIdx + 1,
            difficulty = Mathf.Min(5, 1 + (stageNum - 1) / 20),
            recommendedLevel = Mathf.Max(1, stageNum * 2 - 1),
            waveCount = 3 + stageNum / 10,
            goldReward = Mathf.FloorToInt(50 + stageNum * 30 + stageNum * stageNum * 0.5f),
            expReward = Mathf.FloorToInt(30 + stageNum * 20 + stageNum * stageNum / 3f)
        };
    }

    void CreateStageSelectUI()
    {
        // 创建Canvas
        GameObject canvasGO = new GameObject("StageSelectCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // === 背景层 ===
        CreateBackgroundLayer(canvasGO.transform);

        // === 顶部标题栏 ===
        CreateTopTitleBar(canvasGO.transform);

        // === 关卡卡片列表 ===
        CreateStageListArea(canvasGO.transform);

        // === 底部信息栏 ===
        CreateBottomInfoBar(canvasGO.transform);

        Debug.Log("[StageSelectSetup] 选关UI创建完成（竖排卡片列表）");
    }

    /// <summary>
    /// 创建背景层
    /// </summary>
    void CreateBackgroundLayer(Transform parent)
    {
        Sprite mockupBg = Resources.Load<Sprite>("UI_Mockups/Screens/UI_StageSelect");

        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(parent, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgGO.AddComponent<Image>();

        if (mockupBg != null)
        {
            bgImage.sprite = mockupBg;
            bgImage.preserveAspect = false;
            bgImage.color = Color.white;
        }
        else
        {
            bgImage.color = new Color(0.08f, 0.12f, 0.18f);

            // 渐变层
            GameObject gradientGO = new GameObject("Gradient");
            gradientGO.transform.SetParent(bgGO.transform, false);
            RectTransform gradRect = gradientGO.AddComponent<RectTransform>();
            gradRect.anchorMin = Vector2.zero;
            gradRect.anchorMax = new Vector2(1, 0.5f);
            gradRect.offsetMin = Vector2.zero;
            gradRect.offsetMax = Vector2.zero;

            Image gradImg = gradientGO.AddComponent<Image>();
            gradImg.color = new Color(0.03f, 0.05f, 0.08f, 0.7f);
        }

        // 暗角效果
        GameObject vignetteGO = new GameObject("Vignette");
        vignetteGO.transform.SetParent(bgGO.transform, false);
        RectTransform vigRect = vignetteGO.AddComponent<RectTransform>();
        vigRect.anchorMin = Vector2.zero;
        vigRect.anchorMax = Vector2.one;
        vigRect.offsetMin = Vector2.zero;
        vigRect.offsetMax = Vector2.zero;

        Image vigImage = vignetteGO.AddComponent<Image>();
        vigImage.color = new Color(0, 0, 0, 0.25f);
        vigImage.raycastTarget = false;
    }

    /// <summary>
    /// 创建顶部标题栏 - 简化版：只有"关卡选择"标题
    /// </summary>
    void CreateTopTitleBar(Transform parent)
    {
        GameObject topBarGO = new GameObject("TopTitleBar");
        topBarGO.transform.SetParent(parent, false);
        RectTransform topRect = topBarGO.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.anchoredPosition = Vector2.zero;
        topRect.sizeDelta = new Vector2(0, 140);

        // 半透明背景
        Image topBg = topBarGO.AddComponent<Image>();
        topBg.color = new Color(0.05f, 0.08f, 0.12f, 0.9f);

        // 金色底部边框线
        CreateGoldBorderLine(topBarGO.transform, false);

        // 标题文字 - "关卡选择"
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(topBarGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, -10);

        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "关卡选择";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = UIStyleHelper.Colors.Gold;
        titleText.font = UIStyleHelper.GetDefaultFont();

        // 标题描边
        Outline titleOutline = titleGO.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.3f, 0.2f, 0.1f);
        titleOutline.effectDistance = new Vector2(2, -2);

        // 标题阴影
        Shadow titleShadow = titleGO.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.6f);
        titleShadow.effectDistance = new Vector2(3, -3);

        // 返回主菜单按钮（右上角小按钮）
        GameObject backBtnGO = new GameObject("BackToMenuBtn");
        backBtnGO.transform.SetParent(topBarGO.transform, false);
        RectTransform backRect = backBtnGO.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 0.5f);
        backRect.anchorMax = new Vector2(0, 0.5f);
        backRect.anchoredPosition = new Vector2(60, -5);
        backRect.sizeDelta = new Vector2(70, 70);

        Image backBg = backBtnGO.AddComponent<Image>();
        backBg.color = new Color(0.2f, 0.25f, 0.35f, 0.85f);

        Button backBtn = backBtnGO.AddComponent<Button>();
        backBtn.targetGraphic = backBg;
        backBtn.onClick.AddListener(() =>
        {
            Debug.Log("[StageSelect] 返回主菜单");
            if (UIFeedbackSystem.Instance != null)
                UIFeedbackSystem.Instance.PlayButtonClick(backBtnGO.transform);
            SceneManager.LoadScene("MainMenu");
        });

        // 返回图标
        GameObject backIconGO = new GameObject("Icon");
        backIconGO.transform.SetParent(backBtnGO.transform, false);
        RectTransform backIconRect = backIconGO.AddComponent<RectTransform>();
        backIconRect.anchorMin = Vector2.zero;
        backIconRect.anchorMax = Vector2.one;
        backIconRect.offsetMin = Vector2.zero;
        backIconRect.offsetMax = Vector2.zero;

        Text backIconText = backIconGO.AddComponent<Text>();
        backIconText.text = "←";
        backIconText.fontSize = 36;
        backIconText.alignment = TextAnchor.MiddleCenter;
        backIconText.color = UIStyleHelper.Colors.Gold;
        backIconText.font = UIStyleHelper.GetDefaultFont();
    }

    void CreateGoldBorderLine(Transform parent, bool isTop)
    {
        GameObject borderGO = new GameObject("GoldBorder");
        borderGO.transform.SetParent(parent, false);
        RectTransform borderRect = borderGO.AddComponent<RectTransform>();

        if (isTop)
        {
            borderRect.anchorMin = new Vector2(0, 1);
            borderRect.anchorMax = new Vector2(1, 1);
            borderRect.pivot = new Vector2(0.5f, 1);
        }
        else
        {
            borderRect.anchorMin = new Vector2(0, 0);
            borderRect.anchorMax = new Vector2(1, 0);
            borderRect.pivot = new Vector2(0.5f, 0);
        }
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(0, 4);

        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = UIStyleHelper.Colors.Gold;
        borderImg.raycastTarget = false;

        // 金色发光效果
        GameObject glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(borderGO.transform, false);
        RectTransform glowRect = glowGO.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(0, -8);
        glowRect.offsetMax = new Vector2(0, 8);

        Image glowImg = glowGO.AddComponent<Image>();
        glowImg.color = new Color(1f, 0.8f, 0.3f, 0.15f);
        glowImg.raycastTarget = false;
    }

    /// <summary>
    /// 创建关卡卡片列表区域
    /// </summary>
    void CreateStageListArea(Transform parent)
    {
        // 滚动区域
        GameObject scrollGO = new GameObject("StageListScrollView");
        scrollGO.transform.SetParent(parent, false);
        RectTransform scrollRectT = scrollGO.AddComponent<RectTransform>();
        scrollRectT.anchorMin = new Vector2(0, 0);
        scrollRectT.anchorMax = new Vector2(1, 1);
        scrollRectT.offsetMin = new Vector2(0, 155);   // 底部信息栏上方
        scrollRectT.offsetMax = new Vector2(0, -145);   // 顶部标题栏下方

        ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 40f;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;

        // 视口
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0.01f);
        Mask mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        scroll.viewport = viewportRect;

        // 内容容器 - 使用VerticalLayoutGroup
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 12;
        vlg.padding = new RectOffset(20, 20, 20, 40);
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;

        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;

        // 获取存档数据
        int highestUnlocked = 1;
        if (SaveSystem.Instance != null)
        {
            highestUnlocked = SaveSystem.Instance.GetHighestUnlockedStage();
        }

        // 总关卡数
        int totalStages = 100;

        // 创建100个关卡卡片
        for (int i = 1; i <= totalStages; i++)
        {
            bool isCleared = false;
            if (SaveSystem.Instance != null)
            {
                isCleared = SaveSystem.Instance.IsStageCleared(i);
            }
            else
            {
                isCleared = i < highestUnlocked;
            }
            bool isUnlocked = i <= highestUnlocked;
            bool isCurrent = i == highestUnlocked;

            StageConfigEntry config = GetStageConfig(i);
            CreateStageCard(contentGO.transform, i, config, isCleared, isUnlocked, isCurrent);
        }

        // 自动滚动到当前关卡
        StartCoroutine(ScrollToCurrentStage(scroll, contentRect, highestUnlocked, totalStages));
    }

    /// <summary>
    /// 创建单个关卡卡片
    /// </summary>
    void CreateStageCard(Transform parent, int stageNum, StageConfigEntry config,
        bool isCleared, bool isUnlocked, bool isCurrent)
    {
        GameObject cardGO = new GameObject($"StageCard_{stageNum}");
        cardGO.transform.SetParent(parent, false);
        RectTransform cardRect = cardGO.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(920, 140);

        // 卡片背景
        Image cardBg = cardGO.AddComponent<Image>();
        if (isCleared)
            cardBg.color = new Color(0.25f, 0.18f, 0.12f, 0.9f); // 深棕色已通关
        else if (isUnlocked)
            cardBg.color = new Color(0.2f, 0.17f, 0.12f, 0.95f); // 稍亮棕色可进入
        else
            cardBg.color = new Color(0.15f, 0.15f, 0.18f, 0.7f); // 暗灰锁定

        // 按钮组件
        Button cardBtn = cardGO.AddComponent<Button>();
        cardBtn.targetGraphic = cardBg;
        cardBtn.interactable = isUnlocked;

        ColorBlock colors = cardBtn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.05f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f);
        cardBtn.colors = colors;

        int level = stageNum;
        cardBtn.onClick.AddListener(() =>
        {
            Debug.Log($"[StageSelect] 选择关卡 {level}");
            ShowStageConfirm(level, config);
        });

        // 金色边框（当前关卡高亮）
        if (isCurrent)
        {
            GameObject borderGO = new GameObject("GoldBorder");
            borderGO.transform.SetParent(cardGO.transform, false);
            RectTransform borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-3, -3);
            borderRect.offsetMax = new Vector2(3, 3);

            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(1f, 0.8f, 0.2f, 0.9f);
            borderImg.raycastTarget = false;
            borderGO.transform.SetAsFirstSibling();

            // 内层发光
            GameObject glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(cardGO.transform, false);
            RectTransform glowRect = glowGO.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-8, -8);
            glowRect.offsetMax = new Vector2(8, 8);

            Image glowImg = glowGO.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.15f);
            glowImg.raycastTarget = false;
            glowGO.transform.SetAsFirstSibling();
        }
        else if (isCleared)
        {
            // 通关卡片 - 细边框
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(cardGO.transform, false);
            RectTransform borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2, -2);
            borderRect.offsetMax = new Vector2(2, 2);

            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.5f, 0.4f, 0.25f, 0.6f);
            borderImg.raycastTarget = false;
            borderGO.transform.SetAsFirstSibling();
        }

        // 卡片内容
        if (isUnlocked)
        {
            CreateCardContent(cardGO.transform, stageNum, config, isCleared, isCurrent);
        }
        else
        {
            CreateLockedCardContent(cardGO.transform, stageNum, config);
        }

        // 难度标签（右上角）
        if (isUnlocked)
        {
            CreateDifficultyBadge(cardGO.transform, config.difficulty);
        }

        // "GO!" 标记（当前关卡）
        if (isCurrent)
        {
            CreateCurrentBadge(cardGO.transform);
        }
    }

    /// <summary>
    /// 创建解锁关卡的卡片内容
    /// </summary>
    void CreateCardContent(Transform parent, int stageNum, StageConfigEntry config,
        bool isCleared, bool isCurrent)
    {
        // 第1行: "关卡 N: 名称"
        GameObject nameGO = new GameObject("StageName");
        nameGO.transform.SetParent(parent, false);
        RectTransform nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.58f);
        nameRect.anchorMax = new Vector2(0.8f, 0.95f);
        nameRect.offsetMin = new Vector2(24, 0);
        nameRect.offsetMax = new Vector2(-10, 0);

        Text nameText = nameGO.AddComponent<Text>();
        string displayName = !string.IsNullOrEmpty(config.name) ? config.name : $"关卡{stageNum}";
        nameText.text = $"关卡 {stageNum}: {displayName}";
        nameText.fontSize = 30;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = isCurrent ? UIStyleHelper.Colors.Gold : new Color(0.95f, 0.9f, 0.8f);
        nameText.font = UIStyleHelper.GetDefaultFont();

        Outline nameOutline = nameGO.AddComponent<Outline>();
        nameOutline.effectColor = new Color(0, 0, 0, 0.5f);
        nameOutline.effectDistance = new Vector2(1, -1);

        // 第2行: 星级
        int starCount = 0;
        if (isCleared)
        {
            starCount = SaveSystem.Instance != null
                ? SaveSystem.Instance.GetStageStars(stageNum)
                : 1;
            if (starCount < 1) starCount = 1;
        }

        GameObject starsGO = new GameObject("Stars");
        starsGO.transform.SetParent(parent, false);
        RectTransform starsRect = starsGO.AddComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(0, 0.28f);
        starsRect.anchorMax = new Vector2(0.5f, 0.58f);
        starsRect.offsetMin = new Vector2(24, 0);
        starsRect.offsetMax = new Vector2(0, 0);

        Text starsText = starsGO.AddComponent<Text>();
        string stars = "";
        for (int i = 0; i < 5; i++)
        {
            stars += i < starCount ? "★" : "☆";
        }
        starsText.text = stars;
        starsText.fontSize = 26;
        starsText.alignment = TextAnchor.MiddleLeft;
        starsText.color = isCleared ? new Color(1f, 0.85f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);
        starsText.font = UIStyleHelper.GetDefaultFont();

        // 第3行: 推荐等级 + 波次
        GameObject infoGO = new GameObject("StageInfo");
        infoGO.transform.SetParent(parent, false);
        RectTransform infoRect = infoGO.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0.05f);
        infoRect.anchorMax = new Vector2(0.8f, 0.32f);
        infoRect.offsetMin = new Vector2(24, 0);
        infoRect.offsetMax = new Vector2(0, 0);

        Text infoText = infoGO.AddComponent<Text>();
        infoText.text = $"推荐等级: {config.recommendedLevel}    波次: {config.waveCount}";
        infoText.fontSize = 22;
        infoText.alignment = TextAnchor.MiddleLeft;
        infoText.color = new Color(0.65f, 0.6f, 0.55f);
        infoText.font = UIStyleHelper.GetDefaultFont();
    }

    /// <summary>
    /// 创建锁定关卡的卡片内容
    /// </summary>
    void CreateLockedCardContent(Transform parent, int stageNum, StageConfigEntry config)
    {
        // 关卡名（灰色）
        GameObject nameGO = new GameObject("StageName");
        nameGO.transform.SetParent(parent, false);
        RectTransform nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.45f);
        nameRect.anchorMax = new Vector2(0.75f, 0.95f);
        nameRect.offsetMin = new Vector2(24, 0);
        nameRect.offsetMax = new Vector2(-10, 0);

        Text nameText = nameGO.AddComponent<Text>();
        string displayName = !string.IsNullOrEmpty(config.name) ? config.name : $"关卡{stageNum}";
        nameText.text = $"关卡 {stageNum}: {displayName}";
        nameText.fontSize = 28;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = new Color(0.45f, 0.45f, 0.48f);
        nameText.font = UIStyleHelper.GetDefaultFont();

        // 锁定图标
        GameObject lockGO = new GameObject("LockIcon");
        lockGO.transform.SetParent(parent, false);
        RectTransform lockRect = lockGO.AddComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(1, 0.5f);
        lockRect.anchorMax = new Vector2(1, 0.5f);
        lockRect.anchoredPosition = new Vector2(-60, 0);
        lockRect.sizeDelta = new Vector2(50, 50);

        Text lockText = lockGO.AddComponent<Text>();
        lockText.text = "🔒";
        lockText.fontSize = 34;
        lockText.alignment = TextAnchor.MiddleCenter;
        lockText.font = UIStyleHelper.GetDefaultFont();

        // 推荐等级（灰色）
        GameObject infoGO = new GameObject("StageInfo");
        infoGO.transform.SetParent(parent, false);
        RectTransform infoRect = infoGO.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0.05f);
        infoRect.anchorMax = new Vector2(0.7f, 0.45f);
        infoRect.offsetMin = new Vector2(24, 0);
        infoRect.offsetMax = new Vector2(0, 0);

        Text infoText = infoGO.AddComponent<Text>();
        infoText.text = $"推荐等级: {config.recommendedLevel}    波次: {config.waveCount}";
        infoText.fontSize = 20;
        infoText.alignment = TextAnchor.MiddleLeft;
        infoText.color = new Color(0.4f, 0.4f, 0.42f);
        infoText.font = UIStyleHelper.GetDefaultFont();
    }

    /// <summary>
    /// 创建"GO!"标记（当前关卡）
    /// </summary>
    void CreateCurrentBadge(Transform parent)
    {
        GameObject badgeGO = new GameObject("CurrentBadge");
        badgeGO.transform.SetParent(parent, false);
        RectTransform badgeRect = badgeGO.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(1, 0.5f);
        badgeRect.anchorMax = new Vector2(1, 0.5f);
        badgeRect.anchoredPosition = new Vector2(-60, 0);
        badgeRect.sizeDelta = new Vector2(80, 50);

        Image badgeBg = badgeGO.AddComponent<Image>();
        badgeBg.color = new Color(1f, 0.75f, 0.15f, 1f);
        badgeBg.raycastTarget = false;

        // GO! 文字
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(badgeGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text goText = textGO.AddComponent<Text>();
        goText.text = "GO!";
        goText.fontSize = 26;
        goText.fontStyle = FontStyle.Bold;
        goText.alignment = TextAnchor.MiddleCenter;
        goText.color = new Color(0.25f, 0.15f, 0.05f);
        goText.font = UIStyleHelper.GetDefaultFont();

        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.3f);
        outline.effectDistance = new Vector2(1, -1);
    }

    /// <summary>
    /// 创建难度标签（右上角）
    /// </summary>
    void CreateDifficultyBadge(Transform parent, int difficulty)
    {
        GameObject badgeGO = new GameObject("DifficultyBadge");
        badgeGO.transform.SetParent(parent, false);
        RectTransform badgeRect = badgeGO.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(1, 1);
        badgeRect.anchorMax = new Vector2(1, 1);
        badgeRect.anchoredPosition = new Vector2(-16, -8);
        badgeRect.pivot = new Vector2(1, 1);
        badgeRect.sizeDelta = new Vector2(70, 28);

        Image badgeBg = badgeGO.AddComponent<Image>();
        // 颜色根据难度
        switch (difficulty)
        {
            case 1: badgeBg.color = new Color(0.3f, 0.6f, 0.3f, 0.9f); break; // 绿
            case 2: badgeBg.color = new Color(0.3f, 0.5f, 0.7f, 0.9f); break; // 蓝
            case 3: badgeBg.color = new Color(0.7f, 0.5f, 0.2f, 0.9f); break; // 橙
            case 4: badgeBg.color = new Color(0.7f, 0.25f, 0.25f, 0.9f); break; // 红
            case 5: badgeBg.color = new Color(0.6f, 0.2f, 0.6f, 0.9f); break; // 紫
            default: badgeBg.color = new Color(0.4f, 0.4f, 0.4f, 0.9f); break;
        }
        badgeBg.raycastTarget = false;

        // 难度文字
        string[] diffNames = { "", "简单", "普通", "困难", "噩梦", "地狱" };
        string diffName = difficulty >= 1 && difficulty <= 5 ? diffNames[difficulty] : "???";

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(badgeGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text diffText = textGO.AddComponent<Text>();
        diffText.text = diffName;
        diffText.fontSize = 18;
        diffText.fontStyle = FontStyle.Bold;
        diffText.alignment = TextAnchor.MiddleCenter;
        diffText.color = Color.white;
        diffText.font = UIStyleHelper.GetDefaultFont();
    }

    /// <summary>
    /// 自动滚动到当前关卡位置
    /// </summary>
    IEnumerator ScrollToCurrentStage(ScrollRect scrollRect, RectTransform content, int currentStage, int totalStages)
    {
        yield return null; // 等一帧让布局计算完成
        yield return null; // 再等一帧确保ContentSizeFitter生效

        if (currentStage <= 1) yield break;

        // 计算滚动位置
        float cardHeight = 140f;
        float spacing = 12f;
        float padding = 20f;
        float totalHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        if (totalHeight <= viewportHeight) yield break;

        // 目标关卡的y位置（从顶部开始算）
        float targetY = padding + (currentStage - 1) * (cardHeight + spacing);
        // 居中显示
        targetY -= viewportHeight / 2 - cardHeight / 2;
        targetY = Mathf.Clamp(targetY, 0, totalHeight - viewportHeight);

        float normalizedPos = 1f - (targetY / (totalHeight - viewportHeight));
        normalizedPos = Mathf.Clamp01(normalizedPos);

        scrollRect.verticalNormalizedPosition = normalizedPos;
    }

    void ShowStageConfirm(int stageNum, StageConfigEntry config)
    {
        // 设置当前关卡
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentLevel = stageNum;
        }

        string title = $"关卡 {stageNum}: {config.name}";
        string msg = $"推荐等级: {config.recommendedLevel}\n波次: {config.waveCount}\n\n是否进入此关卡？";

        // 使用确认弹窗
        if (ConfirmDialog.Instance != null)
        {
            ConfirmDialog.Instance.Show(
                title,
                msg,
                () => SceneManager.LoadScene("GameScene"),
                null
            );
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    /// <summary>
    /// 创建底部信息栏
    /// </summary>
    void CreateBottomInfoBar(Transform parent)
    {
        GameObject bottomGO = new GameObject("BottomInfoBar");
        bottomGO.transform.SetParent(parent, false);
        RectTransform bottomRect = bottomGO.AddComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(1, 0);
        bottomRect.pivot = new Vector2(0.5f, 0);
        bottomRect.anchoredPosition = Vector2.zero;
        bottomRect.sizeDelta = new Vector2(0, 150);

        // 背景
        Image bottomBg = bottomGO.AddComponent<Image>();
        bottomBg.color = new Color(0.05f, 0.08f, 0.12f, 0.92f);

        // 顶部金色边框
        CreateGoldBorderLine(bottomGO.transform, true);

        // 玩家信息 - 左侧
        CreatePlayerInfoSection(bottomGO.transform);

        // 进度 - 中间
        CreateProgressSection(bottomGO.transform);

        // 金币 - 右侧
        CreateGoldSection(bottomGO.transform);
    }

    void CreatePlayerInfoSection(Transform parent)
    {
        GameObject sectionGO = new GameObject("PlayerInfo");
        sectionGO.transform.SetParent(parent, false);
        RectTransform sectionRect = sectionGO.AddComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(0, 0.5f);
        sectionRect.anchorMax = new Vector2(0, 0.5f);
        sectionRect.anchoredPosition = new Vector2(120, 0);
        sectionRect.sizeDelta = new Vector2(180, 80);

        Image sectionBg = sectionGO.AddComponent<Image>();
        sectionBg.color = new Color(0.15f, 0.18f, 0.25f, 0.8f);

        int playerLevel = 1;
        if (SaveSystem.Instance?.CurrentPlayerStats != null)
        {
            playerLevel = SaveSystem.Instance.CurrentPlayerStats.level;
        }

        Text levelText = UIStyleHelper.CreateTitleText(sectionGO.transform, "Level",
            $"Lv.{playerLevel}", 34, new Color(0.5f, 0.8f, 1f));
    }

    void CreateProgressSection(Transform parent)
    {
        GameObject sectionGO = new GameObject("ProgressSection");
        sectionGO.transform.SetParent(parent, false);
        RectTransform sectionRect = sectionGO.AddComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(0.5f, 0.5f);
        sectionRect.anchorMax = new Vector2(0.5f, 0.5f);
        sectionRect.sizeDelta = new Vector2(300, 60);

        int clearedCount = 0;
        if (SaveSystem.Instance != null)
        {
            clearedCount = Mathf.Max(0, SaveSystem.Instance.GetHighestUnlockedStage() - 1);
        }

        Text progressText = UIStyleHelper.CreateText(sectionGO.transform, "Text",
            $"进度: {clearedCount}/100 关卡", 26, Color.white, TextAnchor.MiddleCenter);
    }

    void CreateGoldSection(Transform parent)
    {
        GameObject sectionGO = new GameObject("GoldSection");
        sectionGO.transform.SetParent(parent, false);
        RectTransform sectionRect = sectionGO.AddComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(1, 0.5f);
        sectionRect.anchorMax = new Vector2(1, 0.5f);
        sectionRect.anchoredPosition = new Vector2(-120, 0);
        sectionRect.sizeDelta = new Vector2(180, 70);

        Image sectionBg = sectionGO.AddComponent<Image>();
        sectionBg.color = new Color(0.25f, 0.2f, 0.12f, 0.8f);

        // 金币图标
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(sectionGO.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(30, 0);
        iconRect.sizeDelta = new Vector2(40, 40);

        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = UIStyleHelper.Colors.Gold;

        // 金币数量
        int gold = 0;
        if (SaveSystem.Instance?.CurrentPlayerStats != null)
        {
            gold = SaveSystem.Instance.CurrentPlayerStats.gold;
        }
        else if (GameManager.Instance != null)
        {
            gold = GameManager.Instance.PlayerGold;
        }

        GameObject textGO = new GameObject("Amount");
        textGO.transform.SetParent(sectionGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.35f, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text goldText = textGO.AddComponent<Text>();
        goldText.text = FormatNumber(gold);
        goldText.fontSize = 28;
        goldText.fontStyle = FontStyle.Bold;
        goldText.alignment = TextAnchor.MiddleCenter;
        goldText.color = UIStyleHelper.Colors.Gold;
        goldText.font = UIStyleHelper.GetDefaultFont();

        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.6f);
        outline.effectDistance = new Vector2(1, -1);
    }

    string FormatNumber(int number)
    {
        if (number >= 1000000)
            return (number / 1000000f).ToString("0.#") + "M";
        if (number >= 1000)
            return (number / 1000f).ToString("0.#") + "K";
        return number.ToString();
    }
}
