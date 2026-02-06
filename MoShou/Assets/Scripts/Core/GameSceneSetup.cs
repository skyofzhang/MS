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
        // 创建完整的游戏地形
        GameObject terrainParent = new GameObject("Terrain");
        terrainParent.layer = 11;

        // 1. 创建大地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.parent = terrainParent.transform;
        ground.transform.localScale = new Vector3(10, 1, 10); // 100x100单位
        ground.layer = 11;

        // 地面材质 - 草地颜色
        var groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.material = new Material(Shader.Find("Standard"));
        groundRenderer.material.color = new Color(0.25f, 0.45f, 0.2f);

        // 2. 创建边界墙（透明但有碰撞）
        CreateBoundaryWalls(terrainParent.transform, 50f);

        // 3. 创建装饰物
        CreateDecorations(terrainParent.transform);

        // 4. 创建方向光（如果没有）
        if (FindObjectOfType<Light>() == null)
        {
            CreateDirectionalLight();
        }

        Debug.Log("[GameSceneSetup] 地形创建完成 (100x100)");
    }

    void CreateBoundaryWalls(Transform parent, float size)
    {
        float wallHeight = 5f;
        float wallThickness = 1f;
        float halfSize = size;

        // 四面墙
        CreateWall(parent, "WallNorth", new Vector3(0, wallHeight/2, halfSize), new Vector3(halfSize*2, wallHeight, wallThickness));
        CreateWall(parent, "WallSouth", new Vector3(0, wallHeight/2, -halfSize), new Vector3(halfSize*2, wallHeight, wallThickness));
        CreateWall(parent, "WallEast", new Vector3(halfSize, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, halfSize*2));
        CreateWall(parent, "WallWest", new Vector3(-halfSize, wallHeight/2, 0), new Vector3(wallThickness, wallHeight, halfSize*2));
    }

    void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.parent = parent;
        wall.transform.localPosition = pos;
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
        trunk.GetComponent<Renderer>().material.color = new Color(0.4f, 0.25f, 0.1f);
        trunk.layer = 11;

        // 树冠
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.transform.parent = tree.transform;
        crown.transform.localPosition = new Vector3(0, 4f, 0);
        crown.transform.localScale = new Vector3(3f, 3f, 3f);
        crown.GetComponent<Renderer>().material.color = new Color(0.15f, 0.4f, 0.15f);
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
        rock.GetComponent<Renderer>().material.color = new Color(0.45f, 0.45f, 0.45f);
        rock.layer = 11;
    }

    void CreateDirectionalLight()
    {
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
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
