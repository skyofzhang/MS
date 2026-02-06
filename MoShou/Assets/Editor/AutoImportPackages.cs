using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 自动导入场景包工具
/// 放入Editor目录后，在菜单 Tools > Import Scene Packages 执行
/// </summary>
public class AutoImportPackages : Editor
{
    [MenuItem("Tools/Import Scene Packages")]
    public static void ImportScenePackages()
    {
        string packageDir = Path.Combine(Application.dataPath, "ScenePackages");

        if (!Directory.Exists(packageDir))
        {
            Debug.LogError($"场景包目录不存在: {packageDir}");
            return;
        }

        string[] packages = Directory.GetFiles(packageDir, "*.unitypackage");

        if (packages.Length == 0)
        {
            Debug.LogWarning("未找到任何 .unitypackage 文件");
            return;
        }

        Debug.Log($"找到 {packages.Length} 个场景包，开始导入...");

        foreach (string packagePath in packages)
        {
            string packageName = Path.GetFileName(packagePath);
            Debug.Log($"正在导入: {packageName}");

            // 静默导入，不显示导入对话框
            AssetDatabase.ImportPackage(packagePath, false);
        }

        AssetDatabase.Refresh();
        Debug.Log("✅ 所有场景包导入完成！");
    }

    [MenuItem("Tools/Verify Resources")]
    public static void VerifyResources()
    {
        Debug.Log("=== 资源验证开始 ===");

        // 检查关键资源
        string[] criticalPaths = new string[]
        {
            "Models/Player/Player_Archer",
            "Models/Monsters/Slime",
            "Models/Monsters/GoblinKing",
            "Models/Weapons/Weapon_Bow_Basic",
            "Sprites/UI/HUD",
            "Audio/BGM",
            "Audio/SFX"
        };

        int found = 0;
        int missing = 0;

        foreach (string path in criticalPaths)
        {
            Object obj = Resources.Load(path);
            if (obj != null)
            {
                Debug.Log($"✓ {path}");
                found++;
            }
            else
            {
                // 检查目录是否存在
                string fullPath = Path.Combine(Application.dataPath, "Resources", path);
                if (Directory.Exists(fullPath) || File.Exists(fullPath + ".fbx") || File.Exists(fullPath + ".prefab"))
                {
                    Debug.Log($"✓ {path} (目录/文件存在)");
                    found++;
                }
                else
                {
                    Debug.LogWarning($"✗ 缺失: {path}");
                    missing++;
                }
            }
        }

        Debug.Log($"=== 验证完成: {found} 找到, {missing} 缺失 ===");
    }
}
