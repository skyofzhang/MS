using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoShou.Systems;
using MoShou.Data;

namespace MoShou.UI
{
    /// <summary>
    /// 商店面板 - Prefab方式
    /// 面板布局由Prefab控制，代码只负责数据填充和事件绑定
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        public static ShopPanel Instance { get; set; }

        [Header("UI引用")]
        public Transform itemsContainer;    // 商品列表容器
        public Text goldText;               // 玩家金币显示
        public Text titleText;              // 标题
        public Button closeButton;          // 关闭按钮

        [Header("Toast提示")]
        private GameObject toastContainer;  // Toast容器
        private Text toastText;             // Toast文本
        private Image toastBg;              // Toast背景
        private Coroutine toastCoroutine;

        [Header("分类按钮 - 效果图6分类")]
        public Button allTab;           // 全部
        public Button armorTab;         // 护甲
        public Button helmetTab;        // 头盔
        public Button weaponTab;        // 武器
        public Button pantsTab;         // 护腿
        public Button consumableTab;    // 药水
        public Button accessoryTab;     // 饰品(戒指+项链)

        // 商品数据
        private List<ShopItemData> allItems = new List<ShopItemData>();
        private ShopCategory currentCategory = ShopCategory.All;

        // 商品卡片Prefab
        private GameObject shopItemCardPrefab;

        // 商品项UI列表
        private List<ShopItemCardUI> itemUIs = new List<ShopItemCardUI>();

        void Awake()
        {
            Instance = this;
            FindToastUI();
        }

        void Start()
        {
            // 绑定按钮事件
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (allTab != null)
                allTab.onClick.AddListener(() => ShowCategory(ShopCategory.All));
            if (armorTab != null)
                armorTab.onClick.AddListener(() => ShowCategory(ShopCategory.Armor));
            if (helmetTab != null)
                helmetTab.onClick.AddListener(() => ShowCategory(ShopCategory.Helmet));
            if (weaponTab != null)
                weaponTab.onClick.AddListener(() => ShowCategory(ShopCategory.Weapon));
            if (pantsTab != null)
                pantsTab.onClick.AddListener(() => ShowCategory(ShopCategory.Pants));
            if (consumableTab != null)
                consumableTab.onClick.AddListener(() => ShowCategory(ShopCategory.Consumable));
            if (accessoryTab != null)
                accessoryTab.onClick.AddListener(() => ShowCategory(ShopCategory.Accessory));

            // 加载商品卡片Prefab
            shopItemCardPrefab = Resources.Load<GameObject>("Prefabs/UI/ShopItemCard");
            if (shopItemCardPrefab == null)
            {
                Debug.LogError("[ShopPanel] 找不到 Prefabs/UI/ShopItemCard 预制体！");
            }

            // 加载商品数据
            LoadShopData();

            // 初始显示全部分类
            ShowCategory(ShopCategory.All);
        }

        /// <summary>
        /// 查找Prefab中的ToastContainer（如果Prefab中没有则运行时创建）
        /// </summary>
        void FindToastUI()
        {
            Transform toastT = transform.Find("Content/ToastContainer");
            if (toastT == null) toastT = FindDeepChild(transform, "ToastContainer");

            if (toastT != null)
            {
                toastContainer = toastT.gameObject;
                toastBg = toastContainer.GetComponent<Image>();
                Transform textT = toastT.Find("ToastText");
                if (textT != null) toastText = textT.GetComponent<Text>();
            }
            else
            {
                CreateToastUI();
            }
        }

        #region 数据加载

        void LoadShopData()
        {
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
            CreateDefaultShopItems();
        }

        void CreateDefaultShopItems()
        {
            allItems = new List<ShopItemData>
            {
                // 武器
                new ShopItemData { id = "WPN_001", name = "新手木剑", description = "基础武器，攻击+5", price = 100, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_001" },
                new ShopItemData { id = "WPN_002", name = "铁剑", description = "普通铁剑，攻击+15", price = 300, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_002" },
                new ShopItemData { id = "WPN_003", name = "精钢剑", description = "精锻武器，攻击+30", price = 800, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_003" },
                new ShopItemData { id = "WPN_004", name = "猎人弓", description = "远程武器，攻击+20", price = 500, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_004" },
                new ShopItemData { id = "WPN_005", name = "暗影之刃", description = "暗影魔剑，攻击+50，暴击+8%", price = 2000, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_005" },
                new ShopItemData { id = "WPN_006", name = "龙牙大剑", description = "传说神器，攻击+80，暴击+12%", price = 5000, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_006" },
                new ShopItemData { id = "WPN_007", name = "雷霆战弓", description = "雷电战弓，攻击+45，暴击+10%", price = 2500, category = ShopCategory.Weapon, iconPath = "Sprites/Items/WPN_007" },
                // 护甲
                new ShopItemData { id = "ARM_001", name = "布甲", description = "基础护甲，防御+3", price = 80, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_001" },
                new ShopItemData { id = "ARM_002", name = "皮甲", description = "轻型护甲，防御+8", price = 250, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_002" },
                new ShopItemData { id = "ARM_003", name = "锁子甲", description = "中型护甲，防御+15", price = 600, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_003" },
                new ShopItemData { id = "ARM_004", name = "板甲", description = "重型护甲，防御+25", price = 1200, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_004" },
                new ShopItemData { id = "ARM_005", name = "秘银甲", description = "秘银重甲，防御+35，HP+120", price = 3000, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_005" },
                new ShopItemData { id = "ARM_006", name = "龙鳞战甲", description = "传说战甲，防御+50，HP+200", price = 6000, category = ShopCategory.Armor, iconPath = "Sprites/Items/ARM_006" },
                // 头盔
                new ShopItemData { id = "HLM_001", name = "皮帽", description = "基础头盔，防御+2，HP+5", price = 60, category = ShopCategory.Helmet, iconPath = "Sprites/Items/HLM_001" },
                new ShopItemData { id = "HLM_002", name = "铁盔", description = "坚固铁盔，防御+5，HP+15", price = 200, category = ShopCategory.Helmet, iconPath = "Sprites/Items/HLM_002" },
                new ShopItemData { id = "HLM_003", name = "狮鹫头盔", description = "骑士头盔，防御+10，HP+30", price = 600, category = ShopCategory.Helmet, iconPath = "Sprites/Items/HLM_003" },
                new ShopItemData { id = "HLM_004", name = "暗金王冠", description = "暗金王冠，防御+15，HP+60，暴击+5%", price = 1800, category = ShopCategory.Helmet, iconPath = "Sprites/Items/HLM_004" },
                // 护腿
                new ShopItemData { id = "PNT_001", name = "布裤", description = "普通布裤，防御+2，HP+5", price = 60, category = ShopCategory.Pants, iconPath = "Sprites/Items/PNT_001" },
                new ShopItemData { id = "PNT_002", name = "皮裤", description = "结实皮裤，防御+5，HP+12", price = 200, category = ShopCategory.Pants, iconPath = "Sprites/Items/PNT_002" },
                new ShopItemData { id = "PNT_003", name = "铁甲护腿", description = "铁甲护腿，防御+10，HP+25", price = 500, category = ShopCategory.Pants, iconPath = "Sprites/Items/PNT_003" },
                new ShopItemData { id = "PNT_004", name = "暗影行者", description = "暗影护腿，防御+18，HP+50，暴击+4%", price = 1500, category = ShopCategory.Pants, iconPath = "Sprites/Items/PNT_004" },
                // 戒指
                new ShopItemData { id = "RNG_001", name = "铜戒指", description = "攻击+2，暴击+1%", price = 80, category = ShopCategory.Ring, iconPath = "Sprites/Items/RNG_001" },
                new ShopItemData { id = "RNG_002", name = "银戒指", description = "攻击+5，暴击+3%", price = 300, category = ShopCategory.Ring, iconPath = "Sprites/Items/RNG_002" },
                new ShopItemData { id = "RNG_003", name = "烈焰戒指", description = "烈焰之力，攻击+12，暴击+5%", price = 800, category = ShopCategory.Ring, iconPath = "Sprites/Items/RNG_003" },
                new ShopItemData { id = "RNG_004", name = "霜龙之戒", description = "霜龙神戒，攻击+20，暴击+8%", price = 2000, category = ShopCategory.Ring, iconPath = "Sprites/Items/RNG_004" },
                // 项链
                new ShopItemData { id = "NCK_001", name = "护身符", description = "防御+1，HP+10", price = 70, category = ShopCategory.Necklace, iconPath = "Sprites/Items/NCK_001" },
                new ShopItemData { id = "NCK_002", name = "生命吊坠", description = "防御+2，HP+30", price = 250, category = ShopCategory.Necklace, iconPath = "Sprites/Items/NCK_002" },
                new ShopItemData { id = "NCK_003", name = "守护者圣物", description = "守护圣物，防御+8，HP+60", price = 700, category = ShopCategory.Necklace, iconPath = "Sprites/Items/NCK_003" },
                new ShopItemData { id = "NCK_004", name = "不灭心脏", description = "传说之物，防御+12，HP+120，暴击+5%", price = 2200, category = ShopCategory.Necklace, iconPath = "Sprites/Items/NCK_004" },
                // 消耗品
                new ShopItemData { id = "CON_001", name = "小型血瓶", description = "恢复50HP", price = 30, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_001" },
                new ShopItemData { id = "CON_002", name = "中型血瓶", description = "恢复150HP", price = 80, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_002" },
                new ShopItemData { id = "CON_003", name = "大型血瓶", description = "恢复300HP", price = 150, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_003" },
                new ShopItemData { id = "CON_004", name = "攻击药水", description = "攻击+20%持续30秒", price = 200, category = ShopCategory.Consumable, iconPath = "Sprites/Items/CON_004" },
            };
            Debug.Log($"[ShopPanel] 使用默认商品数据: {allItems.Count} 个商品");
        }

        #endregion

        #region 分类与显示

        public void ShowCategory(ShopCategory category)
        {
            currentCategory = category;
            RefreshItemList();
            UpdateTabHighlight();
        }

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
            List<ShopItemData> categoryItems;
            if (currentCategory == ShopCategory.All)
            {
                categoryItems = new List<ShopItemData>(allItems);
            }
            else if (currentCategory == ShopCategory.Accessory)
            {
                categoryItems = allItems.FindAll(item =>
                    item.category == ShopCategory.Ring || item.category == ShopCategory.Necklace);
            }
            else
            {
                categoryItems = allItems.FindAll(item => item.category == currentCategory);
            }

            // 创建商品UI
            foreach (var item in categoryItems)
            {
                CreateShopItemUI(item);
            }

            RefreshGoldDisplay();
        }

        void CreateShopItemUI(ShopItemData item)
        {
            if (itemsContainer == null || shopItemCardPrefab == null) return;

            GameObject cardObj = Instantiate(shopItemCardPrefab, itemsContainer);
            ShopItemCardUI cardUI = cardObj.GetComponent<ShopItemCardUI>();

            if (cardUI == null)
            {
                Debug.LogWarning($"[ShopPanel] ShopItemCard预制体缺少ShopItemCardUI组件！{item.name}跳过");
                Destroy(cardObj);
                return;
            }

            // 加载物品图标
            Sprite itemIcon = Resources.Load<Sprite>(item.iconPath);
            if (itemIcon == null)
            {
                string itemId = System.IO.Path.GetFileName(item.iconPath);
                if (string.IsNullOrEmpty(itemId)) itemId = item.id;
                itemIcon = RuntimeIconGenerator.GetIcon(itemId);
            }

            cardUI.Setup(item, itemIcon, OnItemPurchased);
            itemUIs.Add(cardUI);
        }

        #endregion

        #region Tab高亮

        void UpdateTabHighlight()
        {
            Sprite tabActive = Resources.Load<Sprite>("Sprites/UI/Shop/UI_Shop_Tab_Active");
            Sprite tabInactive = Resources.Load<Sprite>("Sprites/UI/Shop/UI_Shop_Tab_Inactive");

            Color activeColor = new Color(0.4f, 0.6f, 0.8f);
            Color inactiveColor = new Color(0.3f, 0.3f, 0.35f);

            SetTabStyle(allTab, currentCategory == ShopCategory.All, tabActive, tabInactive, activeColor, inactiveColor);
            SetTabStyle(armorTab, currentCategory == ShopCategory.Armor, tabActive, tabInactive, activeColor, inactiveColor);
            SetTabStyle(helmetTab, currentCategory == ShopCategory.Helmet, tabActive, tabInactive, activeColor, inactiveColor);
            SetTabStyle(weaponTab, currentCategory == ShopCategory.Weapon, tabActive, tabInactive, activeColor, inactiveColor);
            SetTabStyle(pantsTab, currentCategory == ShopCategory.Pants, tabActive, tabInactive, activeColor, inactiveColor);
            SetTabStyle(consumableTab, currentCategory == ShopCategory.Consumable, tabActive, tabInactive, activeColor, inactiveColor);
            SetTabStyle(accessoryTab, currentCategory == ShopCategory.Accessory, tabActive, tabInactive, activeColor, inactiveColor);
        }

        void SetTabStyle(Button tab, bool isActive, Sprite activeSprite, Sprite inactiveSprite,
            Color activeColor, Color inactiveColor)
        {
            if (tab != null)
            {
                Image img = tab.GetComponent<Image>();
                if (img != null)
                {
                    if (activeSprite != null && inactiveSprite != null)
                    {
                        img.sprite = isActive ? activeSprite : inactiveSprite;
                        img.color = Color.white;
                    }
                    else
                    {
                        img.color = isActive ? activeColor : inactiveColor;
                    }
                }
            }
        }

        #endregion

        #region 购买逻辑

        void OnItemPurchased(ShopItemData item)
        {
            int playerGold = GetPlayerGold();
            if (playerGold < item.price)
            {
                ShowMessage("金币不足!", false);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonClick);
                return;
            }

            if (ConfirmDialog.Instance != null)
            {
                ConfirmDialog.Instance.ShowBuyConfirm(item.name, item.price, () =>
                {
                    ExecutePurchase(item);
                });
            }
            else
            {
                ExecutePurchase(item);
            }
        }

        void ExecutePurchase(ShopItemData item)
        {
            if (!SpendGold(item.price))
            {
                ShowMessage("购买失败!", false);
                return;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(item.id, 1);
            }

            Debug.Log($"[ShopPanel] 购买成功: {item.name}");
            ShowMessage($"购买成功: {item.name}", true);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.SFX.CoinPickup);

            RefreshGoldDisplay();
        }

        int GetPlayerGold()
        {
            if (GameManager.Instance != null)
                return GameManager.Instance.SessionGold;
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
                return SaveSystem.Instance.CurrentPlayerStats.gold;
            return 0;
        }

        bool SpendGold(int amount)
        {
            if (GameManager.Instance != null)
                return GameManager.Instance.SpendGold(amount);
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
                return SaveSystem.Instance.CurrentPlayerStats.SpendGold(amount);
            return false;
        }

        void RefreshGoldDisplay()
        {
            if (goldText != null)
                goldText.text = $"金币: {GetPlayerGold()}";
        }

        #endregion

        #region Toast提示

        void CreateToastUI()
        {
            toastContainer = new GameObject("ToastContainer");
            toastContainer.transform.SetParent(transform, false);
            RectTransform toastRect = toastContainer.AddComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.5f, 0.7f);
            toastRect.anchorMax = new Vector2(0.5f, 0.7f);
            toastRect.sizeDelta = new Vector2(400, 60);

            toastBg = toastContainer.AddComponent<Image>();
            toastBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            GameObject textGO = new GameObject("ToastText");
            textGO.transform.SetParent(toastContainer.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 5);
            textRect.offsetMax = new Vector2(-15, -5);

            toastText = textGO.AddComponent<Text>();
            toastText.fontSize = 24;
            toastText.alignment = TextAnchor.MiddleCenter;
            toastText.color = Color.white;
            toastText.font = GetDefaultFont();

            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            toastContainer.SetActive(false);
        }

        void ShowMessage(string message, bool isSuccess = true)
        {
            Debug.Log($"[Shop] {message}");

            if (toastContainer == null || toastText == null || toastBg == null)
                CreateToastUI();

            if (isSuccess)
            {
                toastBg.color = new Color(0.15f, 0.4f, 0.15f, 0.95f);
                toastText.color = new Color(0.8f, 1f, 0.8f);
            }
            else
            {
                toastBg.color = new Color(0.5f, 0.15f, 0.15f, 0.95f);
                toastText.color = new Color(1f, 0.8f, 0.8f);
            }

            toastText.text = message;

            if (toastCoroutine != null)
                StopCoroutine(toastCoroutine);

            toastCoroutine = StartCoroutine(ShowToastCoroutine());
        }

        IEnumerator ShowToastCoroutine()
        {
            toastContainer.SetActive(true);
            CanvasGroup cg = toastContainer.GetComponent<CanvasGroup>();
            if (cg == null) cg = toastContainer.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            cg.alpha = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.2f);
                yield return null;
            }
            cg.alpha = 1f;

            yield return new WaitForSecondsRealtime(1.5f);

            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                yield return null;
            }

            toastContainer.SetActive(false);
            toastCoroutine = null;
        }

        #endregion

        #region 面板控制

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshGoldDisplay();
            RefreshItemList();
            if (GameManager.Instance != null) Time.timeScale = 0f;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (GameManager.Instance != null) Time.timeScale = 1f;
        }

        public void Toggle()
        {
            if (gameObject.activeSelf) Hide();
            else Show();
        }

        #endregion

        #region 工具

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

        static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// 商品分类
    /// </summary>
    public enum ShopCategory
    {
        All,        // 全部
        Weapon,     // 武器
        Armor,      // 护甲
        Helmet,     // 头盔
        Pants,      // 护腿（合并到护甲）
        Ring,       // 戒指（合并到饰品）
        Necklace,   // 项链（合并到饰品）
        Consumable, // 药水
        Accessory,  // 饰品（戒指+项链）
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
}
