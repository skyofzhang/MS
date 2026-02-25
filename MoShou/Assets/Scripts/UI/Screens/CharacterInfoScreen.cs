using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 角色信息屏幕 (Prefab方式)
    /// 显示Q版角色头像、装备槽、属性数值
    /// Prefab由 CharacterInfoPrefabCreator 生成，用户可在Inspector中手动调整布局
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
        [SerializeField] private Text goldText;

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

            // 绑定按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }

            // 初始化装备槽字典（Prefab已预设引用）
            if (weaponSlot != null) equipmentSlots[EquipmentSlotType.Weapon] = weaponSlot;
            if (armorSlot != null) equipmentSlots[EquipmentSlotType.Armor] = armorSlot;
            if (helmetSlot != null) equipmentSlots[EquipmentSlotType.Helmet] = helmetSlot;
            if (bootsSlot != null) equipmentSlots[EquipmentSlotType.Boots] = bootsSlot;
            if (accessory1Slot != null) equipmentSlots[EquipmentSlotType.Accessory1] = accessory1Slot;
            if (accessory2Slot != null) equipmentSlots[EquipmentSlotType.Accessory2] = accessory2Slot;

            // 绑定装备槽按钮事件（查找父对象上的Button）
            BindEquipmentSlotButton(weaponSlot, EquipmentSlotType.Weapon);
            BindEquipmentSlotButton(armorSlot, EquipmentSlotType.Armor);
            BindEquipmentSlotButton(helmetSlot, EquipmentSlotType.Helmet);
            BindEquipmentSlotButton(bootsSlot, EquipmentSlotType.Boots);
            BindEquipmentSlotButton(accessory1Slot, EquipmentSlotType.Accessory1);
            BindEquipmentSlotButton(accessory2Slot, EquipmentSlotType.Accessory2);

            isInitialized = true;
        }

        /// <summary>
        /// 绑定装备槽位按钮事件（Button在槽位父对象上）
        /// </summary>
        private void BindEquipmentSlotButton(Image slotIcon, EquipmentSlotType slotType)
        {
            if (slotIcon == null) return;

            // Button挂载在Icon的父对象(Slot_XXX)上
            Button btn = slotIcon.transform.parent != null
                ? slotIcon.transform.parent.GetComponent<Button>()
                : slotIcon.GetComponent<Button>();

            if (btn != null)
            {
                EquipmentSlotType captured = slotType;
                btn.onClick.AddListener(() => OnEquipmentSlotClick(captured));
            }
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
                case EquipmentSlotType.Boots: return MoShou.Data.EquipmentSlot.Pants;
                case EquipmentSlotType.Accessory1: return MoShou.Data.EquipmentSlot.Ring;
                case EquipmentSlotType.Accessory2: return MoShou.Data.EquipmentSlot.Necklace;
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

            if (expText != null)
            {
                int maxExp = CalculateMaxExp(stats.level);
                expText.text = $"{stats.experience}/{maxExp} EXP";
            }

            // 更新金币显示
            if (goldText != null)
            {
                int gold = 0;
                if (GameManager.Instance != null)
                {
                    gold = GameManager.Instance.PlayerGold;
                }
                else if (SaveSystem.Instance != null)
                {
                    gold = SaveSystem.Instance.CurrentPlayerStats.gold;
                }
                goldText.text = $"{gold} 金币";
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
