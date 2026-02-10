using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 主菜单屏幕
    /// 游戏入口界面，提供开始游戏、设置等功能
    /// 对应效果图: UI_MainMenu.png
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        public static MainMenuScreen Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image logoImage;
        [SerializeField] private Text playerLevelText;
        [SerializeField] private Text playerGoldText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button characterButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Text versionText;

        [Header("Character Preview")]
        [SerializeField] private Image characterPreviewImage;
        [SerializeField] private RectTransform characterPreviewContainer;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float buttonAnimationDelay = 0.1f;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private bool isInitialized = false;
        private Button[] allButtons;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 添加CanvasScaler
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // 添加GraphicRaycaster
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        void Start()
        {
            InitializeUI();
            RefreshPlayerInfo();
            PlayEnterAnimation();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeUI()
        {
            if (isInitialized) return;

            // 如果没有预设UI引用，动态创建
            if (backgroundImage == null)
            {
                CreateDynamicUI();
            }

            // 绑定按钮事件
            BindButtonEvents();

            // 收集所有按钮用于动画
            allButtons = new Button[] { startGameButton, continueButton, characterButton, settingsButton, exitButton };

            isInitialized = true;
        }

        private void CreateDynamicUI()
        {
            // 背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform);
            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.12f, 0.18f, 1f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 尝试加载效果图作为背景
            Sprite mainMenuBg = Resources.Load<Sprite>("UI_Mockups/Screens/UI_MainMenu");
            if (mainMenuBg != null)
            {
                backgroundImage.sprite = mainMenuBg;
                backgroundImage.preserveAspect = true;
            }
            else
            {
                // 尝试其他路径
                mainMenuBg = Resources.Load<Sprite>("Sprites/UI/MainMenu/UI_MainMenu_BG");
                if (mainMenuBg != null)
                {
                    backgroundImage.sprite = mainMenuBg;
                }
            }

            // Logo容器
            GameObject logoContainer = CreatePanel("LogoContainer", new Vector2(0.5f, 0.85f), new Vector2(400, 150));

            // Logo图片
            logoImage = CreateImage(logoContainer.transform, "Logo", new Vector2(0.5f, 0.5f), new Vector2(350, 120));
            logoImage.color = UIStyleHelper.Colors.Gold;

            // Logo文字 (备用)
            Text logoText = CreateText(logoContainer.transform, "LogoText", "MOSHOU\nREBORN", 42, TextAnchor.MiddleCenter, UIStyleHelper.Colors.Gold);
            logoText.fontStyle = FontStyle.Bold;

            // 玩家信息面板 (左上角)
            GameObject playerInfoPanel = CreatePanel("PlayerInfo", new Vector2(0.15f, 0.92f), new Vector2(200, 80));
            Image playerInfoBg = playerInfoPanel.AddComponent<Image>();
            playerInfoBg.color = new Color(0.1f, 0.12f, 0.15f, 0.8f);

            playerLevelText = CreateText(playerInfoPanel.transform, "LevelText", "Lv. 1", 22, TextAnchor.MiddleLeft, Color.white);
            RectTransform levelRect = playerLevelText.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0, 0.5f);
            levelRect.anchorMax = new Vector2(0.5f, 1);
            levelRect.offsetMin = new Vector2(10, 0);
            levelRect.offsetMax = Vector2.zero;

            playerGoldText = CreateText(playerInfoPanel.transform, "GoldText", "0", 20, TextAnchor.MiddleLeft, UIStyleHelper.Colors.Gold);
            RectTransform goldRect = playerGoldText.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0, 0);
            goldRect.anchorMax = new Vector2(1, 0.5f);
            goldRect.offsetMin = new Vector2(10, 0);
            goldRect.offsetMax = Vector2.zero;

            // 角色预览 (左侧)
            characterPreviewContainer = CreatePanel("CharacterPreview", new Vector2(0.25f, 0.5f), new Vector2(300, 400)).GetComponent<RectTransform>();
            characterPreviewImage = CreateImage(characterPreviewContainer, "CharacterImage", new Vector2(0.5f, 0.5f), new Vector2(280, 380));
            characterPreviewImage.color = new Color(0.3f, 0.35f, 0.4f, 0.5f);

            // 按钮容器 (右侧)
            GameObject buttonContainer = CreatePanel("ButtonContainer", new Vector2(0.7f, 0.45f), new Vector2(350, 500));

            // 半透明玻璃背景
            Image containerBg = buttonContainer.AddComponent<Image>();
            containerBg.color = new Color(0.15f, 0.18f, 0.22f, 0.85f);

            VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 20;
            layout.padding = new RectOffset(30, 30, 40, 40);

            // 开始游戏按钮 (大按钮)
            startGameButton = CreateMenuButton(buttonContainer.transform, "StartGame", "START GAME", 80, UIStyleHelper.Colors.Gold);

            // 继续游戏按钮
            continueButton = CreateMenuButton(buttonContainer.transform, "Continue", "CONTINUE", 60, new Color(0.3f, 0.7f, 0.4f, 1f));

            // 角色按钮
            characterButton = CreateMenuButton(buttonContainer.transform, "Character", "CHARACTER", 55, new Color(0.4f, 0.5f, 0.7f, 1f));

            // 设置按钮
            settingsButton = CreateMenuButton(buttonContainer.transform, "Settings", "SETTINGS", 55, new Color(0.5f, 0.5f, 0.55f, 1f));

            // 退出按钮
            exitButton = CreateMenuButton(buttonContainer.transform, "Exit", "EXIT", 50, new Color(0.6f, 0.3f, 0.3f, 1f));

            // 版本号 (底部)
            versionText = CreateText(transform, "Version", "v1.0.0", 16, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.5f, 0.8f));
            RectTransform versionRect = versionText.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(0.5f, 0);
            versionRect.anchorMax = new Vector2(0.5f, 0);
            versionRect.pivot = new Vector2(0.5f, 0);
            versionRect.anchoredPosition = new Vector2(0, 30);
            versionRect.sizeDelta = new Vector2(200, 30);
        }

        private Button CreateMenuButton(Transform parent, string name, string text, float height, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent);

            // Layout Element
            LayoutElement layoutElement = btnObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;

            // 按钮背景
            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = bgColor;

            // 按钮组件
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // 按钮文字
            Text btnText = CreateText(btnObj.transform, "Text", text, (int)(height * 0.4f), TextAnchor.MiddleCenter, Color.white);
            btnText.fontStyle = FontStyle.Bold;

            return btn;
        }

        #region Helper Methods

        private GameObject CreatePanel(string name, Vector2 anchorPos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            return obj;
        }

        private Image CreateImage(Transform parent, string name, Vector2 anchor, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            Image img = obj.AddComponent<Image>();
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            return img;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        #endregion

        #region Button Events

        private void BindButtonEvents()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClick);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClick);

            if (characterButton != null)
                characterButton.onClick.AddListener(OnCharacterClick);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClick);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClick);
        }

        private void OnStartGameClick()
        {
            PlayButtonFeedback(startGameButton);
            Debug.Log("[MainMenuScreen] Start Game clicked");

            // 加载关卡选择场景
            PlayExitAnimation(() => {
                SceneManager.LoadScene("StageSelect");
            });
        }

        private void OnContinueClick()
        {
            PlayButtonFeedback(continueButton);
            Debug.Log("[MainMenuScreen] Continue clicked");

            // 继续上次进度
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveData())
            {
                int lastStage = SaveSystem.Instance.GetHighestUnlockedStage();
                // 直接进入战斗场景
                PlayExitAnimation(() => {
                    SceneManager.LoadScene("GameScene");
                });
            }
            else
            {
                // 没有存档，开始新游戏
                OnStartGameClick();
            }
        }

        private void OnCharacterClick()
        {
            PlayButtonFeedback(characterButton);
            Debug.Log("[MainMenuScreen] Character clicked");

            // 显示角色信息
            if (CharacterInfoScreen.Instance != null)
            {
                CharacterInfoScreen.Instance.Show();
            }
        }

        private void OnSettingsClick()
        {
            PlayButtonFeedback(settingsButton);
            Debug.Log("[MainMenuScreen] Settings clicked");

            // 显示设置面板
            if (SettingsPanel.Instance != null)
            {
                SettingsPanel.Instance.Show();
            }
        }

        private void OnExitClick()
        {
            PlayButtonFeedback(exitButton);
            Debug.Log("[MainMenuScreen] Exit clicked");

            // 确认退出
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
        }

        private void PlayButtonFeedback(Button button)
        {
            if (UIFeedbackSystem.Instance != null)
            {
                UIFeedbackSystem.Instance.PlayButtonClick(button.transform);
            }
        }

        #endregion

        #region Player Info

        public void RefreshPlayerInfo()
        {
            if (SaveSystem.Instance?.CurrentPlayerStats != null)
            {
                var stats = SaveSystem.Instance.CurrentPlayerStats;

                if (playerLevelText != null)
                {
                    playerLevelText.text = $"Lv. {stats.level}";
                }

                if (playerGoldText != null)
                {
                    playerGoldText.text = FormatNumber(stats.gold);
                }

                // 更新继续按钮状态
                UpdateContinueButtonState();
            }
            else
            {
                if (playerLevelText != null) playerLevelText.text = "Lv. 1";
                if (playerGoldText != null) playerGoldText.text = "0";

                // 没有存档，禁用继续按钮
                if (continueButton != null)
                {
                    continueButton.interactable = false;
                    continueButton.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
        }

        private void UpdateContinueButtonState()
        {
            if (continueButton == null) return;

            bool hasSaveData = SaveSystem.Instance != null && SaveSystem.Instance.HasSaveData();
            continueButton.interactable = hasSaveData;

            Image btnImage = continueButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = hasSaveData ?
                    new Color(0.3f, 0.7f, 0.4f, 1f) :
                    new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000)
                return (number / 1000000f).ToString("0.#") + "M";
            else if (number >= 1000)
                return (number / 1000f).ToString("0.#") + "K";
            return number.ToString();
        }

        #endregion

        #region Animations

        private void PlayEnterAnimation()
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = 0f;

            // 淡入背景
            if (UITween.Instance != null)
            {
                UITween.Instance.FadeTo(canvasGroup, 1f, fadeInDuration, null);

                // Logo动画
                if (logoImage != null)
                {
                    logoImage.transform.localScale = Vector3.one * 0.5f;
                    UITween.Instance.ScaleTo(logoImage.transform, Vector3.one, 0.5f, null);
                }

                // 按钮逐个滑入
                if (allButtons != null)
                {
                    for (int i = 0; i < allButtons.Length; i++)
                    {
                        if (allButtons[i] != null)
                        {
                            Button btn = allButtons[i];
                            RectTransform btnRect = btn.GetComponent<RectTransform>();

                            // 初始位置在右侧
                            Vector2 originalPos = btnRect.anchoredPosition;
                            btnRect.anchoredPosition = originalPos + new Vector2(300, 0);

                            // 延迟滑入
                            float delay = buttonAnimationDelay * i;
                            StartCoroutine(DelayedButtonAnimation(btnRect, originalPos, delay));
                        }
                    }
                }
            }
            else
            {
                canvasGroup.alpha = 1f;
            }
        }

        private System.Collections.IEnumerator DelayedButtonAnimation(RectTransform btnRect, Vector2 targetPos, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (UITween.Instance != null && btnRect != null)
            {
                UITween.Instance.MoveTo(btnRect, targetPos, 0.4f, null);
            }
        }

        private void PlayExitAnimation(Action onComplete)
        {
            if (canvasGroup != null && UITween.Instance != null)
            {
                UITween.Instance.FadeTo(canvasGroup, 0f, 0.3f, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        #endregion

        #region Visibility

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshPlayerInfo();
            PlayEnterAnimation();
        }

        public void Hide()
        {
            PlayExitAnimation(() => {
                gameObject.SetActive(false);
            });
        }

        #endregion
    }
}
