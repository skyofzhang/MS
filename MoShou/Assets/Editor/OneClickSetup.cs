using UnityEngine;
using UnityEditor;

/// <summary>
/// 一键设置工具
/// 自动执行所有必要的资源生成和配置步骤
/// </summary>
public class OneClickSetup : EditorWindow
{
    [MenuItem("MoShou/★ 一键设置 (全部执行) ★", priority = 0)]
    public static void RunAllSetup()
    {
        Debug.Log("========== 开始一键设置 ==========");

        // Step 1: 修复Sprite导入设置
        Debug.Log("[1/7] 修复Sprite导入设置...");
        SpriteImportFixer.FixAllSpriteImports();

        // Step 2: 生成VFX预制体
        Debug.Log("[2/7] 生成VFX预制体...");
        VFXPrefabGenerator.GenerateVFXPrefabs();

        // Step 3: 应用UI贴图
        Debug.Log("[3/7] 应用UI贴图...");
        UIArtApplier.ApplyUIArtAssets();

        // Step 4: 生成怪物Animator
        Debug.Log("[4/7] 生成怪物Animator Controller...");
        MonsterAnimatorGenerator.GenerateAllMonsterAnimators();

        // Step 5: 修复动画循环设置（Idle/Run循环，Attack/Skill不循环）
        Debug.Log("[5/7] 修复动画循环设置...");
        AnimationLoopFixer.FixAllAnimationLoops();

        // Step 6: 生成UI资源
        Debug.Log("[6/7] 生成UI资源...");
        UIResourceGenerator.GenerateAllUIResources();

        // Step 7: 检查FALLBACK对象
        Debug.Log("[7/7] 检查FALLBACK对象...");
        UIArtApplier.CheckFallbackObjects();

        Debug.Log("========== 一键设置完成 ==========");
        EditorUtility.DisplayDialog("一键设置完成",
            "已执行:\n" +
            "✓ Sprite导入修复\n" +
            "✓ VFX预制体生成\n" +
            "✓ UI贴图应用\n" +
            "✓ 怪物Animator生成\n" +
            "✓ 动画循环修复 (Run/Idle循环)\n" +
            "✓ UI资源生成\n" +
            "✓ FALLBACK检查\n\n" +
            "请进入PlayMode测试效果",
            "确定");
    }

    [MenuItem("MoShou/★ 一键设置 (全部执行) ★", true)]
    public static bool ValidateRunAllSetup()
    {
        // 不在PlayMode时才能运行
        return !Application.isPlaying;
    }
}
