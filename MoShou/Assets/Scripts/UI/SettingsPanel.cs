using UnityEngine;
using UnityEngine.UI;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// 设置面板 - 游戏设置UI
    /// 实现策划案UI-07设置界面功能
    /// 对应效果图: UI_Settings.png
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        public static SettingsPanel Instance { get; private set; }

        [Header("UI References")]
        public Image backgroundImage;
        public Slider bgmSlider;
        public Slider sfxSlider;
        public Toggle vibrateToggle;
        public Toggle muteToggle;
        public Button closeButton;
        public Button saveButton;
        public Text bgmValueText;
        public Text sfxValueText;
        public Text muteLabelText;

        [Header("Settings")]
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        public bool vibrate = true;
        public bool isMuted = false;

        private bool isVisible = false;
        private CanvasGroup canvasGroup;

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

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 尝试加载效果图背景
            LoadMockupBackground();
        }

        private void LoadMockupBackground()
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            if (backgroundImage != null)
            {
                Sprite bgSprite = Resources.Load<Sprite>("UI_Mockups/Screens/UI_Settings");
                if (bgSprite != null)
                {
                    backgroundImage.sprite = bgSprite;
                    backgroundImage.type = Image.Type.Simple;
                }
            }
        }

        void Start()
        {
            // 加载已保存的设置
            LoadSettings();

            // 初始化UI
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (saveButton != null)
                saveButton.onClick.AddListener(SaveSettings);

            if (bgmSlider != null)
            {
                bgmSlider.value = bgmVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
                UpdateBGMValueText();
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                UpdateSFXValueText();
            }

            if (vibrateToggle != null)
            {
                vibrateToggle.isOn = vibrate;
                vibrateToggle.onValueChanged.AddListener(OnVibrateChanged);
            }

            if (muteToggle != null)
            {
                muteToggle.isOn = isMuted;
                muteToggle.onValueChanged.AddListener(OnMuteChanged);
            }

            // 应用加载的设置到AudioManager
            ApplySettingsToAudioManager();

            // 默认隐藏
            gameObject.SetActive(false);
        }

        public void Show()
        {
            // 显示前重新加载设置
            LoadSettings();

            if (bgmSlider != null)
            {
                bgmSlider.value = bgmVolume;
                UpdateBGMValueText();
            }
            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVolume;
                UpdateSFXValueText();
            }
            if (vibrateToggle != null)
                vibrateToggle.isOn = vibrate;
            if (muteToggle != null)
                muteToggle.isOn = isMuted;

            gameObject.SetActive(true);
            isVisible = true;

            // 播放UI音效
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonClick);

            Debug.Log("[SettingsPanel] Shown");
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            isVisible = false;

            // 播放UI音效
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonClick);

            Debug.Log("[SettingsPanel] Hidden");
        }

        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        void OnBGMVolumeChanged(float value)
        {
            bgmVolume = value;

            // 实时应用到AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(value);
            }

            UpdateBGMValueText();
            Debug.Log($"[SettingsPanel] BGM Volume: {value:F2}");
        }

        void OnSFXVolumeChanged(float value)
        {
            sfxVolume = value;

            // 实时应用到AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }

            UpdateSFXValueText();

            // 播放测试音效让用户听到变化
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonClick);
            }

            Debug.Log($"[SettingsPanel] SFX Volume: {value:F2}");
        }

        void OnVibrateChanged(bool value)
        {
            vibrate = value;

            // 如果开启振动，触发一次振动反馈
            if (value)
            {
                TriggerVibration();
            }

            Debug.Log($"[SettingsPanel] Vibrate: {value}");
        }

        void OnMuteChanged(bool value)
        {
            isMuted = value;

            // 实时应用静音设置
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMute(value);
            }

            // 更新UI显示
            if (muteLabelText != null)
            {
                muteLabelText.text = value ? "已静音" : "未静音";
            }

            Debug.Log($"[SettingsPanel] Mute: {value}");
        }

        void UpdateBGMValueText()
        {
            if (bgmValueText != null)
            {
                bgmValueText.text = $"{(int)(bgmVolume * 100)}%";
            }
        }

        void UpdateSFXValueText()
        {
            if (sfxValueText != null)
            {
                sfxValueText.text = $"{(int)(sfxVolume * 100)}%";
            }
        }

        void SaveSettings()
        {
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetInt("Vibrate", vibrate ? 1 : 0);
            PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
            PlayerPrefs.Save();

            // 应用设置
            ApplySettingsToAudioManager();

            // 播放保存成功音效
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonClick);

            Debug.Log("[SettingsPanel] Settings saved");
            Hide();
        }

        void LoadSettings()
        {
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            vibrate = PlayerPrefs.GetInt("Vibrate", 1) == 1;
            isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;

            Debug.Log($"[SettingsPanel] Settings loaded - BGM:{bgmVolume:F2}, SFX:{sfxVolume:F2}, Vibrate:{vibrate}, Muted:{isMuted}");
        }

        /// <summary>
        /// 应用设置到AudioManager
        /// </summary>
        void ApplySettingsToAudioManager()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(bgmVolume);
                AudioManager.Instance.SetSFXVolume(sfxVolume);
                AudioManager.Instance.SetMute(isMuted);
                Debug.Log("[SettingsPanel] Settings applied to AudioManager");
            }
            else
            {
                Debug.LogWarning("[SettingsPanel] AudioManager not found, settings will be applied when available");
            }
        }

        /// <summary>
        /// 静态方法：检查是否静音
        /// </summary>
        public static bool IsMuted()
        {
            return PlayerPrefs.GetInt("Muted", 0) == 1;
        }

        /// <summary>
        /// 静态方法：切换静音状态
        /// </summary>
        public static void ToggleMute()
        {
            bool currentMuted = IsMuted();
            PlayerPrefs.SetInt("Muted", currentMuted ? 0 : 1);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMute(!currentMuted);
            }
        }

        /// <summary>
        /// 触发振动反馈
        /// </summary>
        void TriggerVibration()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (vibrate)
            {
                Handheld.Vibrate();
            }
#endif
        }

        /// <summary>
        /// 静态方法：检查振动是否启用
        /// </summary>
        public static bool IsVibrationEnabled()
        {
            return PlayerPrefs.GetInt("Vibrate", 1) == 1;
        }

        /// <summary>
        /// 静态方法：触发振动（如果启用）
        /// </summary>
        public static void VibrateIfEnabled()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (IsVibrationEnabled())
            {
                Handheld.Vibrate();
            }
#endif
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
