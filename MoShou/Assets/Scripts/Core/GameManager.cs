using UnityEngine;
using UnityEngine.SceneManagement;
using MoShou.Systems;
using MoShou.Data;
using MoShou.Core;

/// <summary>
/// Game Manager - Central game state and flow control
/// </summary>
public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Init,
            MainMenu,
            StageSelect,
            Loading,
            Playing,
            Paused,
            Victory,
            Defeat
        }

        // Game State
        public GameState CurrentState { get; private set; } = GameState.Init;

        [Header("Current Session")]
        public int CurrentLevel = 1;
        public int CurrentWave = 1;
        public int TotalWaves = 3;
        public int KillCount = 0;

        [Header("Session Rewards")]
        public int SessionGold = 0;
        public int SessionExp = 0;

        // Events
        public event System.Action<GameState> OnStateChanged;
        public event System.Action OnWaveCompleted;
        public event System.Action OnAllWavesCompleted;
        public event System.Action OnPlayerDeath;

        #region Singleton
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        #endregion

        private void Start()
        {
            // Initialize at main menu state
            ChangeState(GameState.MainMenu);
        }

        #region State Management
        /// <summary>
        /// Change game state
        /// </summary>
        public void ChangeState(GameState newState)
        {
            GameState oldState = CurrentState;
            CurrentState = newState;

            Debug.Log($"[GameManager] State: {oldState} -> {newState}");

            // Handle state-specific logic
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;

                case GameState.StageSelect:
                    Time.timeScale = 1f;
                    break;

                case GameState.Loading:
                    Time.timeScale = 1f;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.Victory:
                    Time.timeScale = 0f;
                    HandleVictory();
                    break;

                case GameState.Defeat:
                    Time.timeScale = 0f;
                    HandleDefeat();
                    break;
            }

            OnStateChanged?.Invoke(newState);
        }
        #endregion

        #region Game Flow
        /// <summary>
        /// Start a new game session with specified level
        /// </summary>
        public void StartLevel(int level)
        {
            CurrentLevel = level;
            CurrentWave = 1;
            KillCount = 0;
            SessionGold = 0;
            SessionExp = 0;

            // Get stage data if available
            StageData stageData = StageDataHolder.CurrentStage;
            if (stageData != null)
            {
                TotalWaves = stageData.waveCount;
            }
            else
            {
                TotalWaves = 3 + (level - 1) / 2;
            }

            Debug.Log($"[GameManager] Starting Level {level} with {TotalWaves} waves");

            // Load game scene
            ChangeState(GameState.Loading);
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Called when game scene is ready
        /// </summary>
        public void OnGameSceneReady()
        {
            ChangeState(GameState.Playing);
            StartWave(CurrentWave);
        }

        /// <summary>
        /// Start a wave
        /// </summary>
        public void StartWave(int wave)
        {
            CurrentWave = wave;
            Debug.Log($"[GameManager] Wave {wave}/{TotalWaves} started");

            // Notify spawner to start spawning
            if (MonsterSpawner.Instance != null)
            {
                MonsterSpawner.Instance.StartWave(wave);
            }
        }

        /// <summary>
        /// Called when all enemies in current wave are defeated
        /// </summary>
        public void OnWaveCleared()
        {
            Debug.Log($"[GameManager] Wave {CurrentWave} cleared!");
            OnWaveCompleted?.Invoke();

            if (CurrentWave >= TotalWaves)
            {
                // All waves completed - victory!
                OnAllWavesCompleted?.Invoke();
                ChangeState(GameState.Victory);
            }
            else
            {
                // Start next wave after delay
                Invoke(nameof(StartNextWave), 2f);
            }
        }

        private void StartNextWave()
        {
            StartWave(CurrentWave + 1);
        }

        /// <summary>
        /// Add kill count
        /// </summary>
        public void AddKill(int goldDrop = 0, int expDrop = 0)
        {
            KillCount++;
            SessionGold += goldDrop;
            SessionExp += expDrop;

            Debug.Log($"[GameManager] Kill: {KillCount}, Gold: {SessionGold}, EXP: {SessionExp}");
        }

        /// <summary>
        /// Called when player dies
        /// </summary>
        public void OnPlayerDied()
        {
            Debug.Log("[GameManager] Player died!");
            OnPlayerDeath?.Invoke();
            ChangeState(GameState.Defeat);
        }
        #endregion

        #region Victory/Defeat Handling
        /// <summary>
        /// Handle victory
        /// </summary>
        private void HandleVictory()
        {
            Debug.Log($"[GameManager] Victory! Level {CurrentLevel} completed.");

            // Calculate rewards
            StageData stageData = StageDataHolder.CurrentStage;
            int baseGold = stageData?.goldReward ?? (50 + CurrentLevel * 20);
            int baseExp = stageData?.expReward ?? (30 + CurrentLevel * 15);

            int totalGold = baseGold + SessionGold;
            int totalExp = baseExp + SessionExp;

            // Apply rewards to player
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.CurrentPlayerStats.AddGold(totalGold);
                bool leveledUp = SaveSystem.Instance.CurrentPlayerStats.AddExperience(totalExp);
                SaveSystem.Instance.MarkStageCleared(CurrentLevel);
                SaveSystem.Instance.SaveGame();

                if (leveledUp)
                {
                    Debug.Log($"[GameManager] Player leveled up to Lv.{SaveSystem.Instance.CurrentPlayerStats.level}!");
                }
            }

            Debug.Log($"[GameManager] Rewards: {totalGold} Gold, {totalExp} EXP");

            // Show victory UI through UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVictoryPanel();
            }
        }

        /// <summary>
        /// Handle defeat
        /// </summary>
        private void HandleDefeat()
        {
            Debug.Log($"[GameManager] Defeat at Level {CurrentLevel}, Wave {CurrentWave}");

            // Give partial rewards (50%)
            int partialGold = SessionGold / 2;
            int partialExp = SessionExp / 2;

            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.CurrentPlayerStats.AddGold(partialGold);
                SaveSystem.Instance.CurrentPlayerStats.AddExperience(partialExp);
                SaveSystem.Instance.SaveGame();
            }

            Debug.Log($"[GameManager] Partial rewards: {partialGold} Gold, {partialExp} EXP");

            // Show defeat UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowDefeatPanel();
            }
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Pause game
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowPausePanel();
            }
        }

        /// <summary>
        /// Resume game
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                if (UIManager.Instance != null)
                    UIManager.Instance.HideAllPanels();
            }
        }

        /// <summary>
        /// Retry current level
        /// </summary>
        public void RetryLevel()
        {
            StartLevel(CurrentLevel);
        }

        /// <summary>
        /// Go to next level
        /// </summary>
        public void NextLevel()
        {
            StartLevel(CurrentLevel + 1);
        }

        /// <summary>
        /// Return to stage select
        /// </summary>
        public void ReturnToStageSelect()
        {
            ChangeState(GameState.StageSelect);
            SceneManager.LoadScene("StageSelect");
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Start game (legacy - for old code compatibility)
        /// </summary>
        public void StartGame()
        {
            StartLevel(CurrentLevel);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Add gold (for direct calls)
        /// </summary>
        public void AddGold(int amount)
        {
            SessionGold += amount;
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.CurrentPlayerStats.AddGold(amount);
            }
        }

        /// <summary>
        /// Add experience (for direct calls)
        /// </summary>
        public void AddExp(int amount)
        {
            SessionExp += amount;
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.CurrentPlayerStats.AddExperience(amount);
            }
        }

        /// <summary>
        /// Get player gold (from SaveSystem)
        /// </summary>
        public int PlayerGold
        {
            get => SaveSystem.Instance?.CurrentPlayerStats?.gold ?? 0;
        }

        /// <summary>
        /// Get player exp (from SaveSystem)
        /// </summary>
        public int PlayerExp
        {
            get => SaveSystem.Instance?.CurrentPlayerStats?.experience ?? 0;
        }
        #endregion
}
