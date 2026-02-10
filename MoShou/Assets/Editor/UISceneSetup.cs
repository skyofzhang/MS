using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// UI场景设置工具
/// 为MainMenu和StageSelect场景自动设置UI
/// </summary>
public class UISceneSetup : EditorWindow
{
    [MenuItem("MoShou/场景UI设置/设置MainMenu场景")]
    public static void SetupMainMenuScene()
    {
        // 打开MainMenu场景
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError($"场景不存在: {scenePath}");
            return;
        }

        EditorSceneManager.OpenScene(scenePath);

        // 查找或创建Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 应用背景
        SetupUIBackground(canvas.transform, "UI/MainMenu/UI_MainMenu_BG");

        // 应用LOGO
        SetupUIImage(canvas.transform, "Logo", "UI/MainMenu/UI_MainMenu_Logo",
            new Vector2(0.5f, 0.8f), new Vector2(600, 200));

        // 设置按钮样式
        ApplyButtonSprites("StartButton", "UI/MainMenu/UI_Btn_Start_Normal", "UI/MainMenu/UI_Btn_Start_Pressed");
        ApplyButtonSprites("ContinueButton", "UI/MainMenu/UI_Btn_Continue_Normal", "UI/MainMenu/UI_Btn_Continue_Disabled");
        ApplyButtonSprites("SettingsButton", "UI/MainMenu/UI_Btn_Settings_Normal", null);
        ApplyButtonSprites("QuitButton", "UI/MainMenu/UI_Btn_Quit_Normal", null);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[UISceneSetup] MainMenu场景UI设置完成");
    }

    [MenuItem("MoShou/场景UI设置/设置StageSelect场景")]
    public static void SetupStageSelectScene()
    {
        // 打开StageSelect场景
        string scenePath = "Assets/Scenes/StageSelect.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError($"场景不存在: {scenePath}");
            return;
        }

        EditorSceneManager.OpenScene(scenePath);

        // 查找或创建Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 应用背景
        SetupUIBackground(canvas.transform, "UI/StageSelect/UI_StageSelect_BG");

        // 设置章节标题
        SetupUIImage(canvas.transform, "ChapterBanner", "UI/StageSelect/UI_Chapter_Banner",
            new Vector2(0.5f, 0.9f), new Vector2(500, 80));

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[UISceneSetup] StageSelect场景UI设置完成");
    }

    [MenuItem("MoShou/场景UI设置/全部场景UI设置")]
    public static void SetupAllScenes()
    {
        // 首先确保资源已生成
        UIResourceGenerator.GenerateAllUIResources();

        SetupMainMenuScene();
        SetupStageSelectScene();

        EditorUtility.DisplayDialog("场景UI设置完成",
            "已为以下场景设置UI:\n" +
            "✓ MainMenu\n" +
            "✓ StageSelect\n\n" +
            "请检查各场景并调整布局",
            "确定");
    }

    static void SetupUIBackground(Transform parent, string spritePath)
    {
        // 查找或创建背景
        Transform bgTrans = parent.Find("Background");
        GameObject bgGO;

        if (bgTrans == null)
        {
            bgGO = new GameObject("Background");
            bgGO.transform.SetParent(parent, false);
            bgGO.transform.SetAsFirstSibling();
        }
        else
        {
            bgGO = bgTrans.gameObject;
        }

        // 设置RectTransform为全屏
        RectTransform rt = bgGO.GetComponent<RectTransform>();
        if (rt == null) rt = bgGO.AddComponent<RectTransform>();

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 设置Image
        Image img = bgGO.GetComponent<Image>();
        if (img == null) img = bgGO.AddComponent<Image>();

        Sprite sprite = Resources.Load<Sprite>($"Sprites/{spritePath}");
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            Debug.Log($"[UISceneSetup] 应用背景: {spritePath}");
        }
        else
        {
            Debug.LogWarning($"[UISceneSetup] 未找到背景资源: Sprites/{spritePath}");
            img.color = new Color(0.1f, 0.1f, 0.15f); // 使用深色背景作为回退
        }
    }

    static void SetupUIImage(Transform parent, string objName, string spritePath, Vector2 anchorPos, Vector2 size)
    {
        Transform trans = parent.Find(objName);
        GameObject go;

        if (trans == null)
        {
            go = new GameObject(objName);
            go.transform.SetParent(parent, false);
        }
        else
        {
            go = trans.gameObject;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();

        rt.anchorMin = anchorPos;
        rt.anchorMax = anchorPos;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();

        Sprite sprite = Resources.Load<Sprite>($"Sprites/{spritePath}");
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
            Debug.Log($"[UISceneSetup] 应用图片: {spritePath}");
        }
        else
        {
            Debug.LogWarning($"[UISceneSetup] 未找到资源: Sprites/{spritePath}");
        }
    }

    static void ApplyButtonSprites(string buttonName, string normalPath, string pressedPath)
    {
        // 在场景中查找按钮
        Button[] buttons = Object.FindObjectsOfType<Button>(true);
        Button targetButton = null;

        foreach (var btn in buttons)
        {
            if (btn.name.Contains(buttonName) || btn.gameObject.name == buttonName)
            {
                targetButton = btn;
                break;
            }
        }

        if (targetButton == null)
        {
            Debug.LogWarning($"[UISceneSetup] 未找到按钮: {buttonName}");
            return;
        }

        Image img = targetButton.GetComponent<Image>();
        if (img == null) return;

        // 加载Normal状态贴图
        Sprite normalSprite = Resources.Load<Sprite>($"Sprites/{normalPath}");
        if (normalSprite != null)
        {
            img.sprite = normalSprite;
            Debug.Log($"[UISceneSetup] 应用按钮贴图: {buttonName} <- {normalPath}");

            // 设置Button的颜色过渡
            var colors = targetButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            targetButton.colors = colors;

            // 如果有Pressed贴图，设置SpriteState
            if (!string.IsNullOrEmpty(pressedPath))
            {
                Sprite pressedSprite = Resources.Load<Sprite>($"Sprites/{pressedPath}");
                if (pressedSprite != null)
                {
                    var spriteState = targetButton.spriteState;
                    spriteState.pressedSprite = pressedSprite;
                    spriteState.highlightedSprite = normalSprite;
                    targetButton.spriteState = spriteState;
                    targetButton.transition = Selectable.Transition.SpriteSwap;
                }
            }
        }
    }
}
