using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor工具：一键生成商店界面所需的两个Prefab
/// 菜单: MoShou/创建商店Prefab
/// </summary>
public class ShopPrefabCreator
{
    [MenuItem("MoShou/创建商店Prefab/0. 全部生成")]
    public static void CreatePrefabs()
    {
        CreateShopItemCardPrefab();
        CreateShopPanelPrefab();
        AssetDatabase.Refresh();
        Debug.Log("[ShopPrefabCreator] 商店Prefab创建完成！请在Project面板查看 Assets/Resources/Prefabs/UI/");
    }

    [MenuItem("MoShou/创建商店Prefab/1. ShopItemCard商品卡片")]
    public static void CreateShopItemCardPrefab()
    {
        EnsureDirectory("Assets/Resources/Prefabs/UI");

        Sprite rowFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Shop/UI_Shop_ItemRow_Frame.png");
        Sprite iconFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Shop/UI_Shop_ItemIcon_Frame.png");
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Icon_Coin_Stack.png");
        Sprite buyBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Shop/UI_Btn_Buy.png");
        Font defaultFont = GetFont();

        // === Root: ShopItemCard ===
        GameObject root = new GameObject("ShopItemCard");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(0, 110);

        LayoutElement rootLayout = root.AddComponent<LayoutElement>();
        rootLayout.preferredHeight = 110;
        rootLayout.flexibleWidth = 1;

        Image bgImg = root.AddComponent<Image>();
        if (rowFrameSprite != null)
        {
            bgImg.sprite = rowFrameSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = Color.white;
        }
        else
        {
            bgImg.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);
        }

        // === IconContainer (80x80, 左侧) ===
        GameObject iconContGO = CreateChild(root, "IconContainer");
        RectTransform iconContRect = iconContGO.GetComponent<RectTransform>();
        iconContRect.anchorMin = new Vector2(0, 0.5f);
        iconContRect.anchorMax = new Vector2(0, 0.5f);
        iconContRect.anchoredPosition = new Vector2(55, 0);
        iconContRect.sizeDelta = new Vector2(80, 80);

        Image frameImg = iconContGO.AddComponent<Image>();
        if (iconFrameSprite != null)
        {
            frameImg.sprite = iconFrameSprite;
            frameImg.type = Image.Type.Sliced;
            frameImg.color = Color.white;
        }
        else
        {
            frameImg.color = new Color(0.3f, 0.3f, 0.35f, 0.6f);
        }
        frameImg.raycastTarget = false;

        // Icon图片
        GameObject iconGO = CreateChild(iconContGO, "Icon");
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
        iconImg.raycastTarget = false;

        // === Name (物品名称) ===
        GameObject nameGO = CreateChild(root, "Name");
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(0, 0.5f);
        nameRect.anchoredPosition = new Vector2(175, 22);
        nameRect.sizeDelta = new Vector2(220, 32);
        Text nameText = nameGO.AddComponent<Text>();
        nameText.text = "物品名称";
        nameText.fontSize = 24;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = Color.white;
        nameText.font = defaultFont;
        Outline nameOutline = nameGO.AddComponent<Outline>();
        nameOutline.effectColor = new Color(0, 0, 0, 0.4f);
        nameOutline.effectDistance = new Vector2(1, -1);

        // === Description (描述) ===
        GameObject descGO = CreateChild(root, "Description");
        RectTransform descRect = descGO.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.5f);
        descRect.anchorMax = new Vector2(0, 0.5f);
        descRect.anchoredPosition = new Vector2(175, -14);
        descRect.sizeDelta = new Vector2(260, 25);
        Text descText = descGO.AddComponent<Text>();
        descText.text = "物品描述";
        descText.fontSize = 17;
        descText.alignment = TextAnchor.MiddleLeft;
        descText.color = new Color(0.7f, 0.7f, 0.7f);
        descText.font = defaultFont;

        // === PriceArea (价格区域) ===
        GameObject priceAreaGO = CreateChild(root, "PriceArea");
        RectTransform priceAreaRect = priceAreaGO.GetComponent<RectTransform>();
        priceAreaRect.anchorMin = new Vector2(1, 0.5f);
        priceAreaRect.anchorMax = new Vector2(1, 0.5f);
        priceAreaRect.anchoredPosition = new Vector2(-130, 12);
        priceAreaRect.sizeDelta = new Vector2(100, 30);

        // CoinIcon
        GameObject coinGO = CreateChild(priceAreaGO, "CoinIcon");
        RectTransform coinRect = coinGO.GetComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(0, 0.5f);
        coinRect.anchorMax = new Vector2(0, 0.5f);
        coinRect.anchoredPosition = new Vector2(4, 0);
        coinRect.sizeDelta = new Vector2(22, 22);
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

        // Price文字
        GameObject priceGO = CreateChild(priceAreaGO, "Price");
        RectTransform priceRect = priceGO.GetComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0.3f, 0);
        priceRect.anchorMax = new Vector2(1, 1);
        priceRect.offsetMin = Vector2.zero;
        priceRect.offsetMax = Vector2.zero;
        Text priceText = priceGO.AddComponent<Text>();
        priceText.text = "100";
        priceText.fontSize = 20;
        priceText.alignment = TextAnchor.MiddleRight;
        priceText.color = new Color(1f, 0.85f, 0.2f);
        priceText.font = defaultFont;

        // === BuyButton (购买按钮) ===
        GameObject buyGO = CreateChild(root, "BuyButton");
        RectTransform buyRect = buyGO.GetComponent<RectTransform>();
        buyRect.anchorMin = new Vector2(1, 0.5f);
        buyRect.anchorMax = new Vector2(1, 0.5f);
        buyRect.anchoredPosition = new Vector2(-55, -14);
        buyRect.sizeDelta = new Vector2(90, 40);

        Image buyBg = buyGO.AddComponent<Image>();
        if (buyBtnSprite != null)
        {
            buyBg.sprite = buyBtnSprite;
            buyBg.type = Image.Type.Sliced;
            buyBg.color = Color.white;
        }
        else
        {
            buyBg.color = new Color(0.3f, 0.7f, 0.3f);
        }

        Button buyBtn = buyGO.AddComponent<Button>();
        buyBtn.targetGraphic = buyBg;

        GameObject buyTextGO = CreateChild(buyGO, "Text");
        RectTransform buyTextRect = buyTextGO.GetComponent<RectTransform>();
        buyTextRect.anchorMin = Vector2.zero;
        buyTextRect.anchorMax = Vector2.one;
        buyTextRect.offsetMin = Vector2.zero;
        buyTextRect.offsetMax = Vector2.zero;
        Text buyText = buyTextGO.AddComponent<Text>();
        buyText.text = "购买";
        buyText.fontSize = 18;
        buyText.fontStyle = FontStyle.Bold;
        buyText.alignment = TextAnchor.MiddleCenter;
        buyText.color = Color.white;
        buyText.font = defaultFont;
        Outline buyOutline = buyTextGO.AddComponent<Outline>();
        buyOutline.effectColor = new Color(0, 0, 0, 0.3f);
        buyOutline.effectDistance = new Vector2(1, -1);

        // === 挂载 ShopItemCardUI 并绑定引用 ===
        MoShou.UI.ShopItemCardUI cardUI = root.AddComponent<MoShou.UI.ShopItemCardUI>();
        SerializedObject so = new SerializedObject(cardUI);
        so.FindProperty("background").objectReferenceValue = bgImg;
        so.FindProperty("iconFrame").objectReferenceValue = frameImg;
        so.FindProperty("iconImage").objectReferenceValue = iconImg;
        so.FindProperty("nameText").objectReferenceValue = nameText;
        so.FindProperty("descriptionText").objectReferenceValue = descText;
        so.FindProperty("coinIcon").objectReferenceValue = coinImg;
        so.FindProperty("priceText").objectReferenceValue = priceText;
        so.FindProperty("buyButton").objectReferenceValue = buyBtn;
        so.FindProperty("buyButtonText").objectReferenceValue = buyText;
        so.ApplyModifiedPropertiesWithoutUndo();

        // === 保存为Prefab ===
        string path = "Assets/Resources/Prefabs/UI/ShopItemCard.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[ShopPrefabCreator] ShopItemCard Prefab 已创建: {path}");
    }

    [MenuItem("MoShou/创建商店Prefab/2. ShopPanel商店面板")]
    public static void CreateShopPanelPrefab()
    {
        EnsureDirectory("Assets/Resources/Prefabs/UI");

        Sprite shopBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Shop/UI_Shop_BG.png");
        Sprite closeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Btn_Close_X.png");
        Sprite tabActiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Shop/UI_Shop_Tab_Active.png");
        Sprite tabInactiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Shop/UI_Shop_Tab_Inactive.png");
        Font defaultFont = GetFont();

        // === Root: ShopPanel (全屏半透明黑色遮罩) ===
        GameObject panelGO = new GameObject("ShopPanel");
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);

        // === Content (内容框) ===
        GameObject contentGO = CreateChild(panelGO, "Content");
        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.05f, 0.1f);
        contentRect.anchorMax = new Vector2(0.95f, 0.9f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        Image contentBg = contentGO.AddComponent<Image>();
        if (shopBgSprite != null)
        {
            contentBg.sprite = shopBgSprite;
            contentBg.type = Image.Type.Sliced;
            contentBg.color = Color.white;
        }
        else
        {
            contentBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        }

        // === Title ===
        GameObject titleGO = CreateChild(contentGO, "Title");
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -35);
        titleRect.sizeDelta = new Vector2(200, 50);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "商店";
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.6f, 0.4f, 0.1f);
        titleText.font = defaultFont;
        Outline titleOutline = titleGO.AddComponent<Outline>();
        titleOutline.effectColor = new Color(1f, 1f, 1f, 0.3f);
        titleOutline.effectDistance = new Vector2(1, -1);

        // === CloseButton ===
        GameObject closeBtnGO = CreateChild(contentGO, "CloseButton");
        RectTransform closeRect = closeBtnGO.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-30, -30);
        closeRect.sizeDelta = new Vector2(50, 50);
        Image closeImg = closeBtnGO.AddComponent<Image>();
        if (closeBtnSprite != null)
        {
            closeImg.sprite = closeBtnSprite;
            closeImg.color = Color.white;
        }
        else
        {
            closeImg.color = new Color(0.8f, 0.2f, 0.2f);
        }
        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeImg;

        // === GoldDisplay ===
        GameObject goldGO = CreateChild(contentGO, "GoldDisplay");
        RectTransform goldRect = goldGO.GetComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0, 1);
        goldRect.anchorMax = new Vector2(0, 1);
        goldRect.anchoredPosition = new Vector2(100, -35);
        goldRect.sizeDelta = new Vector2(200, 40);
        Text goldText = goldGO.AddComponent<Text>();
        goldText.text = "金币: 0";
        goldText.fontSize = 22;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = new Color(1f, 0.85f, 0.2f);
        goldText.font = defaultFont;

        // === Tabs ===
        GameObject tabsGO = CreateChild(contentGO, "Tabs");
        RectTransform tabsRect = tabsGO.GetComponent<RectTransform>();
        tabsRect.anchorMin = new Vector2(0, 1);
        tabsRect.anchorMax = new Vector2(1, 1);
        tabsRect.anchoredPosition = new Vector2(0, -85);
        tabsRect.sizeDelta = new Vector2(0, 45);
        HorizontalLayoutGroup tabsLayout = tabsGO.AddComponent<HorizontalLayoutGroup>();
        tabsLayout.childAlignment = TextAnchor.MiddleCenter;
        tabsLayout.spacing = 5;
        tabsLayout.childForceExpandWidth = false;
        tabsLayout.padding = new RectOffset(5, 5, 0, 0);

        // 7个Tab，按用户指定顺序：全部 → 武器 → 头盔 → 护甲 → 护腿 → 饰品 → 药水
        string[] tabNames = { "全部", "武器", "头盔", "护甲", "护腿", "饰品", "药水" };
        Button[] tabButtons = new Button[tabNames.Length];
        for (int i = 0; i < tabNames.Length; i++)
        {
            tabButtons[i] = CreateTabButton(tabsGO, tabNames[i], 65, tabActiveSprite, tabInactiveSprite, defaultFont);
        }

        // === ItemsContainer (ScrollRect) ===
        GameObject itemsGO = CreateChild(contentGO, "ItemsContainer");
        RectTransform itemsRect = itemsGO.GetComponent<RectTransform>();
        itemsRect.anchorMin = new Vector2(0.02f, 0.05f);
        itemsRect.anchorMax = new Vector2(0.98f, 0.82f);
        itemsRect.offsetMin = Vector2.zero;
        itemsRect.offsetMax = Vector2.zero;

        ScrollRect scrollRect = itemsGO.AddComponent<ScrollRect>();
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        scrollRect.scrollSensitivity = 40f;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;

        // Viewport
        GameObject viewportGO = CreateChild(itemsGO, "Viewport");
        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        Image viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0.01f);
        Mask mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        scrollRect.viewport = viewportRect;

        // Content (滚动内容)
        GameObject scrollContentGO = CreateChild(viewportGO, "Content");
        RectTransform scrollContentRect = scrollContentGO.GetComponent<RectTransform>();
        scrollContentRect.anchorMin = new Vector2(0, 1);
        scrollContentRect.anchorMax = new Vector2(1, 1);
        scrollContentRect.pivot = new Vector2(0.5f, 1);
        scrollContentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = scrollContentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter csf = scrollContentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = scrollContentRect;

        // === ToastContainer (默认隐藏) ===
        GameObject toastGO = CreateChild(contentGO, "ToastContainer");
        RectTransform toastRect = toastGO.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 0.7f);
        toastRect.anchorMax = new Vector2(0.5f, 0.7f);
        toastRect.sizeDelta = new Vector2(400, 60);
        Image toastBg = toastGO.AddComponent<Image>();
        toastBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        GameObject toastTextGO = CreateChild(toastGO, "ToastText");
        RectTransform toastTextRect = toastTextGO.GetComponent<RectTransform>();
        toastTextRect.anchorMin = Vector2.zero;
        toastTextRect.anchorMax = Vector2.one;
        toastTextRect.offsetMin = new Vector2(15, 5);
        toastTextRect.offsetMax = new Vector2(-15, -5);
        Text toastText = toastTextGO.AddComponent<Text>();
        toastText.fontSize = 24;
        toastText.alignment = TextAnchor.MiddleCenter;
        toastText.color = Color.white;
        toastText.font = defaultFont;
        Outline toastOutline = toastTextGO.AddComponent<Outline>();
        toastOutline.effectColor = new Color(0, 0, 0, 0.5f);
        toastOutline.effectDistance = new Vector2(1, -1);
        toastGO.SetActive(false);

        // === 挂载 ShopPanel 并绑定引用 ===
        MoShou.UI.ShopPanel shopPanel = panelGO.AddComponent<MoShou.UI.ShopPanel>();
        shopPanel.titleText = titleText;
        shopPanel.closeButton = closeBtn;
        shopPanel.goldText = goldText;
        shopPanel.itemsContainer = scrollContentGO.transform;
        shopPanel.allTab = tabButtons[0];         // 全部
        shopPanel.weaponTab = tabButtons[1];      // 武器
        shopPanel.helmetTab = tabButtons[2];      // 头盔
        shopPanel.armorTab = tabButtons[3];       // 护甲
        shopPanel.pantsTab = tabButtons[4];       // 护腿
        shopPanel.accessoryTab = tabButtons[5];   // 饰品
        shopPanel.consumableTab = tabButtons[6];  // 药水

        Debug.Log($"[ShopPrefabCreator] Tab数量: {tabButtons.Length}, 名称: {string.Join(", ", tabNames)}");

        // === 保存为Prefab ===
        panelGO.SetActive(false); // 默认隐藏
        string path = "Assets/Resources/Prefabs/UI/ShopPanel.prefab";
        PrefabUtility.SaveAsPrefabAsset(panelGO, path);
        Object.DestroyImmediate(panelGO);
        Debug.Log($"[ShopPrefabCreator] ShopPanel Prefab 已创建: {path}");
    }

    #region 工具方法

    static Button CreateTabButton(GameObject parent, string text, float width,
        Sprite activeSprite, Sprite inactiveSprite, Font font)
    {
        GameObject btnGO = CreateChild(parent, $"Tab_{text}");
        LayoutElement layout = btnGO.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = 40;

        Image btnImg = btnGO.AddComponent<Image>();
        if (inactiveSprite != null)
        {
            btnImg.sprite = inactiveSprite;
            btnImg.color = Color.white;
        }
        else
        {
            btnImg.color = new Color(0.3f, 0.3f, 0.35f);
        }

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        GameObject textGO = CreateChild(btnGO, "Text");
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text btnText = textGO.AddComponent<Text>();
        btnText.text = text;
        btnText.fontSize = 16;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = font;

        return btn;
    }

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
