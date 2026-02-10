using UnityEngine;
using UnityEngine.UI;
using MoShou.Systems;
using MoShou.Data;

/// <summary>
/// 简化版背包面板 - 接入InventoryManager显示实际物品
/// 每个格子显示：颜色图标 + 物品名称 + 数量 + 已装备标记
/// 点击格子可穿戴/丢弃/售卖装备
/// </summary>
public class SimpleInventoryPanel : MonoBehaviour
{
    public static SimpleInventoryPanel Instance { get; set; }

    [Header("运行时设置的引用")]
    public Transform slotsContainer;
    public Text goldText;
    public Text capacityText;

    private bool isInitialized = false;

    // 格子UI缓存
    private Image[] slotIcons;
    private Image[] slotBorders;     // 边框（已装备高亮）
    private Text[] slotCountTexts;   // 右下角数量
    private Text[] slotNameTexts;    // 物品名称
    private Text[] slotEquipTags;    // 已装备标记

    // 操作菜单
    private GameObject actionMenu;
    private int selectedSlotIndex = -1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Initialize();
    }

    void OnEnable()
    {
        // 订阅背包变化事件
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        }
        RefreshUI();
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        // 缓存格子引用
        if (slotsContainer != null)
        {
            int slotCount = slotsContainer.childCount;
            slotIcons = new Image[slotCount];
            slotBorders = new Image[slotCount];
            slotCountTexts = new Text[slotCount];
            slotNameTexts = new Text[slotCount];
            slotEquipTags = new Text[slotCount];

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            for (int i = 0; i < slotCount; i++)
            {
                Transform slot = slotsContainer.GetChild(i);

                // ===== 图标Image（填充整个格子，留边距） =====
                Transform iconTf = slot.Find("Icon");
                if (iconTf == null)
                {
                    GameObject iconGO = new GameObject("Icon");
                    iconGO.transform.SetParent(slot, false);
                    RectTransform iconRect = iconGO.AddComponent<RectTransform>();
                    iconRect.anchorMin = Vector2.zero;
                    iconRect.anchorMax = Vector2.one;
                    iconRect.offsetMin = new Vector2(4, 4);
                    iconRect.offsetMax = new Vector2(-4, -4);
                    slotIcons[i] = iconGO.AddComponent<Image>();
                    slotIcons[i].color = Color.clear;
                }
                else
                {
                    slotIcons[i] = iconTf.GetComponent<Image>();
                }
                // ★ 关键：Icon图片不拦截点击，让Button能收到事件
                slotIcons[i].raycastTarget = false;

                // ===== 物品名称Text（格子上半部分居中） =====
                Transform nameTf = slot.Find("ItemName");
                if (nameTf == null)
                {
                    GameObject nameGO = new GameObject("ItemName");
                    nameGO.transform.SetParent(slot, false);
                    RectTransform nameRect = nameGO.AddComponent<RectTransform>();
                    nameRect.anchorMin = new Vector2(0, 0.45f);
                    nameRect.anchorMax = new Vector2(1, 1f);
                    nameRect.offsetMin = new Vector2(2, 0);
                    nameRect.offsetMax = new Vector2(-2, -2);
                    slotNameTexts[i] = nameGO.AddComponent<Text>();
                    slotNameTexts[i].fontSize = 11;
                    slotNameTexts[i].alignment = TextAnchor.MiddleCenter;
                    slotNameTexts[i].color = Color.white;
                    slotNameTexts[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                    slotNameTexts[i].verticalOverflow = VerticalWrapMode.Truncate;
                    slotNameTexts[i].raycastTarget = false;
                    if (defaultFont != null) slotNameTexts[i].font = defaultFont;

                    // 描边让文字清晰
                    Outline nameOutline = nameGO.AddComponent<Outline>();
                    nameOutline.effectColor = Color.black;
                    nameOutline.effectDistance = new Vector2(1, -1);
                }
                else
                {
                    slotNameTexts[i] = nameTf.GetComponent<Text>();
                }
                // ★ 名称文字不拦截点击
                slotNameTexts[i].raycastTarget = false;

                // ===== 数量Text（右下角） =====
                Transform countTf = slot.Find("Count");
                if (countTf == null)
                {
                    GameObject countGO = new GameObject("Count");
                    countGO.transform.SetParent(slot, false);
                    RectTransform countRect = countGO.AddComponent<RectTransform>();
                    countRect.anchorMin = new Vector2(0.5f, 0);
                    countRect.anchorMax = new Vector2(1, 0.35f);
                    countRect.offsetMin = Vector2.zero;
                    countRect.offsetMax = Vector2.zero;
                    slotCountTexts[i] = countGO.AddComponent<Text>();
                    slotCountTexts[i].fontSize = 13;
                    slotCountTexts[i].alignment = TextAnchor.LowerRight;
                    slotCountTexts[i].color = Color.white;
                    slotCountTexts[i].raycastTarget = false;
                    if (defaultFont != null) slotCountTexts[i].font = defaultFont;

                    // 描边
                    Outline outline = countGO.AddComponent<Outline>();
                    outline.effectColor = Color.black;
                    outline.effectDistance = new Vector2(1, -1);
                }
                else
                {
                    slotCountTexts[i] = countTf.GetComponent<Text>();
                }
                // ★ 数量文字不拦截点击
                slotCountTexts[i].raycastTarget = false;

                // ===== 已装备标记（左上角绿色小标签） =====
                Transform equipTagTf = slot.Find("EquipTag");
                if (equipTagTf == null)
                {
                    // 标签背景
                    GameObject tagGO = new GameObject("EquipTag");
                    tagGO.transform.SetParent(slot, false);
                    RectTransform tagRect = tagGO.AddComponent<RectTransform>();
                    tagRect.anchorMin = new Vector2(0, 1);
                    tagRect.anchorMax = new Vector2(0, 1);
                    tagRect.anchoredPosition = new Vector2(22, -8);
                    tagRect.sizeDelta = new Vector2(38, 16);
                    Image tagBg = tagGO.AddComponent<Image>();
                    tagBg.color = new Color(0.1f, 0.7f, 0.2f, 0.85f);
                    tagBg.raycastTarget = false;

                    // 标签文字
                    GameObject tagTextGO = new GameObject("Text");
                    tagTextGO.transform.SetParent(tagGO.transform, false);
                    RectTransform ttRect = tagTextGO.AddComponent<RectTransform>();
                    ttRect.anchorMin = Vector2.zero;
                    ttRect.anchorMax = Vector2.one;
                    ttRect.offsetMin = Vector2.zero;
                    ttRect.offsetMax = Vector2.zero;
                    slotEquipTags[i] = tagTextGO.AddComponent<Text>();
                    slotEquipTags[i].text = "装备中";
                    slotEquipTags[i].fontSize = 10;
                    slotEquipTags[i].alignment = TextAnchor.MiddleCenter;
                    slotEquipTags[i].color = Color.white;
                    slotEquipTags[i].raycastTarget = false;
                    if (defaultFont != null) slotEquipTags[i].font = defaultFont;

                    tagGO.SetActive(false); // 默认隐藏
                }
                else
                {
                    slotEquipTags[i] = equipTagTf.GetComponentInChildren<Text>();
                }

                // ===== 装备高亮边框 =====
                Transform borderTf = slot.Find("EquipBorder");
                if (borderTf == null)
                {
                    GameObject borderGO = new GameObject("EquipBorder");
                    borderGO.transform.SetParent(slot, false);
                    RectTransform borderRect = borderGO.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.offsetMin = Vector2.zero;
                    borderRect.offsetMax = Vector2.zero;
                    slotBorders[i] = borderGO.AddComponent<Image>();
                    slotBorders[i].color = new Color(0.2f, 0.8f, 0.3f, 0.6f);
                    slotBorders[i].raycastTarget = false;
                    // 只显示边框，不填充
                    Outline borderOutline = borderGO.AddComponent<Outline>();
                    borderOutline.effectColor = new Color(0.2f, 0.9f, 0.3f, 0.8f);
                    borderOutline.effectDistance = new Vector2(2, -2);
                    slotBorders[i].color = Color.clear; // 背景透明，只有outline
                    borderGO.SetActive(false); // 默认隐藏
                }
                else
                {
                    slotBorders[i] = borderTf.GetComponent<Image>();
                }

                // ===== 给格子添加点击事件（穿戴装备） =====
                int slotIndex = i;
                Button slotBtn = slot.GetComponent<Button>();
                if (slotBtn == null)
                {
                    slotBtn = slot.gameObject.AddComponent<Button>();
                    slotBtn.targetGraphic = slot.GetComponent<Image>();
                }
                slotBtn.onClick.RemoveAllListeners();
                slotBtn.onClick.AddListener(() => OnSlotClicked(slotIndex));
            }
        }

        RefreshUI();
    }

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    public void RefreshUI()
    {
        // 更新金币显示
        if (goldText != null && GameManager.Instance != null)
        {
            goldText.text = $"金币: {GameManager.Instance.SessionGold}";
        }

        // 更新格子
        UpdateSlots();

        // 更新容量显示
        if (capacityText != null)
        {
            int usedSlots = 0;
            int maxSlots = 100;
            if (InventoryManager.Instance != null)
            {
                usedSlots = InventoryManager.Instance.UsedSlots;
                maxSlots = InventoryManager.Instance.MaxSlots;
            }
            capacityText.text = $"{usedSlots}/{maxSlots}";
        }
    }

    /// <summary>
    /// 更新所有格子 - 从InventoryManager读取数据
    /// </summary>
    void UpdateSlots()
    {
        if (slotsContainer == null || slotIcons == null) return;

        // ★ 已装备的物品会从背包移除(UseItem→RemoveFromSlot)，
        // 背包里剩余的同名装备是独立副本，不需要"装备中"标记。
        // 所以不再标记任何背包物品为"装备中"。

        for (int i = 0; i < slotIcons.Length; i++)
        {
            InventoryItem item = null;
            if (InventoryManager.Instance != null)
            {
                item = InventoryManager.Instance.GetItem(i);
            }

            if (item != null && item.count > 0)
            {
                // 有物品 - 显示颜色图标
                slotIcons[i].color = GetItemColor(item.itemId);
                slotIcons[i].gameObject.SetActive(true);

                // 显示物品名称
                if (slotNameTexts[i] != null)
                {
                    string displayName = GetItemDisplayName(item);
                    slotNameTexts[i].text = displayName;
                    slotNameTexts[i].color = GetItemNameColor(item);
                }

                // 显示数量（大于1才显示）
                if (slotCountTexts[i] != null)
                {
                    slotCountTexts[i].text = item.count > 1 ? item.count.ToString() : "";
                }

                // 隐藏装备标记（背包里的物品都是未穿戴的）
                if (slotEquipTags[i] != null)
                {
                    slotEquipTags[i].transform.parent.gameObject.SetActive(false);
                }
                if (slotBorders[i] != null)
                {
                    slotBorders[i].gameObject.SetActive(false);
                }
            }
            else
            {
                // 空格子
                slotIcons[i].color = Color.clear;
                if (slotNameTexts[i] != null)
                {
                    slotNameTexts[i].text = "";
                }
                if (slotCountTexts[i] != null)
                {
                    slotCountTexts[i].text = "";
                }
                if (slotEquipTags[i] != null)
                {
                    slotEquipTags[i].transform.parent.gameObject.SetActive(false);
                }
                if (slotBorders[i] != null)
                {
                    slotBorders[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 获取物品显示名称
    /// 优先从 equipmentData 获取，其次从 EquipmentManager 查询，最后用ID推断
    /// </summary>
    string GetItemDisplayName(InventoryItem item)
    {
        // 1. 直接从装备数据获取名称
        if (item.equipmentData != null && !string.IsNullOrEmpty(item.equipmentData.name))
        {
            return item.equipmentData.name;
        }

        // 2. 从EquipmentManager配置库查询
        if (EquipmentManager.Instance != null)
        {
            Equipment config = EquipmentManager.Instance.GetEquipmentConfig(item.itemId);
            if (config != null && !string.IsNullOrEmpty(config.name))
            {
                return config.name;
            }
        }

        // 3. 根据物品ID前缀推断通用名称
        string id = item.itemId;
        if (id.StartsWith("WPN")) return "武器";
        if (id.StartsWith("ARM")) return "护甲";
        if (id.StartsWith("HLM")) return "头盔";
        if (id.StartsWith("POTION_HP")) return "生命药水";
        if (id.StartsWith("POTION_MP")) return "魔法药水";
        if (id.StartsWith("POTION")) return "药水";
        if (id.StartsWith("CON")) return "消耗品";
        if (id.StartsWith("MAT")) return "材料";

        // 4. 直接显示ID
        return id;
    }

    /// <summary>
    /// 获取物品名称颜色（按品质/类型区分）
    /// </summary>
    Color GetItemNameColor(InventoryItem item)
    {
        // 装备按品质颜色
        if (item.equipmentData != null)
        {
            return GetQualityColor(item.equipmentData.quality);
        }

        // 非装备物品 - 尝试从EquipmentManager获取品质
        if (EquipmentManager.Instance != null)
        {
            Equipment config = EquipmentManager.Instance.GetEquipmentConfig(item.itemId);
            if (config != null)
            {
                return GetQualityColor(config.quality);
            }
        }

        // 消耗品/药水 - 绿色
        if (item.itemId.StartsWith("POTION") || item.itemId.StartsWith("CON"))
            return new Color(0.3f, 1f, 0.3f);

        return Color.white;
    }

    /// <summary>
    /// 品质对应颜色
    /// </summary>
    Color GetQualityColor(EquipmentQuality quality)
    {
        switch (quality)
        {
            case EquipmentQuality.White:  return Color.white;
            case EquipmentQuality.Green:  return new Color(0.3f, 1f, 0.3f);
            case EquipmentQuality.Blue:   return new Color(0.4f, 0.7f, 1f);
            case EquipmentQuality.Purple: return new Color(0.8f, 0.5f, 1f);
            case EquipmentQuality.Orange: return new Color(1f, 0.6f, 0.2f);
            default: return Color.white;
        }
    }

    /// <summary>
    /// 点击格子 - 弹出操作菜单
    /// </summary>
    void OnSlotClicked(int slotIndex)
    {
        if (InventoryManager.Instance == null) return;

        InventoryItem item = InventoryManager.Instance.GetItem(slotIndex);
        if (item == null)
        {
            HideActionMenu();
            return;
        }

        selectedSlotIndex = slotIndex;
        ShowActionMenu(slotIndex, item);
    }

    // ===== 操作菜单系统 =====

    /// <summary>
    /// 显示操作菜单
    /// </summary>
    void ShowActionMenu(int slotIndex, InventoryItem item)
    {
        if (actionMenu == null)
        {
            CreateActionMenu();
        }

        actionMenu.SetActive(true);

        // 定位到点击格子旁边
        RectTransform menuRect = actionMenu.GetComponent<RectTransform>();
        if (slotIndex < slotIcons.Length)
        {
            RectTransform slotRect = slotsContainer.GetChild(slotIndex).GetComponent<RectTransform>();
            // 菜单放在格子右侧
            menuRect.position = slotRect.position + new Vector3(90, 0, 0);
        }

        // 更新按钮文字
        bool isEquipment = item.equipmentData != null;
        Transform useBtn = actionMenu.transform.Find("UseBtn");
        if (useBtn != null)
        {
            Text btnText = useBtn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = isEquipment ? "穿戴" : "使用";
            }
        }
    }

    /// <summary>
    /// 隐藏操作菜单
    /// </summary>
    void HideActionMenu()
    {
        if (actionMenu != null)
        {
            actionMenu.SetActive(false);
        }
        selectedSlotIndex = -1;
    }

    /// <summary>
    /// 创建操作菜单UI
    /// </summary>
    void CreateActionMenu()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        actionMenu = new GameObject("ActionMenu");
        actionMenu.transform.SetParent(transform, false);
        RectTransform menuRect = actionMenu.AddComponent<RectTransform>();
        menuRect.sizeDelta = new Vector2(100, 130);

        // 菜单背景
        Image menuBg = actionMenu.AddComponent<Image>();
        menuBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // 添加Outline边框
        Outline menuOutline = actionMenu.AddComponent<Outline>();
        menuOutline.effectColor = new Color(0.5f, 0.5f, 0.6f);
        menuOutline.effectDistance = new Vector2(2, -2);

        // 穿戴/使用 按钮
        CreateMenuButton(actionMenu.transform, "UseBtn", "穿戴", new Vector2(0, 40),
            new Color(0.2f, 0.6f, 0.3f), defaultFont, OnActionUse);

        // 丢弃 按钮
        CreateMenuButton(actionMenu.transform, "DropBtn", "丢弃", new Vector2(0, 0),
            new Color(0.6f, 0.3f, 0.2f), defaultFont, OnActionDrop);

        // 售卖 按钮
        CreateMenuButton(actionMenu.transform, "SellBtn", "售卖", new Vector2(0, -40),
            new Color(0.6f, 0.6f, 0.2f), defaultFont, OnActionSell);

        actionMenu.SetActive(false);
    }

    void CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Color color, Font font, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        RectTransform btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchoredPosition = pos;
        btnRect.sizeDelta = new Vector2(90, 32);

        Image btnBg = btnGO.AddComponent<Image>();
        btnBg.color = color;

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnBg;

        // 高亮色
        var colors = btn.colors;
        colors.highlightedColor = new Color(color.r + 0.15f, color.g + 0.15f, color.b + 0.15f);
        colors.pressedColor = new Color(color.r - 0.1f, color.g - 0.1f, color.b - 0.1f);
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        // 文字
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text text = textGO.AddComponent<Text>();
        text.text = label;
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        if (font != null) text.font = font;
    }

    /// <summary>
    /// 穿戴/使用
    /// </summary>
    void OnActionUse()
    {
        if (selectedSlotIndex < 0 || InventoryManager.Instance == null)
        {
            HideActionMenu();
            return;
        }

        InventoryItem item = InventoryManager.Instance.GetItem(selectedSlotIndex);
        if (item == null)
        {
            HideActionMenu();
            return;
        }

        string itemName = GetItemDisplayName(item);
        bool used = InventoryManager.Instance.UseItem(selectedSlotIndex);
        if (used)
        {
            Debug.Log($"[背包] 穿戴装备: {itemName} ({item.itemId})");
            if (MoShou.UI.SimpleEquipmentPanel.Instance != null)
            {
                MoShou.UI.SimpleEquipmentPanel.Instance.RefreshUI();
            }
        }
        else
        {
            Debug.Log($"[背包] 物品无法使用: {itemName} ({item.itemId})");
        }

        HideActionMenu();
        RefreshUI();
    }

    /// <summary>
    /// 丢弃物品
    /// </summary>
    void OnActionDrop()
    {
        if (selectedSlotIndex < 0 || InventoryManager.Instance == null)
        {
            HideActionMenu();
            return;
        }

        InventoryItem item = InventoryManager.Instance.GetItem(selectedSlotIndex);
        if (item == null)
        {
            HideActionMenu();
            return;
        }

        string itemName = GetItemDisplayName(item);
        InventoryManager.Instance.RemoveFromSlot(selectedSlotIndex, 1);
        Debug.Log($"[背包] 丢弃物品: {itemName}");

        HideActionMenu();
        RefreshUI();
    }

    /// <summary>
    /// 售卖物品
    /// </summary>
    void OnActionSell()
    {
        if (selectedSlotIndex < 0 || InventoryManager.Instance == null)
        {
            HideActionMenu();
            return;
        }

        InventoryItem item = InventoryManager.Instance.GetItem(selectedSlotIndex);
        if (item == null)
        {
            HideActionMenu();
            return;
        }

        string itemName = GetItemDisplayName(item);

        // 计算售价（装备按品质，其他固定）
        int sellPrice = 10; // 默认售价
        if (item.equipmentData != null)
        {
            switch (item.equipmentData.quality)
            {
                case EquipmentQuality.White:  sellPrice = 10; break;
                case EquipmentQuality.Green:  sellPrice = 30; break;
                case EquipmentQuality.Blue:   sellPrice = 80; break;
                case EquipmentQuality.Purple: sellPrice = 200; break;
                case EquipmentQuality.Orange: sellPrice = 500; break;
            }
        }

        // 获得金币
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(sellPrice);
        }

        // 移除物品
        InventoryManager.Instance.RemoveFromSlot(selectedSlotIndex, 1);
        Debug.Log($"[背包] 售卖物品: {itemName} 获得 {sellPrice} 金币");

        HideActionMenu();
        RefreshUI();
    }

    /// <summary>
    /// 根据物品ID获取颜色（简易分类）
    /// </summary>
    Color GetItemColor(string itemId)
    {
        if (itemId.StartsWith("WPN")) return new Color(0.9f, 0.4f, 0.2f, 1f);    // 武器-橙色
        if (itemId.StartsWith("ARM")) return new Color(0.3f, 0.5f, 0.9f, 1f);    // 护甲-蓝色
        if (itemId.StartsWith("HLM")) return new Color(0.6f, 0.6f, 0.9f, 1f);    // 头盔-浅蓝
        if (itemId.StartsWith("POTION")) return new Color(0.3f, 0.9f, 0.3f, 1f); // 药水-绿色
        if (itemId.StartsWith("CON")) return new Color(0.3f, 0.9f, 0.3f, 1f);    // 消耗品-绿色
        return new Color(0.7f, 0.7f, 0.7f, 1f); // 默认-灰色
    }

    public void Show()
    {
        gameObject.SetActive(true);
        HideActionMenu();
        RefreshUI();
    }

    public void Hide()
    {
        HideActionMenu();
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            Hide();
        else
            Show();
    }
}
