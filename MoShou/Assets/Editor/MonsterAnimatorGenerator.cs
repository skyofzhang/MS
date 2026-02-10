using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 怪物Animator Controller生成器
/// 为每种怪物模型自动创建Animator Controller
/// 支持不同命名规范的动画片段
/// </summary>
public class MonsterAnimatorGenerator : EditorWindow
{
    // 动画状态名称映射（标准化名称 -> 可能的FBX片段名称）
    private static readonly Dictionary<string, string[]> AnimationNameMappings = new Dictionary<string, string[]>
    {
        { "Idle", new[] { "Idle", "idle", "Stand", "stand", "Leisure_1" } },
        { "Run", new[] { "Run", "run", "Walk", "walk" } },
        { "Attack", new[] { "Attack_1", "Attack_01", "Attack01", "attack_1", "Attack" } },
        { "Skill", new[] { "Skill_1", "Skill_01", "Skill01", "skill_1", "Skill" } },
        { "Hit", new[] { "Hit", "hit", "Damage", "damage", "Hurt" } },
        { "Death", new[] { "Death", "death", "Die", "die" } }
    };

    [MenuItem("MoShou/动画系统/生成所有怪物Animator")]
    public static void GenerateAllMonsterAnimators()
    {
        string[] monsterPaths = new string[]
        {
            "Assets/Resources/Models/Monsters/Slime",
            "Assets/Resources/Models/Monsters/Goblin",
            "Assets/Resources/Models/Monsters/GoblinElite",
            "Assets/Resources/Models/Monsters/GoblinKing",
            "Assets/Resources/Models/Monsters/Wolf"
        };

        int created = 0;
        foreach (string path in monsterPaths)
        {
            if (Directory.Exists(path))
            {
                string monsterName = Path.GetFileName(path);
                if (GenerateAnimatorForMonster(path, monsterName))
                {
                    created++;
                }
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("生成完成", $"已为 {created} 种怪物创建Animator Controller", "确定");
    }

    [MenuItem("MoShou/动画系统/生成Player Animator")]
    public static void GeneratePlayerAnimator()
    {
        string playerPath = "Assets/Resources/Models/Player";
        if (!Directory.Exists(playerPath))
        {
            // 尝试其他可能的路径
            string[] possiblePaths = {
                "Assets/Resources/Models/Characters/Player",
                "Assets/Resources/Prefabs/Characters"
            };

            foreach (var p in possiblePaths)
            {
                if (Directory.Exists(p))
                {
                    playerPath = p;
                    break;
                }
            }
        }

        // Player的Animator已经存在，只需要确保映射正确
        string existingController = "Assets/Resources/Animations/Player_Animator.controller";
        if (File.Exists(existingController))
        {
            Debug.Log("[MonsterAnimatorGenerator] Player Animator已存在，跳过生成");
            EditorUtility.DisplayDialog("提示", "Player Animator已存在于:\n" + existingController, "确定");
            return;
        }

        Debug.Log("[MonsterAnimatorGenerator] Player目录: " + playerPath);
    }

    private static bool GenerateAnimatorForMonster(string modelPath, string monsterName)
    {
        // 查找FBX文件
        string[] fbxFiles = Directory.GetFiles(modelPath, "*.fbx", SearchOption.TopDirectoryOnly);
        if (fbxFiles.Length == 0)
        {
            // 尝试查找FBX的meta对应的资源
            string[] allFiles = Directory.GetFiles(modelPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var f in allFiles)
            {
                if (!f.EndsWith(".meta") && !f.EndsWith(".mat") && !f.EndsWith(".png") && !f.EndsWith(".jpg"))
                {
                    // 可能是不带扩展名的FBX
                    GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(f);
                    if (model != null)
                    {
                        Debug.Log($"[MonsterAnimatorGenerator] 找到模型: {f}");
                    }
                }
            }
        }

        // 获取模型中的动画片段
        string modelFile = null;
        foreach (var file in Directory.GetFiles(modelPath))
        {
            if (file.EndsWith(".fbx") || file.EndsWith(".FBX"))
            {
                modelFile = file;
                break;
            }

            // 检查是否是Unity资源（无扩展名的FBX）
            string assetPath = file.Replace("\\", "/");
            if (!assetPath.EndsWith(".meta"))
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (obj != null)
                {
                    modelFile = assetPath;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(modelFile))
        {
            Debug.LogWarning($"[MonsterAnimatorGenerator] 未找到模型文件: {modelPath}");
            return false;
        }

        modelFile = modelFile.Replace("\\", "/");

        // 获取模型中的所有动画片段
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelFile);
        List<AnimationClip> clips = new List<AnimationClip>();

        foreach (var asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
            {
                clips.Add(clip);
                Debug.Log($"[{monsterName}] 找到动画片段: {clip.name}");
            }
        }

        if (clips.Count == 0)
        {
            Debug.LogWarning($"[MonsterAnimatorGenerator] {monsterName} 没有动画片段");
            return false;
        }

        // 创建Animator Controller
        string outputDir = "Assets/Resources/Animations/Monsters";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string controllerPath = $"{outputDir}/{monsterName}_Animator.controller";

        // 如果已存在，先删除
        if (File.Exists(controllerPath))
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 添加参数
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Skill", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        // 获取根状态机
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // 创建状态并映射动画
        AnimatorState idleState = null;
        AnimatorState runState = null;
        AnimatorState attackState = null;
        AnimatorState skillState = null;
        AnimatorState hitState = null;
        AnimatorState deathState = null;

        // 查找并创建Idle状态
        AnimationClip idleClip = FindClipByMapping(clips, "Idle");
        if (idleClip != null)
        {
            idleState = rootStateMachine.AddState("Idle", new Vector3(300, 0, 0));
            idleState.motion = idleClip;
            rootStateMachine.defaultState = idleState;
        }

        // 查找并创建Run状态
        AnimationClip runClip = FindClipByMapping(clips, "Run");
        if (runClip != null)
        {
            runState = rootStateMachine.AddState("Run", new Vector3(300, 100, 0));
            runState.motion = runClip;
        }

        // 查找并创建Attack状态
        AnimationClip attackClip = FindClipByMapping(clips, "Attack");
        if (attackClip != null)
        {
            attackState = rootStateMachine.AddState("Attack", new Vector3(500, 50, 0));
            attackState.motion = attackClip;
        }

        // 查找并创建Skill状态
        AnimationClip skillClip = FindClipByMapping(clips, "Skill");
        if (skillClip != null)
        {
            skillState = rootStateMachine.AddState("Skill", new Vector3(500, 150, 0));
            skillState.motion = skillClip;
        }

        // 查找并创建Hit状态
        AnimationClip hitClip = FindClipByMapping(clips, "Hit");
        if (hitClip != null)
        {
            hitState = rootStateMachine.AddState("Hit", new Vector3(300, 200, 0));
            hitState.motion = hitClip;
        }

        // 查找并创建Death状态
        AnimationClip deathClip = FindClipByMapping(clips, "Death");
        if (deathClip != null)
        {
            deathState = rootStateMachine.AddState("Death", new Vector3(300, 300, 0));
            deathState.motion = deathClip;
        }

        // 创建状态转换
        if (idleState != null && runState != null)
        {
            // Idle -> Run (Speed > 0.1)
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.1f;

            // Run -> Idle (Speed < 0.1)
            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.1f;
        }

        // Any State -> Attack
        if (attackState != null)
        {
            var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            anyToAttack.hasExitTime = false;
            anyToAttack.duration = 0.1f;

            // Attack -> Idle
            if (idleState != null)
            {
                var attackToIdle = attackState.AddTransition(idleState);
                attackToIdle.hasExitTime = true;
                attackToIdle.exitTime = 0.9f;
                attackToIdle.duration = 0.1f;
            }
        }

        // Any State -> Skill
        if (skillState != null)
        {
            var anyToSkill = rootStateMachine.AddAnyStateTransition(skillState);
            anyToSkill.AddCondition(AnimatorConditionMode.If, 0, "Skill");
            anyToSkill.hasExitTime = false;
            anyToSkill.duration = 0.1f;

            // Skill -> Idle
            if (idleState != null)
            {
                var skillToIdle = skillState.AddTransition(idleState);
                skillToIdle.hasExitTime = true;
                skillToIdle.exitTime = 0.9f;
                skillToIdle.duration = 0.1f;
            }
        }

        // Any State -> Hit
        if (hitState != null)
        {
            var anyToHit = rootStateMachine.AddAnyStateTransition(hitState);
            anyToHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
            anyToHit.hasExitTime = false;
            anyToHit.duration = 0.05f;

            // Hit -> Idle
            if (idleState != null)
            {
                var hitToIdle = hitState.AddTransition(idleState);
                hitToIdle.hasExitTime = true;
                hitToIdle.exitTime = 0.9f;
                hitToIdle.duration = 0.1f;
            }
        }

        // Any State -> Death
        if (deathState != null)
        {
            var anyToDeath = rootStateMachine.AddAnyStateTransition(deathState);
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0.1f;
            // Death状态不需要退出，保持在最后一帧
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log($"[MonsterAnimatorGenerator] 已为 {monsterName} 创建 Animator Controller: {controllerPath}");
        return true;
    }

    private static AnimationClip FindClipByMapping(List<AnimationClip> clips, string standardName)
    {
        if (!AnimationNameMappings.ContainsKey(standardName))
            return null;

        string[] possibleNames = AnimationNameMappings[standardName];

        foreach (var possibleName in possibleNames)
        {
            foreach (var clip in clips)
            {
                if (clip.name.Equals(possibleName, System.StringComparison.OrdinalIgnoreCase) ||
                    clip.name.StartsWith(possibleName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return clip;
                }
            }
        }

        return null;
    }

    [MenuItem("MoShou/动画系统/应用Animator到怪物预制体")]
    public static void ApplyAnimatorsToPrefabs()
    {
        string prefabPath = "Assets/Resources/Prefabs/Characters";
        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }

        // 查找所有怪物模型
        string[] monsterDirs = new string[]
        {
            "Assets/Resources/Models/Monsters/Slime",
            "Assets/Resources/Models/Monsters/Goblin",
            "Assets/Resources/Models/Monsters/GoblinElite",
            "Assets/Resources/Models/Monsters/GoblinKing",
            "Assets/Resources/Models/Monsters/Wolf"
        };

        int updated = 0;
        foreach (string dir in monsterDirs)
        {
            if (!Directory.Exists(dir)) continue;

            string monsterName = Path.GetFileName(dir);
            string controllerPath = $"Assets/Resources/Animations/Monsters/{monsterName}_Animator.controller";

            if (!File.Exists(controllerPath))
            {
                Debug.LogWarning($"[ApplyAnimators] 未找到Animator: {controllerPath}");
                continue;
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) continue;

            // 查找模型文件
            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;

                string assetPath = file.Replace("\\", "/");
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (model != null)
                {
                    // 创建或更新预制体
                    string prefabFilePath = $"{prefabPath}/Monster_{monsterName}.prefab";

                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);

                    // 添加或获取Animator
                    Animator animator = instance.GetComponent<Animator>();
                    if (animator == null)
                    {
                        animator = instance.AddComponent<Animator>();
                    }
                    animator.runtimeAnimatorController = controller;

                    // 确保有MonsterController组件
                    var mc = instance.GetComponent<MonsterController>();
                    if (mc == null)
                    {
                        mc = instance.AddComponent<MonsterController>();
                    }
                    mc.monsterId = $"MON_{monsterName.ToUpper()}_001";
                    mc.monsterName = monsterName;

                    // 保存为预制体
                    PrefabUtility.SaveAsPrefabAsset(instance, prefabFilePath);
                    Object.DestroyImmediate(instance);

                    Debug.Log($"[ApplyAnimators] 已更新预制体: {prefabFilePath}");
                    updated++;
                    break;
                }
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("应用完成", $"已更新 {updated} 个怪物预制体的Animator", "确定");
    }
}
