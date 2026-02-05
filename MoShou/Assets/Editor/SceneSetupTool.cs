using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Unity Editor tool for quick scene setup
/// Run from menu: MoShou/Setup All Scenes
/// </summary>
public class SceneSetupTool : EditorWindow
{
    [MenuItem("MoShou/Setup All Scenes")]
    public static void SetupAllScenes()
    {
        if (!EditorUtility.DisplayDialog("Setup Scenes",
            "This will create and setup all game scenes:\n\n" +
            "- MainMenu\n" +
            "- StageSelect\n" +
            "- GameScene\n\n" +
            "Continue?", "Yes", "Cancel"))
        {
            return;
        }

        CreateScenesFolder();
        CreateMainMenuScene();
        CreateStageSelectScene();
        CreateGameScene();
        SetupBuildSettings();

        EditorUtility.DisplayDialog("Complete",
            "All scenes created successfully!\n\n" +
            "Next steps:\n" +
            "1. Open each scene and verify\n" +
            "2. File > Build Settings > Build",
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

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create UI Canvas
        var canvas = CreateCanvas("MainMenuCanvas");

        // Create Main Menu Manager
        var managerObj = new GameObject("MainMenuManager");
        managerObj.AddComponent<MoShou.Core.MainMenuManager>();

        // Create placeholder UI elements (user should customize)
        CreatePlaceholderButton(canvas.transform, "StartButton", "Start Game", new Vector2(0, 50));
        CreatePlaceholderButton(canvas.transform, "ContinueButton", "Continue", new Vector2(0, -20));
        CreatePlaceholderButton(canvas.transform, "SettingsButton", "Settings", new Vector2(0, -90));
        CreatePlaceholderButton(canvas.transform, "QuitButton", "Quit", new Vector2(0, -160));

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("[SceneSetup] MainMenu scene created");
    }

    private static void CreateStageSelectScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create UI Canvas
        var canvas = CreateCanvas("StageSelectCanvas");

        // Create Stage Select Manager
        var managerObj = new GameObject("StageSelectManager");
        managerObj.AddComponent<MoShou.Core.StageSelectManager>();

        // Create placeholder elements
        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvas.transform);
        var titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleText.text = "Select Stage";
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
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

        // Back button
        CreatePlaceholderButton(canvas.transform, "BackButton", "Back", new Vector2(-300, -350));

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/StageSelect.unity");
        Debug.Log("[SceneSetup] StageSelect scene created");
    }

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create GameInitializer (will create other managers)
        var initObj = new GameObject("GameInitializer");
        initObj.AddComponent<MoShou.Core.GameInitializer>();

        // Create ground plane
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5, 1, 5);
        ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.5f, 0.3f);

        // Create player spawn point
        var spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = Vector3.zero;

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
        }

        // Create UI Canvas for HUD
        var canvas = CreateCanvas("GameHUDCanvas");

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
        Debug.Log("[SceneSetup] GameScene scene created");
    }

    private static GameObject CreateCanvas(string name)
    {
        var canvasObj = new GameObject(name);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Event System
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    private static void CreatePlaceholderButton(Transform parent, string name, string text, Vector2 position)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);

        var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

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

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
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
        Debug.Log("[SceneSetup] Build settings updated with 3 scenes");
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
