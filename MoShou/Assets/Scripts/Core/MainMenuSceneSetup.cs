using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using MoShou.UI;
using MoShou.Systems;

/// <summary>
/// 主菜单场景初始化 - 基于UI_MainMenu.png效果图设计
/// 卡通魔兽风格，竖屏 1080x1920
/// </summary>
public class MainMenuSceneSetup : MonoBehaviour
{
    private static bool isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
    }

    static void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            var temp = new GameObject("_MainMenuLoader");
            temp.AddComponent<MainMenuDelayedSetup>();
        }
    }

    private class MainMenuDelayedSetup : MonoBehaviour
    {
        void Start()
        {
            if (FindObjectOfType<MainMenuSceneSetup>() == null)
            {
                var go = new GameObject("MainMenuSceneSetup");
                go.AddComponent<MainMenuSceneSetup>();
            }
            Destroy(gameObject);
        }
    }

    void Awake()
    {
        if (isInitialized)
        {
            Destroy(gameObject);
            return;
        }
        isInitialized = true;
        SetupMainMenu();
    }

    void OnDestroy()
    {
        isInitialized = false;
    }

    void SetupMainMenu()
    {
        Debug.Log("[MainMenuSetup] 开始创建主菜单UI (效果图风格)...");

        // 确保有EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 销毁所有现有的低优先级Canvas，强制创建新UI
        // 使用DestroyImmediate确保旧UI立即被移除，避免新旧UI叠加
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in existingCanvases)
        {
            // 只销毁sortingOrder < 500的Canvas（保留系统级Canvas如ConfirmDialog等）
            if (canvas.sortingOrder < 500)
            {
                Debug.Log($"[MainMenuSetup] 立即销毁现有Canvas: {canvas.gameObject.name}");
                DestroyImmediate(canvas.gameObject);
            }
        }

        // 销毁旧的MainMenuManager（如果存在）
        var oldManager = FindObjectOfType<MoShou.Core.MainMenuManager>();
        if (oldManager != null)
        {
            Debug.Log("[MainMenuSetup] 销毁旧的MainMenuManager");
            Destroy(oldManager.gameObject);
        }

        // 创建新的效果图风格UI
        CreateMainMenuUI();
    }

    void CreateMainMenuUI()
    {
        // 尝试加载效果图资源
        Sprite mockupBg = Resources.Load<Sprite>("UI_Mockups/Screens/UI_MainMenu");
        Sprite menuBg = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_MainMenu_BG");
        Sprite logoSprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_MainMenu_Logo");

        // 面板框架精灵
        Sprite frameSprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_MainMenu_Frame");

        // 按钮精灵
        Sprite btnPlaySprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Play");
        Sprite btnContinueSprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Continue");
        Sprite btnRoleSprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Role");
        Sprite btnSettingsSprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Settings");
        Sprite btnQuitSprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Quit");

        // 按钮状态精灵（SpriteSwap过渡用）
        Sprite btnStartNormal = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Start_Normal");
        Sprite btnStartPressed = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Start_Pressed");
        Sprite btnContinueNormal = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Continue_Normal");
        Sprite btnContinueDisabled = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Continue_Disabled");
        Sprite btnSettingsNormal = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Settings_Normal");
        Sprite btnQuitNormal = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_Btn_Quit_Normal");

        Debug.Log($"[MainMenuSetup] 资源: Mockup={mockupBg != null}, BG={menuBg != null}, Logo={logoSprite != null}, Frame={frameSprite != null}");
        Debug.Log($"[MainMenuSetup] 按钮精灵: Play={btnPlaySprite != null}, Continue={btnContinueSprite != null}, Role={btnRoleSprite != null}, Settings={btnSettingsSprite != null}, Quit={btnQuitSprite != null}");

        // 创建Canvas
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // === 背景 ===
        CreateBackground(canvasGO.transform, mockupBg ?? menuBg);

        // === 顶部Logo区域 ===
        CreateLogoArea(canvasGO.transform, logoSprite);

        // === 角色预览区域（左侧） ===
        // CreateCharacterPreview(canvasGO.transform);

        // === 中央按钮面板 ===
        CreateButtonPanel(canvasGO.transform, frameSprite,
            btnPlaySprite ?? btnStartNormal, btnStartPressed,
            btnContinueSprite ?? btnContinueNormal, btnContinueDisabled,
            btnRoleSprite,
            btnSettingsSprite ?? btnSettingsNormal,
            btnQuitSprite ?? btnQuitNormal);

        // === 底部状态栏 ===
        CreateBottomStatusBar(canvasGO.transform);

        // === 版本号 === (已移除，效果图中无版本号)
        // CreateVersionText(canvasGO.transform);

        Debug.Log("[MainMenuSetup] 主菜单UI创建完成 (效果图风格)");
    }

    void CreateBackground(Transform parent, Sprite bgSprite)
    {
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(parent, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgGO.AddComponent<Image>();

        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.type = Image.Type.Simple;
            bgImage.preserveAspect = false;
            bgImage.color = Color.white;
        }
        else
        {
            // 创建卡通魔兽风格的渐变背景
            bgImage.color = new Color(0.08f, 0.12f, 0.18f, 1f); // 深蓝灰色

            // 顶部暖色渐变（模拟天空）
            CreateGradientOverlay(bgGO.transform, "TopGlow",
                new Vector2(0, 0.6f), Vector2.one,
                new Color(0.4f, 0.25f, 0.15f, 0.4f));

            // 中间光晕效果
            CreateGradientOverlay(bgGO.transform, "CenterGlow",
                new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.7f),
                new Color(0.6f, 0.4f, 0.2f, 0.3f));

            // 底部深色
            CreateGradientOverlay(bgGO.transform, "BottomDark",
                Vector2.zero, new Vector2(1, 0.3f),
                new Color(0.05f, 0.08f, 0.12f, 0.8f));
        }
    }

    void CreateGradientOverlay(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject overlay = new GameObject(name);
        overlay.transform.SetParent(parent, false);
        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image img = overlay.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    void CreateLogoArea(Transform parent, Sprite logoSprite)
    {
        // Logo容器
        GameObject logoContainer = new GameObject("LogoContainer");
        logoContainer.transform.SetParent(parent, false);
        RectTransform containerRect = logoContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.9f);
        containerRect.anchorMax = new Vector2(0.5f, 0.9f);
        containerRect.sizeDelta = new Vector2(900, 200);

        if (logoSprite == null)
        {
            // 仅在无Logo精灵时才创建装饰背景框
            GameObject logoBg = new GameObject("LogoBG");
            logoBg.transform.SetParent(logoContainer.transform, false);
            RectTransform bgRect = logoBg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = logoBg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.1f, 0.08f, 0.6f);

            // 金色边框
            CreateBorder(logoBg.transform, new Color(0.85f, 0.65f, 0.2f, 0.8f), 4);
        }

        // Logo图片或文字
        GameObject logoGO = new GameObject("Logo");
        logoGO.transform.SetParent(logoContainer.transform, false);
        RectTransform logoRect = logoGO.AddComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.05f, 0.1f);
        logoRect.anchorMax = new Vector2(0.95f, 0.9f);
        logoRect.offsetMin = Vector2.zero;
        logoRect.offsetMax = Vector2.zero;

        if (logoSprite != null)
        {
            Image logoImg = logoGO.AddComponent<Image>();
            logoImg.sprite = logoSprite;
            logoImg.preserveAspect = true;
        }
        else
        {
            // 创建文字Logo
            Text logoText = logoGO.AddComponent<Text>();
            logoText.text = "我是MT\n魔兽归来";
            logoText.fontSize = 56;
            logoText.fontStyle = FontStyle.Bold;
            logoText.alignment = TextAnchor.MiddleCenter;
            logoText.color = new Color(1f, 0.85f, 0.3f); // 金色
            logoText.font = GetDefaultFont();
            logoText.lineSpacing = 0.8f;

            // 描边
            Outline outline = logoGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.2f, 0f, 0.9f);
            outline.effectDistance = new Vector2(3, -3);

            // 阴影
            Shadow shadow = logoGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            shadow.effectDistance = new Vector2(4, -4);
        }
    }

    void CreateCharacterPreview(Transform parent)
    {
        // 角色预览容器（左侧，更小一些）
        GameObject previewContainer = new GameObject("CharacterPreview");
        previewContainer.transform.SetParent(parent, false);
        RectTransform containerRect = previewContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0.32f);
        containerRect.anchorMax = new Vector2(0.32f, 0.65f);
        containerRect.offsetMin = new Vector2(20, 0);
        containerRect.offsetMax = new Vector2(0, 0);

        // 角色占位图
        GameObject charImage = new GameObject("CharacterImage");
        charImage.transform.SetParent(previewContainer.transform, false);
        RectTransform charRect = charImage.AddComponent<RectTransform>();
        charRect.anchorMin = new Vector2(0.1f, 0.1f);
        charRect.anchorMax = new Vector2(0.9f, 0.9f);
        charRect.offsetMin = Vector2.zero;
        charRect.offsetMax = Vector2.zero;

        Image charImg = charImage.AddComponent<Image>();

        // 尝试加载角色图片
        Sprite charSprite = Resources.Load<Sprite>("Sprites/Characters/MT_Preview");
        if (charSprite != null)
        {
            charImg.sprite = charSprite;
            charImg.preserveAspect = true;
        }
        else
        {
            charImg.color = new Color(0.3f, 0.35f, 0.4f, 0.3f);
        }
    }

    void CreateButtonPanel(Transform parent, Sprite frameSprite = null,
        Sprite playSprite = null, Sprite playPressedSprite = null,
        Sprite continueSprite = null, Sprite continueDisabledSprite = null,
        Sprite roleSprite = null, Sprite settingsSprite = null, Sprite quitSprite = null)
    {
        // 按钮面板容器（居中）
        GameObject panelGO = new GameObject("ButtonPanel");
        panelGO.transform.SetParent(parent, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.38f);
        panelRect.anchorMax = new Vector2(0.5f, 0.38f);
        panelRect.anchoredPosition = new Vector2(0, 0);  // 居中
        panelRect.sizeDelta = new Vector2(600, 550); // 效果图按钮更宽

        // 按钮布局（直接在面板上，不需要背景层）
        VerticalLayoutGroup vlg = panelGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 10, 10);

        // 自动适配内容大小
        ContentSizeFitter csf = panelGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 简化：按钮直接挂在面板下，无需中间Buttons层
        GameObject buttonsGO = panelGO; // 指向面板本身

        // === 创建按钮 ===

        // 开始游戏 - 大按钮，橙金渐变
        CreateMenuButton(buttonsGO.transform, "PlayButton", "开始游戏", 90,
            new Color(1f, 0.6f, 0.15f), new Color(0.9f, 0.4f, 0.1f),
            () => SceneManager.LoadScene("StageSelect"),
            false, playSprite, playPressedSprite, null);

        // 继续游戏 - 绿色
        bool hasSaveData = SaveSystem.Instance != null && SaveSystem.Instance.HasSaveData();
        CreateMenuButton(buttonsGO.transform, "ContinueButton", "继续游戏", 80,
            hasSaveData ? new Color(0.3f, 0.7f, 0.4f) : new Color(0.4f, 0.4f, 0.4f),
            hasSaveData ? new Color(0.2f, 0.55f, 0.3f) : new Color(0.3f, 0.3f, 0.3f),
            () => {
                if (hasSaveData) SceneManager.LoadScene("GameScene");
            },
            !hasSaveData, continueSprite, null, continueDisabledSprite);

        // 角色 - 蓝色
        CreateMenuButton(buttonsGO.transform, "CharacterButton", "角色", 70,
            new Color(0.3f, 0.5f, 0.8f), new Color(0.2f, 0.35f, 0.6f),
            () => {
                Debug.Log("[MainMenu] 角色按钮点击");
                if (CharacterInfoScreen.Instance != null)
                {
                    CharacterInfoScreen.Instance.Show();
                }
                else
                {
                    Debug.Log("[MainMenu] CharacterInfoScreen未初始化，尝试创建...");
                    // 尝试从Resources加载或动态创建
                    var charScreenPrefab = Resources.Load<GameObject>("Prefabs/UI/Screens/CharacterInfoScreen");
                    if (charScreenPrefab != null)
                    {
                        Instantiate(charScreenPrefab);
                    }
                    else
                    {
                        // 创建一个简单的角色信息显示
                        ShowSimpleCharacterInfo();
                    }
                }
            },
            false, roleSprite, null, null);

        // 设置 - 灰色
        CreateMenuButton(buttonsGO.transform, "SettingsButton", "设置", 70,
            new Color(0.45f, 0.48f, 0.55f), new Color(0.35f, 0.38f, 0.42f),
            () => {
                Debug.Log("[MainMenu] 设置按钮点击");
                if (SettingsPanel.Instance != null)
                {
                    SettingsPanel.Instance.Show();
                }
                else
                {
                    Debug.Log("[MainMenu] SettingsPanel未初始化，尝试创建...");
                    // 尝试从Resources加载或动态创建
                    var settingsPrefab = Resources.Load<GameObject>("Prefabs/UI/SettingsPanel");
                    if (settingsPrefab != null)
                    {
                        var go = Instantiate(settingsPrefab);
                        var panel = go.GetComponent<SettingsPanel>();
                        if (panel != null) panel.Show();
                    }
                    else
                    {
                        // 创建一个简单的设置面板
                        ShowSimpleSettings();
                    }
                }
            },
            false, settingsSprite, null, null);

        // 退出 - 暗红色
        CreateMenuButton(buttonsGO.transform, "QuitButton", "退出", 65,
            new Color(0.6f, 0.35f, 0.3f), new Color(0.45f, 0.25f, 0.2f),
            () => {
                if (ConfirmDialog.Instance != null)
                {
                    ConfirmDialog.Instance.ShowExitConfirm(() => {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    });
                }
                else
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            },
            false, quitSprite, null, null);
    }

    void CreateMenuButton(Transform parent, string name, string text, float height,
        Color colorTop, Color colorBottom, UnityAction onClick, bool disabled = false,
        Sprite btnSprite = null, Sprite pressedSprite = null, Sprite disabledSprite = null)
    {
        bool useSprite = (btnSprite != null);

        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);

        LayoutElement layout = btnGO.AddComponent<LayoutElement>();
        layout.preferredWidth = 560;
        layout.preferredHeight = height;

        // 按钮背景
        Image btnImg = btnGO.AddComponent<Image>();

        if (useSprite)
        {
            // ★ 使用精灵图片作为按钮背景
            if (disabled && disabledSprite != null)
            {
                btnImg.sprite = disabledSprite;
            }
            else
            {
                btnImg.sprite = btnSprite;
            }
            btnImg.type = Image.Type.Simple;
            btnImg.color = disabled && disabledSprite == null
                ? new Color(0.5f, 0.5f, 0.5f, 0.8f)  // 无disabled精灵时变暗
                : Color.white;
            btnImg.SetNativeSize(); // 使用精灵原始尺寸
            // 更新LayoutElement为精灵原始尺寸
            layout.preferredWidth = btnImg.rectTransform.sizeDelta.x;
            layout.preferredHeight = btnImg.rectTransform.sizeDelta.y;
            // 精灵自带渐变/边框/高光效果，不需要创建子层
        }
        else
        {
            // ★ 降级：程序化纯色按钮（原逻辑）
            btnImg.color = disabled ? new Color(0.35f, 0.35f, 0.35f, 0.6f) : colorTop;

            // 渐变层
            if (!disabled)
            {
                GameObject gradient = new GameObject("Gradient");
                gradient.transform.SetParent(btnGO.transform, false);
                RectTransform gradRect = gradient.AddComponent<RectTransform>();
                gradRect.anchorMin = new Vector2(0, 0);
                gradRect.anchorMax = new Vector2(1, 0.5f);
                gradRect.offsetMin = Vector2.zero;
                gradRect.offsetMax = Vector2.zero;
                Image gradImg = gradient.AddComponent<Image>();
                gradImg.color = new Color(colorBottom.r, colorBottom.g, colorBottom.b, 0.7f);
                gradImg.raycastTarget = false;
            }

            // 边框
            CreateBorder(btnGO.transform, new Color(0.9f, 0.8f, 0.5f, disabled ? 0.3f : 0.6f), 2);

            // 高光
            if (!disabled)
            {
                GameObject highlight = new GameObject("Highlight");
                highlight.transform.SetParent(btnGO.transform, false);
                RectTransform hlRect = highlight.AddComponent<RectTransform>();
                hlRect.anchorMin = new Vector2(0.05f, 0.55f);
                hlRect.anchorMax = new Vector2(0.95f, 0.92f);
                hlRect.offsetMin = Vector2.zero;
                hlRect.offsetMax = Vector2.zero;
                Image hlImg = highlight.AddComponent<Image>();
                hlImg.color = new Color(1, 1, 1, 0.12f);
                hlImg.raycastTarget = false;
            }
        }

        // Button组件
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.interactable = !disabled;
        if (!disabled) btn.onClick.AddListener(onClick);

        if (useSprite && pressedSprite != null)
        {
            // 有按下状态精灵时，使用SpriteSwap过渡
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState spriteState = new SpriteState();
            spriteState.pressedSprite = pressedSprite;
            spriteState.highlightedSprite = btnSprite; // 高亮时保持原图
            if (disabledSprite != null)
                spriteState.disabledSprite = disabledSprite;
            btn.spriteState = spriteState;
        }
        else
        {
            // ColorTint过渡
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
            btn.colors = colors;
        }

        // 文字 — 仅在无sprite时创建（sprite按钮自带文字，不需要叠加Text）
        if (!useSprite)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 5);
            textRect.offsetMax = new Vector2(-15, -5);

            Text btnText = textGO.AddComponent<Text>();
            btnText.text = text;
            btnText.fontSize = (int)(height * 0.38f);
            btnText.fontStyle = FontStyle.Bold;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = disabled ? new Color(0.6f, 0.6f, 0.6f) : Color.white;
            btnText.font = GetDefaultFont();

            // 描边
            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.15f, 0.08f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // 阴影
            Shadow shadow = textGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
        }
    }

    void CreateBottomStatusBar(Transform parent)
    {
        // 底部状态栏
        GameObject statusBar = new GameObject("BottomStatusBar");
        statusBar.transform.SetParent(parent, false);
        RectTransform barRect = statusBar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 0);
        barRect.anchorMax = new Vector2(1, 0);
        barRect.pivot = new Vector2(0.5f, 0);
        barRect.anchoredPosition = Vector2.zero;
        barRect.sizeDelta = new Vector2(0, 80);

        // 背景（半透明，效果图中底部状态栏不遮挡背景）
        Image barBg = statusBar.AddComponent<Image>();
        barBg.color = new Color(0.06f, 0.08f, 0.12f, 0.5f);

        // 玩家信息
        CreatePlayerInfo(statusBar.transform);
    }

    void CreatePlayerInfo(Transform parent)
    {
        // 等级显示
        GameObject levelContainer = new GameObject("LevelInfo");
        levelContainer.transform.SetParent(parent, false);
        RectTransform levelRect = levelContainer.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 0.5f);
        levelRect.anchorMax = new Vector2(0, 0.5f);
        levelRect.pivot = new Vector2(0, 0.5f);
        levelRect.anchoredPosition = new Vector2(100, 0);
        levelRect.sizeDelta = new Vector2(150, 60);

        Text levelText = levelContainer.AddComponent<Text>();
        int level = 1;
        if (SaveSystem.Instance?.CurrentPlayerStats != null)
        {
            level = SaveSystem.Instance.CurrentPlayerStats.level;
        }
        levelText.text = $"Lv. {level}";
        levelText.fontSize = 28;
        levelText.fontStyle = FontStyle.Bold;
        levelText.alignment = TextAnchor.MiddleLeft;
        levelText.color = new Color(1f, 0.9f, 0.6f);
        levelText.font = GetDefaultFont();

        // 金币显示
        GameObject goldContainer = new GameObject("GoldInfo");
        goldContainer.transform.SetParent(parent, false);
        RectTransform goldRect = goldContainer.AddComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(1, 0.5f);
        goldRect.anchorMax = new Vector2(1, 0.5f);
        goldRect.pivot = new Vector2(1, 0.5f);
        goldRect.anchoredPosition = new Vector2(-100, 0);
        goldRect.sizeDelta = new Vector2(180, 60);

        int gold = 0;
        if (SaveSystem.Instance?.CurrentPlayerStats != null)
        {
            gold = SaveSystem.Instance.CurrentPlayerStats.gold;
        }

        // 尝试加载金币icon
        Sprite coinSprite = Resources.Load<Sprite>("Sprites/UI/Common/UI_Icon_Coin_Stack");
        if (coinSprite != null)
        {
            // 有icon sprite时：icon + 纯数字
            GameObject coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(goldContainer.transform, false);
            RectTransform coinRect = coinIcon.AddComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0, 0.5f);
            coinRect.anchorMax = new Vector2(0, 0.5f);
            coinRect.pivot = new Vector2(0, 0.5f);
            coinRect.anchoredPosition = new Vector2(0, 0);
            coinRect.sizeDelta = new Vector2(40, 40);
            Image coinImg = coinIcon.AddComponent<Image>();
            coinImg.sprite = coinSprite;
            coinImg.preserveAspect = true;
            coinImg.raycastTarget = false;

            GameObject goldTextGO = new GameObject("GoldText");
            goldTextGO.transform.SetParent(goldContainer.transform, false);
            RectTransform gtRect = goldTextGO.AddComponent<RectTransform>();
            gtRect.anchorMin = new Vector2(0, 0);
            gtRect.anchorMax = new Vector2(1, 1);
            gtRect.offsetMin = new Vector2(45, 0);
            gtRect.offsetMax = Vector2.zero;
            Text goldText = goldTextGO.AddComponent<Text>();
            goldText.text = $"{gold}";
            goldText.fontSize = 26;
            goldText.fontStyle = FontStyle.Bold;
            goldText.alignment = TextAnchor.MiddleRight;
            goldText.color = new Color(1f, 0.85f, 0.2f);
            goldText.font = GetDefaultFont();
        }
        else
        {
            // fallback：原emoji方案
            Text goldText = goldContainer.AddComponent<Text>();
            goldText.text = $"💰 {gold}";
            goldText.fontSize = 26;
            goldText.fontStyle = FontStyle.Bold;
            goldText.alignment = TextAnchor.MiddleRight;
            goldText.color = new Color(1f, 0.85f, 0.2f);
            goldText.font = GetDefaultFont();
        }
    }

    void CreateVersionText(Transform parent)
    {
        GameObject versionGO = new GameObject("Version");
        versionGO.transform.SetParent(parent, false);
        RectTransform versionRect = versionGO.AddComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(0.5f, 0);
        versionRect.anchorMax = new Vector2(0.5f, 0);
        versionRect.pivot = new Vector2(0.5f, 0);
        versionRect.anchoredPosition = new Vector2(0, 130);
        versionRect.sizeDelta = new Vector2(200, 30);

        Text versionText = versionGO.AddComponent<Text>();
        versionText.text = "v1.0.0";
        versionText.fontSize = 18;
        versionText.alignment = TextAnchor.MiddleCenter;
        versionText.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        versionText.font = GetDefaultFont();
    }

    void CreateBorder(Transform parent, Color color, float width)
    {
        GameObject border = new GameObject("Border");
        border.transform.SetParent(parent, false);
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-width, -width);
        borderRect.offsetMax = new Vector2(width, width);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = color;
        borderImg.raycastTarget = false;
        border.transform.SetAsFirstSibling();
    }

    Font GetDefaultFont()
    {
        string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
        foreach (string fontName in fontNames)
        {
            Font font = Resources.GetBuiltinResource<Font>(fontName);
            if (font != null) return font;
        }
        return Font.CreateDynamicFontFromOSFont("Arial", 14);
    }

    /// <summary>
    /// 创建简单的角色信息弹窗
    /// </summary>
    void ShowSimpleCharacterInfo()
    {
        // 创建弹窗Canvas
        GameObject popupCanvas = new GameObject("CharacterInfoPopup");
        Canvas canvas = popupCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = popupCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        popupCanvas.AddComponent<GraphicRaycaster>();

        // 背景遮罩
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(popupCanvas.transform, false);
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);

        Button overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.onClick.AddListener(() => Destroy(popupCanvas));

        // 面板
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(popupCanvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(800, 900);
        Image panelImg = panel.AddComponent<Image>();
        Sprite panelBgSprite = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_MainMenu_BG2");
        if (panelBgSprite != null)
        {
            panelImg.sprite = panelBgSprite;
            panelImg.type = Image.Type.Simple;
            panelImg.color = Color.white;
        }
        else
        {
            panelImg.color = new Color(0.1f, 0.12f, 0.18f, 0.95f);
        }

        // 标题
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(0, 80);

        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "角色信息";
        titleText.fontSize = 42;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.85f, 0.3f);
        titleText.font = GetDefaultFont();

        // 角色属性显示
        int level = 1, gold = 0, exp = 0;
        int hp = 100, atk = 10, def = 5;
        if (SaveSystem.Instance?.CurrentPlayerStats != null)
        {
            var stats = SaveSystem.Instance.CurrentPlayerStats;
            level = stats.level;
            gold = stats.gold;
            exp = stats.experience;
            hp = stats.GetTotalMaxHp();
            atk = stats.GetTotalAttack();
            def = stats.GetTotalDefense();
        }

        string[] labels = { $"等级: {level}", $"生命: {hp}", $"攻击: {atk}", $"防御: {def}", $"金币: {gold}", $"经验: {exp}" };
        for (int i = 0; i < labels.Length; i++)
        {
            GameObject statGO = new GameObject($"Stat_{i}");
            statGO.transform.SetParent(panel.transform, false);
            RectTransform statRect = statGO.AddComponent<RectTransform>();
            statRect.anchorMin = new Vector2(0.18f, 0);
            statRect.anchorMax = new Vector2(0.82f, 0);
            statRect.pivot = new Vector2(0.5f, 0);
            statRect.anchoredPosition = new Vector2(0, 650 - i * 90);
            statRect.sizeDelta = new Vector2(0, 70);

            Image statBg = statGO.AddComponent<Image>();
            statBg.color = new Color(0.15f, 0.18f, 0.25f, 0.8f);

            Text statText = new GameObject("Text").AddComponent<Text>();
            statText.transform.SetParent(statGO.transform, false);
            RectTransform textRect = statText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 0);
            textRect.offsetMax = new Vector2(-20, 0);
            statText.text = labels[i];
            statText.fontSize = 32;
            statText.alignment = TextAnchor.MiddleLeft;
            statText.color = Color.white;
            statText.font = GetDefaultFont();
        }

        // 关闭按钮
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(panel.transform, false);
        RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.5f, 0);
        closeBtnRect.anchorMax = new Vector2(0.5f, 0);
        closeBtnRect.pivot = new Vector2(0.5f, 0);
        closeBtnRect.anchoredPosition = new Vector2(0, 80);
        closeBtnRect.sizeDelta = new Vector2(300, 80);

        Image closeBtnImg = closeBtn.AddComponent<Image>();
        closeBtnImg.color = new Color(0.6f, 0.35f, 0.3f);
        Button closeButton = closeBtn.AddComponent<Button>();
        closeButton.targetGraphic = closeBtnImg;
        closeButton.onClick.AddListener(() => Destroy(popupCanvas));

        Text closeBtnText = new GameObject("Text").AddComponent<Text>();
        closeBtnText.transform.SetParent(closeBtn.transform, false);
        RectTransform cbtRect = closeBtnText.GetComponent<RectTransform>();
        cbtRect.anchorMin = Vector2.zero;
        cbtRect.anchorMax = Vector2.one;
        cbtRect.offsetMin = Vector2.zero;
        cbtRect.offsetMax = Vector2.zero;
        closeBtnText.text = "关闭";
        closeBtnText.fontSize = 32;
        closeBtnText.fontStyle = FontStyle.Bold;
        closeBtnText.alignment = TextAnchor.MiddleCenter;
        closeBtnText.color = Color.white;
        closeBtnText.font = GetDefaultFont();
    }

    /// <summary>
    /// 创建简单的设置弹窗
    /// </summary>
    void ShowSimpleSettings()
    {
        // 创建弹窗Canvas
        GameObject popupCanvas = new GameObject("SettingsPopup");
        Canvas canvas = popupCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = popupCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        popupCanvas.AddComponent<GraphicRaycaster>();

        // 背景遮罩
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(popupCanvas.transform, false);
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);

        Button overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.onClick.AddListener(() => Destroy(popupCanvas));

        // 面板
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(popupCanvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(700, 600);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.12f, 0.18f, 0.95f);

        // 边框
        CreateBorder(panel.transform, new Color(0.7f, 0.55f, 0.25f), 4);

        // 标题
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(0, 80);

        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "设置";
        titleText.fontSize = 42;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.85f, 0.3f);
        titleText.font = GetDefaultFont();

        // 音量设置提示
        GameObject volumeGO = new GameObject("VolumeInfo");
        volumeGO.transform.SetParent(panel.transform, false);
        RectTransform volumeRect = volumeGO.AddComponent<RectTransform>();
        volumeRect.anchorMin = new Vector2(0.1f, 0.5f);
        volumeRect.anchorMax = new Vector2(0.9f, 0.5f);
        volumeRect.sizeDelta = new Vector2(0, 200);

        Text volumeText = volumeGO.AddComponent<Text>();
        volumeText.text = "🔊 音量控制\n\n音乐: 100%\n音效: 100%\n\n（滑动调节功能开发中）";
        volumeText.fontSize = 28;
        volumeText.alignment = TextAnchor.MiddleCenter;
        volumeText.color = new Color(0.8f, 0.8f, 0.8f);
        volumeText.font = GetDefaultFont();

        // 关闭按钮
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(panel.transform, false);
        RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.5f, 0);
        closeBtnRect.anchorMax = new Vector2(0.5f, 0);
        closeBtnRect.pivot = new Vector2(0.5f, 0);
        closeBtnRect.anchoredPosition = new Vector2(0, 50);
        closeBtnRect.sizeDelta = new Vector2(300, 80);

        Image closeBtnImg = closeBtn.AddComponent<Image>();
        closeBtnImg.color = new Color(0.45f, 0.48f, 0.55f);
        Button closeButton = closeBtn.AddComponent<Button>();
        closeButton.targetGraphic = closeBtnImg;
        closeButton.onClick.AddListener(() => Destroy(popupCanvas));

        Text closeBtnText = new GameObject("Text").AddComponent<Text>();
        closeBtnText.transform.SetParent(closeBtn.transform, false);
        RectTransform cbtRect = closeBtnText.GetComponent<RectTransform>();
        cbtRect.anchorMin = Vector2.zero;
        cbtRect.anchorMax = Vector2.one;
        cbtRect.offsetMin = Vector2.zero;
        cbtRect.offsetMax = Vector2.zero;
        closeBtnText.text = "关闭";
        closeBtnText.fontSize = 32;
        closeBtnText.fontStyle = FontStyle.Bold;
        closeBtnText.alignment = TextAnchor.MiddleCenter;
        closeBtnText.color = Color.white;
        closeBtnText.font = GetDefaultFont();
    }
}
