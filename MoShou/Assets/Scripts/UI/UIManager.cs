using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单血条控制器 - 使用Image.fillAmount
/// </summary>
public class SimpleHealthBar : MonoBehaviour
{
    public Image fillImage;

    public void SetValue(float normalized)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(normalized);
        }
    }
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Slider playerHealthBar;
    public SimpleHealthBar simpleHealthBar; // 新的简单血条
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
        // 延迟查找玩家（玩家可能延迟创建）
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        if (player != null)
        {
            float healthPercent = player.currentHealth / player.maxHealth;

            // 更新传统Slider血条
            if (playerHealthBar != null)
            {
                playerHealthBar.value = healthPercent;
            }

            // 更新简单Image血条
            if (simpleHealthBar != null)
            {
                simpleHealthBar.SetValue(healthPercent);
            }
        }

        if (GameManager.Instance != null)
        {
            if (goldText != null)
                goldText.text = $"{GameManager.Instance.SessionGold}";
            if (levelText != null)
                levelText.text = $"波次 {GameManager.Instance.CurrentWave}/{GameManager.Instance.TotalWaves}  击杀: {GameManager.Instance.KillCount}";
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
