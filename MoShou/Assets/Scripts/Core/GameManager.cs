using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState { Init, MainMenu, Playing, Paused, Victory, Defeat }
    public GameState CurrentState { get; private set; } = GameState.Init;
    
    // 游戏配置
    public int CurrentLevel = 1;
    public int PlayerGold = 0;
    public int PlayerExp = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        ChangeState(GameState.MainMenu);
    }
    
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] State changed to: {newState}");
        
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.Victory:
                OnVictory();
                break;
            case GameState.Defeat:
                OnDefeat();
                break;
        }
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        ChangeState(GameState.Playing);
    }
    
    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
            ChangeState(GameState.Paused);
    }
    
    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        ChangeState(GameState.MainMenu);
    }
    
    void OnVictory()
    {
        Debug.Log("[GameManager] Victory! Level " + CurrentLevel + " completed.");
        PlayerGold += 100;
        PlayerExp += 50;
        CurrentLevel++;
    }
    
    void OnDefeat()
    {
        Debug.Log("[GameManager] Defeat! Retaining 50% rewards.");
        PlayerGold += 50;
        PlayerExp += 25;
    }
    
    public void AddGold(int amount) { PlayerGold += amount; }
    public void AddExp(int amount) { PlayerExp += amount; }
}
