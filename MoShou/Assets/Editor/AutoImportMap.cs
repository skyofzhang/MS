using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 自动导入MAP预制体 - Unity启动时自动执行
/// </summary>
[InitializeOnLoad]
public class AutoImportMap
{
    private static readonly string SOURCE_PACKAGE = "Assets/ScenePackages/Scene_BossRoom.unitypackage";
    private static readonly string TARGET_DIR = "Assets/Resources/Models/Environment/BossRoom";
    private static readonly string MARKER_FILE = "Assets/Resources/Models/Environment/BossRoom/.imported";

    static AutoImportMap()
    {
        // 延迟执行，等待Editor完全加载
        EditorApplication.delayCall += TryAutoImport;
    }

    static void TryAutoImport()
    {
        // 检查是否已导入过
        if (File.Exists(MARKER_FILE))
        {
            Debug.Log("[AutoImportMap] MAP已导入，跳过");
            return;
        }

        // 检查MAP预制体是否已存在
        if (Resources.Load<GameObject>("Models/Environment/BossRoom/MAP") != null)
        {
            // 创建标记文件
            CreateMarkerFile();
            Debug.Log("[AutoImportMap] MAP预制体已存在");
            return;
        }

        // 检查源文件是否存在
        if (!File.Exists(SOURCE_PACKAGE))
        {
            Debug.LogWarning("[AutoImportMap] 未找到源文件: " + SOURCE_PACKAGE);
            return;
        }

        Debug.Log("[AutoImportMap] 开始自动导入BOSS房场景...");

        // 确保目标目录存在
        if (!Directory.Exists(TARGET_DIR))
        {
            Directory.CreateDirectory(TARGET_DIR);
        }

        // 静默导入unitypackage
        AssetDatabase.ImportPackage(SOURCE_PACKAGE, false);

        // 延迟执行移动操作
        EditorApplication.delayCall += MoveMapPrefab;
    }

    static void MoveMapPrefab()
    {
        AssetDatabase.Refresh();

        // 搜索MAP预制体
        string[] guids = AssetDatabase.FindAssets("MAP t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 跳过已在目标目录的
            if (path.Contains("Resources/Models/Environment/BossRoom"))
                continue;

            // 只处理名为MAP的预制体
            if (!path.EndsWith("/MAP.prefab"))
                continue;

            string targetPath = TARGET_DIR + "/MAP.prefab";

            // 移动预制体
            if (File.Exists(targetPath))
            {
                AssetDatabase.DeleteAsset(targetPath);
            }

            string error = AssetDatabase.MoveAsset(path, targetPath);
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log("[AutoImportMap] ✓ MAP预制体已移动到: " + targetPath);

                // 移动相关资源
                MoveRelatedAssets(path);

                // 创建标记文件
                CreateMarkerFile();

                AssetDatabase.Refresh();
                return;
            }
            else
            {
                Debug.LogError("[AutoImportMap] 移动失败: " + error);
            }
        }

        // 如果没找到MAP，检查是否有其他类似名称的预制体
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();

            if ((fileName.Contains("map") || fileName.Contains("boss") || fileName.Contains("room")) &&
                !path.Contains("Resources"))
            {
                Debug.Log("[AutoImportMap] 找到可能的场景预制体: " + path);
            }
        }
    }

    static void MoveRelatedAssets(string originalPath)
    {
        string sourceDir = Path.GetDirectoryName(originalPath);
        if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
            return;

        // 创建子目录
        string matsDir = TARGET_DIR + "/Materials";
        string texDir = TARGET_DIR + "/Textures";

        // 移动材质
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { sourceDir });
        if (matGuids.Length > 0)
        {
            if (!Directory.Exists(matsDir)) Directory.CreateDirectory(matsDir);
            foreach (string guid in matGuids)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(guid);
                string targetPath = matsDir + "/" + Path.GetFileName(matPath);
                if (!File.Exists(targetPath))
                {
                    AssetDatabase.MoveAsset(matPath, targetPath);
                }
            }
            Debug.Log($"[AutoImportMap] 移动了 {matGuids.Length} 个材质");
        }

        // 移动贴图
        string[] texGuids = AssetDatabase.FindAssets("t:Texture", new[] { sourceDir });
        if (texGuids.Length > 0)
        {
            if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);
            foreach (string guid in texGuids)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(guid);
                string targetPath = texDir + "/" + Path.GetFileName(texPath);
                if (!File.Exists(targetPath))
                {
                    AssetDatabase.MoveAsset(texPath, targetPath);
                }
            }
            Debug.Log($"[AutoImportMap] 移动了 {texGuids.Length} 个贴图");
        }
    }

    static void CreateMarkerFile()
    {
        if (!Directory.Exists(TARGET_DIR))
        {
            Directory.CreateDirectory(TARGET_DIR);
        }
        File.WriteAllText(MARKER_FILE, "imported");
    }

    [MenuItem("MoShou/Force Re-import MAP")]
    public static void ForceReimport()
    {
        // 删除标记文件
        if (File.Exists(MARKER_FILE))
        {
            File.Delete(MARKER_FILE);
        }

        // 重新导入
        TryAutoImport();
    }
}
