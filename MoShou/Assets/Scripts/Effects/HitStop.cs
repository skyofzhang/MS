using UnityEngine;
using System.Collections;

namespace MoShou.Effects
{
    /// <summary>
    /// 顿帧效果组件
    /// 实现策划案3.1.1定义的命中暂停0.05秒效果
    /// 通过Time.timeScale实现时间冻结
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        [Header("顿帧设置")]
        [SerializeField] private float _defaultDuration = 0.05f;  // 50ms
        [SerializeField] private float _minTimeScale = 0.0f;      // 完全冻结
        [SerializeField] private bool _smoothRestore = true;      // 平滑恢复

        [Header("调试")]
        [SerializeField] private bool _enableHitStop = true;

        private Coroutine _hitStopCoroutine;
        private float _originalTimeScale = 1f;
        private bool _isFrozen;

        // 防止在暂停状态下重复调用
        private static bool _globalFreezing;

        /// <summary>
        /// 是否正在顿帧中
        /// </summary>
        public bool IsFrozen => _isFrozen;

        /// <summary>
        /// 触发顿帧效果
        /// </summary>
        /// <param name="duration">冻结时间 (实际时间，不受timeScale影响)</param>
        public void Freeze(float duration = -1f)
        {
            if (!_enableHitStop) return;
            if (_globalFreezing) return;  // 防止重叠

            if (duration < 0) duration = _defaultDuration;
            if (duration <= 0) return;

            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
            }

            _hitStopCoroutine = StartCoroutine(FreezeCoroutine(duration));
        }

        /// <summary>
        /// 立即恢复时间流速
        /// </summary>
        public void Resume()
        {
            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
                _hitStopCoroutine = null;
            }

            RestoreTimeScale();
        }

        private IEnumerator FreezeCoroutine(float duration)
        {
            _globalFreezing = true;
            _isFrozen = true;

            // 保存原始时间缩放
            _originalTimeScale = Time.timeScale;

            // 冻结时间
            Time.timeScale = _minTimeScale;

            // 等待实际时间 (使用WaitForSecondsRealtime)
            yield return new WaitForSecondsRealtime(duration);

            // 恢复时间
            if (_smoothRestore)
            {
                // 平滑恢复
                float restoreDuration = 0.02f;
                float elapsed = 0f;

                while (elapsed < restoreDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / restoreDuration;
                    Time.timeScale = Mathf.Lerp(_minTimeScale, _originalTimeScale, t);
                    yield return null;
                }
            }

            RestoreTimeScale();

            _hitStopCoroutine = null;
        }

        private void RestoreTimeScale()
        {
            Time.timeScale = _originalTimeScale > 0 ? _originalTimeScale : 1f;
            _isFrozen = false;
            _globalFreezing = false;
        }

        #region 便捷方法

        /// <summary>
        /// 轻微顿帧 (20ms)
        /// </summary>
        public void LightFreeze()
        {
            Freeze(0.02f);
        }

        /// <summary>
        /// 标准顿帧 (50ms，策划案3.1.1)
        /// </summary>
        public void StandardFreeze()
        {
            Freeze(0.05f);
        }

        /// <summary>
        /// 暴击顿帧 (100ms)
        /// </summary>
        public void CriticalFreeze()
        {
            Freeze(0.1f);
        }

        /// <summary>
        /// 重击顿帧 (150ms，用于Boss攻击)
        /// </summary>
        public void HeavyFreeze()
        {
            Freeze(0.15f);
        }

        #endregion

        private void OnDisable()
        {
            // 确保禁用时恢复时间
            if (_isFrozen)
            {
                Resume();
            }
        }

        private void OnDestroy()
        {
            // 确保销毁时恢复时间
            if (_isFrozen)
            {
                RestoreTimeScale();
            }
        }
    }
}
