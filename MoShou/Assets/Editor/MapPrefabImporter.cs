using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// MAP预制体导入器 - 用于导入BOSS房场景预制体
/// 将Scene_BossRoom.unitypackage中的MAP预制体导入到正确的Resources目录
/// </summary>
public class MapPrefabImporter : EditorWindow
{
    private static readonly string SOURCE_PACKAGE_PATH = "Assets/ScenePackages/Scene_BossRoom.unitypackage";
    private static readonly string TARGET_PREFAB_PATH = "Assets/Resources/Models/Environment/BossRoom";
    // PREFAB_NAME用于标识目标预制体名称，在搜索和验证中使用
    private const string PREFAB_NAME = "MAP";

    [MenuItem("MoShou/Import BOSS Room MAP")]
    public static void ShowWindow()
    {
        GetWindow<MapPrefabImporter>("MAP导入器");
    }

    void OnGUI()
    {
        GUILayout.Label("BOSS房场景预制体导入器", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "此工具将导入BOSS房场景预制体(MAP)到Resources目录，用于替代程序化生成的战斗地形。\n\n" +
            "源文件: " + SOURCE_PACKAGE_PATH + "\n" +
            "目标路径: " + TARGET_PREFAB_PATH + "/MAP.prefab",
            MessageType.Info);

        GUILayout.Space(10);

        // 检查源文件是否存在
        bool packageExists = File.Exists(SOURCE_PACKAGE_PATH);

        if (!packageExists)
        {
            EditorGUILayout.HelpBox(
                "未找到Scene_BossRoom.unitypackage文件！\n" +
                "请先从GitHub下载或确认文件存在于:\n" + SOURCE_PACKAGE_PATH,
                MessageType.Error);

            if (GUILayout.Button("从GitHub下载说明"))
            {
                ShowDownloadInstructions();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✓ 找到源文件: Scene_BossRoom.unitypackage (43MB)", MessageType.None);

            GUILayout.Space(10);

            if (GUILayout.Button("步骤1: 导入unitypackage", GUILayout.Height(40)))
            {
                ImportPackage();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("步骤2: 移动MAP到Resources目录", GUILayout.Height(40)))
            {
                MoveMapPrefabToResources();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("步骤3: 验证MAP预制体", GUILayout.Height(40)))
            {
                VerifyMapPrefab();
            }
        }

        GUILayout.Space(20);
        GUILayout.Label("快捷操作", EditorStyles.boldLabel);

        if (GUILayout.Button("一键完成所有步骤", GUILayout.Height(50)))
        {
            if (packageExists)
            {
                OneClickImport();
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "请先确保unitypackage文件存在", "确定");
            }
        }
    }

    static void ImportPackage()
    {
        if (!File.Exists(SOURCE_PACKAGE_PATH))
        {
            EditorUtility.DisplayDialog("错误", "未找到unitypackage文件: " + SOURCE_PACKAGE_PATH, "确定");
            return;
        }

        Debug.Log("[MapImporter] 开始导入unitypackage...");
        AssetDatabase.ImportPackage(SOURCE_PACKAGE_PATH, true);
    }

    static void MoveMapPrefabToResources()
    {
        // 确保目标目录存在
        if (!Directory.Exists(TARGET_PREFAB_PATH))
        {
            Directory.CreateDirectory(TARGET_PREFAB_PATH);
            AssetDatabase.Refresh();
            Debug.Log("[MapImporter] 创建目录: " + TARGET_PREFAB_PATH);
        }

        // 搜索可能的MAP预制体位置
        string[] possiblePaths = new string[]
        {
            "Assets/MAP.prefab",
            "Assets/Prefabs/MAP.prefab",
            "Assets/Models/MAP.prefab",
            "Assets/Scene/MAP.prefab",
            "Assets/场景/MAP.prefab",
            "Assets/场景_BOSS房/MAP.prefab"
        };

        // 也通过AssetDatabase搜索
        string[] guids = AssetDatabase.FindAssets("MAP t:Prefab");
        string foundPath = null;

        // 先检查预定义路径
        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                foundPath = path;
                break;
            }
        }

        // 如果没找到，使用AssetDatabase搜索结果
        if (foundPath == null && guids.Length > 0)
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("MAP.prefab") && !path.Contains("Resources"))
                {
                    foundPath = path;
                    break;
                }
            }
        }

        if (foundPath == null)
        {
            EditorUtility.DisplayDialog("提示",
                "未找到MAP预制体。\n\n" +
                "请先运行'步骤1: 导入unitypackage'，\n" +
                "或者手动将MAP.prefab移动到:\n" + TARGET_PREFAB_PATH,
                "确定");
            return;
        }

        // 移动预制体
        string targetPath = TARGET_PREFAB_PATH + "/MAP.prefab";

        // 如果目标已存在，先删除
        if (File.Exists(targetPath))
        {
            AssetDatabase.DeleteAsset(targetPath);
        }

        string error = AssetDatabase.MoveAsset(foundPath, targetPath);
        if (string.IsNullOrEmpty(error))
        {
            AssetDatabase.Refresh();
            Debug.Log("[MapImporter] MAP预制体已移动到: " + targetPath);
            EditorUtility.DisplayDialog("成功", "MAP预制体已移动到Resources目录:\n" + targetPath, "确定");

            // 尝试移动相关的材质和贴图
            MoveRelatedAssets(foundPath);
        }
        else
        {
            Debug.LogError("[MapImporter] 移动失败: " + error);
            EditorUtility.DisplayDialog("错误", "移动预制体失败: " + error, "确定");
        }
    }

    static void MoveRelatedAssets(string originalPrefabPath)
    {
        // 获取预制体所在目录
        string sourceDir = Path.GetDirectoryName(originalPrefabPath);

        if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
            return;

        // 查找并移动材质和贴图
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { sourceDir });
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { sourceDir });

        // 创建材质和贴图目录
        string materialsDir = TARGET_PREFAB_PATH + "/Materials";
        string texturesDir = TARGET_PREFAB_PATH + "/Textures";

        if (materialGuids.Length > 0)
        {
            if (!Directory.Exists(materialsDir))
            {
                Directory.CreateDirectory(materialsDir);
            }

            foreach (string guid in materialGuids)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(guid);
                string matName = Path.GetFileName(matPath);
                string targetMatPath = materialsDir + "/" + matName;

                if (!File.Exists(targetMatPath))
                {
                    AssetDatabase.MoveAsset(matPath, targetMatPath);
                }
            }
            Debug.Log($"[MapImporter] 移动了 {materialGuids.Length} 个材质文件");
        }

        if (textureGuids.Length > 0)
        {
            if (!Directory.Exists(texturesDir))
            {
                Directory.CreateDirectory(texturesDir);
            }

            foreach (string guid in textureGuids)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(guid);
                string texName = Path.GetFileName(texPath);
                string targetTexPath = texturesDir + "/" + texName;

                if (!File.Exists(targetTexPath))
                {
                    AssetDatabase.MoveAsset(texPath, targetTexPath);
                }
            }
            Debug.Log($"[MapImporter] 移动了 {textureGuids.Length} 个贴图文件");
        }

        AssetDatabase.Refresh();
    }

    static void VerifyMapPrefab()
    {
        string prefabPath = "Models/Environment/BossRoom/MAP";
        GameObject mapPrefab = Resources.Load<GameObject>(prefabPath);

        if (mapPrefab == null)
        {
            EditorUtility.DisplayDialog("验证失败",
                "无法通过Resources.Load加载MAP预制体！\n\n" +
                "预期路径: Resources/" + prefabPath + "\n\n" +
                "请确保预制体位于正确位置。",
                "确定");
            return;
        }

        // 分析预制体内容
        int meshCount = mapPrefab.GetComponentsInChildren<MeshRenderer>(true).Length;
        int colliderCount = mapPrefab.GetComponentsInChildren<Collider>(true).Length;
        int lightCount = mapPrefab.GetComponentsInChildren<Light>(true).Length;

        // 检查包围盒
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var renderer in mapPrefab.GetComponentsInChildren<Renderer>(true))
        {
            bounds.Encapsulate(renderer.bounds);
        }

        string info = $"MAP预制体验证成功！\n\n" +
                      $"预制体名称: {mapPrefab.name}\n" +
                      $"子物体数量: {mapPrefab.transform.childCount}\n" +
                      $"MeshRenderer: {meshCount}\n" +
                      $"Collider: {colliderCount}\n" +
                      $"Light: {lightCount}\n\n" +
                      $"包围盒大小:\n" +
                      $"  X: {bounds.size.x:F1} 单位\n" +
                      $"  Y: {bounds.size.y:F1} 单位\n" +
                      $"  Z: {bounds.size.z:F1} 单位\n\n" +
                      $"Resources.Load路径: \"{prefabPath}\"";

        EditorUtility.DisplayDialog("验证成功", info, "确定");
        Debug.Log("[MapImporter] " + info.Replace("\n", " | "));

        // 选中预制体
        Selection.activeObject = mapPrefab;
        EditorGUIUtility.PingObject(mapPrefab);
    }

    static void OneClickImport()
    {
        Debug.Log("[MapImporter] 开始一键导入...");

        // 步骤1: 导入package
        if (File.Exists(SOURCE_PACKAGE_PATH))
        {
            // 使用静默导入
            AssetDatabase.ImportPackage(SOURCE_PACKAGE_PATH, false);
            AssetDatabase.Refresh();

            // 延迟执行后续步骤
            EditorApplication.delayCall += () =>
            {
                MoveMapPrefabToResources();

                EditorApplication.delayCall += () =>
                {
                    VerifyMapPrefab();
                };
            };
        }
    }

    static void ShowDownloadInstructions()
    {
        string instructions =
            "从GitHub下载unitypackage的方法：\n\n" +
            "1. 访问以下URL:\n" +
            "   https://github.com/skyofzhang/ai-game-templates/blob/main/projects/moshou/场景/场景_BOSS房/map.unitypackage\n\n" +
            "2. 点击 'Download' 或 'Raw' 按钮下载\n\n" +
            "3. 将下载的文件重命名为 'Scene_BossRoom.unitypackage'\n\n" +
            "4. 将文件放入: Assets/ScenePackages/ 目录\n\n" +
            "5. 重新运行此工具";

        EditorUtility.DisplayDialog("下载说明", instructions, "确定");
    }

    [MenuItem("MoShou/Quick Verify MAP")]
    static void QuickVerify()
    {
        VerifyMapPrefab();
    }
}
