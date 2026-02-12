using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor工具：一键生成选关界面所需的两个Prefab
/// 菜单: MoShou/创建选关Prefab
/// </summary>
public class StageSelectPrefabCreator
{
    [MenuItem("MoShou/创建选关Prefab/0. 全部生成")]
    public static void CreatePrefabs()
    {
        CreateStageCardPrefab();
        CreateStageSelectCanvasPrefab();
        AssetDatabase.Refresh();
        Debug.Log("[PrefabCreator] 选关Prefab创建完成！请在Project面板查看 Assets/Resources/Prefabs/UI/");
    }

    [MenuItem("MoShou/创建选关Prefab/1. StageCard卡片")]
    public static void CreateStageCardPrefab()
    {
        // 确保目录存在
        EnsureDirectory("Assets/Resources/Prefabs/UI");

        // 加载sprite资源
        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Frame_Gold_9slice.png");
        Sprite starFilled = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Result/UI_Result_Star_Filled.png");
        Sprite starEmpty = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Result/UI_Result_Star_Empty.png");
        Font defaultFont = GetFont();

        // === Root: StageCard ===
        GameObject root = new GameObject("StageCard");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(920, 140);

        // 背景Image (9-slice金色帧)
        Image cardBg = root.AddComponent<Image>();
        if (frameSprite != null)
        {
            cardBg.sprite = frameSprite;
            cardBg.type = Image.Type.Sliced;
        }
        cardBg.color = Color.white;

        // Button组件
        Button cardBtn = root.AddComponent<Button>();
        cardBtn.targetGraphic = cardBg;
        ColorBlock cb = cardBtn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.1f, 1.1f, 1.05f);
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        cb.disabledColor = new Color(0.7f, 0.7f, 0.7f);
        cardBtn.colors = cb;

        // === Thumbnail (左侧缩略图 100x100) ===
        GameObject thumbGO = CreateChild(root, "Thumbnail");
        RectTransform thumbRect = thumbGO.GetComponent<RectTransform>();
        thumbRect.anchorMin = new Vector2(0, 0.5f);
        thumbRect.anchorMax = new Vector2(0, 0.5f);
        thumbRect.anchoredPosition = new Vector2(65, 0);
        thumbRect.sizeDelta = new Vector2(100, 100);

        Image thumbImg = thumbGO.AddComponent<Image>();
        thumbImg.color = new Color(0.3f, 0.4f, 0.3f, 0.8f);
        thumbImg.raycastTarget = false;

        // === StageName (左上文字) ===
        GameObject nameGO = CreateChild(root, "StageName");
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.55f);
        nameRect.anchorMax = new Vector2(0.7f, 0.95f);
        nameRect.offsetMin = new Vector2(130, 0);
        nameRect.offsetMax = Vector2.zero;

        Text nameText = nameGO.AddComponent<Text>();
        nameText.text = "关卡 1: 示例关卡";
        nameText.fontSize = 26;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = new Color(0.95f, 0.9f, 0.8f);
        nameText.font = defaultFont;

        Outline nameOutline = nameGO.AddComponent<Outline>();
        nameOutline.effectColor = new Color(0, 0, 0, 0.5f);
        nameOutline.effectDistance = new Vector2(1, -1);

        // === StageInfo (左下文字) ===
        GameObject infoGO = CreateChild(root, "StageInfo");
        RectTransform infoRect = infoGO.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0.08f);
        infoRect.anchorMax = new Vector2(0.7f, 0.5f);
        infoRect.offsetMin = new Vector2(130, 0);
        infoRect.offsetMax = Vector2.zero;

        Text infoText = infoGO.AddComponent<Text>();
        infoText.text = "推荐等级: Lv.1    波数: 3";
        infoText.fontSize = 18;
        infoText.alignment = TextAnchor.MiddleLeft;
        infoText.color = new Color(0.6f, 0.58f, 0.52f);
        infoText.font = defaultFont;

        // === Stars (右侧星级, 默认隐藏) ===
        GameObject starsGO = CreateChild(root, "Stars");
        RectTransform starsRect = starsGO.GetComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(1, 0.5f);
        starsRect.anchorMax = new Vector2(1, 0.5f);
        starsRect.anchoredPosition = new Vector2(-70, 0);
        starsRect.sizeDelta = new Vector2(80, 24);

        HorizontalLayoutGroup hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 2;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        Image[] starImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject starObj = CreateChild(starsGO, $"Star_{i}");
            Image starImg = starObj.AddComponent<Image>();
            starImg.sprite = (i < 2) ? starFilled : starEmpty;
            starImg.color = (i < 2) ? new Color(1f, 1f, 0.9f) : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            starImg.raycastTarget = false;
            starImgs[i] = starImg;

            LayoutElement le = starObj.AddComponent<LayoutElement>();
            le.preferredWidth = 22;
            le.preferredHeight = 22;
        }

        starsGO.SetActive(false); // 默认隐藏

        // === GoButton (激活按钮, 默认隐藏) ===
        GameObject goBtnGO = CreateChild(root, "GoButton");
        RectTransform goRect = goBtnGO.GetComponent<RectTransform>();
        goRect.anchorMin = new Vector2(1, 0.5f);
        goRect.anchorMax = new Vector2(1, 0.5f);
        goRect.anchoredPosition = new Vector2(-55, 0);
        goRect.sizeDelta = new Vector2(95, 44);

        Image goBg = goBtnGO.AddComponent<Image>();
        goBg.color = new Color(0.2f, 0.65f, 0.35f, 1f);

        Button goBtn = goBtnGO.AddComponent<Button>();
        goBtn.targetGraphic = goBg;

        GameObject goTextGO = CreateChild(goBtnGO, "Text");
        RectTransform goTextRect = goTextGO.GetComponent<RectTransform>();
        goTextRect.anchorMin = Vector2.zero;
        goTextRect.anchorMax = Vector2.one;
        goTextRect.offsetMin = Vector2.zero;
        goTextRect.offsetMax = Vector2.zero;

        Text goText = goTextGO.AddComponent<Text>();
        goText.text = "激活";
        goText.fontSize = 22;
        goText.fontStyle = FontStyle.Bold;
        goText.alignment = TextAnchor.MiddleCenter;
        goText.color = Color.white;
        goText.font = defaultFont;

        Outline goOutline = goTextGO.AddComponent<Outline>();
        goOutline.effectColor = new Color(0, 0, 0, 0.4f);
        goOutline.effectDistance = new Vector2(1, -1);

        goBtnGO.SetActive(false); // 默认隐藏

        // === LockIcon (锁定图标, 默认隐藏) ===
        GameObject lockGO = CreateChild(root, "LockIcon");
        RectTransform lockRect = lockGO.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(1, 0.5f);
        lockRect.anchorMax = new Vector2(1, 0.5f);
        lockRect.anchoredPosition = new Vector2(-60, 0);
        lockRect.sizeDelta = new Vector2(50, 50);

        // 尝试加载锁定sprite
        Sprite lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/StageSelect/UI_Icon_Lock.png");
        if (lockSprite != null)
        {
            Image lockImg = lockGO.AddComponent<Image>();
            lockImg.sprite = lockSprite;
            lockImg.color = new Color(0.5f, 0.5f, 0.55f);
            lockImg.raycastTarget = false;
        }
        else
        {
            Text lockText = lockGO.AddComponent<Text>();
            lockText.text = "\U0001f512";
            lockText.fontSize = 34;
            lockText.alignment = TextAnchor.MiddleCenter;
            lockText.font = defaultFont;
        }

        lockGO.SetActive(false); // 默认隐藏

        // === 挂载 StageCardUI 脚本并绑定引用 ===
        MoShou.UI.StageCardUI cardUI = root.AddComponent<MoShou.UI.StageCardUI>();

        // 通过SerializedObject设置private [SerializeField]字段
        SerializedObject so = new SerializedObject(cardUI);
        so.FindProperty("cardBackground").objectReferenceValue = cardBg;
        so.FindProperty("cardButton").objectReferenceValue = cardBtn;
        so.FindProperty("thumbnail").objectReferenceValue = thumbImg;
        so.FindProperty("stageNameText").objectReferenceValue = nameText;
        so.FindProperty("stageInfoText").objectReferenceValue = infoText;
        so.FindProperty("starsRoot").objectReferenceValue = starsGO;

        SerializedProperty starsProp = so.FindProperty("starImages");
        starsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starImgs[i];
        }

        so.FindProperty("goButtonRoot").objectReferenceValue = goBtnGO;
        so.FindProperty("goButton").objectReferenceValue = goBtn;
        so.FindProperty("lockRoot").objectReferenceValue = lockGO;
        so.ApplyModifiedPropertiesWithoutUndo();

        // === 保存为Prefab ===
        string cardPath = "Assets/Resources/Prefabs/UI/StageCard.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, cardPath);
        Object.DestroyImmediate(root);

        Debug.Log($"[PrefabCreator] StageCard Prefab 已创建: {cardPath}");
    }

    [MenuItem("MoShou/创建选关Prefab/2. StageSelectCanvas画布")]
    public static void CreateStageSelectCanvasPrefab()
    {
        EnsureDirectory("Assets/Resources/Prefabs/UI");

        // 加载sprite
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/StageSelect/UI_StageSelect_BG.png");
        Sprite bannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Banner_Scroll_Gold.png");
        Font defaultFont = GetFont();

        // === Root: StageSelectCanvas ===
        GameObject canvasGO = new GameObject("StageSelectCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // === Background ===
        GameObject bgGO = CreateChild(canvasGO, "Background");
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImg = bgGO.AddComponent<Image>();
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.preserveAspect = false;
            bgImg.color = Color.white;
        }
        else
        {
            bgImg.color = new Color(0.08f, 0.12f, 0.18f);
        }
        bgImg.raycastTarget = false;

        // === TopTitleBar ===
        GameObject topBarGO = CreateChild(canvasGO, "TopTitleBar");
        RectTransform topRect = topBarGO.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0.5f, 1);
        topRect.anchorMax = new Vector2(0.5f, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.anchoredPosition = new Vector2(0, -8);

        if (bannerSprite != null)
        {
            topRect.sizeDelta = new Vector2(bannerSprite.rect.width, bannerSprite.rect.height);
            Image bannerImg = topBarGO.AddComponent<Image>();
            bannerImg.sprite = bannerSprite;
            bannerImg.type = Image.Type.Simple;
            bannerImg.color = Color.white;
            bannerImg.raycastTarget = false;
        }
        else
        {
            topRect.sizeDelta = new Vector2(700, 120);
            Image bannerImg = topBarGO.AddComponent<Image>();
            bannerImg.color = new Color(0.15f, 0.12f, 0.08f, 0.9f);
            bannerImg.raycastTarget = false;
        }

        // 标题文字
        GameObject titleGO = CreateChild(topBarGO, "Title");
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(40, 0);
        titleRect.offsetMax = new Vector2(-40, 0);

        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "关卡选择";
        titleText.fontSize = 44;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.45f, 0.25f, 0.1f);
        titleText.font = defaultFont;

        Outline titleOutline = titleGO.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.2f, 0.1f, 0.05f, 0.5f);
        titleOutline.effectDistance = new Vector2(1, -1);

        // === StageListScrollView ===
        // 计算banner底部位置来确定ScrollView顶部偏移
        float bannerHeight = bannerSprite != null ? bannerSprite.rect.height : 120f;
        float scrollTopOffset = 8 + bannerHeight + 10; // banner顶部偏移 + banner高度 + 间距

        GameObject scrollGO = CreateChild(canvasGO, "StageListScrollView");
        RectTransform scrollRectT = scrollGO.GetComponent<RectTransform>();
        scrollRectT.anchorMin = Vector2.zero;
        scrollRectT.anchorMax = Vector2.one;
        scrollRectT.offsetMin = new Vector2(0, 150); // 底部给BottomInfoBar留空间
        scrollRectT.offsetMax = new Vector2(0, -scrollTopOffset);

        ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 40f;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;

        // Viewport
        GameObject viewportGO = CreateChild(scrollGO, "Viewport");
        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0.01f);
        Mask mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        scroll.viewport = viewportRect;

        // Content
        GameObject contentGO = CreateChild(viewportGO, "Content");
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
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

        // === BottomInfoBar (底部信息栏: 盾牌等级 | 进度 | 金币) ===
        CreateBottomInfoBar(canvasGO, defaultFont);

        // === 保存为Prefab ===
        string canvasPath = "Assets/Resources/Prefabs/UI/StageSelectCanvas.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGO, canvasPath);
        Object.DestroyImmediate(canvasGO);

        Debug.Log($"[PrefabCreator] StageSelectCanvas Prefab 已创建: {canvasPath}");
    }

    /// <summary>
    /// 创建底部信息栏 - 盾牌等级 | 进度条 | 金币
    /// 子物体命名固定，运行时代码通过名字查找并填充数据
    /// </summary>
    static void CreateBottomInfoBar(GameObject canvasGO, Font defaultFont)
    {
        Sprite shieldSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Badge_Shield_Lv.png");
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Icon_Coin_Stack.png");
        Sprite progressFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_ProgressBar_Frame.png");

        // 底栏容器
        GameObject barGO = CreateChild(canvasGO, "BottomInfoBar");
        RectTransform barRect = barGO.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 0);
        barRect.anchorMax = new Vector2(1, 0);
        barRect.pivot = new Vector2(0.5f, 0);
        barRect.anchoredPosition = Vector2.zero;
        barRect.sizeDelta = new Vector2(0, 150);

        Image barBg = barGO.AddComponent<Image>();
        barBg.color = new Color(0.05f, 0.08f, 0.12f, 0.85f);

        // 顶部金色分界线
        GameObject borderGO = CreateChild(barGO, "GoldBorder");
        RectTransform borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0, 1);
        borderRect.anchorMax = new Vector2(1, 1);
        borderRect.pivot = new Vector2(0.5f, 1);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(0, 4);
        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(1f, 0.85f, 0.2f); // Gold
        borderImg.raycastTarget = false;

        // === 左侧: 盾牌 + 等级 ===
        GameObject playerGO = CreateChild(barGO, "PlayerInfo");
        RectTransform playerRect = playerGO.GetComponent<RectTransform>();
        playerRect.anchorMin = new Vector2(0, 0.5f);
        playerRect.anchorMax = new Vector2(0, 0.5f);
        playerRect.anchoredPosition = new Vector2(100, 0);
        playerRect.sizeDelta = new Vector2(160, 80);

        // 盾牌icon
        if (shieldSprite != null)
        {
            GameObject shieldGO = CreateChild(playerGO, "ShieldIcon");
            RectTransform shieldRect = shieldGO.GetComponent<RectTransform>();
            shieldRect.anchorMin = new Vector2(0, 0.5f);
            shieldRect.anchorMax = new Vector2(0, 0.5f);
            shieldRect.anchoredPosition = new Vector2(30, 0);
            shieldRect.sizeDelta = new Vector2(55, 60);
            Image shieldImg = shieldGO.AddComponent<Image>();
            shieldImg.sprite = shieldSprite;
            shieldImg.color = Color.white;
            shieldImg.raycastTarget = false;

            // 等级数字叠在盾牌上
            GameObject lvNumGO = CreateChild(shieldGO, "LevelNum");
            RectTransform lvNumRect = lvNumGO.GetComponent<RectTransform>();
            lvNumRect.anchorMin = Vector2.zero;
            lvNumRect.anchorMax = Vector2.one;
            lvNumRect.offsetMin = Vector2.zero;
            lvNumRect.offsetMax = Vector2.zero;
            Text lvNumText = lvNumGO.AddComponent<Text>();
            lvNumText.text = "1";
            lvNumText.fontSize = 22;
            lvNumText.fontStyle = FontStyle.Bold;
            lvNumText.alignment = TextAnchor.MiddleCenter;
            lvNumText.color = Color.white;
            lvNumText.font = defaultFont;
        }

        // 等级标签
        GameObject lvLabelGO = CreateChild(playerGO, "LevelLabel");
        RectTransform lvLabelRect = lvLabelGO.GetComponent<RectTransform>();
        lvLabelRect.anchorMin = new Vector2(0.4f, 0);
        lvLabelRect.anchorMax = new Vector2(1, 1);
        lvLabelRect.offsetMin = Vector2.zero;
        lvLabelRect.offsetMax = Vector2.zero;
        Text lvLabelText = lvLabelGO.AddComponent<Text>();
        lvLabelText.text = "Lv.1";
        lvLabelText.fontSize = 30;
        lvLabelText.fontStyle = FontStyle.Bold;
        lvLabelText.alignment = TextAnchor.MiddleCenter;
        lvLabelText.color = new Color(0.5f, 0.8f, 1f);
        lvLabelText.font = defaultFont;
        Outline lvOutline = lvLabelGO.AddComponent<Outline>();
        lvOutline.effectColor = new Color(0, 0, 0, 0.5f);
        lvOutline.effectDistance = new Vector2(1, -1);

        // === 中间: 进度 ===
        GameObject progressGO = CreateChild(barGO, "ProgressSection");
        RectTransform progressRect = progressGO.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.5f, 0.5f);
        progressRect.anchorMax = new Vector2(0.5f, 0.5f);
        progressRect.sizeDelta = new Vector2(340, 70);

        // 进度文字
        GameObject pLabelGO = CreateChild(progressGO, "ProgressLabel");
        RectTransform pLabelRect = pLabelGO.GetComponent<RectTransform>();
        pLabelRect.anchorMin = new Vector2(0, 0.55f);
        pLabelRect.anchorMax = new Vector2(1, 1);
        pLabelRect.offsetMin = Vector2.zero;
        pLabelRect.offsetMax = Vector2.zero;
        Text pLabelText = pLabelGO.AddComponent<Text>();
        pLabelText.text = "进度: 0/100";
        pLabelText.fontSize = 20;
        pLabelText.alignment = TextAnchor.MiddleCenter;
        pLabelText.color = Color.white;
        pLabelText.font = defaultFont;

        // 进度条背景
        GameObject barBgGO = CreateChild(progressGO, "BarBG");
        RectTransform barBgRect = barBgGO.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0.05f, 0.15f);
        barBgRect.anchorMax = new Vector2(0.95f, 0.48f);
        barBgRect.offsetMin = Vector2.zero;
        barBgRect.offsetMax = Vector2.zero;

        if (progressFrameSprite != null)
        {
            Image barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.sprite = progressFrameSprite;
            barBgImg.type = Image.Type.Sliced;
            barBgImg.color = Color.white;
        }
        else
        {
            Image barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);
        }

        // 进度条填充
        GameObject barFillGO = CreateChild(barBgGO, "BarFill");
        RectTransform fillRect = barFillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.02f, 1f);
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        Image fillImg = barFillGO.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 1f, 0.9f);

        // 百分比
        GameObject pctGO = CreateChild(progressGO, "Percent");
        RectTransform pctRect = pctGO.GetComponent<RectTransform>();
        pctRect.anchorMin = new Vector2(0, 0);
        pctRect.anchorMax = new Vector2(1, 0.2f);
        pctRect.offsetMin = Vector2.zero;
        pctRect.offsetMax = Vector2.zero;
        Text pctText = pctGO.AddComponent<Text>();
        pctText.text = "0%";
        pctText.fontSize = 16;
        pctText.alignment = TextAnchor.MiddleCenter;
        pctText.color = new Color(0.7f, 0.8f, 0.9f);
        pctText.font = defaultFont;

        // === 右侧: 金币 ===
        GameObject goldGO = CreateChild(barGO, "GoldSection");
        RectTransform goldRect = goldGO.GetComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(1, 0.5f);
        goldRect.anchorMax = new Vector2(1, 0.5f);
        goldRect.anchoredPosition = new Vector2(-110, 0);
        goldRect.sizeDelta = new Vector2(180, 70);

        // 金币icon
        GameObject coinGO = CreateChild(goldGO, "CoinIcon");
        RectTransform coinRect = coinGO.GetComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(0, 0.5f);
        coinRect.anchorMax = new Vector2(0, 0.5f);
        coinRect.anchoredPosition = new Vector2(30, 0);
        coinRect.sizeDelta = new Vector2(44, 44);
        Image coinImg = coinGO.AddComponent<Image>();
        if (coinSprite != null)
        {
            coinImg.sprite = coinSprite;
            coinImg.color = Color.white;
        }
        else
        {
            coinImg.color = new Color(1f, 0.85f, 0.2f);
        }
        coinImg.raycastTarget = false;

        // 金币数量
        GameObject goldTextGO = CreateChild(goldGO, "GoldAmount");
        RectTransform goldTextRect = goldTextGO.GetComponent<RectTransform>();
        goldTextRect.anchorMin = new Vector2(0.38f, 0);
        goldTextRect.anchorMax = new Vector2(1, 1);
        goldTextRect.offsetMin = Vector2.zero;
        goldTextRect.offsetMax = Vector2.zero;
        Text goldText = goldTextGO.AddComponent<Text>();
        goldText.text = "0金币";
        goldText.fontSize = 24;
        goldText.fontStyle = FontStyle.Bold;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = new Color(1f, 0.85f, 0.2f);
        goldText.font = defaultFont;
        Outline goldOutline = goldTextGO.AddComponent<Outline>();
        goldOutline.effectColor = new Color(0, 0, 0, 0.6f);
        goldOutline.effectDistance = new Vector2(1, -1);
    }

    #region 工具方法

    static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.AddComponent<RectTransform>();
        return child;
    }

    static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }

    static Font GetFont()
    {
        string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf" };
        foreach (string name in fontNames)
        {
            Font f = Resources.GetBuiltinResource<Font>(name);
            if (f != null) return f;
        }
        return Font.CreateDynamicFontFromOSFont("Arial", 14);
    }

    #endregion
}
