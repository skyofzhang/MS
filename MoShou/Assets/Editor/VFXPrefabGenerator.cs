using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// VFX预制体生成器
/// 为VFX贴图创建粒子系统Prefab
/// 按照RULE-RES-011规范生成
/// </summary>
public class VFXPrefabGenerator : EditorWindow
{
    // VFX配置结构
    private struct VFXConfig
    {
        public string texturePath;
        public string prefabName;
        public float lifetime;
        public float startSize;
        public float endSize;
        public Color startColor;
        public Color endColor;
        public int burstCount;
        public float emissionRate;
        public bool loop;
        public float gravityModifier;
        public ParticleSystemShapeType shape;
        public float shapeRadius;

        public VFXConfig(string texPath, string prefab, float life, float sSize, float eSize,
            Color sColor, Color eColor, int burst, float rate, bool isLoop, float gravity,
            ParticleSystemShapeType shapeType, float radius)
        {
            texturePath = texPath;
            prefabName = prefab;
            lifetime = life;
            startSize = sSize;
            endSize = eSize;
            startColor = sColor;
            endColor = eColor;
            burstCount = burst;
            emissionRate = rate;
            loop = isLoop;
            gravityModifier = gravity;
            shape = shapeType;
            shapeRadius = radius;
        }
    }

    private static readonly VFXConfig[] VFXConfigs = new VFXConfig[]
    {
        // 击中火花 - burst=8, lifetime=0.3s, size=0.5→0
        new VFXConfig(
            "Sprites/Generated/VFX_Hit_Spark", "VFX_Hit_Spark",
            0.3f, 0.5f, 0f,
            Color.white, new Color(1f, 0.8f, 0f, 0f),
            8, 0f, false, 0f,
            ParticleSystemShapeType.Sphere, 0.1f
        ),

        // 箭矢拖尾 - rate=30, lifetime=0.5s, trail=true
        new VFXConfig(
            "Sprites/Generated/VFX_Arrow_Trail", "VFX_Arrow_Trail",
            0.5f, 0.3f, 0.1f,
            new Color(1f, 0.85f, 0.3f, 1f), new Color(1f, 0.6f, 0f, 0f),
            0, 30f, true, 0f,
            ParticleSystemShapeType.Cone, 0.05f
        ),

        // 升级特效 - burst=20, lifetime=1.5s, size=1→2, gravity=-0.5
        new VFXConfig(
            "Sprites/Generated/VFX_LevelUp", "VFX_LevelUp",
            1.5f, 1f, 2f,
            new Color(1f, 0.9f, 0.3f, 1f), new Color(1f, 1f, 0.5f, 0f),
            20, 0f, false, -0.5f,
            ParticleSystemShapeType.Circle, 0.5f
        ),

        // 治疗特效 - rate=15, lifetime=1s, size=0.3, gravity=-1
        new VFXConfig(
            "Sprites/Generated/VFX_Heal", "VFX_Heal",
            1f, 0.3f, 0.5f,
            new Color(0.2f, 1f, 0.3f, 1f), new Color(0.5f, 1f, 0.5f, 0f),
            0, 15f, false, -1f,
            ParticleSystemShapeType.Circle, 0.3f
        ),

        // 死亡消融特效
        new VFXConfig(
            "Sprites/Generated/VFX_Hit_Spark", "VFX_Death_Dissolve",
            1.2f, 0.8f, 0f,
            Color.white, new Color(0.3f, 0.3f, 0.3f, 0f),
            15, 0f, false, 0.3f,
            ParticleSystemShapeType.Box, 0.5f
        ),

        // 金币拾取特效
        new VFXConfig(
            "Sprites/Generated/UI_Icon_Coin", "VFX_Gold_Pickup",
            0.8f, 0.4f, 0f,
            new Color(1f, 0.85f, 0f, 1f), new Color(1f, 0.9f, 0.3f, 0f),
            5, 0f, false, -1.5f,
            ParticleSystemShapeType.Sphere, 0.2f
        ),

        // 多重箭技能特效 - 扇形发射
        new VFXConfig(
            "Sprites/Generated/VFX_Arrow_Trail", "VFX_MultiShot",
            0.6f, 0.4f, 0.1f,
            new Color(0.3f, 0.8f, 1f, 1f), new Color(0.5f, 0.9f, 1f, 0f),
            5, 0f, false, 0f,
            ParticleSystemShapeType.Cone, 0.3f
        ),

        // 穿透箭技能特效 - 穿透光线
        new VFXConfig(
            "Sprites/Generated/VFX_Arrow_Trail", "VFX_PierceShot",
            0.8f, 0.6f, 0.2f,
            new Color(1f, 0.3f, 0.3f, 1f), new Color(1f, 0.5f, 0.2f, 0f),
            3, 0f, false, 0f,
            ParticleSystemShapeType.Cone, 0.1f
        ),

        // 战吼技能特效 - 圆形扩散
        new VFXConfig(
            "Sprites/Generated/VFX_LevelUp", "VFX_BattleShout",
            1.0f, 2f, 4f,
            new Color(1f, 0.6f, 0.2f, 0.8f), new Color(1f, 0.8f, 0.4f, 0f),
            12, 0f, false, 0f,
            ParticleSystemShapeType.Circle, 1f
        ),

        // BOSS狂暴特效 - 红色火焰环绕
        new VFXConfig(
            "Sprites/Generated/VFX_Hit_Spark", "VFX_Boss_Rage",
            2f, 1f, 1.5f,
            new Color(1f, 0.2f, 0.1f, 1f), new Color(1f, 0.4f, 0f, 0f),
            0, 20f, true, -0.3f,
            ParticleSystemShapeType.Circle, 1.5f
        ),
    };

    [MenuItem("MoShou/Generate VFX Prefabs")]
    public static void GenerateVFXPrefabs()
    {
        string prefabFolder = "Assets/Resources/Prefabs/VFX";

        // 确保目录存在
        if (!Directory.Exists(prefabFolder))
        {
            Directory.CreateDirectory(prefabFolder);
            AssetDatabase.Refresh();
        }

        int generatedCount = 0;

        foreach (var config in VFXConfigs)
        {
            string prefabPath = $"{prefabFolder}/{config.prefabName}.prefab";

            // 加载贴图
            Sprite sprite = Resources.Load<Sprite>(config.texturePath);
            Texture2D texture = null;
            if (sprite != null)
            {
                texture = sprite.texture;
            }
            else
            {
                // 尝试直接加载Texture
                texture = Resources.Load<Texture2D>(config.texturePath);
            }

            // 创建粒子系统GameObject
            GameObject vfxGO = new GameObject(config.prefabName);
            ParticleSystem ps = vfxGO.AddComponent<ParticleSystem>();

            // 配置Main模块
            var main = ps.main;
            main.duration = config.lifetime * 2f;
            main.loop = config.loop;
            main.startLifetime = config.lifetime;
            main.startSpeed = config.loop ? 1f : 3f;
            main.startSize = config.startSize;
            main.startColor = config.startColor;
            main.gravityModifier = config.gravityModifier;
            main.playOnAwake = true;
            main.stopAction = config.loop ? ParticleSystemStopAction.None : ParticleSystemStopAction.Destroy;

            // 配置Emission模块
            var emission = ps.emission;
            emission.enabled = true;
            if (config.burstCount > 0)
            {
                emission.rateOverTime = 0;
                emission.SetBurst(0, new ParticleSystem.Burst(0f, config.burstCount));
            }
            else
            {
                emission.rateOverTime = config.emissionRate;
            }

            // 配置Shape模块
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = config.shape;
            shape.radius = config.shapeRadius;

            // 配置Size over Lifetime
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, config.endSize / config.startSize);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // 配置Color over Lifetime
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(config.startColor, 0f),
                    new GradientColorKey(config.endColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(config.startColor.a, 0f),
                    new GradientAlphaKey(config.endColor.a, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // 配置Renderer
            var renderer = vfxGO.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // 创建材质
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2); // Fade mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;

            if (texture != null)
            {
                mat.mainTexture = texture;
            }

            // 保存材质
            string matPath = $"{prefabFolder}/{config.prefabName}_Mat.mat";
            AssetDatabase.CreateAsset(mat, matPath);
            renderer.material = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            // 保存Prefab
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }
            PrefabUtility.SaveAsPrefabAsset(vfxGO, prefabPath);

            // 销毁临时对象
            GameObject.DestroyImmediate(vfxGO);

            generatedCount++;
            Debug.Log($"[VFXPrefabGenerator] 生成: {prefabPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[VFXPrefabGenerator] 完成! 共生成 {generatedCount} 个VFX Prefab");
        EditorUtility.DisplayDialog("VFX Prefab Generator",
            $"生成完成!\n共生成 {generatedCount} 个VFX Prefab", "确定");
    }

    [MenuItem("MoShou/Test VFX Prefab")]
    public static void TestVFXPrefab()
    {
        // 在场景中生成一个测试VFX
        GameObject vfxPrefab = Resources.Load<GameObject>("Prefabs/VFX/VFX_Hit_Spark");
        if (vfxPrefab != null)
        {
            GameObject instance = GameObject.Instantiate(vfxPrefab, Vector3.zero, Quaternion.identity);
            instance.name = "TEST_VFX_Hit_Spark";
            Selection.activeGameObject = instance;
            Debug.Log("[VFXPrefabGenerator] 测试VFX已生成在场景中");
        }
        else
        {
            Debug.LogError("[VFXPrefabGenerator] 找不到VFX_Hit_Spark预制体，请先运行 Generate VFX Prefabs");
        }
    }
}
