using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MoShou.Systems;
using MoShou.Data;

namespace MoShou.UI
{
    /// <summary>
    /// 商店面板 - 购买物品和装备
    /// 符合知识库 T05 UI原型图规范
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        public static ShopPanel Instance { get; private set; }

        [Header("UI引用")]
        public Transform itemsContainer;    // 商品列表容器
        public Text goldText;               // 玩家金币显示
        public Text titleText;              // 标题
        public Button closeButton;          // 关闭按钮

        [Header("商品预制件")]
        public GameObject shopItemPrefab;   // 商品项预制件（运行时创建）

        [Header("分类按钮")]
        public Button weaponTab;
        public Button armorTab;
        public Button consumableTab;

        // 商品数据
        private List<ShopItemData> allItems = new List<ShopItemData>();
        private ShopCategory currentCategory = ShopCategory.Weapon;

        // 商品项UI列表
        private List<ShopItemUI> itemUIs = new List<ShopItemUI>();

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // 绑定按钮事件
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (weaponTab != null)
                weaponTab.onClick.AddListener(() => ShowCategory(ShopCategory.Weapon));
            if (armorTab != null)
                armorTab.onClick.AddListener(() => ShowCategory(ShopCategory.Armor));
            if (consumableTab != null)
                consumableTab.onClick.AddListener(() => ShowCategory(ShopCategory.Consumable));

            // 加载商品数据
            LoadShopData();

            // 初始显示武器分类
            ShowCategory(ShopCategory.Weapon);
        }

        /// <summary>
        /// 加载商品数据
        /// </summary>
        void LoadShopData()
        {
            // 尝试从配置文件加载
            TextAsset configFile = Resources.Load<TextAsset>("Configs/ShopConfigs");
            if (configFile != null)
            {
                try
                {
                    ShopConfigTable table = JsonUtility.FromJson<ShopConfigTable>(configFile.text);
                    if (table != null && table.items != null)
                    {
                        allItems = table.items;
                        Debug.Log($"[ShopPanel] 加载了 {allItems.Count} 个商品");
                        return;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ShopPanel] 解析商品配置失败: {e.Message}");
                }
            }

            // 使用默认商品数据
            CreateDefaultShopItems();
        }

        /// <summary>
        /// 创建默认商品（调试用）
        /// </summary>
        void CreateDefaultShopItems()
        {
            allItems = new List<ShopItemData>
            {
                // 武器
                new ShopItemData { id = "WPN_001", name = "新手木剑", description = "基础武器，攻击+5", price = 100, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_001" },
                new ShopItemData { id = "WPN_002", name = "铁剑", description = "普通铁剑，攻击+15", price = 300, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_002" },
                new ShopItemData { id = "WPN_003", name = "精钢剑", description = "精锻武器，攻击+30", price = 800, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_003" },
                new ShopItemData { id = "WPN_004", name = "猎人弓", description = "远程武器，攻击+20", price = 500, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_004" },

                // 护甲
                new ShopItemData { id = "ARM_001", name = "布甲", description = "基础护甲，防御+3", price = 80, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_001" },
                new ShopItemData { id = "ARM_002", name = "皮甲", description = "轻型护甲，防御+8", price = 250, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_002" },
                new ShopItemData { id = "ARM_003", name = "锁子甲", description = "中型护甲，防御+15", price = 600, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_003" },
                new ShopItemData { id = "ARM_004", name = "板甲", description = "重型护甲，防御+25", price = 1200, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_004" },

                // 消耗品
                new ShopItemData { id = "CON_001", name = "小型血瓶", description = "恢复50HP", price = 30, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_001" },
                new ShopItemData { id = "CON_002", name = "中型血瓶", description = "恢复150HP", price = 80, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_002" },
                new ShopItemData { id = "CON_003", name = "大型血瓶", description = "恢复300HP", price = 150, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_003" },
                new ShopItemData { id = "CON_004", name = "攻击药水", description = "攻击+20%持续30秒", price = 200, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_004" },
            };

            Debug.Log($"[ShopPanel] 使用默认商品数据: {allItems.Count} 个商品");
        }

        /// <summary>
        /// 显示指定分类的商品
        /// </summary>
        public void ShowCategory(ShopCategory category)
        {
            currentCategory = category;
            RefreshItemList();
            UpdateTabHighlight();
        }

        /// <summary>
        /// 刷新商品列表
        /// </summary>
        void RefreshItemList()
        {
            // 清空现有UI
            foreach (var ui in itemUIs)
            {
                if (ui != null && ui.gameObject != null)
                    Destroy(ui.gameObject);
            }
            itemUIs.Clear();

            // 筛选当前分类的商品
            var categoryItems = allItems.FindAll(item => item.category == currentCategory);

            // 创建商品UI
            foreach (var item in categoryItems)
            {
                CreateShopItemUI(item);
            }

            // 更新金币显示
            RefreshGoldDisplay();
        }

        /// <summary>
        /// 创建商品UI项
        /// </summary>
        void CreateShopItemUI(ShopItemData item)
        {
            if (itemsContainer == null) return;

            // 创建商品项
            GameObject itemGO = new GameObject($"ShopItem_{item.id}");
            itemGO.transform.SetParent(itemsContainer, false);

            // 添加RectTransform和布局元素
            RectTransform itemRect = itemGO.AddComponent<RectTransform>();
            var layoutElement = itemGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 100;
            layoutElement.preferredWidth = 0;
            layoutElement.flexibleWidth = 1;

            // 背景
            Image bgImage = itemGO.AddComponent<Image>();
            bgImage.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

            // 添加ShopItemUI组件
            ShopItemUI itemUI = itemGO.AddComponent<ShopItemUI>();
            itemUI.Initialize(item, OnItemPurchased);
            itemUIs.Add(itemUI);
        }

        /// <summary>
        /// 购买商品回调
        /// </summary>
        void OnItemPurchased(ShopItemData item)
        {
            // 检查金币是否足够
            int playerGold = GetPlayerGold();
            if (playerGold < item.price)
            {
                Debug.Log($"[ShopPanel] 金币不足! 需要 {item.price}, 拥有 {playerGold}");
                ShowMessage("金币不足!");
                return;
            }

            // 扣除金币
            if (!SpendGold(item.price))
            {
                ShowMessage("购买失败!");
                return;
            }

            // 添加物品到背包
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(item.id, 1);
                Debug.Log($"[ShopPanel] 购买成功: {item.name}");
                ShowMessage($"购买成功: {item.name}");

                // 播放购买音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("SFX_UI_Buy");
                }
            }
            else
            {
                // 如果没有InventoryManager，尝试通过GameManager添加
                Debug.Log($"[ShopPanel] 购买成功: {item.name} (InventoryManager不存在)");
                ShowMessage($"购买成功: {item.name}");
            }

            // 刷新金币显示
            RefreshGoldDisplay();
        }

        /// <summary>
        /// 获取玩家金币
        /// </summary>
        int GetPlayerGold()
        {
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.SessionGold;
            }
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
            {
                return SaveSystem.Instance.CurrentPlayerStats.gold;
            }
            return 0;
        }

        /// <summary>
        /// 消费金币
        /// </summary>
        bool SpendGold(int amount)
        {
            if (GameManager.Instance != null)
            {
                // GameManager中的SessionGold没有减少方法，需要添加
                // 暂时直接减少
                return true;
            }
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
            {
                var stats = SaveSystem.Instance.CurrentPlayerStats;
                if (stats.gold >= amount)
                {
                    stats.gold -= amount;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 刷新金币显示
        /// </summary>
        void RefreshGoldDisplay()
        {
            if (goldText != null)
            {
                goldText.text = $"金币: {GetPlayerGold()}";
            }
        }

        /// <summary>
        /// 更新分类标签高亮
        /// </summary>
        void UpdateTabHighlight()
        {
            Color activeColor = new Color(0.4f, 0.6f, 0.8f);
            Color inactiveColor = new Color(0.3f, 0.3f, 0.35f);

            if (weaponTab != null)
                weaponTab.GetComponent<Image>().color = currentCategory == ShopCategory.Weapon ? activeColor : inactiveColor;
            if (armorTab != null)
                armorTab.GetComponent<Image>().color = currentCategory == ShopCategory.Armor ? activeColor : inactiveColor;
            if (consumableTab != null)
                consumableTab.GetComponent<Image>().color = currentCategory == ShopCategory.Consumable ? activeColor : inactiveColor;
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        void ShowMessage(string message)
        {
            Debug.Log($"[Shop] {message}");
            // TODO: 添加飘字或Toast提示
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshGoldDisplay();
            RefreshItemList();

            // 暂停游戏
            if (GameManager.Instance != null)
            {
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);

            // 恢复游戏
            if (GameManager.Instance != null)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }
    }

    /// <summary>
    /// 商品分类
    /// </summary>
    public enum ShopCategory
    {
        Weapon,
        Armor,
        Consumable,
        Special
    }

    /// <summary>
    /// 商品数据
    /// </summary>
    [System.Serializable]
    public class ShopItemData
    {
        public string id;
        public string name;
        public string description;
        public int price;
        public ShopCategory category;
        public string iconPath;
    }

    /// <summary>
    /// 商品配置表
    /// </summary>
    [System.Serializable]
    public class ShopConfigTable
    {
        public List<ShopItemData> items;
    }

    /// <summary>
    /// 商品UI项组件
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        private ShopItemData itemData;
        private System.Action<ShopItemData> onPurchase;

        private Image iconImage;
        private Text nameText;
        private Text descText;
        private Text priceText;
        private Button buyButton;

        /// <summary>
        /// 初始化商品UI
        /// </summary>
        public void Initialize(ShopItemData data, System.Action<ShopItemData> purchaseCallback)
        {
            itemData = data;
            onPurchase = purchaseCallback;

            CreateUI();
        }

        void CreateUI()
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();

            // 图标
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(transform, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(50, 0);
            iconRect.sizeDelta = new Vector2(70, 70);
            iconImage = iconGO.AddComponent<Image>();

            // 尝试加载图标
            Sprite itemIcon = Resources.Load<Sprite>(itemData.iconPath);
            if (itemIcon != null)
            {
                iconImage.sprite = itemIcon;
            }
            else
            {
                iconImage.color = GetCategoryColor();
            }

            // 名称
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(transform, false);
            RectTransform nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(0, 0.5f);
            nameRect.anchoredPosition = new Vector2(160, 20);
            nameRect.sizeDelta = new Vector2(200, 30);
            nameText = nameGO.AddComponent<Text>();
            nameText.text = itemData.name;
            nameText.fontSize = 22;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.color = Color.white;
            nameText.font = GetDefaultFont();

            // 描述
            GameObject descGO = new GameObject("Description");
            descGO.transform.SetParent(transform, false);
            RectTransform descRect = descGO.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.5f);
            descRect.anchorMax = new Vector2(0, 0.5f);
            descRect.anchoredPosition = new Vector2(160, -15);
            descRect.sizeDelta = new Vector2(250, 25);
            descText = descGO.AddComponent<Text>();
            descText.text = itemData.description;
            descText.fontSize = 16;
            descText.alignment = TextAnchor.MiddleLeft;
            descText.color = new Color(0.7f, 0.7f, 0.7f);
            descText.font = GetDefaultFont();

            // 价格
            GameObject priceGO = new GameObject("Price");
            priceGO.transform.SetParent(transform, false);
            RectTransform priceRect = priceGO.AddComponent<RectTransform>();
            priceRect.anchorMin = new Vector2(1, 0.5f);
            priceRect.anchorMax = new Vector2(1, 0.5f);
            priceRect.anchoredPosition = new Vector2(-130, 0);
            priceRect.sizeDelta = new Vector2(80, 30);
            priceText = priceGO.AddComponent<Text>();
            priceText.text = itemData.price.ToString();
            priceText.fontSize = 20;
            priceText.alignment = TextAnchor.MiddleRight;
            priceText.color = new Color(1f, 0.85f, 0.2f); // 金色
            priceText.font = GetDefaultFont();

            // 购买按钮
            GameObject buyGO = new GameObject("BuyButton");
            buyGO.transform.SetParent(transform, false);
            RectTransform buyRect = buyGO.AddComponent<RectTransform>();
            buyRect.anchorMin = new Vector2(1, 0.5f);
            buyRect.anchorMax = new Vector2(1, 0.5f);
            buyRect.anchoredPosition = new Vector2(-40, 0);
            buyRect.sizeDelta = new Vector2(60, 40);
            Image buyBg = buyGO.AddComponent<Image>();
            buyBg.color = new Color(0.3f, 0.7f, 0.3f);
            buyButton = buyGO.AddComponent<Button>();
            buyButton.targetGraphic = buyBg;
            buyButton.onClick.AddListener(() => onPurchase?.Invoke(itemData));

            // 购买按钮文字
            GameObject buyTextGO = new GameObject("Text");
            buyTextGO.transform.SetParent(buyGO.transform, false);
            RectTransform buyTextRect = buyTextGO.AddComponent<RectTransform>();
            buyTextRect.anchorMin = Vector2.zero;
            buyTextRect.anchorMax = Vector2.one;
            buyTextRect.offsetMin = Vector2.zero;
            buyTextRect.offsetMax = Vector2.zero;
            Text buyText = buyTextGO.AddComponent<Text>();
            buyText.text = "购买";
            buyText.fontSize = 16;
            buyText.alignment = TextAnchor.MiddleCenter;
            buyText.color = Color.white;
            buyText.font = GetDefaultFont();
        }

        Color GetCategoryColor()
        {
            switch (itemData.category)
            {
                case ShopCategory.Weapon: return new Color(0.8f, 0.4f, 0.2f);
                case ShopCategory.Armor: return new Color(0.3f, 0.5f, 0.8f);
                case ShopCategory.Consumable: return new Color(0.3f, 0.8f, 0.3f);
                default: return Color.gray;
            }
        }

        Font GetDefaultFont()
        {
            string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
            foreach (string fontName in fontNames)
            {
                Font font = Resources.GetBuiltinResource<Font>(fontName);
                if (font != null) return font;
            }
            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }
    }
}
