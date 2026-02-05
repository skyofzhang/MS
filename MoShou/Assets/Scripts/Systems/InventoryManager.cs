using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoShou.Data;

namespace MoShou.Systems
{
    /// <summary>
    /// 背包管理器 - 管理玩家背包中的物品
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("背包设置")]
        [SerializeField] private int maxSlots = 30;     // 最大格子数

        // 背包数据
        private List<InventoryItem> items = new List<InventoryItem>();

        // 物品配置库
        private Dictionary<string, ItemData> itemDatabase = new Dictionary<string, ItemData>();

        // 事件
        public event Action<int, InventoryItem> OnSlotChanged;
        public event Action OnInventoryChanged;

        public int MaxSlots => maxSlots;
        public int UsedSlots => items.Count(i => i != null && i.count > 0);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInventory();
                LoadItemDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化背包
        /// </summary>
        private void InitializeInventory()
        {
            items.Clear();
            for (int i = 0; i < maxSlots; i++)
            {
                items.Add(null);
            }
        }

        /// <summary>
        /// 加载物品配置
        /// </summary>
        private void LoadItemDatabase()
        {
            // 从装备管理器获取装备作为物品
            // 实际项目中应该有单独的物品配置文件
            Debug.Log("[InventoryManager] 背包系统初始化完成");
        }

        /// <summary>
        /// 添加物品到背包
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns>实际添加的数量</returns>
        public int AddItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return 0;

            int remaining = amount;

            // 检查是否是装备（装备不可堆叠）
            Equipment equipConfig = EquipmentManager.Instance?.GetEquipmentConfig(itemId);
            bool isEquipment = equipConfig != null;
            int maxStack = isEquipment ? 1 : 99;

            // 先尝试堆叠到已有的同类物品
            if (!isEquipment)
            {
                for (int i = 0; i < items.Count && remaining > 0; i++)
                {
                    if (items[i] != null && items[i].itemId == itemId && items[i].count < maxStack)
                    {
                        int canAdd = Mathf.Min(remaining, maxStack - items[i].count);
                        items[i].count += canAdd;
                        remaining -= canAdd;
                        OnSlotChanged?.Invoke(i, items[i]);
                    }
                }
            }

            // 放入空格子
            while (remaining > 0)
            {
                int emptySlot = FindEmptySlot();
                if (emptySlot < 0)
                {
                    Debug.LogWarning($"[InventoryManager] 背包已满! 剩余 {remaining} 个物品无法添加");
                    break;
                }

                int toAdd = Mathf.Min(remaining, maxStack);
                InventoryItem newItem = new InventoryItem(itemId, toAdd, emptySlot);

                // 如果是装备，附加装备数据
                if (isEquipment)
                {
                    newItem.equipmentData = equipConfig;
                }

                items[emptySlot] = newItem;
                remaining -= toAdd;
                OnSlotChanged?.Invoke(emptySlot, newItem);
            }

            int added = amount - remaining;
            if (added > 0)
            {
                Debug.Log($"[InventoryManager] 添加物品: {itemId} x{added}");
                OnInventoryChanged?.Invoke();
            }

            return added;
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="amount">数量</param>
        /// <returns>实际移除的数量</returns>
        public int RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return 0;

            int remaining = amount;

            for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (items[i] != null && items[i].itemId == itemId)
                {
                    int toRemove = Mathf.Min(remaining, items[i].count);
                    items[i].count -= toRemove;
                    remaining -= toRemove;

                    if (items[i].count <= 0)
                    {
                        items[i] = null;
                    }
                    OnSlotChanged?.Invoke(i, items[i]);
                }
            }

            int removed = amount - remaining;
            if (removed > 0)
            {
                Debug.Log($"[InventoryManager] 移除物品: {itemId} x{removed}");
                OnInventoryChanged?.Invoke();
            }

            return removed;
        }

        /// <summary>
        /// 移除指定格子的物品
        /// </summary>
        public InventoryItem RemoveFromSlot(int slotIndex, int amount = -1)
        {
            if (slotIndex < 0 || slotIndex >= items.Count) return null;
            if (items[slotIndex] == null) return null;

            InventoryItem item = items[slotIndex];
            int toRemove = amount < 0 ? item.count : Mathf.Min(amount, item.count);

            InventoryItem result = new InventoryItem(item.itemId, toRemove, slotIndex);
            result.equipmentData = item.equipmentData;

            item.count -= toRemove;
            if (item.count <= 0)
            {
                items[slotIndex] = null;
            }

            OnSlotChanged?.Invoke(slotIndex, items[slotIndex]);
            OnInventoryChanged?.Invoke();

            return result;
        }

        /// <summary>
        /// 获取物品数量
        /// </summary>
        public int GetItemCount(string itemId)
        {
            return items.Where(i => i != null && i.itemId == itemId).Sum(i => i.count);
        }

        /// <summary>
        /// 获取指定格子的物品
        /// </summary>
        public InventoryItem GetItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= items.Count) return null;
            return items[slotIndex];
        }

        /// <summary>
        /// 获取所有物品
        /// </summary>
        public List<InventoryItem> GetAllItems()
        {
            return items.Where(i => i != null && i.count > 0).ToList();
        }

        /// <summary>
        /// 查找空格子
        /// </summary>
        public int FindEmptySlot()
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null || items[i].count <= 0)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 检查背包是否已满
        /// </summary>
        public bool IsFull()
        {
            return FindEmptySlot() < 0;
        }

        /// <summary>
        /// 交换两个格子的物品
        /// </summary>
        public void SwapSlots(int slotA, int slotB)
        {
            if (slotA < 0 || slotA >= items.Count) return;
            if (slotB < 0 || slotB >= items.Count) return;
            if (slotA == slotB) return;

            var temp = items[slotA];
            items[slotA] = items[slotB];
            items[slotB] = temp;

            // 更新槽位索引
            if (items[slotA] != null) items[slotA].slotIndex = slotA;
            if (items[slotB] != null) items[slotB].slotIndex = slotB;

            OnSlotChanged?.Invoke(slotA, items[slotA]);
            OnSlotChanged?.Invoke(slotB, items[slotB]);
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// 使用物品（穿戴装备）
        /// </summary>
        public bool UseItem(int slotIndex)
        {
            InventoryItem item = GetItem(slotIndex);
            if (item == null) return false;

            // 如果是装备，尝试穿戴
            if (item.equipmentData != null && EquipmentManager.Instance != null)
            {
                Equipment oldEquip = EquipmentManager.Instance.Equip(item.equipmentData);

                // 从背包移除
                RemoveFromSlot(slotIndex, 1);

                // 如果有旧装备，放回背包
                if (oldEquip != null)
                {
                    AddItem(oldEquip.id, 1);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 整理背包
        /// </summary>
        public void SortInventory()
        {
            var validItems = items.Where(i => i != null && i.count > 0).OrderBy(i => i.itemId).ToList();
            InitializeInventory();

            for (int i = 0; i < validItems.Count; i++)
            {
                validItems[i].slotIndex = i;
                items[i] = validItems[i];
            }

            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// 获取保存数据
        /// </summary>
        public Dictionary<string, int> GetSaveData()
        {
            var data = new Dictionary<string, int>();
            foreach (var item in items)
            {
                if (item != null && item.count > 0)
                {
                    if (data.ContainsKey(item.itemId))
                        data[item.itemId] += item.count;
                    else
                        data[item.itemId] = item.count;
                }
            }
            return data;
        }

        /// <summary>
        /// 加载保存数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, int> data)
        {
            InitializeInventory();
            foreach (var kvp in data)
            {
                AddItem(kvp.Key, kvp.Value);
            }
        }
    }
}
