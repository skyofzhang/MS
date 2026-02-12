using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using MoShou.Systems;
using MoShou.UI;
using MoShou.Data;

/// <summary>
/// 选关场景初始化 - Prefab方式
/// 从Resources加载StageSelectCanvas和StageCard预制体
/// 布局由Prefab控制，代码只负责数据填充和事件绑定
/// </summary>
public class StageSelectSceneSetup : MonoBehaviour
{
    private static bool isInitialized = false;

    // 关卡配置缓存
    private StageConfigTable stageConfigTable;

    // 预加载的sprite缓存
    private Sprite starFilled;
    private Sprite starEmpty;
    private Sprite[] regionThumbnails;

    // 区域名（每10关一个区域）
    private static readonly string[] RegionNames = {
        "Forest", "Desert", "Element", "Lava", "Ice",
        "Giant", "Swamp", "Shadow", "Undead", "Final"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
    }

    static void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StageSelect")
        {
            var temp = new GameObject("_StageSelectLoader");
            temp.AddComponent<StageSelectDelayedSetup>();
        }
    }

    private class StageSelectDelayedSetup : MonoBehaviour
    {
        void Start()
        {
            if (FindObjectOfType<StageSelectSceneSetup>() == null)
            {
                var go = new GameObject("StageSelectSceneSetup");
                go.AddComponent<StageSelectSceneSetup>();
            }
            Destroy(gameObject);
        }
    }

    void Awake()
    {
        if (isInitialized)
        {
            Destroy(gameObject);
            return;
        }
        isInitialized = true;
        SetupStageSelect();
    }

    void OnDestroy()
    {
        isInitialized = false;
    }

    void SetupStageSelect()
    {
        Debug.Log("[StageSelectSetup] 开始创建选关UI（Prefab方式）...");

        // 加载关卡配置
        LoadStageConfigs();

        // 预加载sprite
        PreloadSprites();

        // 确保有EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 销毁所有现有的低优先级Canvas
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in existingCanvases)
        {
            if (canvas.sortingOrder < 500)
            {
                Debug.Log($"[StageSelectSetup] 销毁现有Canvas: {canvas.gameObject.name}");
                DestroyImmediate(canvas.gameObject);
            }
        }

        // 销毁旧的Manager和Screen
        var oldManager = FindObjectOfType<MoShou.Core.StageSelectManager>();
        if (oldManager != null) DestroyImmediate(oldManager.gameObject);

        var oldScreen = FindObjectOfType<MoShou.UI.StageSelectScreen>();
        if (oldScreen != null) DestroyImmediate(oldScreen.gameObject);

        var stageButtonsParent = GameObject.Find("StageButtonsParent");
        if (stageButtonsParent != null) DestroyImmediate(stageButtonsParent);

        // 实例化Canvas预制体
        GameObject canvasPrefab = Resources.Load<GameObject>("Prefabs/UI/StageSelectCanvas");
        if (canvasPrefab == null)
        {
            Debug.LogError("[StageSelectSetup] 找不到 Prefabs/UI/StageSelectCanvas 预制体！请在Unity中创建。");
            return;
        }

        GameObject canvasInstance = Instantiate(canvasPrefab);
        canvasInstance.name = "StageSelectCanvas";

        // 查找Content容器（ScrollView > Viewport > Content）
        Transform content = FindDeepChild(canvasInstance.transform, "Content");
        if (content == null)
        {
            Debug.LogError("[StageSelectSetup] Canvas预制体中找不到名为 'Content' 的子物体！");
            return;
        }

        // 加载卡片预制体
        GameObject cardPrefab = Resources.Load<GameObject>("Prefabs/UI/StageCard");
        if (cardPrefab == null)
        {
            Debug.LogError("[StageSelectSetup] 找不到 Prefabs/UI/StageCard 预制体！请在Unity中创建。");
            return;
        }

        // 获取存档数据
        int highestUnlocked = 1;
        if (SaveSystem.Instance != null)
        {
            highestUnlocked = SaveSystem.Instance.GetHighestUnlockedStage();
        }

        // 生成100张关卡卡片
        int totalStages = 100;
        for (int i = 1; i <= totalStages; i++)
        {
            StageConfigEntry config = GetStageConfig(i);

            bool isCleared = SaveSystem.Instance != null
                ? SaveSystem.Instance.IsStageCleared(i)
                : i < highestUnlocked;
            bool isUnlocked = i <= highestUnlocked;
            bool isCurrent = i == highestUnlocked;
            bool isLocked = !isUnlocked;

            int starCount = 0;
            if (isCleared && SaveSystem.Instance != null)
            {
                starCount = SaveSystem.Instance.GetStageStars(i);
                if (starCount < 1) starCount = 1;
                if (starCount > 3) starCount = 3;
            }

            // 实例化卡片
            GameObject cardObj = Instantiate(cardPrefab, content);
            StageCardUI cardUI = cardObj.GetComponent<StageCardUI>();

            if (cardUI == null)
            {
                Debug.LogWarning($"[StageSelectSetup] StageCard预制体缺少StageCardUI组件！关卡{i}跳过");
                continue;
            }

            // 获取缩略图
            Sprite thumbSprite = GetStageThumbnail(i);

            // 配置名和信息
            string displayName = !string.IsNullOrEmpty(config.name) ? config.name : $"关卡{i}";
            string infoLine = $"推荐等级: Lv.{config.recommendedLevel}    波数: {config.waveCount}";

            int capturedStageNum = i;
            StageConfigEntry capturedConfig = config;

            cardUI.Setup(
                stageNum: i,
                displayName: displayName,
                infoLine: infoLine,
                isLocked: isLocked,
                isCleared: isCleared,
                isCurrent: isCurrent,
                starCount: starCount,
                thumbSprite: thumbSprite,
                starFilled: starFilled,
                starEmpty: starEmpty,
                onClick: () => ShowStageConfirm(capturedStageNum, capturedConfig)
            );
        }

        // 填充底部信息栏数据
        FillBottomInfoBar(canvasInstance, highestUnlocked, totalStages);

        // 创建关闭/返回按钮（运行时动态创建，不修改Prefab）
        CreateCloseButton(canvasInstance);

        // 自动滚动到当前关卡
        ScrollRect scrollRect = canvasInstance.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null && scrollRect.content != null)
        {
            StartCoroutine(ScrollToCurrentStage(scrollRect, scrollRect.content, highestUnlocked, totalStages));
        }

        Debug.Log("[StageSelectSetup] 选关UI创建完成（Prefab方式）");
    }

    #region 数据加载

    void LoadStageConfigs()
    {
        TextAsset json = Resources.Load<TextAsset>("Configs/StageConfigs");
        if (json != null)
        {
            stageConfigTable = JsonUtility.FromJson<StageConfigTable>(json.text);
            Debug.Log($"[StageSelectSetup] 加载了 {stageConfigTable.stages.Length} 个关卡配置");
        }
        else
        {
            Debug.LogWarning("[StageSelectSetup] 无法加载 StageConfigs.json，使用算法生成");
            stageConfigTable = new StageConfigTable { stages = new StageConfigEntry[0] };
        }
    }

    void PreloadSprites()
    {
        starFilled = Resources.Load<Sprite>("Sprites/UI/Result/UI_Result_Star_Filled");
        starEmpty = Resources.Load<Sprite>("Sprites/UI/Result/UI_Result_Star_Empty");

        regionThumbnails = new Sprite[RegionNames.Length];
        for (int i = 0; i < RegionNames.Length; i++)
        {
            regionThumbnails[i] = Resources.Load<Sprite>($"Sprites/UI/StageSelect/UI_Stage_Thumb_{RegionNames[i]}");
        }
    }

    StageConfigEntry GetStageConfig(int stageNum)
    {
        if (stageConfigTable != null && stageConfigTable.stages != null)
        {
            foreach (var entry in stageConfigTable.stages)
            {
                if (entry.id == stageNum)
                    return entry;
            }
        }

        // 算法生成 fallback
        string[] themeNames = { "未知区域", "荒野", "山脉", "沙漠", "冰原", "火山", "深渊", "天空", "混沌", "终末" };
        int regionIdx = Mathf.Clamp((stageNum - 1) / 10, 0, themeNames.Length - 1);

        return new StageConfigEntry
        {
            id = stageNum,
            name = $"{themeNames[regionIdx]}·关卡{stageNum}",
            chapter = regionIdx + 1,
            difficulty = Mathf.Min(5, 1 + (stageNum - 1) / 20),
            recommendedLevel = Mathf.Max(1, stageNum * 2 - 1),
            waveCount = 3 + stageNum / 10,
            goldReward = Mathf.FloorToInt(50 + stageNum * 30 + stageNum * stageNum * 0.5f),
            expReward = Mathf.FloorToInt(30 + stageNum * 20 + stageNum * stageNum / 3f)
        };
    }

    Sprite GetStageThumbnail(int stageId)
    {
        int regionIdx = Mathf.Clamp((stageId - 1) / 10, 0, RegionNames.Length - 1);
        if (regionThumbnails != null && regionIdx < regionThumbnails.Length)
        {
            return regionThumbnails[regionIdx];
        }
        return null;
    }

    #endregion

    #region 底部信息栏

    void FillBottomInfoBar(GameObject canvasInstance, int highestUnlocked, int totalStages)
    {
        Transform barRoot = FindDeepChild(canvasInstance.transform, "BottomInfoBar");
        if (barRoot == null)
        {
            Debug.LogWarning("[StageSelectSetup] 未找到BottomInfoBar，跳过底栏数据填充");
            return;
        }

        // 读取玩家数据
        int playerLevel = 1;
        int playerGold = 0;
        if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentPlayerStats != null)
        {
            playerLevel = SaveSystem.Instance.CurrentPlayerStats.level;
            playerGold = SaveSystem.Instance.CurrentPlayerStats.gold;
        }

        // 计算已通关数
        int clearedCount = 0;
        if (SaveSystem.Instance != null)
        {
            for (int i = 1; i <= totalStages; i++)
            {
                if (SaveSystem.Instance.IsStageCleared(i))
                    clearedCount++;
            }
        }

        float progressPercent = (float)clearedCount / totalStages * 100f;

        // === 等级 ===
        Transform levelNumT = FindDeepChild(barRoot, "LevelNum");
        if (levelNumT != null)
        {
            Text txt = levelNumT.GetComponent<Text>();
            if (txt != null) txt.text = playerLevel.ToString();
        }

        Transform levelLabelT = FindDeepChild(barRoot, "LevelLabel");
        if (levelLabelT != null)
        {
            Text txt = levelLabelT.GetComponent<Text>();
            if (txt != null) txt.text = $"Lv.{playerLevel}";
        }

        // === 进度 ===
        Transform progressLabelT = FindDeepChild(barRoot, "ProgressLabel");
        if (progressLabelT != null)
        {
            Text txt = progressLabelT.GetComponent<Text>();
            if (txt != null) txt.text = $"进度: {clearedCount}/{totalStages}";
        }

        Transform percentT = FindDeepChild(barRoot, "Percent");
        if (percentT != null)
        {
            Text txt = percentT.GetComponent<Text>();
            if (txt != null) txt.text = $"{progressPercent:F0}%";
        }

        // 进度条填充 - 通过anchorMax.x控制宽度比例
        Transform barFillT = FindDeepChild(barRoot, "BarFill");
        if (barFillT != null)
        {
            RectTransform fillRect = barFillT as RectTransform;
            if (fillRect != null)
            {
                float fillRatio = Mathf.Clamp01((float)clearedCount / totalStages);
                fillRatio = Mathf.Max(fillRatio, 0.02f); // 最小显示2%
                fillRect.anchorMax = new Vector2(fillRatio, 1f);
            }
        }

        // === 金币 ===
        Transform goldAmountT = FindDeepChild(barRoot, "GoldAmount");
        if (goldAmountT != null)
        {
            Text txt = goldAmountT.GetComponent<Text>();
            if (txt != null)
            {
                if (playerGold >= 10000)
                    txt.text = $"{playerGold / 1000f:F1}K";
                else
                    txt.text = $"{playerGold}金币";
            }
        }

        Debug.Log($"[StageSelectSetup] 底栏数据: Lv.{playerLevel}, 进度{clearedCount}/{totalStages}, 金币{playerGold}");
    }

    #endregion

    #region 交互逻辑

    void CreateCloseButton(GameObject canvasInstance)
    {
        Sprite closeSprite = Resources.Load<Sprite>("Sprites/UI/Common/UI_Btn_Close_X");

        GameObject btnGO = new GameObject("CloseButton");
        btnGO.transform.SetParent(canvasInstance.transform, false);

        RectTransform rect = btnGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(70, 70);

        Image img = btnGO.AddComponent<Image>();
        if (closeSprite != null)
        {
            img.sprite = closeSprite;
            img.color = Color.white;
        }
        else
        {
            img.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        }

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }

    void ShowStageConfirm(int stageNum, StageConfigEntry config)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentLevel = stageNum;
        }

        string title = $"关卡 {stageNum}: {config.name}";
        string msg = $"推荐等级: {config.recommendedLevel}\n波次: {config.waveCount}\n\n是否进入此关卡？";

        if (ConfirmDialog.Instance != null)
        {
            ConfirmDialog.Instance.Show(
                title,
                msg,
                () => SceneManager.LoadScene("GameScene"),
                null
            );
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    IEnumerator ScrollToCurrentStage(ScrollRect scrollRect, RectTransform content, int currentStage, int totalStages)
    {
        yield return null;
        yield return null;

        if (currentStage <= 1) yield break;

        float totalHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        if (totalHeight <= viewportHeight) yield break;

        // 通过LayoutGroup参数计算目标位置
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        float spacing = vlg != null ? vlg.spacing : 12f;
        float topPadding = vlg != null ? vlg.padding.top : 20f;

        // 获取卡片高度（从第一个子物体）
        float cardHeight = 140f;
        if (content.childCount > 0)
        {
            var firstChild = content.GetChild(0) as RectTransform;
            if (firstChild != null) cardHeight = firstChild.rect.height;
        }

        float targetY = topPadding + (currentStage - 1) * (cardHeight + spacing);
        targetY -= viewportHeight / 2 - cardHeight / 2;
        targetY = Mathf.Clamp(targetY, 0, totalHeight - viewportHeight);

        float normalizedPos = 1f - (targetY / (totalHeight - viewportHeight));
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 递归查找子物体（按名字）
    /// </summary>
    static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    #endregion
}
