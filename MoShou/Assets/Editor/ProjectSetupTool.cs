using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 项目环境一键配置工具
/// 解决：Layer定义、Prefab生成、Physics配置等所有前置问题
/// </summary>
public class ProjectSetupTool : EditorWindow
{
    [MenuItem("MoShou/一键配置项目环境", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<ProjectSetupTool>("项目环境配置");
    }

    [MenuItem("MoShou/快速修复/1. 配置Layer", false, 100)]
    public static void SetupLayers()
    {
        SetupPhysicsLayers();
        Debug.Log("[ProjectSetup] ✓ Layer配置完成");
    }

    [MenuItem("MoShou/快速修复/2. 生成所有Prefab", false, 101)]
    public static void GenerateAllPrefabs()
    {
        GeneratePrefabsFromFBX();
        Debug.Log("[ProjectSetup] ✓ Prefab生成完成");
    }

    [MenuItem("MoShou/快速修复/3. 创建UI预制体", false, 102)]
    public static void CreateUIPrefabs()
    {
        CreateStageButtonPrefab();
        Debug.Log("[ProjectSetup] ✓ UI预制体创建完成");
    }

    [MenuItem("MoShou/快速修复/4. 一键全部修复", false, 200)]
    public static void FixAll()
    {
        SetupPhysicsLayers();
        GeneratePrefabsFromFBX();
        CreateStageButtonPrefab();
        ConfigurePhysicsMatrix();
        AssetDatabase.Refresh();
        Debug.Log("[ProjectSetup] ✓✓✓ 所有配置完成！请重新运行游戏。");
    }

    private void OnGUI()
    {
        GUILayout.Label("MoShou 项目环境配置", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "此工具将自动配置：\n" +
            "• Physics Layer (Player=8, Enemy=9, Projectile=10, Environment=11)\n" +
            "• 将FBX模型转换为Prefab\n" +
            "• 创建必要的UI预制体\n" +
            "• 配置碰撞矩阵",
            MessageType.Info);

        GUILayout.Space(20);

        if (GUILayout.Button("一键配置所有", GUILayout.Height(40)))
        {
            FixAll();
        }

        GUILayout.Space(10);
        GUILayout.Label("分步配置：", EditorStyles.boldLabel);

        if (GUILayout.Button("1. 配置Physics Layer"))
            SetupPhysicsLayers();

        if (GUILayout.Button("2. FBX转Prefab"))
            GeneratePrefabsFromFBX();

        if (GUILayout.Button("3. 创建UI预制体"))
            CreateStageButtonPrefab();

        if (GUILayout.Button("4. 配置碰撞矩阵"))
            ConfigurePhysicsMatrix();
    }

    /// <summary>
    /// 配置Physics Layer - 知识库§5
    /// </summary>
    private static void SetupPhysicsLayers()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        // Layer 8 = Player
        SetLayer(layers, 8, "Player");
        // Layer 9 = Enemy
        SetLayer(layers, 9, "Enemy");
        // Layer 10 = Projectile
        SetLayer(layers, 10, "Projectile");
        // Layer 11 = Environment
        SetLayer(layers, 11, "Environment");

        tagManager.ApplyModifiedProperties();
        Debug.Log("[ProjectSetup] Layer配置: 8=Player, 9=Enemy, 10=Projectile, 11=Environment");
    }

    private static void SetLayer(SerializedProperty layers, int index, string name)
    {
        SerializedProperty layer = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(layer.stringValue))
        {
            layer.stringValue = name;
            Debug.Log($"[ProjectSetup] 设置Layer {index} = {name}");
        }
    }

    /// <summary>
    /// 将FBX转换为Prefab
    /// </summary>
    private static void GeneratePrefabsFromFBX()
    {
        string[] fbxPaths = new string[]
        {
            "Assets/Resources/Models/Player/Player_Archer.fbx",
            "Assets/Resources/Models/Monsters/Slime/Monster_Slime.fbx",
            "Assets/Resources/Models/Monsters/Goblin/Monster_Goblin.fbx",
            "Assets/Resources/Models/Monsters/Wolf/Monster_Wolf.fbx",
            "Assets/Resources/Models/Monsters/GoblinElite/Monster_GoblinElite.fbx",
            "Assets/Resources/Models/Monsters/GoblinKing/Boss_GoblinKing.fbx"
        };

        string prefabOutputDir = "Assets/Resources/Prefabs/Characters";
        if (!Directory.Exists(prefabOutputDir))
        {
            Directory.CreateDirectory(prefabOutputDir);
        }

        foreach (string fbxPath in fbxPaths)
        {
            if (!File.Exists(fbxPath))
            {
                Debug.LogWarning($"[ProjectSetup] FBX不存在: {fbxPath}");
                continue;
            }

            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogWarning($"[ProjectSetup] 无法加载FBX: {fbxPath}");
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(fbxPath);
            string prefabPath = $"{prefabOutputDir}/{fileName}.prefab";

            // 实例化FBX
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);

            // 保存为Prefab
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            DestroyImmediate(instance);

            Debug.Log($"[ProjectSetup] 创建Prefab: {prefabPath}");
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 创建StageButton预制体
    /// </summary>
    private static void CreateStageButtonPrefab()
    {
        string prefabDir = "Assets/Resources/Prefabs/UI";
        if (!Directory.Exists(prefabDir))
        {
            Directory.CreateDirectory(prefabDir);
        }

        string prefabPath = $"{prefabDir}/StageButton.prefab";

        // 创建按钮GameObject
        GameObject buttonGO = new GameObject("StageButton");

        // 添加RectTransform
        RectTransform rt = buttonGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 60);

        // 添加Image作为背景
        UnityEngine.UI.Image bg = buttonGO.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.2f, 0.4f, 0.6f, 1f);

        // 添加Button组件
        UnityEngine.UI.Button btn = buttonGO.AddComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = bg;

        // 创建文本子对象
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
        text.text = "Stage";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 24;
        text.color = Color.white;

        // 添加StageButtonUI组件
        buttonGO.AddComponent<MoShou.UI.StageButtonUI>();

        // 保存为Prefab
        PrefabUtility.SaveAsPrefabAsset(buttonGO, prefabPath);
        DestroyImmediate(buttonGO);

        Debug.Log($"[ProjectSetup] 创建UI预制体: {prefabPath}");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 配置碰撞矩阵 - 知识库§5
    /// </summary>
    private static void ConfigurePhysicsMatrix()
    {
        // Player(8) vs Enemy(9) = true
        Physics.IgnoreLayerCollision(8, 9, false);

        // Player(8) vs Projectile(10) = false (自己的子弹不打自己)
        Physics.IgnoreLayerCollision(8, 10, true);

        // Enemy(9) vs Enemy(9) = false (怪物间不碰撞)
        Physics.IgnoreLayerCollision(9, 9, true);

        // Projectile(10) vs Enemy(9) = true
        Physics.IgnoreLayerCollision(10, 9, false);

        // Projectile(10) vs Environment(11) = true
        Physics.IgnoreLayerCollision(10, 11, false);

        Debug.Log("[ProjectSetup] 碰撞矩阵配置完成");
    }
}
