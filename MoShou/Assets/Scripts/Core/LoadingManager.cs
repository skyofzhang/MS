using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MoShou.Core
{
    /// <summary>
    /// Loading screen manager - handles scene transitions with loading UI
    /// Dynamically creates loading screen with sprite support and programmatic fallback
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance { get; private set; }

        [Header("UI References (auto-created if null)")]
        [SerializeField] private GameObject loadingCanvas;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Text tipText;
        [SerializeField] private Image loadingIcon;

        [Header("Loading Settings")]
        [SerializeField] private float minimumLoadTime = 0.5f;
        [SerializeField] private float iconRotationSpeed = 180f;

        [Header("Loading Tips")]
        [SerializeField] private string[] loadingTips = new string[]
        {
            "提示: 装备更好的装备来提升属性!",
            "提示: 合理使用技能 - 注意冷却时间。",
            "提示: 击败敌人可以获得金币和物品。",
            "提示: 去商店升级你的装备吧。",
            "提示: 完成关卡解锁新区域。",
            "提示: 暴击造成150%伤害!",
            "提示: 更高难度的关卡会给予更好的奖励。",
            "提示: 被动技能同样重要，别忘了升级!",
            "提示: 不同装备搭配会有意想不到的效果。"
        };

        private bool isLoading = false;

        // Dynamic UI references
        private Image backgroundImage;
        private Image progressBarBg;
        private Image progressBarFill;
        private RectTransform progressFillRect;
        private Text titleText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Create dynamic UI if no serialized canvas assigned
                if (loadingCanvas == null)
                {
                    CreateLoadingUI();
                }

                // Hide loading UI initially
                if (loadingCanvas != null)
                    loadingCanvas.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Dynamically create the loading screen UI
        /// Full-screen background + thin bottom progress bar + tip text
        /// </summary>
        private void CreateLoadingUI()
        {
            // ===== Canvas =====
            GameObject canvasGO = new GameObject("LoadingCanvas");
            canvasGO.transform.SetParent(transform, false);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Above everything

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            loadingCanvas = canvasGO;

            // ===== Full-screen Background =====
            CreateBackground(canvasGO.transform);

            // ===== Center Title Text =====
            CreateTitleArea(canvasGO.transform);

            // ===== Bottom Tip Text =====
            CreateTipArea(canvasGO.transform);

            // ===== Bottom Progress Bar (ultra-thin) =====
            CreateProgressBar(canvasGO.transform);

            // ===== Loading Icon (rotating) =====
            CreateLoadingIcon(canvasGO.transform);

            // ===== Progress Percentage Text =====
            CreateProgressText(canvasGO.transform);
        }

        /// <summary>
        /// Full-screen background image
        /// </summary>
        private void CreateBackground(Transform parent)
        {
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(parent, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            backgroundImage = bgGO.AddComponent<Image>();

            // Try to load background sprite
            Sprite bgSprite = Resources.Load<Sprite>("Sprites/UI/Loading/UI_Loading_BG");
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                backgroundImage.type = Image.Type.Simple;
                backgroundImage.preserveAspect = false;
                backgroundImage.color = Color.white;
            }
            else
            {
                // Programmatic fallback: dark fantasy gradient feel
                backgroundImage.color = new Color(0.05f, 0.03f, 0.08f, 1f);

                // Add a subtle overlay layer for depth
                GameObject overlayGO = new GameObject("GradientOverlay");
                overlayGO.transform.SetParent(bgGO.transform, false);
                RectTransform overlayRect = overlayGO.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = new Vector2(1f, 0.6f);
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;

                Image overlayImg = overlayGO.AddComponent<Image>();
                overlayImg.color = new Color(0.02f, 0.01f, 0.04f, 0.7f);
                overlayImg.raycastTarget = false;

                // Top atmosphere glow
                GameObject glowGO = new GameObject("TopGlow");
                glowGO.transform.SetParent(bgGO.transform, false);
                RectTransform glowRect = glowGO.AddComponent<RectTransform>();
                glowRect.anchorMin = new Vector2(0f, 0.5f);
                glowRect.anchorMax = Vector2.one;
                glowRect.offsetMin = Vector2.zero;
                glowRect.offsetMax = Vector2.zero;

                Image glowImg = glowGO.AddComponent<Image>();
                glowImg.color = new Color(0.1f, 0.05f, 0.15f, 0.4f);
                glowImg.raycastTarget = false;
            }
        }

        /// <summary>
        /// Game title / loading title area at upper center
        /// </summary>
        private void CreateTitleArea(Transform parent)
        {
            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(parent, false);
            RectTransform titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.55f);
            titleRect.anchorMax = new Vector2(0.5f, 0.55f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(800, 120);

            titleText = titleGO.AddComponent<Text>();
            titleText.text = "Loading...";
            titleText.fontSize = 42;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.9f, 0.85f, 0.7f, 0.9f);
            titleText.font = GetFont();

            // Outline
            Outline outline = titleGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Shadow
            Shadow shadow = titleGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(3f, -3f);
        }

        /// <summary>
        /// Tip text area near the bottom
        /// </summary>
        private void CreateTipArea(Transform parent)
        {
            GameObject tipGO = new GameObject("TipText");
            tipGO.transform.SetParent(parent, false);
            RectTransform tipRect = tipGO.AddComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0.5f, 0.06f);
            tipRect.anchorMax = new Vector2(0.5f, 0.06f);
            tipRect.pivot = new Vector2(0.5f, 0.5f);
            tipRect.sizeDelta = new Vector2(900, 60);

            tipText = tipGO.AddComponent<Text>();
            tipText.text = "";
            tipText.fontSize = 22;
            tipText.fontStyle = FontStyle.Italic;
            tipText.alignment = TextAnchor.MiddleCenter;
            tipText.color = new Color(0.7f, 0.7f, 0.65f, 0.8f);
            tipText.font = GetFont();

            // Subtle outline
            Outline outline = tipGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        /// <summary>
        /// Ultra-thin progress bar at the very bottom of the screen
        /// </summary>
        private void CreateProgressBar(Transform parent)
        {
            // Container for the progress bar
            GameObject barContainerGO = new GameObject("ProgressBarContainer");
            barContainerGO.transform.SetParent(parent, false);
            RectTransform containerRect = barContainerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 0f);
            containerRect.anchorMax = new Vector2(1f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, 20f);
            containerRect.sizeDelta = new Vector2(-80f, 4f); // 4px height, 40px margin each side

            // Background track
            GameObject bgGO = new GameObject("BarBackground");
            bgGO.transform.SetParent(barContainerGO.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            progressBarBg = bgGO.AddComponent<Image>();

            // Try to load bar background sprite
            Sprite barBgSprite = Resources.Load<Sprite>("Sprites/UI/Loading/UI_Loading_Bar_BG");
            if (barBgSprite != null)
            {
                progressBarBg.sprite = barBgSprite;
                progressBarBg.type = Image.Type.Sliced;
                progressBarBg.color = Color.white;
            }
            else
            {
                progressBarBg.color = new Color(0.2f, 0.18f, 0.25f, 0.6f);
            }

            // Fill area
            GameObject fillAreaGO = new GameObject("FillArea");
            fillAreaGO.transform.SetParent(barContainerGO.transform, false);
            RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill bar (will be scaled via Slider)
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            progressFillRect = fillGO.AddComponent<RectTransform>();
            progressFillRect.anchorMin = Vector2.zero;
            progressFillRect.anchorMax = Vector2.one;
            progressFillRect.offsetMin = Vector2.zero;
            progressFillRect.offsetMax = Vector2.zero;

            progressBarFill = fillGO.AddComponent<Image>();

            // Try to load fill sprite
            Sprite fillSprite = Resources.Load<Sprite>("Sprites/UI/Loading/UI_Loading_Bar_Fill");
            if (fillSprite != null)
            {
                progressBarFill.sprite = fillSprite;
                progressBarFill.type = Image.Type.Sliced;
                progressBarFill.color = Color.white;
            }
            else
            {
                // Golden gradient fill
                progressBarFill.color = new Color(0.9f, 0.75f, 0.3f, 0.9f);
            }

            // Slider component
            progressBar = barContainerGO.AddComponent<Slider>();
            progressBar.fillRect = progressFillRect;
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
            progressBar.interactable = false;
            progressBar.transition = Selectable.Transition.None;

            // Subtle glow effect above the progress bar
            GameObject glowGO = new GameObject("BarGlow");
            glowGO.transform.SetParent(barContainerGO.transform, false);
            RectTransform glowRect = glowGO.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0f, 1f);
            glowRect.anchorMax = new Vector2(1f, 1f);
            glowRect.pivot = new Vector2(0.5f, 0f);
            glowRect.sizeDelta = new Vector2(0f, 8f);

            Image glowImg = glowGO.AddComponent<Image>();
            glowImg.color = new Color(0.9f, 0.75f, 0.3f, 0.15f);
            glowImg.raycastTarget = false;
        }

        /// <summary>
        /// Rotating loading icon near center-bottom
        /// </summary>
        private void CreateLoadingIcon(Transform parent)
        {
            GameObject iconGO = new GameObject("LoadingIcon");
            iconGO.transform.SetParent(parent, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.42f);
            iconRect.anchorMax = new Vector2(0.5f, 0.42f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(60, 60);

            loadingIcon = iconGO.AddComponent<Image>();

            // Try to load loading icon sprite
            Sprite iconSprite = Resources.Load<Sprite>("Sprites/UI/Loading/UI_Loading_Icon");
            if (iconSprite != null)
            {
                loadingIcon.sprite = iconSprite;
                loadingIcon.color = Color.white;
                loadingIcon.preserveAspect = true;
            }
            else
            {
                // Programmatic fallback: simple circle-ish indicator
                loadingIcon.color = new Color(0.9f, 0.8f, 0.4f, 0.7f);

                // Inner dot
                GameObject dotGO = new GameObject("InnerDot");
                dotGO.transform.SetParent(iconGO.transform, false);
                RectTransform dotRect = dotGO.AddComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.25f, 0.25f);
                dotRect.anchorMax = new Vector2(0.75f, 0.75f);
                dotRect.offsetMin = Vector2.zero;
                dotRect.offsetMax = Vector2.zero;

                Image dotImg = dotGO.AddComponent<Image>();
                dotImg.color = new Color(0.05f, 0.03f, 0.08f, 0.9f);
                dotImg.raycastTarget = false;
            }
        }

        /// <summary>
        /// Progress percentage text below the loading icon
        /// </summary>
        private void CreateProgressText(Transform parent)
        {
            GameObject textGO = new GameObject("ProgressText");
            textGO.transform.SetParent(parent, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.35f);
            textRect.anchorMax = new Vector2(0.5f, 0.35f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(300, 50);

            progressText = textGO.AddComponent<Text>();
            progressText.text = "0%";
            progressText.fontSize = 28;
            progressText.fontStyle = FontStyle.Bold;
            progressText.alignment = TextAnchor.MiddleCenter;
            progressText.color = new Color(0.85f, 0.8f, 0.65f, 0.8f);
            progressText.font = GetFont();

            // Outline
            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.7f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        /// <summary>
        /// Load a scene with loading screen
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (!isLoading)
            {
                StartCoroutine(LoadSceneAsync(sceneName));
            }
        }

        /// <summary>
        /// Load scene asynchronously with progress display
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;

            // Show loading UI
            if (loadingCanvas != null)
                loadingCanvas.SetActive(true);

            // Update title
            if (titleText != null)
                titleText.text = "Loading...";

            // Show random tip
            ShowRandomTip();

            // Reset progress
            if (progressBar != null)
                progressBar.value = 0;
            if (progressText != null)
                progressText.text = "0%";

            float startTime = Time.realtimeSinceStartup;

            // Start async load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Update progress
            while (!asyncLoad.isDone)
            {
                // Progress goes from 0 to 0.9 during loading
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                // Update UI
                if (progressBar != null)
                    progressBar.value = progress;
                if (progressText != null)
                    progressText.text = $"{(int)(progress * 100)}%";

                // Rotate loading icon
                if (loadingIcon != null)
                    loadingIcon.transform.Rotate(0, 0, -iconRotationSpeed * Time.deltaTime);

                // Check if loading is complete
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum load time for smooth transition
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime < minimumLoadTime)
                    {
                        yield return new WaitForSecondsRealtime(minimumLoadTime - elapsedTime);
                    }

                    // Set progress to 100%
                    if (progressBar != null)
                        progressBar.value = 1f;
                    if (progressText != null)
                        progressText.text = "100%";

                    // Brief title change
                    if (titleText != null)
                        titleText.text = "Complete!";

                    yield return new WaitForSecondsRealtime(0.15f);

                    // Allow scene activation
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Hide loading UI
            if (loadingCanvas != null)
                loadingCanvas.SetActive(false);

            isLoading = false;
        }

        /// <summary>
        /// Show a random loading tip
        /// </summary>
        private void ShowRandomTip()
        {
            if (tipText == null || loadingTips == null || loadingTips.Length == 0)
                return;

            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }

        /// <summary>
        /// Load scene immediately (no loading screen)
        /// </summary>
        public void LoadSceneImmediate(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Check if currently loading
        /// </summary>
        public bool IsLoading => isLoading;

        /// <summary>
        /// Get a usable font
        /// </summary>
        private Font GetFont()
        {
            // Try UIStyleHelper first
            try
            {
                Font styleFont = MoShou.UI.UIStyleHelper.GetDefaultFont();
                if (styleFont != null) return styleFont;
            }
            catch { }

            // Fallback
            string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
            foreach (string fontName in fontNames)
            {
                Font font = Resources.GetBuiltinResource<Font>(fontName);
                if (font != null) return font;
            }
            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }
    }
}
