using UnityEngine;
using System.Collections.Generic;

namespace MoShou.Systems
{
    /// <summary>
    /// 音效管理器 - 管理BGM和SFX的播放
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("音量设置")]
        [Range(0f, 1f)] public float bgmVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.7f;
        public bool isMuted = false;

        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();

        // 音效路径常量 (与实际文件名匹配)
        public static class SFX
        {
            // 战斗音效
            public const string ArrowShoot = "SFX_Arrow_Shoot";
            public const string ArrowHit = "SFX_Arrow_Hit";
            public const string PlayerHit = "SFX_Player_Hit";
            public const string PlayerDeath = "SFX_Player_Death";
            public const string EnemyHit = "SFX_Monster_Hit";      // 修正: Monster 而非 Enemy
            public const string EnemyDeath = "SFX_Monster_Death";  // 修正: Monster 而非 Enemy

            // 技能音效
            public const string SkillMultiShot = "SFX_Skill_MultiShot";
            public const string SkillPierce = "SFX_Skill_Pierce";
            public const string SkillBattleShout = "SFX_Skill_BattleShout";

            // UI音效
            public const string ButtonClick = "SFX_UI_Button_Click";    // 修正: Button_Click 而非 Click
            public const string CoinPickup = "SFX_UI_Gold_Pickup";      // 修正: UI_Gold_Pickup 而非 Coin_Pickup
            public const string LevelUp = "SFX_UI_LevelUp";             // 修正: UI_LevelUp 而非 LevelUp
            public const string PanelOpen = "SFX_UI_Panel_Open";
            public const string PanelClose = "SFX_UI_Panel_Close";
            public const string SkillSelect = "SFX_UI_Skill_Select";
            public const string WaveStart = "SFX_UI_Wave_Start";
            public const string WaveClear = "SFX_UI_Wave_Clear";

            // Boss音效
            public const string BossAttack = "SFX_Boss_Attack";
            public const string BossRoar = "SFX_Boss_Rage";             // 修正: Rage 而非 Roar
            public const string BossSkill = "SFX_Boss_Skill";

            // 环境音效
            public const string AmbientForest = "SFX_Amb_Forest";
            public const string AmbientBossRoom = "SFX_Amb_BossRoom";

            // 结算音效（使用BGM路径）
            public const string Victory = "BGM_Victory";      // 使用BGM作为胜利音效
            public const string Defeat = "BGM_Defeat";        // 使用BGM作为失败音效
        }

        public static class BGM
        {
            public const string MainMenu = "BGM_MainMenu";
            public const string BattleNormal = "BGM_Battle_Normal";
            public const string BossBattle = "BGM_Boss_Battle";
            public const string Victory = "BGM_Victory";
            public const string Defeat = "BGM_Defeat";
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitAudioSources();
                Debug.Log("[AudioManager] 初始化完成");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void InitAudioSources()
        {
            // BGM音源
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.playOnAwake = false;

            // SFX音源
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
            sfxSource.playOnAwake = false;
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBGM(string clipName, bool fadeIn = true)
        {
            if (isMuted) return;

            AudioClip clip = LoadAudioClip($"Audio/BGM/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] BGM未找到: {clipName}");
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
            Debug.Log($"[AudioManager] 播放BGM: {clipName}");
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM(bool fadeOut = true)
        {
            bgmSource.Stop();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(string clipName, float volumeScale = 1f)
        {
            if (isMuted) return;

            AudioClip clip = LoadAudioClip($"Audio/SFX/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX未找到: {clipName}");
                return;
            }

            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }

        /// <summary>
        /// 在指定位置播放3D音效
        /// </summary>
        public void PlaySFXAtPosition(string clipName, Vector3 position, float volumeScale = 1f)
        {
            if (isMuted) return;

            AudioClip clip = LoadAudioClip($"Audio/SFX/{clipName}");
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeScale);
        }

        /// <summary>
        /// 加载音频资源（带缓存）
        /// </summary>
        private AudioClip LoadAudioClip(string path)
        {
            if (audioCache.TryGetValue(path, out AudioClip cached))
                return cached;

            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                audioCache[path] = clip;
            }
            return clip;
        }

        /// <summary>
        /// 设置BGM音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            bgmSource.volume = bgmVolume;
        }

        /// <summary>
        /// 设置SFX音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        /// <summary>
        /// 设置静音
        /// </summary>
        public void SetMute(bool mute)
        {
            isMuted = mute;
            bgmSource.mute = mute;
            sfxSource.mute = mute;
        }

        /// <summary>
        /// 预加载常用音效
        /// </summary>
        public void PreloadCommonSFX()
        {
            LoadAudioClip($"Audio/SFX/{SFX.ArrowShoot}");
            LoadAudioClip($"Audio/SFX/{SFX.ArrowHit}");
            LoadAudioClip($"Audio/SFX/{SFX.EnemyHit}");
            LoadAudioClip($"Audio/SFX/{SFX.EnemyDeath}");
            Debug.Log("[AudioManager] 常用音效预加载完成");
        }
    }
}
