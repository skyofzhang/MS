using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// 自动创建玩家Animator Controller的编辑器工具
/// </summary>
public class PlayerAnimatorSetup : EditorWindow
{
    [MenuItem("MoShou/Setup Player Animator")]
    public static void SetupPlayerAnimator()
    {
        // 获取FBX中的动画
        string fbxPath = "Assets/Resources/Models/Player/Player_Archer.fbx";
        var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

        // 创建Animator Controller
        string controllerPath = "Assets/Resources/Animations/Player_Animator.controller";

        // 确保目录存在
        string directory = Path.GetDirectoryName(controllerPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 如果已存在，删除旧的
        if (File.Exists(controllerPath))
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }

        // 创建新的AnimatorController
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 获取基础层
        var rootStateMachine = controller.layers[0].stateMachine;

        // 添加参数
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Skill1", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Skill2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Skill3", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Victory", AnimatorControllerParameterType.Trigger);

        AnimatorState idleState = null;
        AnimatorState runState = null;

        // 添加所有动画状态
        foreach (var obj in clips)
        {
            var clip = obj as AnimationClip;
            if (clip == null || clip.name.StartsWith("__preview__")) continue;

            Debug.Log($"Found animation clip: {clip.name}");

            var state = rootStateMachine.AddState(clip.name);
            state.motion = clip;

            // 设置循环
            if (clip.name == "Idle" || clip.name == "Run")
            {
                // 通过SerializedObject设置循环
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            // 记录关键状态
            if (clip.name == "Idle") idleState = state;
            if (clip.name == "Run") runState = state;
        }

        // 设置默认状态
        if (idleState != null)
        {
            rootStateMachine.defaultState = idleState;
        }

        // 创建过渡
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

        // 添加Any State过渡用于触发动画
        foreach (var obj in clips)
        {
            var clip = obj as AnimationClip;
            if (clip == null || clip.name.StartsWith("__preview__")) continue;

            // 跳过Idle和Run
            if (clip.name == "Idle" || clip.name == "Run") continue;

            var targetState = FindState(rootStateMachine, clip.name);
            if (targetState == null) continue;

            // 创建Any State过渡
            var anyTransition = rootStateMachine.AddAnyStateTransition(targetState);
            anyTransition.hasExitTime = false;
            anyTransition.duration = 0.1f;

            // 设置触发条件
            string triggerName = clip.name;
            if (clip.name.StartsWith("Attack"))
                triggerName = "Attack";
            else if (clip.name.StartsWith("Skill"))
                triggerName = clip.name;

            if (controller.parameters.Length > 0)
            {
                foreach (var param in controller.parameters)
                {
                    if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        anyTransition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                        break;
                    }
                }
            }

            // 添加返回Idle的过渡
            if (idleState != null)
            {
                var backToIdle = targetState.AddTransition(idleState);
                backToIdle.hasExitTime = true;
                backToIdle.exitTime = 0.9f;
                backToIdle.duration = 0.1f;
            }
        }

        // 保存
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Player Animator Controller created at: {controllerPath}");

        // 自动应用到Prefab
        ApplyAnimatorToPrefab(controllerPath);
    }

    static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == name)
                return state.state;
        }
        return null;
    }

    static void ApplyAnimatorToPrefab(string controllerPath)
    {
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (controller == null) return;

        string prefabPath = "Assets/Resources/Prefabs/Characters/Player_Archer.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;

        var animator = prefab.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = prefab.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();

        Debug.Log($"Applied animator controller to {prefabPath}");
    }
}
