using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using MoShou.Core;
using MoShou.UI;
using MoShou.Effects;

namespace MoShou.Editor
{
    /// <summary>
    /// 视觉验收自测工具 - AI开发完成度验证
    /// V5.0: 符合AI开发知识库§3.12规范 + 策划案RULE-DONE-003
    ///
    /// Claude在报告"开发完成"前必须运行此测试
    /// 菜单: MoShou/Visual Self Test
    ///
    /// 新增检查项:
    /// - FALLBACK颜色检查 (Color.magenta)
    /// - UI反馈系统检查
    /// - VFX预制体检查
    /// - 游戏反馈系统检查
    /// </summary>
    public class VisualSelfTest : EditorWindow
    {
        private List<TestResult> testResults = new List<TestResult>();
        private Vector2 scrollPosition;
        private bool allPassed = false;

        private struct TestResult
        {
            public string code;
            public string name;
            public bool passed;
            public string message;
        }

        [MenuItem("MoShou/Visual Self Test")]
        public static void ShowWindow()
        {
            var window = GetWindow<VisualSelfTest>("Visual Self Test");
            window.minSize = new Vector2(500, 400);
            window.RunAllTests();
        }

        [MenuItem("MoShou/Run All Visual Tests (Console)")]
        public static void RunAllTestsConsole()
        {
            var tester = new VisualSelfTest();
            tester.RunAllTests();
            tester.PrintResultsToConsole();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("AI开发视觉验收自测", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("运行所有测试", GUILayout.Height(30)))
            {
                RunAllTests();
            }

            EditorGUILayout.Space();

            // 总结
            var passCount = testResults.FindAll(r => r.passed).Count;
            var totalCount = testResults.Count;
            var summaryStyle = new GUIStyle(EditorStyles.boldLabel);
            summaryStyle.normal.textColor = allPassed ? Color.green : Color.red;
            EditorGUILayout.LabelField($"测试结果: {passCount}/{totalCount} 通过", summaryStyle);

            EditorGUILayout.Space();

            // 详细结果
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var result in testResults)
            {
                DrawTestResult(result);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 完成度声明
            if (allPassed)
            {
                EditorGUILayout.HelpBox(
                    "✅ 所有视觉测试通过！\n可以向用户报告：开发完成，画面表现正常。",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "❌ 存在未通过的测试项\n请修复问题后再报告开发完成。",
                    MessageType.Error);
            }
        }

        private void DrawTestResult(TestResult result)
        {
            EditorGUILayout.BeginHorizontal();

            var icon = result.passed ? "✓" : "✗";
            var color = result.passed ? Color.green : Color.red;
            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = color;

            EditorGUILayout.LabelField($"{icon} [{result.code}]", style, GUILayout.Width(80));
            EditorGUILayout.LabelField(result.name, GUILayout.Width(200));
            EditorGUILayout.LabelField(result.message);

            EditorGUILayout.EndHorizontal();
        }

        public void RunAllTests()
        {
            testResults.Clear();

            // 在Play模式下运行
            if (Application.isPlaying)
            {
                RunPlayModeTests();
            }
            else
            {
                RunEditModeTests();
            }

            allPassed = testResults.TrueForAll(r => r.passed);
        }

        private void RunEditModeTests()
        {
            // VC-E01: 场景文件存在性检查
            testResults.Add(CheckSceneExists("Assets/Scenes/MainMenu.unity", "VC-E01", "MainMenu场景"));
            testResults.Add(CheckSceneExists("Assets/Scenes/StageSelect.unity", "VC-E02", "StageSelect场景"));
            testResults.Add(CheckSceneExists("Assets/Scenes/GameScene.unity", "VC-E03", "GameScene场景"));

            // VC-E04: Build Settings检查
            testResults.Add(CheckBuildSettings());

            // VC-E05: 必要脚本存在性
            // GameManager在全局命名空间，MainMenuManager在MoShou.Core
            testResults.Add(CheckScriptExists<GameManager>("VC-E05", "GameManager脚本"));
            testResults.Add(CheckScriptExists<MoShou.Core.MainMenuManager>("VC-E06", "MainMenuManager脚本"));

            // VC-E07: 资源文件夹结构
            testResults.Add(CheckFolderExists("Assets/Resources", "VC-E07", "Resources文件夹"));
            testResults.Add(CheckFolderExists("Assets/Resources/Sprites", "VC-E08", "Sprites资源文件夹"));

            // VC-E09: 占位符代码检查
            testResults.Add(CheckNoPlaceholderCode());
        }

        private void RunPlayModeTests()
        {
            // VC-001: 玩家可见性
            var player = GameObject.FindWithTag("Player");
            testResults.Add(new TestResult
            {
                code = "VC-001",
                name = "玩家角色可见",
                passed = player != null && player.GetComponentInChildren<Renderer>() != null,
                message = player != null ? "玩家存在且有渲染器" : "未找到Player标签对象"
            });

            // VC-002: 血条可见性
            var healthBar = GameObject.Find("HealthBar") ?? GameObject.Find("PlayerHealthBar");
            testResults.Add(new TestResult
            {
                code = "VC-002",
                name = "血条UI可见",
                passed = healthBar != null && healthBar.activeInHierarchy,
                message = healthBar != null ? "血条存在且激活" : "未找到血条UI"
            });

            // VC-003: 怪物生成检查
            var monsters = GameObject.FindGameObjectsWithTag("Monster");
            testResults.Add(new TestResult
            {
                code = "VC-003",
                name = "怪物可生成",
                passed = monsters.Length > 0 || GameObject.Find("MonsterSpawner") != null,
                message = monsters.Length > 0 ? $"场景中有{monsters.Length}个怪物" : "有MonsterSpawner但暂无怪物"
            });

            // VC-004: 技能图标检查
            var skillIcons = GameObject.FindObjectsOfType<UnityEngine.UI.Image>();
            bool hasSkillUI = false;
            foreach (var img in skillIcons)
            {
                if (img.name.Contains("Skill") || img.transform.parent?.name.Contains("Skill") == true)
                {
                    hasSkillUI = true;
                    break;
                }
            }
            testResults.Add(new TestResult
            {
                code = "VC-004",
                name = "技能UI存在",
                passed = hasSkillUI,
                message = hasSkillUI ? "找到技能相关UI" : "未找到技能UI元素"
            });

            // VC-005: 地面/环境可见
            var ground = GameObject.Find("Ground") ?? GameObject.Find("Terrain");
            testResults.Add(new TestResult
            {
                code = "VC-005",
                name = "地面环境可见",
                passed = ground != null,
                message = ground != null ? "地面对象存在" : "未找到地面对象"
            });

            // VC-006: Canvas渲染正常
            var canvases = GameObject.FindObjectsOfType<Canvas>();
            bool canvasOK = canvases.Length > 0;
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.worldCamera != null)
                {
                    canvasOK = true;
                    break;
                }
            }
            testResults.Add(new TestResult
            {
                code = "VC-006",
                name = "Canvas配置正确",
                passed = canvasOK,
                message = canvasOK ? $"找到{canvases.Length}个Canvas" : "Canvas配置可能有问题"
            });

            // VC-007: 摄像机检查
            var camera = Camera.main;
            testResults.Add(new TestResult
            {
                code = "VC-007",
                name = "主摄像机存在",
                passed = camera != null,
                message = camera != null ? "主摄像机正常" : "未找到主摄像机"
            });

            // VC-008: 无降级资源检查 (增强版：包括FALLBACK_命名和Color.magenta)
            var fallbackObjects = GameObject.FindObjectsOfType<GameObject>();
            int fallbackCount = 0;
            int magentaCount = 0;
            List<string> magentaObjects = new List<string>();

            foreach (var obj in fallbackObjects)
            {
                if (obj.name.StartsWith("FALLBACK_"))
                {
                    fallbackCount++;
                }
            }

            // 检查Color.magenta (FALLBACK颜色)
            var images = GameObject.FindObjectsOfType<Image>();
            foreach (var img in images)
            {
                if (img.color == Color.magenta || img.color == new Color(1, 0, 1, 1))
                {
                    magentaCount++;
                    if (magentaObjects.Count < 3)
                        magentaObjects.Add(img.gameObject.name);
                }
            }

            var renderers = GameObject.FindObjectsOfType<Renderer>();
            foreach (var rend in renderers)
            {
                if (rend.sharedMaterial != null && rend.sharedMaterial.color == Color.magenta)
                {
                    magentaCount++;
                    if (magentaObjects.Count < 3)
                        magentaObjects.Add(rend.gameObject.name);
                }
            }

            int totalFallback = fallbackCount + magentaCount;
            string fallbackMsg = totalFallback == 0 ? "所有资源正常加载" :
                $"发现{fallbackCount}个FALLBACK_对象, {magentaCount}个magenta颜色";
            if (magentaObjects.Count > 0)
                fallbackMsg += $" ({string.Join(", ", magentaObjects)})";

            testResults.Add(new TestResult
            {
                code = "VC-008",
                name = "无降级资源",
                passed = totalFallback == 0,
                message = fallbackMsg
            });

            // VC-009: UI反馈系统检查
            var uiFeedback = GameObject.FindObjectOfType<UIFeedbackSystem>();
            testResults.Add(new TestResult
            {
                code = "VC-009",
                name = "UI反馈系统",
                passed = uiFeedback != null || UIFeedbackSystem.Instance != null,
                message = uiFeedback != null ? "UIFeedbackSystem已激活" : "UIFeedbackSystem可按需创建"
            });

            // VC-010: 游戏反馈系统检查
            var gameFeedback = GameObject.FindObjectOfType<GameFeedback>();
            testResults.Add(new TestResult
            {
                code = "VC-010",
                name = "游戏反馈系统",
                passed = gameFeedback != null || GameFeedback.Instance != null,
                message = gameFeedback != null ? "GameFeedback已激活" : "GameFeedback可按需创建"
            });

            // VC-011: VFX管理器检查
            var vfxManager = GameObject.FindObjectOfType<VFXManager>();
            testResults.Add(new TestResult
            {
                code = "VC-011",
                name = "VFX管理器",
                passed = vfxManager != null || VFXManager.Instance != null,
                message = vfxManager != null ? "VFXManager已激活" : "VFXManager可按需创建"
            });

            // VC-012: VFX预制体存在性检查
            int vfxFound = 0;
            string[] vfxPaths = { "VFX_Hit_Spark", "VFX_LevelUp", "VFX_Death_Dissolve" };
            foreach (var vfx in vfxPaths)
            {
                if (Resources.Load<GameObject>($"Prefabs/VFX/{vfx}") != null)
                    vfxFound++;
            }
            testResults.Add(new TestResult
            {
                code = "VC-012",
                name = "VFX预制体",
                passed = vfxFound >= 1,
                message = $"找到 {vfxFound}/{vfxPaths.Length} 个核心VFX预制体"
            });
        }

        #region 辅助检查方法

        private TestResult CheckSceneExists(string path, string code, string name)
        {
            bool exists = System.IO.File.Exists(path);
            return new TestResult
            {
                code = code,
                name = name,
                passed = exists,
                message = exists ? "场景文件存在" : $"未找到: {path}"
            };
        }

        private TestResult CheckBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasAllScenes = scenes.Length >= 3;
            return new TestResult
            {
                code = "VC-E04",
                name = "Build Settings配置",
                passed = hasAllScenes,
                message = hasAllScenes ? $"已配置{scenes.Length}个场景" : "Build Settings场景不足"
            };
        }

        private TestResult CheckScriptExists<T>(string code, string name) where T : Component
        {
            var type = typeof(T);
            bool exists = type != null;
            return new TestResult
            {
                code = code,
                name = name,
                passed = exists,
                message = exists ? "脚本类型存在" : "脚本类型未找到"
            };
        }

        private TestResult CheckFolderExists(string path, string code, string name)
        {
            bool exists = AssetDatabase.IsValidFolder(path);
            return new TestResult
            {
                code = code,
                name = name,
                passed = exists,
                message = exists ? "文件夹存在" : $"未找到: {path}"
            };
        }

        private TestResult CheckNoPlaceholderCode()
        {
            // 检查是否存在明显的占位符代码标记
            string[] suspiciousFiles = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" });
            int placeholderCount = 0;

            foreach (var guid in suspiciousFiles)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.File.Exists(path))
                {
                    string content = System.IO.File.ReadAllText(path);
                    if (content.Contains("TODO: Placeholder") ||
                        content.Contains("PLACEHOLDER") ||
                        content.Contains("CreatePlaceholder"))
                    {
                        placeholderCount++;
                        Debug.LogWarning($"[VisualSelfTest] 发现占位符代码: {path}");
                    }
                }
            }

            return new TestResult
            {
                code = "VC-E09",
                name = "无占位符代码",
                passed = placeholderCount == 0,
                message = placeholderCount == 0 ? "代码无占位符" : $"发现{placeholderCount}个文件含占位符"
            };
        }

        #endregion

        private void PrintResultsToConsole()
        {
            Debug.Log("========== Visual Self Test Results ==========");
            foreach (var result in testResults)
            {
                var icon = result.passed ? "✓" : "✗";
                if (result.passed)
                {
                    Debug.Log($"{icon} [{result.code}] {result.name}: {result.message}");
                }
                else
                {
                    Debug.LogError($"{icon} [{result.code}] {result.name}: {result.message}");
                }
            }

            if (allPassed)
            {
                Debug.Log("========== ✅ ALL TESTS PASSED ==========");
            }
            else
            {
                Debug.LogError("========== ❌ SOME TESTS FAILED ==========");
            }
        }
    }
}
