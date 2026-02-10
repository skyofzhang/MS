using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoShou.Systems;
using MoShou.Data;

namespace MoShou.UI
{
    /// <summary>
    /// 背包界面
    /// 对应效果图: UI_Inventory.png
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        public static InventoryPanel Instance { get; private set; }

        [Header("UI引用")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button sortButton;
        [SerializeField] private Text goldText;
        [SerializeField] private Text capacityText;

        private List<InventorySlotUI> slots = new List<InventorySlotUI>();
        private int selectedSlot = -1;
        private CharacterInfoScreen.EquipmentSlotType? filterEquipmentSlot = null;
        private CanvasGroup canvasGroup;

        private void Awake()
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

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 尝试加载效果图背景
            LoadMockupBackground();
        }

        private void LoadMockupBackground()
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            if (backgroundImage != null)
            {
                Sprite bgSprite = Resources.Load<Sprite>("UI_Mockups/Screens/UI_Inventory");
                if (bgSprite != null)
                {
                    backgroundImage.sprite = bgSprite;
                    backgroundImage.type = Image.Type.Simple;
                }
            }
        }

        private void Start()
        {
            InitializeSlots();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (sortButton != null)
                sortButton.onClick.AddListener(OnSortClick);

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            }

            RefreshUI();
        }

        /// <summary>
        /// 初始化格子
        /// </summary>
        private void InitializeSlots()
        {
            if (slotsContainer == null || slotPrefab == null) return;

            int maxSlots = InventoryManager.Instance != null ? InventoryManager.Instance.MaxSlots : 30;

            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI == null)
                {
                    slotUI = slotObj.AddComponent<InventorySlotUI>();
                }
                slotUI.Initialize(i, OnSlotClick);
                slots.Add(slotUI);
            }
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        public void RefreshUI()
        {
            if (InventoryManager.Instance == null) return;

            // 更新每个格子
            for (int i = 0; i < slots.Count; i++)
            {
                InventoryItem item = InventoryManager.Instance.GetItem(i);

                // 如果有装备过滤条件，检查是否匹配
                if (filterEquipmentSlot.HasValue && item != null)
                {
                    bool matchesFilter = DoesItemMatchEquipmentSlot(item, filterEquipmentSlot.Value);
                    slots[i].SetItem(item);
                    slots[i].SetFilterHighlight(matchesFilter); // 匹配的物品高亮显示
                }
                else
                {
                    slots[i].SetItem(item);
                    slots[i].SetFilterHighlight(true); // 无过滤时全部正常显示
                }
            }

            // 更新金币显示
            if (goldText != null && SaveSystem.Instance != null)
            {
                goldText.text = SaveSystem.Instance.CurrentPlayerStats.gold.ToString("N0");
            }

            // 更新容量显示
            if (capacityText != null)
            {
                capacityText.text = $"{InventoryManager.Instance.UsedSlots}/{InventoryManager.Instance.MaxSlots}";
            }
        }

        /// <summary>
        /// 检查物品是否匹配指定的装备槽位
        /// </summary>
        private bool DoesItemMatchEquipmentSlot(InventoryItem item, CharacterInfoScreen.EquipmentSlotType slotType)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId)) return false;

            // 根据物品类型判断是否匹配槽位
            // 假设物品ID包含类型信息，如 WPN_001, ARM_001 等
            string itemId = item.itemId.ToUpper();

            switch (slotType)
            {
                case CharacterInfoScreen.EquipmentSlotType.Weapon:
                    return itemId.StartsWith("WPN") || itemId.Contains("WEAPON") || itemId.Contains("SWORD") || itemId.Contains("BOW");
                case CharacterInfoScreen.EquipmentSlotType.Helmet:
                    return itemId.StartsWith("HLM") || itemId.Contains("HELMET") || itemId.Contains("HAT");
                case CharacterInfoScreen.EquipmentSlotType.Armor:
                    return itemId.StartsWith("ARM") || itemId.Contains("ARMOR") || itemId.Contains("CHEST");
                case CharacterInfoScreen.EquipmentSlotType.Boots:
                    return itemId.StartsWith("BOT") || itemId.StartsWith("PNT") || itemId.Contains("BOOTS") || itemId.Contains("PANTS") || itemId.Contains("SHOE");
                case CharacterInfoScreen.EquipmentSlotType.Accessory1:
                    return itemId.StartsWith("RNG") || itemId.Contains("RING");
                case CharacterInfoScreen.EquipmentSlotType.Accessory2:
                    return itemId.StartsWith("NKL") || itemId.StartsWith("ACC") || itemId.Contains("NECKLACE") || itemId.Contains("ACCESSORY");
                default:
                    return true;
            }
        }

        /// <summary>
        /// 格子点击回调
        /// </summary>
        private void OnSlotClick(int slotIndex)
        {
            if (selectedSlot < 0)
            {
                // 第一次点击，选中
                InventoryItem item = InventoryManager.Instance?.GetItem(slotIndex);
                if (item != null)
                {
                    selectedSlot = slotIndex;
                    slots[slotIndex].SetSelected(true);
                }
            }
            else if (selectedSlot == slotIndex)
            {
                // 点击同一个格子，使用物品
                InventoryManager.Instance?.UseItem(slotIndex);
                ClearSelection();
                RefreshUI();
            }
            else
            {
                // 点击不同格子，交换
                InventoryManager.Instance?.SwapSlots(selectedSlot, slotIndex);
                ClearSelection();
                RefreshUI();
            }
        }

        /// <summary>
        /// 清除选中状态
        /// </summary>
        private void ClearSelection()
        {
            if (selectedSlot >= 0 && selectedSlot < slots.Count)
            {
                slots[selectedSlot].SetSelected(false);
            }
            selectedSlot = -1;
        }

        /// <summary>
        /// 整理按钮点击
        /// </summary>
        private void OnSortClick()
        {
            InventoryManager.Instance?.SortInventory();
            RefreshUI();
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshUI();
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            ClearSelection();
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

        /// <summary>
        /// 为装备选择显示背包
        /// </summary>
        public void ShowForEquipment(CharacterInfoScreen.EquipmentSlotType slotType)
        {
            filterEquipmentSlot = slotType;
            Show();
            Debug.Log($"[InventoryPanel] 显示背包，筛选槽位: {slotType}");
        }

        /// <summary>
        /// 清除装备筛选
        /// </summary>
        public void ClearEquipmentFilter()
        {
            filterEquipmentSlot = null;
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
            }
        }
    }
}
