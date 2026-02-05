using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoShou.Systems;
using MoShou.Data;

namespace MoShou.UI
{
    /// <summary>
    /// 背包界面
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button sortButton;
        [SerializeField] private Text goldText;
        [SerializeField] private Text capacityText;

        private List<InventorySlotUI> slots = new List<InventorySlotUI>();
        private int selectedSlot = -1;

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

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
            }
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
                slots[i].SetItem(item);
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
    }
}
