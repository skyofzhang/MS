using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace MoShou.Editor.AutoBuild
{
    /// <summary>
    /// Scene Modifier for automated scene modifications
    /// Can be triggered via:
    /// 1. Unity Menu: Tools/AutoBuild/...
    /// 2. Unity CLI: -executeMethod MoShou.Editor.AutoBuild.SceneModifier.XXX
    /// 3. Auto-trigger: Watches .task/.execute_trigger file
    /// </summary>
    [InitializeOnLoad]
    public static class SceneModifier
    {
        private const string LOG_PREFIX = "[SceneModifier]";
        private const string TRIGGER_FILE = "E:/AI_Project/MS/.task/.execute_trigger";

        // Static constructor for InitializeOnLoad
        static SceneModifier()
        {
            // Check for trigger file on Editor startup
            EditorApplication.delayCall += CheckAndExecuteTrigger;
            // Also register for update to continuously check
            EditorApplication.update += OnEditorUpdate;
        }

        private static float _lastCheckTime = 0;
        private static void OnEditorUpdate()
        {
            // Check every 5 seconds
            if (EditorApplication.timeSinceStartup - _lastCheckTime > 5f)
            {
                _lastCheckTime = (float)EditorApplication.timeSinceStartup;
                CheckAndExecuteTrigger();
            }
        }

        /// <summary>
        /// Check if there's a pending trigger and execute it
        /// </summary>
        private static void CheckAndExecuteTrigger()
        {
            if (File.Exists(TRIGGER_FILE))
            {
                try
                {
                    string content = File.ReadAllText(TRIGGER_FILE);
                    Debug.Log($"{LOG_PREFIX} Found trigger file: {content}");

                    // Parse and execute
                    if (content.Contains("AddStartButton"))
                    {
                        AddStartButtonToMainMenu();
                    }
                    else if (content.Contains("BuildAPK"))
                    {
                        MoShou.Editor.BuildScript.BuildAndroid();
                    }

                    // Delete trigger file after execution
                    File.Delete(TRIGGER_FILE);
                    Debug.Log($"{LOG_PREFIX} Trigger file processed and deleted");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{LOG_PREFIX} Error processing trigger: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Menu item for manual execution
        /// </summary>
        [MenuItem("Tools/AutoBuild/MS-009 Add Start Button")]
        public static void MenuAddStartButton()
        {
            AddStartButtonToMainMenu();
        }

        /// <summary>
        /// MS-009: Add Start Button to MainMenu scene
        /// </summary>
        public static void AddStartButtonToMainMenu()
        {
            Debug.Log($"{LOG_PREFIX} Starting AddStartButtonToMainMenu...");

            string scenePath = "Assets/Scenes/MainMenu.unity";

            // Open scene
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"{LOG_PREFIX} Failed to open scene: {scenePath}");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"{LOG_PREFIX} Opened scene: {scenePath}");

            // Find or create Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            GameObject canvasObj;

            if (canvas == null)
            {
                Debug.Log($"{LOG_PREFIX} Creating new Canvas...");
                canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Add CanvasScaler
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                // Add GraphicRaycaster
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvasObj = canvas.gameObject;
                Debug.Log($"{LOG_PREFIX} Found existing Canvas");
            }

            // Check if StartButton already exists
            Transform existingButton = canvasObj.transform.Find("StartButton");
            if (existingButton != null)
            {
                Debug.Log($"{LOG_PREFIX} StartButton already exists, updating...");
                Object.DestroyImmediate(existingButton.gameObject);
            }

            // Create StartButton
            Debug.Log($"{LOG_PREFIX} Creating StartButton...");
            GameObject buttonObj = new GameObject("StartButton");
            buttonObj.transform.SetParent(canvasObj.transform, false);

            // Add RectTransform
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 80);
            rectTransform.anchoredPosition = new Vector2(0, -100); // Center, slightly below middle

            // Add Image (button background)
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.29f, 0.56f, 0.85f, 1f); // #4A90D9

            // Add Button
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.29f, 0.56f, 0.85f, 1f);      // #4A90D9
            colors.highlightedColor = new Color(0.36f, 0.63f, 0.91f, 1f); // #5BA0E9
            colors.pressedColor = new Color(0.23f, 0.50f, 0.79f, 1f);     // #3A80C9
            colors.selectedColor = colors.normalColor;
            button.colors = colors;

            // Create Text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = "开始游戏";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            // Find or create MainMenuManager
            MoShou.Core.MainMenuManager menuManager = Object.FindObjectOfType<MoShou.Core.MainMenuManager>();
            if (menuManager == null)
            {
                Debug.Log($"{LOG_PREFIX} Creating MainMenuManager...");
                GameObject managerObj = new GameObject("MainMenuManager");
                menuManager = managerObj.AddComponent<MoShou.Core.MainMenuManager>();
            }

            // Connect button to manager using SerializedObject
            SerializedObject serializedManager = new SerializedObject(menuManager);
            SerializedProperty startButtonProp = serializedManager.FindProperty("startButton");
            if (startButtonProp != null)
            {
                startButtonProp.objectReferenceValue = button;
                serializedManager.ApplyModifiedProperties();
                Debug.Log($"{LOG_PREFIX} Connected StartButton to MainMenuManager.startButton");
            }
            else
            {
                Debug.LogWarning($"{LOG_PREFIX} Could not find startButton property on MainMenuManager");
            }

            // Mark scene dirty and save
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);

            if (saved)
            {
                Debug.Log($"{LOG_PREFIX} Scene saved successfully!");
                WriteResult("SUCCESS", "MainMenu scene updated with StartButton");
            }
            else
            {
                Debug.LogError($"{LOG_PREFIX} Failed to save scene!");
                WriteResult("FAILED", "Could not save scene");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Write result to file for Claude Code to read
        /// </summary>
        private static void WriteResult(string status, string message)
        {
            string resultPath = "E:/AI_Project/MS/.task/modify_result.json";
            string json = $@"{{
  ""task_id"": ""MS-009-01"",
  ""status"": ""{status}"",
  ""message"": ""{message}"",
  ""timestamp"": ""{System.DateTime.Now:o}""
}}";
            File.WriteAllText(resultPath, json);
            Debug.Log($"{LOG_PREFIX} Result written to {resultPath}");
        }

        /// <summary>
        /// Verify MainMenu scene has required components
        /// </summary>
        public static void VerifyMainMenuScene()
        {
            Debug.Log($"{LOG_PREFIX} Verifying MainMenu scene...");

            string scenePath = "Assets/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            bool hasCanvas = Object.FindObjectOfType<Canvas>() != null;
            bool hasStartButton = GameObject.Find("StartButton") != null;
            bool hasManager = Object.FindObjectOfType<MoShou.Core.MainMenuManager>() != null;

            string status = (hasCanvas && hasStartButton && hasManager) ? "VERIFIED" : "INCOMPLETE";
            string details = $"Canvas:{hasCanvas}, StartButton:{hasStartButton}, MainMenuManager:{hasManager}";

            Debug.Log($"{LOG_PREFIX} Verification: {status} - {details}");
            WriteResult(status, details);
        }
    }
}
