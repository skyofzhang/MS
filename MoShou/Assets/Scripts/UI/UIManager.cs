using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Slider playerHealthBar;
    public Text goldText;
    public Text levelText;
    
    [Header("Skill Buttons")]
    public Button skill1Button;  // 多重箭
    public Button skill2Button;  // 穿透箭
    
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    
    private PlayerController player;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        
        // 绑定技能按钮
        if (skill1Button != null)
            skill1Button.onClick.AddListener(OnSkill1Click);
        if (skill2Button != null)
            skill2Button.onClick.AddListener(OnSkill2Click);
        
        // 隐藏所有面板
        HideAllPanels();
    }
    
    void Update()
    {
        UpdateHUD();
    }
    
    void UpdateHUD()
    {
        if (player != null && playerHealthBar != null)
        {
            playerHealthBar.value = player.currentHealth / player.maxHealth;
        }
        
        if (GameManager.Instance != null)
        {
            if (goldText != null)
                goldText.text = $"Gold: {GameManager.Instance.PlayerGold}";
            if (levelText != null)
                levelText.text = $"Level: {GameManager.Instance.CurrentLevel}";
        }
    }
    
    void OnSkill1Click()
    {
        if (player != null)
            player.UseSkill1();
    }
    
    void OnSkill2Click()
    {
        if (player != null)
            player.UseSkill2();
    }
    
    public void ShowPausePanel()
    {
        HideAllPanels();
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    public void ShowVictoryPanel()
    {
        HideAllPanels();
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }
    
    public void ShowDefeatPanel()
    {
        HideAllPanels();
        if (defeatPanel != null)
            defeatPanel.SetActive(true);
    }
    
    public void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }
    
    // UI按钮事件
    public void OnResumeClick()
    {
        HideAllPanels();
        GameManager.Instance?.ResumeGame();
    }
    
    public void OnRetryClick()
    {
        HideAllPanels();
        GameManager.Instance?.StartGame();
    }
    
    public void OnMainMenuClick()
    {
        HideAllPanels();
        GameManager.Instance?.ReturnToMainMenu();
    }
}
