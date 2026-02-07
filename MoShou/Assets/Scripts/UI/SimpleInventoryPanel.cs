using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简化版背包面板 - 用于运行时动态创建，不依赖SerializeField
/// </summary>
public class SimpleInventoryPanel : MonoBehaviour
{
    // 静态实例，用于在面板被禁用时也能访问
    public static SimpleInventoryPanel Instance { get; set; }

    [Header("运行时设置的引用")]
    public Transform slotsContainer;
    public Text goldText;
    public Text capacityText;

    private int maxSlots = 30;
    private bool isInitialized = false;

    void Awake()
    {
        // 设置静态实例
        Instance = this;
        Debug.Log("[SimpleInventoryPanel] Instance已设置");
    }

    void Start()
    {
        Initialize();
    }

    void OnEnable()
    {
        RefreshUI();
    }

    /// <summary>
    /// 初始化背包
    /// </summary>
    void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        Debug.Log("[SimpleInventoryPanel] 初始化完成");
        RefreshUI();
    }

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    public void RefreshUI()
    {
        // 更新金币显示
        if (goldText != null && GameManager.Instance != null)
        {
            goldText.text = $"金币: {GameManager.Instance.SessionGold}";
        }

        // 更新容量显示
        if (capacityText != null)
        {
            int usedSlots = GetUsedSlotCount();
            capacityText.text = $"{usedSlots}/{maxSlots}";
        }

        // 更新格子状态
        UpdateSlots();
    }

    /// <summary>
    /// 获取已使用的格子数量
    /// </summary>
    int GetUsedSlotCount()
    {
        // 这里可以接入实际的背包系统
        // 目前返回模拟值
        return 0;
    }

    /// <summary>
    /// 更新所有格子
    /// </summary>
    void UpdateSlots()
    {
        if (slotsContainer == null) return;

        // 遍历所有格子并更新显示
        for (int i = 0; i < slotsContainer.childCount; i++)
        {
            Transform slot = slotsContainer.GetChild(i);
            // 可以在这里设置格子的物品图标和数量
            // 目前格子都是空的
        }
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        RefreshUI();
        Debug.Log("[SimpleInventoryPanel] 显示背包");
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("[SimpleInventoryPanel] 隐藏背包");
    }

    /// <summary>
    /// 切换显示/隐藏
    /// </summary>
    public void Toggle()
    {
        if (gameObject.activeSelf)
            Hide();
        else
            Show();
    }

    /// <summary>
    /// 添加物品到背包（接口预留）
    /// </summary>
    public bool AddItem(string itemId, int count = 1)
    {
        Debug.Log($"[SimpleInventoryPanel] 添加物品: {itemId} x{count}");
        RefreshUI();
        return true;
    }

    /// <summary>
    /// 从背包移除物品（接口预留）
    /// </summary>
    public bool RemoveItem(string itemId, int count = 1)
    {
        Debug.Log($"[SimpleInventoryPanel] 移除物品: {itemId} x{count}");
        RefreshUI();
        return true;
    }
}
