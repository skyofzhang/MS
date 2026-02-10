using UnityEngine;
using System.Collections;
using MoShou.Systems;

namespace MoShou.Effects
{
    /// <summary>
    /// 游戏反馈系统
    /// 实现策划案3.1.1定义的打击感反馈
    /// - 屏幕震动: 中等震动100ms
    /// - 顿帧效果: 命中暂停0.05秒
    /// - 击中反馈: 怪物受击特效+后退+僵直
    /// </summary>
    public class GameFeedback : MonoBehaviour
    {
        private static GameFeedback _instance;
        public static GameFeedback Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameFeedback");
                    _instance = go.AddComponent<GameFeedback>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #region 反馈参数 (策划案3.1.1)

        [Header("屏幕震动参数")]
        [SerializeField] private float _shakeIntensity = 0.003f;    // 极轻微震动强度
        [SerializeField] private float _shakeDuration = 0.05f;      // 50ms 极短震动

        [Header("顿帧参数")]
        [SerializeField] private float _hitStopDuration = 0.05f;    // 50ms顿帧
        [SerializeField] private float _criticalHitStopDuration = 0.1f; // 暴击100ms顿帧

        [Header("击退参数")]
        [SerializeField] private float _knockbackForce = 2f;        // 击退力度
        [SerializeField] private float _knockbackDuration = 0.2f;   // 击退持续时间

        [Header("受击僵直")]
        [SerializeField] private float _stunDuration = 0.15f;       // 僵直时间

        #endregion

        #region 组件引用

        private ScreenShake _screenShake;
        private HitStop _hitStop;
        private VFXManager _vfxManager;

        #endregion

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeComponents();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeComponents()
        {
            // 获取或创建ScreenShake组件
            _screenShake = FindObjectOfType<ScreenShake>();
            if (_screenShake == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    _screenShake = mainCamera.gameObject.AddComponent<ScreenShake>();
                }
            }

            // 获取或创建HitStop组件
            _hitStop = FindObjectOfType<HitStop>();
            if (_hitStop == null)
            {
                _hitStop = gameObject.AddComponent<HitStop>();
            }

            // 获取VFXManager
            _vfxManager = FindObjectOfType<VFXManager>();
        }

        #region 公开API

        /// <summary>
        /// 触发命中反馈 (策划案3.1.1标准打击感)
        /// </summary>
        /// <param name="hitPosition">命中位置</param>
        /// <param name="isCritical">是否暴击</param>
        public void TriggerHitFeedback(Vector3 hitPosition, bool isCritical = false)
        {
            // 1. 播放命中VFX
            PlayHitVFX(hitPosition, isCritical);

            // 2. 屏幕震动 (中等震动100ms)
            TriggerScreenShake(isCritical ? _shakeIntensity * 1.5f : _shakeIntensity);

            // 3. 顿帧效果 (命中暂停0.05秒)
            TriggerHitStop(isCritical ? _criticalHitStopDuration : _hitStopDuration);

            // 4. 播放命中音效
            PlayHitSound(isCritical);
        }

        /// <summary>
        /// 触发攻击反馈 (玩家攻击时)
        /// </summary>
        public void TriggerAttackFeedback(Vector3 attackPosition)
        {
            // 轻微震动
            TriggerScreenShake(_shakeIntensity * 0.3f, _shakeDuration * 0.5f);

            // 攻击挥砍VFX (如有)
            if (_vfxManager != null)
            {
                _vfxManager.PlayVFX("VFX_Attack_Swing", attackPosition);
            }
        }

        /// <summary>
        /// 触发怪物受击反馈
        /// </summary>
        /// <param name="target">受击目标</param>
        /// <param name="attackDirection">攻击方向</param>
        /// <param name="damage">伤害值</param>
        /// <param name="isCritical">是否暴击</param>
        public void TriggerEnemyHitFeedback(GameObject target, Vector3 attackDirection,
            int damage, bool isCritical = false)
        {
            if (target == null) return;

            // 1. 触发击退
            ApplyKnockback(target, attackDirection);

            // 2. 触发僵直
            ApplyStun(target);

            // 3. 播放受击动画
            PlayHitAnimation(target);

            // 4. 闪白效果
            StartCoroutine(FlashWhiteEffect(target));

            // 5. 显示伤害数字 (通过UI系统)
            ShowDamageNumber(target.transform.position, damage, isCritical);
        }

        /// <summary>
        /// 触发死亡反馈
        /// </summary>
        /// <param name="target">死亡目标</param>
        public void TriggerDeathFeedback(GameObject target)
        {
            if (target == null) return;

            Vector3 deathPosition = target.transform.position;

            // 1. 死亡VFX
            if (_vfxManager != null)
            {
                _vfxManager.PlayVFX("VFX_Death_Dissolve", deathPosition);
            }

            // 2. 轻微震动
            TriggerScreenShake(_shakeIntensity * 0.5f);

            // 3. 短暂顿帧
            TriggerHitStop(_hitStopDuration * 0.5f);

            // 4. 死亡音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_Enemy_Death");
            }
        }

        /// <summary>
        /// 触发技能释放反馈
        /// </summary>
        /// <param name="skillId">技能ID</param>
        /// <param name="castPosition">释放位置</param>
        public void TriggerSkillFeedback(string skillId, Vector3 castPosition)
        {
            // 技能VFX
            if (_vfxManager != null)
            {
                _vfxManager.PlayVFX($"VFX_Skill_{skillId}", castPosition);
            }

            // 技能震动 (根据技能类型调整)
            TriggerScreenShake(_shakeIntensity * 0.8f);

            // 技能音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX($"SFX_Skill_{skillId}");
            }
        }

        /// <summary>
        /// 触发升级反馈
        /// </summary>
        public void TriggerLevelUpFeedback(Vector3 playerPosition)
        {
            // 升级VFX
            if (_vfxManager != null)
            {
                _vfxManager.PlayVFX("VFX_LevelUp", playerPosition);
            }

            // 强烈震动
            TriggerScreenShake(_shakeIntensity * 1.2f, 0.2f);

            // 升级音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("SFX_LevelUp");
            }
        }

        /// <summary>
        /// 触发拾取反馈
        /// </summary>
        public void TriggerPickupFeedback(Vector3 pickupPosition, string pickupType)
        {
            // 拾取VFX
            if (_vfxManager != null)
            {
                if (pickupType == "Gold")
                {
                    _vfxManager.PlayVFX("VFX_Gold_Pickup", pickupPosition);
                }
                else if (pickupType == "Exp")
                {
                    _vfxManager.PlayVFX("VFX_Exp_Pickup", pickupPosition);
                }
            }

            // 拾取音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX($"SFX_Pickup_{pickupType}");
            }
        }

        #endregion

        #region 内部实现

        private void PlayHitVFX(Vector3 position, bool isCritical)
        {
            if (_vfxManager == null)
            {
                _vfxManager = FindObjectOfType<VFXManager>();
            }

            if (_vfxManager != null)
            {
                // 暴击使用橙红色特效，普通命中使用白色火花特效
                string vfxId = isCritical ? "VFX_Hit_Critical" : "VFX_Hit_Spark";
                _vfxManager.PlayVFX(vfxId, position);
            }
        }

        private void TriggerScreenShake(float intensity, float duration = -1f)
        {
            // 完全禁用屏幕震动 - 与CameraFollow冲突会产生大幅度旋转
            // 用户反馈震动导致头晕，故禁用
            return;
        }

        private void TriggerHitStop(float duration)
        {
            if (_hitStop == null)
            {
                _hitStop = FindObjectOfType<HitStop>();
                if (_hitStop == null)
                {
                    _hitStop = gameObject.AddComponent<HitStop>();
                }
            }

            if (_hitStop != null)
            {
                _hitStop.Freeze(duration);
            }
        }

        private void PlayHitSound(bool isCritical)
        {
            if (AudioManager.Instance != null)
            {
                string sfxId = isCritical ? "SFX_Hit_Critical" : "SFX_Hit_Normal";
                AudioManager.Instance.PlaySFX(sfxId);
            }
        }

        private void ApplyKnockback(GameObject target, Vector3 direction)
        {
            // 尝试获取Rigidbody2D
            Rigidbody2D rb2d = target.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                Vector2 knockbackDir = new Vector2(direction.x, direction.y).normalized;
                rb2d.AddForce(knockbackDir * _knockbackForce, ForceMode2D.Impulse);
                return;
            }

            // 尝试获取Rigidbody
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction.normalized * _knockbackForce, ForceMode.Impulse);
                return;
            }

            // 如果没有物理组件，使用协程进行位移
            StartCoroutine(KnockbackCoroutine(target.transform, direction.normalized));
        }

        private IEnumerator KnockbackCoroutine(Transform target, Vector3 direction)
        {
            if (target == null) yield break;

            Vector3 startPos = target.position;
            Vector3 endPos = startPos + direction * _knockbackForce * 0.5f;
            float elapsed = 0f;

            while (elapsed < _knockbackDuration && target != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _knockbackDuration;
                // 使用Ease Out Quad
                t = t * (2 - t);
                target.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
        }

        private void ApplyStun(GameObject target)
        {
            // 尝试调用怪物的Stun方法
            var enemyController = target.GetComponent<MonoBehaviour>();
            if (enemyController != null)
            {
                // 使用反射或接口调用Stun
                var stunMethod = enemyController.GetType().GetMethod("Stun");
                if (stunMethod != null)
                {
                    stunMethod.Invoke(enemyController, new object[] { _stunDuration });
                }
            }
        }

        private void PlayHitAnimation(GameObject target)
        {
            // 尝试获取Animator
            Animator animator = target.GetComponent<Animator>();
            if (animator != null)
            {
                // 尝试触发受击动画
                animator.SetTrigger("Hit");
            }
        }

        private IEnumerator FlashWhiteEffect(GameObject target)
        {
            if (target == null) yield break;

            // 获取所有SpriteRenderer
            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0) yield break;

            // 保存原始颜色
            Color[] originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    originalColors[i] = renderers[i].color;
            }

            // 闪白
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    renderer.color = Color.white;
            }

            yield return new WaitForSeconds(0.05f);

            // 恢复颜色
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].color = originalColors[i];
            }
        }

        private void ShowDamageNumber(Vector3 position, int damage, bool isCritical)
        {
            // 通过UI系统显示伤害数字
            // 这里可以扩展为对象池
            if (UI.UIFeedbackSystem.Instance != null)
            {
                // 需要创建伤害数字UI预制体并实例化
                // 暂时通过Debug显示
                Debug.Log($"[GameFeedback] 伤害数字: {damage} (暴击: {isCritical}) at {position}");
            }
        }

        #endregion
    }
}
