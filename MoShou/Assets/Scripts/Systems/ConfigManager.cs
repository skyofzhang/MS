using System;
using UnityEngine;

namespace MoShou.Systems
{
    /// <summary>
    /// 全局配置管理器 - 加载 GameSettings.json 并提供配置值
    /// 纯C#单例（非MonoBehaviour），首次访问时自动加载
    /// </summary>
    public class ConfigManager
    {
        private static ConfigManager _instance;
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                    _instance.LoadSettings();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 全局配置数据
        /// </summary>
        public GameSettingsData Settings { get; private set; }

        /// <summary>
        /// 是否成功加载了配置文件
        /// </summary>
        public bool IsLoaded { get; private set; }

        private void LoadSettings()
        {
            TextAsset configFile = Resources.Load<TextAsset>("Configs/GameSettings");
            if (configFile != null)
            {
                try
                {
                    Settings = JsonUtility.FromJson<GameSettingsData>(configFile.text);
                    if (Settings != null)
                    {
                        IsLoaded = true;
                        Debug.Log("[ConfigManager] GameSettings.json 加载成功");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ConfigManager] 解析GameSettings失败: {e.Message}，使用默认配置");
                }
            }
            else
            {
                Debug.LogWarning("[ConfigManager] GameSettings.json 未找到，使用默认配置");
            }

            // 加载失败时使用默认值
            Settings = new GameSettingsData();
            IsLoaded = false;
        }

        /// <summary>
        /// 强制重新加载配置（用于编辑器调试）
        /// </summary>
        public void Reload()
        {
            LoadSettings();
        }
    }

    // ============================================================
    // 配置数据类 — 字段名与 GameSettings.json 的 key 一一对应
    // ============================================================

    [Serializable]
    public class GameSettingsData
    {
        public GameSettingsSection gameSettings = new GameSettingsSection();
        public PlayerSettingsSection playerSettings = new PlayerSettingsSection();
        public CombatSettingsSection combatSettings = new CombatSettingsSection();
        public DefeatSettingsSection defeatSettings = new DefeatSettingsSection();
    }

    [Serializable]
    public class GameSettingsSection
    {
        public string version = "1.0.0";
        public int targetFrameRate = 60;
    }

    [Serializable]
    public class PlayerSettingsSection
    {
        public int startingGold = 100;
        public int startingLevel = 1;
        public int baseMaxHp = 100;
        public int baseAttack = 10;
        public int baseDefense = 5;
        public float baseCritRate = 0.05f;
        public float baseCritDamage = 1.5f;
        public float moveSpeed = 5.0f;
        public float attackRange = 1.5f;
        public float attackSpeed = 1.0f;
    }

    [Serializable]
    public class CombatSettingsSection
    {
        public int minDamage = 1;
        public float knockbackForce = 3.0f;
        public float invincibilityDuration = 0.5f;
    }

    [Serializable]
    public class DefeatSettingsSection
    {
        public float partialGoldRatio = 0.5f;
        public float partialExpRatio = 0.5f;
    }
}
