using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using MoShou.Utils;
using MoShou.UI;
using MoShou.Core;
using System.Collections;

/// <summary>
/// 游戏场景初始化 - 符合知识库§2 RULE-RES-001
/// </summary>
public class GameSceneSetup : MonoBehaviour
{
    private static bool isInitialized = false;

    [Header("Scene Map Settings")]
    [Tooltip("是否使用预制体地图(MAP)替代程序化生成的地形")]
    public bool usePrefabMap = true;  // 启用MAP预制体
    [Tooltip("MAP预制体在Resources中的路径")]
    public string mapPrefabPath = "Models/Environment/BossRoom/MAP";
    [Tooltip("MAP生成位置")]
    public Vector3 mapSpawnPosition = Vector3.zero;
    [Tooltip("MAP缩放比例")]
    public float mapScale = 1.0f;

    // MAP地图边界(根据实际MAP尺寸调整)
    private float mapBoundsX = 25f;
    private float mapBoundsZ = 25f;

    // 玩家生成位置（由CreatePrefabMap计算后设置）
    private Vector3 playerSpawnPosition = new Vector3(0, 1, 0);

    /// <summary>
    /// 自动在游戏启动时注册场景加载回调
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        isInitialized = false;
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
        Debug.Log("[GameSceneSetup] 已注册场景加载回调");
    }

    static void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameSceneSetup] 场景加载: {scene.name}");

        // 只在GameScene中创建
        if (scene.name == "GameScene")
        {
            // 延迟一帧创建，确保场景完全加载
            var temp = new GameObject("_TempLoader");
            temp.AddComponent<DelayedSetup>();
        }
    }

    /// <summary>
    /// 延迟初始化辅助类
    /// </summary>
    private class DelayedSetup : MonoBehaviour
    {
        void Start()
        {
            if (FindObjectOfType<GameSceneSetup>() == null)
            {
                Debug.Log("[GameSceneSetup] 创建GameSceneSetup...");
                var go = new GameObject("GameSceneSetup");
                go.AddComponent<GameSceneSetup>();
            }
            Destroy(gameObject);
        }
    }

    void Awake()
    {
        if (isInitialized)
        {
            Debug.LogWarning("[GameSceneSetup] 已存在实例，销毁重复");
            Destroy(gameObject);
            return;
        }
        isInitialized = true;

        Debug.Log("[GameSceneSetup] Awake - 开始初始化场景");
        SetupScene();
    }

    void OnDestroy()
    {
        if (isInitialized)
        {
            isInitialized = false;
        }
    }
    
    void SetupScene()
    {
        Debug.Log("[GameSceneSetup] SetupScene开始...");

        // 确保有GameManager
        if (GameManager.Instance == null)
        {
            var gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
            Debug.Log("[GameSceneSetup] GameManager已创建");
        }

        // 确保有AudioManager
        if (MoShou.Systems.AudioManager.Instance == null)
        {
            var audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<MoShou.Systems.AudioManager>();
            Debug.Log("[GameSceneSetup] AudioManager已创建");
        }

        // 播放战斗BGM
        StartCoroutine(PlayBGMDelayed());

        // 创建地面（先创建地面，让玩家有地方站）
        // 简化逻辑：强制删除所有旧地形，重新创建
        Debug.Log("[GameSceneSetup] 检查并创建地形...");

        // 删除所有可能存在的旧地形
        string[] terrainNames = { "Terrain", "Terrain_Fallback", "Ground" };
        foreach (string name in terrainNames)
        {
            GameObject oldTerrain = GameObject.Find(name);
            if (oldTerrain != null)
            {
                Debug.Log($"[GameSceneSetup] 删除旧地形: {name}");
                DestroyImmediate(oldTerrain);
            }
        }

        // 强制创建地形
        CreateGround();

        // 安全检查：确保场景中有地面，如果没有则创建简单地面
        StartCoroutine(EnsureGroundExists());

        // 创建主摄像机(如果没有) - 在玩家之前创建
        if (Camera.main == null)
        {
            CreateCamera();
            Debug.Log("[GameSceneSetup] 主摄像机已创建");
        }

        // 创建玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            CreatePlayer();
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // 确保摄像机跟随玩家 - 玩家创建后立即设置
        SetupCameraFollow();

        // 创建怪物生成器
        if (FindObjectOfType<MonsterSpawner>() == null)
        {
            var spawnerGO = new GameObject("MonsterSpawner");
            spawnerGO.AddComponent<MonsterSpawner>();
            Debug.Log("[GameSceneSetup] MonsterSpawner已创建");
        }

        // 创建UI资源绑定器 - 自动应用美术资源到UI组件
        if (UIResourceBinder.Instance == null)
        {
            var binderGO = new GameObject("UIResourceBinder");
            binderGO.AddComponent<UIResourceBinder>();
            Debug.Log("[GameSceneSetup] UIResourceBinder已创建");
        }

        // 创建游戏UI
        if (UIManager.Instance == null)
        {
            CreateGameUI();
        }

        Debug.Log("[GameSceneSetup] Scene setup complete!");

        // 预加载常用音效
        if (MoShou.Systems.AudioManager.Instance != null)
        {
            MoShou.Systems.AudioManager.Instance.PreloadCommonSFX();
        }

        // 通知GameManager场景准备完成，开始游戏！
        if (GameManager.Instance != null)
        {
            // 强制设置为Playing状态
            if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                GameManager.Instance.OnGameSceneReady();
            }
            Debug.Log($"[GameSceneSetup] 游戏状态: {GameManager.Instance.CurrentState}");
        }
        else
        {
            // 如果GameManager不存在，创建一个
            Debug.LogWarning("[GameSceneSetup] GameManager.Instance为null，尝试创建...");
            var gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();

            // 等待一帧后再设置状态
            StartCoroutine(DelayedStartGame());
        }

        // 验证摄像机跟随设置
        VerifyCameraSetup();
    }

    /// <summary>
    /// 延迟启动游戏（等待GameManager初始化）
    /// </summary>
    IEnumerator DelayedStartGame()
    {
        yield return null; // 等待一帧

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameSceneReady();
            Debug.Log($"[GameSceneSetup] 延迟启动游戏，状态: {GameManager.Instance.CurrentState}");
        }
    }

    /// <summary>
    /// 延迟播放BGM
    /// </summary>
    IEnumerator PlayBGMDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (MoShou.Systems.AudioManager.Instance != null)
        {
            MoShou.Systems.AudioManager.Instance.PlayBGM(MoShou.Systems.AudioManager.BGM.BattleNormal);
            Debug.Log("[GameSceneSetup] 战斗BGM开始播放");
        }
    }

    /// <summary>
    /// 验证摄像机设置是否正确
    /// </summary>
    void VerifyCameraSetup()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[GameSceneSetup] 验证失败: 未找到主摄像机!");
            return;
        }

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
        {
            Debug.LogError("[GameSceneSetup] 验证失败: 摄像机没有CameraFollow组件!");
            return;
        }

        if (follow.target == null)
        {
            Debug.LogWarning("[GameSceneSetup] 验证警告: CameraFollow.target为null，尝试重新设置...");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                follow.target = player.transform;
                Debug.Log("[GameSceneSetup] 已重新设置摄像机目标: " + player.name);
            }
        }
        else
        {
            Debug.Log($"[GameSceneSetup] 验证成功: 摄像机正在跟随 {follow.target.name}");
        }
    }

    void SetupCameraFollow()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[GameSceneSetup] 未找到主摄像机!");
            return;
        }

        // 确保摄像机有CameraFollow组件
        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = cam.gameObject.AddComponent<CameraFollow>();
            Debug.Log("[GameSceneSetup] 添加CameraFollow组件到摄像机");
        }

        // 查找玩家并设置跟随
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            follow.target = player.transform;

            // 立即设置摄像机位置
            cam.transform.position = player.transform.position + follow.offset;
            cam.transform.LookAt(player.transform.position + Vector3.up * 1.5f);

            Debug.Log("[GameSceneSetup] 摄像机跟随玩家设置完成，玩家位置: " + player.transform.position);
        }
        else
        {
            Debug.LogWarning("[GameSceneSetup] 未找到玩家，摄像机跟随将延迟初始化");
        }
    }
    
    void CreatePlayer()
    {
        // 知识库§2 RULE-RES-001: 加载玩家模型
        GameObject player = null;

        // 尝试多个路径加载
        string[] prefabPaths = {
            "Prefabs/Characters/Player_Archer",
            "Prefabs/Characters/human/120033_ain_LOD1",  // everclan人类模型
            "Models/Player/Player_Archer"
        };

        GameObject prefab = null;
        foreach (string path in prefabPaths)
        {
            prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                Debug.Log($"[GameSceneSetup] 找到玩家模型: {path}");
                break;
            }
            else
            {
                Debug.Log($"[GameSceneSetup] 尝试加载失败: {path}");
            }
        }

        if (prefab != null)
        {
            player = Instantiate(prefab, playerSpawnPosition, Quaternion.identity);
            player.name = "Player";
            // 恢复模型原始大小
            player.transform.localScale = Vector3.one * 1f;

            // 确保材质正确（修复URP粉色问题）
            FixMaterials(player);

            Debug.Log($"[GameSceneSetup] 玩家模型加载成功! 生成位置: {playerSpawnPosition}");
        }
        else
        {
            // FALLBACK: 使用可见的胶囊体
            Debug.LogWarning("[GameSceneSetup] 所有玩家模型路径都失败，使用FALLBACK");
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_FALLBACK";
            player.transform.position = playerSpawnPosition;
            // 恢复模型原始大小
            player.transform.localScale = Vector3.one * 1f;

            // 使用URP兼容的材质
            var renderer = player.GetComponent<Renderer>();
            renderer.material = CreateURPMaterial(Color.blue);
        }

        player.tag = "Player";
        player.layer = 8; // Layer 8 = Player

        // 设置所有子对象的Layer
        foreach (Transform child in player.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = 8;
        }

        // 确保有PlayerController
        if (player.GetComponent<PlayerController>() == null)
        {
            player.AddComponent<PlayerController>();
        }

        // 确保有CharacterController
        if (player.GetComponent<CharacterController>() == null)
        {
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0, 1, 0);
        }
    }

    /// <summary>
    /// 修复材质（解决URP粉色问题）
    /// </summary>
    void FixMaterials(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.sharedMaterial == null || renderer.sharedMaterial.shader.name.Contains("Error"))
            {
                renderer.material = CreateURPMaterial(Color.gray);
            }
        }
    }

    /// <summary>
    /// 创建URP兼容材质
    /// </summary>
    Material CreateURPMaterial(Color color)
    {
        // 方法1: 尝试加载预设的URP材质模板
        Material templateMat = Resources.Load<Material>("Materials/URP_Template");
        if (templateMat != null)
        {
            Material newMat1 = new Material(templateMat);
            SetMaterialColor(newMat1, color);
            return newMat1;
        }

        // 方法2: 尝试从Ground_Grass材质复制shader
        Material grassMat = Resources.Load<Material>("Materials/Ground_Grass");
        if (grassMat != null && grassMat.shader != null)
        {
            Material newMat2 = new Material(grassMat.shader);
            SetMaterialColor(newMat2, color);
            Debug.Log($"[GameSceneSetup] 使用Ground_Grass的Shader: {grassMat.shader.name}");
            return newMat2;
        }

        // 方法3: 尝试从现有Renderer获取shader
        Renderer existingRenderer = FindObjectOfType<Renderer>();
        if (existingRenderer != null && existingRenderer.sharedMaterial != null &&
            existingRenderer.sharedMaterial.shader != null &&
            !existingRenderer.sharedMaterial.shader.name.Contains("Error"))
        {
            Material newMat3 = new Material(existingRenderer.sharedMaterial.shader);
            SetMaterialColor(newMat3, color);
            Debug.Log($"[GameSceneSetup] 使用现有Shader: {existingRenderer.sharedMaterial.shader.name}");
            return newMat3;
        }

        // 方法4: 尝试多种Shader名称
        string[] shaderNames = {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit",
            "Sprites/Default"
        };

        Shader shader = null;
        foreach (string name in shaderNames)
        {
            shader = Shader.Find(name);
            if (shader != null && !shader.name.Contains("Error"))
            {
                Debug.Log($"[GameSceneSetup] 使用Shader: {name}");
                break;
            }
        }

        // 方法5: 使用默认Sprite材质
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            Debug.LogError("[GameSceneSetup] 无法找到任何可用Shader!");
            return new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        Material finalMat = new Material(shader);
        SetMaterialColor(finalMat, color);
        return finalMat;
    }

    /// <summary>
    /// 设置材质颜色（兼容多种shader）
    /// </summary>
    void SetMaterialColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color); // URP使用_BaseColor
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color); // Standard/Sprites使用_Color
        }
        if (mat.HasProperty("_TintColor"))
        {
            mat.SetColor("_TintColor", color);
        }
        // 尝试设置主颜色
        try { mat.color = color; } catch { }
    }

    void CreateGround()
    {
        // 调试日志：显示当前配置
        Debug.Log($"[GameSceneSetup] CreateGround - usePrefabMap={usePrefabMap}, path={mapPrefabPath}");

        // 根据配置决定使用预制体地图还是程序化生成
        if (usePrefabMap)
        {
            CreatePrefabMap();
        }
        else
        {
            Debug.Log("[GameSceneSetup] 使用程序化地形（usePrefabMap=false）");
            CreateProceduralGround();
        }
    }

    /// <summary>
    /// 加载并实例化MAP预制体 - BOSS房场景
    /// 关键：移动MAP使Terrain中心位于世界原点(0,0,0)
    /// </summary>
    void CreatePrefabMap()
    {
        // 尝试加载MAP预制体
        GameObject mapPrefab = Resources.Load<GameObject>(mapPrefabPath);

        if (mapPrefab == null)
        {
            Debug.LogWarning($"[GameSceneSetup] MAP预制体未找到: {mapPrefabPath}, 使用程序化地形");
            CreateProceduralGround();
            return;
        }

        // 实例化MAP到原点
        GameObject mapInstance = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        mapInstance.name = "Terrain";
        mapInstance.transform.localScale = Vector3.one * mapScale;

        // 获取Terrain组件
        Terrain instanceTerrain = mapInstance.GetComponentInChildren<Terrain>();
        if (instanceTerrain != null && instanceTerrain.terrainData != null)
        {
            // 获取Terrain的本地位置和尺寸
            Vector3 terrainLocalPos = instanceTerrain.transform.localPosition;  // 例如 (-84.3, 0, -81.4)
            Vector3 terrainSize = instanceTerrain.terrainData.size;              // 例如 (100, x, 100)

            Debug.Log($"[GameSceneSetup] Terrain本地位置: {terrainLocalPos}, 尺寸: {terrainSize}");

            // 计算Terrain中心的本地坐标
            // Terrain的position是左下角，中心 = localPos + size/2
            float terrainCenterLocalX = terrainLocalPos.x + terrainSize.x / 2f;
            float terrainCenterLocalZ = terrainLocalPos.z + terrainSize.z / 2f;

            // ★★★ 关键修复：移动MAP使Terrain中心位于世界原点 ★★★
            // MAP的位置 = -Terrain中心的本地坐标
            mapInstance.transform.position = new Vector3(-terrainCenterLocalX, 0, -terrainCenterLocalZ);

            Debug.Log($"[GameSceneSetup] MAP已移动到: {mapInstance.transform.position}，Terrain中心现在在世界原点");

            // 现在Terrain中心在世界原点(0,0,0)
            // 玩家生成在(0, 地面高度, 0)
            float groundY = GetGroundHeightAtPosition(0, 0);
            playerSpawnPosition = new Vector3(0, groundY + 1f, 0);

            // 边界基于Terrain尺寸（留5单位边距）
            mapBoundsX = terrainSize.x / 2f - 5f;
            mapBoundsZ = terrainSize.z / 2f - 5f;

            Debug.Log($"[GameSceneSetup] 玩家生成位置: {playerSpawnPosition}, 边界: ±{mapBoundsX}");
        }
        else
        {
            Debug.LogWarning("[GameSceneSetup] MAP中未找到有效Terrain组件，使用默认设置");
            playerSpawnPosition = new Vector3(0, 1, 0);
            mapBoundsX = 45f;
            mapBoundsZ = 45f;
        }

        // 设置Layer (Layer 11 = Environment)
        SetLayerRecursively(mapInstance, 11);

        // 确保有地面碰撞体
        EnsureGroundColliders(mapInstance);

        // 创建边界墙（现在位置相对于世界原点，正确）
        CreateBoundaryWalls(mapInstance.transform, mapBoundsX);
        Debug.Log($"[GameSceneSetup] 边界墙已创建在 ±{mapBoundsX} 位置");

        // 更新怪物生成器边界（简化版，直接用计算好的边界）
        UpdateSpawnerBoundsSimple();

        // 设置专业级灯光系统
        SetupProfessionalLighting();

        Debug.Log($"[GameSceneSetup] MAP预制体加载成功，坐标系已统一到原点");
    }

    /// <summary>
    /// 获取指定位置的地面高度
    /// </summary>
    float GetGroundHeightAtPosition(float x, float z)
    {
        RaycastHit hit;
        Vector3 rayStart = new Vector3(x, 100f, z);
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f))
        {
            return hit.point.y;
        }

        // 备用：尝试使用活动Terrain
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null && terrain.terrainData != null)
        {
            return terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
        }

        return 0f;
    }

    /// <summary>
    /// 简化版更新怪物生成边界
    /// </summary>
    void UpdateSpawnerBoundsSimple()
    {
        var spawner = FindObjectOfType<MonsterSpawner>();
        if (spawner != null)
        {
            spawner.terrainMinX = -mapBoundsX;
            spawner.terrainMaxX = mapBoundsX;
            spawner.terrainMinZ = -mapBoundsZ;
            spawner.terrainMaxZ = mapBoundsZ;
            Debug.Log($"[GameSceneSetup] 怪物生成边界: X({-mapBoundsX} to {mapBoundsX}), Z({-mapBoundsZ} to {mapBoundsZ})");
        }
    }

    /// <summary>
    /// 程序化生成地形（原有逻辑）
    /// </summary>
    void CreateProceduralGround()
    {
        // 创建完整的游戏地形
        GameObject terrainParent = new GameObject("Terrain");
        terrainParent.layer = 11;

        // 1. 创建大地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.parent = terrainParent.transform;
        ground.transform.localScale = new Vector3(10, 1, 10); // 100x100单位
        ground.layer = 11;

        // 地面材质 - 尝试加载预设材质
        var groundRenderer = ground.GetComponent<Renderer>();
        Material grassMat = Resources.Load<Material>("Materials/Ground_Grass");
        if (grassMat != null)
        {
            groundRenderer.material = grassMat;
            Debug.Log("[GameSceneSetup] 使用预设草地材质");
        }
        else
        {
            groundRenderer.material = CreateURPMaterial(new Color(0.25f, 0.45f, 0.2f));
        }

        // 2. 创建边界墙（透明但有碰撞）
        CreateBoundaryWalls(terrainParent.transform, 50f);

        // 3. 创建装饰物
        CreateDecorations(terrainParent.transform);

        // 4. 设置专业级灯光系统
        SetupProfessionalLighting();

        Debug.Log("[GameSceneSetup] 程序化地形创建完成 (100x100)");
    }

    /// <summary>
    /// 设置专业级灯光系统（会清理旧灯光并创建新的）
    /// </summary>
    void SetupProfessionalLighting()
    {
        // 清理场景中已存在的灯光（避免重复）
        Light[] existingLights = FindObjectsOfType<Light>();
        foreach (var light in existingLights)
        {
            // 只删除场景灯光，保留UI或特效灯光
            if (light.gameObject.name.Contains("Light") ||
                light.gameObject.name.Contains("Directional") ||
                light.gameObject.name.Contains("Sun"))
            {
                Debug.Log($"[GameSceneSetup] 移除旧灯光: {light.gameObject.name}");
                Destroy(light.gameObject);
            }
        }

        // 创建新的专业级灯光
        CreateDirectionalLight();
    }

    /// <summary>
    /// 递归设置所有子物体的Layer
    /// </summary>
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// 确保MAP有地面碰撞体
    /// </summary>
    void EnsureGroundColliders(GameObject map)
    {
        bool hasGroundCollider = false;

        foreach (var collider in map.GetComponentsInChildren<Collider>())
        {
            string objName = collider.gameObject.name.ToLower();
            if (objName.Contains("ground") || objName.Contains("floor") ||
                objName.Contains("terrain") || objName.Contains("plane"))
            {
                hasGroundCollider = true;
                Debug.Log($"[GameSceneSetup] 找到地面碰撞体: {collider.gameObject.name}");
                break;
            }
        }

        if (!hasGroundCollider)
        {
            // 检查是否有任何MeshCollider
            var meshColliders = map.GetComponentsInChildren<MeshCollider>();
            if (meshColliders.Length > 0)
            {
                hasGroundCollider = true;
                Debug.Log($"[GameSceneSetup] 使用现有MeshCollider作为地面 ({meshColliders.Length}个)");
            }
        }

        if (!hasGroundCollider)
        {
            // 添加大型地面碰撞体作为后备
            GameObject groundCollider = new GameObject("GroundCollider");
            groundCollider.transform.parent = map.transform;
            groundCollider.transform.localPosition = new Vector3(0, -0.5f, 0);

            BoxCollider box = groundCollider.AddComponent<BoxCollider>();
            box.size = new Vector3(mapBoundsX * 2, 1f, mapBoundsZ * 2);
            groundCollider.layer = 11;

            Debug.Log("[GameSceneSetup] 为MAP添加了后备地面碰撞体");
        }
    }

    /// <summary>
    /// 当TerrainData缺失时，创建替代地面（可见+可碰撞）
    /// </summary>
    void CreateReplacementGround(Transform parent)
    {
        // 创建一个大的平面作为地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "ReplacementGround";
        ground.transform.SetParent(parent, false);
        ground.transform.localPosition = new Vector3(0, 0, 0);
        ground.transform.localScale = new Vector3(5f, 1f, 5f); // 50x50单位

        // 设置Layer
        ground.layer = 11;

        // 创建草地材质
        var renderer = ground.GetComponent<Renderer>();
        Material grassMat = Resources.Load<Material>("Materials/Ground_Grass");
        if (grassMat != null)
        {
            renderer.material = grassMat;
        }
        else
        {
            // 创建URP兼容的绿色材质
            renderer.material = CreateURPMaterial(new Color(0.25f, 0.45f, 0.2f));
        }

        Debug.Log("[GameSceneSetup] 创建了替代地面（TerrainData缺失的后备方案）");
    }

    /// <summary>
    /// 检查MAP是否有边界碰撞体
    /// </summary>
    bool HasBoundaryColliders(GameObject map)
    {
        foreach (var collider in map.GetComponentsInChildren<Collider>())
        {
            string name = collider.gameObject.name.ToLower();
            if (name.Contains("wall") || name.Contains("boundary") ||
                name.Contains("border") || name.Contains("barrier"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 根据MAP尺寸更新怪物生成器的边界
    /// </summary>
    void UpdateSpawnerBounds(GameObject map)
    {
        // 计算MAP的包围盒
        Bounds bounds = new Bounds(map.transform.position, Vector3.zero);

        foreach (var renderer in map.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // 更新边界变量
        mapBoundsX = Mathf.Max(bounds.extents.x, 10f);
        mapBoundsZ = Mathf.Max(bounds.extents.z, 10f);

        // 更新MonsterSpawner
        var spawner = FindObjectOfType<MonsterSpawner>();
        if (spawner != null)
        {
            float margin = 5f;
            spawner.terrainMinX = bounds.min.x + margin;
            spawner.terrainMaxX = bounds.max.x - margin;
            spawner.terrainMinZ = bounds.min.z + margin;
            spawner.terrainMaxZ = bounds.max.z - margin;

            Debug.Log($"[GameSceneSetup] 更新怪物生成边界: X({spawner.terrainMinX:F1} to {spawner.terrainMaxX:F1}), Z({spawner.terrainMinZ:F1} to {spawner.terrainMaxZ:F1})");
        }
        else
        {
            Debug.Log($"[GameSceneSetup] MAP边界计算完成: X(±{mapBoundsX:F1}), Z(±{mapBoundsZ:F1})");
        }
    }

    void CreateBoundaryWalls(Transform parent, float size)
    {
        float wallHeight = 5f;
        float wallThickness = 1f;
        float halfSize = size;

        // 创建边界墙容器（使用世界坐标，不受parent位置影响）
        GameObject wallContainer = new GameObject("BoundaryWalls");
        wallContainer.transform.position = Vector3.zero;  // 固定在世界原点
        wallContainer.transform.rotation = Quaternion.identity;

        // 四面墙 - 使用世界坐标，围绕原点
        CreateWallWorld(wallContainer.transform, "WallNorth", new Vector3(0, wallHeight/2, halfSize), new Vector3(halfSize*2, wallHeight, wallThickness));
        CreateWallWorld(wallContainer.transform, "WallSouth", new Vector3(0, wallHeight/2, -halfSize), new Vector3(halfSize*2, wallHeight, wallThickness));
        CreateWallWorld(wallContainer.transform, "WallEast", new Vector3(halfSize, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, halfSize*2));
        CreateWallWorld(wallContainer.transform, "WallWest", new Vector3(-halfSize, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, halfSize*2));

        Debug.Log($"[GameSceneSetup] 边界墙创建完成: N(0,{halfSize}) S(0,{-halfSize}) E({halfSize},0) W({-halfSize},0)");
    }

    void CreateWallWorld(Transform parent, string name, Vector3 worldPos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.parent = parent;
        wall.transform.position = worldPos;  // 使用世界坐标
        wall.transform.localScale = scale;
        wall.layer = 11;
        wall.GetComponent<Renderer>().enabled = false; // 不可见
    }

    void CreateDecorations(Transform parent)
    {
        GameObject decoParent = new GameObject("Decorations");
        decoParent.transform.parent = parent;

        // 随机生成树木
        for (int i = 0; i < 15; i++)
        {
            Vector3 pos = GetRandomTerrainPosition(45f, 8f);
            CreateTree(decoParent.transform, pos);
        }

        // 随机生成石头
        for (int i = 0; i < 10; i++)
        {
            Vector3 pos = GetRandomTerrainPosition(45f, 5f);
            CreateRock(decoParent.transform, pos);
        }
    }

    Vector3 GetRandomTerrainPosition(float maxDist, float minDist)
    {
        float x, z;
        do {
            x = Random.Range(-maxDist, maxDist);
            z = Random.Range(-maxDist, maxDist);
        } while (Mathf.Abs(x) < minDist && Mathf.Abs(z) < minDist);
        return new Vector3(x, 0, z);
    }

    void CreateTree(Transform parent, Vector3 pos)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.parent = parent;
        tree.transform.position = pos;
        tree.layer = 11;

        // 树干
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, 1.5f, 0);
        trunk.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
        trunk.GetComponent<Renderer>().material = CreateURPMaterial(new Color(0.4f, 0.25f, 0.1f));
        trunk.layer = 11;

        // 树冠
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.transform.parent = tree.transform;
        crown.transform.localPosition = new Vector3(0, 4f, 0);
        crown.transform.localScale = new Vector3(3f, 3f, 3f);
        crown.GetComponent<Renderer>().material = CreateURPMaterial(new Color(0.15f, 0.4f, 0.15f));
        crown.layer = 11;

        // 碰撞体
        CapsuleCollider col = tree.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 2f, 0);
        col.radius = 0.5f;
        col.height = 4f;
    }

    void CreateRock(Transform parent, Vector3 pos)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        rock.transform.parent = parent;
        rock.transform.position = pos;
        float scale = Random.Range(0.5f, 1.2f);
        rock.transform.localScale = new Vector3(scale * 1.5f, scale * 0.7f, scale);
        rock.GetComponent<Renderer>().material = CreateURPMaterial(new Color(0.45f, 0.45f, 0.45f));
        rock.layer = 11;
    }

    void CreateDirectionalLight()
    {
        // 创建专业级3A游戏灯光系统
        // 参考：原神、崩坏星穹铁道、王者荣耀的户外场景灯光

        // === 1. 主光源（太阳光）===
        GameObject mainLightGO = new GameObject("Main Light (Sun)");
        Light mainLight = mainLightGO.AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.intensity = 1.5f;  // 主光强度
        // 暖色阳光，带轻微橙色调
        mainLight.color = new Color(1f, 0.96f, 0.88f);
        mainLight.shadows = LightShadows.Soft;
        mainLight.shadowStrength = 0.6f;  // 柔和阴影
        mainLight.shadowBias = 0.05f;
        mainLight.shadowNormalBias = 0.4f;
        // 太阳角度：45度俯角，稍微偏西
        mainLightGO.transform.rotation = Quaternion.Euler(45f, 30f, 0f);

        // === 2. 补光（Fill Light）===
        // 模拟天空散射光，减少暗部死黑
        GameObject fillLightGO = new GameObject("Fill Light (Sky)");
        Light fillLight = fillLightGO.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.4f;  // 较弱的补光
        // 冷色天光，带轻微蓝色调
        fillLight.color = new Color(0.75f, 0.85f, 1f);
        fillLight.shadows = LightShadows.None;  // 补光不产生阴影
        // 从相反方向照射
        fillLightGO.transform.rotation = Quaternion.Euler(135f, -150f, 0f);
        fillLightGO.transform.parent = mainLightGO.transform;

        // === 3. 环境光增强 ===
        // 设置环境光颜色（如果有RenderSettings）
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.45f);  // 柔和的环境光
        RenderSettings.ambientIntensity = 1.2f;

        // === 4. 雾效设置（增加层次感）===
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.6f, 0.65f, 0.75f, 1f);  // 淡蓝灰色雾
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 80f;

        Debug.Log("[GameSceneSetup] 专业级灯光系统已创建：主光+补光+环境光+雾效");
    }

    /// <summary>
    /// 获取默认字体（兼容不同Unity版本）
    /// </summary>
    Font GetDefaultFont()
    {
        // 尝试多种字体名称
        string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
        foreach (string fontName in fontNames)
        {
            Font font = Resources.GetBuiltinResource<Font>(fontName);
            if (font != null) return font;
        }
        // 最后尝试Font.CreateDynamicFontFromOSFont
        return Font.CreateDynamicFontFromOSFont("Arial", 14);
    }

    /// <summary>
    /// 创建游戏UI (HUD、胜利/失败面板)
    /// 依据策划案: 竖屏 1080x1920
    /// </summary>
    void CreateGameUI()
    {
        // 确保有EventSystem（UI点击必需）
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[GameSceneSetup] EventSystem已创建");
        }

        // 销毁所有现有的低优先级Canvas，强制使用新UI
        // 使用DestroyImmediate确保旧UI立即被移除
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
        foreach (var oldCanvas in existingCanvases)
        {
            if (oldCanvas.sortingOrder < 500)
            {
                Debug.Log($"[GameSceneSetup] 立即销毁现有Canvas: {oldCanvas.gameObject.name}");
                DestroyImmediate(oldCanvas.gameObject);
            }
        }

        // 创建Canvas
        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // 配置CanvasScaler用于移动端适配 - 策划案要求竖屏1080x1920
        var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); // 竖屏分辨率
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 添加UIManager
        UIManager uiManager = canvasGO.AddComponent<UIManager>();

        // 创建战斗HUD (Prefab: 全部UI元素)
        CreateBattleHUD(canvasGO.transform, uiManager);

        // 创建背包面板（初始隐藏）
        CreateInventoryPanel(canvasGO.transform);

        // 创建装备面板（初始隐藏）
        CreateEquipmentPanel(canvasGO.transform);

        // 创建商店面板（初始隐藏）
        CreateShopPanel(canvasGO.transform);

        // 创建技能升级面板（初始隐藏）
        CreateSkillUpgradePanel(canvasGO.transform);

        // 创建角色信息面板（初始隐藏，点击小地图时显示）
        CreateCharacterInfoPanel(canvasGO.transform);

        // 创建胜利面板（旧版，保留兼容）
        uiManager.victoryPanel = CreateResultPanel(canvasGO.transform, "VictoryPanel", "胜利!", new Color(0.2f, 0.8f, 0.3f));

        // 创建失败面板（旧版，保留兼容）
        uiManager.defeatPanel = CreateResultPanel(canvasGO.transform, "DefeatPanel", "失败!", new Color(0.8f, 0.2f, 0.2f));

        // 创建新版 ResultScreen（优先使用）
        CreateNewResultScreen(canvasGO.transform);

        // 创建新版 DefeatScreen（优先使用）
        CreateNewDefeatScreen(canvasGO.transform);

        // 暂停面板已在BattleHUD Prefab中，由GameHUD.WireUIManager()绑定

        // 创建小地图（左上角）
        CreateMinimap(canvasGO.transform);

        // 创建 BattleStatsManager
        if (MoShou.Core.BattleStatsManager.Instance == null)
        {
            var statsGO = new GameObject("BattleStatsManager");
            statsGO.AddComponent<MoShou.Core.BattleStatsManager>();
            Debug.Log("[GameSceneSetup] BattleStatsManager已创建");
        }

        Debug.Log("[GameSceneSetup] 游戏UI已创建");
    }

    /// <summary>
    /// 创建新版胜利结算屏幕
    /// </summary>
    void CreateNewResultScreen(Transform parent)
    {
        if (ResultScreen.Instance != null)
        {
            Debug.Log("[GameSceneSetup] ResultScreen已存在");
            return;
        }

        GameObject resultGO = new GameObject("ResultScreen");
        resultGO.transform.SetParent(parent, false);
        resultGO.AddComponent<ResultScreen>();
        resultGO.SetActive(false);

        Debug.Log("[GameSceneSetup] ResultScreen已创建");
    }

    /// <summary>
    /// 创建新版失败屏幕
    /// </summary>
    void CreateNewDefeatScreen(Transform parent)
    {
        if (DefeatScreen.Instance != null)
        {
            Debug.Log("[GameSceneSetup] DefeatScreen已存在");
            return;
        }

        GameObject defeatGO = new GameObject("DefeatScreen");
        defeatGO.transform.SetParent(parent, false);
        defeatGO.AddComponent<DefeatScreen>();
        defeatGO.SetActive(false);

        Debug.Log("[GameSceneSetup] DefeatScreen已创建");
    }

    /// <summary>
    /// 创建小地图
    /// </summary>
    void CreateMinimap(Transform parent)
    {
        // 检查是否已存在
        if (MinimapSystem.Instance != null)
        {
            Debug.Log("[GameSceneSetup] 小地图已存在");
            return;
        }

        // 创建小地图容器
        GameObject minimapGO = new GameObject("MinimapSystem");
        minimapGO.transform.SetParent(parent, false);
        minimapGO.AddComponent<MinimapSystem>();

        Debug.Log("[GameSceneSetup] 小地图已创建");
    }

    /// <summary>
    /// 创建战斗HUD (从Prefab加载: 全部UI元素)
    /// 包含：顶部状态栏 + 左侧快捷按钮(5) + 技能栏 + 虚拟摇杆 + 技能动作按钮 + 暂停按钮 + 暂停面板
    /// </summary>
    void CreateBattleHUD(Transform parent, UIManager uiManager)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/BattleHUD");
        if (prefab == null)
        {
            Debug.LogError("[GameSceneSetup] BattleHUD Prefab未找到! 请先运行 MoShou/创建战斗HUD Prefab/0. 全部生成");
            return;
        }

        GameObject hudGO = Instantiate(prefab, parent);
        hudGO.name = "BattleHUD";

        // 获取GameHUD组件并绑定UIManager引用
        MoShou.UI.GameHUD gameHUD = hudGO.GetComponent<MoShou.UI.GameHUD>();
        if (gameHUD != null)
        {
            gameHUD.WireUIManager(uiManager);
        }

        // SimpleHealthBar绑定
        SimpleHealthBar healthBar = hudGO.GetComponentInChildren<SimpleHealthBar>();
        if (healthBar != null)
        {
            uiManager.simpleHealthBar = healthBar;
        }

        // 暂停面板按钮回调绑定（Prefab中按钮没有onClick，需要运行时绑定）
        Transform pausePanel = hudGO.transform.Find("PausePanel");
        if (pausePanel != null)
        {
            BindPausePanelButtons(pausePanel);
        }

        Debug.Log("[GameSceneSetup] BattleHUD(Prefab)已加载");
    }

    /// <summary>
    /// 绑定暂停面板中的按钮回调
    /// </summary>
    void BindPausePanelButtons(Transform pausePanel)
    {
        Transform content = pausePanel.Find("Content");
        if (content == null) return;

        // 继续游戏按钮
        Transform resumeBtn = content.Find("ResumeButton");
        if (resumeBtn != null)
        {
            var btn = resumeBtn.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
                btn.onClick.AddListener(() => {
                    if (UIManager.Instance != null) UIManager.Instance.OnResumeClick();
                });
        }

        // 重试按钮
        Transform retryBtn = content.Find("RetryButton");
        if (retryBtn != null)
        {
            var btn = retryBtn.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
                btn.onClick.AddListener(() => {
                    if (UIManager.Instance != null) UIManager.Instance.OnRetryClick();
                });
        }

        // 返回主菜单按钮
        Transform menuBtn = content.Find("MenuButton");
        if (menuBtn != null)
        {
            var btn = menuBtn.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
                btn.onClick.AddListener(() => {
                    if (UIManager.Instance != null) UIManager.Instance.OnMainMenuClick();
                });
        }
    }

    GameObject CreateResultPanel(Transform parent, string name, string title, Color bgColor)
    {
        // 加载结算面板资源
        bool isVictory = name.Contains("Victory");
        Sprite panelBgSprite = isVictory
            ? Resources.Load<Sprite>("Sprites/UI/Result/UI_Result_Victory_BG")
            : Resources.Load<Sprite>("Sprites/UI/Result/UI_Result_Defeat_BG");
        Sprite starFilled = Resources.Load<Sprite>("Sprites/UI/Result/UI_Result_Star_Filled");
        Sprite starEmpty = Resources.Load<Sprite>("Sprites/UI/Result/UI_Result_Star_Empty");
        Sprite primaryBtn = Resources.Load<Sprite>("Sprites/UI/Buttons/UI_Btn_Primary_Normal");

        // 面板背景（全屏遮罩）
        GameObject panelGO = new GameObject(name);
        panelGO.transform.SetParent(parent, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image bgImage = panelGO.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // 中心内容框 - 策划案尺寸 anchorMin:[0.05,0.2], anchorMax:[0.95,0.85]
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(panelGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.25f);
        contentRect.anchorMax = new Vector2(0.9f, 0.75f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image contentBg = contentGO.AddComponent<UnityEngine.UI.Image>();
        if (panelBgSprite != null)
        {
            contentBg.sprite = panelBgSprite;
            contentBg.type = UnityEngine.UI.Image.Type.Sliced;
            contentBg.color = Color.white;
        }
        else
        {
            contentBg.color = new Color(bgColor.r * 0.3f, bgColor.g * 0.3f, bgColor.b * 0.3f, 0.95f);
        }

        // 标题 - 策划案: 48px Bold
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(contentGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Text titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
        titleText.text = title;
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = isVictory ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f);
        titleText.font = GetDefaultFont();

        // 星级显示（仅胜利面板）
        if (isVictory)
        {
            GameObject starsGO = new GameObject("Stars");
            starsGO.transform.SetParent(contentGO.transform, false);
            RectTransform starsRect = starsGO.AddComponent<RectTransform>();
            starsRect.anchorMin = new Vector2(0.2f, 0.65f);
            starsRect.anchorMax = new Vector2(0.8f, 0.8f);
            starsRect.offsetMin = Vector2.zero;
            starsRect.offsetMax = Vector2.zero;
            var starsLayout = starsGO.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            starsLayout.childAlignment = TextAnchor.MiddleCenter;
            starsLayout.spacing = 20;
            starsLayout.childForceExpandWidth = false;
            starsLayout.childForceExpandHeight = false;

            for (int i = 0; i < 3; i++)
            {
                GameObject starGO = new GameObject($"Star_{i}");
                starGO.transform.SetParent(starsGO.transform, false);
                var starLayout = starGO.AddComponent<UnityEngine.UI.LayoutElement>();
                starLayout.preferredWidth = 50;
                starLayout.preferredHeight = 50;
                UnityEngine.UI.Image starImg = starGO.AddComponent<UnityEngine.UI.Image>();
                if (starFilled != null)
                    starImg.sprite = starFilled;
                else
                    starImg.color = new Color(1f, 0.85f, 0.2f);
            }
        }

        // 奖励显示区域
        GameObject rewardsGO = new GameObject("Rewards");
        rewardsGO.transform.SetParent(contentGO.transform, false);
        RectTransform rewardsRect = rewardsGO.AddComponent<RectTransform>();
        rewardsRect.anchorMin = new Vector2(0.1f, 0.35f);
        rewardsRect.anchorMax = new Vector2(0.9f, 0.6f);
        rewardsRect.offsetMin = Vector2.zero;
        rewardsRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Text rewardsText = rewardsGO.AddComponent<UnityEngine.UI.Text>();
        rewardsText.text = "经验: +50\n金币: +100";
        rewardsText.fontSize = 24;
        rewardsText.alignment = TextAnchor.MiddleCenter;
        rewardsText.color = Color.white;
        rewardsText.font = GetDefaultFont();

        // 按钮区域
        GameObject buttonsGO = new GameObject("Buttons");
        buttonsGO.transform.SetParent(contentGO.transform, false);
        RectTransform buttonsRect = buttonsGO.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.1f, 0.05f);
        buttonsRect.anchorMax = new Vector2(0.9f, 0.3f);
        buttonsRect.offsetMin = Vector2.zero;
        buttonsRect.offsetMax = Vector2.zero;
        var buttonsLayout = buttonsGO.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonsLayout.spacing = 30;
        buttonsLayout.childForceExpandWidth = false;
        buttonsLayout.childForceExpandHeight = false;

        // 主按钮（下一关/重试）
        CreateResultButton(buttonsGO.transform, isVictory ? "下一关" : "重试",
            isVictory ? new Color(0.3f, 0.7f, 0.3f) : new Color(0.8f, 0.3f, 0.3f),
            primaryBtn, () => {
                if (UIManager.Instance != null) UIManager.Instance.OnRetryClick();
            });

        // 次按钮（返回主城）
        CreateResultButton(buttonsGO.transform, "返回主城",
            new Color(0.4f, 0.4f, 0.5f), null, () => {
                if (UIManager.Instance != null) UIManager.Instance.OnMainMenuClick();
            });

        panelGO.SetActive(false);
        return panelGO;
    }

    /// <summary>
    /// 创建结算面板按钮
    /// </summary>
    void CreateResultButton(Transform parent, string text, Color color, Sprite bgSprite, UnityAction onClick)
    {
        GameObject btnGO = new GameObject($"Btn_{text}");
        btnGO.transform.SetParent(parent, false);
        var layout = btnGO.AddComponent<UnityEngine.UI.LayoutElement>();
        layout.preferredWidth = 160;
        layout.preferredHeight = 55;

        UnityEngine.UI.Image btnImg = btnGO.AddComponent<UnityEngine.UI.Image>();
        if (bgSprite != null)
        {
            btnImg.sprite = bgSprite;
            btnImg.type = UnityEngine.UI.Image.Type.Sliced;
            btnImg.color = color;
        }
        else
        {
            btnImg.color = color;
        }

        UnityEngine.UI.Button btn = btnGO.AddComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(onClick);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Text btnText = textGO.AddComponent<UnityEngine.UI.Text>();
        btnText.text = text;
        btnText.fontSize = 22;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = GetDefaultFont();
    }

    // [已迁移到BattleHUD Prefab] CreateVirtualJoystick - 虚拟摇杆

    // [已迁移到BattleHUD Prefab] CreateSkillButtons - 技能动作按钮

    // [已迁移到BattleHUD Prefab] CreateSkillButtonWithIcon - 辅助方法

    // [已迁移到BattleHUD Prefab] CreateFunctionButtons - 暂停+功能按钮

    // [已迁移到BattleHUD Prefab] CreateSideFunctionButton - 辅助方法


    // [已迁移到BattleHUD Prefab] CreateCircleSprite - 圆形Sprite(现为Editor资源文件)

    // [已迁移到BattleHUD Prefab] CreatePausePanel - 暂停面板

    // [已迁移到BattleHUD Prefab] CreatePauseButton - 暂停面板按钮辅助

    void CreateButton(Transform parent, string name, string text, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        RectTransform btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = pos;
        btnRect.sizeDelta = new Vector2(200, 50);

        UnityEngine.UI.Image btnImage = btnGO.AddComponent<UnityEngine.UI.Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f);

        UnityEngine.UI.Button btn = btnGO.AddComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(onClick);

        // 按钮文字
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Text btnText = textGO.AddComponent<UnityEngine.UI.Text>();
        btnText.text = text;
        btnText.fontSize = 28;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = GetDefaultFont();
    }

    /// <summary>
    /// 创建背包面板
    /// 注意：由于InventoryPanel使用SerializeField引用，运行时动态创建需要使用简化版
    /// </summary>
    void CreateInventoryPanel(Transform parent)
    {
        // 背包面板背景
        GameObject panelGO = new GameObject("InventoryPanel");
        panelGO.transform.SetParent(parent, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image bgImage = panelGO.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // 添加简化版背包组件（不依赖SerializeField）
        SimpleInventoryPanel invPanel = panelGO.AddComponent<SimpleInventoryPanel>();
        // 确保静态实例被设置（Awake可能因为执行顺序问题未被调用）
        SimpleInventoryPanel.Instance = invPanel;
        Debug.Log("[GameSceneSetup] SimpleInventoryPanel.Instance 已手动设置");

        // 内容框
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(panelGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(780, 700);
        UnityEngine.UI.Image contentBg = contentGO.AddComponent<UnityEngine.UI.Image>();
        // 使用Kenney面板背景
        Sprite invPanelBg = UIResourceLoader.PanelBeige;
        if (invPanelBg != null)
        {
            contentBg.sprite = invPanelBg;
            contentBg.type = UnityEngine.UI.Image.Type.Sliced;
            contentBg.color = Color.white;
        }
        else
        {
            contentBg.color = new Color(0.2f, 0.2f, 0.25f);
        }

        // 标题
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(contentGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(200, 50);
        UnityEngine.UI.Text titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
        titleText.text = "背包";
        titleText.fontSize = 32;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.3f, 0.2f, 0.1f); // 棕色文字配浅色面板
        titleText.font = GetDefaultFont();
        UnityEngine.UI.Outline titleOutline = titleGO.AddComponent<UnityEngine.UI.Outline>();
        titleOutline.effectColor = new Color(1f, 1f, 1f, 0.3f);
        titleOutline.effectDistance = new Vector2(1, -1);

        // 关闭按钮 - 使用Kenney方形按钮
        GameObject closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(contentGO.transform, false);
        RectTransform closeRect = closeBtnGO.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-30, -30);
        closeRect.sizeDelta = new Vector2(50, 50);
        UnityEngine.UI.Image closeImg = closeBtnGO.AddComponent<UnityEngine.UI.Image>();
        Sprite closeBtnSprite = UIResourceLoader.ButtonSquareBrown;
        if (closeBtnSprite != null)
        {
            closeImg.sprite = closeBtnSprite;
            closeImg.type = UnityEngine.UI.Image.Type.Sliced;
            closeImg.color = Color.white;
        }
        else
        {
            closeImg.color = new Color(0.8f, 0.2f, 0.2f);
        }
        UnityEngine.UI.Button closeBtn = closeBtnGO.AddComponent<UnityEngine.UI.Button>();
        closeBtn.onClick.AddListener(() => invPanel.Hide());

        // 关闭按钮图标 - 优先使用Kenney X图标
        GameObject closeTextGO = new GameObject("Text");
        closeTextGO.transform.SetParent(closeBtnGO.transform, false);
        RectTransform ctRect = closeTextGO.AddComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero;
        ctRect.anchorMax = Vector2.one;
        ctRect.offsetMin = Vector2.zero;
        ctRect.offsetMax = Vector2.zero;
        Sprite crossIcon = UIResourceLoader.IconCrossBrown;
        if (crossIcon != null)
        {
            UnityEngine.UI.Image crossImg = closeTextGO.AddComponent<UnityEngine.UI.Image>();
            crossImg.sprite = crossIcon;
            crossImg.color = Color.white;
            crossImg.raycastTarget = false;
            // 缩小图标留边距
            ctRect.offsetMin = new Vector2(8, 8);
            ctRect.offsetMax = new Vector2(-8, -8);
        }
        else
        {
            UnityEngine.UI.Text closeText = closeTextGO.AddComponent<UnityEngine.UI.Text>();
            closeText.text = "X";
            closeText.fontSize = 28;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.color = Color.white;
            closeText.font = GetDefaultFont();
        }

        // ScrollView容器（支持滚动）
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(contentGO.transform, false);
        RectTransform scrollViewRect = scrollViewGO.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollViewRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollViewRect.anchoredPosition = new Vector2(0, -20);
        scrollViewRect.sizeDelta = new Vector2(720, 520);

        // 添加ScrollRect组件
        var scrollRect = scrollViewGO.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        // 添加遮罩 - 使用Kenney内嵌面板
        UnityEngine.UI.Image scrollBg = scrollViewGO.AddComponent<UnityEngine.UI.Image>();
        Sprite invInsetBg = UIResourceLoader.PanelInsetBeige;
        if (invInsetBg != null)
        {
            scrollBg.sprite = invInsetBg;
            scrollBg.type = UnityEngine.UI.Image.Type.Sliced;
            scrollBg.color = Color.white;
        }
        else
        {
            scrollBg.color = new Color(0.18f, 0.18f, 0.22f, 0.3f);
        }
        var mask = scrollViewGO.AddComponent<UnityEngine.UI.Mask>();
        mask.showMaskGraphic = true;

        // Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        // 物品格子容器（Content - 在Viewport内）
        GameObject slotsGO = new GameObject("SlotsContainer");
        slotsGO.transform.SetParent(viewportGO.transform, false);
        RectTransform slotsRect = slotsGO.AddComponent<RectTransform>();
        slotsRect.anchorMin = new Vector2(0, 1);
        slotsRect.anchorMax = new Vector2(1, 1);
        slotsRect.pivot = new Vector2(0.5f, 1);
        slotsRect.anchoredPosition = Vector2.zero;
        // 宽度撑满，高度由ContentSizeFitter自适应
        slotsRect.sizeDelta = new Vector2(0, 0);

        // ContentSizeFitter让内容自适应高度
        var sizeFitter = slotsGO.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        sizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        // 添加GridLayoutGroup
        var grid = slotsGO.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        grid.cellSize = new Vector2(130, 130);
        grid.spacing = new Vector2(10, 10);
        grid.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.padding = new RectOffset(5, 5, 5, 5);

        // 设置ScrollRect的content
        scrollRect.content = slotsRect;
        scrollRect.viewport = viewportRect;

        // 设置简化版背包组件的引用
        invPanel.slotsContainer = slotsGO.transform;

        // 创建50个格子（支持更多物品，可滚动查看）
        for (int i = 0; i < 50; i++)
        {
            CreateInventorySlot(slotsGO.transform, i);
        }

        // 金币显示
        GameObject goldGO = new GameObject("GoldDisplay");
        goldGO.transform.SetParent(contentGO.transform, false);
        RectTransform goldRect = goldGO.AddComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0, 0);
        goldRect.anchorMax = new Vector2(0, 0);
        goldRect.anchoredPosition = new Vector2(100, 30);
        goldRect.sizeDelta = new Vector2(200, 40);
        UnityEngine.UI.Text goldText = goldGO.AddComponent<UnityEngine.UI.Text>();
        goldText.text = "金币: 0";
        goldText.fontSize = 24;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = new Color(1f, 0.85f, 0.2f);
        goldText.font = GetDefaultFont();
        invPanel.goldText = goldText;

        // 容量显示
        GameObject capacityGO = new GameObject("CapacityDisplay");
        capacityGO.transform.SetParent(contentGO.transform, false);
        RectTransform capRect = capacityGO.AddComponent<RectTransform>();
        capRect.anchorMin = new Vector2(1, 0);
        capRect.anchorMax = new Vector2(1, 0);
        capRect.anchoredPosition = new Vector2(-100, 30);
        capRect.sizeDelta = new Vector2(150, 40);
        UnityEngine.UI.Text capText = capacityGO.AddComponent<UnityEngine.UI.Text>();
        capText.text = "0/30";
        capText.fontSize = 24;
        capText.alignment = TextAnchor.MiddleRight;
        capText.color = Color.white;
        capText.font = GetDefaultFont();
        invPanel.capacityText = capText;

        // 初始隐藏 - 注意：SetActive(false)后Awake仍会被调用，但Start不会
        panelGO.SetActive(false);

        Debug.Log($"[GameSceneSetup] 背包面板已创建, Instance={SimpleInventoryPanel.Instance != null}");
    }

    /// <summary>
    /// 创建背包格子
    /// </summary>
    void CreateInventorySlot(Transform parent, int index)
    {
        GameObject slotGO = new GameObject($"Slot_{index}");
        slotGO.transform.SetParent(parent, false);

        UnityEngine.UI.Image slotBg = slotGO.AddComponent<UnityEngine.UI.Image>();
        slotBg.color = new Color(0.3f, 0.3f, 0.35f);

        UnityEngine.UI.Button slotBtn = slotGO.AddComponent<UnityEngine.UI.Button>();
        slotBtn.targetGraphic = slotBg;

        // 物品图标（子对象）
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(slotGO.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(5, 5);
        iconRect.offsetMax = new Vector2(-5, -5);
        UnityEngine.UI.Image iconImg = iconGO.AddComponent<UnityEngine.UI.Image>();
        iconImg.color = new Color(1, 1, 1, 0); // 初始透明

        // 数量文字
        GameObject countGO = new GameObject("Count");
        countGO.transform.SetParent(slotGO.transform, false);
        RectTransform countRect = countGO.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(1, 0);
        countRect.anchorMax = new Vector2(1, 0);
        countRect.anchoredPosition = new Vector2(-5, 5);
        countRect.sizeDelta = new Vector2(30, 20);
        UnityEngine.UI.Text countText = countGO.AddComponent<UnityEngine.UI.Text>();
        countText.text = "";
        countText.fontSize = 14;
        countText.alignment = TextAnchor.LowerRight;
        countText.color = Color.white;
        countText.font = GetDefaultFont();
    }

    /// <summary>
    /// 创建装备面板
    /// </summary>
    void CreateEquipmentPanel(Transform parent)
    {
        // 装备面板背景
        GameObject panelGO = new GameObject("EquipmentPanel");
        panelGO.transform.SetParent(parent, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image bgImage = panelGO.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // 内容框
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(panelGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(540, 740);
        UnityEngine.UI.Image contentBg = contentGO.AddComponent<UnityEngine.UI.Image>();
        // 使用Kenney蓝色面板背景（装备面板区分于背包面板）
        Sprite equipPanelBg = UIResourceLoader.PanelBlue;
        if (equipPanelBg != null)
        {
            contentBg.sprite = equipPanelBg;
            contentBg.type = UnityEngine.UI.Image.Type.Sliced;
            contentBg.color = Color.white;
        }
        else
        {
            contentBg.color = new Color(0.2f, 0.2f, 0.25f);
        }

        // 标题
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(contentGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(200, 50);
        UnityEngine.UI.Text titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
        titleText.text = "装备";
        titleText.fontSize = 32;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.font = GetDefaultFont();
        UnityEngine.UI.Outline equipTitleOutline = titleGO.AddComponent<UnityEngine.UI.Outline>();
        equipTitleOutline.effectColor = new Color(0, 0, 0, 0.5f);
        equipTitleOutline.effectDistance = new Vector2(1, -1);

        // 关闭按钮 - 使用Kenney方形按钮
        GameObject closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(contentGO.transform, false);
        RectTransform closeRect = closeBtnGO.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-30, -30);
        closeRect.sizeDelta = new Vector2(50, 50);
        UnityEngine.UI.Image closeImg = closeBtnGO.AddComponent<UnityEngine.UI.Image>();
        Sprite eqCloseBtnSprite = UIResourceLoader.ButtonSquareBlue;
        if (eqCloseBtnSprite != null)
        {
            closeImg.sprite = eqCloseBtnSprite;
            closeImg.type = UnityEngine.UI.Image.Type.Sliced;
            closeImg.color = Color.white;
        }
        else
        {
            closeImg.color = new Color(0.8f, 0.2f, 0.2f);
        }
        UnityEngine.UI.Button closeBtn = closeBtnGO.AddComponent<UnityEngine.UI.Button>();
        closeBtn.onClick.AddListener(() => panelGO.SetActive(false));

        // 关闭按钮图标
        GameObject closeTextGO = new GameObject("Text");
        closeTextGO.transform.SetParent(closeBtnGO.transform, false);
        RectTransform ctRect = closeTextGO.AddComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero;
        ctRect.anchorMax = Vector2.one;
        ctRect.offsetMin = Vector2.zero;
        ctRect.offsetMax = Vector2.zero;
        Sprite eqCrossIcon = UIResourceLoader.IconCrossGrey;
        if (eqCrossIcon != null)
        {
            UnityEngine.UI.Image eqCrossImg = closeTextGO.AddComponent<UnityEngine.UI.Image>();
            eqCrossImg.sprite = eqCrossIcon;
            eqCrossImg.color = Color.white;
            eqCrossImg.raycastTarget = false;
            ctRect.offsetMin = new Vector2(8, 8);
            ctRect.offsetMax = new Vector2(-8, -8);
        }
        else
        {
            UnityEngine.UI.Text closeText = closeTextGO.AddComponent<UnityEngine.UI.Text>();
            closeText.text = "X";
            closeText.fontSize = 28;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.color = Color.white;
            closeText.font = GetDefaultFont();
        }

        // ★ 专用装备槽容器（避免与Title/CloseButton/Stats混在一起导致索引错乱）
        GameObject slotsContainerGO = new GameObject("SlotsContainer");
        slotsContainerGO.transform.SetParent(contentGO.transform, false);
        RectTransform slotsContainerRect = slotsContainerGO.AddComponent<RectTransform>();
        slotsContainerRect.anchorMin = new Vector2(0, 0.15f);
        slotsContainerRect.anchorMax = new Vector2(1, 0.88f);
        slotsContainerRect.offsetMin = new Vector2(15, 10);
        slotsContainerRect.offsetMax = new Vector2(-15, -50);

        // 装备槽位 - 放入专用容器
        string[] slotNames = { "武器", "护甲", "头盔", "护腿", "戒指", "项链" };
        for (int i = 0; i < slotNames.Length; i++)
        {
            CreateEquipmentSlot(slotsContainerGO.transform, slotNames[i], i);
        }

        // 属性显示
        GameObject statsGO = new GameObject("Stats");
        statsGO.transform.SetParent(contentGO.transform, false);
        RectTransform statsRect = statsGO.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0);
        statsRect.anchorMax = new Vector2(1, 0);
        statsRect.anchoredPosition = new Vector2(0, 55);
        statsRect.sizeDelta = new Vector2(0, 90);
        UnityEngine.UI.Text statsText = statsGO.AddComponent<UnityEngine.UI.Text>();
        statsText.text = "攻击: +0\n防御: +0\n生命: +0";
        statsText.fontSize = 22;
        statsText.alignment = TextAnchor.MiddleCenter;
        statsText.color = Color.white;
        statsText.font = GetDefaultFont();

        // 添加SimpleEquipmentPanel组件
        MoShou.UI.SimpleEquipmentPanel equipPanel = panelGO.AddComponent<MoShou.UI.SimpleEquipmentPanel>();
        MoShou.UI.SimpleEquipmentPanel.Instance = equipPanel;

        // ★ 关键：使用专用装备槽容器（不再是contentGO，避免Title/CloseButton被当成装备槽）
        equipPanel.slotsContainer = slotsContainerGO.transform;
        equipPanel.statsText = statsText;

        // 初始隐藏
        panelGO.SetActive(false);

        Debug.Log($"[GameSceneSetup] 装备面板已创建, 装备槽数={slotsContainerGO.transform.childCount}, Instance={MoShou.UI.SimpleEquipmentPanel.Instance != null}");
    }

    /// <summary>
    /// 创建装备槽位
    /// </summary>
    void CreateEquipmentSlot(Transform parent, string slotName, int index)
    {
        int row = index / 2;  // 0,0,1,1,2,2
        int col = index % 2;  // 0,1,0,1,0,1

        GameObject slotGO = new GameObject($"EquipSlot_{slotName}");
        slotGO.transform.SetParent(parent, false);
        RectTransform slotRect = slotGO.AddComponent<RectTransform>();
        // 使用相对于容器的定位（容器已经在正确位置）
        slotRect.anchorMin = new Vector2(0.5f, 1);
        slotRect.anchorMax = new Vector2(0.5f, 1);
        slotRect.anchoredPosition = new Vector2(-115 + col * 230, -8 - row * 120);
        slotRect.sizeDelta = new Vector2(220, 110);

        UnityEngine.UI.Image slotBg = slotGO.AddComponent<UnityEngine.UI.Image>();
        // 使用RPGKit通用框架作为默认槽位背景
        Sprite eqSlotFrame = UIResourceLoader.FrameRarityCommon;
        if (eqSlotFrame != null)
        {
            slotBg.sprite = eqSlotFrame;
            slotBg.type = UnityEngine.UI.Image.Type.Simple;
            slotBg.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        }
        else
        {
            slotBg.color = new Color(0.3f, 0.3f, 0.35f);
        }

        // 槽位类型标签（左上角小标签）
        GameObject labelGO = new GameObject("SlotLabel");
        labelGO.transform.SetParent(slotGO.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.anchoredPosition = new Vector2(25, -10);
        labelRect.sizeDelta = new Vector2(50, 20);
        UnityEngine.UI.Image labelBg = labelGO.AddComponent<UnityEngine.UI.Image>();
        labelBg.color = new Color(0.4f, 0.4f, 0.5f, 0.7f);
        labelBg.raycastTarget = false;

        GameObject labelTextGO = new GameObject("Text");
        labelTextGO.transform.SetParent(labelGO.transform, false);
        RectTransform ltRect = labelTextGO.AddComponent<RectTransform>();
        ltRect.anchorMin = Vector2.zero;
        ltRect.anchorMax = Vector2.one;
        ltRect.offsetMin = Vector2.zero;
        ltRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Text labelText = labelTextGO.AddComponent<UnityEngine.UI.Text>();
        labelText.text = slotName;
        labelText.fontSize = 15;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = new Color(0.7f, 0.7f, 0.7f);
        labelText.font = GetDefaultFont();
        labelText.raycastTarget = false;

        // 装备名称（显示已装备的物品名或默认槽位名）
        GameObject nameGO = new GameObject("Name");
        nameGO.transform.SetParent(slotGO.transform, false);
        RectTransform nameRect = nameGO.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(1, 0.4f);
        nameRect.offsetMin = new Vector2(5, 2);
        nameRect.offsetMax = new Vector2(-5, 0);
        UnityEngine.UI.Text nameText = nameGO.AddComponent<UnityEngine.UI.Text>();
        nameText.text = slotName;
        nameText.fontSize = 17;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.gray;
        nameText.font = GetDefaultFont();
        nameText.raycastTarget = false;

        // 装备图标
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(slotGO.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.4f);
        iconRect.anchorMax = new Vector2(0.5f, 0.95f);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.sizeDelta = new Vector2(55, 0);
        UnityEngine.UI.Image iconImg = iconGO.AddComponent<UnityEngine.UI.Image>();
        iconImg.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        iconImg.raycastTarget = false;
    }

    /// <summary>
    /// 创建角色信息面板 — 使用CharacterInfoScreen（新版效果图布局）
    /// 点击小地图或其他入口时显示
    /// </summary>
    void CreateCharacterInfoPanel(Transform parent)
    {
        GameObject charInfoGO = new GameObject("CharacterInfoScreen");
        charInfoGO.transform.SetParent(parent, false);
        RectTransform rt = charInfoGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 添加CanvasGroup用于淡入淡出（alpha=0避免Start()隐藏前的闪烁）
        CanvasGroup cg = charInfoGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 添加CharacterInfoScreen组件 — Awake()设置Instance，Start()初始化UI并隐藏
        MoShou.UI.CharacterInfoScreen charScreen = charInfoGO.AddComponent<MoShou.UI.CharacterInfoScreen>();

        // 不在此处SetActive(false)！让Start()执行InitializeUI()后自行隐藏
        // 否则Start()不会执行，UI不会创建
        Debug.Log($"[GameSceneSetup] 角色信息面板已创建, Instance={MoShou.UI.CharacterInfoScreen.Instance != null}");
    }

    /// <summary>
    /// 创建商店面板 - Prefab方式
    /// </summary>
    void CreateShopPanel(Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/ShopPanel");
        if (prefab == null)
        {
            Debug.LogError("[GameSceneSetup] 找不到 Prefabs/UI/ShopPanel 预制体！请执行 MoShou/创建商店Prefab");
            return;
        }

        GameObject panelGO = Instantiate(prefab, parent);
        panelGO.name = "ShopPanel";

        MoShou.UI.ShopPanel shopPanel = panelGO.GetComponent<MoShou.UI.ShopPanel>();
        if (shopPanel != null)
        {
            MoShou.UI.ShopPanel.Instance = shopPanel;
        }

        // Prefab默认隐藏，确保初始状态正确
        panelGO.SetActive(false);
        Debug.Log("[GameSceneSetup] 商店面板已创建（Prefab方式）");
    }

    /// <summary>
    /// 创建技能升级面板
    /// </summary>
    void CreateSkillUpgradePanel(Transform parent)
    {
        // 技能面板背景
        GameObject panelGO = new GameObject("SkillUpgradePanel");
        panelGO.transform.SetParent(parent, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image bgImage = panelGO.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);

        // 添加SkillUpgradePanel组件
        MoShou.UI.SkillUpgradePanel skillPanel = panelGO.AddComponent<MoShou.UI.SkillUpgradePanel>();
        MoShou.UI.SkillUpgradePanel.Instance = skillPanel;

        // 内容框（左侧技能列表）
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(panelGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.03f, 0.1f);
        contentRect.anchorMax = new Vector2(0.52f, 0.9f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image contentBg = contentGO.AddComponent<UnityEngine.UI.Image>();
        // 使用Kenney棕色面板（技能面板）
        Sprite skillPanelBg = UIResourceLoader.PanelBrown;
        if (skillPanelBg != null)
        {
            contentBg.sprite = skillPanelBg;
            contentBg.type = UnityEngine.UI.Image.Type.Sliced;
            contentBg.color = Color.white;
        }
        else
        {
            contentBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        }

        // 标题
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(contentGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -35);
        titleRect.sizeDelta = new Vector2(200, 50);
        UnityEngine.UI.Text titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
        titleText.text = "技能升级";
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.9f, 0.6f); // 暖金色文字
        titleText.font = GetDefaultFont();
        UnityEngine.UI.Outline skillTitleOutline = titleGO.AddComponent<UnityEngine.UI.Outline>();
        skillTitleOutline.effectColor = new Color(0, 0, 0, 0.6f);
        skillTitleOutline.effectDistance = new Vector2(1, -1);
        skillPanel.titleText = titleText;

        // 关闭按钮 - 使用Kenney方形按钮
        GameObject closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(panelGO.transform, false);
        RectTransform closeRect = closeBtnGO.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-30, -30);
        closeRect.sizeDelta = new Vector2(50, 50);
        UnityEngine.UI.Image closeImg = closeBtnGO.AddComponent<UnityEngine.UI.Image>();
        Sprite skCloseBtnSprite = UIResourceLoader.ButtonSquareBrown;
        if (skCloseBtnSprite != null)
        {
            closeImg.sprite = skCloseBtnSprite;
            closeImg.type = UnityEngine.UI.Image.Type.Sliced;
            closeImg.color = Color.white;
        }
        else
        {
            closeImg.color = new Color(0.8f, 0.2f, 0.2f);
        }
        UnityEngine.UI.Button closeBtn = closeBtnGO.AddComponent<UnityEngine.UI.Button>();
        closeBtn.onClick.AddListener(() => skillPanel.Hide());
        skillPanel.closeButton = closeBtn;

        // 关闭按钮图标
        GameObject closeTextGO = new GameObject("Text");
        closeTextGO.transform.SetParent(closeBtnGO.transform, false);
        RectTransform ctRect = closeTextGO.AddComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero;
        ctRect.anchorMax = Vector2.one;
        ctRect.offsetMin = Vector2.zero;
        ctRect.offsetMax = Vector2.zero;
        Sprite skCrossIcon = UIResourceLoader.IconCrossBrown;
        if (skCrossIcon != null)
        {
            UnityEngine.UI.Image skCrossImg = closeTextGO.AddComponent<UnityEngine.UI.Image>();
            skCrossImg.sprite = skCrossIcon;
            skCrossImg.color = Color.white;
            skCrossImg.raycastTarget = false;
            ctRect.offsetMin = new Vector2(8, 8);
            ctRect.offsetMax = new Vector2(-8, -8);
        }
        else
        {
            UnityEngine.UI.Text closeText = closeTextGO.AddComponent<UnityEngine.UI.Text>();
            closeText.text = "X";
            closeText.fontSize = 28;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.color = Color.white;
            closeText.font = GetDefaultFont();
        }

        // 金币显示
        GameObject goldGO = new GameObject("GoldDisplay");
        goldGO.transform.SetParent(contentGO.transform, false);
        RectTransform goldRect = goldGO.AddComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0, 1);
        goldRect.anchorMax = new Vector2(0, 1);
        goldRect.anchoredPosition = new Vector2(20, -35);
        goldRect.sizeDelta = new Vector2(150, 40);
        UnityEngine.UI.Text goldText = goldGO.AddComponent<UnityEngine.UI.Text>();
        goldText.text = "金币: 0";
        goldText.fontSize = 20;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = new Color(1f, 0.85f, 0.2f);
        goldText.font = GetDefaultFont();
        skillPanel.goldText = goldText;

        // 技能列表容器（带滚动）
        GameObject skillsGO = new GameObject("SkillsContainer");
        skillsGO.transform.SetParent(contentGO.transform, false);
        RectTransform skillsRect = skillsGO.AddComponent<RectTransform>();
        skillsRect.anchorMin = new Vector2(0.02f, 0.02f);
        skillsRect.anchorMax = new Vector2(0.98f, 0.85f);
        skillsRect.offsetMin = Vector2.zero;
        skillsRect.offsetMax = Vector2.zero;

        // 添加滚动视图
        var scrollRect = skillsGO.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        // 内容容器
        GameObject scrollContentGO = new GameObject("Content");
        scrollContentGO.transform.SetParent(skillsGO.transform, false);
        RectTransform scrollContentRect = scrollContentGO.AddComponent<RectTransform>();
        scrollContentRect.anchorMin = new Vector2(0, 1);
        scrollContentRect.anchorMax = new Vector2(1, 1);
        scrollContentRect.pivot = new Vector2(0.5f, 1);
        scrollContentRect.anchoredPosition = Vector2.zero;
        scrollContentRect.sizeDelta = new Vector2(0, 800);

        var verticalLayout = scrollContentGO.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.spacing = 5;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.padding = new RectOffset(5, 5, 5, 5);

        var contentSizeFitter = scrollContentGO.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        contentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = scrollContentRect;
        skillPanel.skillsContainer = scrollContentGO.transform;

        // 初始隐藏
        panelGO.SetActive(false);
        Debug.Log("[GameSceneSetup] 技能升级面板已创建");
    }

    void CreateCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.5f, 0.7f, 0.9f); // 天蓝色背景
        cam.fieldOfView = 50f; // 稍微窄一点的视野

        // 添加AudioListener
        if (FindObjectOfType<AudioListener>() == null)
        {
            camGO.AddComponent<AudioListener>();
        }

        // 设置初始第三人称视角 - 更低更近
        camGO.transform.position = new Vector3(0, 6, -8);
        camGO.transform.rotation = Quaternion.Euler(30, 0, 0);

        // 添加跟随脚本
        var follow = camGO.AddComponent<CameraFollow>();

        // 立即查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            follow.target = player.transform;
            camGO.transform.position = player.transform.position + follow.offset;
            camGO.transform.LookAt(player.transform.position + Vector3.up * 1.2f);
            Debug.Log("[GameSceneSetup] 新摄像机已创建并跟随玩家");
        }
    }

    /// <summary>
    /// 安全检查：确保场景中有地面，如果没有则创建简单地面
    /// </summary>
    IEnumerator EnsureGroundExists()
    {
        // 等待两帧，让所有初始化完成
        yield return null;
        yield return null;

        // 检查是否有任何类型的地面
        bool hasGround = false;

        // 方法1: 检查Terrain对象和组件
        GameObject terrain = GameObject.Find("Terrain");
        if (terrain != null)
        {
            // 检查Unity Terrain组件是否有效（TerrainData不能为空）
            Terrain terrainComponent = terrain.GetComponentInChildren<Terrain>();
            if (terrainComponent != null)
            {
                if (terrainComponent.terrainData == null)
                {
                    // TerrainData缺失！这是MAP预制体资源丢失的问题
                    Debug.LogError("[GameSceneSetup] Terrain组件存在但TerrainData缺失! 需要创建后备地面");
                    // 禁用无效的Terrain组件
                    terrainComponent.enabled = false;
                    TerrainCollider tc = terrain.GetComponentInChildren<TerrainCollider>();
                    if (tc != null) tc.enabled = false;
                }
                else
                {
                    hasGround = true;
                    Debug.Log("[GameSceneSetup] 地面检查通过: Terrain组件有效");
                }
            }

            // 检查其他有效碰撞体（排除无效的TerrainCollider）
            if (!hasGround)
            {
                Collider[] colliders = terrain.GetComponentsInChildren<Collider>();
                int validColliders = 0;
                foreach (var col in colliders)
                {
                    // 跳过禁用的碰撞体和无效的TerrainCollider
                    if (col.enabled)
                    {
                        TerrainCollider tc = col as TerrainCollider;
                        if (tc != null && tc.terrainData == null)
                            continue; // 跳过无效的TerrainCollider
                        validColliders++;
                    }
                }
                if (validColliders > 0)
                {
                    hasGround = true;
                    Debug.Log($"[GameSceneSetup] 地面检查通过: 找到 {validColliders} 个有效碰撞体");
                }
            }
        }

        // 方法2: 检查是否有名为Ground的对象
        if (!hasGround)
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null && ground.GetComponent<Collider>() != null)
            {
                hasGround = true;
                Debug.Log("[GameSceneSetup] 地面检查通过: 找到Ground对象");
            }
        }

        // 方法3: 使用射线检测从空中向下
        if (!hasGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(0, 50, 0), Vector3.down, out hit, 100f))
            {
                hasGround = true;
                Debug.Log($"[GameSceneSetup] 地面检查通过: 射线检测到 {hit.collider.gameObject.name}");
            }
        }

        // 如果没有地面，创建后备地面
        if (!hasGround)
        {
            Debug.LogWarning("[GameSceneSetup] 未检测到有效地面! 创建后备地面...");
            CreateFallbackGround();
        }
    }

    /// <summary>
    /// 创建后备地面 - 当MAP加载失败时使用
    /// </summary>
    void CreateFallbackGround()
    {
        Debug.Log("[GameSceneSetup] 创建后备地面...");

        // 创建父对象
        GameObject terrainGO = new GameObject("Terrain_Fallback");
        terrainGO.layer = 11; // Environment layer

        // 创建地面平面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.parent = terrainGO.transform;
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10); // 100x100单位
        ground.layer = 11;

        // 设置材质为绿色草地
        Renderer rend = ground.GetComponent<Renderer>();
        if (rend != null)
        {
            // 尝试使用URP着色器
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
                urpShader = Shader.Find("Standard");

            Material groundMat = new Material(urpShader);
            groundMat.color = new Color(0.2f, 0.5f, 0.15f); // 草地绿色
            rend.material = groundMat;
        }

        // 创建边界墙
        CreateBoundaryWalls(terrainGO.transform, 45f);

        // 更新怪物生成边界
        MonsterSpawner spawner = FindObjectOfType<MonsterSpawner>();
        if (spawner != null)
        {
            spawner.terrainMinX = -40f;
            spawner.terrainMaxX = 40f;
            spawner.terrainMinZ = -40f;
            spawner.terrainMaxZ = 40f;
        }

        // 设置专业灯光
        if (FindObjectOfType<Light>() == null)
        {
            SetupProfessionalLighting();
        }

        Debug.Log("[GameSceneSetup] 后备地面创建完成 (100x100单位)");

        // 重新定位玩家到地面上
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0, 1f, 0);
            Debug.Log("[GameSceneSetup] 玩家已重新定位到后备地面上");
        }
    }
}

/// <summary>
/// 第三人称摄像机跟随 - 始终跟随玩家
/// 使用固定旋转角度，不使用LookAt避免与屏幕抖动冲突
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 6, -8);  // 更低的第三人称视角
    public float smoothSpeed = 10f;
    public float lookAtHeight = 1.2f;  // 看向玩家的高度偏移

    private bool initialized = false;
    private Quaternion fixedRotation;  // 固定的相机旋转角度

    void Start()
    {
        InitializeTarget();
    }

    void InitializeTarget()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                initialized = true;
                Debug.Log("[CameraFollow] 目标设置成功: " + player.name);

                // 立即设置初始位置
                transform.position = target.position + offset;

                // 计算并保存固定旋转角度（只在初始化时计算一次）
                Vector3 lookAtPos = target.position + Vector3.up * lookAtHeight;
                transform.LookAt(lookAtPos);
                fixedRotation = transform.rotation;
            }
        }
    }

    void LateUpdate()
    {
        // 每帧尝试查找目标（以防玩家延迟创建）
        if (target == null)
        {
            InitializeTarget();
            if (target == null) return;
        }

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;

        // 平滑移动摄像机位置
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 使用固定旋转，不再每帧调用LookAt
        // 这样ScreenShake的微小位移不会被立即覆盖
        transform.rotation = fixedRotation;
    }

    void OnEnable()
    {
        // 重新激活时也尝试初始化
        if (!initialized)
        {
            InitializeTarget();
        }
    }
}
