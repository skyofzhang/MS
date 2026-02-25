using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor工具：一键生成角色信息面板Prefab
/// 菜单: MoShou/创建角色信息Prefab
/// 生成 CharacterInfoScreen.prefab — 全屏模态面板，显示角色头像、属性、装备
/// </summary>
public class CharacterInfoPrefabCreator
{
    [MenuItem("MoShou/创建角色信息Prefab")]
    public static void CreateCharacterInfoPrefab()
    {
        EnsureDirectory("Assets/Resources/Prefabs/UI");

        // === 加载Sprite资源 ===
        Sprite panelBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/CharInfo/UI_CharInfo_BG.png");
        Sprite portraitFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/CharInfo/UI_CharInfo_Portrait_Frame.png");
        Sprite avatarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/Characters/MT_Avatar.png");
        Sprite bannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Banner_Scroll_Gold.png");
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Icon_Coin_Stack.png");
        Sprite emptyFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/CharInfo/UI_Equip_Slot_Empty.png");
        Sprite statHP = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/CharInfo/UI_Icon_Stat_HP.png");
        Sprite statATK = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/CharInfo/UI_Icon_Stat_ATK.png");
        Sprite statDEF = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/CharInfo/UI_Icon_Stat_DEF.png");
        Sprite statCRIT = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/CharInfo/UI_Icon_Stat_CRIT.png");
        Font defaultFont = GetFont();

        Color gold = new Color(1f, 0.85f, 0.2f);

        // === Root: CharacterInfoScreen (全屏容器) ===
        GameObject root = new GameObject("CharacterInfoScreen");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // CanvasGroup用于淡入淡出
        CanvasGroup cg = root.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // ================================================================
        // 1. 半透明遮罩背景
        // ================================================================
        GameObject overlayGO = CreateChild(root, "Overlay");
        RectTransform overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);

        // ================================================================
        // 2. 面板容器 (960x1700, 居中)
        // ================================================================
        GameObject panelGO = CreateChild(root, "Panel");
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(960, 1700);
        panelRect.anchoredPosition = Vector2.zero;

        Image bgImg = panelGO.AddComponent<Image>();
        if (panelBgSprite != null)
        {
            bgImg.sprite = panelBgSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = Color.white;
        }
        else
        {
            bgImg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        }

        // ================================================================
        // 3. 关闭按钮 (右上角 60x60)
        // ================================================================
        GameObject closeBtnGO = CreateChild(panelGO, "CloseButton");
        RectTransform closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 1);
        closeBtnRect.anchorMax = new Vector2(1, 1);
        closeBtnRect.pivot = new Vector2(1, 1);
        closeBtnRect.anchoredPosition = new Vector2(-15, -15);
        closeBtnRect.sizeDelta = new Vector2(60, 60);

        Image closeBtnBg = closeBtnGO.AddComponent<Image>();
        closeBtnBg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnBg;

        // X符号
        GameObject closeTextGO = CreateChild(closeBtnGO, "X");
        SetFullStretch(closeTextGO.GetComponent<RectTransform>());
        Text closeText = closeTextGO.AddComponent<Text>();
        closeText.text = "\u2715";
        closeText.fontSize = 28;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;
        closeText.font = defaultFont;

        // ================================================================
        // 4. 标题 — 卷轴Banner "角色信息"
        // ================================================================
        GameObject titleGO = CreateChild(panelGO, "Title");
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(500, 90);

        if (bannerSprite != null)
        {
            Image bannerImg = titleGO.AddComponent<Image>();
            bannerImg.sprite = bannerSprite;
            bannerImg.type = Image.Type.Sliced;
            bannerImg.color = Color.white;
            bannerImg.raycastTarget = false;
        }

        GameObject titleTextGO = CreateChild(titleGO, "TitleText");
        SetFullStretch(titleTextGO.GetComponent<RectTransform>());
        Text titleText = titleTextGO.AddComponent<Text>();
        titleText.text = "角色信息";
        titleText.fontSize = 38;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = gold;
        titleText.font = defaultFont;
        Outline titleOutline = titleTextGO.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.3f, 0.2f, 0.1f);
        titleOutline.effectDistance = new Vector2(2, -2);

        // ================================================================
        // 5. 角色展示区 (头像+名字+等级+经验+金币)
        // ================================================================
        GameObject portraitArea = CreateChild(panelGO, "CharacterPortrait");
        RectTransform portraitAreaRect = portraitArea.GetComponent<RectTransform>();
        portraitAreaRect.anchorMin = new Vector2(0, 0.62f);
        portraitAreaRect.anchorMax = new Vector2(1, 0.93f);
        portraitAreaRect.offsetMin = new Vector2(40, 0);
        portraitAreaRect.offsetMax = new Vector2(-40, 0);

        // --- 头像 300x300 ---
        GameObject avatarGO = CreateChild(portraitArea, "Avatar");
        RectTransform avatarRect = avatarGO.GetComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0.5f, 0.55f);
        avatarRect.anchorMax = new Vector2(0.5f, 0.55f);
        avatarRect.sizeDelta = new Vector2(300, 300);
        avatarRect.anchoredPosition = Vector2.zero;

        Image avatarBg = avatarGO.AddComponent<Image>();
        if (avatarSprite != null)
        {
            avatarBg.sprite = avatarSprite;
            avatarBg.color = Color.white;
        }
        else
        {
            avatarBg.color = new Color(0.2f, 0.22f, 0.28f, 1f);
        }

        // 头像内框
        GameObject innerGO = CreateChild(avatarGO, "Inner");
        RectTransform innerRect = innerGO.GetComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0.05f, 0.05f);
        innerRect.anchorMax = new Vector2(0.95f, 0.95f);
        innerRect.offsetMin = Vector2.zero;
        innerRect.offsetMax = Vector2.zero;
        Image characterAvatar = innerGO.AddComponent<Image>();
        characterAvatar.color = Color.white;

        // 肖像帧
        if (portraitFrame != null)
        {
            GameObject frameGO = CreateChild(avatarGO, "PortraitFrame");
            Image frameImg = frameGO.AddComponent<Image>();
            frameImg.sprite = portraitFrame;
            frameImg.type = Image.Type.Sliced;
            frameImg.color = Color.white;
            frameImg.raycastTarget = false;
            RectTransform frameRect = frameGO.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(-10, -10);
            frameRect.offsetMax = new Vector2(10, 10);
        }
        else
        {
            // fallback: 金色边框
            GameObject borderGO = CreateChild(avatarGO, "Border");
            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = gold;
            borderImg.raycastTarget = false;
            RectTransform borderRect = borderGO.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-5, -5);
            borderRect.offsetMax = new Vector2(5, 5);
            borderGO.transform.SetAsFirstSibling();
        }

        // --- 角色名 ---
        GameObject nameGO = CreateChild(portraitArea, "CharacterName");
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.1f, 0.22f);
        nameRect.anchorMax = new Vector2(0.9f, 0.32f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        Text characterNameText = nameGO.AddComponent<Text>();
        characterNameText.text = "战士·艾瑞克";
        characterNameText.fontSize = 30;
        characterNameText.fontStyle = FontStyle.Bold;
        characterNameText.alignment = TextAnchor.MiddleCenter;
        characterNameText.color = gold;
        characterNameText.font = defaultFont;
        nameGO.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);

        // --- 等级 ---
        GameObject levelGO = CreateChild(portraitArea, "Level");
        RectTransform levelRect = levelGO.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.3f, 0.14f);
        levelRect.anchorMax = new Vector2(0.7f, 0.22f);
        levelRect.offsetMin = Vector2.zero;
        levelRect.offsetMax = Vector2.zero;
        Text levelText = levelGO.AddComponent<Text>();
        levelText.text = "Lv.1";
        levelText.fontSize = 24;
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = new Color(0.8f, 0.85f, 0.9f);
        levelText.font = defaultFont;

        // --- 经验条 ---
        GameObject expBarBg = CreateChild(portraitArea, "ExpBarBG");
        RectTransform expBgRect = expBarBg.GetComponent<RectTransform>();
        expBgRect.anchorMin = new Vector2(0.15f, 0.08f);
        expBgRect.anchorMax = new Vector2(0.85f, 0.13f);
        expBgRect.offsetMin = Vector2.zero;
        expBgRect.offsetMax = Vector2.zero;
        Image expBgImg = expBarBg.AddComponent<Image>();
        expBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 经验条填充
        GameObject expFillGO = CreateChild(expBarBg, "ExpFill");
        RectTransform expFillRect = expFillGO.GetComponent<RectTransform>();
        expFillRect.anchorMin = Vector2.zero;
        expFillRect.anchorMax = new Vector2(0.6f, 1f);
        expFillRect.offsetMin = new Vector2(2, 2);
        expFillRect.offsetMax = new Vector2(-2, -2);
        Image expFillImg = expFillGO.AddComponent<Image>();
        expFillImg.color = new Color(0.3f, 0.7f, 1f, 1f);

        Slider expSlider = expBarBg.AddComponent<Slider>();
        expSlider.fillRect = expFillRect;
        expSlider.interactable = false;
        expSlider.minValue = 0;
        expSlider.maxValue = 1;
        expSlider.value = 0.6f;

        // 经验值文字
        GameObject expTextGO = CreateChild(expBarBg, "ExpText");
        SetFullStretch(expTextGO.GetComponent<RectTransform>());
        Text expText = expTextGO.AddComponent<Text>();
        expText.text = "0/100 EXP";
        expText.fontSize = 16;
        expText.alignment = TextAnchor.MiddleCenter;
        expText.color = Color.white;
        expText.font = defaultFont;

        // --- 金币行 ---
        GameObject goldRow = CreateChild(portraitArea, "GoldRow");
        RectTransform goldRowRect = goldRow.GetComponent<RectTransform>();
        goldRowRect.anchorMin = new Vector2(0.3f, 0.0f);
        goldRowRect.anchorMax = new Vector2(0.7f, 0.07f);
        goldRowRect.offsetMin = Vector2.zero;
        goldRowRect.offsetMax = Vector2.zero;

        // 金币icon
        GameObject coinGO = CreateChild(goldRow, "CoinIcon");
        RectTransform coinRect = coinGO.GetComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(0, 0.5f);
        coinRect.anchorMax = new Vector2(0, 0.5f);
        coinRect.anchoredPosition = new Vector2(20, 0);
        coinRect.sizeDelta = new Vector2(28, 28);
        Image coinImg = coinGO.AddComponent<Image>();
        if (coinSprite != null)
        {
            coinImg.sprite = coinSprite;
            coinImg.color = Color.white;
        }
        else
        {
            coinImg.color = gold;
        }
        coinImg.raycastTarget = false;

        // 金币文字
        GameObject goldTextGO = CreateChild(goldRow, "GoldText");
        RectTransform gtRect = goldTextGO.GetComponent<RectTransform>();
        gtRect.anchorMin = new Vector2(0.2f, 0);
        gtRect.anchorMax = new Vector2(1, 1);
        gtRect.offsetMin = Vector2.zero;
        gtRect.offsetMax = Vector2.zero;
        Text goldText = goldTextGO.AddComponent<Text>();
        goldText.text = "0 金币";
        goldText.fontSize = 22;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = gold;
        goldText.font = defaultFont;

        // ================================================================
        // 6. 角色属性面板 (2列网格)
        // ================================================================
        // "角色属性" 标题
        GameObject statsTitleGO = CreateChild(panelGO, "StatsTitle");
        RectTransform statsTitleRect = statsTitleGO.GetComponent<RectTransform>();
        statsTitleRect.anchorMin = new Vector2(0, 0.55f);
        statsTitleRect.anchorMax = new Vector2(1, 0.59f);
        statsTitleRect.offsetMin = Vector2.zero;
        statsTitleRect.offsetMax = Vector2.zero;
        Image stsDivider = statsTitleGO.AddComponent<Image>();
        stsDivider.color = new Color(0.4f, 0.35f, 0.25f, 0.3f);

        GameObject stsTitleTextGO = CreateChild(statsTitleGO, "Text");
        SetFullStretch(stsTitleTextGO.GetComponent<RectTransform>());
        Text statsTitle = stsTitleTextGO.AddComponent<Text>();
        statsTitle.text = "角色属性";
        statsTitle.fontSize = 26;
        statsTitle.fontStyle = FontStyle.Bold;
        statsTitle.alignment = TextAnchor.MiddleCenter;
        statsTitle.color = gold;
        statsTitle.font = defaultFont;

        // 属性Grid
        GameObject statsGridGO = CreateChild(panelGO, "StatsGrid");
        RectTransform statsGridRect = statsGridGO.GetComponent<RectTransform>();
        statsGridRect.anchorMin = new Vector2(0, 0.23f);
        statsGridRect.anchorMax = new Vector2(1, 0.55f);
        statsGridRect.offsetMin = new Vector2(30, 0);
        statsGridRect.offsetMax = new Vector2(-30, -5);

        GridLayoutGroup statsGrid = statsGridGO.AddComponent<GridLayoutGroup>();
        statsGrid.cellSize = new Vector2(400, 50);
        statsGrid.spacing = new Vector2(10, 8);
        statsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        statsGrid.constraintCount = 2;
        statsGrid.childAlignment = TextAnchor.UpperCenter;
        statsGrid.padding = new RectOffset(10, 10, 10, 10);

        // 4个属性行
        Text healthText = CreateStatGridItem(statsGridGO, "Stat_生命值:", "UI_Icon_Stat_HP", "生命值:", new Color(0.9f, 0.3f, 0.3f), statHP, defaultFont);
        Text attackText = CreateStatGridItem(statsGridGO, "Stat_攻击力:", "UI_Icon_Stat_ATK", "攻击力:", new Color(0.9f, 0.7f, 0.3f), statATK, defaultFont);
        Text defenseText = CreateStatGridItem(statsGridGO, "Stat_防御力:", "UI_Icon_Stat_DEF", "防御力:", new Color(0.3f, 0.6f, 0.9f), statDEF, defaultFont);
        Text critRateText = CreateStatGridItem(statsGridGO, "Stat_暴击率:", "UI_Icon_Stat_CRIT", "暴击率:", new Color(0.9f, 0.8f, 0.2f), statCRIT, defaultFont);

        // 4个待扩展空位
        for (int i = 0; i < 4; i++)
        {
            CreateStatGridItem(statsGridGO, $"Stat_待扩展_{i}", null, "待扩展", new Color(0.4f, 0.4f, 0.4f, 0.5f), null, defaultFont);
        }

        // ================================================================
        // 7. 已穿戴装备 (3列网格)
        // ================================================================
        // "已穿戴装备" 标题
        GameObject eqTitleGO = CreateChild(panelGO, "EquipmentTitle");
        RectTransform eqTitleRect = eqTitleGO.GetComponent<RectTransform>();
        eqTitleRect.anchorMin = new Vector2(0, 0.18f);
        eqTitleRect.anchorMax = new Vector2(1, 0.22f);
        eqTitleRect.offsetMin = Vector2.zero;
        eqTitleRect.offsetMax = Vector2.zero;
        Image eqDivider = eqTitleGO.AddComponent<Image>();
        eqDivider.color = new Color(0.4f, 0.35f, 0.25f, 0.3f);

        GameObject eqTitleTextGO = CreateChild(eqTitleGO, "Text");
        SetFullStretch(eqTitleTextGO.GetComponent<RectTransform>());
        Text eqTitle = eqTitleTextGO.AddComponent<Text>();
        eqTitle.text = "已穿戴装备";
        eqTitle.fontSize = 26;
        eqTitle.fontStyle = FontStyle.Bold;
        eqTitle.alignment = TextAnchor.MiddleCenter;
        eqTitle.color = gold;
        eqTitle.font = defaultFont;

        // 装备Grid
        GameObject equipGridGO = CreateChild(panelGO, "EquipmentGrid");
        RectTransform equipGridRect = equipGridGO.GetComponent<RectTransform>();
        equipGridRect.anchorMin = new Vector2(0, 0.02f);
        equipGridRect.anchorMax = new Vector2(1, 0.18f);
        equipGridRect.offsetMin = new Vector2(60, 10);
        equipGridRect.offsetMax = new Vector2(-60, -10);

        GridLayoutGroup equipGrid = equipGridGO.AddComponent<GridLayoutGroup>();
        equipGrid.cellSize = new Vector2(140, 160);
        equipGrid.spacing = new Vector2(20, 10);
        equipGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        equipGrid.constraintCount = 3;
        equipGrid.childAlignment = TextAnchor.UpperCenter;
        equipGrid.padding = new RectOffset(30, 30, 5, 5);

        // 6个装备槽
        Image helmetSlot = CreateEquipmentSlot(equipGridGO, "Slot_Helmet", "头盔", emptyFrame, defaultFont);
        Image weaponSlot = CreateEquipmentSlot(equipGridGO, "Slot_Weapon", "武器", emptyFrame, defaultFont);
        Image accessory1Slot = CreateEquipmentSlot(equipGridGO, "Slot_Accessory1", "饰品", emptyFrame, defaultFont);
        Image accessory2Slot = CreateEquipmentSlot(equipGridGO, "Slot_Accessory2", "项链", emptyFrame, defaultFont);
        Image armorSlot = CreateEquipmentSlot(equipGridGO, "Slot_Armor", "护甲", emptyFrame, defaultFont);
        Image bootsSlot = CreateEquipmentSlot(equipGridGO, "Slot_Boots", "鞋子", emptyFrame, defaultFont);

        // ================================================================
        // 8. 金色装饰线
        // ================================================================
        CreateDecoLine(panelGO, "LineTop", new Vector2(0.05f, 0.98f), new Vector2(0.95f, 0.98f), gold);
        CreateDecoLine(panelGO, "LineBottom", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.02f), gold);

        // ================================================================
        // 9. 绑定SerializeField到CharacterInfoScreen组件
        // ================================================================
        MoShou.UI.CharacterInfoScreen screen = root.AddComponent<MoShou.UI.CharacterInfoScreen>();
        SerializedObject so = new SerializedObject(screen);

        so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
        so.FindProperty("characterAvatar").objectReferenceValue = characterAvatar;
        so.FindProperty("characterNameText").objectReferenceValue = characterNameText;
        so.FindProperty("levelText").objectReferenceValue = levelText;
        so.FindProperty("expSlider").objectReferenceValue = expSlider;
        so.FindProperty("expText").objectReferenceValue = expText;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("goldText").objectReferenceValue = goldText;

        // 装备槽 (Icon子对象的Image)
        so.FindProperty("weaponSlot").objectReferenceValue = weaponSlot;
        so.FindProperty("armorSlot").objectReferenceValue = armorSlot;
        so.FindProperty("helmetSlot").objectReferenceValue = helmetSlot;
        so.FindProperty("bootsSlot").objectReferenceValue = bootsSlot;
        so.FindProperty("accessory1Slot").objectReferenceValue = accessory1Slot;
        so.FindProperty("accessory2Slot").objectReferenceValue = accessory2Slot;

        // 属性文本
        so.FindProperty("attackText").objectReferenceValue = attackText;
        so.FindProperty("defenseText").objectReferenceValue = defenseText;
        so.FindProperty("healthText").objectReferenceValue = healthText;
        so.FindProperty("critRateText").objectReferenceValue = critRateText;

        so.ApplyModifiedPropertiesWithoutUndo();

        // ================================================================
        // 10. 保存Prefab
        // ================================================================
        string prefabPath = "Assets/Resources/Prefabs/UI/CharacterInfoScreen.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[CharacterInfoPrefabCreator] 角色信息Prefab创建完成！路径: {prefabPath}");
    }

    #region Helper Methods

    /// <summary>
    /// 创建属性网格单行: icon + 标签 + 数值
    /// 返回数值Text引用
    /// </summary>
    static Text CreateStatGridItem(GameObject parent, string name, string iconName, string label, Color labelColor, Sprite iconSprite, Font font)
    {
        GameObject rowGO = CreateChild(parent, name);

        // icon (28x28)
        if (iconSprite != null)
        {
            GameObject iconGO = CreateChild(rowGO, "Icon");
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(18, 0);
            iconRect.sizeDelta = new Vector2(28, 28);
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;
        }

        // 标签
        GameObject labelGO = CreateChild(rowGO, "Label");
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.1f, 0);
        labelRect.anchorMax = new Vector2(0.55f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = label;
        labelText.fontSize = 22;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = labelColor;
        labelText.font = font;

        // 数值
        GameObject valueGO = CreateChild(rowGO, "Value");
        RectTransform valueRect = valueGO.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.55f, 0);
        valueRect.anchorMax = new Vector2(0.98f, 1);
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;
        Text valueText = valueGO.AddComponent<Text>();
        valueText.text = "0";
        valueText.fontSize = 22;
        valueText.alignment = TextAnchor.MiddleRight;
        valueText.color = Color.white;
        valueText.font = font;
        valueText.supportRichText = true;

        return valueText;
    }

    /// <summary>
    /// 创建装备槽: 背景 + icon + 标签 + 按钮
    /// 返回Icon的Image引用 (用于刷新装备图标)
    /// </summary>
    static Image CreateEquipmentSlot(GameObject parent, string name, string label, Sprite frameSprite, Font font)
    {
        GameObject slotGO = CreateChild(parent, name);

        // 槽背景
        Image slotBg = slotGO.AddComponent<Image>();
        if (frameSprite != null)
        {
            slotBg.sprite = frameSprite;
            slotBg.type = Image.Type.Sliced;
            slotBg.color = Color.white;
        }
        else
        {
            slotBg.color = new Color(0.15f, 0.18f, 0.22f, 1f);
        }

        // 按钮
        Button slotBtn = slotGO.AddComponent<Button>();
        slotBtn.targetGraphic = slotBg;

        // 图标区域 100x100
        GameObject iconGO = CreateChild(slotGO, "Icon");
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.55f);
        iconRect.anchorMax = new Vector2(0.5f, 0.55f);
        iconRect.sizeDelta = new Vector2(100, 100);
        iconRect.anchoredPosition = Vector2.zero;
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        // 名称标签 (底部)
        GameObject labelGO = CreateChild(slotGO, "Label");
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 0);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.anchoredPosition = new Vector2(0, 5);
        labelRect.sizeDelta = new Vector2(0, 30);
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = label;
        labelText.fontSize = 18;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = new Color(0.8f, 0.8f, 0.8f);
        labelText.font = font;

        return iconImg;
    }

    static void CreateDecoLine(GameObject parent, string name, Vector2 start, Vector2 end, Color color)
    {
        GameObject lineGO = CreateChild(parent, name);
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();
        lineRect.anchorMin = start;
        lineRect.anchorMax = new Vector2(end.x, start.y);
        lineRect.pivot = new Vector2(0, 0.5f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(0, 3);

        Image lineImg = lineGO.AddComponent<Image>();
        lineImg.color = color;
    }

    static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.AddComponent<RectTransform>();
        return child;
    }

    static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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
