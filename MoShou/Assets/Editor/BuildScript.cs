using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace MoShou.Editor
{
    /// <summary>
    /// 构建脚本 - 自动化打包APK
    /// </summary>
    public class BuildScript
    {
        private static readonly string[] SCENES = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/StageSelect.unity",
            "Assets/Scenes/GameScene.unity",
            "Assets/Scenes/MainScene.unity"
        };

        /// <summary>
        /// 构建Android APK (菜单项)
        /// </summary>
        [MenuItem("MoShou/Build/Android APK")]
        public static void BuildAndroid()
        {
            BuildAndroidInternal(false);
        }

        /// <summary>
        /// 构建Android APK (开发版本)
        /// </summary>
        [MenuItem("MoShou/Build/Android APK (Development)")]
        public static void BuildAndroidDev()
        {
            BuildAndroidInternal(true);
        }

        /// <summary>
        /// 内部构建方法
        /// </summary>
        private static void BuildAndroidInternal(bool development)
        {
            // 配置构建设置
            ConfigureAndroidSettings();

            // 确定输出路径
            string buildFolder = Path.Combine(Application.dataPath, "..", "Builds", "Android");
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string suffix = development ? "_dev" : "";
            string apkPath = Path.Combine(buildFolder, $"MoShou_{timestamp}{suffix}.apk");

            // 构建选项
            BuildOptions options = BuildOptions.None;
            if (development)
            {
                options |= BuildOptions.Development;
                options |= BuildOptions.AllowDebugging;
            }

            // 获取有效场景列表
            string[] validScenes = GetValidScenes();
            if (validScenes.Length == 0)
            {
                Debug.LogError("[BuildScript] 没有找到有效的场景文件!");
                return;
            }

            Debug.Log($"[BuildScript] 开始构建Android APK...");
            Debug.Log($"[BuildScript] 输出路径: {apkPath}");
            Debug.Log($"[BuildScript] 场景数量: {validScenes.Length}");

            // 执行构建
            var report = BuildPipeline.BuildPlayer(validScenes, apkPath, BuildTarget.Android, options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] 构建成功! APK大小: {report.summary.totalSize / 1024 / 1024}MB");
                Debug.Log($"[BuildScript] 路径: {apkPath}");

                // 打开输出文件夹
                EditorUtility.RevealInFinder(apkPath);
            }
            else
            {
                Debug.LogError($"[BuildScript] 构建失败! 错误数: {report.summary.totalErrors}");
            }
        }

        /// <summary>
        /// 配置Android设置
        /// </summary>
        private static void ConfigureAndroidSettings()
        {
            // 基本设置
            PlayerSettings.productName = "魔兽小游戏";
            PlayerSettings.companyName = "MoShou Studio";
            PlayerSettings.bundleVersion = "1.0.0";

            // Android特定设置
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.moshou.game");

            // 最低API级别
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22; // Android 5.1

            // 目标API级别
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel31; // Android 12

            // 脚本后端
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

            // 目标架构
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            // 图形API
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[]
            {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan
            });

            // 屏幕方向设置 - 竖屏
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            Debug.Log("[BuildScript] Android设置配置完成");
        }

        /// <summary>
        /// 获取有效场景列表
        /// </summary>
        private static string[] GetValidScenes()
        {
            var validScenes = new System.Collections.Generic.List<string>();

            foreach (var scene in SCENES)
            {
                if (File.Exists(Path.Combine(Application.dataPath, "..", scene)))
                {
                    validScenes.Add(scene);
                }
            }

            // 如果没有预设场景，使用Build Settings中的场景
            if (validScenes.Count == 0)
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (scene.enabled)
                    {
                        validScenes.Add(scene.path);
                    }
                }
            }

            return validScenes.ToArray();
        }

        /// <summary>
        /// 构建WebGL (菜单项)
        /// </summary>
        [MenuItem("MoShou/Build/WebGL")]
        public static void BuildWebGL()
        {
            // 配置WebGL设置
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.memorySize = 256;

            string buildFolder = Path.Combine(Application.dataPath, "..", "Builds", "WebGL");
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string[] validScenes = GetValidScenes();
            if (validScenes.Length == 0)
            {
                Debug.LogError("[BuildScript] 没有找到有效的场景文件!");
                return;
            }

            Debug.Log($"[BuildScript] 开始构建WebGL...");

            var report = BuildPipeline.BuildPlayer(validScenes, buildFolder, BuildTarget.WebGL, BuildOptions.None);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] WebGL构建成功!");
                EditorUtility.RevealInFinder(buildFolder);
            }
            else
            {
                Debug.LogError($"[BuildScript] WebGL构建失败!");
            }
        }

        /// <summary>
        /// 清理构建文件夹
        /// </summary>
        [MenuItem("MoShou/Build/Clean Build Folder")]
        public static void CleanBuildFolder()
        {
            string buildFolder = Path.Combine(Application.dataPath, "..", "Builds");
            if (Directory.Exists(buildFolder))
            {
                Directory.Delete(buildFolder, true);
                Debug.Log("[BuildScript] 构建文件夹已清理");
            }
        }
    }
}
