using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

        [Header("商品预制件")]
        public GameObject shopItemPrefab;   // 商品项预制件（运行时创建）

        [Header("分类按钮")]
        public Button weaponTab;
        public Button armorTab;
        public Button helmetTab;
        public Button pantsTab;
        public Button ringTab;
        public Button necklaceTab;
        public Button consumableTab;

        // 商品数据
        private List<ShopItemData> allItems = new List<ShopItemData>();
        private ShopCategory currentCategory = ShopCategory.Weapon;

        // 商品项UI列表
        private List<ShopItemUI> itemUIs = new List<ShopItemUI>();

        void Awake()
        {
            Instance = this;
            CreateToastUI();
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
            if (helmetTab != null)
                helmetTab.onClick.AddListener(() => ShowCategory(ShopCategory.Helmet));
            if (pantsTab != null)
                pantsTab.onClick.AddListener(() => ShowCategory(ShopCategory.Pants));
            if (ringTab != null)
                ringTab.onClick.AddListener(() => ShowCategory(ShopCategory.Ring));
            if (necklaceTab != null)
                necklaceTab.onClick.AddListener(() => ShowCategory(ShopCategory.Necklace));
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
        /// 购买商品回调 - 显示确认对话框
        /// </summary>
        void OnItemPurchased(ShopItemData item)
        {
            // 检查金币是否足够
            int playerGold = GetPlayerGold();
            if (playerGold < item.price)
            {
                Debug.Log($"[ShopPanel] 金币不足! 需要 {item.price}, 拥有 {playerGold}");
                ShowMessage("金币不足!", false);

                // 播放失败音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonClick);
                }
                return;
            }

            // 显示购买确认对话框
            if (ConfirmDialog.Instance != null)
            {
                ConfirmDialog.Instance.ShowBuyConfirm(item.name, item.price, () =>
                {
                    ExecutePurchase(item);
                });
            }
            else
            {
                // 如果没有ConfirmDialog，直接购买
                ExecutePurchase(item);
            }
        }

        /// <summary>
        /// 执行购买逻辑
        /// </summary>
        void ExecutePurchase(ShopItemData item)
        {
            // 扣除金币
            if (!SpendGold(item.price))
            {
                ShowMessage("购买失败!", false);
                return;
            }

            // 添加物品到背包
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(item.id, 1);
                Debug.Log($"[ShopPanel] 购买成功: {item.name}");
                ShowMessage($"购买成功: {item.name}", true);

                // 播放购买音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.SFX.CoinPickup);
                }
            }
            else
            {
                // 如果没有InventoryManager，尝试通过GameManager添加
                Debug.Log($"[ShopPanel] 购买成功: {item.name} (InventoryManager不存在)");
                ShowMessage($"购买成功: {item.name}", true);

                // 播放购买音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.SFX.CoinPickup);
                }
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
                return GameManager.Instance.SpendGold(amount);
            }
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
            {
                return SaveSystem.Instance.CurrentPlayerStats.SpendGold(amount);
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

            SetTabColor(weaponTab, currentCategory == ShopCategory.Weapon, activeColor, inactiveColor);
            SetTabColor(armorTab, currentCategory == ShopCategory.Armor, activeColor, inactiveColor);
            SetTabColor(helmetTab, currentCategory == ShopCategory.Helmet, activeColor, inactiveColor);
            SetTabColor(pantsTab, currentCategory == ShopCategory.Pants, activeColor, inactiveColor);
            SetTabColor(ringTab, currentCategory == ShopCategory.Ring, activeColor, inactiveColor);
            SetTabColor(necklaceTab, currentCategory == ShopCategory.Necklace, activeColor, inactiveColor);
            SetTabColor(consumableTab, currentCategory == ShopCategory.Consumable, activeColor, inactiveColor);
        }

        void SetTabColor(Button tab, bool isActive, Color activeColor, Color inactiveColor)
        {
            if (tab != null)
            {
                Image img = tab.GetComponent<Image>();
                if (img != null) img.color = isActive ? activeColor : inactiveColor;
            }
        }

        /// <summary>
        /// 创建Toast UI
        /// </summary>
        void CreateToastUI()
        {
            // Toast容器
            toastContainer = new GameObject("ToastContainer");
            toastContainer.transform.SetParent(transform, false);
            RectTransform toastRect = toastContainer.AddComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.5f, 0.7f);
            toastRect.anchorMax = new Vector2(0.5f, 0.7f);
            toastRect.sizeDelta = new Vector2(400, 60);

            // 背景
            toastBg = toastContainer.AddComponent<Image>();
            toastBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // 文本
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

            // 描边效果
            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            // 默认隐藏
            toastContainer.SetActive(false);
        }

        /// <summary>
        /// 显示消息（Toast提示）
        /// </summary>
        void ShowMessage(string message, bool isSuccess = true)
        {
            Debug.Log($"[Shop] {message}");

            if (toastContainer == null || toastText == null || toastBg == null)
            {
                CreateToastUI();
            }

            // 设置颜色
            if (isSuccess)
            {
                toastBg.color = new Color(0.15f, 0.4f, 0.15f, 0.95f); // 绿色背景
                toastText.color = new Color(0.8f, 1f, 0.8f);
            }
            else
            {
                toastBg.color = new Color(0.5f, 0.15f, 0.15f, 0.95f); // 红色背景
                toastText.color = new Color(1f, 0.8f, 0.8f);
            }

            toastText.text = message;

            // 停止之前的协程
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }

            // 显示并自动隐藏
            toastCoroutine = StartCoroutine(ShowToastCoroutine());
        }

        IEnumerator ShowToastCoroutine()
        {
            toastContainer.SetActive(true);

            // 淡入
            CanvasGroup cg = toastContainer.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = toastContainer.AddComponent<CanvasGroup>();
            }

            // 淡入动画
            float fadeInDuration = 0.2f;
            float elapsed = 0f;
            cg.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            cg.alpha = 1f;

            // 显示时间
            yield return new WaitForSecondsRealtime(1.5f);

            // 淡出动画
            float fadeOutDuration = 0.3f;
            elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            toastContainer.SetActive(false);
            toastCoroutine = null;
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
        Helmet,
        Pants,
        Ring,
        Necklace,
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

            // 尝试加载图标（优先PNG文件，回退到运行时生成）
            Sprite itemIcon = Resources.Load<Sprite>(itemData.iconPath);
            if (itemIcon == null)
            {
                // 从iconPath提取物品ID (如 "Sprites/Items/WPN_001" -> "WPN_001")
                string itemId = System.IO.Path.GetFileName(itemData.iconPath);
                if (string.IsNullOrEmpty(itemId)) itemId = itemData.id;
                itemIcon = RuntimeIconGenerator.GetIcon(itemId);
            }
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
                case ShopCategory.Helmet: return new Color(0.5f, 0.5f, 0.7f);
                case ShopCategory.Pants: return new Color(0.4f, 0.6f, 0.5f);
                case ShopCategory.Ring: return new Color(0.7f, 0.5f, 0.8f);
                case ShopCategory.Necklace: return new Color(0.8f, 0.6f, 0.4f);
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
