using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using MoShou.Utils;

/// <summary>
/// Unity Editor tool for quick scene setup
/// V4.0: 资源先行模式 - 使用ResourceLoader加载真实资源
/// Run from menu: MoShou/Setup All Scenes
/// </summary>
public class SceneSetupTool : EditorWindow
{
    private const string TAG = "[SceneSetup]";

    [MenuItem("MoShou/Setup All Scenes")]
    public static void SetupAllScenes()
    {
        if (!EditorUtility.DisplayDialog("Setup Scenes",
            "This will create and setup all game scenes:\n\n" +
            "- MainMenu\n" +
            "- StageSelect\n" +
            "- GameScene\n\n" +
            "注意: 资源先行模式 - 确保Resources文件夹中已有对应资源\n\n" +
            "Continue?", "Yes", "Cancel"))
        {
            return;
        }

        CreateScenesFolder();
        CreateResourcesFolderStructure();
        CreateMainMenuScene();
        CreateStageSelectScene();
        CreateGameScene();
        SetupBuildSettings();

        EditorUtility.DisplayDialog("Complete",
            "All scenes created successfully!\n\n" +
            "Next steps:\n" +
            "1. Run MoShou/Visual Self Test 验证资源\n" +
            "2. Open each scene and verify\n" +
            "3. File > Build Settings > Build",
            "OK");
    }

    private static void CreateScenesFolder()
    {
        string scenesPath = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }

    /// <summary>
    /// 创建Resources文件夹结构 - 符合资源路径映射规范
    /// </summary>
    private static void CreateResourcesFolderStructure()
    {
        string[] folders = new string[]
        {
            "Assets/Resources",
            "Assets/Resources/Models",
            "Assets/Resources/Models/Player",
            "Assets/Resources/Models/Monsters",
            "Assets/Resources/Sprites",
            "Assets/Resources/Sprites/UI",
            "Assets/Resources/Sprites/UI/HUD",
            "Assets/Resources/Sprites/UI/Icons",
            "Assets/Resources/Sprites/UI/Buttons",
            "Assets/Resources/Sprites/UI/Backgrounds",
            "Assets/Resources/Prefabs",
            "Assets/Resources/Prefabs/VFX",
            "Assets/Resources/Prefabs/VFX/Skills",
            "Assets/Resources/Audio",
            "Assets/Resources/Audio/BGM",
            "Assets/Resources/Audio/SFX"
        };

        foreach (var folderPath in folders)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
                string folderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parent, folderName);
                Debug.Log($"{TAG} Created folder: {folderPath}");
            }
        }

        AssetDatabase.Refresh();
    }

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create UI Canvas
        var canvas = CreateCanvas("MainMenuCanvas");

        // Create Main Menu Manager
        var managerObj = new GameObject("MainMenuManager");
        managerObj.AddComponent<MoShou.Core.MainMenuManager>();

        // 尝试加载真实UI资源，使用ResourceLoader
        CreateUIButton(canvas.transform, "StartButton", "开始游戏", new Vector2(0, 50), "Sprites/UI/Buttons/btn_start");
        CreateUIButton(canvas.transform, "ContinueButton", "继续游戏", new Vector2(0, -20), "Sprites/UI/Buttons/btn_continue");
        CreateUIButton(canvas.transform, "SettingsButton", "设置", new Vector2(0, -90), "Sprites/UI/Buttons/btn_settings");
        CreateUIButton(canvas.transform, "QuitButton", "退出", new Vector2(0, -160), "Sprites/UI/Buttons/btn_quit");

        // 尝试加载背景
        TryLoadBackground(canvas.transform, "Sprites/UI/Backgrounds/bg_mainmenu");

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log($"{TAG} MainMenu scene created");
    }

    private static void CreateStageSelectScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create UI Canvas
        var canvas = CreateCanvas("StageSelectCanvas");

        // Create Stage Select Manager
        var managerObj = new GameObject("StageSelectManager");
        managerObj.AddComponent<MoShou.Core.StageSelectManager>();

        // Create title using real resources
        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvas.transform);
        var titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleText.text = "选择关卡";
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 300);
        titleRect.sizeDelta = new Vector2(400, 60);

        // Create stage buttons container
        var stageContainer = new GameObject("StageButtonsContainer");
        stageContainer.transform.SetParent(canvas.transform);
        var containerRect = stageContainer.AddComponent<RectTransform>();
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(800, 400);
        var gridLayout = stageContainer.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(120, 120);
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;

        // Back button with real resource
        CreateUIButton(canvas.transform, "BackButton", "返回", new Vector2(-300, -350), "Sprites/UI/Buttons/btn_back");

        // 尝试加载背景
        TryLoadBackground(canvas.transform, "Sprites/UI/Backgrounds/bg_stageselect");

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/StageSelect.unity");
        Debug.Log($"{TAG} StageSelect scene created");
    }

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create GameInitializer (will create other managers)
        var initObj = new GameObject("GameInitializer");
        initObj.AddComponent<MoShou.Core.GameInitializer>();

        // 尝试加载地面模型，否则使用Plane
        var groundPrefab = Resources.Load<GameObject>("Models/Environment/Ground");
        GameObject ground;
        if (groundPrefab != null)
        {
            ground = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab);
            ground.name = "Ground";
            Debug.Log($"{TAG} Loaded ground model from resources");
        }
        else
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5, 1, 5);
            // 标记为需要替换的临时对象
            ground.tag = "EditorOnly";
            Debug.LogWarning($"{TAG} Ground model not found, using primitive plane (marked EditorOnly)");
        }

        // Create player spawn point
        var spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = Vector3.zero;
        // 添加Gizmo可视化
        var gizmo = spawnPoint.AddComponent<SpawnPointGizmo>();

        // Create monster spawn points
        for (int i = 0; i < 4; i++)
        {
            var monsterSpawn = new GameObject($"MonsterSpawnPoint_{i}");
            float angle = i * 90f * Mathf.Deg2Rad;
            monsterSpawn.transform.position = new Vector3(
                Mathf.Cos(angle) * 15f,
                0,
                Mathf.Sin(angle) * 15f
            );
            monsterSpawn.AddComponent<SpawnPointGizmo>();
        }

        // Create UI Canvas for HUD
        var canvas = CreateCanvas("GameHUDCanvas");

        // 创建HUD元素 - 使用真实资源
        CreateHUDElements(canvas.transform);

        // Create MonsterSpawner
        var spawnerObj = new GameObject("MonsterSpawner");
        var spawner = spawnerObj.AddComponent<MonsterSpawner>();
        // Assign spawn points
        var spawnPoints = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            spawnPoints[i] = GameObject.Find($"MonsterSpawnPoint_{i}").transform;
        }
        spawner.spawnPoints = spawnPoints;

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log($"{TAG} GameScene scene created");
    }

    private static GameObject CreateCanvas(string name)
    {
        var canvasObj = new GameObject(name);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Event System
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    /// <summary>
    /// 创建UI按钮 - 资源先行模式
    /// 优先使用Resources中的真实资源，无资源时使用白色Image并记录警告
    /// </summary>
    private static void CreateUIButton(Transform parent, string name, string text, Vector2 position, string spritePath)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);

        var image = buttonObj.AddComponent<UnityEngine.UI.Image>();

        // 尝试加载真实按钮资源
        var sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            Debug.Log($"{TAG} Loaded button sprite: {spritePath}");
        }
        else
        {
            // 资源未找到，使用默认颜色并记录警告
            image.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            Debug.LogWarning($"{TAG} Button sprite not found: {spritePath}, using default color");
        }

        var button = buttonObj.AddComponent<UnityEngine.UI.Button>();

        var rect = buttonObj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 50);

        // Add text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
        textComp.text = text;
        textComp.fontSize = 24;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 尝试加载背景图
    /// </summary>
    private static void TryLoadBackground(Transform parent, string spritePath)
    {
        var bgSprite = Resources.Load<Sprite>(spritePath);
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(parent);
            bgObj.transform.SetAsFirstSibling(); // 放到最底层

            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.sprite = bgSprite;
            bgImage.type = UnityEngine.UI.Image.Type.Sliced;

            var rect = bgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            Debug.Log($"{TAG} Loaded background: {spritePath}");
        }
        else
        {
            Debug.LogWarning($"{TAG} Background not found: {spritePath}");
        }
    }

    /// <summary>
    /// 创建HUD元素
    /// </summary>
    private static void CreateHUDElements(Transform canvas)
    {
        // 血条容器
        var healthBarContainer = new GameObject("HealthBarContainer");
        healthBarContainer.transform.SetParent(canvas);
        var containerRect = healthBarContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);
        containerRect.sizeDelta = new Vector2(300, 30);

        // 血条背景
        var healthBg = new GameObject("HealthBarBg");
        healthBg.transform.SetParent(healthBarContainer.transform);
        var bgImage = healthBg.AddComponent<UnityEngine.UI.Image>();

        var bgSprite = Resources.Load<Sprite>("Sprites/UI/HUD/health_bar_bg");
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
        }
        else
        {
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }

        var bgRect = healthBg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 血条填充
        var healthFill = new GameObject("HealthBarFill");
        healthFill.transform.SetParent(healthBarContainer.transform);
        var fillImage = healthFill.AddComponent<UnityEngine.UI.Image>();

        var fillSprite = Resources.Load<Sprite>("Sprites/UI/HUD/health_bar_fill");
        if (fillSprite != null)
        {
            fillImage.sprite = fillSprite;
            fillImage.type = UnityEngine.UI.Image.Type.Filled;
            fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        }
        else
        {
            fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        }

        var fillRect = healthFill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.02f, 0.1f);
        fillRect.anchorMax = new Vector2(0.98f, 0.9f);
        fillRect.sizeDelta = Vector2.zero;

        // 技能栏容器
        var skillBarContainer = new GameObject("SkillBarContainer");
        skillBarContainer.transform.SetParent(canvas);
        var skillRect = skillBarContainer.AddComponent<RectTransform>();
        skillRect.anchorMin = new Vector2(0.5f, 0);
        skillRect.anchorMax = new Vector2(0.5f, 0);
        skillRect.pivot = new Vector2(0.5f, 0);
        skillRect.anchoredPosition = new Vector2(0, 20);
        skillRect.sizeDelta = new Vector2(400, 80);

        var skillLayout = skillBarContainer.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        skillLayout.spacing = 10;
        skillLayout.childAlignment = TextAnchor.MiddleCenter;
        skillLayout.childForceExpandWidth = false;
        skillLayout.childForceExpandHeight = false;

        // 创建4个技能槽位
        for (int i = 0; i < 4; i++)
        {
            CreateSkillSlot(skillBarContainer.transform, i);
        }

        Debug.Log($"{TAG} HUD elements created");
    }

    /// <summary>
    /// 创建技能槽位
    /// </summary>
    private static void CreateSkillSlot(Transform parent, int index)
    {
        var slotObj = new GameObject($"SkillSlot_{index}");
        slotObj.transform.SetParent(parent);

        var slotImage = slotObj.AddComponent<UnityEngine.UI.Image>();
        var slotSprite = Resources.Load<Sprite>("Sprites/UI/HUD/skill_slot_bg");
        if (slotSprite != null)
        {
            slotImage.sprite = slotSprite;
        }
        else
        {
            slotImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }

        var slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(70, 70);

        // 技能图标
        var iconObj = new GameObject("SkillIcon");
        iconObj.transform.SetParent(slotObj.transform);
        var iconImage = iconObj.AddComponent<UnityEngine.UI.Image>();

        // 尝试加载对应技能图标
        string[] skillNames = { "skill_attack", "skill_fireball", "skill_heal", "skill_dash" };
        var iconSprite = Resources.Load<Sprite>($"Sprites/UI/Icons/{skillNames[index]}");
        if (iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }
        else
        {
            iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        var iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        // 冷却遮罩
        var cooldownObj = new GameObject("CooldownMask");
        cooldownObj.transform.SetParent(slotObj.transform);
        var cooldownImage = cooldownObj.AddComponent<UnityEngine.UI.Image>();
        cooldownImage.color = new Color(0, 0, 0, 0.6f);
        cooldownImage.type = UnityEngine.UI.Image.Type.Filled;
        cooldownImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
        cooldownImage.fillAmount = 0; // 初始无冷却

        var cooldownRect = cooldownObj.GetComponent<RectTransform>();
        cooldownRect.anchorMin = Vector2.zero;
        cooldownRect.anchorMax = Vector2.one;
        cooldownRect.sizeDelta = Vector2.zero;
        cooldownRect.anchoredPosition = Vector2.zero;

        // 快捷键提示
        var keyObj = new GameObject("KeyHint");
        keyObj.transform.SetParent(slotObj.transform);
        var keyText = keyObj.AddComponent<UnityEngine.UI.Text>();
        keyText.text = (index + 1).ToString();
        keyText.fontSize = 14;
        keyText.alignment = TextAnchor.LowerRight;
        keyText.color = Color.white;
        keyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.anchorMin = Vector2.zero;
        keyRect.anchorMax = Vector2.one;
        keyRect.sizeDelta = new Vector2(-4, -4);
        keyRect.anchoredPosition = Vector2.zero;
    }

    private static void SetupBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/StageSelect.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };

        EditorBuildSettings.scenes = scenes;
        Debug.Log($"{TAG} Build settings updated with 3 scenes");
    }

    [MenuItem("MoShou/Quick Build Windows")]
    public static void QuickBuildWindows()
    {
        string buildPath = "Builds/Windows/MoShou.exe";
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            buildPath,
            BuildTarget.StandaloneWindows64,
            BuildOptions.None
        );

        Debug.Log($"[Build] Windows build complete: {buildPath}");
        EditorUtility.RevealInFinder(buildPath);
    }

    [MenuItem("MoShou/Quick Build Android")]
    public static void QuickBuildAndroid()
    {
        string buildPath = "Builds/Android/MoShou.apk";
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            buildPath,
            BuildTarget.Android,
            BuildOptions.None
        );

        Debug.Log($"[Build] Android build complete: {buildPath}");
        EditorUtility.RevealInFinder(buildPath);
    }
}

/// <summary>
/// 出生点可视化Gizmo
/// </summary>
public class SpawnPointGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.green;
    public float radius = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}
