using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 场景Canvas清理工具
/// 移除场景中预设的Canvas对象，让代码动态创建UI
/// </summary>
public class SceneCanvasCleaner : Editor
{
    [MenuItem("MoShou/Clean Scene Canvases (All Scenes)")]
    public static void CleanAllScenes()
    {
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/StageSelect.unity",
            "Assets/Scenes/GameScene.unity"
        };

        int totalRemoved = 0;

        foreach (string scenePath in scenePaths)
        {
            int removed = CleanSceneCanvases(scenePath);
            totalRemoved += removed;
        }

        EditorUtility.DisplayDialog("场景Canvas清理完成",
            $"已清理 {totalRemoved} 个预设Canvas对象。\n\n" +
            "现在场景会使用代码动态创建的UI。",
            "确定");
    }

    [MenuItem("MoShou/Clean Current Scene Canvases")]
    public static void CleanCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("[SceneCanvasCleaner] 没有打开的场景");
            return;
        }

        // 查找所有Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        int removed = 0;

        foreach (var canvas in canvases)
        {
            // 只移除sortingOrder < 500的Canvas（保留系统级UI）
            if (canvas.sortingOrder < 500)
            {
                string name = canvas.gameObject.name;
                DestroyImmediate(canvas.gameObject);
                Debug.Log($"[SceneCanvasCleaner] 已移除Canvas: {name}");
                removed++;
            }
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[SceneCanvasCleaner] 场景 {scene.name} 已移除 {removed} 个Canvas，请保存场景");
        }
        else
        {
            Debug.Log($"[SceneCanvasCleaner] 场景 {scene.name} 没有需要移除的Canvas");
        }
    }

    private static int CleanSceneCanvases(string scenePath)
    {
        // 打开场景
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogWarning($"[SceneCanvasCleaner] 无法打开场景: {scenePath}");
            return 0;
        }

        // 查找所有Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        int removed = 0;

        foreach (var canvas in canvases)
        {
            // 只移除sortingOrder < 500的Canvas
            if (canvas.sortingOrder < 500)
            {
                string name = canvas.gameObject.name;
                DestroyImmediate(canvas.gameObject);
                Debug.Log($"[SceneCanvasCleaner] {scene.name}: 已移除Canvas '{name}'");
                removed++;
            }
        }

        if (removed > 0)
        {
            // 保存场景
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[SceneCanvasCleaner] 场景 {scene.name} 已保存，移除了 {removed} 个Canvas");
        }

        return removed;
    }
}
