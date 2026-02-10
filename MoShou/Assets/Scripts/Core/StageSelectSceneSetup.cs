using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using MoShou.Systems;
using MoShou.UI;

/// <summary>
/// 选关场景初始化 - 自动创建选关UI
/// 依据策划案: 竖屏 1080x1920
/// 基于效果图: UI_StageSelect.png
/// 风格: 卡通魔兽风格，地图路径布局，金属边框装饰
/// </summary>
public class StageSelectSceneSetup : MonoBehaviour
{
    private static bool isInitialized = false;

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
        Debug.Log("[StageSelectSetup] 开始创建选关UI（效果图风格）...");

        // 确保有EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 销毁所有现有的低优先级Canvas，强制创建新UI
        // 使用DestroyImmediate确保旧UI立即被移除，避免新旧UI叠加
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in existingCanvases)
        {
            // 只销毁sortingOrder < 500的Canvas（保留系统级Canvas如ConfirmDialog等）
            if (canvas.sortingOrder < 500)
            {
                Debug.Log($"[StageSelectSetup] 立即销毁现有Canvas: {canvas.gameObject.name}");
                DestroyImmediate(canvas.gameObject);
            }
        }

        // 销毁旧的StageSelectManager（如果存在）
        var oldManager = FindObjectOfType<MoShou.Core.StageSelectManager>();
        if (oldManager != null)
        {
            Debug.Log("[StageSelectSetup] 销毁旧的StageSelectManager");
            Destroy(oldManager.gameObject);
        }

        // 创建新的效果图风格UI
        CreateStageSelectUI();
    }

    void CreateStageSelectUI()
    {
        // 尝试加载效果图作为背景
        Sprite mockupBg = Resources.Load<Sprite>("UI_Mockups/Screens/UI_StageSelect");

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
        CreateBackgroundLayer(canvasGO.transform, mockupBg);

        // === 顶部标题栏 ===
        CreateTopTitleBar(canvasGO.transform);

        // === 章节标签页 ===
        CreateChapterTabs(canvasGO.transform);

        // === 地图路径关卡 ===
        CreateMapPathArea(canvasGO.transform);

        // === 底部信息栏 ===
        CreateBottomInfoBar(canvasGO.transform);

        Debug.Log("[StageSelectSetup] 选关UI创建完成（效果图风格）");
    }

    /// <summary>
    /// 创建背景层 - 效果图风格：深色幻想地图背景
    /// </summary>
    void CreateBackgroundLayer(Transform parent, Sprite mockupBg)
    {
        // 主背景
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
            Debug.Log("[StageSelectSetup] 效果图背景加载成功");
        }
        else
        {
            // 创建深色幻想地图风格背景
            bgImage.color = new Color(0.08f, 0.12f, 0.18f);

            // 渐变层 - 底部更暗
            GameObject gradientGO = new GameObject("Gradient");
            gradientGO.transform.SetParent(bgGO.transform, false);
            RectTransform gradRect = gradientGO.AddComponent<RectTransform>();
            gradRect.anchorMin = Vector2.zero;
            gradRect.anchorMax = new Vector2(1, 0.5f);
            gradRect.offsetMin = Vector2.zero;
            gradRect.offsetMax = Vector2.zero;

            Image gradImg = gradientGO.AddComponent<Image>();
            gradImg.color = new Color(0.03f, 0.05f, 0.08f, 0.7f);

            // 地图纹理效果 - 多层叠加
            CreateMapTexture(bgGO.transform);
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

    void CreateMapTexture(Transform parent)
    {
        // 模拟地图格子纹理
        for (int i = 0; i < 8; i++)
        {
            GameObject lineGO = new GameObject($"MapLine_{i}");
            lineGO.transform.SetParent(parent, false);
            RectTransform lineRect = lineGO.AddComponent<RectTransform>();

            bool isHorizontal = i < 4;
            if (isHorizontal)
            {
                lineRect.anchorMin = new Vector2(0, 0.2f + i * 0.2f);
                lineRect.anchorMax = new Vector2(1, 0.2f + i * 0.2f);
                lineRect.sizeDelta = new Vector2(0, 1);
            }
            else
            {
                int col = i - 4;
                lineRect.anchorMin = new Vector2(0.15f + col * 0.25f, 0);
                lineRect.anchorMax = new Vector2(0.15f + col * 0.25f, 1);
                lineRect.sizeDelta = new Vector2(1, 0);
            }

            Image lineImg = lineGO.AddComponent<Image>();
            lineImg.color = new Color(0.15f, 0.2f, 0.28f, 0.3f);
            lineImg.raycastTarget = false;
        }
    }

    /// <summary>
    /// 创建顶部标题栏 - 效果图风格：金属装饰边框
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
        topRect.sizeDelta = new Vector2(0, 200);

        // 半透明背景
        Image topBg = topBarGO.AddComponent<Image>();
        topBg.color = new Color(0.05f, 0.08f, 0.12f, 0.9f);

        // 金色底部边框线
        CreateGoldBorderLine(topBarGO.transform, false);

        // 返回按钮 - 左侧
        CreateBackButton(topBarGO.transform);

        // 章节标题 - 中央
        CreateChapterTitleBanner(topBarGO.transform);

        // 设置按钮 - 右侧
        CreateSettingsButton(topBarGO.transform);
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

    void CreateBackButton(Transform parent)
    {
        GameObject btnGO = new GameObject("BackButton");
        btnGO.transform.SetParent(parent, false);
        RectTransform btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0.5f);
        btnRect.anchorMax = new Vector2(0, 0.5f);
        btnRect.anchoredPosition = new Vector2(80, -10);
        btnRect.sizeDelta = new Vector2(100, 80);

        // 按钮背景 - 半透明圆角
        Image btnBg = btnGO.AddComponent<Image>();
        btnBg.color = new Color(0.2f, 0.25f, 0.35f, 0.85f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnBg;

        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.1f, 1.0f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            Debug.Log("[StageSelect] 返回主菜单");
            // 播放点击反馈
            if (UIFeedbackSystem.Instance != null)
                UIFeedbackSystem.Instance.PlayButtonClick(btnGO.transform);
            SceneManager.LoadScene("MainMenu");
        });

        // 返回箭头图标
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(btnGO.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Text iconText = iconGO.AddComponent<Text>();
        iconText.text = "◀";
        iconText.fontSize = 42;
        iconText.alignment = TextAnchor.MiddleCenter;
        iconText.color = UIStyleHelper.Colors.Gold;
        iconText.font = UIStyleHelper.GetDefaultFont();

        // 边框
        Outline outline = iconGO.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.25f, 0.1f);
        outline.effectDistance = new Vector2(2, -2);
    }

    void CreateChapterTitleBanner(Transform parent)
    {
        GameObject bannerGO = new GameObject("ChapterBanner");
        bannerGO.transform.SetParent(parent, false);
        RectTransform bannerRect = bannerGO.AddComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
        bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRect.anchoredPosition = new Vector2(0, -10);
        bannerRect.sizeDelta = new Vector2(500, 90);

        // 横幅背景 - 木纹/皮革效果
        Image bannerBg = bannerGO.AddComponent<Image>();
        bannerBg.color = new Color(0.25f, 0.18f, 0.12f, 0.95f);

        // 金色边框
        GameObject borderGO = new GameObject("Border");
        borderGO.transform.SetParent(bannerGO.transform, false);
        RectTransform borderRect = borderGO.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-4, -4);
        borderRect.offsetMax = new Vector2(4, 4);

        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = UIStyleHelper.Colors.GoldDark;
        borderImg.raycastTarget = false;
        borderGO.transform.SetAsFirstSibling();

        // 内层高光
        GameObject innerGO = new GameObject("Inner");
        innerGO.transform.SetParent(bannerGO.transform, false);
        RectTransform innerRect = innerGO.AddComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0, 0.7f);
        innerRect.anchorMax = new Vector2(1, 1);
        innerRect.offsetMin = new Vector2(5, 0);
        innerRect.offsetMax = new Vector2(-5, -3);

        Image innerImg = innerGO.AddComponent<Image>();
        innerImg.color = new Color(1, 1, 1, 0.08f);
        innerImg.raycastTarget = false;

        // 章节标题文字
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(bannerGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "第一章 · 新手村";
        titleText.fontSize = 40;
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

        // 左右装饰
        CreateBannerDecoration(bannerGO.transform, true);
        CreateBannerDecoration(bannerGO.transform, false);
    }

    void CreateBannerDecoration(Transform parent, bool isLeft)
    {
        GameObject decoGO = new GameObject(isLeft ? "LeftDeco" : "RightDeco");
        decoGO.transform.SetParent(parent, false);
        RectTransform decoRect = decoGO.AddComponent<RectTransform>();
        decoRect.anchorMin = new Vector2(isLeft ? 0 : 1, 0.5f);
        decoRect.anchorMax = new Vector2(isLeft ? 0 : 1, 0.5f);
        decoRect.anchoredPosition = new Vector2(isLeft ? -30 : 30, 0);
        decoRect.sizeDelta = new Vector2(40, 60);

        Text decoText = decoGO.AddComponent<Text>();
        decoText.text = isLeft ? "◆" : "◆";
        decoText.fontSize = 30;
        decoText.alignment = TextAnchor.MiddleCenter;
        decoText.color = UIStyleHelper.Colors.Gold;
        decoText.font = UIStyleHelper.GetDefaultFont();
    }

    void CreateSettingsButton(Transform parent)
    {
        GameObject btnGO = new GameObject("SettingsButton");
        btnGO.transform.SetParent(parent, false);
        RectTransform btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0.5f);
        btnRect.anchorMax = new Vector2(1, 0.5f);
        btnRect.anchoredPosition = new Vector2(-80, -10);
        btnRect.sizeDelta = new Vector2(80, 80);

        Image btnBg = btnGO.AddComponent<Image>();
        btnBg.color = new Color(0.2f, 0.25f, 0.35f, 0.85f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        btn.onClick.AddListener(() =>
        {
            if (SettingsPanel.Instance != null)
                SettingsPanel.Instance.Show();
        });

        // 齿轮图标
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(btnGO.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Text iconText = iconGO.AddComponent<Text>();
        iconText.text = "⚙";
        iconText.fontSize = 38;
        iconText.alignment = TextAnchor.MiddleCenter;
        iconText.color = Color.white;
        iconText.font = UIStyleHelper.GetDefaultFont();
    }

    /// <summary>
    /// 创建章节标签页
    /// </summary>
    void CreateChapterTabs(Transform parent)
    {
        // Notion UI_003规范: 章节标签区域
        // anchorMin:[0.05, 0.82], anchorMax:[0.95, 0.88]
        GameObject tabsGO = new GameObject("ChapterTabs");
        tabsGO.transform.SetParent(parent, false);
        RectTransform tabsRect = tabsGO.AddComponent<RectTransform>();
        tabsRect.anchorMin = new Vector2(0.05f, 0.82f);
        tabsRect.anchorMax = new Vector2(0.95f, 0.88f);
        tabsRect.offsetMin = Vector2.zero;
        tabsRect.offsetMax = Vector2.zero;

        // 标签背景 - 半透明深色
        Image tabsBg = tabsGO.AddComponent<Image>();
        tabsBg.color = new Color(0.08f, 0.1f, 0.15f, 0.85f);

        // 水平布局 - Notion规范: spacing=16px, padding=8px
        HorizontalLayoutGroup hlg = tabsGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 16;
        hlg.padding = new RectOffset(16, 16, 8, 8);
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        // 创建4个章节标签
        string[] chapters = { "第一章", "第二章", "第三章", "第四章" };
        int[] chapterFirstStage = { 1, 11, 21, 31 }; // 每章第一关的关卡ID

        // 从SaveSystem获取最高解锁关卡
        int highestUnlockedStage = MoShou.Systems.SaveSystem.Instance != null
            ? MoShou.Systems.SaveSystem.Instance.GetHighestUnlockedStage()
            : 1;

        // 动态计算章节解锁状态
        bool[] unlocked = new bool[chapters.Length];
        for (int i = 0; i < chapters.Length; i++)
        {
            // 章节解锁条件：最高解锁关卡 >= 该章节第一关
            unlocked[i] = highestUnlockedStage >= chapterFirstStage[i];
        }

        Debug.Log($"[StageSelectSetup] 最高解锁关卡: {highestUnlockedStage}, 章节解锁: [{string.Join(", ", unlocked)}]");

        for (int i = 0; i < chapters.Length; i++)
        {
            CreateChapterTab(tabsGO.transform, chapters[i], i + 1, i == 0, unlocked[i]);
        }
    }

    void CreateChapterTab(Transform parent, string name, int chapter, bool isSelected, bool isUnlocked)
    {
        GameObject tabGO = new GameObject($"Tab_{chapter}");
        tabGO.transform.SetParent(parent, false);

        Image tabBg = tabGO.AddComponent<Image>();
        if (isSelected)
            tabBg.color = new Color(0.8f, 0.6f, 0.2f, 0.9f);
        else if (isUnlocked)
            tabBg.color = new Color(0.2f, 0.25f, 0.35f, 0.8f);
        else
            tabBg.color = new Color(0.15f, 0.15f, 0.2f, 0.6f);

        Button tabBtn = tabGO.AddComponent<Button>();
        tabBtn.targetGraphic = tabBg;
        tabBtn.interactable = isUnlocked;

        int chapterNum = chapter;
        tabBtn.onClick.AddListener(() =>
        {
            Debug.Log($"[StageSelect] 切换到章节 {chapterNum}");
            // TODO: 切换章节逻辑
        });

        // 标签文字
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(tabGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text tabText = textGO.AddComponent<Text>();
        tabText.text = name;
        tabText.fontSize = 26;
        tabText.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
        tabText.alignment = TextAnchor.MiddleCenter;
        tabText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        tabText.font = UIStyleHelper.GetDefaultFont();

        if (isSelected)
        {
            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.25f, 0.1f);
            outline.effectDistance = new Vector2(1, -1);
        }

        // 锁定图标
        if (!isUnlocked)
        {
            GameObject lockGO = new GameObject("Lock");
            lockGO.transform.SetParent(tabGO.transform, false);
            RectTransform lockRect = lockGO.AddComponent<RectTransform>();
            lockRect.anchorMin = new Vector2(1, 1);
            lockRect.anchorMax = new Vector2(1, 1);
            lockRect.anchoredPosition = new Vector2(-5, -5);
            lockRect.sizeDelta = new Vector2(25, 25);

            Text lockText = lockGO.AddComponent<Text>();
            lockText.text = "🔒";
            lockText.fontSize = 18;
            lockText.alignment = TextAnchor.MiddleCenter;
            lockText.font = UIStyleHelper.GetDefaultFont();
        }
    }

    /// <summary>
    /// 创建地图路径区域 - 效果图风格：节点连线布局
    /// </summary>
    void CreateMapPathArea(Transform parent)
    {
        // 滚动区域
        GameObject scrollGO = new GameObject("MapScrollView");
        scrollGO.transform.SetParent(parent, false);
        RectTransform scrollRect = scrollGO.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(30, 180);
        scrollRect.offsetMax = new Vector2(-30, -280);

        ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30f;

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

        // 内容容器
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1);
        contentRect.anchorMax = new Vector2(0.5f, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(1000, 1600);

        scroll.content = contentRect;

        // 获取存档数据
        int clearedCount = 3;
        int highestUnlocked = 4;
        if (SaveSystem.Instance != null)
        {
            highestUnlocked = SaveSystem.Instance.GetHighestUnlockedStage();
            clearedCount = highestUnlocked - 1;
        }

        // 创建路径连接线
        CreatePathLines(contentGO.transform, 12);

        // 创建关卡节点 - Z字形布局
        CreateStageNodes(contentGO.transform, 12, clearedCount, highestUnlocked);
    }

    void CreatePathLines(Transform parent, int nodeCount)
    {
        // 创建节点之间的连线
        GameObject linesGO = new GameObject("PathLines");
        linesGO.transform.SetParent(parent, false);
        RectTransform linesRect = linesGO.AddComponent<RectTransform>();
        linesRect.anchorMin = Vector2.zero;
        linesRect.anchorMax = Vector2.one;
        linesRect.offsetMin = Vector2.zero;
        linesRect.offsetMax = Vector2.zero;

        // 使用简化的垂直路径
        float startY = -100;
        float spacing = 120;

        for (int i = 0; i < nodeCount - 1; i++)
        {
            float y1 = startY - i * spacing;
            float y2 = startY - (i + 1) * spacing;

            // Z字形偏移
            float x1 = (i % 2 == 0) ? -100 : 100;
            float x2 = ((i + 1) % 2 == 0) ? -100 : 100;

            // 垂直线段
            CreateLineBetween(linesGO.transform, new Vector2(x1, y1), new Vector2(x2, y2), i);
        }
    }

    void CreateLineBetween(Transform parent, Vector2 start, Vector2 end, int index)
    {
        GameObject lineGO = new GameObject($"Line_{index}");
        lineGO.transform.SetParent(parent, false);
        RectTransform lineRect = lineGO.AddComponent<RectTransform>();

        Vector2 center = (start + end) / 2;
        float distance = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

        lineRect.anchorMin = new Vector2(0.5f, 1);
        lineRect.anchorMax = new Vector2(0.5f, 1);
        lineRect.anchoredPosition = center;
        lineRect.sizeDelta = new Vector2(distance, 8);
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);

        Image lineImg = lineGO.AddComponent<Image>();
        lineImg.color = new Color(0.5f, 0.4f, 0.25f, 0.6f);
        lineImg.raycastTarget = false;

        // 发光效果
        GameObject glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(lineGO.transform, false);
        RectTransform glowRect = glowGO.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(0, -4);
        glowRect.offsetMax = new Vector2(0, 4);

        Image glowImg = glowGO.AddComponent<Image>();
        glowImg.color = new Color(0.8f, 0.6f, 0.3f, 0.15f);
        glowImg.raycastTarget = false;
    }

    void CreateStageNodes(Transform parent, int nodeCount, int clearedCount, int highestUnlocked)
    {
        float startY = -100;
        float spacing = 120;

        for (int i = 0; i < nodeCount; i++)
        {
            int stageNum = i + 1;
            bool isCleared = stageNum <= clearedCount;
            bool isUnlocked = stageNum <= highestUnlocked;
            bool isCurrent = stageNum == highestUnlocked;

            // Z字形布局
            float xOffset = (i % 2 == 0) ? -100 : 100;
            float yPos = startY - i * spacing;

            CreateStageNode(parent, stageNum, xOffset, yPos, isCleared, isUnlocked, isCurrent);
        }
    }

    void CreateStageNode(Transform parent, int stageNum, float xPos, float yPos,
        bool isCleared, bool isUnlocked, bool isCurrent)
    {
        GameObject nodeGO = new GameObject($"Stage_{stageNum}");
        nodeGO.transform.SetParent(parent, false);
        RectTransform nodeRect = nodeGO.AddComponent<RectTransform>();
        nodeRect.anchorMin = new Vector2(0.5f, 1);
        nodeRect.anchorMax = new Vector2(0.5f, 1);
        nodeRect.anchoredPosition = new Vector2(xPos, yPos);
        nodeRect.sizeDelta = new Vector2(160, 160);

        // 节点背景 - 圆形/六边形效果
        Image nodeBg = nodeGO.AddComponent<Image>();
        if (isCleared)
            nodeBg.color = new Color(0.2f, 0.5f, 0.3f, 0.95f); // 绿色已通关
        else if (isUnlocked)
            nodeBg.color = new Color(0.3f, 0.35f, 0.5f, 0.95f); // 蓝色可进入
        else
            nodeBg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f); // 灰色锁定

        Button nodeBtn = nodeGO.AddComponent<Button>();
        nodeBtn.targetGraphic = nodeBg;
        nodeBtn.interactable = isUnlocked;

        ColorBlock colors = nodeBtn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
        nodeBtn.colors = colors;

        int level = stageNum;
        nodeBtn.onClick.AddListener(() =>
        {
            Debug.Log($"[StageSelect] 选择关卡 {level}");
            ShowStageConfirm(level);
        });

        // 边框
        CreateNodeBorder(nodeGO.transform, isCleared, isUnlocked, isCurrent);

        // 关卡图标/数字
        CreateNodeIcon(nodeGO.transform, stageNum, isUnlocked);

        // 关卡名称
        CreateNodeName(nodeGO.transform, stageNum, isUnlocked);

        // 星星评级
        if (isCleared)
        {
            int starCount = Random.Range(1, 4);
            CreateNodeStars(nodeGO.transform, starCount);
        }

        // 锁定图标
        if (!isUnlocked)
        {
            CreateLockIcon(nodeGO.transform);
        }

        // 当前关卡指示器
        if (isCurrent)
        {
            CreateCurrentIndicator(nodeGO.transform);
        }
    }

    void CreateNodeBorder(Transform parent, bool isCleared, bool isUnlocked, bool isCurrent)
    {
        GameObject borderGO = new GameObject("Border");
        borderGO.transform.SetParent(parent, false);
        RectTransform borderRect = borderGO.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-5, -5);
        borderRect.offsetMax = new Vector2(5, 5);

        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.raycastTarget = false;

        if (isCurrent)
            borderImg.color = new Color(1f, 0.8f, 0.2f, 1f); // 金色当前
        else if (isCleared)
            borderImg.color = new Color(0.4f, 0.7f, 0.4f, 0.8f); // 绿色通关
        else if (isUnlocked)
            borderImg.color = new Color(0.5f, 0.6f, 0.8f, 0.6f); // 蓝色解锁
        else
            borderImg.color = new Color(0.3f, 0.3f, 0.35f, 0.5f); // 灰色锁定

        borderGO.transform.SetAsFirstSibling();
    }

    void CreateNodeIcon(Transform parent, int stageNum, bool isUnlocked)
    {
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(parent, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.55f);
        iconRect.anchorMax = new Vector2(0.5f, 0.55f);
        iconRect.sizeDelta = new Vector2(80, 70);

        Text iconText = iconGO.AddComponent<Text>();
        iconText.text = stageNum.ToString();
        iconText.fontSize = 48;
        iconText.fontStyle = FontStyle.Bold;
        iconText.alignment = TextAnchor.MiddleCenter;
        iconText.color = isUnlocked ? UIStyleHelper.Colors.Gold : new Color(0.4f, 0.4f, 0.4f);
        iconText.font = UIStyleHelper.GetDefaultFont();

        if (isUnlocked)
        {
            Outline outline = iconGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.2f, 0.1f);
            outline.effectDistance = new Vector2(2, -2);

            Shadow shadow = iconGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);
        }
    }

    void CreateNodeName(Transform parent, int stageNum, bool isUnlocked)
    {
        GameObject nameGO = new GameObject("Name");
        nameGO.transform.SetParent(parent, false);
        RectTransform nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.15f);
        nameRect.anchorMax = new Vector2(1, 0.35f);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, 0);

        Text nameText = nameGO.AddComponent<Text>();
        nameText.text = $"关卡 1-{stageNum}";
        nameText.fontSize = 20;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        nameText.font = UIStyleHelper.GetDefaultFont();

        if (isUnlocked)
        {
            Outline outline = nameGO.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.6f);
            outline.effectDistance = new Vector2(1, -1);
        }
    }

    void CreateNodeStars(Transform parent, int starCount)
    {
        GameObject starsGO = UIStyleHelper.CreateStarRating(parent, "Stars", starCount, 3,
            new Vector2(24, 24));

        RectTransform starsRect = starsGO.GetComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(0.5f, 0);
        starsRect.anchorMax = new Vector2(0.5f, 0);
        starsRect.anchoredPosition = new Vector2(0, 25);
        starsRect.sizeDelta = new Vector2(90, 30);
    }

    void CreateLockIcon(Transform parent)
    {
        GameObject lockGO = new GameObject("LockIcon");
        lockGO.transform.SetParent(parent, false);
        RectTransform lockRect = lockGO.AddComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.sizeDelta = new Vector2(60, 60);

        // 半透明遮罩
        Image lockBg = lockGO.AddComponent<Image>();
        lockBg.color = new Color(0, 0, 0, 0.5f);
        lockBg.raycastTarget = false;

        // 锁图标
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(lockGO.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Text iconText = iconGO.AddComponent<Text>();
        iconText.text = "🔒";
        iconText.fontSize = 36;
        iconText.alignment = TextAnchor.MiddleCenter;
        iconText.font = UIStyleHelper.GetDefaultFont();
    }

    void CreateCurrentIndicator(Transform parent)
    {
        // 当前关卡闪烁指示器
        GameObject indicatorGO = new GameObject("CurrentIndicator");
        indicatorGO.transform.SetParent(parent, false);
        RectTransform indRect = indicatorGO.AddComponent<RectTransform>();
        indRect.anchorMin = new Vector2(0.5f, 1);
        indRect.anchorMax = new Vector2(0.5f, 1);
        indRect.anchoredPosition = new Vector2(0, 20);
        indRect.sizeDelta = new Vector2(80, 30);

        Image indBg = indicatorGO.AddComponent<Image>();
        indBg.color = new Color(1f, 0.8f, 0.2f, 0.9f);
        indBg.raycastTarget = false;

        Text indText = indicatorGO.AddComponent<Text>();
        if (indText == null)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(indicatorGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textGO.AddComponent<Text>();
            text.text = "NEW!";
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.3f, 0.2f, 0.1f);
            text.font = UIStyleHelper.GetDefaultFont();
        }
    }

    void ShowStageConfirm(int stageNum)
    {
        // 设置当前关卡并进入游戏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentLevel = stageNum;
        }

        // 使用确认弹窗
        if (ConfirmDialog.Instance != null)
        {
            ConfirmDialog.Instance.Show(
                $"关卡 1-{stageNum}",
                "是否进入此关卡？",
                () => SceneManager.LoadScene("GameScene"),
                null
            );
        }
        else
        {
            // 直接进入
            SceneManager.LoadScene("GameScene");
        }
    }

    /// <summary>
    /// 创建底部信息栏
    /// </summary>
    void CreateBottomInfoBar(Transform parent)
    {
        // Notion UI_003规范: 底部信息栏
        // anchorMin:[0, 0], anchorMax:[1, 0.08]
        GameObject bottomGO = new GameObject("BottomInfoBar");
        bottomGO.transform.SetParent(parent, false);
        RectTransform bottomRect = bottomGO.AddComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(1, 0.08f);
        bottomRect.offsetMin = Vector2.zero;
        bottomRect.offsetMax = Vector2.zero;

        // 背景 - 深色半透明
        Image bottomBg = bottomGO.AddComponent<Image>();
        bottomBg.color = new Color(0.05f, 0.08f, 0.12f, 0.92f);

        // 顶部金色边框
        CreateGoldBorderLine(bottomGO.transform, true);

        // 玩家信息 - 左侧
        CreatePlayerInfoSection(bottomGO.transform);

        // 章节进度 - 中间
        CreateChapterProgressSection(bottomGO.transform);

        // 金币显示 - 右侧
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

        // 背景
        Image sectionBg = sectionGO.AddComponent<Image>();
        sectionBg.color = new Color(0.15f, 0.18f, 0.25f, 0.8f);

        // 等级文字
        int playerLevel = 1;
        if (SaveSystem.Instance?.CurrentPlayerStats != null)
        {
            playerLevel = SaveSystem.Instance.CurrentPlayerStats.level;
        }

        Text levelText = UIStyleHelper.CreateTitleText(sectionGO.transform, "Level",
            $"Lv.{playerLevel}", 34, new Color(0.5f, 0.8f, 1f));
    }

    void CreateChapterProgressSection(Transform parent)
    {
        GameObject sectionGO = new GameObject("ChapterProgress");
        sectionGO.transform.SetParent(parent, false);
        RectTransform sectionRect = sectionGO.AddComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(0.5f, 0.5f);
        sectionRect.anchorMax = new Vector2(0.5f, 0.5f);
        sectionRect.sizeDelta = new Vector2(300, 60);

        int clearedCount = 3;
        if (SaveSystem.Instance != null)
        {
            clearedCount = Mathf.Max(0, SaveSystem.Instance.GetHighestUnlockedStage() - 1);
        }

        Text progressText = UIStyleHelper.CreateText(sectionGO.transform, "Text",
            $"进度: {clearedCount}/12 关卡", 26, Color.white, TextAnchor.MiddleCenter);
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

        // 背景
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
