using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Everclan模型资源导入器 - 从共享目录导入everclan模型资源
/// 自动创建完整预制体(FBX + 材质 + 贴图 + 动画)
/// </summary>
public class EverclanModelImporter : Editor
{
    // 共享目录路径
    private const string SOURCE_ROOT = @"\\192.168.1.198\共享\AI目录\art-assets-ai\assets\models\everclan";
    private const string TYPE_MAPPING_PATH = @"\\192.168.1.198\共享\AI目录\art-assets-ai\meta\manifests\everclan_model_type_mapping.json";

    // Unity项目目标路径
    private const string TARGET_MODELS = "Assets/Resources/Models/Everclan";
    private const string TARGET_PREFABS = "Assets/Resources/Prefabs/Characters";

    [MenuItem("MoShou/资源导入/1. 导入Everclan模型 (全部)")]
    public static void ImportAllModels()
    {
        ImportModels(null, int.MaxValue);
    }

    [MenuItem("MoShou/资源导入/2. 导入前10个模型 (测试)")]
    public static void ImportFirst10Models()
    {
        ImportModels(null, 10);
    }

    [MenuItem("MoShou/资源导入/3. 只导入人类模型")]
    public static void ImportHumanModels()
    {
        ImportModels("human", int.MaxValue);
    }

    [MenuItem("MoShou/资源导入/4. 只导入怪物模型")]
    public static void ImportMonsterModels()
    {
        ImportModels("monster", int.MaxValue);
    }

    /// <summary>
    /// 导入模型
    /// </summary>
    private static void ImportModels(string typeFilter, int maxCount)
    {
        // 检查源目录
        if (!Directory.Exists(SOURCE_ROOT))
        {
            EditorUtility.DisplayDialog("错误", $"源目录不存在: {SOURCE_ROOT}\n请检查网络连接", "OK");
            return;
        }

        // 加载类型映射
        Dictionary<string, string> typeMapping = LoadTypeMapping();

        // 创建目标目录
        EnsureDirectory(TARGET_MODELS);
        EnsureDirectory(TARGET_PREFABS);

        // 获取所有模型目录
        string[] modelDirs = Directory.GetDirectories(SOURCE_ROOT);
        int imported = 0;
        int skipped = 0;

        foreach (string modelDir in modelDirs)
        {
            if (imported >= maxCount) break;

            string modelId = Path.GetFileName(modelDir);
            string modelType = typeMapping.ContainsKey(modelId) ? typeMapping[modelId] : "monster";

            // 类型过滤
            if (typeFilter != null && modelType != typeFilter)
            {
                skipped++;
                continue;
            }

            EditorUtility.DisplayProgressBar("导入模型", $"正在导入: {modelId}", (float)imported / Mathf.Min(modelDirs.Length, maxCount));

            try
            {
                ImportSingleModel(modelDir, modelId, modelType);
                imported++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EverclanImporter] 导入失败: {modelId} - {e.Message}");
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = $"导入完成!\n- 导入: {imported} 个模型\n- 跳过: {skipped} 个模型";
        Debug.Log($"[EverclanImporter] {message}");
        EditorUtility.DisplayDialog("导入完成", message, "OK");
    }

    /// <summary>
    /// 导入单个模型
    /// </summary>
    private static void ImportSingleModel(string sourceDir, string modelId, string modelType)
    {
        // 创建模型目录
        string targetDir = $"{TARGET_MODELS}/{modelType}/{modelId}";
        EnsureDirectory(targetDir);

        // 查找FBX文件
        string[] fbxFiles = Directory.GetFiles(sourceDir, "*.fbx");
        if (fbxFiles.Length == 0)
        {
            Debug.LogWarning($"[EverclanImporter] 未找到FBX: {modelId}");
            return;
        }

        string sourceFbx = fbxFiles[0];
        string fbxName = Path.GetFileName(sourceFbx);
        string targetFbx = $"{targetDir}/{fbxName}";

        // 复制FBX
        if (!File.Exists(targetFbx))
        {
            File.Copy(sourceFbx, targetFbx, true);
        }

        // 复制贴图
        string[] textureExtensions = { "*.png", "*.jpg", "*.tga", "*.tif", "*.bmp" };
        foreach (string ext in textureExtensions)
        {
            foreach (string texFile in Directory.GetFiles(sourceDir, ext))
            {
                string texName = Path.GetFileName(texFile);
                string targetTex = $"{targetDir}/{texName}";
                if (!File.Exists(targetTex))
                {
                    File.Copy(texFile, targetTex, true);
                }
            }
        }

        // 刷新资源数据库
        AssetDatabase.Refresh();

        // 配置FBX导入设置
        ConfigureFbxImport(targetFbx, modelType);

        // 创建材质
        CreateMaterial(targetDir, modelId);

        // 创建预制体
        CreatePrefab(targetFbx, modelId, modelType);

        Debug.Log($"[EverclanImporter] 导入成功: {modelId} ({modelType})");
    }

    /// <summary>
    /// 配置FBX导入设置
    /// </summary>
    private static void ConfigureFbxImport(string fbxPath, string modelType)
    {
        UnityEditor.ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as UnityEditor.ModelImporter;
        if (importer == null) return;

        // 基础设置
        importer.globalScale = 1f;
        importer.useFileScale = false;
        importer.importBlendShapes = false;
        importer.importCameras = false;
        importer.importLights = false;

        // 材质设置 - 不导入材质，使用我们自己创建的
        importer.materialImportMode = ModelImporterMaterialImportMode.None;

        // 动画设置
        importer.animationType = ModelImporterAnimationType.Generic;

        // 保存设置
        importer.SaveAndReimport();
    }

    /// <summary>
    /// 创建材质
    /// </summary>
    private static void CreateMaterial(string modelDir, string modelId)
    {
        string matPath = $"{modelDir}/{modelId}.mat";
        if (File.Exists(matPath)) return;

        // 查找贴图
        Texture2D mainTex = null;
        string[] texFiles = Directory.GetFiles(modelDir, "*.png")
            .Concat(Directory.GetFiles(modelDir, "*.jpg"))
            .Concat(Directory.GetFiles(modelDir, "*.tga"))
            .ToArray();

        foreach (string texFile in texFiles)
        {
            string assetPath = texFile.Replace("\\", "/");
            mainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (mainTex != null) break;
        }

        // 创建材质
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = modelId;

        if (mainTex != null)
        {
            mat.mainTexture = mainTex;
            mat.SetFloat("_Glossiness", 0.2f);
            mat.SetFloat("_Metallic", 0f);
        }

        AssetDatabase.CreateAsset(mat, matPath);
    }

    /// <summary>
    /// 创建预制体
    /// </summary>
    private static void CreatePrefab(string fbxPath, string modelId, string modelType)
    {
        string prefabDir = $"{TARGET_PREFABS}/{modelType}";
        EnsureDirectory(prefabDir);

        string prefabPath = $"{prefabDir}/{modelId}.prefab";
        if (File.Exists(prefabPath)) return;

        // 加载FBX
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxAsset == null)
        {
            Debug.LogWarning($"[EverclanImporter] 无法加载FBX: {fbxPath}");
            return;
        }

        // 实例化
        GameObject instance = Instantiate(fbxAsset);
        instance.name = modelId;

        // 查找并应用材质
        string modelDir = Path.GetDirectoryName(fbxPath).Replace("\\", "/");
        string matPath = $"{modelDir}/{modelId}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (mat != null)
        {
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterial = mat;
            }
        }

        // 添加Animator组件
        if (instance.GetComponent<Animator>() == null)
        {
            instance.AddComponent<Animator>();
        }

        // 设置Layer
        int layer = modelType == "human" ? 8 : 9; // 8=Player, 9=Enemy
        instance.layer = layer;
        foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = layer;
        }

        // 添加Collider
        if (instance.GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = instance.AddComponent<CapsuleCollider>();
            collider.height = 1.8f;
            collider.radius = 0.3f;
            collider.center = new Vector3(0, 0.9f, 0);
        }

        // 保存预制体
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);
    }

    /// <summary>
    /// 加载类型映射
    /// </summary>
    private static Dictionary<string, string> LoadTypeMapping()
    {
        Dictionary<string, string> mapping = new Dictionary<string, string>();

        if (!File.Exists(TYPE_MAPPING_PATH))
        {
            Debug.LogWarning("[EverclanImporter] 类型映射文件不存在，使用默认类型");
            return mapping;
        }

        try
        {
            string json = File.ReadAllText(TYPE_MAPPING_PATH);
            // 简单解析JSON (避免引入第三方库)
            // 格式: {"items": [{"model_id": "xxx", "type": "human/monster"}, ...]}

            int itemsStart = json.IndexOf("\"items\"");
            if (itemsStart < 0) return mapping;

            // 查找所有 model_id 和 type 对
            int pos = 0;
            while (true)
            {
                int modelIdPos = json.IndexOf("\"model_id\"", pos);
                if (modelIdPos < 0) break;

                int colonPos = json.IndexOf(":", modelIdPos);
                int quoteStart = json.IndexOf("\"", colonPos + 1);
                int quoteEnd = json.IndexOf("\"", quoteStart + 1);
                string modelIdValue = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);

                int typePos = json.IndexOf("\"type\"", quoteEnd);
                if (typePos < 0 || typePos > json.IndexOf("}", quoteEnd))
                {
                    pos = quoteEnd + 1;
                    continue;
                }

                int typeColonPos = json.IndexOf(":", typePos);
                int typeQuoteStart = json.IndexOf("\"", typeColonPos + 1);
                int typeQuoteEnd = json.IndexOf("\"", typeQuoteStart + 1);
                string type = json.Substring(typeQuoteStart + 1, typeQuoteEnd - typeQuoteStart - 1);

                mapping[modelIdValue] = type;
                pos = typeQuoteEnd + 1;
            }

            Debug.Log($"[EverclanImporter] 加载类型映射: {mapping.Count} 个模型");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EverclanImporter] 解析类型映射失败: {e.Message}");
        }

        return mapping;
    }

    /// <summary>
    /// 确保目录存在
    /// </summary>
    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    [MenuItem("MoShou/资源导入/5. 检查导入状态")]
    public static void CheckImportStatus()
    {
        int humanCount = 0;
        int monsterCount = 0;
        int prefabCount = 0;

        string humanDir = $"{TARGET_MODELS}/human";
        string monsterDir = $"{TARGET_MODELS}/monster";
        string prefabDir = TARGET_PREFABS;

        if (Directory.Exists(humanDir))
            humanCount = Directory.GetDirectories(humanDir).Length;
        if (Directory.Exists(monsterDir))
            monsterCount = Directory.GetDirectories(monsterDir).Length;
        if (Directory.Exists(prefabDir))
            prefabCount = Directory.GetFiles(prefabDir, "*.prefab", SearchOption.AllDirectories).Length;

        string message = $"导入状态:\n" +
                        $"- 人类模型: {humanCount}\n" +
                        $"- 怪物模型: {monsterCount}\n" +
                        $"- 预制体: {prefabCount}\n\n" +
                        $"共享目录: {(Directory.Exists(SOURCE_ROOT) ? "可访问" : "不可访问")}";

        EditorUtility.DisplayDialog("导入状态", message, "OK");
    }
}
