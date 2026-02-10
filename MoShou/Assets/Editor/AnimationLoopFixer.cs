using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 动画循环修复工具
/// 自动设置FBX中动画片段的Loop Time属性
/// Idle和Run动画需要循环，Attack/Skill/Hit/Death不循环
/// </summary>
public class AnimationLoopFixer : EditorWindow
{
    // 需要循环的动画名称关键字
    private static readonly string[] LoopAnimationKeywords = new string[]
    {
        "Idle", "idle", "Stand", "stand", "Leisure",
        "Run", "run", "Walk", "walk", "Move", "move"
    };

    // 不需要循环的动画名称关键字
    private static readonly string[] NoLoopAnimationKeywords = new string[]
    {
        "Attack", "attack", "Skill", "skill", "Hit", "hit",
        "Death", "death", "Die", "die", "Victory", "victory"
    };

    [MenuItem("MoShou/动画系统/修复动画循环设置")]
    public static void FixAllAnimationLoops()
    {
        // 查找所有FBX文件
        string[] fbxPaths = new string[]
        {
            "Assets/Resources/Models/Player",
            "Assets/Resources/Models/Monsters/Slime",
            "Assets/Resources/Models/Monsters/Goblin",
            "Assets/Resources/Models/Monsters/GoblinElite",
            "Assets/Resources/Models/Monsters/GoblinKing",
            "Assets/Resources/Models/Monsters/Wolf"
        };

        int fixedCount = 0;

        foreach (string basePath in fbxPaths)
        {
            if (!Directory.Exists(basePath)) continue;

            string[] files = Directory.GetFiles(basePath, "*.fbx", SearchOption.TopDirectoryOnly);

            // 如果没找到.fbx，尝试其他模型格式
            if (files.Length == 0)
            {
                files = Directory.GetFiles(basePath, "*", SearchOption.TopDirectoryOnly);
            }

            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;

                string assetPath = file.Replace("\\", "/");
                if (FixAnimationLoopsInModel(assetPath))
                {
                    fixedCount++;
                }
            }
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("动画循环修复完成",
            $"已处理 {fixedCount} 个模型文件\n\n" +
            "循环动画: Idle, Run, Walk, Stand\n" +
            "非循环动画: Attack, Skill, Hit, Death, Victory",
            "确定");
    }

    [MenuItem("MoShou/动画系统/修复Player动画循环")]
    public static void FixPlayerAnimationLoops()
    {
        string playerFBX = "Assets/Resources/Models/Player/Player_Archer.fbx";

        if (!File.Exists(playerFBX))
        {
            // 尝试查找其他可能的路径
            string[] searchPaths = Directory.GetFiles("Assets/Resources/Models/Player", "*", SearchOption.TopDirectoryOnly);
            foreach (var path in searchPaths)
            {
                if (!path.EndsWith(".meta"))
                {
                    playerFBX = path.Replace("\\", "/");
                    break;
                }
            }
        }

        if (FixAnimationLoopsInModel(playerFBX))
        {
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Player动画修复完成",
                $"已修复: {playerFBX}\n\n" +
                "Run和Idle动画已设置为循环\n" +
                "Attack, Skill等动画已设置为不循环",
                "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("修复失败",
                $"未能修复动画: {playerFBX}\n请检查文件是否存在",
                "确定");
        }
    }

    private static bool FixAnimationLoopsInModel(string assetPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[AnimationLoopFixer] 无法获取导入器: {assetPath}");
            return false;
        }

        // 获取当前的动画片段设置
        ModelImporterClipAnimation[] clips = importer.clipAnimations;

        // 如果没有自定义剪辑，使用默认剪辑
        if (clips == null || clips.Length == 0)
        {
            clips = importer.defaultClipAnimations;
        }

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"[AnimationLoopFixer] 没有动画片段: {assetPath}");
            return false;
        }

        bool modified = false;
        List<ModelImporterClipAnimation> newClips = new List<ModelImporterClipAnimation>();

        foreach (var clip in clips)
        {
            ModelImporterClipAnimation newClip = clip;
            bool shouldLoop = ShouldAnimationLoop(clip.name);

            if (newClip.loopTime != shouldLoop)
            {
                newClip.loopTime = shouldLoop;
                newClip.loopPose = shouldLoop; // 循环姿态
                modified = true;
                Debug.Log($"[AnimationLoopFixer] {Path.GetFileName(assetPath)}/{clip.name}: Loop = {shouldLoop}");
            }

            newClips.Add(newClip);
        }

        if (modified)
        {
            importer.clipAnimations = newClips.ToArray();
            importer.SaveAndReimport();
            Debug.Log($"[AnimationLoopFixer] 已更新: {assetPath}");
        }

        return true;
    }

    private static bool ShouldAnimationLoop(string animationName)
    {
        // 检查是否应该循环
        foreach (string keyword in LoopAnimationKeywords)
        {
            if (animationName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        // 检查是否不应该循环
        foreach (string keyword in NoLoopAnimationKeywords)
        {
            if (animationName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }
        }

        // 默认不循环
        return false;
    }

    [MenuItem("MoShou/动画系统/检查动画循环状态")]
    public static void CheckAnimationLoopStatus()
    {
        string playerFBX = "Assets/Resources/Models/Player/Player_Archer.fbx";

        ModelImporter importer = AssetImporter.GetAtPath(playerFBX) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"[AnimationLoopFixer] 找不到: {playerFBX}");
            return;
        }

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            clips = importer.defaultClipAnimations;
        }

        string report = "=== Player动画循环状态 ===\n";
        foreach (var clip in clips)
        {
            string loopStatus = clip.loopTime ? "✓ 循环" : "✗ 不循环";
            report += $"{clip.name}: {loopStatus}\n";
        }

        Debug.Log(report);

        EditorUtility.DisplayDialog("动画循环状态", report, "确定");
    }
}
