using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoShou.Data;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 装备面板UI - 显示玩家已装备的物品
    /// </summary>
    public class EquipmentPanel : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private Transform slotsParent;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text panelTitle;

        [Header("属性显示")]
        [SerializeField] private Text attackText;
        [SerializeField] private Text defenseText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text critRateText;

        // 装备槽位UI
        private Dictionary<EquipmentSlot, EquipmentSlotUI> slotUIs = new Dictionary<EquipmentSlot, EquipmentSlotUI>();

        private void Start()
        {
            InitializeSlots();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // 订阅装备变化事件
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged += OnEquipmentChanged;
            }

            RefreshAllSlots();
            UpdateStatsDisplay();
        }

        private void OnDestroy()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged -= OnEquipmentChanged;
            }
        }

        /// <summary>
        /// 初始化装备槽位
        /// </summary>
        private void InitializeSlots()
        {
            // 为每个装备槽位类型创建UI
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slotPrefab != null && slotsParent != null)
                {
                    GameObject slotObj = Instantiate(slotPrefab, slotsParent);
                    EquipmentSlotUI slotUI = slotObj.GetComponent<EquipmentSlotUI>();
                    if (slotUI == null)
                        slotUI = slotObj.AddComponent<EquipmentSlotUI>();

                    slotUI.Initialize(slot, OnSlotClicked);
                    slotUIs[slot] = slotUI;
                }
            }
        }

        /// <summary>
        /// 刷新所有槽位
        /// </summary>
        public void RefreshAllSlots()
        {
            if (EquipmentManager.Instance == null) return;

            foreach (var kvp in slotUIs)
            {
                Equipment equip = EquipmentManager.Instance.GetEquipment(kvp.Key);
                kvp.Value.SetEquipment(equip);
            }
        }

        /// <summary>
        /// 装备变化回调（slot=槽位, newEquip=当前装备，旧装备由 Refresh 时已更新故未传）
        /// </summary>
        private void OnEquipmentChanged(EquipmentSlot slot, Equipment newEquip)
        {
            if (slotUIs.TryGetValue(slot, out EquipmentSlotUI slotUI))
            {
                slotUI.SetEquipment(newEquip);
            }
            UpdateStatsDisplay();
        }

        /// <summary>
        /// 槽位点击回调
        /// </summary>
        private void OnSlotClicked(EquipmentSlot slot)
        {
            Equipment equip = EquipmentManager.Instance?.GetEquipment(slot);
            if (equip != null)
            {
                // 卸下装备到背包
                Equipment unequipped = EquipmentManager.Instance.Unequip(slot);
                if (unequipped != null && InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(unequipped.id, 1);
                    Debug.Log($"[EquipmentPanel] 卸下装备: {unequipped.name}");
                }
            }
        }

        /// <summary>
        /// 更新属性显示
        /// </summary>
        private void UpdateStatsDisplay()
        {
            if (EquipmentManager.Instance == null) return;

            var stats = EquipmentManager.Instance.GetTotalStats();

            if (attackText != null)
                attackText.text = $"攻击: +{stats.attack}";
            if (defenseText != null)
                defenseText.text = $"防御: +{stats.defense}";
            if (healthText != null)
                healthText.text = $"生命: +{stats.health}";
            if (critRateText != null)
                critRateText.text = $"暴击: +{stats.critRate:P1}";
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshAllSlots();
            UpdateStatsDisplay();
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
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
    /// 装备槽位UI组件
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrame;
        [SerializeField] private Text slotNameText;
        [SerializeField] private Button slotButton;

        private EquipmentSlot slotType;
        private Equipment currentEquipment;
        private Action<EquipmentSlot> onClickCallback;

        /// <summary>
        /// 初始化槽位
        /// </summary>
        public void Initialize(EquipmentSlot slot, Action<EquipmentSlot> onClick)
        {
            slotType = slot;
            onClickCallback = onClick;

            if (slotButton == null)
                slotButton = GetComponent<Button>();

            if (slotButton != null)
                slotButton.onClick.AddListener(OnClick);

            // 设置槽位名称
            if (slotNameText != null)
                slotNameText.text = GetSlotDisplayName(slot);

            SetEmpty();
        }

        /// <summary>
        /// 设置装备
        /// </summary>
        public void SetEquipment(Equipment equip)
        {
            currentEquipment = equip;

            if (equip == null)
            {
                SetEmpty();
                return;
            }

            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.color = Color.white;
            }

            if (qualityFrame != null)
            {
                qualityFrame.gameObject.SetActive(true);
                qualityFrame.color = equip.GetQualityColor();
            }
        }

        /// <summary>
        /// 设置为空槽位
        /// </summary>
        private void SetEmpty()
        {
            currentEquipment = null;

            if (iconImage != null)
                iconImage.gameObject.SetActive(false);

            if (qualityFrame != null)
                qualityFrame.gameObject.SetActive(false);
        }

        /// <summary>
        /// 点击回调
        /// </summary>
        private void OnClick()
        {
            onClickCallback?.Invoke(slotType);
        }

        /// <summary>
        /// 获取槽位显示名称
        /// </summary>
        private string GetSlotDisplayName(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: return "武器";
                case EquipmentSlot.Armor: return "护甲";
                case EquipmentSlot.Helmet: return "头盔";
                case EquipmentSlot.Boots: return "靴子";
                case EquipmentSlot.Ring: return "戒指";
                case EquipmentSlot.Necklace: return "项链";
                default: return slot.ToString();
            }
        }

        /// <summary>
        /// 获取当前装备
        /// </summary>
        public Equipment GetEquipment() => currentEquipment;
    }
}
