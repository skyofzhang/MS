using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 材质球生成器 - 自动为3D模型创建对应的材质球
/// 解决问题：美术清单缺少材质球(.mat)文件
/// </summary>
public class MaterialGenerator : Editor
{
    [MenuItem("MoShou/资源工具/1. 生成所有材质球")]
    public static void GenerateAllMaterials()
    {
        int created = 0;

        // 1. 玩家材质
        created += CreateMaterial(
            "Assets/Resources/Models/Player/",
            "Player_Archer",
            "Player_Archer_Diffuse"
        );

        // 2. 怪物材质
        string[] monsters = { "Slime", "Goblin", "Wolf", "GoblinElite" };
        foreach (var monster in monsters)
        {
            created += CreateMaterial(
                $"Assets/Resources/Models/Monsters/{monster}/",
                $"Monster_{monster}",
                $"Monster_{monster}_Diffuse"
            );
        }

        // 3. BOSS材质
        created += CreateMaterial(
            "Assets/Resources/Models/Monsters/GoblinKing/",
            "Boss_GoblinKing",
            "Boss_GoblinKing_Diffuse"
        );

        // 4. 武器材质
        created += CreateMaterial(
            "Assets/Resources/Models/Weapons/",
            "Weapon_Bow_Basic",
            "Weapon_Bow_Diffuse"
        );
        created += CreateMaterial(
            "Assets/Resources/Models/Weapons/",
            "Weapon_Bow_Iron",
            "Weapon_Bow_Iron_Diffuse"
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[MaterialGenerator] 创建了 {created} 个材质球");
        EditorUtility.DisplayDialog("材质生成完成", $"成功创建 {created} 个材质球", "OK");
    }

    [MenuItem("MoShou/资源工具/2. 应用材质到Prefab")]
    public static void ApplyMaterialsToPrefabs()
    {
        int updated = 0;

        // 查找所有角色Prefab
        string prefabPath = "Assets/Resources/Prefabs/Characters/";
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabPath });

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            string prefabName = prefab.name;
            Material mat = FindMaterialForPrefab(prefabName);

            if (mat != null)
            {
                // 打开Prefab进行编辑
                string prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabAssetPath);

                // 应用材质到所有Renderer
                Renderer[] renderers = prefabRoot.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.sharedMaterial = mat;
                }

                // 保存Prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);

                updated++;
                Debug.Log($"[MaterialGenerator] 更新Prefab材质: {prefabName}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialGenerator] 更新了 {updated} 个Prefab的材质");
        EditorUtility.DisplayDialog("材质应用完成", $"成功更新 {updated} 个Prefab", "OK");
    }

    [MenuItem("MoShou/资源工具/3. 一键完成材质配置")]
    public static void OneClickMaterialSetup()
    {
        GenerateAllMaterials();
        ApplyMaterialsToPrefabs();
        Debug.Log("[MaterialGenerator] 一键材质配置完成!");
    }

    /// <summary>
    /// 创建单个材质球
    /// </summary>
    private static int CreateMaterial(string folderPath, string materialName, string textureName)
    {
        // 检查文件夹是否存在
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"[MaterialGenerator] 文件夹不存在: {folderPath}");
            return 0;
        }

        string matPath = folderPath + materialName + ".mat";

        // 检查材质是否已存在
        if (File.Exists(matPath))
        {
            Debug.Log($"[MaterialGenerator] 材质已存在: {matPath}");
            return 0;
        }

        // 查找贴图
        Texture2D texture = null;
        string[] searchPaths = new[]
        {
            folderPath + textureName + ".png",
            folderPath + textureName + ".jpg",
            folderPath + textureName + ".tga"
        };

        foreach (string texPath in searchPaths)
        {
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (texture != null) break;
        }

        // 创建材质
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = materialName;

        if (texture != null)
        {
            mat.mainTexture = texture;
            mat.SetFloat("_Glossiness", 0.2f); // 降低光泽度，更卡通
            mat.SetFloat("_Metallic", 0f); // 无金属感
            Debug.Log($"[MaterialGenerator] 创建材质: {materialName} (贴图: {texture.name})");
        }
        else
        {
            // 无贴图时使用默认颜色
            mat.color = GetDefaultColor(materialName);
            Debug.LogWarning($"[MaterialGenerator] 未找到贴图，使用默认颜色: {materialName}");
        }

        // 保存材质
        AssetDatabase.CreateAsset(mat, matPath);
        return 1;
    }

    /// <summary>
    /// 根据Prefab名称查找对应材质
    /// </summary>
    private static Material FindMaterialForPrefab(string prefabName)
    {
        // 映射Prefab名称到材质路径
        string matPath = "";

        if (prefabName == "Player_Archer")
            matPath = "Assets/Resources/Models/Player/Player_Archer.mat";
        else if (prefabName.StartsWith("Monster_Slime"))
            matPath = "Assets/Resources/Models/Monsters/Slime/Monster_Slime.mat";
        else if (prefabName.StartsWith("Monster_Goblin") && !prefabName.Contains("Elite"))
            matPath = "Assets/Resources/Models/Monsters/Goblin/Monster_Goblin.mat";
        else if (prefabName.StartsWith("Monster_Wolf"))
            matPath = "Assets/Resources/Models/Monsters/Wolf/Monster_Wolf.mat";
        else if (prefabName.Contains("GoblinElite"))
            matPath = "Assets/Resources/Models/Monsters/GoblinElite/Monster_GoblinElite.mat";
        else if (prefabName.Contains("GoblinKing"))
            matPath = "Assets/Resources/Models/Monsters/GoblinKing/Boss_GoblinKing.mat";

        if (!string.IsNullOrEmpty(matPath))
        {
            return AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }

        return null;
    }

    /// <summary>
    /// 获取默认颜色（无贴图时使用）
    /// </summary>
    private static Color GetDefaultColor(string name)
    {
        if (name.Contains("Slime")) return new Color(0.2f, 0.8f, 0.2f); // 绿色
        if (name.Contains("Goblin")) return new Color(0.4f, 0.6f, 0.3f); // 深绿
        if (name.Contains("Wolf")) return new Color(0.5f, 0.5f, 0.5f); // 灰色
        if (name.Contains("Player")) return new Color(0.8f, 0.6f, 0.4f); // 肤色
        if (name.Contains("Boss")) return new Color(0.6f, 0.3f, 0.3f); // 红褐色
        return Color.white;
    }
}
