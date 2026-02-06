using UnityEngine;

namespace MoShou.Gameplay
{
    /// <summary>
    /// 简单地形生成器 - 创建游戏场地
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("地形设置")]
        [SerializeField] private float terrainWidth = 50f;
        [SerializeField] private float terrainLength = 50f;
        [SerializeField] private Material groundMaterial;

        [Header("装饰物")]
        [SerializeField] private bool generateDecorations = true;
        [SerializeField] private int treeCount = 20;
        [SerializeField] private int rockCount = 15;

        private void Start()
        {
            GenerateTerrain();
        }

        /// <summary>
        /// 生成地形
        /// </summary>
        public void GenerateTerrain()
        {
            // 创建地面
            CreateGround();

            // 创建边界
            CreateBoundaries();

            // 生成装饰物
            if (generateDecorations)
            {
                GenerateDecorations();
            }

            Debug.Log("[TerrainGenerator] 地形生成完成");
        }

        /// <summary>
        /// 创建地面
        /// </summary>
        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.parent = transform;
            ground.transform.localPosition = Vector3.zero;
            ground.transform.localScale = new Vector3(terrainWidth / 10f, 1f, terrainLength / 10f);
            ground.layer = 11; // Environment layer

            // 应用材质
            Renderer renderer = ground.GetComponent<Renderer>();
            if (groundMaterial != null)
            {
                renderer.material = groundMaterial;
            }
            else
            {
                // 使用默认绿色材质
                renderer.material.color = new Color(0.3f, 0.5f, 0.2f);
            }
        }

        /// <summary>
        /// 创建边界墙
        /// </summary>
        private void CreateBoundaries()
        {
            float wallHeight = 3f;
            float wallThickness = 1f;

            // 四面墙
            CreateWall("WallNorth", new Vector3(0, wallHeight / 2, terrainLength / 2), new Vector3(terrainWidth, wallHeight, wallThickness));
            CreateWall("WallSouth", new Vector3(0, wallHeight / 2, -terrainLength / 2), new Vector3(terrainWidth, wallHeight, wallThickness));
            CreateWall("WallEast", new Vector3(terrainWidth / 2, wallHeight / 2, 0), new Vector3(wallThickness, wallHeight, terrainLength));
            CreateWall("WallWest", new Vector3(-terrainWidth / 2, wallHeight / 2, 0), new Vector3(wallThickness, wallHeight, terrainLength));
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = transform;
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.layer = 11; // Environment layer

            // 设置为不可见但有碰撞
            Renderer renderer = wall.GetComponent<Renderer>();
            renderer.enabled = false;
        }

        /// <summary>
        /// 生成装饰物
        /// </summary>
        private void GenerateDecorations()
        {
            GameObject decorationsParent = new GameObject("Decorations");
            decorationsParent.transform.parent = transform;

            // 生成简单的树木（使用胶囊体代替）
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 pos = GetRandomPosition();
                CreateTree(decorationsParent.transform, pos);
            }

            // 生成简单的石头（使用球体代替）
            for (int i = 0; i < rockCount; i++)
            {
                Vector3 pos = GetRandomPosition();
                CreateRock(decorationsParent.transform, pos);
            }
        }

        private void CreateTree(Transform parent, Vector3 position)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.parent = parent;
            tree.transform.position = position;
            tree.layer = 11;

            // 树干
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.parent = tree.transform;
            trunk.transform.localPosition = new Vector3(0, 1f, 0);
            trunk.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
            trunk.GetComponent<Renderer>().material.color = new Color(0.4f, 0.25f, 0.1f);
            trunk.layer = 11;

            // 树冠
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.transform.parent = tree.transform;
            crown.transform.localPosition = new Vector3(0, 2.5f, 0);
            crown.transform.localScale = new Vector3(2f, 2f, 2f);
            crown.GetComponent<Renderer>().material.color = new Color(0.1f, 0.4f, 0.1f);
            crown.layer = 11;

            // 添加碰撞体
            CapsuleCollider collider = tree.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 1.5f, 0);
            collider.radius = 0.5f;
            collider.height = 3f;
        }

        private void CreateRock(Transform parent, Vector3 position)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.parent = parent;
            rock.transform.position = position;

            float scale = Random.Range(0.3f, 0.8f);
            rock.transform.localScale = new Vector3(scale * 1.5f, scale, scale);
            rock.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            rock.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
            rock.layer = 11;
        }

        private Vector3 GetRandomPosition()
        {
            float margin = 5f; // 边缘留白
            float x = Random.Range(-terrainWidth / 2 + margin, terrainWidth / 2 - margin);
            float z = Random.Range(-terrainLength / 2 + margin, terrainLength / 2 - margin);

            // 避免生成在中心区域（玩家出生点附近）
            if (Mathf.Abs(x) < 5f && Mathf.Abs(z) < 5f)
            {
                x = x > 0 ? x + 5f : x - 5f;
            }

            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// 获取有效的生成位置（用于怪物生成）
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            float margin = 3f;
            float x = Random.Range(-terrainWidth / 2 + margin, terrainWidth / 2 - margin);
            float z = Random.Range(-terrainLength / 2 + margin, terrainLength / 2 - margin);

            // 避免在玩家附近生成
            if (Mathf.Abs(x) < 8f && Mathf.Abs(z) < 8f)
            {
                x = x > 0 ? x + 8f : x - 8f;
                z = z > 0 ? z + 8f : z - 8f;
            }

            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// 获取地形边界
        /// </summary>
        public Bounds GetTerrainBounds()
        {
            return new Bounds(Vector3.zero, new Vector3(terrainWidth, 10f, terrainLength));
        }
    }
}
