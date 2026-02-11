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
        public float minimapSize = 180f;       // 小地图UI尺寸（像素）- 放大至180
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

        // 角色详情面板
        private GameObject characterDetailPanel;

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
            // 自动匹配地图世界尺寸到实际地形大小
            AutoDetectMapWorldSize();

            // 延迟创建，确保Canvas已经存在
            Invoke("CreateMinimapUI", 0.1f);
        }

        /// <summary>
        /// 自动检测地形尺寸，同步小地图的mapWorldSize
        /// </summary>
        void AutoDetectMapWorldSize()
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null && terrain.terrainData != null)
            {
                // 取地形X和Z中较大的值作为小地图世界尺寸
                float terrainX = terrain.terrainData.size.x;
                float terrainZ = terrain.terrainData.size.z;
                mapWorldSize = Mathf.Max(terrainX, terrainZ);
                Debug.Log($"[Minimap] 自动检测地形尺寸: {terrainX}x{terrainZ}, mapWorldSize={mapWorldSize}");
            }
            else
            {
                Debug.Log($"[Minimap] 未找到地形，使用默认mapWorldSize={mapWorldSize}");
            }
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
            minimapContainer.anchoredPosition = new Vector2(10, -210); // 左上角，HUD下方（往下移避免重叠）
            minimapContainer.sizeDelta = new Vector2(minimapSize, minimapSize);

            // 添加CanvasGroup实现整体半透明
            CanvasGroup minimapCanvasGroup = containerObj.AddComponent<CanvasGroup>();
            minimapCanvasGroup.alpha = 0.75f; // 75%不透明度，让小地图稍微透明

            // 创建背景和边框
            CreateMinimapBackground();

            // 创建地图区域
            CreateMapArea();

            // 创建玩家图标
            CreatePlayerIcon();

            // 添加点击事件 - 点击小地图打开角色详情
            Button minimapBtn = containerObj.AddComponent<Button>();
            minimapBtn.transition = Selectable.Transition.None; // 无视觉变化
            minimapBtn.onClick.AddListener(OnMinimapClicked);

            Debug.Log($"[Minimap] 小地图初始化完成，父Canvas: {uiCanvas.name}, mapWorldSize={mapWorldSize}");
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

        /// <summary>
        /// 小地图点击回调 - 打开角色详情面板
        /// </summary>
        void OnMinimapClicked()
        {
            // 已有面板则切换显示
            if (characterDetailPanel != null)
            {
                bool isActive = characterDetailPanel.activeSelf;
                characterDetailPanel.SetActive(!isActive);
                if (!isActive)
                {
                    RefreshCharacterDetail();
                    // 重置滚动位置到顶部
                    var scrollRect = characterDetailPanel.GetComponentInChildren<ScrollRect>();
                    if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
                }
                return;
            }

            // 创建角色详情面板
            CreateCharacterDetailPanel();
        }

        /// <summary>
        /// 创建简易角色详情面板
        /// </summary>
        void CreateCharacterDetailPanel()
        {
            if (uiCanvas == null) return;

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);

            // 全屏遮罩
            characterDetailPanel = new GameObject("CharacterDetailPanel");
            characterDetailPanel.transform.SetParent(uiCanvas.transform, false);
            RectTransform panelRect = characterDetailPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image overlay = characterDetailPanel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.6f);

            // 内容框
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(characterDetailPanel.transform, false);
            RectTransform contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(400, 620);
            contentRect.anchoredPosition = Vector2.zero;

            Image contentBg = contentGO.AddComponent<Image>();
            contentBg.color = new Color(0.15f, 0.15f, 0.22f, 0.98f);

            // 标题
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(contentGO.transform, false);
            RectTransform titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(0, 50);
            Text titleText = titleGO.AddComponent<Text>();
            titleText.text = "角色信息";
            titleText.fontSize = 30;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.85f, 0.4f);
            titleText.font = defaultFont;

            // 关闭按钮
            GameObject closeBtnGO = new GameObject("CloseBtn");
            closeBtnGO.transform.SetParent(contentGO.transform, false);
            RectTransform closeRect = closeBtnGO.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-25, -25);
            closeRect.sizeDelta = new Vector2(40, 40);
            Image closeBg = closeBtnGO.AddComponent<Image>();
            closeBg.color = new Color(0.7f, 0.2f, 0.2f);
            Button closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtn.targetGraphic = closeBg;
            closeBtn.onClick.AddListener(() => characterDetailPanel.SetActive(false));

            GameObject closeTextGO = new GameObject("X");
            closeTextGO.transform.SetParent(closeBtnGO.transform, false);
            RectTransform cxRect = closeTextGO.AddComponent<RectTransform>();
            cxRect.anchorMin = Vector2.zero;
            cxRect.anchorMax = Vector2.one;
            cxRect.offsetMin = Vector2.zero;
            cxRect.offsetMax = Vector2.zero;
            Text closeText = closeTextGO.AddComponent<Text>();
            closeText.text = "X";
            closeText.fontSize = 22;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.color = Color.white;
            closeText.font = defaultFont;

            // 角色属性区域 - 使用ScrollRect实现滚动
            // 1. 滚动视口（带Mask裁剪）
            GameObject scrollViewGO = new GameObject("ScrollView");
            scrollViewGO.transform.SetParent(contentGO.transform, false);
            RectTransform scrollViewRect = scrollViewGO.AddComponent<RectTransform>();
            scrollViewRect.anchorMin = new Vector2(0.05f, 0.12f);
            scrollViewRect.anchorMax = new Vector2(0.95f, 0.88f);
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;
            Image scrollViewBg = scrollViewGO.AddComponent<Image>();
            scrollViewBg.color = new Color(0, 0, 0, 0.01f); // 几乎透明，仅用于Mask
            Mask scrollMask = scrollViewGO.AddComponent<Mask>();
            scrollMask.showMaskGraphic = false;

            // 2. 内容容器（高度会根据文本自动扩展）
            GameObject statsGO = new GameObject("Stats");
            statsGO.transform.SetParent(scrollViewGO.transform, false);
            RectTransform statsRect = statsGO.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 1); // 顶部对齐
            statsRect.anchorMax = new Vector2(1, 1);
            statsRect.pivot = new Vector2(0.5f, 1);
            statsRect.anchoredPosition = Vector2.zero;
            statsRect.sizeDelta = new Vector2(0, 800); // 初始高度，会被ContentSizeFitter覆盖
            Text statsText = statsGO.AddComponent<Text>();
            statsText.fontSize = 20;
            statsText.alignment = TextAnchor.UpperLeft;
            statsText.color = Color.white;
            statsText.font = defaultFont;
            statsText.supportRichText = true;
            statsText.lineSpacing = 1.5f;
            // ContentSizeFitter让文本高度自适应
            ContentSizeFitter fitter = statsGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 3. 添加ScrollRect组件
            ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
            scrollRect.content = statsRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 30f;

            RefreshCharacterDetail();
        }

        /// <summary>
        /// 刷新角色详情数据
        /// </summary>
        void RefreshCharacterDetail()
        {
            if (characterDetailPanel == null) return;

            Text statsText = characterDetailPanel.GetComponentInChildren<Text>();
            // 通过name查找statsText
            foreach (var t in characterDetailPanel.GetComponentsInChildren<Text>())
            {
                if (t.gameObject.name == "Stats")
                {
                    statsText = t;
                    break;
                }
            }
            if (statsText == null || statsText.gameObject.name != "Stats") return;

            // 获取玩家数据
            string info = "";

            // 基本信息
            int level = 1;
            int gold = 0;
            int exp = 0;
            if (MoShou.Systems.SaveSystem.Instance != null && MoShou.Systems.SaveSystem.Instance.CurrentPlayerStats != null)
            {
                var stats = MoShou.Systems.SaveSystem.Instance.CurrentPlayerStats;
                level = stats.level;
                gold = stats.gold;
                exp = stats.experience;
            }
            if (GameManager.Instance != null)
            {
                gold = GameManager.Instance.SessionGold;
            }

            info += $"<color=#FFD700><b>等级:</b></color>  {level}\n";
            info += $"<color=#FFD700><b>金币:</b></color>  {gold}\n";
            info += $"<color=#FFD700><b>经验:</b></color>  {exp}\n\n";

            // 角色总属性（基础 + 装备加成）
            int charAtk = 10, charDef = 5, charMaxHp = 100;
            float charCritRate = 5f;
            if (MoShou.Systems.SaveSystem.Instance?.CurrentPlayerStats != null)
            {
                var ps = MoShou.Systems.SaveSystem.Instance.CurrentPlayerStats;
                charAtk = ps.GetTotalAttack();
                charDef = ps.GetTotalDefense();
                charMaxHp = ps.GetTotalMaxHp();
                charCritRate = ps.GetTotalCritRate();
            }
            info += "<color=#AAAAFF><b>── 角色属性 ──</b></color>\n";
            info += $"  <color=#6BFF6B>生命值:</color>  {charMaxHp}\n";
            info += $"  <color=#FF6B6B>攻击力:</color>  {charAtk}\n";
            info += $"  <color=#6BB5FF>防御力:</color>  {charDef}\n";
            info += $"  <color=#FFD700>暴击率:</color>  {charCritRate:F1}%\n\n";

            // 装备属性
            int totalAtk = 0, totalDef = 0, totalHp = 0;
            float totalCrit = 0f;

            if (MoShou.Systems.EquipmentManager.Instance != null)
            {
                var equipped = MoShou.Systems.EquipmentManager.Instance.GetAllEquipments();
                if (equipped != null)
                {
                    info += "<color=#AAAAFF><b>── 已穿戴装备 ──</b></color>\n";
                    foreach (var kvp in equipped)
                    {
                        if (kvp.Value != null)
                        {
                            string slotName = GetSlotName(kvp.Key);
                            info += $"  <color=#888>{slotName}:</color> <color=#FFFFFF>{kvp.Value.name}</color>\n";
                            totalAtk += kvp.Value.attackBonus;
                            totalDef += kvp.Value.defenseBonus;
                            totalHp += kvp.Value.hpBonus;
                            totalCrit += kvp.Value.critRateBonus;
                        }
                    }
                    info += "\n";
                }
            }

            info += "<color=#AAAAFF><b>── 装备加成 ──</b></color>\n";
            info += $"  <color=#FF6B6B>攻击力:</color>  +{totalAtk}\n";
            info += $"  <color=#6BB5FF>防御力:</color>  +{totalDef}\n";
            info += $"  <color=#6BFF6B>生命值:</color>  +{totalHp}\n";
            info += $"  <color=#FFD700>暴击率:</color>  +{totalCrit:P1}\n";

            statsText.text = info;
        }

        string GetSlotName(MoShou.Data.EquipmentSlot slot)
        {
            switch (slot)
            {
                case MoShou.Data.EquipmentSlot.Weapon: return "武器";
                case MoShou.Data.EquipmentSlot.Helmet: return "头盔";
                case MoShou.Data.EquipmentSlot.Armor: return "护甲";
                case MoShou.Data.EquipmentSlot.Pants: return "护腿";
                case MoShou.Data.EquipmentSlot.Ring: return "戒指";
                case MoShou.Data.EquipmentSlot.Necklace: return "项链";
                default: return "未知";
            }
        }
    }
}
