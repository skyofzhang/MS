using System;
using UnityEngine;
using UnityEngine.UI;
using MoShou.Data;

namespace MoShou.UI
{
    /// <summary>
    /// 背包格子UI组件
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text countText;
        [SerializeField] private Image qualityFrame;
        [SerializeField] private Image selectedHighlight;
        [SerializeField] private Button slotButton;

        private int slotIndex;
        private Action<int> onClickCallback;
        private InventoryItem currentItem;

        /// <summary>
        /// 初始化格子
        /// </summary>
        public void Initialize(int index, Action<int> onClick)
        {
            slotIndex = index;
            onClickCallback = onClick;

            if (slotButton == null)
                slotButton = GetComponent<Button>();

            if (slotButton != null)
                slotButton.onClick.AddListener(OnClick);

            SetEmpty();
        }

        /// <summary>
        /// 设置物品
        /// </summary>
        public void SetItem(InventoryItem item)
        {
            currentItem = item;

            if (item == null || item.count <= 0)
            {
                SetEmpty();
                return;
            }

            // 显示图标（简化版，实际应该从资源加载）
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.color = Color.white;
            }

            // 显示数量
            if (countText != null)
            {
                countText.gameObject.SetActive(item.count > 1);
                countText.text = item.count.ToString();
            }

            // 显示品质框（如果是装备）
            if (qualityFrame != null && item.equipmentData != null)
            {
                qualityFrame.gameObject.SetActive(true);
                qualityFrame.color = item.equipmentData.GetQualityColor();
            }
            else if (qualityFrame != null)
            {
                qualityFrame.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 设置为空格子
        /// </summary>
        private void SetEmpty()
        {
            currentItem = null;

            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            if (countText != null)
            {
                countText.gameObject.SetActive(false);
            }

            if (qualityFrame != null)
            {
                qualityFrame.gameObject.SetActive(false);
            }

            SetSelected(false);
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.gameObject.SetActive(selected);
            }
        }

        /// <summary>
        /// 点击回调
        /// </summary>
        private void OnClick()
        {
            onClickCallback?.Invoke(slotIndex);
        }

        /// <summary>
        /// 获取当前物品
        /// </summary>
        public InventoryItem GetItem() => currentItem;
    }
}
