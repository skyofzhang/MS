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

            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
            }

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

            // 面板容器
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(transform);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900, 1400);
            panelRect.anchoredPosition = Vector2.zero;

            // 面板背景
            backgroundImage = panelObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            // 尝试加载效果图
            Sprite bgSprite = Resources.Load<Sprite>("UI_Mockups/Screens/UI_CharacterInfo");
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                backgroundImage.type = Image.Type.Simple;
            }

            // 关闭按钮
            CreateCloseButton(panelObj.transform);

            // 标题
            CreateTitle(panelObj.transform);

            // 角色区域（头像+装备槽环绕）
            CreateCharacterArea(panelObj.transform);

            // 属性面板
            CreateStatsPanel(panelObj.transform);

            // 升级按钮
            CreateUpgradeButton(panelObj.transform);

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
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(0, 80);

            characterNameText = CreateText(titleObj.transform, "CharacterName", "MT (哀木涕)", 36, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            characterNameText.fontStyle = FontStyle.Bold;
        }

        private void CreateCharacterArea(Transform parent)
        {
            GameObject areaObj = new GameObject("CharacterArea");
            areaObj.transform.SetParent(parent);

            RectTransform areaRect = areaObj.AddComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0, 0.45f);
            areaRect.anchorMax = new Vector2(1, 0.9f);
            areaRect.offsetMin = new Vector2(30, 0);
            areaRect.offsetMax = new Vector2(-30, 0);

            // 中心头像
            CreateCharacterAvatar(areaObj.transform);

            // 环绕的装备槽
            CreateEquipmentSlots(areaObj.transform);

            // 等级和经验条
            CreateLevelDisplay(areaObj.transform);
        }

        private void CreateCharacterAvatar(Transform parent)
        {
            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(parent);

            RectTransform avatarRect = avatarObj.AddComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0.5f, 0.5f);
            avatarRect.anchorMax = new Vector2(0.5f, 0.5f);
            avatarRect.sizeDelta = new Vector2(250, 250);
            avatarRect.anchoredPosition = Vector2.zero;

            // 头像背景
            Image avatarBg = avatarObj.AddComponent<Image>();
            avatarBg.color = new Color(0.2f, 0.22f, 0.28f, 1f);

            // 尝试加载角色头像
            Sprite avatarSprite = Resources.Load<Sprite>("Sprites/Characters/MT_Avatar");
            if (avatarSprite != null)
            {
                avatarBg.sprite = avatarSprite;
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

            // 金色边框
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(avatarObj.transform);
            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = UIStyleHelper.Colors.Gold;

            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-5, -5);
            borderRect.offsetMax = new Vector2(5, 5);
            borderObj.transform.SetAsFirstSibling();
        }

        private void CreateEquipmentSlots(Transform parent)
        {
            // 装备槽位置配置 (相对于中心)
            var slotConfigs = new List<(EquipmentSlotType type, Vector2 position, string label)>
            {
                (EquipmentSlotType.Weapon, new Vector2(-200, 100), "武器"),
                (EquipmentSlotType.Helmet, new Vector2(0, 200), "头盔"),
                (EquipmentSlotType.Armor, new Vector2(200, 100), "护甲"),
                (EquipmentSlotType.Accessory1, new Vector2(-200, -100), "饰品"),
                (EquipmentSlotType.Boots, new Vector2(0, -200), "鞋子"),
                (EquipmentSlotType.Accessory2, new Vector2(200, -100), "饰品"),
            };

            foreach (var config in slotConfigs)
            {
                CreateEquipmentSlot(parent, config.type, config.position, config.label);
            }
        }

        private void CreateEquipmentSlot(Transform parent, EquipmentSlotType slotType, Vector2 position, string label)
        {
            GameObject slotObj = new GameObject($"Slot_{slotType}");
            slotObj.transform.SetParent(parent);

            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(100, 100);
            slotRect.anchoredPosition = position;

            // 槽背景
            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0.15f, 0.18f, 0.22f, 1f);

            // 尝试加载槽位图标
            Sprite slotSprite = Resources.Load<Sprite>($"Sprites/UI/Equipment/Slot_{slotType}");
            if (slotSprite != null)
            {
                slotBg.sprite = slotSprite;
            }

            // 槽内图标区域
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // 按钮交互
            Button slotBtn = slotObj.AddComponent<Button>();
            EquipmentSlotType capturedType = slotType;
            slotBtn.onClick.AddListener(() => OnEquipmentSlotClick(capturedType));

            // 标签
            Text labelText = CreateText(slotObj.transform, "Label", label, 14, TextAnchor.UpperCenter, new Color(0.7f, 0.7f, 0.7f, 1f));
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1.3f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // 存储引用
            equipmentSlots[slotType] = iconImg;

            // 根据类型存储到对应字段
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

        private void CreateLevelDisplay(Transform parent)
        {
            GameObject levelObj = new GameObject("LevelDisplay");
            levelObj.transform.SetParent(parent);

            RectTransform levelRect = levelObj.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 0);
            levelRect.anchorMax = new Vector2(0.5f, 0);
            levelRect.pivot = new Vector2(0.5f, 0);
            levelRect.sizeDelta = new Vector2(400, 80);
            levelRect.anchoredPosition = new Vector2(0, 30);

            // 等级文字
            levelText = CreateText(levelObj.transform, "Level", "Lv. 1", 28, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            levelText.fontStyle = FontStyle.Bold;
            RectTransform lvRect = levelText.GetComponent<RectTransform>();
            lvRect.anchorMin = new Vector2(0, 0.5f);
            lvRect.anchorMax = new Vector2(1, 1);
            lvRect.offsetMin = Vector2.zero;
            lvRect.offsetMax = Vector2.zero;

            // 经验条背景
            GameObject expBarBg = new GameObject("ExpBarBG");
            expBarBg.transform.SetParent(levelObj.transform);
            Image expBgImg = expBarBg.AddComponent<Image>();
            expBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            RectTransform expBgRect = expBarBg.GetComponent<RectTransform>();
            expBgRect.anchorMin = new Vector2(0.1f, 0);
            expBgRect.anchorMax = new Vector2(0.9f, 0.4f);
            expBgRect.offsetMin = Vector2.zero;
            expBgRect.offsetMax = Vector2.zero;

            // 经验条填充
            GameObject expFillObj = new GameObject("ExpFill");
            expFillObj.transform.SetParent(expBarBg.transform);
            Image expFillImg = expFillObj.AddComponent<Image>();
            expFillImg.color = new Color(0.3f, 0.7f, 1f, 1f);

            RectTransform expFillRect = expFillObj.GetComponent<RectTransform>();
            expFillRect.anchorMin = Vector2.zero;
            expFillRect.anchorMax = new Vector2(0.5f, 1f);
            expFillRect.offsetMin = new Vector2(2, 2);
            expFillRect.offsetMax = new Vector2(-2, -2);

            // 创建Slider
            expSlider = expBarBg.AddComponent<Slider>();
            expSlider.fillRect = expFillRect;
            expSlider.interactable = false;
            expSlider.minValue = 0;
            expSlider.maxValue = 1;
            expSlider.value = 0.5f;
        }

        private void CreateStatsPanel(Transform parent)
        {
            GameObject statsObj = new GameObject("StatsPanel");
            statsObj.transform.SetParent(parent);

            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0.12f);
            statsRect.anchorMax = new Vector2(1, 0.42f);
            statsRect.offsetMin = new Vector2(40, 0);
            statsRect.offsetMax = new Vector2(-40, 0);

            // 面板背景
            Image statsBg = statsObj.AddComponent<Image>();
            statsBg.color = new Color(0.1f, 0.12f, 0.15f, 0.8f);

            // 标题
            Text statsTitle = CreateText(statsObj.transform, "Title", "ATTRIBUTES", 24, TextAnchor.UpperCenter, UIStyleHelper.Colors.Gold);
            statsTitle.fontStyle = FontStyle.Bold;
            RectTransform titleRect = statsTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // 属性列表
            CreateStatRow(statsObj.transform, "ATK", out attackText, 0.7f, new Color(1f, 0.5f, 0.3f, 1f));
            CreateStatRow(statsObj.transform, "DEF", out defenseText, 0.5f, new Color(0.3f, 0.7f, 1f, 1f));
            CreateStatRow(statsObj.transform, "HP", out healthText, 0.3f, new Color(0.3f, 1f, 0.5f, 1f));
            CreateStatRow(statsObj.transform, "CRIT", out critRateText, 0.1f, new Color(1f, 0.8f, 0.2f, 1f));
        }

        private void CreateStatRow(Transform parent, string label, out Text valueText, float yPos, Color labelColor)
        {
            GameObject rowObj = new GameObject($"Stat_{label}");
            rowObj.transform.SetParent(parent);

            RectTransform rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.05f, yPos);
            rowRect.anchorMax = new Vector2(0.95f, yPos + 0.15f);
            rowRect.offsetMin = Vector2.zero;
            rowRect.offsetMax = Vector2.zero;

            // 属性标签
            Text labelText = CreateText(rowObj.transform, "Label", label, 22, TextAnchor.MiddleLeft, labelColor);
            labelText.fontStyle = FontStyle.Bold;
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.4f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // 属性值
            valueText = CreateText(rowObj.transform, "Value", "0", 22, TextAnchor.MiddleRight, Color.white);
            RectTransform valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.4f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
        }

        private void CreateUpgradeButton(Transform parent)
        {
            GameObject btnObj = new GameObject("UpgradeButton");
            btnObj.transform.SetParent(parent);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0);
            btnRect.anchorMax = new Vector2(0.5f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 30);
            btnRect.sizeDelta = new Vector2(300, 70);

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.6f, 0.3f, 1f);

            // 尝试加载按钮贴图
            Sprite btnSprite = Resources.Load<Sprite>("Sprites/UI/Common/UI_Button_Primary");
            if (btnSprite != null)
            {
                btnBg.sprite = btnSprite;
                btnBg.type = Image.Type.Sliced;
            }

            upgradeButton = btnObj.AddComponent<Button>();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClick);

            // 按钮文字
            Text btnText = CreateText(btnObj.transform, "Text", "UPGRADE", 24, TextAnchor.MiddleCenter, Color.white);
            btnText.fontStyle = FontStyle.Bold;
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
            isVisible = true;

            RefreshAll();

            if (canvasGroup != null && UITween.Instance != null)
            {
                canvasGroup.alpha = 0f;
                transform.localScale = Vector3.one * 0.8f;

                UITween.Instance.FadeTo(canvasGroup, 1f, 0.3f, null);
                UITween.Instance.ScaleTo(transform, Vector3.one, 0.3f, null);
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
