using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 自动应用UI贴图的Editor工具
/// 按照策划案RULE-RES-005~010的映射规则应用贴图
/// </summary>
public class UIArtApplier : EditorWindow
{
    // 资源映射规则 (来自notion_ai_dev_kb.txt RULE-RES-xxx)
    private static readonly Dictionary<string, string> SpriteMapping = new Dictionary<string, string>
    {
        // HUD资源 (RULE-RES-005)
        { "HPBar_BG", "Sprites/UI/HUD/UI_HUD_HPBar_BG" },
        { "HPBar_Fill", "Sprites/UI/HUD/UI_HUD_HPBar_Fill" },
        { "HP_BG", "Sprites/UI/HUD/UI_HUD_HPBar_BG" },
        { "HP_Fill", "Sprites/UI/HUD/UI_HUD_HPBar_Fill" },
        { "HealthBar_BG", "Sprites/UI/HUD/UI_HUD_HPBar_BG" },
        { "HealthBar_Fill", "Sprites/UI/HUD/UI_HUD_HPBar_Fill" },
        { "EXPBar_BG", "Sprites/UI/HUD/UI_HUD_EXPBar_BG" },
        { "EXPBar_Fill", "Sprites/UI/HUD/UI_HUD_EXPBar_Fill" },
        { "ExpBar_BG", "Sprites/UI/HUD/UI_HUD_EXPBar_BG" },
        { "ExpBar_Fill", "Sprites/UI/HUD/UI_HUD_EXPBar_Fill" },
        { "PlayerIcon_Frame", "Sprites/UI/HUD/UI_HUD_PlayerIcon_Frame" },
        { "Avatar_Frame", "Sprites/UI/HUD/UI_HUD_PlayerIcon_Frame" },
        { "Level_BG", "Sprites/UI/HUD/UI_HUD_Level_BG" },
        { "Gold_Icon", "Sprites/UI/HUD/UI_HUD_Gold_Icon" },
        { "GoldIcon", "Sprites/UI/HUD/UI_HUD_Gold_Icon" },
        { "Wave_BG", "Sprites/UI/HUD/UI_HUD_Wave_BG" },

        // 技能槽资源 (RULE-RES-006)
        { "Skill_Slot_BG", "Sprites/UI/Skills/UI_Skill_Slot_BG" },
        { "SkillSlot_BG", "Sprites/UI/Skills/UI_Skill_Slot_BG" },
        { "Skill_Slot_Locked", "Sprites/UI/Skills/UI_Skill_Slot_Locked" },
        { "Cooldown_Mask", "Sprites/UI/Skills/UI_Skill_Cooldown_Mask" },
        { "CooldownMask", "Sprites/UI/Skills/UI_Skill_Cooldown_Mask" },
        { "Skill_MultiShot", "Sprites/UI/Skills/UI_Skill_Icon_MultiShot" },
        { "Skill_Pierce", "Sprites/UI/Skills/UI_Skill_Icon_Pierce" },
        { "Skill_BattleShout", "Sprites/UI/Skills/UI_Skill_Icon_BattleShout" },

        // 升级面板资源 (RULE-RES-007)
        { "LevelUp_Panel_BG", "Sprites/UI/LevelUp/UI_LevelUp_Panel_BG" },
        { "LevelUp_Card_BG", "Sprites/UI/LevelUp/UI_LevelUp_Card_BG" },
        { "LevelUp_Card_Selected", "Sprites/UI/LevelUp/UI_LevelUp_Card_Selected" },
        { "LevelUp_Title_BG", "Sprites/UI/LevelUp/UI_LevelUp_Title_BG" },
        { "Rarity_Common", "Sprites/UI/LevelUp/UI_Rarity_Common" },
        { "Rarity_Rare", "Sprites/UI/LevelUp/UI_Rarity_Rare" },
        { "Rarity_Epic", "Sprites/UI/LevelUp/UI_Rarity_Epic" },

        // 结算面板资源 (RULE-RES-008)
        { "Result_Victory_BG", "Sprites/UI/Result/UI_Result_Victory_BG" },
        { "Victory_BG", "Sprites/UI/Result/UI_Result_Victory_BG" },
        { "Result_Defeat_BG", "Sprites/UI/Result/UI_Result_Defeat_BG" },
        { "Defeat_BG", "Sprites/UI/Result/UI_Result_Defeat_BG" },
        { "Star_Empty", "Sprites/UI/Result/UI_Result_Star_Empty" },
        { "Star_Filled", "Sprites/UI/Result/UI_Result_Star_Filled" },
        { "Reward_Slot", "Sprites/UI/Result/UI_Result_Reward_Slot" },

        // 按钮资源 (RULE-RES-009)
        { "Btn_Primary_Normal", "Sprites/UI/Buttons/UI_Btn_Primary_Normal" },
        { "Btn_Primary_Pressed", "Sprites/UI/Buttons/UI_Btn_Primary_Pressed" },
        { "Btn_Primary_Disabled", "Sprites/UI/Buttons/UI_Btn_Primary_Disabled" },
        { "Btn_Secondary_Normal", "Sprites/UI/Buttons/UI_Btn_Secondary_Normal" },
        { "Btn_Close", "Sprites/UI/Buttons/UI_Btn_Close" },
        { "Btn_Pause", "Sprites/UI/Buttons/UI_Btn_Pause" },
        { "CloseButton", "Sprites/UI/Buttons/UI_Btn_Close" },
        { "PauseButton", "Sprites/UI/Buttons/UI_Btn_Pause" },

        // 通用UI资源 (RULE-RES-010)
        { "Tooltip_BG", "Sprites/UI/Common/UI_Common_Tooltip_BG" },
        { "Dialog_BG", "Sprites/UI/Common/UI_Common_Dialog_BG" },
        { "Progress_BG", "Sprites/UI/Common/UI_Common_Progress_BG" },
        { "Progress_Fill", "Sprites/UI/Common/UI_Common_Progress_Fill" },
        { "Damage_Numbers", "Sprites/UI/Common/UI_Damage_Numbers" },
        { "Damage_Crit_Numbers", "Sprites/UI/Common/UI_Damage_Crit_Numbers" },

        // Generated资源 (额外生成的)
        { "Joystick_Base", "Sprites/Generated/UI_Joystick_Base" },
        { "JoystickBase", "Sprites/Generated/UI_Joystick_Base" },
        { "Joystick_Knob", "Sprites/Generated/UI_Joystick_Knob" },
        { "JoystickKnob", "Sprites/Generated/UI_Joystick_Knob" },
        { "Panel_Background", "Sprites/Generated/UI_Panel_Background" },
        { "PanelBackground", "Sprites/Generated/UI_Panel_Background" },
        { "Panel_BG", "Sprites/Generated/UI_Panel_Background" },
        { "Button_Normal", "Sprites/Generated/UI_Button_Normal" },
        { "Button_Highlight", "Sprites/Generated/UI_Button_Highlight" },
        { "Slot_Empty", "Sprites/Generated/UI_Slot_Empty" },
        { "SlotEmpty", "Sprites/Generated/UI_Slot_Empty" },
        { "Slot_Selected", "Sprites/Generated/UI_Slot_Selected" },
        { "SlotSelected", "Sprites/Generated/UI_Slot_Selected" },
        { "HealthBar_Background", "Sprites/Generated/UI_HealthBar_Background" },
        { "HealthBar_Fill_Gen", "Sprites/Generated/UI_HealthBar_Fill" },
        { "ManaBar_Fill", "Sprites/Generated/UI_ManaBar_Fill" },
        { "ExpBar_Fill_Gen", "Sprites/Generated/UI_ExpBar_Fill" },
        { "Icon_Coin", "Sprites/Generated/UI_Icon_Coin" },
        { "CoinIcon", "Sprites/Generated/UI_Icon_Coin" },
        { "Icon_Gem", "Sprites/Generated/UI_Icon_Gem" },
        { "Icon_Settings", "Sprites/Generated/UI_Icon_Settings" },
        { "Icon_Close", "Sprites/Generated/UI_Icon_Close" },
        { "Arrow_Left", "Sprites/Generated/UI_Arrow_Left" },
        { "Arrow_Right", "Sprites/Generated/UI_Arrow_Right" },
        { "Skill_Frame", "Sprites/Generated/UI_Skill_Frame" },
        { "SkillFrame", "Sprites/Generated/UI_Skill_Frame" },
        { "Avatar_Frame_Gen", "Sprites/Generated/UI_Avatar_Frame" },
        { "Checkbox_Off", "Sprites/Generated/UI_Checkbox_Off" },
        { "Checkbox_On", "Sprites/Generated/UI_Checkbox_On" },
        { "Slider_Background", "Sprites/Generated/UI_Slider_Background" },
        { "Slider_Handle", "Sprites/Generated/UI_Slider_Handle" },
        { "Tab_Active", "Sprites/Generated/UI_Tab_Active" },
        { "Tab_Inactive", "Sprites/Generated/UI_Tab_Inactive" },
        { "Notification_Badge", "Sprites/Generated/UI_Notification_Badge" },
        { "Icon_Lock", "Sprites/Generated/UI_Icon_Lock" },
        { "Level_Badge", "Sprites/Generated/UI_Level_Badge" },
    };

    [MenuItem("MoShou/Apply UI Art Assets")]
    public static void ApplyUIArtAssets()
    {
        // 查找所有Prefab
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Prefabs" });
        int appliedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;

            bool modified = false;

            // 获取所有Image组件
            Image[] images = prefab.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (TryApplySprite(img))
                {
                    modified = true;
                    appliedCount++;
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                Debug.Log($"[UIArtApplier] 更新Prefab: {prefabPath}");
            }
        }

        // 查找场景中的UI
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (TryApplySprite(img))
                {
                    appliedCount++;
                    EditorUtility.SetDirty(img);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[UIArtApplier] 完成! 共应用 {appliedCount} 个Sprite");
        EditorUtility.DisplayDialog("UI Art Applier", $"应用完成!\n共更新 {appliedCount} 个UI Image", "确定");
    }

    private static bool TryApplySprite(Image image)
    {
        if (image == null) return false;

        string objectName = image.gameObject.name;

        // 尝试匹配映射规则
        foreach (var mapping in SpriteMapping)
        {
            if (objectName.Contains(mapping.Key) ||
                objectName.Replace("_", "").Contains(mapping.Key.Replace("_", "")))
            {
                Sprite sprite = Resources.Load<Sprite>(mapping.Value);
                if (sprite != null)
                {
                    if (image.sprite != sprite)
                    {
                        image.sprite = sprite;
                        Debug.Log($"[UIArtApplier] 应用 {mapping.Value} 到 {objectName}");
                        return true;
                    }
                }
                else
                {
                    Debug.LogWarning($"[UIArtApplier] 找不到Sprite: {mapping.Value}");
                }
            }
        }

        // 检查是否使用了FALLBACK颜色(magenta)
        if (image.sprite == null && image.color == Color.magenta)
        {
            Debug.LogWarning($"[UIArtApplier] 发现FALLBACK对象(magenta): {GetFullPath(image.transform)}");
        }

        return false;
    }

    private static string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    [MenuItem("MoShou/Apply UI Art (Scene Only)")]
    public static void ApplyUIArtToScene()
    {
        int appliedCount = 0;

        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (TryApplySprite(img))
                {
                    appliedCount++;
                    EditorUtility.SetDirty(img);
                }
            }
        }

        Debug.Log($"[UIArtApplier] 场景应用完成! 共更新 {appliedCount} 个UI Image");
    }

    [MenuItem("MoShou/Check FALLBACK Objects")]
    public static void CheckFallbackObjects()
    {
        int fallbackCount = 0;
        List<string> fallbackObjects = new List<string>();

        // 检查场景中的所有Image
        Image[] allImages = GameObject.FindObjectsOfType<Image>(true);
        foreach (Image img in allImages)
        {
            // 检查magenta颜色(FALLBACK)
            if (img.color == Color.magenta || img.color == new Color(1, 0, 1, 1))
            {
                string path = GetFullPath(img.transform);
                fallbackObjects.Add(path);
                fallbackCount++;
            }

            // 检查没有sprite但应该有的
            if (img.sprite == null && img.type == Image.Type.Simple)
            {
                string objName = img.gameObject.name.ToLower();
                if (objName.Contains("icon") || objName.Contains("bg") ||
                    objName.Contains("fill") || objName.Contains("button"))
                {
                    string path = GetFullPath(img.transform);
                    if (!fallbackObjects.Contains(path))
                    {
                        fallbackObjects.Add(path + " (无Sprite)");
                        fallbackCount++;
                    }
                }
            }
        }

        // 检查Renderer的材质颜色
        Renderer[] allRenderers = GameObject.FindObjectsOfType<Renderer>(true);
        foreach (Renderer rend in allRenderers)
        {
            if (rend.sharedMaterial != null && rend.sharedMaterial.color == Color.magenta)
            {
                string path = GetFullPath(rend.transform);
                fallbackObjects.Add(path + " (Renderer)");
                fallbackCount++;
            }
        }

        if (fallbackCount > 0)
        {
            Debug.LogWarning($"[UIArtApplier] 发现 {fallbackCount} 个FALLBACK对象:");
            foreach (string obj in fallbackObjects)
            {
                Debug.LogWarning($"  - {obj}");
            }
            EditorUtility.DisplayDialog("FALLBACK Check",
                $"发现 {fallbackCount} 个FALLBACK对象!\n请查看Console获取详细列表", "确定");
        }
        else
        {
            Debug.Log("[UIArtApplier] 未发现FALLBACK对象，验收通过!");
            EditorUtility.DisplayDialog("FALLBACK Check", "未发现FALLBACK对象\n验收通过!", "确定");
        }
    }
}
