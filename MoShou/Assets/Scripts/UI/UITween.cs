using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MoShou.UI
{
    /// <summary>
    /// 轻量级Tween动画工具
    /// 不依赖DOTween，实现策划案要求的UI动画效果
    /// </summary>
    public class UITween : MonoBehaviour
    {
        private static UITween _instance;
        public static UITween Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("UITween");
                    _instance = go.AddComponent<UITween>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<int, Coroutine> _runningTweens = new Dictionary<int, Coroutine>();
        private int _tweenIdCounter = 0;

        #region Scale Tweens

        /// <summary>
        /// 缩放动画 - 用于按钮点击效果
        /// </summary>
        public int ScaleTo(Transform target, Vector3 endScale, float duration, Action onComplete = null)
        {
            return StartTween(ScaleCoroutine(target, endScale, duration, EaseType.EaseOutBack, onComplete));
        }

        /// <summary>
        /// 弹性缩放动画 - 用于弹窗出现效果
        /// </summary>
        public int ScalePunch(Transform target, Vector3 punchScale, float duration, Action onComplete = null)
        {
            return StartTween(ScalePunchCoroutine(target, punchScale, duration, onComplete));
        }

        /// <summary>
        /// 按钮点击缩放效果 (策划案8.1要求的100ms按下态)
        /// </summary>
        public int ButtonClick(Transform target, Action onComplete = null)
        {
            Vector3 originalScale = target.localScale;
            return StartTween(ButtonClickCoroutine(target, originalScale, onComplete));
        }

        private IEnumerator ScaleCoroutine(Transform target, Vector3 endScale, float duration, EaseType ease, Action onComplete)
        {
            if (target == null) yield break;

            Vector3 startScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = ApplyEase(t, ease);
                target.localScale = Vector3.LerpUnclamped(startScale, endScale, t);
                yield return null;
            }

            if (target != null)
                target.localScale = endScale;
            onComplete?.Invoke();
        }

        private IEnumerator ScalePunchCoroutine(Transform target, Vector3 punchScale, float duration, Action onComplete)
        {
            if (target == null) yield break;

            Vector3 originalScale = target.localScale;
            Vector3 maxScale = originalScale + punchScale;

            // 放大阶段
            float halfDuration = duration * 0.3f;
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = ApplyEase(Mathf.Clamp01(elapsed / halfDuration), EaseType.EaseOutQuad);
                target.localScale = Vector3.Lerp(originalScale, maxScale, t);
                yield return null;
            }

            // 弹回阶段
            elapsed = 0f;
            float restDuration = duration * 0.7f;
            while (elapsed < restDuration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = ApplyEase(Mathf.Clamp01(elapsed / restDuration), EaseType.EaseOutElastic);
                target.localScale = Vector3.Lerp(maxScale, originalScale, t);
                yield return null;
            }

            if (target != null)
                target.localScale = originalScale;
            onComplete?.Invoke();
        }

        private IEnumerator ButtonClickCoroutine(Transform target, Vector3 originalScale, Action onComplete)
        {
            if (target == null) yield break;

            // 按下态 - 100ms (策划案8.1)
            Vector3 pressedScale = originalScale * 0.9f;
            float pressTime = 0.1f;
            float elapsed = 0f;

            while (elapsed < pressTime)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / pressTime);
                target.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }

            // 释放态 - 100ms (策划案8.1)
            elapsed = 0f;
            while (elapsed < pressTime)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = ApplyEase(Mathf.Clamp01(elapsed / pressTime), EaseType.EaseOutBack);
                target.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }

            if (target != null)
                target.localScale = originalScale;
            onComplete?.Invoke();
        }

        #endregion

        #region Alpha/Fade Tweens

        /// <summary>
        /// 淡入淡出动画
        /// </summary>
        public int FadeTo(CanvasGroup target, float endAlpha, float duration, Action onComplete = null)
        {
            return StartTween(FadeCoroutine(target, endAlpha, duration, onComplete));
        }

        /// <summary>
        /// Image淡入淡出
        /// </summary>
        public int FadeTo(Image target, float endAlpha, float duration, Action onComplete = null)
        {
            return StartTween(FadeImageCoroutine(target, endAlpha, duration, onComplete));
        }

        private IEnumerator FadeCoroutine(CanvasGroup target, float endAlpha, float duration, Action onComplete)
        {
            if (target == null) yield break;

            float startAlpha = target.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }

            if (target != null)
                target.alpha = endAlpha;
            onComplete?.Invoke();
        }

        private IEnumerator FadeImageCoroutine(Image target, float endAlpha, float duration, Action onComplete)
        {
            if (target == null) yield break;

            float startAlpha = target.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Color c = target.color;
                c.a = Mathf.Lerp(startAlpha, endAlpha, t);
                target.color = c;
                yield return null;
            }

            if (target != null)
            {
                Color c = target.color;
                c.a = endAlpha;
                target.color = c;
            }
            onComplete?.Invoke();
        }

        #endregion

        #region Color Tweens

        /// <summary>
        /// 颜色动画
        /// </summary>
        public int ColorTo(Image target, Color endColor, float duration, Action onComplete = null)
        {
            return StartTween(ColorCoroutine(target, endColor, duration, onComplete));
        }

        /// <summary>
        /// 闪烁效果 - 用于血条低血量警告
        /// </summary>
        public int Flash(Image target, Color flashColor, float frequency, float duration, Action onComplete = null)
        {
            return StartTween(FlashCoroutine(target, flashColor, frequency, duration, onComplete));
        }

        private IEnumerator ColorCoroutine(Image target, Color endColor, float duration, Action onComplete)
        {
            if (target == null) yield break;

            Color startColor = target.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            if (target != null)
                target.color = endColor;
            onComplete?.Invoke();
        }

        private IEnumerator FlashCoroutine(Image target, Color flashColor, float frequency, float duration, Action onComplete)
        {
            if (target == null) yield break;

            Color originalColor = target.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;

                // 使用正弦波实现闪烁
                float t = Mathf.Sin(elapsed * frequency * Mathf.PI * 2) * 0.5f + 0.5f;
                target.color = Color.Lerp(originalColor, flashColor, t);
                yield return null;
            }

            if (target != null)
                target.color = originalColor;
            onComplete?.Invoke();
        }

        #endregion

        #region Fill Amount Tweens

        /// <summary>
        /// 填充量动画 - 用于血条/经验条 (策划案8.2/8.4要求)
        /// </summary>
        public int FillTo(Image target, float endFill, float duration, Action onComplete = null)
        {
            return StartTween(FillCoroutine(target, endFill, duration, onComplete));
        }

        private IEnumerator FillCoroutine(Image target, float endFill, float duration, Action onComplete)
        {
            if (target == null) yield break;

            float startFill = target.fillAmount;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), EaseType.EaseOutQuad);
                target.fillAmount = Mathf.Lerp(startFill, endFill, t);
                yield return null;
            }

            if (target != null)
                target.fillAmount = endFill;
            onComplete?.Invoke();
        }

        #endregion

        #region Number/Text Tweens

        /// <summary>
        /// 数字滚动动画 - 用于金币/经验数字 (策划案8.3/8.4要求)
        /// </summary>
        public int NumberTo(Text target, int startValue, int endValue, float duration, string format = "{0}", Action onComplete = null)
        {
            return StartTween(NumberCoroutine(target, startValue, endValue, duration, format, onComplete));
        }

        private IEnumerator NumberCoroutine(Text target, int startValue, int endValue, float duration, string format, Action onComplete)
        {
            if (target == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), EaseType.EaseOutQuad);
                int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
                target.text = string.Format(format, currentValue);
                yield return null;
            }

            if (target != null)
                target.text = string.Format(format, endValue);
            onComplete?.Invoke();
        }

        #endregion

        #region Position Tweens

        /// <summary>
        /// 位置动画 - 用于物品飞入效果 (策划案8.5)
        /// </summary>
        public int MoveTo(RectTransform target, Vector2 endPosition, float duration, Action onComplete = null)
        {
            return StartTween(MoveCoroutine(target, endPosition, duration, onComplete));
        }

        /// <summary>
        /// 弹跳位置动画 - 用于伤害数字弹出
        /// </summary>
        public int PopUp(RectTransform target, float height, float duration, Action onComplete = null)
        {
            return StartTween(PopUpCoroutine(target, height, duration, onComplete));
        }

        private IEnumerator MoveCoroutine(RectTransform target, Vector2 endPosition, float duration, Action onComplete)
        {
            if (target == null) yield break;

            Vector2 startPosition = target.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = ApplyEase(Mathf.Clamp01(elapsed / duration), EaseType.EaseOutQuad);
                target.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
                yield return null;
            }

            if (target != null)
                target.anchoredPosition = endPosition;
            onComplete?.Invoke();
        }

        private IEnumerator PopUpCoroutine(RectTransform target, float height, float duration, Action onComplete)
        {
            if (target == null) yield break;

            Vector2 startPosition = target.anchoredPosition;
            Vector2 peakPosition = startPosition + Vector2.up * height;
            float elapsed = 0f;

            // 上升阶段 (使用抛物线)
            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // 抛物线：y = -4(t-0.5)^2 + 1，在t=0.5时达到最大值1
                float heightT = 1f - 4f * (t - 0.5f) * (t - 0.5f);
                target.anchoredPosition = startPosition + Vector2.up * (height * heightT);
                yield return null;
            }

            onComplete?.Invoke();
        }

        #endregion

        #region Easing Functions

        private enum EaseType
        {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseOutBack,
            EaseOutElastic
        }

        private float ApplyEase(float t, EaseType ease)
        {
            switch (ease)
            {
                case EaseType.Linear:
                    return t;
                case EaseType.EaseInQuad:
                    return t * t;
                case EaseType.EaseOutQuad:
                    return t * (2 - t);
                case EaseType.EaseInOutQuad:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case EaseType.EaseOutBack:
                    float c1 = 1.70158f;
                    float c3 = c1 + 1;
                    return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
                case EaseType.EaseOutElastic:
                    if (t == 0 || t == 1) return t;
                    float c4 = (2 * Mathf.PI) / 3;
                    return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
                default:
                    return t;
            }
        }

        #endregion

        #region Tween Management

        private int StartTween(IEnumerator tweenCoroutine)
        {
            int id = ++_tweenIdCounter;
            Coroutine coroutine = StartCoroutine(TweenWrapper(id, tweenCoroutine));
            _runningTweens[id] = coroutine;
            return id;
        }

        private IEnumerator TweenWrapper(int id, IEnumerator tweenCoroutine)
        {
            yield return tweenCoroutine;
            _runningTweens.Remove(id);
        }

        /// <summary>
        /// 停止指定Tween
        /// </summary>
        public void StopTween(int tweenId)
        {
            if (_runningTweens.TryGetValue(tweenId, out Coroutine coroutine))
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                _runningTweens.Remove(tweenId);
            }
        }

        /// <summary>
        /// 停止所有Tween
        /// </summary>
        public void StopAllTweens()
        {
            foreach (var coroutine in _runningTweens.Values)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            _runningTweens.Clear();
        }

        #endregion
    }
}
