using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MoShou.Systems;
using MoShou.Data;

namespace MoShou.UI
{
    /// <summary>
    /// 技能升级面板 - 显示和升级玩家技能
    /// 符合知识库 T05 UI原型图规范
    /// </summary>
    public class SkillUpgradePanel : MonoBehaviour
    {
        public static SkillUpgradePanel Instance { get; set; }

        [Header("UI引用")]
        public Transform skillsContainer;    // 技能列表容器
        public Text goldText;                // 玩家金币显示
        public Text skillPointsText;         // 技能点显示
        public Text titleText;               // 标题
        public Button closeButton;           // 关闭按钮

        [Header("技能详情面板")]
        public GameObject detailPanel;       // 技能详情面板
        public Image detailIcon;             // 详情技能图标
        public Text detailName;              // 详情技能名称
        public Text detailDescription;       // 详情技能描述
        public Text detailLevel;             // 详情技能等级
        public Text detailCost;              // 升级消耗
        public Button upgradeButton;         // 升级按钮

        // 技能数据
        private List<SkillData> allSkills = new List<SkillData>();
        private List<SkillSlotUI> skillUIs = new List<SkillSlotUI>();
        private SkillData selectedSkill;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // 绑定按钮事件
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClick);

            // 加载技能数据
            LoadSkillData();

            // 创建技能列表
            CreateSkillList();

            // 隐藏详情面板
            if (detailPanel != null)
                detailPanel.SetActive(false);
        }

        /// <summary>
        /// 加载技能数据
        /// </summary>
        void LoadSkillData()
        {
            // 尝试从配置文件加载
            TextAsset configFile = Resources.Load<TextAsset>("Configs/SkillConfigs");
            if (configFile != null)
            {
                try
                {
                    SkillConfigTable table = JsonUtility.FromJson<SkillConfigTable>(configFile.text);
                    if (table != null && table.skills != null)
                    {
                        allSkills = table.skills;
                        Debug.Log($"[SkillUpgradePanel] 加载了 {allSkills.Count} 个技能");
                        return;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SkillUpgradePanel] 解析技能配置失败: {e.Message}");
                }
            }

            // 使用默认技能数据（符合知识库技能表）
            CreateDefaultSkills();
        }

        /// <summary>
        /// 创建默认技能数据（符合策划案）
        /// </summary>
        void CreateDefaultSkills()
        {
            allSkills = new List<SkillData>
            {
                // 主动技能
                new SkillData
                {
                    id = "SK001",
                    name = "多重箭",
                    description = "向前方射出3支箭矢，造成基础攻击力60%的伤害",
                    skillType = SkillType.Active,
                    maxLevel = 10,
                    currentLevel = 1,
                    unlockLevel = 1,
                    baseCooldown = 8f,
                    baseGoldCost = 100,
                    levelGoldMultiplier = 1.5f,
                    iconPath = "Sprites/UI/Skills/UI_Skill_Icon_MultiShot",
                    effectPerLevel = "伤害+5%, 箭矢数+1(每3级)"
                },
                new SkillData
                {
                    id = "SK002",
                    name = "穿透箭",
                    description = "发射一支穿透敌人的箭矢，对路径上所有敌人造成伤害",
                    skillType = SkillType.Active,
                    maxLevel = 10,
                    currentLevel = 1,
                    unlockLevel = 3,
                    baseCooldown = 12f,
                    baseGoldCost = 150,
                    levelGoldMultiplier = 1.6f,
                    iconPath = "Sprites/UI/Skills/UI_Skill_Icon_Pierce",
                    effectPerLevel = "伤害+8%, 穿透距离+10%"
                },
                new SkillData
                {
                    id = "SK003",
                    name = "战吼",
                    description = "发出战吼，提升自身攻击力30%，持续10秒",
                    skillType = SkillType.Active,
                    maxLevel = 5,
                    currentLevel = 0,
                    unlockLevel = 5,
                    baseCooldown = 30f,
                    baseGoldCost = 200,
                    levelGoldMultiplier = 2f,
                    iconPath = "Sprites/UI/Skills/UI_Skill_Icon_BattleShout",
                    effectPerLevel = "攻击加成+5%, 持续时间+2秒"
                },

                // 被动技能
                new SkillData
                {
                    id = "PS001",
                    name = "精准瞄准",
                    description = "提升暴击率",
                    skillType = SkillType.Passive,
                    maxLevel = 10,
                    currentLevel = 0,
                    unlockLevel = 2,
                    baseCooldown = 0f,
                    baseGoldCost = 80,
                    levelGoldMultiplier = 1.4f,
                    iconPath = "Sprites/UI/Skills/UI_Skill_Icon_Precision",
                    effectPerLevel = "暴击率+2%"
                },
                new SkillData
                {
                    id = "PS002",
                    name = "强韧体魄",
                    description = "提升最大生命值",
                    skillType = SkillType.Passive,
                    maxLevel = 10,
                    currentLevel = 0,
                    unlockLevel = 1,
                    baseCooldown = 0f,
                    baseGoldCost = 60,
                    levelGoldMultiplier = 1.3f,
                    iconPath = "Sprites/UI/Skills/UI_Skill_Icon_Vitality",
                    effectPerLevel = "最大生命+20"
                },
                new SkillData
                {
                    id = "PS003",
                    name = "疾风步",
                    description = "提升移动速度",
                    skillType = SkillType.Passive,
                    maxLevel = 5,
                    currentLevel = 0,
                    unlockLevel = 4,
                    baseCooldown = 0f,
                    baseGoldCost = 120,
                    levelGoldMultiplier = 1.5f,
                    iconPath = "Sprites/UI/Skills/UI_Skill_Icon_Swift",
                    effectPerLevel = "移动速度+5%"
                },
                new SkillData
                {
                    id = "PS004",
                    name = "吸血本能",
                    description = "攻击时回复生命",
                    skillType = SkillType.Passive,
                    maxLevel = 5,
                    currentLevel = 0,
                    unlockLevel = 8,
                    baseCooldown = 0f,
                    baseGoldCost = 300,
                    levelGoldMultiplier = 2f,
                    iconPath = "Sprites/UI/Skills/UI_Skill_Icon_Lifesteal",
                    effectPerLevel = "吸血+1%"
                }
            };

            Debug.Log($"[SkillUpgradePanel] 使用默认技能数据: {allSkills.Count} 个技能");
        }

        /// <summary>
        /// 创建技能列表UI
        /// </summary>
        void CreateSkillList()
        {
            if (skillsContainer == null) return;

            // 清空现有UI
            foreach (var ui in skillUIs)
            {
                if (ui != null && ui.gameObject != null)
                    Destroy(ui.gameObject);
            }
            skillUIs.Clear();

            // 按类型分组：先主动后被动
            var activeSkills = allSkills.FindAll(s => s.skillType == SkillType.Active);
            var passiveSkills = allSkills.FindAll(s => s.skillType == SkillType.Passive);

            // 创建主动技能标题
            CreateSectionTitle("主动技能");
            foreach (var skill in activeSkills)
            {
                CreateSkillSlotUI(skill);
            }

            // 创建被动技能标题
            CreateSectionTitle("被动技能");
            foreach (var skill in passiveSkills)
            {
                CreateSkillSlotUI(skill);
            }
        }

        /// <summary>
        /// 创建分区标题
        /// </summary>
        void CreateSectionTitle(string title)
        {
            if (skillsContainer == null) return;

            GameObject titleGO = new GameObject($"Title_{title}");
            titleGO.transform.SetParent(skillsContainer, false);

            var layoutElement = titleGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40;
            layoutElement.flexibleWidth = 1;

            Text titleText = titleGO.AddComponent<Text>();
            titleText.text = title;
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.color = new Color(0.8f, 0.7f, 0.4f);
            titleText.font = GetDefaultFont();
        }

        /// <summary>
        /// 创建技能槽位UI
        /// </summary>
        void CreateSkillSlotUI(SkillData skill)
        {
            if (skillsContainer == null) return;

            GameObject slotGO = new GameObject($"Skill_{skill.id}");
            slotGO.transform.SetParent(skillsContainer, false);

            // 添加LayoutElement
            var layoutElement = slotGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 90;
            layoutElement.flexibleWidth = 1;

            // 背景
            Image bgImage = slotGO.AddComponent<Image>();
            bgImage.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

            // 添加按钮功能
            Button slotBtn = slotGO.AddComponent<Button>();
            slotBtn.targetGraphic = bgImage;
            slotBtn.onClick.AddListener(() => OnSkillSelected(skill));

            // 添加SkillSlotUI组件
            SkillSlotUI slotUI = slotGO.AddComponent<SkillSlotUI>();
            slotUI.Initialize(skill, GetPlayerLevel());
            skillUIs.Add(slotUI);
        }

        /// <summary>
        /// 技能选中回调
        /// </summary>
        void OnSkillSelected(SkillData skill)
        {
            selectedSkill = skill;
            ShowSkillDetail(skill);

            // 高亮选中的技能
            foreach (var ui in skillUIs)
            {
                if (ui != null)
                {
                    ui.SetSelected(ui.GetSkillData().id == skill.id);
                }
            }
        }

        /// <summary>
        /// 显示技能详情
        /// </summary>
        void ShowSkillDetail(SkillData skill)
        {
            if (detailPanel == null)
            {
                // 动态创建详情面板
                CreateDetailPanel();
            }

            detailPanel.SetActive(true);

            // 更新详情信息
            if (detailName != null)
                detailName.text = skill.name;

            if (detailDescription != null)
                detailDescription.text = $"{skill.description}\n\n每级效果: {skill.effectPerLevel}";

            if (detailLevel != null)
                detailLevel.text = $"等级: {skill.currentLevel}/{skill.maxLevel}";

            // 计算升级消耗
            int upgradeCost = skill.GetUpgradeCost();
            bool canUpgrade = skill.currentLevel < skill.maxLevel && GetPlayerGold() >= upgradeCost;

            if (detailCost != null)
            {
                if (skill.currentLevel >= skill.maxLevel)
                    detailCost.text = "已满级";
                else
                    detailCost.text = $"升级消耗: {upgradeCost} 金币";
            }

            // 更新升级按钮状态
            if (upgradeButton != null)
            {
                upgradeButton.interactable = canUpgrade;
                var btnImage = upgradeButton.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = canUpgrade ? new Color(0.3f, 0.7f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);
                }
            }

            // 加载技能图标
            if (detailIcon != null)
            {
                Sprite skillIcon = Resources.Load<Sprite>(skill.iconPath);
                if (skillIcon != null)
                {
                    detailIcon.sprite = skillIcon;
                    detailIcon.color = Color.white;
                }
                else
                {
                    detailIcon.color = skill.skillType == SkillType.Active ?
                        new Color(0.8f, 0.4f, 0.2f) : new Color(0.2f, 0.6f, 0.8f);
                }
            }
        }

        /// <summary>
        /// 动态创建详情面板
        /// </summary>
        void CreateDetailPanel()
        {
            // 创建详情面板（右侧）
            GameObject panelGO = new GameObject("DetailPanel");
            panelGO.transform.SetParent(transform, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.55f, 0.1f);
            panelRect.anchorMax = new Vector2(0.95f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelBg = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
            detailPanel = panelGO;

            // 技能图标
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(panelGO.transform, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1);
            iconRect.anchorMax = new Vector2(0.5f, 1);
            iconRect.anchoredPosition = new Vector2(0, -60);
            iconRect.sizeDelta = new Vector2(80, 80);
            detailIcon = iconGO.AddComponent<Image>();
            detailIcon.color = Color.gray;

            // 技能名称
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(panelGO.transform, false);
            RectTransform nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.anchoredPosition = new Vector2(0, -120);
            nameRect.sizeDelta = new Vector2(0, 40);
            detailName = nameGO.AddComponent<Text>();
            detailName.fontSize = 28;
            detailName.fontStyle = FontStyle.Bold;
            detailName.alignment = TextAnchor.MiddleCenter;
            detailName.color = Color.white;
            detailName.font = GetDefaultFont();

            // 技能等级
            GameObject levelGO = new GameObject("Level");
            levelGO.transform.SetParent(panelGO.transform, false);
            RectTransform levelRect = levelGO.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0, 1);
            levelRect.anchorMax = new Vector2(1, 1);
            levelRect.anchoredPosition = new Vector2(0, -160);
            levelRect.sizeDelta = new Vector2(0, 30);
            detailLevel = levelGO.AddComponent<Text>();
            detailLevel.fontSize = 20;
            detailLevel.alignment = TextAnchor.MiddleCenter;
            detailLevel.color = new Color(0.8f, 0.7f, 0.4f);
            detailLevel.font = GetDefaultFont();

            // 技能描述
            GameObject descGO = new GameObject("Description");
            descGO.transform.SetParent(panelGO.transform, false);
            RectTransform descRect = descGO.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.05f, 0.3f);
            descRect.anchorMax = new Vector2(0.95f, 0.65f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            detailDescription = descGO.AddComponent<Text>();
            detailDescription.fontSize = 18;
            detailDescription.alignment = TextAnchor.UpperLeft;
            detailDescription.color = new Color(0.8f, 0.8f, 0.8f);
            detailDescription.font = GetDefaultFont();

            // 升级消耗
            GameObject costGO = new GameObject("Cost");
            costGO.transform.SetParent(panelGO.transform, false);
            RectTransform costRect = costGO.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 0);
            costRect.anchorMax = new Vector2(1, 0);
            costRect.anchoredPosition = new Vector2(0, 90);
            costRect.sizeDelta = new Vector2(0, 30);
            detailCost = costGO.AddComponent<Text>();
            detailCost.fontSize = 20;
            detailCost.alignment = TextAnchor.MiddleCenter;
            detailCost.color = new Color(1f, 0.85f, 0.2f);
            detailCost.font = GetDefaultFont();

            // 升级按钮
            GameObject btnGO = new GameObject("UpgradeButton");
            btnGO.transform.SetParent(panelGO.transform, false);
            RectTransform btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0);
            btnRect.anchorMax = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 40);
            btnRect.sizeDelta = new Vector2(150, 50);
            Image btnBg = btnGO.AddComponent<Image>();
            btnBg.color = new Color(0.3f, 0.7f, 0.3f);
            upgradeButton = btnGO.AddComponent<Button>();
            upgradeButton.targetGraphic = btnBg;
            upgradeButton.onClick.AddListener(OnUpgradeClick);

            // 按钮文字
            GameObject btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            RectTransform btnTextRect = btnTextGO.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.text = "升级";
            btnText.fontSize = 24;
            btnText.fontStyle = FontStyle.Bold;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.font = GetDefaultFont();
        }

        /// <summary>
        /// 升级按钮点击
        /// </summary>
        void OnUpgradeClick()
        {
            if (selectedSkill == null) return;

            int cost = selectedSkill.GetUpgradeCost();
            int playerGold = GetPlayerGold();

            if (playerGold < cost)
            {
                Debug.Log($"[SkillUpgradePanel] 金币不足! 需要 {cost}, 拥有 {playerGold}");
                return;
            }

            if (selectedSkill.currentLevel >= selectedSkill.maxLevel)
            {
                Debug.Log("[SkillUpgradePanel] 技能已满级!");
                return;
            }

            // 扣除金币
            SpendGold(cost);

            // 升级技能
            selectedSkill.currentLevel++;
            Debug.Log($"[SkillUpgradePanel] 技能升级成功: {selectedSkill.name} -> Lv.{selectedSkill.currentLevel}");

            // 播放升级音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_UI_LevelUp");
            }

            // 刷新UI
            RefreshUI();
            ShowSkillDetail(selectedSkill);
        }

        /// <summary>
        /// 刷新整体UI
        /// </summary>
        void RefreshUI()
        {
            // 刷新金币显示
            if (goldText != null)
            {
                goldText.text = $"金币: {GetPlayerGold()}";
            }

            // 刷新技能列表
            foreach (var ui in skillUIs)
            {
                if (ui != null)
                    ui.Refresh();
            }
        }

        /// <summary>
        /// 获取玩家金币
        /// </summary>
        int GetPlayerGold()
        {
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.SessionGold;
            }
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
            {
                return SaveSystem.Instance.CurrentPlayerStats.gold;
            }
            return 0;
        }

        /// <summary>
        /// 获取玩家等级
        /// </summary>
        int GetPlayerLevel()
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
            {
                return SaveSystem.Instance.CurrentPlayerStats.level;
            }
            return 1;
        }

        /// <summary>
        /// 消费金币
        /// </summary>
        bool SpendGold(int amount)
        {
            // 优先通过GameManager扣金币（同步SessionGold和SaveSystem）
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.SpendGold(amount);
            }
            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
            {
                return SaveSystem.Instance.CurrentPlayerStats.SpendGold(amount);
            }
            return false;
        }

        Font GetDefaultFont()
        {
            string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
            foreach (string fontName in fontNames)
            {
                Font font = Resources.GetBuiltinResource<Font>(fontName);
                if (font != null) return font;
            }
            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshUI();
            if (detailPanel != null)
                detailPanel.SetActive(false);

            // 暂停游戏
            Time.timeScale = 0f;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }
    }

    /// <summary>
    /// 技能类型
    /// </summary>
    public enum SkillType
    {
        Active,
        Passive
    }

    /// <summary>
    /// 技能数据
    /// </summary>
    [System.Serializable]
    public class SkillData
    {
        public string id;
        public string name;
        public string description;
        public SkillType skillType;
        public int maxLevel;
        public int currentLevel;
        public int unlockLevel;      // 解锁所需玩家等级
        public float baseCooldown;
        public int baseGoldCost;
        public float levelGoldMultiplier;
        public string iconPath;
        public string effectPerLevel;

        /// <summary>
        /// 计算升级所需金币
        /// </summary>
        public int GetUpgradeCost()
        {
            return Mathf.RoundToInt(baseGoldCost * Mathf.Pow(levelGoldMultiplier, currentLevel));
        }

        /// <summary>
        /// 检查是否已解锁
        /// </summary>
        public bool IsUnlocked(int playerLevel)
        {
            return playerLevel >= unlockLevel;
        }
    }

    /// <summary>
    /// 技能配置表
    /// </summary>
    [System.Serializable]
    public class SkillConfigTable
    {
        public List<SkillData> skills;
    }

    /// <summary>
    /// 技能槽位UI组件
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        private SkillData skillData;
        private int playerLevel;
        private bool isSelected;

        private Image iconImage;
        private Text nameText;
        private Text levelText;
        private Image lockOverlay;
        private Image bgImage;

        public void Initialize(SkillData data, int level)
        {
            skillData = data;
            playerLevel = level;
            CreateUI();
        }

        void CreateUI()
        {
            bgImage = GetComponent<Image>();

            // 图标
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(transform, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(50, 0);
            iconRect.sizeDelta = new Vector2(60, 60);
            iconImage = iconGO.AddComponent<Image>();

            // 尝试加载图标
            Sprite skillIcon = Resources.Load<Sprite>(skillData.iconPath);
            if (skillIcon != null)
            {
                iconImage.sprite = skillIcon;
            }
            else
            {
                iconImage.color = skillData.skillType == SkillType.Active ?
                    new Color(0.8f, 0.4f, 0.2f) : new Color(0.2f, 0.6f, 0.8f);
            }

            // 名称
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(transform, false);
            RectTransform nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(0, 0.5f);
            nameRect.anchoredPosition = new Vector2(150, 15);
            nameRect.sizeDelta = new Vector2(200, 30);
            nameText = nameGO.AddComponent<Text>();
            nameText.text = skillData.name;
            nameText.fontSize = 20;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.color = Color.white;
            nameText.font = GetDefaultFont();

            // 等级
            GameObject levelGO = new GameObject("Level");
            levelGO.transform.SetParent(transform, false);
            RectTransform levelRect = levelGO.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0, 0.5f);
            levelRect.anchorMax = new Vector2(0, 0.5f);
            levelRect.anchoredPosition = new Vector2(150, -15);
            levelRect.sizeDelta = new Vector2(150, 25);
            levelText = levelGO.AddComponent<Text>();
            levelText.fontSize = 16;
            levelText.alignment = TextAnchor.MiddleLeft;
            levelText.color = new Color(0.7f, 0.7f, 0.7f);
            levelText.font = GetDefaultFont();

            // 锁定遮罩
            if (!skillData.IsUnlocked(playerLevel))
            {
                GameObject lockGO = new GameObject("LockOverlay");
                lockGO.transform.SetParent(transform, false);
                RectTransform lockRect = lockGO.AddComponent<RectTransform>();
                lockRect.anchorMin = Vector2.zero;
                lockRect.anchorMax = Vector2.one;
                lockRect.offsetMin = Vector2.zero;
                lockRect.offsetMax = Vector2.zero;
                lockOverlay = lockGO.AddComponent<Image>();
                lockOverlay.color = new Color(0, 0, 0, 0.6f);

                // 锁定文字
                GameObject lockTextGO = new GameObject("LockText");
                lockTextGO.transform.SetParent(lockGO.transform, false);
                RectTransform lockTextRect = lockTextGO.AddComponent<RectTransform>();
                lockTextRect.anchorMin = Vector2.zero;
                lockTextRect.anchorMax = Vector2.one;
                lockTextRect.offsetMin = Vector2.zero;
                lockTextRect.offsetMax = Vector2.zero;
                Text lockText = lockTextGO.AddComponent<Text>();
                lockText.text = $"需要等级 {skillData.unlockLevel}";
                lockText.fontSize = 18;
                lockText.alignment = TextAnchor.MiddleCenter;
                lockText.color = Color.red;
                lockText.font = GetDefaultFont();
            }

            Refresh();
        }

        public void Refresh()
        {
            if (levelText != null)
            {
                if (skillData.currentLevel > 0)
                    levelText.text = $"Lv.{skillData.currentLevel}/{skillData.maxLevel}";
                else
                    levelText.text = "未学习";
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (bgImage != null)
            {
                bgImage.color = selected ?
                    new Color(0.35f, 0.45f, 0.55f, 0.95f) :
                    new Color(0.25f, 0.25f, 0.3f, 0.9f);
            }
        }

        public SkillData GetSkillData()
        {
            return skillData;
        }

        Font GetDefaultFont()
        {
            string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
            foreach (string fontName in fontNames)
            {
                Font font = Resources.GetBuiltinResource<Font>(fontName);
                if (font != null) return font;
            }
            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }
    }
}
