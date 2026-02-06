using UnityEngine;

namespace MoShou.Utils
{
    /// <summary>
    /// 资源加载器 - 资源先行开发模式
    /// 负责加载资源并提供优雅降级策略
    /// V4.0: 符合AI开发知识库§3.11规范
    /// </summary>
    public static class ResourceLoader
    {
        private const string TAG = "[ResourceLoader]";

        #region 路径常量 - 对应Git资源库映射

        // 模型路径
        public const string PATH_PLAYER_MODELS = "Models/Player/";
        public const string PATH_MONSTER_MODELS = "Models/Monsters/";
        public const string PATH_ENVIRONMENT = "Models/Environment/";

        // UI路径
        public const string PATH_UI_HUD = "Sprites/UI/HUD/";
        public const string PATH_UI_ICONS = "Sprites/UI/Icons/";
        public const string PATH_UI_BUTTONS = "Sprites/UI/Buttons/";
        public const string PATH_UI_BACKGROUNDS = "Sprites/UI/Backgrounds/";

        // 特效路径
        public const string PATH_VFX = "Prefabs/VFX/";
        public const string PATH_SKILL_VFX = "Prefabs/VFX/Skills/";

        // 音频路径
        public const string PATH_AUDIO_BGM = "Audio/BGM/";
        public const string PATH_AUDIO_SFX = "Audio/SFX/";

        #endregion

        #region 模型加载

        /// <summary>
        /// 加载3D模型
        /// </summary>
        /// <param name="path">Resources下的相对路径</param>
        /// <returns>模型GameObject，失败时返回降级立方体</returns>
        public static GameObject LoadModel(string path)
        {
            var obj = Resources.Load<GameObject>(path);
            if (obj == null)
            {
                Debug.LogWarning($"{TAG} Model not found: {path}, using fallback cube");
                return CreateFallbackCube(path);
            }
            Debug.Log($"{TAG} Model loaded: {path}");
            return obj;
        }

        /// <summary>
        /// 加载玩家模型
        /// </summary>
        public static GameObject LoadPlayerModel(string modelName)
        {
            return LoadModel(PATH_PLAYER_MODELS + modelName);
        }

        /// <summary>
        /// 加载怪物模型
        /// </summary>
        public static GameObject LoadMonsterModel(string monsterName)
        {
            return LoadModel(PATH_MONSTER_MODELS + monsterName);
        }

        private static GameObject CreateFallbackCube(string originalPath)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"FALLBACK_{System.IO.Path.GetFileName(originalPath)}";

            // 使用红色材质标识降级对象
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.3f, 0.3f, 1f);
            }

            return cube;
        }

        #endregion

        #region 精灵/纹理加载

        /// <summary>
        /// 加载Sprite
        /// </summary>
        /// <param name="path">Resources下的相对路径</param>
        /// <returns>Sprite，失败时返回降级白色精灵</returns>
        public static Sprite LoadSprite(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"{TAG} Sprite not found: {path}, using fallback white");
                return CreateFallbackSprite();
            }
            Debug.Log($"{TAG} Sprite loaded: {path}");
            return sprite;
        }

        /// <summary>
        /// 加载UI图标
        /// </summary>
        public static Sprite LoadIcon(string iconName)
        {
            return LoadSprite(PATH_UI_ICONS + iconName);
        }

        /// <summary>
        /// 加载HUD元素
        /// </summary>
        public static Sprite LoadHUDElement(string elementName)
        {
            return LoadSprite(PATH_UI_HUD + elementName);
        }

        private static Sprite CreateFallbackSprite()
        {
            // 创建1x1白色纹理作为降级
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.magenta); // 使用品红色标识缺失
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        }

        #endregion

        #region 音频加载

        /// <summary>
        /// 加载音频片段
        /// </summary>
        /// <param name="path">Resources下的相对路径</param>
        /// <returns>AudioClip，失败时返回null并记录警告</returns>
        public static AudioClip LoadAudio(string path)
        {
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"{TAG} Audio not found: {path}, audio will be silent");
                return null; // 音频降级为静音
            }
            Debug.Log($"{TAG} Audio loaded: {path}");
            return clip;
        }

        /// <summary>
        /// 加载BGM
        /// </summary>
        public static AudioClip LoadBGM(string bgmName)
        {
            return LoadAudio(PATH_AUDIO_BGM + bgmName);
        }

        /// <summary>
        /// 加载音效
        /// </summary>
        public static AudioClip LoadSFX(string sfxName)
        {
            return LoadAudio(PATH_AUDIO_SFX + sfxName);
        }

        #endregion

        #region 特效加载

        /// <summary>
        /// 加载VFX预制体
        /// </summary>
        /// <param name="path">Resources下的相对路径</param>
        /// <returns>特效预制体，失败时返回降级粒子系统</returns>
        public static GameObject LoadVFX(string path)
        {
            var vfx = Resources.Load<GameObject>(path);
            if (vfx == null)
            {
                Debug.LogWarning($"{TAG} VFX not found: {path}, using fallback particles");
                return CreateFallbackVFX(path);
            }
            Debug.Log($"{TAG} VFX loaded: {path}");
            return vfx;
        }

        /// <summary>
        /// 加载技能特效
        /// </summary>
        public static GameObject LoadSkillVFX(string skillName)
        {
            return LoadVFX(PATH_SKILL_VFX + skillName);
        }

        private static GameObject CreateFallbackVFX(string originalPath)
        {
            var obj = new GameObject($"FALLBACK_VFX_{System.IO.Path.GetFileName(originalPath)}");
            var ps = obj.AddComponent<ParticleSystem>();

            // 配置简单的降级粒子效果
            var main = ps.main;
            main.startColor = new Color(1f, 0.5f, 0f, 1f); // 橙色标识降级
            main.startSize = 0.5f;
            main.startLifetime = 1f;
            main.maxParticles = 10;

            var emission = ps.emission;
            emission.rateOverTime = 5f;

            return obj;
        }

        #endregion

        #region 资源存在性检查

        /// <summary>
        /// 检查资源是否存在（不实际加载）
        /// </summary>
        public static bool ResourceExists<T>(string path) where T : Object
        {
            var resource = Resources.Load<T>(path);
            return resource != null;
        }

        /// <summary>
        /// 批量检查资源存在性
        /// </summary>
        /// <returns>返回缺失的资源路径列表</returns>
        public static string[] CheckResourcesExist<T>(params string[] paths) where T : Object
        {
            var missing = new System.Collections.Generic.List<string>();
            foreach (var path in paths)
            {
                if (!ResourceExists<T>(path))
                {
                    missing.Add(path);
                }
            }
            return missing.ToArray();
        }

        #endregion
    }
}
