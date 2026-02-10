using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MoShou.UI
{
    /// <summary>
    /// 小地图系统 - 显示玩家、怪物位置
    /// 位置：屏幕左上角
    /// </summary>
    public class MinimapSystem : MonoBehaviour
    {
        public static MinimapSystem Instance { get; private set; }

        [Header("Minimap Settings")]
        public float mapWorldSize = 50f;       // 地图世界尺寸（对应地形大小）
        public float minimapSize = 120f;       // 小地图UI尺寸（像素）
        public float iconSize = 8f;            // 图标大小

        [Header("Colors")]
        public Color backgroundColor = new Color(0.1f, 0.15f, 0.1f, 0.85f);
        public Color borderColor = new Color(0.4f, 0.35f, 0.2f, 1f);
        public Color terrainColor = new Color(0.15f, 0.35f, 0.15f, 1f);
        public Color playerColor = new Color(0.3f, 0.7f, 1f, 1f);
        public Color enemyColor = new Color(1f, 0.3f, 0.2f, 1f);
        public Color bossColor = new Color(1f, 0.8f, 0f, 1f);

        // UI组件 - 必须在正确的Canvas下
        private Canvas uiCanvas;
        private RectTransform minimapContainer;
        private RectTransform mapArea;
        private RectTransform playerIcon;
        private List<RectTransform> enemyIcons = new List<RectTransform>();
        private Transform playerTransform;

        // 图标池
        private Queue<RectTransform> iconPool = new Queue<RectTransform>();

        // 性能优化：敌人更新间隔
        private float enemyUpdateInterval = 0.5f;  // 每0.5秒更新一次敌人图标
        private float lastEnemyUpdateTime;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            // 延迟创建，确保Canvas已经存在
            Invoke("CreateMinimapUI", 0.1f);
        }

        void Update()
        {
            if (minimapContainer == null) return;

            if (playerTransform == null)
            {
                FindPlayer();
            }

            UpdatePlayerIcon();

            // 性能优化：降低敌人图标更新频率（从每帧改为每0.5秒）
            if (Time.time - lastEnemyUpdateTime > enemyUpdateInterval)
            {
                UpdateEnemyIcons();
                lastEnemyUpdateTime = Time.time;
            }
        }

        void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        /// <summary>
        /// 查找正确的UI Canvas（Screen Space Overlay）
        /// </summary>
        Canvas FindUICanvas()
        {
            // 查找名为GameCanvas的Canvas（由GameSceneSetup创建）
            GameObject canvasObj = GameObject.Find("GameCanvas");
            if (canvasObj != null)
            {
                Canvas canvas = canvasObj.GetComponent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Debug.Log("[Minimap] 找到GameCanvas");
                    return canvas;
                }
            }

            // 查找所有Canvas，找Screen Space Overlay的
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in allCanvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Debug.Log($"[Minimap] 使用Canvas: {canvas.name}");
                    return canvas;
                }
            }

            // 如果没有找到，创建一个新的
            Debug.Log("[Minimap] 创建新的MinimapCanvas");
            GameObject newCanvasObj = new GameObject("MinimapCanvas");
            Canvas newCanvas = newCanvasObj.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.sortingOrder = 95; // 在其他UI之上

            var scaler = newCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            newCanvasObj.AddComponent<GraphicRaycaster>();

            return newCanvas;
        }

        /// <summary>
        /// 创建小地图UI
        /// </summary>
        void CreateMinimapUI()
        {
            // 获取正确的Canvas
            uiCanvas = FindUICanvas();
            if (uiCanvas == null)
            {
                Debug.LogError("[Minimap] 无法找到或创建Canvas!");
                return;
            }

            // 创建小地图容器
            GameObject containerObj = new GameObject("MinimapContainer");
            containerObj.transform.SetParent(uiCanvas.transform, false);
            minimapContainer = containerObj.AddComponent<RectTransform>();

            // 定位到左上角（在HUD下方）
            minimapContainer.anchorMin = new Vector2(0, 1);
            minimapContainer.anchorMax = new Vector2(0, 1);
            minimapContainer.pivot = new Vector2(0, 1);
            minimapContainer.anchoredPosition = new Vector2(10, -100); // 左上角，HUD下方
            minimapContainer.sizeDelta = new Vector2(minimapSize, minimapSize);

            // 创建背景和边框
            CreateMinimapBackground();

            // 创建地图区域
            CreateMapArea();

            // 创建玩家图标
            CreatePlayerIcon();

            Debug.Log($"[Minimap] 小地图初始化完成，父Canvas: {uiCanvas.name}");
        }

        void CreateMinimapBackground()
        {
            // 外边框
            GameObject borderObj = new GameObject("MinimapBorder");
            borderObj.transform.SetParent(minimapContainer, false);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = borderColor;

            // 内部背景
            GameObject bgObj = new GameObject("MinimapBG");
            bgObj.transform.SetParent(minimapContainer, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(3, 3);
            bgRect.offsetMax = new Vector2(-3, -3);

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = backgroundColor;
        }

        void CreateMapArea()
        {
            // 地图显示区域
            GameObject mapObj = new GameObject("MapArea");
            mapObj.transform.SetParent(minimapContainer, false);
            mapArea = mapObj.AddComponent<RectTransform>();
            mapArea.anchorMin = Vector2.zero;
            mapArea.anchorMax = Vector2.one;
            mapArea.offsetMin = new Vector2(6, 6);
            mapArea.offsetMax = new Vector2(-6, -6);

            // 地形颜色
            Image mapImage = mapObj.AddComponent<Image>();
            mapImage.color = terrainColor;

            // 添加遮罩，让图标不超出地图区域
            mapObj.AddComponent<Mask>().showMaskGraphic = true;

            // 网格线
            CreateGridLines();
        }

        void CreateGridLines()
        {
            if (mapArea == null) return;

            int gridCount = 4;
            Color gridColor = new Color(0.3f, 0.5f, 0.3f, 0.4f);

            for (int i = 1; i < gridCount; i++)
            {
                float pos = (float)i / gridCount;

                // 垂直线
                GameObject vLine = new GameObject($"VLine_{i}");
                vLine.transform.SetParent(mapArea, false);
                RectTransform vRect = vLine.AddComponent<RectTransform>();
                vRect.anchorMin = new Vector2(pos, 0);
                vRect.anchorMax = new Vector2(pos, 1);
                vRect.sizeDelta = new Vector2(1, 0);
                vRect.anchoredPosition = Vector2.zero;
                Image vImg = vLine.AddComponent<Image>();
                vImg.color = gridColor;

                // 水平线
                GameObject hLine = new GameObject($"HLine_{i}");
                hLine.transform.SetParent(mapArea, false);
                RectTransform hRect = hLine.AddComponent<RectTransform>();
                hRect.anchorMin = new Vector2(0, pos);
                hRect.anchorMax = new Vector2(1, pos);
                hRect.sizeDelta = new Vector2(0, 1);
                hRect.anchoredPosition = Vector2.zero;
                Image hImg = hLine.AddComponent<Image>();
                hImg.color = gridColor;
            }
        }

        void CreatePlayerIcon()
        {
            if (mapArea == null) return;

            // 玩家图标
            GameObject playerObj = new GameObject("PlayerIcon");
            playerObj.transform.SetParent(mapArea, false);
            playerIcon = playerObj.AddComponent<RectTransform>();
            playerIcon.sizeDelta = new Vector2(iconSize * 1.5f, iconSize * 1.5f);
            playerIcon.anchorMin = new Vector2(0.5f, 0.5f);
            playerIcon.anchorMax = new Vector2(0.5f, 0.5f);

            Image playerImage = playerObj.AddComponent<Image>();
            playerImage.color = playerColor;

            // 方向指示器
            GameObject dirObj = new GameObject("Direction");
            dirObj.transform.SetParent(playerIcon, false);
            RectTransform dirRect = dirObj.AddComponent<RectTransform>();
            dirRect.anchoredPosition = new Vector2(0, iconSize * 0.6f);
            dirRect.sizeDelta = new Vector2(iconSize * 0.5f, iconSize * 0.5f);

            Image dirImage = dirObj.AddComponent<Image>();
            dirImage.color = new Color(playerColor.r * 0.8f, playerColor.g * 0.8f, playerColor.b, 0.9f);
        }

        /// <summary>
        /// 更新玩家图标位置和方向
        /// </summary>
        void UpdatePlayerIcon()
        {
            if (playerTransform == null || playerIcon == null || mapArea == null) return;

            // 将世界坐标转换为小地图坐标
            Vector2 minimapPos = WorldToMinimapPosition(playerTransform.position);
            playerIcon.anchoredPosition = minimapPos;

            // 更新方向（根据玩家朝向旋转）
            float angle = -playerTransform.eulerAngles.y;
            playerIcon.localRotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// 更新敌人图标
        /// </summary>
        void UpdateEnemyIcons()
        {
            if (mapArea == null) return;

            // 回收所有现有图标
            foreach (var icon in enemyIcons)
            {
                if (icon != null)
                {
                    icon.gameObject.SetActive(false);
                    iconPool.Enqueue(icon);
                }
            }
            enemyIcons.Clear();

            // 获取所有敌人
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                // 检查是否在地图范围内
                Vector3 pos = enemy.transform.position;
                if (Mathf.Abs(pos.x) > mapWorldSize / 2 + 5 || Mathf.Abs(pos.z) > mapWorldSize / 2 + 5)
                    continue;

                // 获取或创建图标
                RectTransform icon = GetEnemyIcon();
                enemyIcons.Add(icon);

                // 设置位置
                Vector2 minimapPos = WorldToMinimapPosition(pos);
                icon.anchoredPosition = minimapPos;

                // 根据怪物类型设置颜色和大小
                MonsterController monster = enemy.GetComponent<MonsterController>();
                Image iconImage = icon.GetComponent<Image>();

                if (monster != null && monster.monsterId.Contains("BOSS"))
                {
                    iconImage.color = bossColor;
                    icon.sizeDelta = new Vector2(iconSize * 2f, iconSize * 2f);
                }
                else if (monster != null && monster.monsterId.Contains("ELITE"))
                {
                    iconImage.color = new Color(1f, 0.6f, 0f, 1f);
                    icon.sizeDelta = new Vector2(iconSize * 1.4f, iconSize * 1.4f);
                }
                else
                {
                    iconImage.color = enemyColor;
                    icon.sizeDelta = new Vector2(iconSize, iconSize);
                }

                icon.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 获取敌人图标（从池中获取或创建新的）
        /// </summary>
        RectTransform GetEnemyIcon()
        {
            RectTransform icon;

            if (iconPool.Count > 0)
            {
                icon = iconPool.Dequeue();
            }
            else
            {
                GameObject iconObj = new GameObject("EnemyIcon");
                iconObj.transform.SetParent(mapArea, false);
                icon = iconObj.AddComponent<RectTransform>();
                icon.sizeDelta = new Vector2(iconSize, iconSize);
                icon.anchorMin = new Vector2(0.5f, 0.5f);
                icon.anchorMax = new Vector2(0.5f, 0.5f);

                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.color = enemyColor;
            }

            return icon;
        }

        /// <summary>
        /// 世界坐标转换为小地图坐标（相对于mapArea中心）
        /// </summary>
        Vector2 WorldToMinimapPosition(Vector3 worldPos)
        {
            if (mapArea == null) return Vector2.zero;

            // 地图中心在原点，范围是 -mapWorldSize/2 到 +mapWorldSize/2
            float normalizedX = worldPos.x / (mapWorldSize / 2);
            float normalizedZ = worldPos.z / (mapWorldSize / 2);

            // 钳制到 -1 到 1 范围
            normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
            normalizedZ = Mathf.Clamp(normalizedZ, -1f, 1f);

            // 转换为小地图像素坐标
            float mapAreaWidth = mapArea.rect.width;
            float mapAreaHeight = mapArea.rect.height;

            float minimapX = normalizedX * (mapAreaWidth / 2);
            float minimapY = normalizedZ * (mapAreaHeight / 2);

            return new Vector2(minimapX, minimapY);
        }

        /// <summary>
        /// 设置小地图可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (minimapContainer != null)
            {
                minimapContainer.gameObject.SetActive(visible);
            }
        }
    }
}
