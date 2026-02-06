using UnityEngine;
using MoShou.Utils;

/// <summary>
/// 游戏场景初始化 - 符合知识库§2 RULE-RES-001
/// </summary>
public class GameSceneSetup : MonoBehaviour
{
    void Awake()
    {
        SetupScene();
    }
    
    void SetupScene()
    {
        // 确保有GameManager
        if (GameManager.Instance == null)
        {
            var gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
        }
        
        // 创建玩家
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            CreatePlayer();
        }
        
        // 创建地面
        if (GameObject.Find("Ground") == null)
        {
            CreateGround();
        }
        
        // 创建怪物生成器
        if (FindObjectOfType<MonsterSpawner>() == null)
        {
            var spawnerGO = new GameObject("MonsterSpawner");
            spawnerGO.AddComponent<MonsterSpawner>();
        }
        
        // 创建主摄像机(如果没有)
        if (Camera.main == null)
        {
            CreateCamera();
        }

        // 创建UI资源绑定器 - 自动应用美术资源到UI组件
        if (UIResourceBinder.Instance == null)
        {
            var binderGO = new GameObject("UIResourceBinder");
            binderGO.AddComponent<UIResourceBinder>();
            Debug.Log("[GameSceneSetup] UIResourceBinder已创建");
        }

        Debug.Log("[GameSceneSetup] Scene setup complete!");
    }
    
    void CreatePlayer()
    {
        // 知识库§2 RULE-RES-001: 加载玩家模型
        // 优先加载Prefab（更可靠），其次尝试FBX
        GameObject player = null;

        // 1. 首先尝试加载Prefab（由ProjectSetupTool生成）
        var prefab = Resources.Load<GameObject>("Prefabs/Characters/Player_Archer");

        // 2. 如果Prefab不存在，尝试加载FBX
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Models/Player/Player_Archer");
            if (prefab != null)
                Debug.Log("[GameSceneSetup] 从FBX加载玩家模型");
        }
        else
        {
            Debug.Log("[GameSceneSetup] 从Prefab加载玩家模型");
        }

        if (prefab != null)
        {
            player = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            player.name = "Player";
            Debug.Log("[GameSceneSetup] 玩家模型加载成功: Player_Archer");
        }

        // 如果都加载失败，使用FALLBACK
        if (player == null)
        {
            // FALLBACK: 知识库允许的降级策略
            Debug.LogWarning("[GameSceneSetup] 玩家模型未找到，使用FALLBACK胶囊体");
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_FALLBACK";
            player.GetComponent<Renderer>().material.color = Color.green;
        }

        player.tag = "Player";
        player.layer = 8; // 知识库§5: Layer 8 = Player

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
    
    void CreateGround()
    {
        // 尝试加载地面模型
        var prefab = Resources.Load<GameObject>("Models/Environment/Forest/Ground_Grass_01");
        GameObject ground;

        if (prefab != null)
        {
            ground = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            ground.name = "Ground";
            Debug.Log("[GameSceneSetup] 地面模型加载成功");
        }
        else
        {
            // FALLBACK
            Debug.LogWarning("[GameSceneSetup] 地面模型未找到，使用FALLBACK平面");
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_FALLBACK";
            ground.transform.localScale = new Vector3(3, 1, 3);
            ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.5f, 0.3f);
        }

        ground.layer = 11; // 知识库§5: Layer 11 = Environment
    }
    
    void CreateCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.2f, 0.3f, 0.4f);
        
        // 设置为俯视角
        camGO.transform.position = new Vector3(0, 15, -10);
        camGO.transform.rotation = Quaternion.Euler(50, 0, 0);
        
        // 添加简单跟随脚本
        camGO.AddComponent<CameraFollow>();
    }
}

// 简单的摄像机跟随
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 15, -10);
    public float smoothSpeed = 5f;
    
    void LateUpdate()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }
        
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
