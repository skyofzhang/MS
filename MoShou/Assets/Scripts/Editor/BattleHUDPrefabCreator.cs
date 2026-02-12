using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor工具：一键生成战斗HUD Prefab
/// 菜单: MoShou/创建战斗HUD Prefab
/// 包含：顶部状态栏 + 左侧快捷按钮(5个) + 右下技能栏 + 虚拟摇杆 + 技能动作按钮 + 暂停按钮 + 暂停面板
/// </summary>
public class BattleHUDPrefabCreator
{
    [MenuItem("MoShou/创建战斗HUD Prefab/0. 全部生成")]
    public static void CreateBattleHUDPrefab()
    {
        EnsureDirectory("Assets/Resources/Prefabs/UI");
        EnsureDirectory("Assets/Resources/Sprites/UI/HUD");

        // === 生成圆形Sprite资源（供摇杆使用）===
        Sprite circleSprite = CreateOrLoadCircleSprite();

        // === 加载所有Sprite资源 ===
        Sprite portraitFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_Portrait_Frame.png");
        Sprite hpBarFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_HPBar_Frame.png");
        Sprite hpBarFill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_HPBar_Fill.png");
        Sprite topBanner = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_TopBanner.png");
        Sprite coinIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Common/UI_Icon_Coin_Small.png");
        Sprite circleFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_Btn_Circle_Frame.png");
        Sprite btnShop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_Btn_Shop.png");
        Sprite btnBag = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_Btn_Bag.png");
        Sprite btnSkill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_Btn_Skill.png");
        Sprite btnMap = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/HUD/UI_HUD_Btn_Map.png");
        Sprite skillSlotBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Skills/UI_Skill_Slot_BG.png");
        Sprite pauseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Buttons/UI_Btn_Pause.png");
        Sprite multiShotIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Skills/UI_Skill_Icon_MultiShot.png");
        Sprite pierceIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Skills/UI_Skill_Icon_Pierce.png");
        Sprite battleShoutIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/Skills/UI_Skill_Icon_BattleShout.png");
        Sprite equipWeaponIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/RPGKit/Slot_Equip_Weapon.png");
        Font defaultFont = GetFont();

        // === Root: BattleHUD (全屏透明容器) ===
        GameObject root = new GameObject("BattleHUD");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // ================================================================
        // 1. 顶部HUD状态栏
        // ================================================================
        GameObject hudGO = CreateChild(root, "HUD");
        RectTransform hudRect = hudGO.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 0.88f);
        hudRect.anchorMax = new Vector2(1, 1);
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        Image hudBg = hudGO.AddComponent<Image>();
        hudBg.color = new Color(0, 0, 0, 0.5f);
        hudBg.raycastTarget = false;

        // --- 玩家头像框 (左上) ---
        GameObject iconFrameGO = CreateChild(hudGO, "PlayerIconFrame");
        RectTransform iconFrameRect = iconFrameGO.GetComponent<RectTransform>();
        iconFrameRect.anchorMin = new Vector2(0, 0.5f);
        iconFrameRect.anchorMax = new Vector2(0, 0.5f);
        iconFrameRect.anchoredPosition = new Vector2(60, 10);
        iconFrameRect.sizeDelta = new Vector2(80, 80);
        Image iconFrameImg = iconFrameGO.AddComponent<Image>();
        if (portraitFrame != null)
        {
            iconFrameImg.sprite = portraitFrame;
            iconFrameImg.color = Color.white;
        }
        else
        {
            iconFrameImg.color = new Color(0.4f, 0.4f, 0.4f);
        }
        iconFrameImg.raycastTarget = true; // 可点击，用于打开角色详情
        Button portraitBtn = iconFrameGO.AddComponent<Button>();
        portraitBtn.targetGraphic = iconFrameImg;
        portraitBtn.transition = Selectable.Transition.ColorTint;

        // --- 血条背景 ---
        GameObject healthBgGO = CreateChild(hudGO, "HealthBarBG");
        RectTransform hbRect = healthBgGO.GetComponent<RectTransform>();
        hbRect.anchorMin = new Vector2(0, 0.5f);
        hbRect.anchorMax = new Vector2(0, 0.5f);
        hbRect.anchoredPosition = new Vector2(270, 20);
        hbRect.sizeDelta = new Vector2(340, 32);
        Image hbImage = healthBgGO.AddComponent<Image>();
        if (hpBarFrame != null)
        {
            hbImage.sprite = hpBarFrame;
            hbImage.type = Image.Type.Sliced;
            hbImage.color = Color.white;
        }
        else
        {
            hbImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }

        // 血条填充
        GameObject healthFillGO = CreateChild(healthBgGO, "HealthFill");
        RectTransform hfRect = healthFillGO.GetComponent<RectTransform>();
        hfRect.anchorMin = Vector2.zero;
        hfRect.anchorMax = Vector2.one;
        hfRect.offsetMin = new Vector2(4, 4);
        hfRect.offsetMax = new Vector2(-4, -4);
        Image healthFillImg = healthFillGO.AddComponent<Image>();
        healthFillImg.type = Image.Type.Filled;
        healthFillImg.fillMethod = Image.FillMethod.Horizontal;
        if (hpBarFill != null)
        {
            healthFillImg.sprite = hpBarFill;
            healthFillImg.color = Color.white;
        }
        else
        {
            healthFillImg.color = new Color(0.3f, 0.8f, 0.3f);
        }

        // SimpleHealthBar组件
        SimpleHealthBar healthBar = healthBgGO.AddComponent<SimpleHealthBar>();
        healthBar.fillImage = healthFillImg;

        // 血条上HP文字
        GameObject hpTextGO = CreateChild(healthBgGO, "HealthText");
        RectTransform hpTextRect = hpTextGO.GetComponent<RectTransform>();
        hpTextRect.anchorMin = Vector2.zero;
        hpTextRect.anchorMax = Vector2.one;
        hpTextRect.offsetMin = Vector2.zero;
        hpTextRect.offsetMax = Vector2.zero;
        Text hpText = hpTextGO.AddComponent<Text>();
        hpText.text = "100/100";
        hpText.fontSize = 20;
        hpText.fontStyle = FontStyle.Bold;
        hpText.alignment = TextAnchor.MiddleCenter;
        hpText.color = Color.white;
        hpText.font = defaultFont;
        hpText.raycastTarget = false;
        Outline hpOutline = hpTextGO.AddComponent<Outline>();
        hpOutline.effectColor = Color.black;
        hpOutline.effectDistance = new Vector2(1, -1);

        // --- 波次Banner (居中) ---
        GameObject waveBgGO = CreateChild(hudGO, "WaveBG");
        RectTransform waveBgRect = waveBgGO.GetComponent<RectTransform>();
        waveBgRect.anchorMin = new Vector2(0.5f, 0.5f);
        waveBgRect.anchorMax = new Vector2(0.5f, 0.5f);
        waveBgRect.anchoredPosition = new Vector2(0, 10);
        waveBgRect.sizeDelta = new Vector2(280, 48);
        Image waveBgImg = waveBgGO.AddComponent<Image>();
        if (topBanner != null)
        {
            waveBgImg.sprite = topBanner;
            waveBgImg.type = Image.Type.Sliced;
            waveBgImg.color = Color.white;
        }
        else
        {
            waveBgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
        }
        waveBgImg.raycastTarget = false;

        // 波次文字
        GameObject waveTextGO = CreateChild(waveBgGO, "WaveText");
        RectTransform waveTextRect = waveTextGO.GetComponent<RectTransform>();
        waveTextRect.anchorMin = Vector2.zero;
        waveTextRect.anchorMax = Vector2.one;
        waveTextRect.offsetMin = Vector2.zero;
        waveTextRect.offsetMax = Vector2.zero;
        Text waveText = waveTextGO.AddComponent<Text>();
        waveText.text = "波次 1/3  击杀 0";
        waveText.fontSize = 22;
        waveText.fontStyle = FontStyle.Bold;
        waveText.alignment = TextAnchor.MiddleCenter;
        waveText.color = Color.white;
        waveText.font = defaultFont;
        waveText.raycastTarget = false;

        // --- 金币Icon (右侧) ---
        GameObject goldIconGO = CreateChild(hudGO, "GoldIcon");
        RectTransform goldIconRect = goldIconGO.GetComponent<RectTransform>();
        goldIconRect.anchorMin = new Vector2(1, 0.5f);
        goldIconRect.anchorMax = new Vector2(1, 0.5f);
        goldIconRect.anchoredPosition = new Vector2(-160, 0);
        goldIconRect.sizeDelta = new Vector2(36, 36);
        Image goldIconImg = goldIconGO.AddComponent<Image>();
        if (coinIcon != null)
        {
            goldIconImg.sprite = coinIcon;
            goldIconImg.color = Color.white;
        }
        else
        {
            goldIconImg.color = new Color(1f, 0.85f, 0.2f);
        }
        goldIconImg.raycastTarget = false;

        // 金币数量文字
        GameObject goldTextGO = CreateChild(hudGO, "GoldText");
        RectTransform goldTextRect = goldTextGO.GetComponent<RectTransform>();
        goldTextRect.anchorMin = new Vector2(1, 0.5f);
        goldTextRect.anchorMax = new Vector2(1, 0.5f);
        goldTextRect.anchoredPosition = new Vector2(-56, 0);
        goldTextRect.sizeDelta = new Vector2(100, 36);
        Text goldText = goldTextGO.AddComponent<Text>();
        goldText.text = "0";
        goldText.fontSize = 30;
        goldText.fontStyle = FontStyle.Bold;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = new Color(1f, 0.85f, 0.2f);
        goldText.font = defaultFont;

        // --- 等级文字 (头像下方) ---
        GameObject lvTextGO = CreateChild(hudGO, "LevelText");
        RectTransform lvTextRect = lvTextGO.GetComponent<RectTransform>();
        lvTextRect.anchorMin = new Vector2(0, 0.5f);
        lvTextRect.anchorMax = new Vector2(0, 0.5f);
        lvTextRect.anchoredPosition = new Vector2(60, -50);
        lvTextRect.sizeDelta = new Vector2(90, 26);
        Text lvText = lvTextGO.AddComponent<Text>();
        lvText.text = "Lv.1";
        lvText.fontSize = 20;
        lvText.fontStyle = FontStyle.Bold;
        lvText.alignment = TextAnchor.MiddleCenter;
        lvText.color = new Color(1f, 0.9f, 0.5f);
        lvText.font = defaultFont;
        lvText.raycastTarget = false;
        Outline lvOutline = lvTextGO.AddComponent<Outline>();
        lvOutline.effectColor = Color.black;
        lvOutline.effectDistance = new Vector2(1, -1);

        // --- ATK文字 (血条下方) ---
        GameObject atkTextGO = CreateChild(hudGO, "AttackText");
        RectTransform atkTextRect = atkTextGO.GetComponent<RectTransform>();
        atkTextRect.anchorMin = new Vector2(0, 0.5f);
        atkTextRect.anchorMax = new Vector2(0, 0.5f);
        atkTextRect.anchoredPosition = new Vector2(180, -18);
        atkTextRect.sizeDelta = new Vector2(150, 26);
        Text atkText = atkTextGO.AddComponent<Text>();
        atkText.text = "攻击: 15";
        atkText.fontSize = 20;
        atkText.alignment = TextAnchor.MiddleLeft;
        atkText.color = new Color(1f, 0.5f, 0.3f);
        atkText.font = defaultFont;
        atkText.raycastTarget = false;
        Outline atkOutline = atkTextGO.AddComponent<Outline>();
        atkOutline.effectColor = Color.black;
        atkOutline.effectDistance = new Vector2(1, -1);

        // --- DEF文字 (ATK右侧) ---
        GameObject defTextGO = CreateChild(hudGO, "DefenseText");
        RectTransform defTextRect = defTextGO.GetComponent<RectTransform>();
        defTextRect.anchorMin = new Vector2(0, 0.5f);
        defTextRect.anchorMax = new Vector2(0, 0.5f);
        defTextRect.anchoredPosition = new Vector2(340, -18);
        defTextRect.sizeDelta = new Vector2(150, 26);
        Text defText = defTextGO.AddComponent<Text>();
        defText.text = "防御: 5";
        defText.fontSize = 20;
        defText.alignment = TextAnchor.MiddleLeft;
        defText.color = new Color(0.3f, 0.7f, 1f);
        defText.font = defaultFont;
        defText.raycastTarget = false;
        Outline defOutline = defTextGO.AddComponent<Outline>();
        defOutline.effectColor = Color.black;
        defOutline.effectDistance = new Vector2(1, -1);

        // ================================================================
        // 2. 左侧快捷按钮 (商店/背包/技能/装备/地图) — 5个
        // ================================================================
        string[] sideNames = { "商店", "背包", "技能", "装备", "地图" };
        Sprite[] sideIcons = { btnShop, btnBag, btnSkill, equipWeaponIcon, btnMap };
        Button[] sideButtons = new Button[5];

        for (int i = 0; i < sideNames.Length; i++)
        {
            GameObject btnGO = CreateChild(root, $"SideBtn_{sideNames[i]}");
            RectTransform btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(0, 1);
            btnRect.anchoredPosition = new Vector2(50, -280 - i * 90);
            btnRect.sizeDelta = new Vector2(70, 70);

            // 圆形帧背景
            Image btnBg = btnGO.AddComponent<Image>();
            if (circleFrame != null)
            {
                btnBg.sprite = circleFrame;
                btnBg.color = Color.white;
            }
            else
            {
                btnBg.color = new Color(0.15f, 0.2f, 0.3f, 0.85f);
            }

            // 按钮icon
            if (sideIcons[i] != null)
            {
                GameObject iconGO = CreateChild(btnGO, "Icon");
                RectTransform iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.15f, 0.15f);
                iconRect.anchorMax = new Vector2(0.85f, 0.85f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                Image iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = sideIcons[i];
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;
            }

            // 文字标签
            GameObject labelGO = CreateChild(btnGO, "Label");
            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0);
            labelRect.pivot = new Vector2(0.5f, 1);
            labelRect.anchoredPosition = new Vector2(0, -2);
            labelRect.sizeDelta = new Vector2(70, 20);
            Text labelText = labelGO.AddComponent<Text>();
            labelText.text = sideNames[i];
            labelText.fontSize = 14;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color(0.8f, 0.8f, 0.8f);
            labelText.font = defaultFont;
            labelText.raycastTarget = false;

            // Button组件
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            sideButtons[i] = btn;
        }

        // ================================================================
        // 3. 右下技能栏 (4个slot横排)
        // ================================================================
        GameObject skillBarGO = CreateChild(root, "SkillBar");
        RectTransform skillBarRect = skillBarGO.GetComponent<RectTransform>();
        skillBarRect.anchorMin = new Vector2(1, 0);
        skillBarRect.anchorMax = new Vector2(1, 0);
        skillBarRect.pivot = new Vector2(1, 0);
        skillBarRect.anchoredPosition = new Vector2(-15, 30);
        skillBarRect.sizeDelta = new Vector2(360, 90);

        HorizontalLayoutGroup hlg = skillBarGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(5, 5, 5, 5);

        for (int i = 0; i < 4; i++)
        {
            GameObject slotGO = CreateChild(skillBarGO, $"SkillSlot_{i}");
            LayoutElement le = slotGO.AddComponent<LayoutElement>();
            le.preferredWidth = 80;
            le.preferredHeight = 80;

            // slot帧
            Image slotBg = slotGO.AddComponent<Image>();
            if (skillSlotBg != null)
            {
                slotBg.sprite = skillSlotBg;
                slotBg.type = Image.Type.Sliced;
                slotBg.color = Color.white;
            }
            else
            {
                slotBg.color = new Color(0.18f, 0.2f, 0.28f, 0.85f);
            }

            // 技能icon占位
            GameObject iconGO = CreateChild(slotGO, "SkillIcon");
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.color = new Color(0.3f, 0.3f, 0.35f, 0.5f);
            iconImg.raycastTarget = false;

            // 等级badge
            GameObject lvBadge = CreateChild(slotGO, "LvBadge");
            RectTransform lvBadgeRect = lvBadge.GetComponent<RectTransform>();
            lvBadgeRect.anchorMin = new Vector2(0.5f, 0);
            lvBadgeRect.anchorMax = new Vector2(0.5f, 0);
            lvBadgeRect.anchoredPosition = new Vector2(0, -2);
            lvBadgeRect.sizeDelta = new Vector2(50, 20);
            Image lvBadgeBg = lvBadge.AddComponent<Image>();
            lvBadgeBg.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            lvBadgeBg.raycastTarget = false;

            GameObject lvBadgeTextGO = CreateChild(lvBadge, "Text");
            RectTransform lvBadgeTextRect = lvBadgeTextGO.GetComponent<RectTransform>();
            lvBadgeTextRect.anchorMin = Vector2.zero;
            lvBadgeTextRect.anchorMax = Vector2.one;
            lvBadgeTextRect.offsetMin = Vector2.zero;
            lvBadgeTextRect.offsetMax = Vector2.zero;
            Text lvBadgeText = lvBadgeTextGO.AddComponent<Text>();
            lvBadgeText.text = $"Lv.{i + 1}";
            lvBadgeText.fontSize = 14;
            lvBadgeText.alignment = TextAnchor.MiddleCenter;
            lvBadgeText.color = Color.white;
            lvBadgeText.font = defaultFont;
            lvBadgeText.raycastTarget = false;
        }

        // ================================================================
        // 4. 虚拟摇杆 (左下角)
        // ================================================================
        GameObject joystickGO = CreateChild(root, "VirtualJoystick");
        RectTransform joyRect = joystickGO.GetComponent<RectTransform>();
        joyRect.anchorMin = new Vector2(0, 0);
        joyRect.anchorMax = new Vector2(0, 0);
        joyRect.pivot = new Vector2(0, 0);
        joyRect.anchoredPosition = new Vector2(50, 50);
        joyRect.sizeDelta = new Vector2(200, 200);

        // 摇杆背景
        GameObject joyBgGO = CreateChild(joystickGO, "Background");
        RectTransform joyBgRect = joyBgGO.GetComponent<RectTransform>();
        joyBgRect.anchorMin = new Vector2(0.5f, 0.5f);
        joyBgRect.anchorMax = new Vector2(0.5f, 0.5f);
        joyBgRect.anchoredPosition = Vector2.zero;
        joyBgRect.sizeDelta = new Vector2(180, 180);
        Image joyBgImg = joyBgGO.AddComponent<Image>();
        joyBgImg.color = new Color(1, 1, 1, 0.3f);
        if (circleSprite != null) joyBgImg.sprite = circleSprite;

        // 摇杆手柄
        GameObject joyHandleGO = CreateChild(joyBgGO, "Handle");
        RectTransform joyHandleRect = joyHandleGO.GetComponent<RectTransform>();
        joyHandleRect.anchorMin = new Vector2(0.5f, 0.5f);
        joyHandleRect.anchorMax = new Vector2(0.5f, 0.5f);
        joyHandleRect.anchoredPosition = Vector2.zero;
        joyHandleRect.sizeDelta = new Vector2(80, 80);
        Image joyHandleImg = joyHandleGO.AddComponent<Image>();
        joyHandleImg.color = new Color(1, 1, 1, 0.7f);
        if (circleSprite != null) joyHandleImg.sprite = circleSprite;

        // 挂载 VirtualJoystick 组件（public字段，直接赋值）
        VirtualJoystick joystick = joystickGO.AddComponent<VirtualJoystick>();
        joystick.background = joyBgRect;
        joystick.handle = joyHandleRect;
        joystick.handleRange = 60f;

        // ================================================================
        // 5. 技能动作按钮 (右下角)
        // ================================================================
        GameObject skillActGO = CreateChild(root, "SkillButtons");
        RectTransform skillActRect = skillActGO.GetComponent<RectTransform>();
        skillActRect.anchorMin = new Vector2(1, 0);
        skillActRect.anchorMax = new Vector2(1, 0);
        skillActRect.pivot = new Vector2(1, 0);
        skillActRect.anchoredPosition = new Vector2(-30, 50);
        skillActRect.sizeDelta = new Vector2(400, 300);

        // 攻击按钮 (最大，右下角)
        Button attackBtn = CreateSkillActionButton(skillActGO, "AttackBtn", skillSlotBg, null,
            new Vector2(-70, 70), new Vector2(130, 130),
            new Color(0.8f, 0.2f, 0.2f, 0.9f), "攻击", defaultFont, circleSprite);

        // 技能1: 多重箭
        Button skill1Btn = CreateSkillActionButton(skillActGO, "Skill1", skillSlotBg, multiShotIcon,
            new Vector2(-70, 210), new Vector2(100, 100),
            new Color(0.8f, 0.4f, 0.2f, 0.9f), multiShotIcon == null ? "多重箭" : null, defaultFont, circleSprite);

        // 技能2: 穿透箭
        Button skill2Btn = CreateSkillActionButton(skillActGO, "Skill2", skillSlotBg, pierceIcon,
            new Vector2(-190, 130), new Vector2(100, 100),
            new Color(0.2f, 0.6f, 0.8f, 0.9f), pierceIcon == null ? "穿透箭" : null, defaultFont, circleSprite);

        // 技能3: 战吼
        Button skill3Btn = CreateSkillActionButton(skillActGO, "Skill3", skillSlotBg, battleShoutIcon,
            new Vector2(-310, 130), new Vector2(100, 100),
            new Color(0.8f, 0.7f, 0.2f, 0.9f), battleShoutIcon == null ? "战吼" : null, defaultFont, circleSprite);

        // ================================================================
        // 6. 暂停按钮 (右上角，HUD下方)
        // ================================================================
        GameObject pauseBtnGO = CreateChild(root, "PauseButton");
        RectTransform pauseRect = pauseBtnGO.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(1, 1);
        pauseRect.anchorMax = new Vector2(1, 1);
        pauseRect.pivot = new Vector2(1, 1);
        pauseRect.anchoredPosition = new Vector2(-20, -130);
        pauseRect.sizeDelta = new Vector2(60, 60);
        Image pauseImg = pauseBtnGO.AddComponent<Image>();
        if (pauseSprite != null)
        {
            pauseImg.sprite = pauseSprite;
            pauseImg.color = Color.white;
        }
        else
        {
            pauseImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            // 暂停图标文字
            GameObject ptGO = CreateChild(pauseBtnGO, "Text");
            RectTransform ptRect = ptGO.GetComponent<RectTransform>();
            ptRect.anchorMin = Vector2.zero;
            ptRect.anchorMax = Vector2.one;
            ptRect.offsetMin = Vector2.zero;
            ptRect.offsetMax = Vector2.zero;
            Text ptText = ptGO.AddComponent<Text>();
            ptText.text = "II";
            ptText.fontSize = 28;
            ptText.alignment = TextAnchor.MiddleCenter;
            ptText.color = Color.white;
            ptText.font = defaultFont;
        }
        Button pauseButton = pauseBtnGO.AddComponent<Button>();
        pauseButton.targetGraphic = pauseImg;

        // ================================================================
        // 7. 暂停面板 (全屏遮罩+居中面板，默认隐藏)
        // ================================================================
        GameObject pausePanelGO = CreateChild(root, "PausePanel");
        RectTransform ppRect = pausePanelGO.GetComponent<RectTransform>();
        ppRect.anchorMin = Vector2.zero;
        ppRect.anchorMax = Vector2.one;
        ppRect.offsetMin = Vector2.zero;
        ppRect.offsetMax = Vector2.zero;
        Image ppBg = pausePanelGO.AddComponent<Image>();
        ppBg.color = new Color(0, 0, 0, 0.65f);

        // 中心内容框
        GameObject ppContent = CreateChild(pausePanelGO, "Content");
        RectTransform ppCRect = ppContent.GetComponent<RectTransform>();
        ppCRect.anchorMin = new Vector2(0.5f, 0.5f);
        ppCRect.anchorMax = new Vector2(0.5f, 0.5f);
        ppCRect.sizeDelta = new Vector2(340, 380);
        Image ppCBg = ppContent.AddComponent<Image>();
        ppCBg.color = new Color(0.18f, 0.18f, 0.28f, 0.97f);

        // 顶部装饰条
        GameObject ppTopBar = CreateChild(ppContent, "TopBar");
        RectTransform ppTBRect = ppTopBar.GetComponent<RectTransform>();
        ppTBRect.anchorMin = new Vector2(0, 1);
        ppTBRect.anchorMax = new Vector2(1, 1);
        ppTBRect.anchoredPosition = Vector2.zero;
        ppTBRect.sizeDelta = new Vector2(0, 5);
        Image ppTBImg = ppTopBar.AddComponent<Image>();
        ppTBImg.color = new Color(0.6f, 0.5f, 0.2f, 1f);

        // 标题
        GameObject ppTitle = CreateChild(ppContent, "Title");
        RectTransform ppTitleRect = ppTitle.GetComponent<RectTransform>();
        ppTitleRect.anchorMin = new Vector2(0, 1);
        ppTitleRect.anchorMax = new Vector2(1, 1);
        ppTitleRect.anchoredPosition = new Vector2(0, -45);
        ppTitleRect.sizeDelta = new Vector2(0, 60);
        Text ppTitleText = ppTitle.AddComponent<Text>();
        ppTitleText.text = "游戏暂停";
        ppTitleText.fontSize = 36;
        ppTitleText.fontStyle = FontStyle.Bold;
        ppTitleText.alignment = TextAnchor.MiddleCenter;
        ppTitleText.color = new Color(1f, 0.9f, 0.6f);
        ppTitleText.font = defaultFont;

        // 分隔线
        GameObject ppSep = CreateChild(ppContent, "SepLine");
        RectTransform ppSepRect = ppSep.GetComponent<RectTransform>();
        ppSepRect.anchorMin = new Vector2(0.15f, 1);
        ppSepRect.anchorMax = new Vector2(0.85f, 1);
        ppSepRect.anchoredPosition = new Vector2(0, -80);
        ppSepRect.sizeDelta = new Vector2(0, 2);
        Image ppSepImg = ppSep.AddComponent<Image>();
        ppSepImg.color = new Color(0.5f, 0.5f, 0.6f, 0.5f);

        // 暂停面板按钮
        CreatePausePanelButton(ppContent, "ResumeButton", "继续游戏",
            new Vector2(0, -115), new Color(0.25f, 0.55f, 0.3f), defaultFont);
        CreatePausePanelButton(ppContent, "RetryButton", "重试",
            new Vector2(0, -190), new Color(0.4f, 0.35f, 0.2f), defaultFont);
        CreatePausePanelButton(ppContent, "MenuButton", "返回主菜单",
            new Vector2(0, -265), new Color(0.5f, 0.2f, 0.2f), defaultFont);

        pausePanelGO.SetActive(false);

        // ================================================================
        // 8. 挂载 GameHUD 并绑定所有 SerializeField 引用
        // ================================================================
        MoShou.UI.GameHUD gameHUD = root.AddComponent<MoShou.UI.GameHUD>();
        SerializedObject so = new SerializedObject(gameHUD);

        // 玩家状态
        so.FindProperty("levelText").objectReferenceValue = lvText;
        so.FindProperty("healthText").objectReferenceValue = hpText;
        so.FindProperty("goldText").objectReferenceValue = goldText;

        // 血条Image (UIFeedbackSystem)
        so.FindProperty("healthFillImage").objectReferenceValue = healthFillImg;
        so.FindProperty("goldIcon").objectReferenceValue = goldIconImg;

        // 战斗信息
        so.FindProperty("waveText").objectReferenceValue = waveText;

        // 属性显示
        so.FindProperty("attackText").objectReferenceValue = atkText;
        so.FindProperty("defenseText").objectReferenceValue = defText;

        // 角色头像按钮
        so.FindProperty("portraitButton").objectReferenceValue = portraitBtn;

        // 左侧快捷按钮 (5个)
        so.FindProperty("shopButton").objectReferenceValue = sideButtons[0];
        so.FindProperty("bagButton").objectReferenceValue = sideButtons[1];
        so.FindProperty("skillButton").objectReferenceValue = sideButtons[2];
        so.FindProperty("equipSideButton").objectReferenceValue = sideButtons[3];
        so.FindProperty("mapButton").objectReferenceValue = sideButtons[4];

        // 技能动作按钮
        so.FindProperty("attackButton").objectReferenceValue = attackBtn;
        so.FindProperty("skill1Button").objectReferenceValue = skill1Btn;
        so.FindProperty("skill2Button").objectReferenceValue = skill2Btn;
        so.FindProperty("skill3Button").objectReferenceValue = skill3Btn;

        // 暂停
        so.FindProperty("pauseButton").objectReferenceValue = pauseButton;
        so.FindProperty("pausePanel").objectReferenceValue = pausePanelGO;

        so.ApplyModifiedPropertiesWithoutUndo();

        // ================================================================
        // 保存为Prefab
        // ================================================================
        string path = "Assets/Resources/Prefabs/UI/BattleHUD.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        Debug.Log($"[BattleHUDPrefabCreator] BattleHUD Prefab 已创建: {path}");
        AssetDatabase.Refresh();
    }

    #region 工具方法

    static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.AddComponent<RectTransform>();
        return child;
    }

    static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }

    static Font GetFont()
    {
        string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf" };
        foreach (string name in fontNames)
        {
            Font f = Resources.GetBuiltinResource<Font>(name);
            if (f != null) return f;
        }
        return Font.CreateDynamicFontFromOSFont("Arial", 14);
    }

    /// <summary>
    /// 生成或加载圆形Sprite资源（64x64白色圆形PNG）
    /// </summary>
    static Sprite CreateOrLoadCircleSprite()
    {
        string pngPath = "Assets/Resources/Sprites/UI/HUD/UI_Circle_64.png";

        // 如果已存在则直接加载
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        if (existing != null) return existing;

        // 生成64x64圆形纹理
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = center - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        }
        tex.Apply();

        // 保存为PNG文件
        byte[] pngData = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);
        File.WriteAllBytes(pngPath, pngData);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

        // 设置为Sprite导入格式
        TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
    }

    /// <summary>
    /// 创建技能动作按钮（带图标或文字回退）
    /// </summary>
    static Button CreateSkillActionButton(GameObject parent, string name, Sprite bgSprite, Sprite icon,
        Vector2 pos, Vector2 size, Color fallbackColor, string fallbackText, Font font, Sprite circleSprite)
    {
        GameObject btnGO = CreateChild(parent, name);
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.anchoredPosition = pos;
        btnRect.sizeDelta = size;

        Image btnImg = btnGO.AddComponent<Image>();
        if (bgSprite != null)
        {
            btnImg.sprite = bgSprite;
            btnImg.color = Color.white;
        }
        else if (circleSprite != null)
        {
            btnImg.sprite = circleSprite;
            btnImg.color = fallbackColor;
        }
        else
        {
            btnImg.color = fallbackColor;
        }

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // 技能图标（子对象）
        if (icon != null)
        {
            GameObject iconGO = CreateChild(btnGO, "Icon");
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
        }
        else if (!string.IsNullOrEmpty(fallbackText))
        {
            // 没有图标时显示文字
            GameObject textGO = CreateChild(btnGO, "Text");
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text btnText = textGO.AddComponent<Text>();
            btnText.text = fallbackText;
            btnText.fontSize = size.x > 100 ? 24 : 16;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.font = font;
        }

        return btn;
    }

    /// <summary>
    /// 创建暂停面板中的按钮
    /// </summary>
    static void CreatePausePanelButton(GameObject parent, string name, string text,
        Vector2 pos, Color bgColor, Font font)
    {
        GameObject btnGO = CreateChild(parent, name);
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 1);
        btnRect.anchorMax = new Vector2(0.5f, 1);
        btnRect.anchoredPosition = pos;
        btnRect.sizeDelta = new Vector2(250, 55);

        Image btnImage = btnGO.AddComponent<Image>();
        btnImage.color = bgColor;

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        var colors = btn.colors;
        colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f);
        colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
        btn.colors = colors;

        // 按钮文字
        GameObject textGO = CreateChild(btnGO, "Text");
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text btnText = textGO.AddComponent<Text>();
        btnText.text = text;
        btnText.fontSize = 26;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.font = font;
    }

    #endregion
}
