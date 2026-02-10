using UnityEngine;

namespace MoShou.Combat
{
    /// <summary>
    /// 投射物拖尾效果组件
    /// 为弓箭等投射物添加视觉拖尾
    /// </summary>
    public class ProjectileTrail : MonoBehaviour
    {
        [Header("拖尾设置")]
        public float trailTime = 0.3f;
        public float startWidth = 0.15f;
        public float endWidth = 0f;
        public Color startColor = new Color(1f, 0.9f, 0.3f, 1f);  // 金黄色
        public Color endColor = new Color(1f, 0.5f, 0.1f, 0f);    // 橙色渐隐

        private TrailRenderer trailRenderer;

        void Awake()
        {
            SetupTrail();
        }

        void SetupTrail()
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = trailTime;
            trailRenderer.startWidth = startWidth;
            trailRenderer.endWidth = endWidth;
            trailRenderer.numCapVertices = 5;
            trailRenderer.numCornerVertices = 5;

            // 创建材质 - 使用URP兼容Shader
            Shader trailShader = Shader.Find("Universal Render Pipeline/Unlit")
                              ?? Shader.Find("Universal Render Pipeline/Lit")
                              ?? Shader.Find("Sprites/Default")
                              ?? Shader.Find("Standard");
            Material trailMat = new Material(trailShader);
            trailRenderer.material = trailMat;

            // 设置渐变色
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(startColor, 0f),
                    new GradientColorKey(endColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trailRenderer.colorGradient = gradient;

            // 使用Additive混合让拖尾更亮
            trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trailRenderer.receiveShadows = false;

            Debug.Log("[ProjectileTrail] 拖尾效果已添加");
        }

        /// <summary>
        /// 设置拖尾颜色
        /// </summary>
        public void SetColors(Color start, Color end)
        {
            startColor = start;
            endColor = end;

            if (trailRenderer != null)
            {
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(start, 0f),
                        new GradientColorKey(end, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(start.a, 0f),
                        new GradientAlphaKey(end.a, 1f)
                    }
                );
                trailRenderer.colorGradient = gradient;
            }
        }

        /// <summary>
        /// 清除拖尾（用于回收对象池时）
        /// </summary>
        public void ClearTrail()
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        /// <summary>
        /// 静态工厂方法 - 为投射物添加拖尾
        /// </summary>
        public static ProjectileTrail AddToProjectile(GameObject projectile, TrailPreset preset = TrailPreset.Arrow)
        {
            var trail = projectile.AddComponent<ProjectileTrail>();
            trail.ApplyPreset(preset);
            return trail;
        }

        /// <summary>
        /// 应用预设
        /// </summary>
        public void ApplyPreset(TrailPreset preset)
        {
            switch (preset)
            {
                case TrailPreset.Arrow:
                    startColor = new Color(1f, 0.9f, 0.3f, 1f);
                    endColor = new Color(1f, 0.5f, 0.1f, 0f);
                    trailTime = 0.25f;
                    startWidth = 0.12f;
                    break;

                case TrailPreset.FireArrow:
                    startColor = new Color(1f, 0.5f, 0f, 1f);
                    endColor = new Color(1f, 0.2f, 0f, 0f);
                    trailTime = 0.35f;
                    startWidth = 0.18f;
                    break;

                case TrailPreset.IceArrow:
                    startColor = new Color(0.5f, 0.8f, 1f, 1f);
                    endColor = new Color(0.2f, 0.5f, 1f, 0f);
                    trailTime = 0.3f;
                    startWidth = 0.15f;
                    break;

                case TrailPreset.MultiShot:
                    startColor = new Color(0.3f, 1f, 0.5f, 1f);
                    endColor = new Color(0.1f, 0.8f, 0.3f, 0f);
                    trailTime = 0.2f;
                    startWidth = 0.1f;
                    break;

                case TrailPreset.Pierce:
                    startColor = new Color(1f, 1f, 0.3f, 1f);
                    endColor = new Color(1f, 0.8f, 0f, 0f);
                    trailTime = 0.4f;
                    startWidth = 0.2f;
                    break;
            }

            if (trailRenderer != null)
            {
                trailRenderer.time = trailTime;
                trailRenderer.startWidth = startWidth;
                SetColors(startColor, endColor);
            }
        }

        public enum TrailPreset
        {
            Arrow,
            FireArrow,
            IceArrow,
            MultiShot,
            Pierce
        }
    }
}
