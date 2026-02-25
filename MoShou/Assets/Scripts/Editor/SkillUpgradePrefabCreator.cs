using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor工具：一键生成技能升级面板Prefab
/// 菜单: MoShou/创建技能升级Prefab
/// 生成 SkillUpgradePanel.prefab — 全屏模态面板，左侧技能列表 + 右侧详情
/// </summary>
public class SkillUpgradePrefabCreator
{
    [MenuItem("MoShou/创建技能升级Prefab")]
    public static void CreateSkillUpgradePrefab()
    {
        EnsureDirectory("Assets/Resources/Prefabs/UI");

        // === 加载Sprite资源 ===
        Sprite panelBrown = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Kenney/panel_brown.png");
        Sprite btnSquareBrown = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Kenney/buttonSquare_brown.png");
        Sprite iconCross = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Kenney/iconCross_brown.png");
        Sprite parchmentSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Skills/UI_Skill_RightPanel_Parchment.png");
        Sprite iconFrameGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Skills/UI_Skill_Icon_Frame_Gold.png");
        Sprite upgradeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Skills/UI_Btn_Upgrade_Cyan.png");
        Font defaultFont = GetFont();

        Color warmGold = new Color(1f, 0.9f, 0.6f);
        Color mutedGold = new Color(0.8f, 0.7f, 0.4f);
        Color brightGold = new Color(1f, 0.85f, 0.2f);

        // === Root: SkillUpgradePanel (全屏容器 + 半透明背景) ===
        GameObject root = new GameObject("SkillUpgradePanel");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image rootBg = root.AddComponent<Image>();
        rootBg.color = new Color(0, 0, 0, 0.8f);

        // ================================================================
        // 1. 左侧内容区 (3%-52%, 10%-90%)
        // ================================================================
        GameObject contentGO = CreateChild(root, "Content");
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.03f, 0.1f);
        contentRect.anchorMax = new Vector2(0.52f, 0.9f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        Image contentBg = contentGO.AddComponent<Image>();
        if (panelBrown != null)
        {
            contentBg.sprite = panelBrown;
            contentBg.type = Image.Type.Sliced;
            contentBg.color = Color.white;
        }
        else
        {
            contentBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        }

        // --- 标题 "技能升级" ---
        GameObject titleGO = CreateChild(contentGO, "Title");
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -35);
        titleRect.sizeDelta = new Vector2(200, 50);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "技能升级";
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = warmGold;
        titleText.font = defaultFont;
        Outline titleOutline = titleGO.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0, 0, 0, 0.6f);
        titleOutline.effectDistance = new Vector2(1, -1);

        // --- 金币显示 ---
        GameObject goldGO = CreateChild(contentGO, "GoldDisplay");
        RectTransform goldRect = goldGO.GetComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0, 1);
        goldRect.anchorMax = new Vector2(0, 1);
        goldRect.anchoredPosition = new Vector2(20, -35);
        goldRect.sizeDelta = new Vector2(150, 40);
        Text goldText = goldGO.AddComponent<Text>();
        goldText.text = "金币: 0";
        goldText.fontSize = 20;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = brightGold;
        goldText.font = defaultFont;

        // --- 技能列表容器 (带滚动) ---
        GameObject skillsGO = CreateChild(contentGO, "SkillsContainer");
        RectTransform skillsRect = skillsGO.GetComponent<RectTransform>();
        skillsRect.anchorMin = new Vector2(0.02f, 0.02f);
        skillsRect.anchorMax = new Vector2(0.98f, 0.85f);
        skillsRect.offsetMin = Vector2.zero;
        skillsRect.offsetMax = Vector2.zero;

        ScrollRect scrollRect = skillsGO.AddComponent<ScrollRect>();
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        // 滚动内容容器
        GameObject scrollContentGO = CreateChild(skillsGO, "Content");
        RectTransform scrollContentRect = scrollContentGO.GetComponent<RectTransform>();
        scrollContentRect.anchorMin = new Vector2(0, 1);
        scrollContentRect.anchorMax = new Vector2(1, 1);
        scrollContentRect.pivot = new Vector2(0.5f, 1);
        scrollContentRect.anchoredPosition = Vector2.zero;
        scrollContentRect.sizeDelta = new Vector2(0, 800);

        VerticalLayoutGroup vlg = scrollContentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 5;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(5, 5, 5, 5);

        ContentSizeFitter csf = scrollContentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = scrollContentRect;

        // ================================================================
        // 2. 关闭按钮 (右上角 50x50)
        // ================================================================
        GameObject closeBtnGO = CreateChild(root, "CloseButton");
        RectTransform closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 1);
        closeBtnRect.anchorMax = new Vector2(1, 1);
        closeBtnRect.anchoredPosition = new Vector2(-30, -30);
        closeBtnRect.sizeDelta = new Vector2(50, 50);

        Image closeBtnImg = closeBtnGO.AddComponent<Image>();
        if (btnSquareBrown != null)
        {
            closeBtnImg.sprite = btnSquareBrown;
            closeBtnImg.type = Image.Type.Sliced;
            closeBtnImg.color = Color.white;
        }
        else
        {
            closeBtnImg.color = new Color(0.8f, 0.2f, 0.2f);
        }

        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnImg;

        // 关闭按钮图标
        GameObject closeIconGO = CreateChild(closeBtnGO, "Icon");
        RectTransform closeIconRect = closeIconGO.GetComponent<RectTransform>();
        if (iconCross != null)
        {
            closeIconRect.anchorMin = Vector2.zero;
            closeIconRect.anchorMax = Vector2.one;
            closeIconRect.offsetMin = new Vector2(8, 8);
            closeIconRect.offsetMax = new Vector2(-8, -8);
            Image crossImg = closeIconGO.AddComponent<Image>();
            crossImg.sprite = iconCross;
            crossImg.color = Color.white;
            crossImg.raycastTarget = false;
        }
        else
        {
            SetFullStretch(closeIconRect);
            Text crossText = closeIconGO.AddComponent<Text>();
            crossText.text = "X";
            crossText.fontSize = 28;
            crossText.alignment = TextAnchor.MiddleCenter;
            crossText.color = Color.white;
            crossText.font = defaultFont;
        }

        // ================================================================
        // 3. 右侧详情面板 (50%-95%, 10%-88%), 默认隐藏
        // ================================================================
        GameObject detailGO = CreateChild(root, "DetailPanel");
        RectTransform detailRect = detailGO.GetComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0.50f, 0.1f);
        detailRect.anchorMax = new Vector2(0.95f, 0.88f);
        detailRect.offsetMin = Vector2.zero;
        detailRect.offsetMax = Vector2.zero;

        Image detailBg = detailGO.AddComponent<Image>();
        if (parchmentSprite != null)
        {
            detailBg.sprite = parchmentSprite;
            detailBg.type = Image.Type.Sliced;
            detailBg.color = Color.white;
        }
        else
        {
            detailBg.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
        }

        // --- 技能图标 (120x120 + 金色帧) ---
        GameObject iconGO = CreateChild(detailGO, "Icon");
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 1);
        iconRect.anchorMax = new Vector2(0.5f, 1);
        iconRect.anchoredPosition = new Vector2(0, -85);
        iconRect.sizeDelta = new Vector2(120, 120);
        Image detailIcon = iconGO.AddComponent<Image>();
        detailIcon.color = Color.gray;

        // 图标帧
        if (iconFrameGold != null)
        {
            GameObject frameGO = CreateChild(iconGO, "Frame");
            Image frameImg = frameGO.AddComponent<Image>();
            frameImg.sprite = iconFrameGold;
            frameImg.type = Image.Type.Sliced;
            frameImg.color = Color.white;
            frameImg.raycastTarget = false;
            RectTransform frameRect = frameGO.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(-8, -8);
            frameRect.offsetMax = new Vector2(8, 8);
        }

        // --- 技能名称 ---
        GameObject nameGO = CreateChild(detailGO, "Name");
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.anchoredPosition = new Vector2(0, -165);
        nameRect.sizeDelta = new Vector2(0, 45);
        Text detailName = nameGO.AddComponent<Text>();
        detailName.text = "技能名称";
        detailName.fontSize = 30;
        detailName.fontStyle = FontStyle.Bold;
        detailName.alignment = TextAnchor.MiddleCenter;
        detailName.color = Color.white;
        detailName.font = defaultFont;

        // --- 技能等级 ---
        GameObject levelGO = CreateChild(detailGO, "Level");
        RectTransform levelRect = levelGO.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 1);
        levelRect.anchorMax = new Vector2(1, 1);
        levelRect.anchoredPosition = new Vector2(0, -210);
        levelRect.sizeDelta = new Vector2(0, 35);
        Text detailLevel = levelGO.AddComponent<Text>();
        detailLevel.text = "等级: 0/5";
        detailLevel.fontSize = 22;
        detailLevel.alignment = TextAnchor.MiddleCenter;
        detailLevel.color = mutedGold;
        detailLevel.font = defaultFont;

        // --- 技能描述 ---
        GameObject descGO = CreateChild(detailGO, "Description");
        RectTransform descRect = descGO.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.08f, 0.25f);
        descRect.anchorMax = new Vector2(0.92f, 0.62f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;
        Text detailDescription = descGO.AddComponent<Text>();
        detailDescription.text = "技能描述";
        detailDescription.fontSize = 20;
        detailDescription.alignment = TextAnchor.UpperLeft;
        detailDescription.color = new Color(0.8f, 0.8f, 0.8f);
        detailDescription.font = defaultFont;

        // --- 升级消耗 ---
        GameObject costGO = CreateChild(detailGO, "Cost");
        RectTransform costRect = costGO.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0, 0);
        costRect.anchorMax = new Vector2(1, 0);
        costRect.anchoredPosition = new Vector2(0, 100);
        costRect.sizeDelta = new Vector2(0, 30);
        Text detailCost = costGO.AddComponent<Text>();
        detailCost.text = "升级消耗: 0 金币";
        detailCost.fontSize = 22;
        detailCost.alignment = TextAnchor.MiddleCenter;
        detailCost.color = brightGold;
        detailCost.font = defaultFont;

        // --- 升级按钮 (200x60) ---
        GameObject upgradeBtnGO = CreateChild(detailGO, "UpgradeButton");
        RectTransform upgradeBtnRect = upgradeBtnGO.GetComponent<RectTransform>();
        upgradeBtnRect.anchorMin = new Vector2(0.5f, 0);
        upgradeBtnRect.anchorMax = new Vector2(0.5f, 0);
        upgradeBtnRect.anchoredPosition = new Vector2(0, 45);
        upgradeBtnRect.sizeDelta = new Vector2(200, 60);

        Image upgradeBtnBg = upgradeBtnGO.AddComponent<Image>();
        if (upgradeBtnSprite != null)
        {
            upgradeBtnBg.sprite = upgradeBtnSprite;
            upgradeBtnBg.type = Image.Type.Sliced;
            upgradeBtnBg.color = Color.white;
        }
        else
        {
            upgradeBtnBg.color = new Color(0.2f, 0.6f, 0.7f);
        }

        Button upgradeBtn = upgradeBtnGO.AddComponent<Button>();
        upgradeBtn.targetGraphic = upgradeBtnBg;

        // 按钮文字
        GameObject btnTextGO = CreateChild(upgradeBtnGO, "Text");
        SetFullStretch(btnTextGO.GetComponent<RectTransform>());
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.text = "升级";
        btnText.fontSize = 26;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = defaultFont;
        Outline btnOutline = btnTextGO.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0, 0, 0, 0.4f);
        btnOutline.effectDistance = new Vector2(1, -1);

        // 详情面板默认隐藏
        detailGO.SetActive(false);

        // ================================================================
        // 4. 绑定SerializeField到SkillUpgradePanel组件
        // ================================================================
        MoShou.UI.SkillUpgradePanel panel = root.AddComponent<MoShou.UI.SkillUpgradePanel>();
        SerializedObject so = new SerializedObject(panel);

        so.FindProperty("skillsContainer").objectReferenceValue = scrollContentGO.transform;
        so.FindProperty("goldText").objectReferenceValue = goldText;
        so.FindProperty("titleText").objectReferenceValue = titleText;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("detailPanel").objectReferenceValue = detailGO;
        so.FindProperty("detailIcon").objectReferenceValue = detailIcon;
        so.FindProperty("detailName").objectReferenceValue = detailName;
        so.FindProperty("detailDescription").objectReferenceValue = detailDescription;
        so.FindProperty("detailLevel").objectReferenceValue = detailLevel;
        so.FindProperty("detailCost").objectReferenceValue = detailCost;
        so.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn;

        so.ApplyModifiedPropertiesWithoutUndo();

        // ================================================================
        // 5. 保存Prefab
        // ================================================================
        string prefabPath = "Assets/Resources/Prefabs/UI/SkillUpgradePanel.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[SkillUpgradePrefabCreator] 技能升级Prefab创建完成！路径: {prefabPath}");
    }

    #region Helper Methods

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
