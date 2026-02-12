using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Slider playerHealthBar;
    public SimpleHealthBar simpleHealthBar; // 新的简单血条
    public Text goldText;
    public Text levelText;
    public Text healthText;     // 生命值文字 (如 "150/200")
    public Text attackText;     // 攻击力文字
    public Text defenseText;    // 防御力文字
    public Text playerLevelText; // 玩家等级文字

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

            // 更新生命值文字
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(player.currentHealth)}/{Mathf.CeilToInt(player.maxHealth)}";
            }
        }

        if (GameManager.Instance != null)
        {
            if (goldText != null)
                goldText.text = $"{GameManager.Instance.SessionGold}";
            if (levelText != null)
                levelText.text = $"波次 {GameManager.Instance.CurrentWave}/{GameManager.Instance.TotalWaves}  击杀: {GameManager.Instance.KillCount}";
        }

        // 更新ATK/DEF/Level（从PlayerStats获取）
        if (MoShou.Systems.SaveSystem.Instance != null && MoShou.Systems.SaveSystem.Instance.CurrentPlayerStats != null)
        {
            var stats = MoShou.Systems.SaveSystem.Instance.CurrentPlayerStats;
            if (attackText != null)
                attackText.text = $"攻击: {stats.GetTotalAttack()}";
            if (defenseText != null)
                defenseText.text = $"防御: {stats.GetTotalDefense()}";
            if (playerLevelText != null)
                playerLevelText.text = $"Lv.{stats.level}";
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
