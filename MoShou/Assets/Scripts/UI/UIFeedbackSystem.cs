using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoShou.Systems;

namespace MoShou.UI
{
    /// <summary>
    /// UI反馈系统
    /// 实现策划案第8章定义的所有UI反馈效果
    /// </summary>
    public class UIFeedbackSystem : MonoBehaviour
    {
        private static UIFeedbackSystem _instance;
        public static UIFeedbackSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIFeedbackSystem");
                    _instance = go.AddComponent<UIFeedbackSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #region 时间常量 (策划案8.1-8.5)

        // 按钮反馈 (8.1)
        private const float BUTTON_PRESS_DURATION = 0.1f;     // 100ms
        private const float BUTTON_RELEASE_DURATION = 0.1f;   // 100ms

        // 血条反馈 (8.2)
        private const float HEALTHBAR_CHANGE_DURATION = 0.3f; // 0.3秒缓动
        private const float LOW_HP_FLASH_FREQUENCY = 1f;      // 1Hz闪烁
        private const float LOW_HP_THRESHOLD = 0.3f;          // 30%血量阈值

        // 金币反馈 (8.3)
        private const float GOLD_NUMBER_DURATION = 0.5f;      // 0.5秒缓动
        private const float GOLD_ICON_FLASH_DURATION = 0.3f;  // 0.3秒闪光

        // 经验反馈 (8.4)
        private const float EXP_BAR_DURATION = 1.5f;          // 1.5秒缓动
        private const float EXP_NUMBER_DURATION = 1.0f;       // 1秒缓动
        private const float LEVELUP_EFFECT_DURATION = 2.0f;   // 2秒升级特效

        // 物品反馈 (8.5)
        private const float ITEM_FLY_DURATION = 1.0f;         // 1秒飞入
        private const float SLOT_FLASH_DURATION = 0.5f;       // 0.5秒闪光

        // 技能反馈 (8.6)
        // 技能动画由PlayerController处理

        #endregion

        #region 血条反馈 (策划案8.2)

        private Coroutine _healthFlashCoroutine;
        private bool _isLowHealth;

        /// <summary>
        /// 血条变化动画
        /// </summary>
        public void AnimateHealthBar(Image healthFill, float targetRatio, Text percentText = null)
        {
            if (healthFill == null) return;

            // 填充动画 (0.3秒缓动)
            UITween.Instance.FillTo(healthFill, targetRatio, HEALTHBAR_CHANGE_DURATION);

            // 百分比文字动画
            if (percentText != null)
            {
                int startPercent = Mathf.RoundToInt(healthFill.fillAmount * 100);
                int endPercent = Mathf.RoundToInt(targetRatio * 100);
                UITween.Instance.NumberTo(percentText, startPercent, endPercent,
                    HEALTHBAR_CHANGE_DURATION, "{0}%");
            }

            // 颜色变化
            UpdateHealthBarColor(healthFill, targetRatio);

            // 低血量闪烁检查
            if (targetRatio < LOW_HP_THRESHOLD && !_isLowHealth)
            {
                _isLowHealth = true;
                StartLowHealthFlash(healthFill);
            }
            else if (targetRatio >= LOW_HP_THRESHOLD && _isLowHealth)
            {
                _isLowHealth = false;
                StopLowHealthFlash(healthFill);
            }
        }

        private void UpdateHealthBarColor(Image healthFill, float ratio)
        {
            Color targetColor;
            if (ratio > 0.5f)
            {
                // 50%以上：绿色
                targetColor = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2);
            }
            else if (ratio > 0.3f)
            {
                // 30%-50%：黄色
                targetColor = Color.Lerp(Color.red, Color.yellow, (ratio - 0.3f) / 0.2f);
            }
            else
            {
                // 30%以下：红色
                targetColor = Color.red;
            }

            UITween.Instance.ColorTo(healthFill, targetColor, HEALTHBAR_CHANGE_DURATION);
        }

        private void StartLowHealthFlash(Image healthFill)
        {
            if (_healthFlashCoroutine != null)
                StopCoroutine(_healthFlashCoroutine);

            _healthFlashCoroutine = StartCoroutine(LowHealthFlashCoroutine(healthFill));
        }

        private void StopLowHealthFlash(Image healthFill)
        {
            if (_healthFlashCoroutine != null)
            {
                StopCoroutine(_healthFlashCoroutine);
                _healthFlashCoroutine = null;
            }

            // 恢复正常颜色
            if (healthFill != null)
                healthFill.color = Color.red;
        }

        private IEnumerator LowHealthFlashCoroutine(Image healthFill)
        {
            Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);
            Color normalColor = Color.red;

            while (_isLowHealth && healthFill != null)
            {
                // 闪亮
                float t = 0f;
                while (t < 0.5f / LOW_HP_FLASH_FREQUENCY)
                {
                    if (healthFill == null) yield break;
                    t += Time.deltaTime;
                    healthFill.color = Color.Lerp(normalColor, flashColor, t * LOW_HP_FLASH_FREQUENCY * 2);
                    yield return null;
                }

                // 恢复
                t = 0f;
                while (t < 0.5f / LOW_HP_FLASH_FREQUENCY)
                {
                    if (healthFill == null) yield break;
                    t += Time.deltaTime;
                    healthFill.color = Color.Lerp(flashColor, normalColor, t * LOW_HP_FLASH_FREQUENCY * 2);
                    yield return null;
                }
            }
        }

        #endregion

        #region 金币反馈 (策划案8.3)

        /// <summary>
        /// 金币变化动画
        /// </summary>
        public void AnimateGoldChange(Text goldText, Image goldIcon, int oldValue, int newValue)
        {
            if (goldText != null)
            {
                // 数字滚动 (0.5秒)
                UITween.Instance.NumberTo(goldText, oldValue, newValue, GOLD_NUMBER_DURATION);
            }

            if (goldIcon != null)
            {
                // 图标闪光 (0.3秒)
                StartCoroutine(GoldIconFlashCoroutine(goldIcon));
            }
        }

        private IEnumerator GoldIconFlashCoroutine(Image goldIcon)
        {
            if (goldIcon == null) yield break;

            Color originalColor = goldIcon.color;
            Color flashColor = new Color(1f, 1f, 0.5f, 1f);

            // 闪亮
            float elapsed = 0f;
            float halfDuration = GOLD_ICON_FLASH_DURATION / 2f;

            while (elapsed < halfDuration)
            {
                if (goldIcon == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                goldIcon.color = Color.Lerp(originalColor, flashColor, t);
                yield return null;
            }

            // 恢复
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (goldIcon == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                goldIcon.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }

            if (goldIcon != null)
                goldIcon.color = originalColor;
        }

        #endregion

        #region 经验反馈 (策划案8.4)

        /// <summary>
        /// 经验条变化动画
        /// </summary>
        public void AnimateExpBar(Image expFill, float targetRatio, Text expText = null,
            int oldExp = 0, int newExp = 0)
        {
            if (expFill != null)
            {
                // 填充动画 (1.5秒)
                UITween.Instance.FillTo(expFill, targetRatio, EXP_BAR_DURATION);
            }

            if (expText != null && newExp > oldExp)
            {
                // 数字滚动 (1秒)
                UITween.Instance.NumberTo(expText, oldExp, newExp, EXP_NUMBER_DURATION);
            }
        }

        /// <summary>
        /// 升级特效
        /// </summary>
        public void PlayLevelUpEffect(Text levelText, Transform effectAnchor = null)
        {
            if (levelText != null)
            {
                // 等级数字闪光放大
                StartCoroutine(LevelUpTextEffectCoroutine(levelText));
            }

            // 播放VFX特效 (需要VFXManager)
            if (effectAnchor != null)
            {
                // 通过VFXManager播放升级特效
                var vfxManager = FindObjectOfType<Effects.VFXManager>();
                if (vfxManager != null)
                {
                    vfxManager.PlayVFX("VFX_LevelUp", effectAnchor.position);
                }
            }

            // 播放升级音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_LevelUp");
            }
        }

        private IEnumerator LevelUpTextEffectCoroutine(Text levelText)
        {
            if (levelText == null) yield break;

            Color originalColor = levelText.color;
            Color flashColor = new Color(1f, 0.9f, 0.3f, 1f); // 金色

            Vector3 originalScale = levelText.transform.localScale;
            Vector3 maxScale = originalScale * 1.5f;

            float elapsed = 0f;

            // 放大并变金色
            while (elapsed < LEVELUP_EFFECT_DURATION * 0.3f)
            {
                if (levelText == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / (LEVELUP_EFFECT_DURATION * 0.3f);
                levelText.color = Color.Lerp(originalColor, flashColor, t);
                levelText.transform.localScale = Vector3.Lerp(originalScale, maxScale, t);
                yield return null;
            }

            // 保持
            yield return new WaitForSeconds(LEVELUP_EFFECT_DURATION * 0.4f);

            // 恢复
            elapsed = 0f;
            while (elapsed < LEVELUP_EFFECT_DURATION * 0.3f)
            {
                if (levelText == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / (LEVELUP_EFFECT_DURATION * 0.3f);
                levelText.color = Color.Lerp(flashColor, originalColor, t);
                levelText.transform.localScale = Vector3.Lerp(maxScale, originalScale, t);
                yield return null;
            }

            if (levelText != null)
            {
                levelText.color = originalColor;
                levelText.transform.localScale = originalScale;
            }
        }

        #endregion

        #region 物品获得反馈 (策划案8.5)

        /// <summary>
        /// 物品飞入背包动画
        /// </summary>
        public void PlayItemFlyToInventory(RectTransform itemIcon, RectTransform targetSlot,
            System.Action onComplete = null)
        {
            if (itemIcon == null || targetSlot == null)
            {
                onComplete?.Invoke();
                return;
            }

            UITween.Instance.MoveTo(itemIcon, targetSlot.anchoredPosition, ITEM_FLY_DURATION, () =>
            {
                // 播放槽位闪光
                Image slotImage = targetSlot.GetComponent<Image>();
                if (slotImage != null)
                {
                    PlaySlotFlash(slotImage);
                }

                onComplete?.Invoke();
            });

            // 同时缩小图标
            UITween.Instance.ScaleTo(itemIcon, Vector3.one * 0.5f, ITEM_FLY_DURATION);
        }

        /// <summary>
        /// 槽位闪光效果
        /// </summary>
        public void PlaySlotFlash(Image slotImage)
        {
            if (slotImage == null) return;

            StartCoroutine(SlotFlashCoroutine(slotImage));
        }

        private IEnumerator SlotFlashCoroutine(Image slotImage)
        {
            if (slotImage == null) yield break;

            Color originalColor = slotImage.color;
            Color flashColor = new Color(1f, 1f, 0.5f, originalColor.a);

            // 闪亮
            float elapsed = 0f;
            float halfDuration = SLOT_FLASH_DURATION / 2f;

            while (elapsed < halfDuration)
            {
                if (slotImage == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                slotImage.color = Color.Lerp(originalColor, flashColor, t);
                yield return null;
            }

            // 恢复
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (slotImage == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                slotImage.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }

            if (slotImage != null)
                slotImage.color = originalColor;
        }

        #endregion

        #region 按钮反馈 (策划案8.1)

        /// <summary>
        /// 按钮点击反馈
        /// </summary>
        public void PlayButtonClick(Transform buttonTransform, System.Action onComplete = null)
        {
            if (buttonTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            UITween.Instance.ButtonClick(buttonTransform, onComplete);

            // 播放点击音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_ButtonClick");
            }
        }

        #endregion

        #region 弹窗动画

        /// <summary>
        /// 弹窗弹性出现
        /// </summary>
        public void ShowPanelWithBounce(Transform panelTransform, System.Action onComplete = null)
        {
            if (panelTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            panelTransform.localScale = Vector3.zero;
            UITween.Instance.ScalePunch(panelTransform, Vector3.one * 0.2f, 0.4f, onComplete);

            // 同时调用ScaleTo确保最终缩放正确
            UITween.Instance.ScaleTo(panelTransform, Vector3.one, 0.3f);

            // 播放面板打开音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_UI_Panel_Open");
            }
        }

        /// <summary>
        /// 弹窗缩小消失
        /// </summary>
        public void HidePanelWithShrink(Transform panelTransform, System.Action onComplete = null)
        {
            if (panelTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            UITween.Instance.ScaleTo(panelTransform, Vector3.zero, 0.2f, onComplete);

            // 播放面板关闭音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_UI_Panel_Close");
            }
        }

        #endregion

        #region 伤害数字反馈

        /// <summary>
        /// 伤害数字弹出
        /// </summary>
        public void ShowDamagePopup(RectTransform damageText, int damage, bool isCritical = false)
        {
            if (damageText == null) return;

            Text text = damageText.GetComponent<Text>();
            if (text != null)
            {
                text.text = damage.ToString();

                if (isCritical)
                {
                    text.color = new Color(1f, 0.8f, 0f, 1f); // 金色暴击
                    damageText.localScale = Vector3.one * 1.5f;
                }
                else
                {
                    text.color = Color.white;
                    damageText.localScale = Vector3.one;
                }
            }

            // 弹出动画
            UITween.Instance.PopUp(damageText, 50f, 0.8f, () =>
            {
                // 淡出
                CanvasGroup cg = damageText.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = damageText.gameObject.AddComponent<CanvasGroup>();

                UITween.Instance.FadeTo(cg, 0f, 0.3f, () =>
                {
                    // 销毁或回收
                    Destroy(damageText.gameObject);
                });
            });
        }

        #endregion

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
