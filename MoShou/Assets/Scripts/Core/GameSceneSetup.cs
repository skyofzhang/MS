using UnityEngine;

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
        
        Debug.Log("[GameSceneSetup] Scene setup complete!");
    }
    
    void CreatePlayer()
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, 0);
        player.GetComponent<Renderer>().material.color = Color.green;
        
        player.AddComponent<PlayerController>();
        
        // 添加CharacterController
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 1, 0);
    }
    
    void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(3, 1, 3);
        ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.5f, 0.3f);
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
