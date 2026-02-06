using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置面板 - 游戏设置UI
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle vibrateToggle;
    public Button closeButton;
    public Button saveButton;

    [Header("Settings")]
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public bool vibrate = true;

    private bool isVisible = false;

    void Start()
    {
        // 初始化UI
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveSettings);

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVolume;
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (vibrateToggle != null)
        {
            vibrateToggle.isOn = vibrate;
            vibrateToggle.onValueChanged.AddListener(OnVibrateChanged);
        }

        // 默认隐藏
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        isVisible = true;
        Debug.Log("[SettingsPanel] Shown");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        isVisible = false;
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
        // TODO: Apply to AudioManager
        Debug.Log($"[SettingsPanel] BGM Volume: {value}");
    }

    void OnSFXVolumeChanged(float value)
    {
        sfxVolume = value;
        // TODO: Apply to AudioManager
        Debug.Log($"[SettingsPanel] SFX Volume: {value}");
    }

    void OnVibrateChanged(bool value)
    {
        vibrate = value;
        Debug.Log($"[SettingsPanel] Vibrate: {value}");
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("Vibrate", vibrate ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("[SettingsPanel] Settings saved");
        Hide();
    }

    void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        vibrate = PlayerPrefs.GetInt("Vibrate", 1) == 1;
    }
}
