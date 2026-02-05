using System;
using System.Collections.Generic;
using UnityEngine;
using MoShou.Data;

namespace MoShou.Systems
{
    /// <summary>
    /// 存档数据结构
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string saveTime;

        // 玩家数据
        public PlayerStats playerStats;

        // 装备数据（槽位 -> 装备ID）
        public SerializableDictionary<string, string> equippedItems;

        // 背包数据（物品ID -> 数量）
        public SerializableDictionary<string, int> inventory;

        // 游戏进度
        public int highestLevel = 1;
        public int totalPlayTime = 0;
    }

    /// <summary>
    /// 可序列化的字典（Unity JsonUtility 不支持 Dictionary）
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public void Add(TKey key, TValue value)
        {
            keys.Add(key);
            values.Add(value);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count && i < values.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }

        public static SerializableDictionary<TKey, TValue> FromDictionary(Dictionary<TKey, TValue> dict)
        {
            var sd = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in dict)
            {
                sd.Add(kvp.Key, kvp.Value);
            }
            return sd;
        }
    }

    /// <summary>
    /// 存档系统 - 使用 PlayerPrefs 本地存储
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string SAVE_KEY = "MS_SAVE_DATA";
        private const string BACKUP_KEY = "MS_SAVE_BACKUP";

        // 当前玩家数据
        public PlayerStats CurrentPlayerStats { get; private set; }

        // 事件
        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;

        private float playTimeThisSession = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            playTimeThisSession += Time.deltaTime;
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame()
        {
            try
            {
                // 备份旧存档
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    PlayerPrefs.SetString(BACKUP_KEY, PlayerPrefs.GetString(SAVE_KEY));
                }

                SaveData saveData = new SaveData
                {
                    version = 1,
                    saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    playerStats = CurrentPlayerStats.Clone(),
                    highestLevel = GetHighestLevel(),
                    totalPlayTime = GetTotalPlayTime() + (int)playTimeThisSession
                };

                // 保存装备数据
                if (EquipmentManager.Instance != null)
                {
                    var equipData = EquipmentManager.Instance.GetSaveData();
                    saveData.equippedItems = SerializableDictionary<string, string>.FromDictionary(equipData);
                }

                // 保存背包数据
                if (InventoryManager.Instance != null)
                {
                    var invData = InventoryManager.Instance.GetSaveData();
                    saveData.inventory = SerializableDictionary<string, int>.FromDictionary(invData);
                }

                string json = JsonUtility.ToJson(saveData, true);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();

                Debug.Log($"[SaveSystem] 游戏已保存: {saveData.saveTime}");
                OnSaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 保存失败: {e.Message}");
            }
        }

        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                    if (saveData != null && saveData.playerStats != null)
                    {
                        CurrentPlayerStats = saveData.playerStats;

                        // 加载装备数据
                        if (saveData.equippedItems != null && EquipmentManager.Instance != null)
                        {
                            EquipmentManager.Instance.LoadSaveData(saveData.equippedItems.ToDictionary());
                        }

                        // 加载背包数据
                        if (saveData.inventory != null && InventoryManager.Instance != null)
                        {
                            InventoryManager.Instance.LoadSaveData(saveData.inventory.ToDictionary());
                        }

                        Debug.Log($"[SaveSystem] 游戏已加载: 等级{CurrentPlayerStats.level}, 金币{CurrentPlayerStats.gold}");
                        OnLoadCompleted?.Invoke();
                        return;
                    }
                }

                // 没有存档，创建新存档
                CreateNewSave();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 加载失败: {e.Message}，尝试恢复备份");
                TryRestoreBackup();
            }
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        public void CreateNewSave()
        {
            CurrentPlayerStats = PlayerStats.CreateDefault();
            playTimeThisSession = 0f;
            Debug.Log("[SaveSystem] 创建新存档");
            SaveGame();
        }

        /// <summary>
        /// 尝试从备份恢复
        /// </summary>
        private void TryRestoreBackup()
        {
            if (PlayerPrefs.HasKey(BACKUP_KEY))
            {
                PlayerPrefs.SetString(SAVE_KEY, PlayerPrefs.GetString(BACKUP_KEY));
                LoadGame();
            }
            else
            {
                CreateNewSave();
            }
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.DeleteKey(BACKUP_KEY);
            PlayerPrefs.Save();
            CreateNewSave();
            Debug.Log("[SaveSystem] 存档已删除");
        }

        /// <summary>
        /// 获取最高通关关卡
        /// </summary>
        public int GetHighestLevel()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SAVE_KEY));
                    return data?.highestLevel ?? 1;
                }
                catch { }
            }
            return 1;
        }

        /// <summary>
        /// 获取总游戏时间（秒）
        /// </summary>
        public int GetTotalPlayTime()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SAVE_KEY));
                    return data?.totalPlayTime ?? 0;
                }
                catch { }
            }
            return 0;
        }

        /// <summary>
        /// 更新最高通关关卡
        /// </summary>
        public void UpdateHighestLevel(int level)
        {
            if (level > GetHighestLevel())
            {
                Debug.Log($"[SaveSystem] 新纪录! 最高关卡: {level}");
                SaveGame();
            }
        }

        /// <summary>
        /// 检查是否有存档
        /// </summary>
        public bool HasSave()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }
    }
}
