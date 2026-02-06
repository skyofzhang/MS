using UnityEngine;
using MoShou.Systems;
using MoShou.UI;
using MoShou.Utils;

namespace MoShou.Core
{
    /// <summary>
    /// 游戏初始化器 - 负责初始化所有系统
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("系统预制体")]
        [SerializeField] private GameObject saveSystemPrefab;
        [SerializeField] private GameObject equipmentManagerPrefab;
        [SerializeField] private GameObject inventoryManagerPrefab;
        [SerializeField] private GameObject lootManagerPrefab;

        [Header("UI预制体")]
        [SerializeField] private GameObject gameHUDPrefab;
        [SerializeField] private GameObject inventoryPanelPrefab;
        [SerializeField] private GameObject equipmentPanelPrefab;

        [Header("调试设置")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool addTestItems = true;

        private static GameInitializer instance;
        public static GameInitializer Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                if (autoInitialize)
                {
                    InitializeAllSystems();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化所有系统
        /// </summary>
        public void InitializeAllSystems()
        {
            Debug.Log("[GameInitializer] 开始初始化游戏系统...");

            // 0. 初始化GameManager (最重要!)
            InitializeSystem<GameManager>("GameManager");

            // 1. 初始化存档系统
            InitializeSystem<SaveSystem>("SaveSystem");

            // 2. 初始化装备管理器
            InitializeSystem<EquipmentManager>("EquipmentManager");

            // 3. 初始化背包管理器
            InitializeSystem<InventoryManager>("InventoryManager");

            // 4. 初始化掉落管理器
            InitializeSystem<LootManager>("LootManager");

            // 5. 初始化UI资源绑定器
            InitializeSystem<UIResourceBinder>("UIResourceBinder");

            // 6. 加载玩家数据
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.LoadGame();
            }

            // 7. 添加测试物品（调试用）
            if (addTestItems)
            {
                AddTestItems();
            }

            Debug.Log("[GameInitializer] 所有系统初始化完成!");
        }

        /// <summary>
        /// 初始化单个系统
        /// </summary>
        private void InitializeSystem<T>(string systemName) where T : MonoBehaviour
        {
            if (FindObjectOfType<T>() == null)
            {
                GameObject systemObj = new GameObject(systemName);
                systemObj.AddComponent<T>();
                DontDestroyOnLoad(systemObj);
                Debug.Log($"[GameInitializer] 创建系统: {systemName}");
            }
            else
            {
                Debug.Log($"[GameInitializer] 系统已存在: {systemName}");
            }
        }

        /// <summary>
        /// 添加测试物品
        /// </summary>
        private void AddTestItems()
        {
            if (InventoryManager.Instance == null) return;

            // 添加一些测试装备
            InventoryManager.Instance.AddItem("WPN_001", 1);
            InventoryManager.Instance.AddItem("ARM_001", 1);
            InventoryManager.Instance.AddItem("HLM_001", 1);

            // 添加金币和经验
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.CurrentPlayerStats.AddGold(100);
                SaveSystem.Instance.CurrentPlayerStats.AddExperience(50);
            }

            Debug.Log("[GameInitializer] 添加测试物品完成");
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveGame();
                Debug.Log("[GameInitializer] 游戏已保存");
            }
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public void ResetGame()
        {
            // 清除存档
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.DeleteSave();
            }

            // 重新初始化
            InitializeAllSystems();

            Debug.Log("[GameInitializer] 游戏已重置");
        }

        private void OnApplicationQuit()
        {
            // 退出时自动保存
            SaveGame();
        }

        private void OnApplicationPause(bool pause)
        {
            // 移动端后台时保存
            if (pause)
            {
                SaveGame();
            }
        }
    }
}
