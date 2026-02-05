using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MoShou.Systems;
using MoShou.Data;

namespace MoShou.Core
{
    /// <summary>
    /// 主菜单管理器
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Player Info Display")]
        [SerializeField] private Text playerLevelText;
        [SerializeField] private Text playerGoldText;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            // Ensure systems are initialized
            InitializeSystems();

            // Bind button events
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClick);
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClick);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClick);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClick);

            // Update UI
            UpdatePlayerInfo();

            // Check if there's a saved game
            UpdateContinueButton();
        }

        /// <summary>
        /// Initialize game systems
        /// </summary>
        private void InitializeSystems()
        {
            if (GameInitializer.Instance == null)
            {
                GameObject initObj = new GameObject("GameInitializer");
                initObj.AddComponent<GameInitializer>();
            }
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
        /// Update continue button availability
        /// </summary>
        private void UpdateContinueButton()
        {
            if (continueButton == null) return;

            bool hasSaveData = SaveSystem.Instance != null && SaveSystem.Instance.HasSaveData();
            continueButton.interactable = hasSaveData;
        }

        /// <summary>
        /// Start new game
        /// </summary>
        private void OnStartClick()
        {
            Debug.Log("[MainMenu] Starting new game...");

            // Reset player progress (optional - could ask for confirmation)
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.ResetProgress();
            }

            // Load stage select scene
            LoadStageSelect();
        }

        /// <summary>
        /// Continue saved game
        /// </summary>
        private void OnContinueClick()
        {
            Debug.Log("[MainMenu] Continuing saved game...");

            // Load stage select with existing progress
            LoadStageSelect();
        }

        /// <summary>
        /// Open settings panel
        /// </summary>
        private void OnSettingsClick()
        {
            Debug.Log("[MainMenu] Opening settings...");
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        /// <summary>
        /// Quit game
        /// </summary>
        private void OnQuitClick()
        {
            Debug.Log("[MainMenu] Quitting game...");

            // Save before quit
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.SaveGame();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Load stage select scene
        /// </summary>
        private void LoadStageSelect()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
            }
            SceneManager.LoadScene("StageSelect");
        }

        /// <summary>
        /// Close settings panel
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
    }
}
