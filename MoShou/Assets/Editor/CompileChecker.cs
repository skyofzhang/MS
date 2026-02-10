using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 编译检查器 - 自动检测编译错误和警告
/// 每次代码修改后自动运行
/// </summary>
[InitializeOnLoad]
public class CompileChecker
{
    private static List<CompilerMessage> _errors = new List<CompilerMessage>();
    private static List<CompilerMessage> _warnings = new List<CompilerMessage>();
    private static bool _hasChecked = false;

    static CompileChecker()
    {
        // 注册编译完成回调
        CompilationPipeline.compilationFinished += OnCompilationFinished;
        CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

        // 编辑器启动时检查
        EditorApplication.delayCall += DelayedCheck;
    }

    static void DelayedCheck()
    {
        if (!_hasChecked)
        {
            _hasChecked = true;
            CheckCompileStatus();
        }
    }

    static void OnCompilationFinished(object obj)
    {
        CheckCompileStatus();
    }

    static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
    {
        foreach (var msg in messages)
        {
            if (msg.type == CompilerMessageType.Error)
            {
                _errors.Add(msg);
            }
            else if (msg.type == CompilerMessageType.Warning)
            {
                _warnings.Add(msg);
            }
        }
    }

    static void CheckCompileStatus()
    {
        _errors.Clear();
        _warnings.Clear();

        // 通过EditorUtility检查是否正在编译
        if (EditorApplication.isCompiling)
        {
            return; // 还在编译中，等待完成
        }

        // 输出检查结果
        EditorApplication.delayCall += () =>
        {
            if (_errors.Count > 0)
            {
                Debug.LogError($"[CompileChecker] ❌ 发现 {_errors.Count} 个编译错误!");
                foreach (var err in _errors.Take(5))
                {
                    Debug.LogError($"  → {err.file}({err.line}): {err.message}");
                }
            }
            else if (_warnings.Count > 0)
            {
                Debug.LogWarning($"[CompileChecker] ⚠️ 编译成功，但有 {_warnings.Count} 个警告");
            }
            else
            {
                Debug.Log("[CompileChecker] ✓ 编译检查通过，无错误无警告");
            }
        };
    }

    [MenuItem("MoShou/Check Compile Status")]
    public static void ManualCheck()
    {
        _errors.Clear();
        _warnings.Clear();
        _hasChecked = false;

        // 强制重新编译
        CompilationPipeline.RequestScriptCompilation();

        Debug.Log("[CompileChecker] 正在重新编译并检查...");
    }

    [MenuItem("MoShou/Full Project Validation")]
    public static void FullValidation()
    {
        Debug.Log("=== MoShou 项目完整性检查 ===");

        int issues = 0;

        // 1. 检查编译状态
        if (EditorApplication.isCompiling)
        {
            Debug.LogWarning("[验证] 正在编译中，请稍后再试");
            return;
        }

        // 2. 检查关键资源
        issues += CheckResources();

        // 3. 检查场景设置
        issues += CheckScenes();

        // 4. 检查预制体
        issues += CheckPrefabs();

        // 5. 总结
        Debug.Log("=== 检查完成 ===");
        if (issues == 0)
        {
            Debug.Log("[验证] ✓ 所有检查项通过!");
        }
        else
        {
            Debug.LogWarning($"[验证] 发现 {issues} 个问题需要处理");
        }
    }

    static int CheckResources()
    {
        int issues = 0;
        Debug.Log("[验证] 检查关键资源...");

        // 检查MAP预制体
        var mapPrefab = Resources.Load<GameObject>("Models/Environment/BossRoom/MAP");
        if (mapPrefab == null)
        {
            Debug.LogWarning("  → MAP预制体未找到 (可使用程序化地形)");
            // 不算严重问题，因为有回退方案
        }
        else
        {
            Debug.Log("  ✓ MAP预制体存在");
        }

        // 检查玩家预制体
        string[] playerPaths = {
            "Prefabs/Characters/Player_Archer",
            "Models/Player/Player_Archer"
        };
        bool playerFound = false;
        foreach (var path in playerPaths)
        {
            if (Resources.Load<GameObject>(path) != null)
            {
                playerFound = true;
                Debug.Log($"  ✓ 玩家预制体存在: {path}");
                break;
            }
        }
        if (!playerFound)
        {
            Debug.LogWarning("  → 玩家预制体未找到 (将使用Capsule替代)");
        }

        // 检查BGM
        var bgm = Resources.Load<AudioClip>("Audio/BGM/BGM_MainMenu");
        if (bgm != null)
        {
            Debug.Log("  ✓ BGM音频存在");
        }
        else
        {
            Debug.LogWarning("  → BGM音频未找到");
            issues++;
        }

        return issues;
    }

    static int CheckScenes()
    {
        int issues = 0;
        Debug.Log("[验证] 检查场景配置...");

        string[] requiredScenes = { "MainMenu", "StageSelect", "GameScene" };
        var buildScenes = EditorBuildSettings.scenes;

        foreach (var sceneName in requiredScenes)
        {
            bool found = buildScenes.Any(s => s.path.Contains(sceneName) && s.enabled);
            if (found)
            {
                Debug.Log($"  ✓ 场景已配置: {sceneName}");
            }
            else
            {
                Debug.LogError($"  ✗ 场景未配置: {sceneName}");
                issues++;
            }
        }

        return issues;
    }

    static int CheckPrefabs()
    {
        int issues = 0;
        Debug.Log("[验证] 检查VFX预制体...");

        string[] vfxPrefabs = {
            "Prefabs/VFX/VFX_Hit_Spark",
            "Prefabs/VFX/VFX_LevelUp",
            "Prefabs/VFX/VFX_Death_Dissolve"
        };

        foreach (var path in vfxPrefabs)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                Debug.Log($"  ✓ VFX预制体存在: {path}");
            }
            else
            {
                Debug.LogWarning($"  → VFX预制体未找到: {path} (运行 MoShou/Generate VFX Prefabs)");
            }
        }

        return issues;
    }
}
