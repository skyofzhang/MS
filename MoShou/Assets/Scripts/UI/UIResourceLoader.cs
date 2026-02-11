using UnityEngine;
using System.Collections.Generic;

namespace MoShou.UI
{
    /// <summary>
    /// UI资源加载器 - 统一管理UI图片资源的加载和缓存
    /// 使用从网上下载的免费UI素材包:
    /// - Kenney UI Pack RPG Expansion (CC0, 面板/按钮/进度条)
    /// - ZSS Game Lab Free Fantasy RPG UI Kit (装备槽框/金币图标)
    /// </summary>
    public static class UIResourceLoader
    {
        // Kenney资源路径前缀
        private const string KENNEY_PATH = "Sprites/UI/Kenney/";
        // RPGKit资源路径前缀
        private const string RPGKIT_PATH = "Sprites/UI/RPGKit/";

        // 缓存
        private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        // ======== 面板背景 ========
        public static Sprite PanelBeige => Load(KENNEY_PATH + "panel_beige");
        public static Sprite PanelBeigeLight => Load(KENNEY_PATH + "panel_beigeLight");
        public static Sprite PanelBlue => Load(KENNEY_PATH + "panel_blue");
        public static Sprite PanelBrown => Load(KENNEY_PATH + "panel_brown");
        public static Sprite PanelInsetBeige => Load(KENNEY_PATH + "panelInset_beige");
        public static Sprite PanelInsetBeigeLight => Load(KENNEY_PATH + "panelInset_beigeLight");
        public static Sprite PanelInsetBlue => Load(KENNEY_PATH + "panelInset_blue");
        public static Sprite PanelInsetBrown => Load(KENNEY_PATH + "panelInset_brown");

        // ======== 按钮 ========
        public static Sprite ButtonBeige => Load(KENNEY_PATH + "buttonLong_beige");
        public static Sprite ButtonBeigePressed => Load(KENNEY_PATH + "buttonLong_beige_pressed");
        public static Sprite ButtonBlue => Load(KENNEY_PATH + "buttonLong_blue");
        public static Sprite ButtonBluePressed => Load(KENNEY_PATH + "buttonLong_blue_pressed");
        public static Sprite ButtonBrown => Load(KENNEY_PATH + "buttonLong_brown");
        public static Sprite ButtonBrownPressed => Load(KENNEY_PATH + "buttonLong_brown_pressed");
        public static Sprite ButtonGrey => Load(KENNEY_PATH + "buttonLong_grey");
        public static Sprite ButtonGreyPressed => Load(KENNEY_PATH + "buttonLong_grey_pressed");

        // ======== 方形按钮 ========
        public static Sprite ButtonSquareBeige => Load(KENNEY_PATH + "buttonSquare_beige");
        public static Sprite ButtonSquareBlue => Load(KENNEY_PATH + "buttonSquare_blue");
        public static Sprite ButtonSquareBrown => Load(KENNEY_PATH + "buttonSquare_brown");

        // ======== 图标 ========
        public static Sprite IconCrossBrown => Load(KENNEY_PATH + "iconCross_brown");
        public static Sprite IconCrossGrey => Load(KENNEY_PATH + "iconCross_grey");
        public static Sprite IconCheckBlue => Load(KENNEY_PATH + "iconCheck_blue");
        public static Sprite IconCheckBronze => Load(KENNEY_PATH + "iconCheck_bronze");

        // ======== 装备槽框 (ZSS RPGKit) ========
        public static Sprite SlotHelm => Load(RPGKIT_PATH + "Slot_Equip_Helm");
        public static Sprite SlotRing => Load(RPGKIT_PATH + "Slot_Equip_Ring");
        public static Sprite SlotWeapon => Load(RPGKIT_PATH + "Slot_Equip_Weapon");
        public static Sprite SlotArmor => Load(RPGKIT_PATH + "Slot_Equip_Armor");
        public static Sprite SlotBoots => Load(RPGKIT_PATH + "Slot_Equip_Boots");
        public static Sprite SlotGloves => Load(RPGKIT_PATH + "Slot_Equip_Gloves");
        public static Sprite FrameRarityCommon => Load(RPGKIT_PATH + "Frame_Rarity_Common");
        public static Sprite IconGold => Load(RPGKIT_PATH + "Icon_Currency_Gold");
        public static Sprite IconBag => Load(RPGKIT_PATH + "Icon_UI_Bag");

        /// <summary>
        /// 加载并缓存Sprite
        /// </summary>
        private static Sprite Load(string path)
        {
            if (spriteCache.TryGetValue(path, out Sprite cached))
            {
                return cached;
            }

            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                spriteCache[path] = sprite;
            }
            return sprite;
        }

        /// <summary>
        /// 将Sprite应用到Image上（带9-slice支持）
        /// </summary>
        public static void ApplySprite(UnityEngine.UI.Image image, Sprite sprite, UnityEngine.UI.Image.Type imageType = UnityEngine.UI.Image.Type.Sliced)
        {
            if (image == null || sprite == null) return;
            image.sprite = sprite;
            image.type = imageType;
            image.color = Color.white;
        }

        /// <summary>
        /// 根据装备槽位获取对应的槽框Sprite
        /// </summary>
        public static Sprite GetSlotFrame(MoShou.Data.EquipmentSlot slot)
        {
            switch (slot)
            {
                case MoShou.Data.EquipmentSlot.Weapon: return SlotWeapon;
                case MoShou.Data.EquipmentSlot.Helmet: return SlotHelm;
                case MoShou.Data.EquipmentSlot.Armor: return SlotArmor;
                case MoShou.Data.EquipmentSlot.Pants: return SlotBoots; // 裤子用靴子框作替代
                case MoShou.Data.EquipmentSlot.Ring: return SlotRing;
                case MoShou.Data.EquipmentSlot.Necklace: return SlotGloves; // 项链用手套框作替代
                default: return FrameRarityCommon;
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            spriteCache.Clear();
        }
    }
}
