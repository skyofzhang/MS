using UnityEngine;
using UnityEngine.SceneManagement;
using MoShou.Systems;
using MoShou.Data;
using MoShou.Core;
using MoShou.UI;

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

        [Header("Session Tracking")]
        private float sessionStartTime;

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
            sessionStartTime = Time.time;
            ChangeState(GameState.Playing);
            StartWave(CurrentWave);
        }

        /// <summary>
        /// 获取本次游戏存活时间
        /// </summary>
        public float GetSurvivalTime()
        {
            return Time.time - sessionStartTime;
        }

        /// <summary>
        /// 计算星级评价 (根据击杀效率和血量)
        /// </summary>
        private int CalculateStars()
        {
            // 基础评价：完成即1星
            int stars = 1;

            // 全部波次完成 +1星
            if (CurrentWave >= TotalWaves)
            {
                stars++;
            }

            // 击杀效率高（击杀数 >= 波次*5） +1星
            int expectedKills = TotalWaves * 5;
            if (KillCount >= expectedKills)
            {
                stars++;
            }

            return Mathf.Clamp(stars, 1, 3);
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

            // Apply rewards to player (ResultScreen will also save, but we do it here for safety)
            int starsEarned = CalculateStars();
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.CurrentPlayerStats.AddGold(totalGold);
                bool leveledUp = SaveSystem.Instance.CurrentPlayerStats.AddExperience(totalExp);
                SaveSystem.Instance.MarkStageCleared(CurrentLevel);
                SaveSystem.Instance.SetStageStars(CurrentLevel, starsEarned); // 保存星级
                SaveSystem.Instance.SaveGame();

                if (leveledUp)
                {
                    Debug.Log($"[GameManager] Player leveled up to Lv.{SaveSystem.Instance.CurrentPlayerStats.level}!");
                }
            }

            Debug.Log($"[GameManager] Stars earned: {starsEarned}");

            Debug.Log($"[GameManager] Rewards: {totalGold} Gold, {totalExp} EXP");

            // 播放胜利音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.BGM.Victory, false);
            }

            // 尝试使用新的 ResultScreen
            if (ResultScreen.Instance != null)
            {
                ResultData resultData = new ResultData
                {
                    stageId = CurrentLevel,
                    stageName = stageData?.stageName ?? $"关卡 {CurrentLevel}",
                    goldReward = totalGold,
                    expReward = totalExp,
                    killCount = KillCount,
                    wavesCompleted = CurrentWave,
                    totalWaves = TotalWaves,
                    starsEarned = CalculateStars()
                };
                ResultScreen.Instance.Show(resultData);
                Debug.Log("[GameManager] 显示胜利结算界面");
            }
            // 回退到旧版 UIManager
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVictoryPanel();
            }
            else
            {
                // 如果都不存在，创建一个简单的 ResultScreen
                CreateResultScreen(totalGold, totalExp);
            }
        }

        /// <summary>
        /// 动态创建 ResultScreen
        /// </summary>
        private void CreateResultScreen(int gold, int exp)
        {
            GameObject resultObj = new GameObject("ResultScreen");
            ResultScreen resultScreen = resultObj.AddComponent<ResultScreen>();

            ResultData resultData = new ResultData
            {
                stageId = CurrentLevel,
                stageName = $"关卡 {CurrentLevel}",
                goldReward = gold,
                expReward = exp,
                killCount = KillCount,
                wavesCompleted = CurrentWave,
                totalWaves = TotalWaves,
                starsEarned = CalculateStars()
            };
            resultScreen.Show(resultData);
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

            // DefeatScreen 会保存奖励，这里不重复保存
            Debug.Log($"[GameManager] Partial rewards: {partialGold} Gold, {partialExp} EXP");

            // 播放失败音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.BGM.Defeat, false);
            }

            // 尝试使用新的 DefeatScreen
            if (DefeatScreen.Instance != null)
            {
                StageData stageData = StageDataHolder.CurrentStage;
                DefeatData defeatData = new DefeatData
                {
                    stageId = CurrentLevel,
                    stageName = stageData?.stageName ?? $"关卡 {CurrentLevel}",
                    waveReached = CurrentWave,
                    totalWaves = TotalWaves,
                    killCount = KillCount,
                    survivalTime = GetSurvivalTime(),
                    partialGold = partialGold,
                    partialExp = partialExp
                };
                DefeatScreen.Instance.Show(defeatData);
                Debug.Log("[GameManager] 显示失败结算界面");
            }
            // 回退到旧版 UIManager
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowDefeatPanel();
            }
            else
            {
                // 如果都不存在，创建一个简单的 DefeatScreen
                CreateDefeatScreen(partialGold, partialExp);
            }
        }

        /// <summary>
        /// 动态创建 DefeatScreen
        /// </summary>
        private void CreateDefeatScreen(int gold, int exp)
        {
            GameObject defeatObj = new GameObject("DefeatScreen");
            DefeatScreen defeatScreen = defeatObj.AddComponent<DefeatScreen>();

            StageData stageData = StageDataHolder.CurrentStage;
            DefeatData defeatData = new DefeatData
            {
                stageId = CurrentLevel,
                stageName = stageData?.stageName ?? $"关卡 {CurrentLevel}",
                waveReached = CurrentWave,
                totalWaves = TotalWaves,
                killCount = KillCount,
                survivalTime = GetSurvivalTime(),
                partialGold = gold,
                partialExp = exp
            };
            defeatScreen.Show(defeatData);
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
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
                PauseGame();
            else if (CurrentState == GameState.Paused)
                ResumeGame();
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
        /// Spend gold (for purchases)
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (SessionGold >= amount)
            {
                SessionGold -= amount;
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.CurrentPlayerStats.SpendGold(amount);
                }
                return true;
            }
            return false;
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
