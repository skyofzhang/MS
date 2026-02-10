using UnityEngine;
using System.Collections.Generic;

namespace MoShou.Effects
{
    /// <summary>
    /// VFX特效管理器
    /// 实现策划案RULE-RES-011定义的VFX资源管理
    /// 使用对象池优化性能
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        private static VFXManager _instance;
        public static VFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("VFXManager");
                    _instance = go.AddComponent<VFXManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #region 配置

        [Header("对象池设置")]
        [SerializeField] private int _initialPoolSize = 5;
        [SerializeField] private int _maxPoolSize = 20;

        [Header("VFX预制体路径")]
        [SerializeField] private string _vfxPrefabPath = "Prefabs/VFX/";

        #endregion

        #region 内部数据

        // VFX对象池
        private Dictionary<string, Queue<GameObject>> _vfxPools = new Dictionary<string, Queue<GameObject>>();

        // VFX预制体缓存
        private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

        // 活跃的VFX实例
        private Dictionary<int, VFXHandle> _activeVFX = new Dictionary<int, VFXHandle>();

        private int _handleCounter = 0;

        #endregion

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                PreloadCommonVFX();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 预加载常用VFX
        /// </summary>
        private void PreloadCommonVFX()
        {
            // 预加载策划案RULE-RES-011定义的核心VFX
            string[] commonVFX = new string[]
            {
                "VFX_Hit_Spark",
                "VFX_Arrow_Trail",
                "VFX_LevelUp",
                "VFX_Heal",
                "VFX_Death_Dissolve",
                "VFX_Gold_Pickup"
            };

            foreach (string vfxId in commonVFX)
            {
                PrewarmPool(vfxId, _initialPoolSize);
            }
        }

        #region 公开API

        /// <summary>
        /// 播放VFX特效
        /// </summary>
        /// <param name="vfxId">VFX标识符 (对应Prefab名称)</param>
        /// <param name="position">世界坐标位置</param>
        /// <param name="rotation">旋转 (默认无旋转)</param>
        /// <param name="parent">父物体 (可选)</param>
        /// <returns>VFX句柄，用于手动停止</returns>
        public int PlayVFX(string vfxId, Vector3 position, Quaternion? rotation = null,
            Transform parent = null)
        {
            GameObject vfxInstance = GetFromPool(vfxId);
            if (vfxInstance == null)
            {
                Debug.LogWarning($"[VFXManager] 无法获取VFX: {vfxId}");
                return -1;
            }

            // 设置位置和旋转
            vfxInstance.transform.position = position;
            vfxInstance.transform.rotation = rotation ?? Quaternion.identity;

            // 设置父物体
            if (parent != null)
            {
                vfxInstance.transform.SetParent(parent, true);
            }

            // 激活
            vfxInstance.SetActive(true);

            // 获取ParticleSystem
            ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            // 创建句柄
            int handle = ++_handleCounter;
            VFXHandle vfxHandle = new VFXHandle
            {
                Handle = handle,
                VFXId = vfxId,
                Instance = vfxInstance,
                ParticleSystem = ps,
                StartTime = Time.time
            };

            _activeVFX[handle] = vfxHandle;

            // 如果粒子系统不循环，设置自动回收
            if (ps != null && !ps.main.loop)
            {
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                StartCoroutine(AutoRecycleCoroutine(handle, lifetime));
            }

            return handle;
        }

        /// <summary>
        /// 在目标位置播放VFX (简化版)
        /// </summary>
        public int PlayVFX(string vfxId, Vector3 position)
        {
            return PlayVFX(vfxId, position, null, null);
        }

        /// <summary>
        /// 停止VFX
        /// </summary>
        /// <param name="handle">VFX句柄</param>
        /// <param name="immediate">是否立即停止 (false则等待粒子自然消失)</param>
        public void StopVFX(int handle, bool immediate = false)
        {
            if (!_activeVFX.TryGetValue(handle, out VFXHandle vfxHandle))
            {
                return;
            }

            if (vfxHandle.Instance == null)
            {
                _activeVFX.Remove(handle);
                return;
            }

            if (immediate)
            {
                ReturnToPool(vfxHandle.VFXId, vfxHandle.Instance);
                _activeVFX.Remove(handle);
            }
            else
            {
                // 停止发射但等待现有粒子消失
                if (vfxHandle.ParticleSystem != null)
                {
                    vfxHandle.ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                    float remainingLife = vfxHandle.ParticleSystem.main.startLifetime.constantMax;
                    StartCoroutine(AutoRecycleCoroutine(handle, remainingLife));
                }
                else
                {
                    ReturnToPool(vfxHandle.VFXId, vfxHandle.Instance);
                    _activeVFX.Remove(handle);
                }
            }
        }

        /// <summary>
        /// 停止所有VFX
        /// </summary>
        public void StopAllVFX(bool immediate = true)
        {
            List<int> handles = new List<int>(_activeVFX.Keys);
            foreach (int handle in handles)
            {
                StopVFX(handle, immediate);
            }
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        public void PrewarmPool(string vfxId, int count)
        {
            GameObject prefab = GetPrefab(vfxId);
            if (prefab == null) return;

            if (!_vfxPools.ContainsKey(vfxId))
            {
                _vfxPools[vfxId] = new Queue<GameObject>();
            }

            for (int i = 0; i < count; i++)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.SetActive(false);
                _vfxPools[vfxId].Enqueue(instance);
            }
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void ClearPool(string vfxId = null)
        {
            if (vfxId != null)
            {
                if (_vfxPools.TryGetValue(vfxId, out Queue<GameObject> pool))
                {
                    while (pool.Count > 0)
                    {
                        GameObject obj = pool.Dequeue();
                        if (obj != null) Destroy(obj);
                    }
                }
            }
            else
            {
                foreach (var pool in _vfxPools.Values)
                {
                    while (pool.Count > 0)
                    {
                        GameObject obj = pool.Dequeue();
                        if (obj != null) Destroy(obj);
                    }
                }
                _vfxPools.Clear();
            }
        }

        #endregion

        #region 对象池管理

        private GameObject GetFromPool(string vfxId)
        {
            // 先检查池中是否有可用对象
            if (_vfxPools.TryGetValue(vfxId, out Queue<GameObject> pool) && pool.Count > 0)
            {
                GameObject pooled = pool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            // 池中没有，创建新的
            GameObject prefab = GetPrefab(vfxId);
            if (prefab == null)
            {
                return null;
            }

            GameObject newInstance = Instantiate(prefab, transform);
            newInstance.name = vfxId;
            return newInstance;
        }

        private void ReturnToPool(string vfxId, GameObject instance)
        {
            if (instance == null) return;

            // 重置
            instance.SetActive(false);
            instance.transform.SetParent(transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            // 检查池大小
            if (!_vfxPools.ContainsKey(vfxId))
            {
                _vfxPools[vfxId] = new Queue<GameObject>();
            }

            if (_vfxPools[vfxId].Count < _maxPoolSize)
            {
                _vfxPools[vfxId].Enqueue(instance);
            }
            else
            {
                Destroy(instance);
            }
        }

        private GameObject GetPrefab(string vfxId)
        {
            // 检查缓存
            if (_prefabCache.TryGetValue(vfxId, out GameObject cached))
            {
                return cached;
            }

            // 加载预制体
            string path = _vfxPrefabPath + vfxId;
            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab != null)
            {
                _prefabCache[vfxId] = prefab;
            }
            else
            {
                Debug.LogWarning($"[VFXManager] 找不到VFX预制体: {path}");
            }

            return prefab;
        }

        private System.Collections.IEnumerator AutoRecycleCoroutine(int handle, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_activeVFX.TryGetValue(handle, out VFXHandle vfxHandle))
            {
                if (vfxHandle.Instance != null)
                {
                    ReturnToPool(vfxHandle.VFXId, vfxHandle.Instance);
                }
                _activeVFX.Remove(handle);
            }
        }

        #endregion

        #region 内部类

        private class VFXHandle
        {
            public int Handle;
            public string VFXId;
            public GameObject Instance;
            public ParticleSystem ParticleSystem;
            public float StartTime;
        }

        #endregion

        private void OnDestroy()
        {
            StopAllVFX(true);
            ClearPool();
        }
    }
}
