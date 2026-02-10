using UnityEngine;
using UnityEngine.UI;
using MoShou.Systems;
using MoShou.Data;
using System.Collections.Generic;

namespace MoShou.UI
{
    /// <summary>
    /// 简化版装备面板 - 接入EquipmentManager显示已穿戴装备
    /// 支持点击查看装备详情 + 卸下装备
    /// </summary>
    public class SimpleEquipmentPanel : MonoBehaviour
    {
        public static SimpleEquipmentPanel Instance { get; set; }

        // 装备槽UI（由GameSceneSetup创建时设置）
        public Transform slotsContainer;
        public Text statsText;

        // 内部缓存
        private Dictionary<EquipmentSlot, Transform> slotUIMap = new Dictionary<EquipmentSlot, Transform>();
        private bool isInitialized = false;

        // 装备详情弹窗
        private GameObject detailPopup;
        private Text detailNameText;
        private Text detailStatsText;
        private Text detailQualityText;
        private Button detailUnequipBtn;
        private Button detailCloseBtn;
        private EquipmentSlot currentDetailSlot;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            // ★ 关键修复：只有slotsContainer已赋值时才尝试初始化
            // AddComponent时OnEnable会在slotsContainer赋值之前触发，此时不应标记已初始化
            if (slotsContainer != null && !isInitialized)
            {
                Initialize();
            }

            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged += OnEquipChanged;
            }
            RefreshUI();
        }

        private void OnDisable()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged -= OnEquipChanged;
            }
            HideDetailPopup();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Initialize()
        {
            if (isInitialized) return;

            // ★ 必须有slotsContainer才能初始化
            if (slotsContainer == null)
            {
                Debug.LogWarning("[SimpleEquipmentPanel] slotsContainer未设置，跳过初始化");
                return;
            }

            isInitialized = true;
            slotUIMap.Clear();

            // 定义默认槽位顺序（与GameSceneSetup.CreateEquipmentPanel一致）
            EquipmentSlot[] defaultSlots = {
                EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Helmet,
                EquipmentSlot.Pants, EquipmentSlot.Ring, EquipmentSlot.Necklace
            };

            for (int i = 0; i < slotsContainer.childCount; i++)
            {
                Transform slotTf = slotsContainer.GetChild(i);
                string slotName = slotTf.name;

                EquipmentSlot? slot = null;

                // 先尝试名称匹配
                if (slotName.Contains("武器") || slotName.ToLower().Contains("weapon"))
                    slot = EquipmentSlot.Weapon;
                else if (slotName.Contains("护甲") || slotName.ToLower().Contains("armor") || slotName.ToLower().Contains("body"))
                    slot = EquipmentSlot.Armor;
                else if (slotName.Contains("头盔") || slotName.ToLower().Contains("helmet") || slotName.ToLower().Contains("head"))
                    slot = EquipmentSlot.Helmet;
                else if (slotName.Contains("护腿") || slotName.Contains("裤") || slotName.ToLower().Contains("pants") || slotName.ToLower().Contains("leg"))
                    slot = EquipmentSlot.Pants;
                else if (slotName.Contains("戒指") || slotName.ToLower().Contains("ring"))
                    slot = EquipmentSlot.Ring;
                else if (slotName.Contains("项链") || slotName.ToLower().Contains("neck") || slotName.ToLower().Contains("amulet"))
                    slot = EquipmentSlot.Necklace;

                // 如果名称没匹配到，按索引顺序分配
                if (!slot.HasValue && i < defaultSlots.Length)
                {
                    slot = defaultSlots[i];
                }

                if (slot.HasValue)
                {
                    slotUIMap[slot.Value] = slotTf;

                    // ★ 添加Button和点击事件
                    EquipmentSlot capturedSlot = slot.Value;
                    Button btn = slotTf.GetComponent<Button>();
                    if (btn == null)
                    {
                        btn = slotTf.gameObject.AddComponent<Button>();
                        btn.targetGraphic = slotTf.GetComponent<Image>();
                    }

                    // 设置高亮色让按钮有反馈
                    var colors = btn.colors;
                    colors.highlightedColor = new Color(0.4f, 0.45f, 0.5f);
                    colors.pressedColor = new Color(0.2f, 0.25f, 0.3f);
                    colors.selectedColor = new Color(0.35f, 0.4f, 0.45f);
                    btn.colors = colors;

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnSlotClicked(capturedSlot));
                }
            }

            Debug.Log($"[SimpleEquipmentPanel] 初始化完成, 找到 {slotUIMap.Count} 个装备槽");
        }

        void OnEquipChanged(EquipmentSlot slot, Equipment equip)
        {
            RefreshUI();
        }

        /// <summary>
        /// 刷新装备面板UI
        /// </summary>
        public void RefreshUI()
        {
            if (EquipmentManager.Instance == null) return;

            // ★ 如果还未初始化（slotsContainer刚赋值），尝试初始化
            if (!isInitialized && slotsContainer != null)
            {
                Initialize();
            }

            var allEquips = EquipmentManager.Instance.GetAllEquipments();

            // 更新每个装备槽
            foreach (var kvp in slotUIMap)
            {
                EquipmentSlot slot = kvp.Key;
                Transform slotTf = kvp.Value;

                Equipment equip = null;
                allEquips.TryGetValue(slot, out equip);

                UpdateSlotUI(slotTf, slot, equip);
            }

            // 更新总属性加成
            UpdateStatsText();
        }

        /// <summary>
        /// 更新单个槽位UI
        /// </summary>
        void UpdateSlotUI(Transform slotTf, EquipmentSlot slot, Equipment equip)
        {
            // 查找名称Text
            Text nameText = null;
            Transform nameTf = slotTf.Find("Name");
            if (nameTf == null) nameTf = slotTf.Find("EquipName");
            if (nameTf == null)
            {
                GameObject nameGO = new GameObject("Name");
                nameGO.transform.SetParent(slotTf, false);
                RectTransform nameRect = nameGO.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0);
                nameRect.anchorMax = new Vector2(1, 0.35f);
                nameRect.offsetMin = new Vector2(2, 2);
                nameRect.offsetMax = new Vector2(-2, -2);
                nameText = nameGO.AddComponent<Text>();
                nameText.fontSize = 12;
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.color = Color.white;
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font != null) nameText.font = font;

                Outline outline = nameGO.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, -1);
            }
            else
            {
                nameText = nameTf.GetComponent<Text>();
            }
            if (nameText != null) nameText.raycastTarget = false;

            // 查找图标Image
            Image slotIcon = null;
            Transform iconTf = slotTf.Find("Icon");
            if (iconTf == null) iconTf = slotTf.Find("EquipIcon");
            if (iconTf == null)
            {
                GameObject iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(slotTf, false);
                RectTransform iconRect = iconGO.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.15f, 0.3f);
                iconRect.anchorMax = new Vector2(0.85f, 0.95f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                slotIcon = iconGO.AddComponent<Image>();
            }
            else
            {
                slotIcon = iconTf.GetComponent<Image>();
            }
            if (slotIcon != null) slotIcon.raycastTarget = false;

            // ★ 确保SlotLabel也不拦截点击
            Transform labelTf = slotTf.Find("SlotLabel");
            if (labelTf != null)
            {
                Image labelImg = labelTf.GetComponent<Image>();
                if (labelImg != null) labelImg.raycastTarget = false;
                Text labelText = labelTf.GetComponentInChildren<Text>();
                if (labelText != null) labelText.raycastTarget = false;
            }

            if (equip != null)
            {
                // 已装备 - 显示装备信息
                if (nameText != null)
                {
                    nameText.text = equip.name;
                    nameText.color = GetQualityColor(equip.quality);
                    nameText.fontSize = 14;
                }
                if (slotIcon != null)
                {
                    slotIcon.color = GetSlotColor(slot);
                    slotIcon.gameObject.SetActive(true);
                }

                // 改变槽位背景色表示已装备
                Image bgImage = slotTf.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = new Color(0.25f, 0.3f, 0.35f);
                }
            }
            else
            {
                // 空槽位
                if (nameText != null)
                {
                    nameText.text = GetSlotDisplayName(slot);
                    nameText.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                    nameText.fontSize = 14;
                }
                if (slotIcon != null)
                {
                    slotIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                }

                Image bgImage = slotTf.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = new Color(0.3f, 0.3f, 0.35f);
                }
            }
        }

        /// <summary>
        /// 更新属性加成文字
        /// </summary>
        void UpdateStatsText()
        {
            if (statsText == null) return;

            if (EquipmentManager.Instance == null)
            {
                statsText.text = "攻击: +0\n防御: +0\n生命: +0";
                return;
            }

            var stats = EquipmentManager.Instance.GetTotalStats();
            string critText = stats.critRate > 0 ? $"\n暴击: +{stats.critRate:P1}" : "";
            statsText.text = $"攻击: +{stats.attack:F0}  防御: +{stats.defense:F0}\n生命: +{stats.health:F0}{critText}";
        }

        // ===== 装备详情弹窗系统 =====

        /// <summary>
        /// 点击装备槽 - 显示装备详情弹窗
        /// </summary>
        void OnSlotClicked(EquipmentSlot slot)
        {
            Debug.Log($"[SimpleEquipmentPanel] 点击装备槽: {slot}");

            if (EquipmentManager.Instance == null) return;

            Equipment equip = EquipmentManager.Instance.GetEquipment(slot);
            if (equip == null)
            {
                Debug.Log($"[SimpleEquipmentPanel] 槽位 {slot} 无装备");
                HideDetailPopup();
                return;
            }

            // 显示装备详情
            currentDetailSlot = slot;
            ShowDetailPopup(equip, slot);
        }

        /// <summary>
        /// 创建装备详情弹窗
        /// </summary>
        void CreateDetailPopup()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 弹窗容器
            detailPopup = new GameObject("EquipDetailPopup");
            detailPopup.transform.SetParent(transform, false);
            RectTransform popupRect = detailPopup.AddComponent<RectTransform>();
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.sizeDelta = new Vector2(280, 260);
            popupRect.anchoredPosition = new Vector2(0, 20);

            // 弹窗背景
            Image popupBg = detailPopup.AddComponent<Image>();
            popupBg.color = new Color(0.12f, 0.12f, 0.18f, 0.97f);

            // 外边框
            Outline popupOutline = detailPopup.AddComponent<Outline>();
            popupOutline.effectColor = new Color(0.5f, 0.5f, 0.6f);
            popupOutline.effectDistance = new Vector2(2, -2);

            // 装备品质标签（顶部颜色条）
            GameObject qualityBarGO = new GameObject("QualityBar");
            qualityBarGO.transform.SetParent(detailPopup.transform, false);
            RectTransform qbRect = qualityBarGO.AddComponent<RectTransform>();
            qbRect.anchorMin = new Vector2(0, 1);
            qbRect.anchorMax = new Vector2(1, 1);
            qbRect.anchoredPosition = new Vector2(0, 0);
            qbRect.sizeDelta = new Vector2(0, 6);
            Image qbImage = qualityBarGO.AddComponent<Image>();
            qbImage.color = Color.white;
            qbImage.raycastTarget = false;

            // 装备名称
            GameObject nameGO = new GameObject("DetailName");
            nameGO.transform.SetParent(detailPopup.transform, false);
            RectTransform nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.anchoredPosition = new Vector2(0, -25);
            nameRect.sizeDelta = new Vector2(-20, 35);
            detailNameText = nameGO.AddComponent<Text>();
            detailNameText.fontSize = 22;
            detailNameText.alignment = TextAnchor.MiddleCenter;
            detailNameText.color = Color.white;
            if (defaultFont != null) detailNameText.font = defaultFont;
            detailNameText.raycastTarget = false;

            Outline nameOutline = nameGO.AddComponent<Outline>();
            nameOutline.effectColor = Color.black;
            nameOutline.effectDistance = new Vector2(1, -1);

            // 品质文字
            GameObject qualityGO = new GameObject("DetailQuality");
            qualityGO.transform.SetParent(detailPopup.transform, false);
            RectTransform qualityRect = qualityGO.AddComponent<RectTransform>();
            qualityRect.anchorMin = new Vector2(0, 1);
            qualityRect.anchorMax = new Vector2(1, 1);
            qualityRect.anchoredPosition = new Vector2(0, -52);
            qualityRect.sizeDelta = new Vector2(-20, 22);
            detailQualityText = qualityGO.AddComponent<Text>();
            detailQualityText.fontSize = 14;
            detailQualityText.alignment = TextAnchor.MiddleCenter;
            detailQualityText.color = Color.gray;
            if (defaultFont != null) detailQualityText.font = defaultFont;
            detailQualityText.raycastTarget = false;

            // 分割线
            GameObject dividerGO = new GameObject("Divider");
            dividerGO.transform.SetParent(detailPopup.transform, false);
            RectTransform divRect = dividerGO.AddComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.1f, 1);
            divRect.anchorMax = new Vector2(0.9f, 1);
            divRect.anchoredPosition = new Vector2(0, -68);
            divRect.sizeDelta = new Vector2(0, 2);
            Image divImage = dividerGO.AddComponent<Image>();
            divImage.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);
            divImage.raycastTarget = false;

            // 属性详情文字
            GameObject statsGO = new GameObject("DetailStats");
            statsGO.transform.SetParent(detailPopup.transform, false);
            RectTransform statsRect = statsGO.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 1);
            statsRect.anchorMax = new Vector2(1, 1);
            statsRect.anchoredPosition = new Vector2(0, -115);
            statsRect.sizeDelta = new Vector2(-30, 80);
            detailStatsText = statsGO.AddComponent<Text>();
            detailStatsText.fontSize = 17;
            detailStatsText.alignment = TextAnchor.MiddleLeft;
            detailStatsText.color = new Color(0.85f, 0.85f, 0.85f);
            detailStatsText.lineSpacing = 1.3f;
            if (defaultFont != null) detailStatsText.font = defaultFont;
            detailStatsText.raycastTarget = false;

            // 卸下按钮
            GameObject unequipBtnGO = new GameObject("UnequipBtn");
            unequipBtnGO.transform.SetParent(detailPopup.transform, false);
            RectTransform ubRect = unequipBtnGO.AddComponent<RectTransform>();
            ubRect.anchorMin = new Vector2(0.5f, 0);
            ubRect.anchorMax = new Vector2(0.5f, 0);
            ubRect.anchoredPosition = new Vector2(-55, 35);
            ubRect.sizeDelta = new Vector2(100, 38);
            Image ubBg = unequipBtnGO.AddComponent<Image>();
            ubBg.color = new Color(0.7f, 0.3f, 0.2f);
            detailUnequipBtn = unequipBtnGO.AddComponent<Button>();
            detailUnequipBtn.targetGraphic = ubBg;
            var ubColors = detailUnequipBtn.colors;
            ubColors.highlightedColor = new Color(0.85f, 0.4f, 0.3f);
            ubColors.pressedColor = new Color(0.55f, 0.2f, 0.15f);
            detailUnequipBtn.colors = ubColors;
            detailUnequipBtn.onClick.AddListener(OnDetailUnequip);

            // 卸下按钮文字
            GameObject ubTextGO = new GameObject("Text");
            ubTextGO.transform.SetParent(unequipBtnGO.transform, false);
            RectTransform ubtRect = ubTextGO.AddComponent<RectTransform>();
            ubtRect.anchorMin = Vector2.zero;
            ubtRect.anchorMax = Vector2.one;
            ubtRect.offsetMin = Vector2.zero;
            ubtRect.offsetMax = Vector2.zero;
            Text ubText = ubTextGO.AddComponent<Text>();
            ubText.text = "卸下";
            ubText.fontSize = 18;
            ubText.alignment = TextAnchor.MiddleCenter;
            ubText.color = Color.white;
            if (defaultFont != null) ubText.font = defaultFont;
            ubText.raycastTarget = false;

            // 关闭按钮
            GameObject closeBtnGO = new GameObject("CloseBtn");
            closeBtnGO.transform.SetParent(detailPopup.transform, false);
            RectTransform cbRect = closeBtnGO.AddComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.5f, 0);
            cbRect.anchorMax = new Vector2(0.5f, 0);
            cbRect.anchoredPosition = new Vector2(55, 35);
            cbRect.sizeDelta = new Vector2(100, 38);
            Image cbBg = closeBtnGO.AddComponent<Image>();
            cbBg.color = new Color(0.35f, 0.35f, 0.4f);
            detailCloseBtn = closeBtnGO.AddComponent<Button>();
            detailCloseBtn.targetGraphic = cbBg;
            var cbColors = detailCloseBtn.colors;
            cbColors.highlightedColor = new Color(0.5f, 0.5f, 0.55f);
            cbColors.pressedColor = new Color(0.25f, 0.25f, 0.3f);
            detailCloseBtn.colors = cbColors;
            detailCloseBtn.onClick.AddListener(HideDetailPopup);

            // 关闭按钮文字
            GameObject cbTextGO = new GameObject("Text");
            cbTextGO.transform.SetParent(closeBtnGO.transform, false);
            RectTransform cbtRect = cbTextGO.AddComponent<RectTransform>();
            cbtRect.anchorMin = Vector2.zero;
            cbtRect.anchorMax = Vector2.one;
            cbtRect.offsetMin = Vector2.zero;
            cbtRect.offsetMax = Vector2.zero;
            Text cbText = cbTextGO.AddComponent<Text>();
            cbText.text = "关闭";
            cbText.fontSize = 18;
            cbText.alignment = TextAnchor.MiddleCenter;
            cbText.color = Color.white;
            if (defaultFont != null) cbText.font = defaultFont;
            cbText.raycastTarget = false;

            detailPopup.SetActive(false);
        }

        /// <summary>
        /// 显示装备详情弹窗
        /// </summary>
        void ShowDetailPopup(Equipment equip, EquipmentSlot slot)
        {
            if (detailPopup == null)
            {
                CreateDetailPopup();
            }

            detailPopup.SetActive(true);

            // 更新名称
            if (detailNameText != null)
            {
                detailNameText.text = equip.name;
                detailNameText.color = GetQualityColor(equip.quality);
            }

            // 更新品质文字
            if (detailQualityText != null)
            {
                string qualityName = GetQualityName(equip.quality);
                string slotDisplayName = GetSlotDisplayName(slot);
                detailQualityText.text = $"{qualityName} · {slotDisplayName}";
                detailQualityText.color = GetQualityColor(equip.quality);
            }

            // 品质颜色条
            Transform qBar = detailPopup.transform.Find("QualityBar");
            if (qBar != null)
            {
                Image qbImg = qBar.GetComponent<Image>();
                if (qbImg != null) qbImg.color = GetQualityColor(equip.quality);
            }

            // 更新属性详情
            if (detailStatsText != null)
            {
                string statsStr = "";
                if (equip.attackBonus > 0)
                    statsStr += $"  攻击力  <color=#FF6B6B>+{equip.attackBonus:F0}</color>\n";
                if (equip.defenseBonus > 0)
                    statsStr += $"  防御力  <color=#6BB5FF>+{equip.defenseBonus:F0}</color>\n";
                if (equip.hpBonus > 0)
                    statsStr += $"  生命值  <color=#6BFF6B>+{equip.hpBonus:F0}</color>\n";
                if (equip.critRateBonus > 0)
                    statsStr += $"  暴击率  <color=#FFD700>+{equip.critRateBonus:P1}</color>\n";

                if (string.IsNullOrEmpty(statsStr))
                    statsStr = "  无属性加成";

                detailStatsText.text = statsStr.TrimEnd('\n');
                detailStatsText.supportRichText = true;
            }
        }

        /// <summary>
        /// 隐藏装备详情弹窗
        /// </summary>
        void HideDetailPopup()
        {
            if (detailPopup != null)
            {
                detailPopup.SetActive(false);
            }
        }

        /// <summary>
        /// 点击"卸下"按钮
        /// </summary>
        void OnDetailUnequip()
        {
            if (EquipmentManager.Instance == null) return;

            Equipment equip = EquipmentManager.Instance.GetEquipment(currentDetailSlot);
            if (equip == null)
            {
                HideDetailPopup();
                return;
            }

            // 卸下装备放回背包
            Equipment removed = EquipmentManager.Instance.Unequip(currentDetailSlot);
            if (removed != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(removed.id, 1);
                Debug.Log($"[装备] 卸下: {removed.name} -> 背包");
            }

            HideDetailPopup();
            RefreshUI();

            // 同步刷新背包面板
            if (SimpleInventoryPanel.Instance != null)
            {
                SimpleInventoryPanel.Instance.RefreshUI();
            }
        }

        // ===== 辅助方法 =====

        string GetQualityName(EquipmentQuality quality)
        {
            switch (quality)
            {
                case EquipmentQuality.White:  return "普通";
                case EquipmentQuality.Green:  return "优秀";
                case EquipmentQuality.Blue:   return "精良";
                case EquipmentQuality.Purple: return "史诗";
                case EquipmentQuality.Orange: return "传说";
                default: return "未知";
            }
        }

        Color GetQualityColor(EquipmentQuality quality)
        {
            switch (quality)
            {
                case EquipmentQuality.White:  return Color.white;
                case EquipmentQuality.Green:  return new Color(0.3f, 1f, 0.3f);
                case EquipmentQuality.Blue:   return new Color(0.4f, 0.6f, 1f);
                case EquipmentQuality.Purple: return new Color(0.8f, 0.4f, 1f);
                case EquipmentQuality.Orange: return new Color(1f, 0.6f, 0.2f);
                default: return Color.gray;
            }
        }

        Color GetSlotColor(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:   return new Color(0.9f, 0.4f, 0.2f, 0.8f);
                case EquipmentSlot.Armor:    return new Color(0.3f, 0.5f, 0.9f, 0.8f);
                case EquipmentSlot.Helmet:   return new Color(0.6f, 0.6f, 0.9f, 0.8f);
                case EquipmentSlot.Ring:     return new Color(0.9f, 0.8f, 0.3f, 0.8f);
                case EquipmentSlot.Necklace: return new Color(0.4f, 0.9f, 0.7f, 0.8f);
                case EquipmentSlot.Pants:    return new Color(0.6f, 0.4f, 0.3f, 0.8f);
                default: return Color.gray;
            }
        }

        string GetSlotDisplayName(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:   return "武器";
                case EquipmentSlot.Armor:    return "护甲";
                case EquipmentSlot.Helmet:   return "头盔";
                case EquipmentSlot.Ring:     return "戒指";
                case EquipmentSlot.Necklace: return "项链";
                case EquipmentSlot.Pants:    return "护腿";
                default: return "空";
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            // ★ 确保初始化（此时slotsContainer一定已赋值）
            if (!isInitialized) Initialize();
            RefreshUI();
        }

        public void Hide()
        {
            HideDetailPopup();
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }
    }
}
