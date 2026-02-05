using UnityEngine;
using UnityEngine.UI;
using MoShou.Data;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 游戏主界面HUD - 显示玩家状态和快捷操作
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("玩家状态")]
        [SerializeField] private Text levelText;
        [SerializeField] private Slider expSlider;
        [SerializeField] private Text expText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Text goldText;

        [Header("快捷按钮")]
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button equipmentButton;
        [SerializeField] private Button settingsButton;

        [Header("面板引用")]
        [SerializeField] private InventoryPanel inventoryPanel;
        [SerializeField] private EquipmentPanel equipmentPanel;

        [Header("战斗信息")]
        [SerializeField] private Text killCountText;
        [SerializeField] private Text waveText;

        private PlayerStats playerStats;
        private int killCount = 0;
        private int currentWave = 1;

        private void Start()
        {
            // 绑定按钮事件
            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OnInventoryClick);
            if (equipmentButton != null)
                equipmentButton.onClick.AddListener(OnEquipmentClick);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClick);

            // 获取玩家状态
            if (SaveSystem.Instance != null)
            {
                playerStats = SaveSystem.Instance.CurrentPlayerStats;
            }

            // 订阅事件
            SubscribeEvents();

            // 初始刷新
            RefreshAll();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            if (LootManager.Instance != null)
            {
                LootManager.Instance.OnGoldPickup += OnGoldPickup;
                LootManager.Instance.OnExpPickup += OnExpPickup;
            }
            // PlayerStats事件通过轮询方式检查，避免事件订阅的复杂性
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeEvents()
        {
            if (LootManager.Instance != null)
            {
                LootManager.Instance.OnGoldPickup -= OnGoldPickup;
                LootManager.Instance.OnExpPickup -= OnExpPickup;
            }
        }

        /// <summary>
        /// 刷新所有显示
        /// </summary>
        public void RefreshAll()
        {
            RefreshPlayerStats();
            RefreshCombatInfo();
        }

        /// <summary>
        /// 刷新玩家状态显示
        /// </summary>
        public void RefreshPlayerStats()
        {
            if (playerStats == null)
            {
                if (SaveSystem.Instance != null)
                    playerStats = SaveSystem.Instance.CurrentPlayerStats;
                if (playerStats == null) return;
            }

            // 等级
            if (levelText != null)
                levelText.text = $"Lv.{playerStats.level}";

            // 经验值
            int expToNext = playerStats.GetExpToNextLevel();
            if (expSlider != null)
            {
                expSlider.maxValue = expToNext;
                expSlider.value = playerStats.experience;
            }
            if (expText != null)
                expText.text = $"{playerStats.experience}/{expToNext}";

            // 生命值
            if (healthSlider != null)
            {
                int maxHealth = playerStats.GetTotalMaxHp();
                healthSlider.maxValue = maxHealth;
                healthSlider.value = playerStats.currentHp > 0 ? playerStats.currentHp : maxHealth;
            }
            if (healthText != null)
            {
                int maxHealth = playerStats.GetTotalMaxHp();
                int currentHealth = playerStats.currentHp > 0 ? playerStats.currentHp : maxHealth;
                healthText.text = $"{currentHealth}/{maxHealth}";
            }

            // 金币
            if (goldText != null)
                goldText.text = playerStats.gold.ToString();
        }

        /// <summary>
        /// 刷新战斗信息
        /// </summary>
        public void RefreshCombatInfo()
        {
            if (killCountText != null)
                killCountText.text = $"击杀: {killCount}";
            if (waveText != null)
                waveText.text = $"波次: {currentWave}";
        }

        /// <summary>
        /// 更新当前生命值
        /// </summary>
        public void UpdateHealth(int current, int max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
            if (healthText != null)
                healthText.text = $"{current}/{max}";
        }

        /// <summary>
        /// 增加击杀数
        /// </summary>
        public void AddKill()
        {
            killCount++;
            RefreshCombatInfo();
        }

        /// <summary>
        /// 设置波次
        /// </summary>
        public void SetWave(int wave)
        {
            currentWave = wave;
            RefreshCombatInfo();
        }

        /// <summary>
        /// 金币拾取回调
        /// </summary>
        private void OnGoldPickup(int amount)
        {
            RefreshPlayerStats();
            // 可以添加金币飘字效果
        }

        /// <summary>
        /// 经验拾取回调
        /// </summary>
        private void OnExpPickup(int amount)
        {
            RefreshPlayerStats();
        }

        /// <summary>
        /// 升级回调
        /// </summary>
        private void OnLevelUp(int newLevel)
        {
            RefreshPlayerStats();
            Debug.Log($"[GameHUD] 玩家升级到 Lv.{newLevel}!");
            // 可以添加升级特效
        }

        /// <summary>
        /// 背包按钮点击
        /// </summary>
        private void OnInventoryClick()
        {
            if (inventoryPanel != null)
                inventoryPanel.Toggle();
        }

        /// <summary>
        /// 装备按钮点击
        /// </summary>
        private void OnEquipmentClick()
        {
            if (equipmentPanel != null)
                equipmentPanel.Toggle();
        }

        /// <summary>
        /// 设置按钮点击
        /// </summary>
        private void OnSettingsClick()
        {
            Debug.Log("[GameHUD] 设置按钮点击 - 功能待实现");
            // 打开设置面板
        }

        /// <summary>
        /// 重置战斗统计
        /// </summary>
        public void ResetCombatStats()
        {
            killCount = 0;
            currentWave = 1;
            RefreshCombatInfo();
        }
    }
}
