using UnityEngine;
using MoShou.Systems;

namespace MoShou.Gameplay
{
    /// <summary>
    /// 掉落物拾取组件
    /// </summary>
    public class DropPickup : MonoBehaviour
    {
        [Header("掉落物设置")]
        [SerializeField] private float lifetimeSeconds = 30f;       // 存在时间
        [SerializeField] private float magnetSpeed = 10f;           // 吸附速度
        [SerializeField] private float bobSpeed = 2f;               // 上下浮动速度
        [SerializeField] private float bobHeight = 0.2f;            // 上下浮动高度

        private DropPickupType pickupType;
        private int amount;
        private string itemId;

        private Transform playerTransform;
        private Vector3 startPosition;
        private float spawnTime;
        private bool isBeingPickedUp = false;

        /// <summary>
        /// 初始化掉落物
        /// </summary>
        public void Initialize(DropPickupType type, int amt, string id)
        {
            pickupType = type;
            amount = amt;
            itemId = id;
            startPosition = transform.position;
            spawnTime = Time.time;

            // 根据类型设置外观（简化版，实际应该用不同预制体或修改材质）
            SetAppearance();
        }

        private void Start()
        {
            // 查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            // 生命周期检查
            if (Time.time - spawnTime > lifetimeSeconds)
            {
                Destroy(gameObject);
                return;
            }

            // 上下浮动效果
            if (!isBeingPickedUp)
            {
                float bobOffset = Mathf.Sin((Time.time - spawnTime) * bobSpeed) * bobHeight;
                transform.position = startPosition + Vector3.up * bobOffset;
            }

            // 检查玩家距离并吸附
            if (playerTransform != null && !isBeingPickedUp)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                float pickupRadius = LootManager.Instance != null ? LootManager.Instance.GetPickupRadius() : 2f;

                if (distance <= pickupRadius)
                {
                    isBeingPickedUp = true;
                }
            }

            // 吸附到玩家
            if (isBeingPickedUp && playerTransform != null)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    playerTransform.position + Vector3.up * 0.5f,
                    magnetSpeed * Time.deltaTime
                );

                // 到达玩家位置时拾取
                if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
                {
                    Pickup();
                }
            }
        }

        /// <summary>
        /// 执行拾取
        /// </summary>
        private void Pickup()
        {
            if (LootManager.Instance != null)
            {
                LootManager.Instance.DirectPickup(pickupType, amount, itemId);
            }

            // 播放拾取效果（可扩展）
            PlayPickupEffect();

            Destroy(gameObject);
        }

        /// <summary>
        /// 设置外观
        /// </summary>
        private void SetAppearance()
        {
            // 获取或添加渲染器
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                // 添加简单的球体作为默认外观
                MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                filter.mesh = CreateSphereMesh();
                renderer = gameObject.AddComponent<MeshRenderer>();
            }

            // 根据类型设置颜色 - 使用URP兼容Shader
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            switch (pickupType)
            {
                case DropPickupType.Gold:
                    mat.color = new Color(1f, 0.84f, 0f); // 金色
                    transform.localScale = Vector3.one * 0.3f;
                    break;
                case DropPickupType.Exp:
                    mat.color = new Color(0.5f, 0.8f, 1f); // 蓝色
                    transform.localScale = Vector3.one * 0.25f;
                    break;
                case DropPickupType.Item:
                    mat.color = new Color(0.8f, 0.4f, 1f); // 紫色
                    transform.localScale = Vector3.one * 0.4f;
                    break;
                case DropPickupType.Equipment:
                    mat.color = new Color(1f, 0.5f, 0f); // 橙色 - 装备
                    transform.localScale = Vector3.one * 0.5f;
                    break;
            }
            renderer.material = mat;

            // 添加碰撞器（可选，用于其他检测）
            if (GetComponent<Collider>() == null)
            {
                SphereCollider col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 0.5f;
            }
        }

        /// <summary>
        /// 创建简单的球体网格
        /// </summary>
        private Mesh CreateSphereMesh()
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
            Destroy(temp);
            return mesh;
        }

        /// <summary>
        /// 播放拾取效果
        /// </summary>
        private void PlayPickupEffect()
        {
            // 可以在这里添加粒子效果或音效
            Debug.Log($"[DropPickup] 拾取 {pickupType}: {amount}");
        }
    }
}
