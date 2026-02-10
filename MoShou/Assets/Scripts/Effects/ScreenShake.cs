using UnityEngine;
using System.Collections;

namespace MoShou.Effects
{
    /// <summary>
    /// 屏幕震动组件
    /// 实现策划案3.1.1定义的中等震动100ms效果
    /// 附加到主相机上使用
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        [Header("震动设置")]
        [SerializeField] private float _defaultIntensity = 0.003f;  // 极轻微：0.02 -> 0.003
        [SerializeField] private float _defaultDuration = 0.05f;    // 50ms 更短
        [SerializeField] private float _decreaseFactor = 2.0f;      // 更快衰减

        [Header("频率设置")]
        [SerializeField] private float _shakeFrequency = 50f;       // 更高频率，更细微

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Coroutine _shakeCoroutine;
        private float _currentIntensity;
        private bool _isShaking;

        private void Awake()
        {
            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
        }

        /// <summary>
        /// 触发屏幕震动
        /// </summary>
        /// <param name="intensity">震动强度 (0.1-1.0)</param>
        /// <param name="duration">持续时间 (秒)</param>
        public void Shake(float intensity = -1f, float duration = -1f)
        {
            if (intensity < 0) intensity = _defaultIntensity;
            if (duration < 0) duration = _defaultDuration;

            // 如果新的震动更强，或者当前没有震动
            if (intensity > _currentIntensity || !_isShaking)
            {
                if (_shakeCoroutine != null)
                {
                    StopCoroutine(_shakeCoroutine);
                }

                _shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
            }
        }

        /// <summary>
        /// 立即停止震动
        /// </summary>
        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            _isShaking = false;
            _currentIntensity = 0f;

            // 恢复原位置
            transform.localPosition = _originalPosition;
            transform.localRotation = _originalRotation;
        }

        private IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            _isShaking = true;
            _currentIntensity = intensity;

            float elapsed = 0f;
            float seed = Random.Range(0f, 100f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                // 使用Perlin噪声生成平滑的随机位移
                float percentComplete = elapsed / duration;

                // 强度随时间衰减
                float currentStrength = intensity * (1f - percentComplete * _decreaseFactor);
                currentStrength = Mathf.Max(0, currentStrength);

                // 使用Perlin噪声获得平滑的随机值
                float time = elapsed * _shakeFrequency;
                float offsetX = (Mathf.PerlinNoise(seed, time) * 2f - 1f) * currentStrength;
                float offsetY = (Mathf.PerlinNoise(seed + 1f, time) * 2f - 1f) * currentStrength;

                // 应用位移
                transform.localPosition = _originalPosition + new Vector3(offsetX, offsetY, 0f);

                // 禁用旋转，只保留位移，彻底避免头晕
                // float rotationZ = 0; // 完全禁用旋转
                transform.localRotation = _originalRotation;

                _currentIntensity = currentStrength;

                yield return null;
            }

            // 恢复原位置
            transform.localPosition = _originalPosition;
            transform.localRotation = _originalRotation;

            _isShaking = false;
            _currentIntensity = 0f;
            _shakeCoroutine = null;
        }

        #region 便捷方法

        /// <summary>
        /// 轻微震动 (用于普通攻击) - 极轻微
        /// </summary>
        public void LightShake()
        {
            Shake(0.001f, 0.03f);
        }

        /// <summary>
        /// 中等震动 (用于命中反馈) - 轻微可感知
        /// </summary>
        public void MediumShake()
        {
            Shake(0.003f, 0.05f);
        }

        /// <summary>
        /// 强烈震动 (用于暴击/Boss攻击) - 略明显
        /// </summary>
        public void HeavyShake()
        {
            Shake(0.006f, 0.06f);
        }

        /// <summary>
        /// 特效震动 (用于技能/升级)
        /// </summary>
        public void EffectShake()
        {
            Shake(0.004f, 0.08f);
        }

        #endregion

        private void OnDisable()
        {
            StopShake();
        }
    }
}
