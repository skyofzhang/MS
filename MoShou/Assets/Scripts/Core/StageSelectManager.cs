using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MoShou.Systems;
using MoShou.Data;

namespace MoShou.Core
{
    /// <summary>
    /// Stage selection manager - handles level/chapter selection
    /// </summary>
    public class StageSelectManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform stageButtonsParent;
        [SerializeField] private GameObject stageButtonPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Text chapterTitleText;

        [Header("Player Info")]
        [SerializeField] private Text playerLevelText;
        [SerializeField] private Text playerGoldText;

        [Header("Stage Info Panel")]
        [SerializeField] private GameObject stageInfoPanel;
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text stageDescText;
        [SerializeField] private Text stageRewardText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeInfoButton;

        [Header("Stage Configuration")]
        [SerializeField] private int totalStages = 10;
        [SerializeField] private int currentChapter = 1;

        private List<StageButtonUI> stageButtons = new List<StageButtonUI>();
        private int selectedStageIndex = -1;
        private StageData selectedStage;

        private void Start()
        {
            // Bind events
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClick);
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClick);
            if (closeInfoButton != null)
                closeInfoButton.onClick.AddListener(CloseStageInfo);

            // Generate stage buttons
            GenerateStageButtons();

            // Update UI
            UpdatePlayerInfo();
            UpdateChapterTitle();

            // Hide info panel initially
            if (stageInfoPanel != null)
                stageInfoPanel.SetActive(false);
        }

        /// <summary>
        /// Generate stage selection buttons
        /// </summary>
        private void GenerateStageButtons()
        {
            // 自动创建父容器（如果没有配置）
            if (stageButtonsParent == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    var canvasGO = new GameObject("Canvas");
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }

                var parentGO = new GameObject("StageButtonsParent");
                parentGO.transform.SetParent(canvas.transform, false);
                var vlg = parentGO.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                vlg.spacing = 10;
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childControlWidth = false;
                vlg.childControlHeight = false;

                var rt = parentGO.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 0);

                stageButtonsParent = parentGO.transform;
                Debug.Log("[StageSelect] 自动创建StageButtonsParent");
            }

            // Clear existing buttons
            foreach (Transform child in stageButtonsParent)
            {
                Destroy(child.gameObject);
            }
            stageButtons.Clear();

            // Get player progress
            int unlockedStage = 1;
            if (SaveSystem.Instance != null)
            {
                unlockedStage = SaveSystem.Instance.GetHighestUnlockedStage();
            }

            // 自动加载stageButtonPrefab（如果没有配置）
            if (stageButtonPrefab == null)
            {
                stageButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/StageButton");
                if (stageButtonPrefab == null)
                {
                    Debug.LogError("[StageSelect] StageButton prefab未找到！请运行 MoShou/快速修复/3. 创建UI预制体");
                    return;
                }
                Debug.Log("[StageSelect] 自动加载StageButton prefab");
            }

            // Create buttons for each stage
            for (int i = 1; i <= totalStages; i++)
            {
                GameObject buttonObj = Instantiate(stageButtonPrefab, stageButtonsParent);
                StageButtonUI buttonUI = buttonObj.GetComponent<StageButtonUI>();

                if (buttonUI == null)
                    buttonUI = buttonObj.AddComponent<StageButtonUI>();

                StageData stageData = GetStageData(i);
                bool isUnlocked = i <= unlockedStage;
                bool isCleared = SaveSystem.Instance != null && SaveSystem.Instance.IsStageCleared(i);

                buttonUI.Initialize(stageData, isUnlocked, isCleared, OnStageSelected);
                stageButtons.Add(buttonUI);
            }
        }

        /// <summary>
        /// Get stage data for a specific stage
        /// </summary>
        private StageData GetStageData(int stageIndex)
        {
            // Could be loaded from JSON config, here we generate default data
            return new StageData
            {
                stageId = stageIndex,
                stageName = $"Stage {currentChapter}-{stageIndex}",
                description = GetStageDescription(stageIndex),
                difficulty = Mathf.Min(1 + (stageIndex - 1) / 3, 5),
                recommendedLevel = stageIndex,
                goldReward = 50 + stageIndex * 20,
                expReward = 30 + stageIndex * 15,
                waveCount = 3 + (stageIndex - 1) / 2,
                enemyTypes = GetEnemyTypes(stageIndex)
            };
        }

        /// <summary>
        /// Get stage description based on index
        /// </summary>
        private string GetStageDescription(int stageIndex)
        {
            string[] descriptions = new string[]
            {
                "A peaceful forest path. Perfect for beginners.",
                "The forest deepens. Watch for ambushes.",
                "An ancient ruins area. Enemies grow stronger.",
                "The dark caves beneath the ruins.",
                "A swamp area filled with poisonous creatures.",
                "The abandoned fortress of the enemy.",
                "Enemy stronghold. Expect heavy resistance.",
                "The cursed graveyard. Undead rise here.",
                "Volcanic region. Fire elementals patrol.",
                "The final challenge. Defeat the boss!"
            };

            int index = (int)Mathf.Clamp(stageIndex - 1, 0, descriptions.Length - 1);
            return descriptions[index];
        }

        /// <summary>
        /// Get enemy types for a stage
        /// </summary>
        private string[] GetEnemyTypes(int stageIndex)
        {
            if (stageIndex <= 3)
                return new string[] { "Slime", "Goblin" };
            else if (stageIndex <= 6)
                return new string[] { "Goblin", "Skeleton", "Orc" };
            else if (stageIndex <= 9)
                return new string[] { "Orc", "Dark Knight", "Mage" };
            else
                return new string[] { "Elite Guard", "Boss" };
        }

        /// <summary>
        /// Stage button clicked callback
        /// </summary>
        private void OnStageSelected(StageData stageData)
        {
            selectedStage = stageData;
            selectedStageIndex = stageData.stageId;
            ShowStageInfo(stageData);
        }

        /// <summary>
        /// Show stage info panel
        /// </summary>
        private void ShowStageInfo(StageData stageData)
        {
            if (stageInfoPanel == null) return;

            stageInfoPanel.SetActive(true);

            if (stageNameText != null)
                stageNameText.text = stageData.stageName;
            if (stageDescText != null)
                stageDescText.text = stageData.description;
            if (stageRewardText != null)
                stageRewardText.text = $"Rewards: {stageData.goldReward} Gold, {stageData.expReward} EXP";

            // Check if player level is sufficient
            if (playButton != null)
            {
                int playerLevel = SaveSystem.Instance?.CurrentPlayerStats?.level ?? 1;
                bool canPlay = playerLevel >= stageData.recommendedLevel - 2;
                playButton.interactable = canPlay;
            }
        }

        /// <summary>
        /// Close stage info panel
        /// </summary>
        private void CloseStageInfo()
        {
            if (stageInfoPanel != null)
                stageInfoPanel.SetActive(false);
            selectedStageIndex = -1;
            selectedStage = null;
        }

        /// <summary>
        /// Play button clicked
        /// </summary>
        private void OnPlayClick()
        {
            if (selectedStage == null) return;

            Debug.Log($"[StageSelect] Starting Stage {selectedStage.stageId}...");

            // Set current stage in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentLevel = selectedStage.stageId;
            }

            // Store stage data for battle scene
            StageDataHolder.CurrentStage = selectedStage;

            // Load game scene
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void OnBackClick()
        {
            Debug.Log("[StageSelect] Returning to main menu...");
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Update player info display
        /// </summary>
        private void UpdatePlayerInfo()
        {
            if (SaveSystem.Instance == null) return;

            PlayerStats stats = SaveSystem.Instance.CurrentPlayerStats;
            if (stats == null) return;

            if (playerLevelText != null)
                playerLevelText.text = $"Lv.{stats.level}";
            if (playerGoldText != null)
                playerGoldText.text = stats.gold.ToString();
        }

        /// <summary>
        /// Update chapter title
        /// </summary>
        private void UpdateChapterTitle()
        {
            if (chapterTitleText != null)
                chapterTitleText.text = $"Chapter {currentChapter}";
        }

        /// <summary>
        /// Refresh stage buttons (call after completing a stage)
        /// </summary>
        public void RefreshStages()
        {
            GenerateStageButtons();
        }
    }

    /// <summary>
    /// Stage data structure
    /// </summary>
    [System.Serializable]
    public class StageData
    {
        public int stageId;
        public string stageName;
        public string description;
        public int difficulty;          // 1-5 stars
        public int recommendedLevel;
        public int goldReward;
        public int expReward;
        public int waveCount;
        public string[] enemyTypes;
    }

    /// <summary>
    /// Static holder for passing stage data between scenes
    /// </summary>
    public static class StageDataHolder
    {
        public static StageData CurrentStage { get; set; }
    }

    /// <summary>
    /// Stage button UI component
    /// </summary>
    public class StageButtonUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button button;
        [SerializeField] private Text stageNumberText;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Image[] starIcons;
        [SerializeField] private Image clearMark;

        private StageData stageData;
        private System.Action<StageData> onClickCallback;

        /// <summary>
        /// Initialize the stage button
        /// </summary>
        public void Initialize(StageData data, bool isUnlocked, bool isCleared, System.Action<StageData> onClick)
        {
            stageData = data;
            onClickCallback = onClick;

            // Get components if not assigned
            if (button == null)
                button = GetComponent<Button>();

            // Set up button
            if (button != null)
            {
                button.interactable = isUnlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }

            // Update visuals
            if (stageNumberText != null)
                stageNumberText.text = data.stageId.ToString();

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!isUnlocked);

            if (clearMark != null)
                clearMark.gameObject.SetActive(isCleared);

            // Update star display (difficulty)
            UpdateStars(data.difficulty);
        }

        /// <summary>
        /// Update star icons based on difficulty
        /// </summary>
        private void UpdateStars(int difficulty)
        {
            if (starIcons == null) return;

            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].gameObject.SetActive(i < difficulty);
            }
        }

        /// <summary>
        /// Button click handler
        /// </summary>
        private void OnClick()
        {
            onClickCallback?.Invoke(stageData);
        }
    }
}
