using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 角色信息屏幕
    /// 显示Q版角色头像、装备槽、属性数值
    /// 对应效果图: UI_CharacterInfo.png
    /// </summary>
    public class CharacterInfoScreen : MonoBehaviour
    {
        public static CharacterInfoScreen Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image characterAvatar;
        [SerializeField] private Text characterNameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Slider expSlider;
        [SerializeField] private Text expText;

        [Header("Equipment Slots")]
        [SerializeField] private Image weaponSlot;
        [SerializeField] private Image armorSlot;
        [SerializeField] private Image helmetSlot;
        [SerializeField] private Image bootsSlot;
        [SerializeField] private Image accessory1Slot;
        [SerializeField] private Image accessory2Slot;

        [Header("Stat Displays")]
        [SerializeField] private Text attackText;
        [SerializeField] private Text defenseText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text critRateText;
        [SerializeField] private Text critDamageText;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button upgradeButton;

        // Equipment slot data
        private Dictionary<EquipmentSlotType, Image> equipmentSlots = new Dictionary<EquipmentSlotType, Image>();

        // Events
        public event Action OnCloseClicked;
        public event Action<EquipmentSlotType> OnEquipmentSlotClicked;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private bool isInitialized = false;
        private bool isVisible = false;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        void Start()
        {
            InitializeUI();
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeUI()
        {
            if (isInitialized) return;

            // 如果没有预设UI引用，动态创建
            if (backgroundImage == null)
            {
                CreateDynamicUI();
            }

            // 绑定按钮
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }

            // 升级按钮已移除（效果图不含）

            isInitialized = true;
        }

        private void CreateDynamicUI()
        {
            // 设置RectTransform为全屏
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // 半透明遮罩背景
            GameObject overlayObj = new GameObject("Overlay");
            overlayObj.transform.SetParent(transform);
            Image overlayImg = overlayObj.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.7f);
            RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            // 面板容器 - 效果图尺寸
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(transform);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(960, 1700);
            panelRect.anchoredPosition = Vector2.zero;

            // 面板背景
            backgroundImage = panelObj.AddComponent<Image>();
            Sprite panelBgSprite = Resources.Load<Sprite>("Sprites/UI/CharInfo/UI_CharInfo_BG");
            if (panelBgSprite != null)
            {
                backgroundImage.sprite = panelBgSprite;
                backgroundImage.type = Image.Type.Sliced;
                backgroundImage.color = Color.white;
            }
            else
            {
                backgroundImage.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            }

            // 关闭按钮
            CreateCloseButton(panelObj.transform);

            // 标题 - 卷轴banner "角色信息"
            CreateTitle(panelObj.transform);

            // 角色展示区（头像+名字+等级+经验+金币）
            CreateCharacterPortrait(panelObj.transform);

            // 角色属性面板（2列网格）
            CreateStatsPanel(panelObj.transform);

            // 已穿戴装备（3列网格）
            CreateEquipmentGrid(panelObj.transform);

            // 金色装饰边框
            CreateDecorations(panelObj.transform);
        }

        private void CreateCloseButton(Transform parent)
        {
            GameObject btnObj = new GameObject("CloseButton");
            btnObj.transform.SetParent(parent);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(1, 1);
            btnRect.anchoredPosition = new Vector2(-15, -15);
            btnRect.sizeDelta = new Vector2(60, 60);

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

            closeButton = btnObj.AddComponent<Button>();
            closeButton.onClick.AddListener(OnCloseButtonClick);

            // X符号
            Text xText = CreateText(btnObj.transform, "X", "✕", 28, TextAnchor.MiddleCenter, Color.white);
        }

        private void CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(500, 90);

            // 卷轴banner背景
            Sprite bannerSprite = Resources.Load<Sprite>("Sprites/UI/Common/UI_Banner_Scroll_Gold");
            if (bannerSprite != null)
            {
                Image bannerImg = titleObj.AddComponent<Image>();
                bannerImg.sprite = bannerSprite;
                bannerImg.type = Image.Type.Sliced;
                bannerImg.color = Color.white;
                bannerImg.raycastTarget = false;
            }

            // "角色信息" 标题文字
            Text titleText = CreateText(titleObj.transform, "TitleText", "角色信息", 38, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            titleText.fontStyle = FontStyle.Bold;
            Outline titleOutline = titleText.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0.3f, 0.2f, 0.1f);
            titleOutline.effectDistance = new Vector2(2, -2);
        }

        /// <summary>
        /// 创建角色展示区 — 效果图纵向布局: 头像+名字+等级+经验+金币
        /// </summary>
        private void CreateCharacterPortrait(Transform parent)
        {
            GameObject areaObj = new GameObject("CharacterPortrait");
            areaObj.transform.SetParent(parent);

            RectTransform areaRect = areaObj.AddComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0, 0.62f);
            areaRect.anchorMax = new Vector2(1, 0.93f);
            areaRect.offsetMin = new Vector2(40, 0);
            areaRect.offsetMax = new Vector2(-40, 0);

            // === 中心头像 300×300 ===
            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(areaObj.transform);

            RectTransform avatarRect = avatarObj.AddComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0.5f, 0.55f);
            avatarRect.anchorMax = new Vector2(0.5f, 0.55f);
            avatarRect.sizeDelta = new Vector2(300, 300);
            avatarRect.anchoredPosition = Vector2.zero;

            // 头像背景
            Image avatarBg = avatarObj.AddComponent<Image>();
            avatarBg.color = new Color(0.2f, 0.22f, 0.28f, 1f);

            // 尝试加载角色头像
            Sprite avatarSprite = Resources.Load<Sprite>("Sprites/Characters/MT_Avatar");
            if (avatarSprite != null)
            {
                avatarBg.sprite = avatarSprite;
                avatarBg.color = Color.white;
            }

            // 头像内框
            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(avatarObj.transform);
            characterAvatar = innerObj.AddComponent<Image>();
            characterAvatar.color = Color.white;

            RectTransform innerRect = innerObj.GetComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.05f, 0.05f);
            innerRect.anchorMax = new Vector2(0.95f, 0.95f);
            innerRect.offsetMin = Vector2.zero;
            innerRect.offsetMax = Vector2.zero;

            // 金色肖像帧sprite
            Sprite portraitFrame = Resources.Load<Sprite>("Sprites/UI/CharInfo/UI_CharInfo_Portrait_Frame");
            if (portraitFrame != null)
            {
                GameObject frameObj = new GameObject("PortraitFrame");
                frameObj.transform.SetParent(avatarObj.transform);
                Image frameImg = frameObj.AddComponent<Image>();
                frameImg.sprite = portraitFrame;
                frameImg.type = Image.Type.Sliced;
                frameImg.color = Color.white;
                frameImg.raycastTarget = false;

                RectTransform frameRect = frameObj.GetComponent<RectTransform>();
                frameRect.anchorMin = Vector2.zero;
                frameRect.anchorMax = Vector2.one;
                frameRect.offsetMin = new Vector2(-10, -10);
                frameRect.offsetMax = new Vector2(10, 10);
            }
            else
            {
                // fallback: 金色边框
                GameObject borderObj = new GameObject("Border");
                borderObj.transform.SetParent(avatarObj.transform);
                Image borderImg = borderObj.AddComponent<Image>();
                borderImg.color = UIStyleHelper.Colors.Gold;
                borderImg.raycastTarget = false;

                RectTransform borderRect = borderObj.GetComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = new Vector2(-5, -5);
                borderRect.offsetMax = new Vector2(5, 5);
                borderObj.transform.SetAsFirstSibling();
            }

            // === 头像下方信息 ===
            // 角色职业+名字 (金色, 30pt)
            characterNameText = CreateText(areaObj.transform, "CharacterName", "战士·艾瑞克", 30, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            characterNameText.fontStyle = FontStyle.Bold;
            RectTransform nameRect = characterNameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.1f, 0.22f);
            nameRect.anchorMax = new Vector2(0.9f, 0.32f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            characterNameText.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);

            // 等级 (24pt)
            levelText = CreateText(areaObj.transform, "Level", "Lv.11", 24, TextAnchor.MiddleCenter, new Color(0.8f, 0.85f, 0.9f));
            RectTransform lvRect = levelText.GetComponent<RectTransform>();
            lvRect.anchorMin = new Vector2(0.3f, 0.14f);
            lvRect.anchorMax = new Vector2(0.7f, 0.22f);
            lvRect.offsetMin = Vector2.zero;
            lvRect.offsetMax = Vector2.zero;

            // 经验条
            GameObject expBarBg = new GameObject("ExpBarBG");
            expBarBg.transform.SetParent(areaObj.transform);
            Image expBgImg = expBarBg.AddComponent<Image>();
            expBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            RectTransform expBgRect = expBarBg.GetComponent<RectTransform>();
            expBgRect.anchorMin = new Vector2(0.15f, 0.08f);
            expBgRect.anchorMax = new Vector2(0.85f, 0.13f);
            expBgRect.offsetMin = Vector2.zero;
            expBgRect.offsetMax = Vector2.zero;

            // 经验条填充
            GameObject expFillObj = new GameObject("ExpFill");
            expFillObj.transform.SetParent(expBarBg.transform);
            Image expFillImg = expFillObj.AddComponent<Image>();
            expFillImg.color = new Color(0.3f, 0.7f, 1f, 1f);

            RectTransform expFillRect = expFillObj.GetComponent<RectTransform>();
            expFillRect.anchorMin = Vector2.zero;
            expFillRect.anchorMax = new Vector2(0.6f, 1f);
            expFillRect.offsetMin = new Vector2(2, 2);
            expFillRect.offsetMax = new Vector2(-2, -2);

            expSlider = expBarBg.AddComponent<Slider>();
            expSlider.fillRect = expFillRect;
            expSlider.interactable = false;
            expSlider.minValue = 0;
            expSlider.maxValue = 1;
            expSlider.value = 0.6f;

            // 经验值文字
            expText = CreateText(expBarBg.transform, "ExpText", "120/200 EXP", 16, TextAnchor.MiddleCenter, Color.white);

            // 金币行
            GameObject goldRow = new GameObject("GoldRow");
            goldRow.transform.SetParent(areaObj.transform);
            RectTransform goldRect = goldRow.AddComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0.3f, 0.0f);
            goldRect.anchorMax = new Vector2(0.7f, 0.07f);
            goldRect.offsetMin = Vector2.zero;
            goldRect.offsetMax = Vector2.zero;

            // coin icon
            GameObject coinGO = new GameObject("CoinIcon");
            coinGO.transform.SetParent(goldRow.transform);
            Image coinImg = coinGO.AddComponent<Image>();
            Sprite coinSprite = Resources.Load<Sprite>("Sprites/UI/Common/UI_Icon_Coin_Stack");
            if (coinSprite != null)
            {
                coinImg.sprite = coinSprite;
                coinImg.color = Color.white;
            }
            else
            {
                coinImg.color = UIStyleHelper.Colors.Gold;
            }
            coinImg.raycastTarget = false;
            RectTransform coinRect = coinGO.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0, 0.5f);
            coinRect.anchorMax = new Vector2(0, 0.5f);
            coinRect.anchoredPosition = new Vector2(20, 0);
            coinRect.sizeDelta = new Vector2(28, 28);

            // 金币数字
            Text goldText = CreateText(goldRow.transform, "GoldText", "4300 金币", 22, TextAnchor.MiddleLeft, UIStyleHelper.Colors.Gold);
            RectTransform gtRect = goldText.GetComponent<RectTransform>();
            gtRect.anchorMin = new Vector2(0.2f, 0);
            gtRect.anchorMax = new Vector2(1, 1);
            gtRect.offsetMin = Vector2.zero;
            gtRect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 创建装备网格 — 效果图3列布局
        /// </summary>
        private void CreateEquipmentGrid(Transform parent)
        {
            // "已穿戴装备" 标题
            GameObject eqTitleObj = new GameObject("EquipmentTitle");
            eqTitleObj.transform.SetParent(parent);
            RectTransform eqTitleRect = eqTitleObj.AddComponent<RectTransform>();
            eqTitleRect.anchorMin = new Vector2(0, 0.18f);
            eqTitleRect.anchorMax = new Vector2(1, 0.22f);
            eqTitleRect.offsetMin = Vector2.zero;
            eqTitleRect.offsetMax = Vector2.zero;

            // 分割线
            Image divider = eqTitleObj.AddComponent<Image>();
            divider.color = new Color(0.4f, 0.35f, 0.25f, 0.3f);

            Text eqTitle = CreateText(eqTitleObj.transform, "Text", "已穿戴装备", 26, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            eqTitle.fontStyle = FontStyle.Bold;

            // 装备网格容器
            GameObject gridObj = new GameObject("EquipmentGrid");
            gridObj.transform.SetParent(parent);

            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 0.02f);
            gridRect.anchorMax = new Vector2(1, 0.18f);
            gridRect.offsetMin = new Vector2(60, 10);
            gridRect.offsetMax = new Vector2(-60, -10);

            GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(140, 160);
            grid.spacing = new Vector2(20, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(30, 30, 5, 5);

            // 创建6个装备格
            var slotConfigs = new List<(EquipmentSlotType type, string label)>
            {
                (EquipmentSlotType.Helmet, "头盔"),
                (EquipmentSlotType.Weapon, "武器"),
                (EquipmentSlotType.Accessory1, "饰品"),
                (EquipmentSlotType.Accessory2, "项链"),
                (EquipmentSlotType.Armor, "护甲"),
                (EquipmentSlotType.Boots, "鞋子"),
            };

            foreach (var config in slotConfigs)
            {
                CreateEquipmentGridSlot(gridObj.transform, config.type, config.label);
            }
        }

        /// <summary>
        /// 创建装备网格单格
        /// </summary>
        private void CreateEquipmentGridSlot(Transform parent, EquipmentSlotType slotType, string label)
        {
            GameObject slotObj = new GameObject($"Slot_{slotType}");
            slotObj.transform.SetParent(parent);

            // 槽背景
            Image slotBg = slotObj.AddComponent<Image>();

            // 尝试加载帧sprite
            Sprite filledFrame = Resources.Load<Sprite>("Sprites/UI/CharInfo/UI_Equip_Slot_Filled");
            Sprite emptyFrame = Resources.Load<Sprite>("Sprites/UI/CharInfo/UI_Equip_Slot_Empty");
            // 默认使用空帧，RefreshEquipment会更新
            if (emptyFrame != null)
            {
                slotBg.sprite = emptyFrame;
                slotBg.type = Image.Type.Sliced;
                slotBg.color = Color.white;
            }
            else
            {
                slotBg.color = new Color(0.15f, 0.18f, 0.22f, 1f);
            }

            // 图标区域 100×100
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.55f);
            iconRect.anchorMax = new Vector2(0.5f, 0.55f);
            iconRect.sizeDelta = new Vector2(100, 100);
            iconRect.anchoredPosition = Vector2.zero;

            // 按钮交互
            Button slotBtn = slotObj.AddComponent<Button>();
            slotBtn.targetGraphic = slotBg;
            EquipmentSlotType capturedType = slotType;
            slotBtn.onClick.AddListener(() => OnEquipmentSlotClick(capturedType));

            // 名称标签（底部）
            Text labelText = CreateText(slotObj.transform, "Label", label, 18, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.8f));
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0);
            labelRect.pivot = new Vector2(0.5f, 0);
            labelRect.anchoredPosition = new Vector2(0, 5);
            labelRect.sizeDelta = new Vector2(0, 30);

            // 存储引用
            equipmentSlots[slotType] = iconImg;

            switch (slotType)
            {
                case EquipmentSlotType.Weapon: weaponSlot = iconImg; break;
                case EquipmentSlotType.Armor: armorSlot = iconImg; break;
                case EquipmentSlotType.Helmet: helmetSlot = iconImg; break;
                case EquipmentSlotType.Boots: bootsSlot = iconImg; break;
                case EquipmentSlotType.Accessory1: accessory1Slot = iconImg; break;
                case EquipmentSlotType.Accessory2: accessory2Slot = iconImg; break;
            }
        }

        /// <summary>
        /// 创建属性面板 — 效果图2列网格，中文标签+icon
        /// </summary>
        private void CreateStatsPanel(Transform parent)
        {
            // "角色属性" 标题
            GameObject statsTitleObj = new GameObject("StatsTitle");
            statsTitleObj.transform.SetParent(parent);
            RectTransform stsTitleRect = statsTitleObj.AddComponent<RectTransform>();
            stsTitleRect.anchorMin = new Vector2(0, 0.55f);
            stsTitleRect.anchorMax = new Vector2(1, 0.59f);
            stsTitleRect.offsetMin = Vector2.zero;
            stsTitleRect.offsetMax = Vector2.zero;

            Image stsDivider = statsTitleObj.AddComponent<Image>();
            stsDivider.color = new Color(0.4f, 0.35f, 0.25f, 0.3f);

            Text statsTitle = CreateText(statsTitleObj.transform, "Text", "角色属性", 26, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            statsTitle.fontStyle = FontStyle.Bold;

            // 属性网格容器
            GameObject statsObj = new GameObject("StatsGrid");
            statsObj.transform.SetParent(parent);

            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0.23f);
            statsRect.anchorMax = new Vector2(1, 0.55f);
            statsRect.offsetMin = new Vector2(30, 0);
            statsRect.offsetMax = new Vector2(-30, -5);

            GridLayoutGroup statsGrid = statsObj.AddComponent<GridLayoutGroup>();
            statsGrid.cellSize = new Vector2(400, 50);
            statsGrid.spacing = new Vector2(10, 8);
            statsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            statsGrid.constraintCount = 2;
            statsGrid.childAlignment = TextAnchor.UpperCenter;
            statsGrid.padding = new RectOffset(10, 10, 10, 10);

            // 属性行: icon路径, 标签, 引用, 颜色
            CreateStatGridItem(statsObj.transform, "UI_Icon_Stat_HP", "生命值:", out healthText, new Color(0.9f, 0.3f, 0.3f));
            CreateStatGridItem(statsObj.transform, "UI_Icon_Stat_ATK", "攻击力:", out attackText, new Color(0.9f, 0.7f, 0.3f));
            CreateStatGridItem(statsObj.transform, "UI_Icon_Stat_DEF", "防御力:", out defenseText, new Color(0.3f, 0.6f, 0.9f));
            CreateStatGridItem(statsObj.transform, "UI_Icon_Stat_CRIT", "暴击率:", out critRateText, new Color(0.9f, 0.8f, 0.2f));

            // 待扩展项 (空位)
            for (int i = 0; i < 4; i++)
            {
                Text dummy;
                CreateStatGridItem(statsObj.transform, null, "待扩展", out dummy, new Color(0.4f, 0.4f, 0.4f, 0.5f));
            }
        }

        private void CreateStatGridItem(Transform parent, string iconName, string label, out Text valueText, Color labelColor)
        {
            GameObject rowObj = new GameObject($"Stat_{label}");
            rowObj.transform.SetParent(parent);

            // icon (28×28)
            if (!string.IsNullOrEmpty(iconName))
            {
                Sprite iconSprite = Resources.Load<Sprite>($"Sprites/UI/CharInfo/{iconName}");
                if (iconSprite != null)
                {
                    GameObject iconGO = new GameObject("Icon");
                    iconGO.transform.SetParent(rowObj.transform);
                    Image iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = iconSprite;
                    iconImg.color = Color.white;
                    iconImg.raycastTarget = false;

                    RectTransform iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0, 0.5f);
                    iconRect.anchorMax = new Vector2(0, 0.5f);
                    iconRect.anchoredPosition = new Vector2(18, 0);
                    iconRect.sizeDelta = new Vector2(28, 28);
                }
            }

            // 属性标签
            Text labelText = CreateText(rowObj.transform, "Label", label, 22, TextAnchor.MiddleLeft, labelColor);
            labelText.fontStyle = FontStyle.Bold;
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.1f, 0);
            labelRect.anchorMax = new Vector2(0.55f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // 属性值
            valueText = CreateText(rowObj.transform, "Value", "0", 22, TextAnchor.MiddleRight, Color.white);
            RectTransform valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.55f, 0);
            valueRect.anchorMax = new Vector2(0.98f, 1);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
        }

        private void CreateDecorations(Transform parent)
        {
            // 顶部金色线
            CreateLine(parent, new Vector2(0.05f, 0.98f), new Vector2(0.95f, 0.98f), 3, UIStyleHelper.Colors.Gold);
            // 底部金色线
            CreateLine(parent, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.02f), 3, UIStyleHelper.Colors.Gold);
        }

        private void CreateLine(Transform parent, Vector2 start, Vector2 end, float height, Color color)
        {
            GameObject lineObj = new GameObject("Line");
            lineObj.transform.SetParent(parent);

            RectTransform lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.anchorMin = start;
            lineRect.anchorMax = new Vector2(end.x, start.y);
            lineRect.pivot = new Vector2(0, 0.5f);
            lineRect.anchoredPosition = Vector2.zero;
            lineRect.sizeDelta = new Vector2(0, height);

            Image lineImg = lineObj.AddComponent<Image>();
            lineImg.color = color;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        #region Refresh Methods

        public void RefreshAll()
        {
            RefreshStats();
            RefreshEquipment();
            RefreshLevel();
        }

        private void RefreshStats()
        {
            if (SaveSystem.Instance?.CurrentPlayerStats == null) return;

            var stats = SaveSystem.Instance.CurrentPlayerStats;

            // 计算基础属性
            int baseAttack = stats.GetTotalAttack();
            int baseDefense = stats.GetTotalDefense();
            int baseHealth = stats.GetTotalMaxHp();
            float baseCritRate = 5f; // 默认5%暴击

            // 获取装备加成
            int equipAttack = 0;
            int equipDefense = 0;
            int equipHealth = 0;
            float equipCritRate = 0f;

            if (MoShou.Systems.EquipmentManager.Instance != null)
            {
                var equipStats = MoShou.Systems.EquipmentManager.Instance.GetTotalStats();
                equipAttack = equipStats.attack;
                equipDefense = equipStats.defense;
                equipHealth = equipStats.health;
                equipCritRate = equipStats.critRate * 100f; // 转为百分比
            }

            // 计算总属性
            int totalAttack = baseAttack + equipAttack;
            int totalDefense = baseDefense + equipDefense;
            int totalHealth = baseHealth + equipHealth;
            float totalCritRate = baseCritRate + equipCritRate;

            // 显示属性（如果有加成则显示 基础+加成 格式）
            if (attackText != null)
            {
                attackText.text = equipAttack > 0 ? $"{totalAttack} <color=#00FF00>(+{equipAttack})</color>" : totalAttack.ToString();
            }
            if (defenseText != null)
            {
                defenseText.text = equipDefense > 0 ? $"{totalDefense} <color=#00FF00>(+{equipDefense})</color>" : totalDefense.ToString();
            }
            if (healthText != null)
            {
                healthText.text = equipHealth > 0 ? $"{totalHealth} <color=#00FF00>(+{equipHealth})</color>" : totalHealth.ToString();
            }
            if (critRateText != null)
            {
                critRateText.text = equipCritRate > 0 ? $"{totalCritRate:F1}% <color=#00FF00>(+{equipCritRate:F1}%)</color>" : $"{totalCritRate:F1}%";
            }
        }

        private void RefreshEquipment()
        {
            // 从EquipmentManager获取装备数据并更新槽位显示
            foreach (var slot in equipmentSlots)
            {
                if (slot.Value == null) continue;

                // 获取对应槽位的装备
                MoShou.Data.Equipment equip = null;
                if (MoShou.Systems.EquipmentManager.Instance != null)
                {
                    // 将UI槽位类型映射到装备槽位类型
                    MoShou.Data.EquipmentSlot dataSlot = MapSlotType(slot.Key);
                    equip = MoShou.Systems.EquipmentManager.Instance.GetEquipment(dataSlot);
                }

                if (equip != null)
                {
                    // 显示已装备的装备
                    slot.Value.color = GetQualityColor(equip.quality);

                    // 尝试加载装备图标（优先用iconPath，回退到ID构造路径）
                    Sprite icon = null;
                    if (!string.IsNullOrEmpty(equip.iconPath))
                    {
                        icon = Resources.Load<Sprite>(equip.iconPath);
                    }
                    if (icon == null)
                    {
                        icon = Resources.Load<Sprite>($"Sprites/Items/{equip.id}");
                    }
                    // 最终回退：运行时内存生成图标
                    if (icon == null)
                    {
                        icon = MoShou.Systems.RuntimeIconGenerator.GetIcon(equip.id);
                    }
                    if (icon != null)
                    {
                        slot.Value.sprite = icon;
                        slot.Value.color = Color.white; // 显示图标原色
                    }
                    else
                    {
                        // 使用品质颜色作为占位符
                        slot.Value.sprite = null;
                    }
                }
                else
                {
                    // 空槽位
                    slot.Value.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                    slot.Value.sprite = null;
                }
            }
        }

        /// <summary>
        /// 映射UI槽位类型到数据槽位类型
        /// </summary>
        private MoShou.Data.EquipmentSlot MapSlotType(EquipmentSlotType uiSlot)
        {
            switch (uiSlot)
            {
                case EquipmentSlotType.Weapon: return MoShou.Data.EquipmentSlot.Weapon;
                case EquipmentSlotType.Helmet: return MoShou.Data.EquipmentSlot.Helmet;
                case EquipmentSlotType.Armor: return MoShou.Data.EquipmentSlot.Armor;
                case EquipmentSlotType.Boots: return MoShou.Data.EquipmentSlot.Pants;  // Boots映射到Pants
                case EquipmentSlotType.Accessory1: return MoShou.Data.EquipmentSlot.Ring;  // 饰品1映射到戒指
                case EquipmentSlotType.Accessory2: return MoShou.Data.EquipmentSlot.Necklace;  // 饰品2映射到项链
                default: return MoShou.Data.EquipmentSlot.Weapon;
            }
        }

        /// <summary>
        /// 根据品质获取颜色
        /// </summary>
        private Color GetQualityColor(MoShou.Data.EquipmentQuality quality)
        {
            switch (quality)
            {
                case MoShou.Data.EquipmentQuality.White: return new Color(0.9f, 0.9f, 0.9f, 1f);
                case MoShou.Data.EquipmentQuality.Green: return new Color(0.4f, 0.9f, 0.4f, 1f);
                case MoShou.Data.EquipmentQuality.Blue: return new Color(0.4f, 0.6f, 1f, 1f);
                case MoShou.Data.EquipmentQuality.Purple: return new Color(0.8f, 0.4f, 1f, 1f);
                case MoShou.Data.EquipmentQuality.Orange: return new Color(1f, 0.6f, 0.2f, 1f);
                default: return Color.white;
            }
        }

        private void RefreshLevel()
        {
            if (SaveSystem.Instance?.CurrentPlayerStats == null) return;

            var stats = SaveSystem.Instance.CurrentPlayerStats;

            if (levelText != null)
            {
                levelText.text = $"Lv. {stats.level}";
            }

            if (expSlider != null)
            {
                int maxExp = CalculateMaxExp(stats.level);
                float progress = maxExp > 0 ? (float)stats.experience / maxExp : 0f;
                expSlider.value = progress;
            }
        }

        private int CalculateMaxExp(int level)
        {
            return Mathf.RoundToInt(100f * Mathf.Pow(level, 1.5f));
        }

        #endregion

        #region Event Handlers

        private void OnCloseButtonClick()
        {
            if (UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(closeButton.transform);
            }

            Hide();
            OnCloseClicked?.Invoke();
        }

        private void OnUpgradeButtonClick()
        {
            if (UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(upgradeButton.transform);
            }

            // TODO: 打开升级界面或执行升级逻辑
            Debug.Log("[CharacterInfoScreen] Upgrade clicked");
        }

        private void OnEquipmentSlotClick(EquipmentSlotType slotType)
        {
            OnEquipmentSlotClicked?.Invoke(slotType);

            // 打开装备选择界面
            if (InventoryPanel.Instance != null)
            {
                InventoryPanel.Instance.ShowForEquipment(slotType);
            }
        }

        #endregion

        #region Visibility

        public void Show()
        {
            if (isVisible) return;

            gameObject.SetActive(true);

            // 确保UI已初始化（首次Show时Start可能还没执行过）
            if (!isInitialized)
            {
                InitializeUI();
            }

            isVisible = true;

            RefreshAll();

            if (canvasGroup != null && UITween.Instance != null)
            {
                canvasGroup.alpha = 0f;
                transform.localScale = Vector3.one * 0.8f;

                UITween.Instance.FadeTo(canvasGroup, 1f, 0.3f, null);
                UITween.Instance.ScaleTo(transform, Vector3.one, 0.3f, null);
            }
            else if (canvasGroup != null)
            {
                // 没有UITween时直接显示
                canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
            }
        }

        public void Hide()
        {
            if (!isVisible) return;

            isVisible = false;

            if (canvasGroup != null && UITween.Instance != null)
            {
                UITween.Instance.FadeTo(canvasGroup, 0f, 0.2f, () => {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        #endregion

        #region Enums

        public enum EquipmentSlotType
        {
            Weapon,
            Armor,
            Helmet,
            Boots,
            Accessory1,
            Accessory2
        }

        #endregion
    }
}
