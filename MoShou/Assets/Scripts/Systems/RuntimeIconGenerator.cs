using UnityEngine;
using System.Collections.Generic;
using MoShou.Data;

namespace MoShou.Systems
{
    /// <summary>
    /// 运行时图标生成器 - 在游戏启动时自动为所有装备/物品生成Sprite图标
    /// 图标直接在内存中生成，无需手动操作，无需PNG文件
    /// 用法: RuntimeIconGenerator.GetIcon("WPN_001") 获取图标
    /// 256x256高分辨率版本
    /// </summary>
    public static class RuntimeIconGenerator
    {
        private const int ICON_SIZE = 256;

        // 已生成的Sprite缓存
        private static Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();
        private static bool isInitialized = false;

        // 品质背景颜色
        static readonly Color WHITE_BG = new Color(0.35f, 0.35f, 0.38f);
        static readonly Color GREEN_BG = new Color(0.2f, 0.45f, 0.25f);
        static readonly Color BLUE_BG = new Color(0.2f, 0.3f, 0.55f);
        static readonly Color PURPLE_BG = new Color(0.4f, 0.2f, 0.55f);
        static readonly Color ORANGE_BG = new Color(0.55f, 0.35f, 0.15f);

        // 品质边框颜色
        static readonly Color WHITE_BORDER = new Color(0.6f, 0.6f, 0.6f);
        static readonly Color GREEN_BORDER = new Color(0.3f, 0.8f, 0.3f);
        static readonly Color BLUE_BORDER = new Color(0.3f, 0.5f, 1f);
        static readonly Color PURPLE_BORDER = new Color(0.7f, 0.3f, 1f);
        static readonly Color ORANGE_BORDER = new Color(1f, 0.6f, 0.1f);

        enum IconShape { Sword, Bow, Armor, Helmet, Pants, Ring, Necklace, Potion }

        /// <summary>
        /// 获取物品图标（自动初始化）
        /// </summary>
        public static Sprite GetIcon(string itemId)
        {
            if (!isInitialized) Initialize();

            if (string.IsNullOrEmpty(itemId)) return null;

            Sprite cached;
            if (iconCache.TryGetValue(itemId, out cached))
                return cached;

            // 尝试根据ID动态生成
            Sprite generated = GenerateIconForId(itemId);
            if (generated != null)
            {
                iconCache[itemId] = generated;
            }
            return generated;
        }

        /// <summary>
        /// 初始化 - 生成所有已知物品的图标
        /// </summary>
        static void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            // ===== 武器 =====
            CacheIcon("WPN_001", WHITE_BG, WHITE_BORDER, new Color(0.7f, 0.7f, 0.75f), IconShape.Sword);
            CacheIcon("WPN_002", GREEN_BG, GREEN_BORDER, new Color(0.75f, 0.75f, 0.8f), IconShape.Sword);
            CacheIcon("WPN_003", BLUE_BG, BLUE_BORDER, new Color(0.8f, 0.85f, 1f), IconShape.Sword);
            CacheIcon("WPN_004", GREEN_BG, GREEN_BORDER, new Color(0.6f, 0.45f, 0.3f), IconShape.Bow);
            CacheIcon("WPN_005", PURPLE_BG, PURPLE_BORDER, new Color(0.3f, 0.1f, 0.4f), IconShape.Sword);
            CacheIcon("WPN_006", ORANGE_BG, ORANGE_BORDER, new Color(1f, 0.85f, 0.3f), IconShape.Sword);
            CacheIcon("WPN_007", PURPLE_BG, PURPLE_BORDER, new Color(0.4f, 0.7f, 1f), IconShape.Bow);

            // ===== 护甲 =====
            CacheIcon("ARM_001", WHITE_BG, WHITE_BORDER, new Color(0.65f, 0.55f, 0.45f), IconShape.Armor);
            CacheIcon("ARM_002", GREEN_BG, GREEN_BORDER, new Color(0.6f, 0.4f, 0.25f), IconShape.Armor);
            CacheIcon("ARM_003", BLUE_BG, BLUE_BORDER, new Color(0.6f, 0.65f, 0.7f), IconShape.Armor);
            CacheIcon("ARM_004", PURPLE_BG, PURPLE_BORDER, new Color(0.7f, 0.7f, 0.75f), IconShape.Armor);
            CacheIcon("ARM_005", PURPLE_BG, PURPLE_BORDER, new Color(0.8f, 0.85f, 0.9f), IconShape.Armor);
            CacheIcon("ARM_006", ORANGE_BG, ORANGE_BORDER, new Color(0.2f, 0.6f, 0.3f), IconShape.Armor);

            // ===== 头盔 =====
            CacheIcon("HLM_001", WHITE_BG, WHITE_BORDER, new Color(0.6f, 0.5f, 0.35f), IconShape.Helmet);
            CacheIcon("HLM_002", GREEN_BG, GREEN_BORDER, new Color(0.6f, 0.6f, 0.65f), IconShape.Helmet);
            CacheIcon("HLM_003", BLUE_BG, BLUE_BORDER, new Color(0.8f, 0.75f, 0.5f), IconShape.Helmet);
            CacheIcon("HLM_004", PURPLE_BG, PURPLE_BORDER, new Color(0.9f, 0.75f, 0.2f), IconShape.Helmet);

            // ===== 护腿 =====
            CacheIcon("PNT_001", WHITE_BG, WHITE_BORDER, new Color(0.55f, 0.5f, 0.4f), IconShape.Pants);
            CacheIcon("PNT_002", GREEN_BG, GREEN_BORDER, new Color(0.55f, 0.4f, 0.25f), IconShape.Pants);
            CacheIcon("PNT_003", BLUE_BG, BLUE_BORDER, new Color(0.6f, 0.6f, 0.7f), IconShape.Pants);
            CacheIcon("PNT_004", PURPLE_BG, PURPLE_BORDER, new Color(0.25f, 0.15f, 0.35f), IconShape.Pants);

            // ===== 戒指 =====
            CacheIcon("RNG_001", WHITE_BG, WHITE_BORDER, new Color(0.7f, 0.5f, 0.3f), IconShape.Ring);
            CacheIcon("RNG_002", GREEN_BG, GREEN_BORDER, new Color(0.8f, 0.82f, 0.85f), IconShape.Ring);
            CacheIcon("RNG_003", BLUE_BG, BLUE_BORDER, new Color(1f, 0.4f, 0.15f), IconShape.Ring);
            CacheIcon("RNG_004", PURPLE_BG, PURPLE_BORDER, new Color(0.4f, 0.7f, 1f), IconShape.Ring);

            // ===== 项链 =====
            CacheIcon("NCK_001", WHITE_BG, WHITE_BORDER, new Color(0.7f, 0.65f, 0.4f), IconShape.Necklace);
            CacheIcon("NCK_002", GREEN_BG, GREEN_BORDER, new Color(0.3f, 0.8f, 0.4f), IconShape.Necklace);
            CacheIcon("NCK_003", BLUE_BG, BLUE_BORDER, new Color(0.9f, 0.85f, 0.5f), IconShape.Necklace);
            CacheIcon("NCK_004", PURPLE_BG, PURPLE_BORDER, new Color(1f, 0.3f, 0.3f), IconShape.Necklace);

            // ===== 消耗品 =====
            CacheIcon("CON_001", WHITE_BG, WHITE_BORDER, new Color(0.9f, 0.2f, 0.2f), IconShape.Potion);
            CacheIcon("CON_002", GREEN_BG, GREEN_BORDER, new Color(0.9f, 0.3f, 0.3f), IconShape.Potion);
            CacheIcon("CON_003", BLUE_BG, BLUE_BORDER, new Color(1f, 0.2f, 0.2f), IconShape.Potion);
            CacheIcon("CON_004", GREEN_BG, GREEN_BORDER, new Color(1f, 0.5f, 0.1f), IconShape.Potion);

            Debug.Log($"[RuntimeIconGenerator] 生成完毕，共 {iconCache.Count} 个图标 (256x256)");
        }

        /// <summary>
        /// 根据物品ID前缀动态生成图标（用于未预定义的物品）
        /// </summary>
        static Sprite GenerateIconForId(string itemId)
        {
            IconShape shape = IconShape.Potion;
            Color bgColor = WHITE_BG;
            Color borderColor = WHITE_BORDER;
            Color itemColor = new Color(0.7f, 0.7f, 0.7f);

            if (itemId.StartsWith("WPN")) { shape = IconShape.Sword; itemColor = new Color(0.8f, 0.6f, 0.3f); }
            else if (itemId.StartsWith("ARM")) { shape = IconShape.Armor; itemColor = new Color(0.5f, 0.6f, 0.8f); }
            else if (itemId.StartsWith("HLM")) { shape = IconShape.Helmet; itemColor = new Color(0.6f, 0.6f, 0.7f); }
            else if (itemId.StartsWith("PNT")) { shape = IconShape.Pants; itemColor = new Color(0.5f, 0.45f, 0.4f); }
            else if (itemId.StartsWith("RNG")) { shape = IconShape.Ring; itemColor = new Color(0.8f, 0.7f, 0.3f); }
            else if (itemId.StartsWith("NCK")) { shape = IconShape.Necklace; itemColor = new Color(0.7f, 0.5f, 0.8f); }
            else if (itemId.StartsWith("CON") || itemId.StartsWith("POTION")) { shape = IconShape.Potion; itemColor = new Color(0.9f, 0.3f, 0.3f); }

            return CreateSprite(itemId, bgColor, borderColor, itemColor, shape);
        }

        static void CacheIcon(string id, Color bgColor, Color borderColor, Color itemColor, IconShape shape)
        {
            Sprite sp = CreateSprite(id, bgColor, borderColor, itemColor, shape);
            if (sp != null)
                iconCache[id] = sp;
        }

        static Sprite CreateSprite(string id, Color bgColor, Color borderColor, Color itemColor, IconShape shape)
        {
            Texture2D tex = new Texture2D(ICON_SIZE, ICON_SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[ICON_SIZE * ICON_SIZE];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            int center = ICON_SIZE / 2;

            // 1. 圆角矩形背景
            DrawRoundedRect(pixels, 8, 8, ICON_SIZE - 16, ICON_SIZE - 16, 24, bgColor);

            // 2. 边框
            DrawRoundedRectBorder(pixels, 4, 4, ICON_SIZE - 8, ICON_SIZE - 8, 28, borderColor, 6);

            // 3. 装备形状
            switch (shape)
            {
                case IconShape.Sword: DrawSword(pixels, center, center - 10, itemColor); break;
                case IconShape.Bow: DrawBow(pixels, center, center - 10, itemColor); break;
                case IconShape.Armor: DrawArmor(pixels, center, center - 4, itemColor); break;
                case IconShape.Helmet: DrawHelmet(pixels, center, center - 10, itemColor); break;
                case IconShape.Pants: DrawPants(pixels, center, center - 4, itemColor); break;
                case IconShape.Ring: DrawRing(pixels, center, center - 6, itemColor); break;
                case IconShape.Necklace: DrawNecklace(pixels, center, center - 10, itemColor); break;
                case IconShape.Potion: DrawPotion(pixels, center, center - 10, itemColor); break;
            }

            // 4. 品质光晕
            AddCornerGlow(pixels, borderColor);

            tex.SetPixels(pixels);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, ICON_SIZE, ICON_SIZE), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = id;
            return sprite;
        }

        // ===== 绘制工具方法 =====

        static void SetPixelSafe(Color[] pixels, int x, int y, Color color)
        {
            if (x >= 0 && x < ICON_SIZE && y >= 0 && y < ICON_SIZE)
            {
                int idx = y * ICON_SIZE + x;
                float a = color.a;
                pixels[idx] = new Color(
                    pixels[idx].r * (1 - a) + color.r * a,
                    pixels[idx].g * (1 - a) + color.g * a,
                    pixels[idx].b * (1 - a) + color.b * a,
                    Mathf.Max(pixels[idx].a, a)
                );
            }
        }

        static void DrawRoundedRect(Color[] pixels, int x, int y, int w, int h, int radius, Color color)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    bool inside = true;
                    if (px < x + radius && py < y + radius)
                        inside = (px - x - radius) * (px - x - radius) + (py - y - radius) * (py - y - radius) <= radius * radius;
                    else if (px >= x + w - radius && py < y + radius)
                        inside = (px - x - w + radius) * (px - x - w + radius) + (py - y - radius) * (py - y - radius) <= radius * radius;
                    else if (px < x + radius && py >= y + h - radius)
                        inside = (px - x - radius) * (px - x - radius) + (py - y - h + radius) * (py - y - h + radius) <= radius * radius;
                    else if (px >= x + w - radius && py >= y + h - radius)
                        inside = (px - x - w + radius) * (px - x - w + radius) + (py - y - h + radius) * (py - y - h + radius) <= radius * radius;

                    if (inside)
                        SetPixelSafe(pixels, px, py, color);
                }
            }
        }

        static void DrawRoundedRectBorder(Color[] pixels, int x, int y, int w, int h, int radius, Color color, int thickness)
        {
            for (int t = 0; t < thickness; t++)
            {
                int cx = x + t, cy = y + t, cw = w - t * 2, ch = h - t * 2;
                int r = Mathf.Max(radius - t, 0);
                for (int px = cx + r; px < cx + cw - r; px++)
                {
                    SetPixelSafe(pixels, px, cy, color);
                    SetPixelSafe(pixels, px, cy + ch - 1, color);
                }
                for (int py = cy + r; py < cy + ch - r; py++)
                {
                    SetPixelSafe(pixels, cx, py, color);
                    SetPixelSafe(pixels, cx + cw - 1, py, color);
                }
                DrawArc(pixels, cx + r, cy + r, r, 180, 270, color);
                DrawArc(pixels, cx + cw - 1 - r, cy + r, r, 270, 360, color);
                DrawArc(pixels, cx + r, cy + ch - 1 - r, r, 90, 180, color);
                DrawArc(pixels, cx + cw - 1 - r, cy + ch - 1 - r, r, 0, 90, color);
            }
        }

        static void DrawArc(Color[] pixels, int cx, int cy, int r, int startAngle, int endAngle, Color color)
        {
            for (int angle = startAngle; angle <= endAngle; angle++)
            {
                float rad = angle * Mathf.Deg2Rad;
                int px = cx + Mathf.RoundToInt(Mathf.Cos(rad) * r);
                int py = cy + Mathf.RoundToInt(Mathf.Sin(rad) * r);
                SetPixelSafe(pixels, px, py, color);
            }
        }

        static void DrawLine(Color[] pixels, int x0, int y0, int x1, int y1, Color color, int thickness = 2)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int half = thickness / 2;

            while (true)
            {
                for (int tx = -half; tx <= half; tx++)
                    for (int ty = -half; ty <= half; ty++)
                        SetPixelSafe(pixels, x0 + tx, y0 + ty, color);

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        static void DrawCircle(Color[] pixels, int cx, int cy, int r, Color color, bool filled = true)
        {
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (filled ? dist <= r : Mathf.Abs(dist - r) < 1.5f)
                        SetPixelSafe(pixels, cx + x, cy + y, color);
                }
        }

        static void DrawEllipse(Color[] pixels, int cx, int cy, int rx, int ry, Color color, bool filled = true)
        {
            for (int y = -ry; y <= ry; y++)
                for (int x = -rx; x <= rx; x++)
                {
                    float dist = (float)(x * x) / (rx * rx) + (float)(y * y) / (ry * ry);
                    if (filled ? dist <= 1f : Mathf.Abs(dist - 1f) < 0.15f)
                        SetPixelSafe(pixels, cx + x, cy + y, color);
                }
        }

        static void FillTriangle(Color[] pixels, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
            for (int py = minY; py <= maxY; py++)
                for (int px = minX; px <= maxX; px++)
                    if (PointInTriangle(px, py, x0, y0, x1, y1, x2, y2))
                        SetPixelSafe(pixels, px, py, color);
        }

        static bool PointInTriangle(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            float d1 = Sign(px, py, x0, y0, x1, y1);
            float d2 = Sign(px, py, x1, y1, x2, y2);
            float d3 = Sign(px, py, x2, y2, x0, y0);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        static float Sign(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            return (x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3);
        }

        // ===== 装备形状绘制 (256x256分辨率，所有坐标为128版本的2倍) =====

        static void DrawSword(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.4f);
            Color dark = Color.Lerp(color, Color.black, 0.3f);
            // 剑身（对角线）
            DrawLine(pixels, cx - 30, cy + 40, cx + 30, cy - 50, color, 10);
            // 剑身高光
            DrawLine(pixels, cx - 28, cy + 38, cx + 28, cy - 48, highlight, 4);
            // 剑尖
            FillTriangle(pixels, cx + 26, cy - 46, cx + 40, cy - 64, cx + 34, cy - 56, highlight);
            // 护手
            DrawLine(pixels, cx - 44, cy + 10, cx + 10, cy + 10, dark, 8);
            // 剑柄
            DrawLine(pixels, cx - 36, cy + 44, cx - 24, cy + 24, new Color(0.5f, 0.35f, 0.2f), 8);
            // 宝石
            DrawCircle(pixels, cx - 16, cy + 10, 6, new Color(1f, 0.3f, 0.2f), true);
        }

        static void DrawBow(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.3f);
            // 弓身（抛物线曲线，加粗）
            for (int i = -60; i <= 60; i++)
            {
                float t = i / 60f;
                int bx = cx - 20 + (int)(36 * (1 - t * t));
                int by = cy + i;
                // 加粗弓身为5像素宽
                for (int dx = -2; dx <= 2; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        SetPixelSafe(pixels, bx + dx, by + dy, color);
                SetPixelSafe(pixels, bx, by, highlight);
                SetPixelSafe(pixels, bx + 1, by, highlight);
            }
            // 弓弦（加粗）
            DrawLine(pixels, cx - 20 + 36, cy - 60, cx - 20 + 36, cy + 60, new Color(0.8f, 0.8f, 0.7f), 3);
            // 箭杆（加粗）
            DrawLine(pixels, cx - 10, cy + 30, cx + 50, cy - 40, new Color(0.6f, 0.5f, 0.3f), 4);
            // 箭头
            FillTriangle(pixels, cx + 46, cy - 36, cx + 60, cy - 56, cx + 56, cy - 30, new Color(0.7f, 0.7f, 0.75f));
            // 箭羽
            DrawLine(pixels, cx - 10, cy + 30, cx - 20, cy + 40, new Color(1f, 0.3f, 0.2f), 4);
            DrawLine(pixels, cx - 10, cy + 30, cx - 4, cy + 44, new Color(1f, 0.3f, 0.2f), 4);
        }

        static void DrawArmor(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.3f);
            Color dark = Color.Lerp(color, Color.black, 0.25f);
            // 躯干（大椭圆）
            DrawEllipse(pixels, cx, cy, 44, 56, color, true);
            // 胸甲高光
            DrawEllipse(pixels, cx - 10, cy - 16, 20, 30, highlight, true);
            // 领口
            DrawEllipse(pixels, cx, cy - 52, 24, 12, dark, true);
            DrawEllipse(pixels, cx, cy - 52, 20, 8, new Color(0.15f, 0.15f, 0.2f), true);
            // 肩甲
            DrawEllipse(pixels, cx - 44, cy - 30, 20, 14, dark, true);
            DrawEllipse(pixels, cx + 44, cy - 30, 20, 14, dark, true);
            // 腰带
            for (int x = cx - 40; x <= cx + 40; x++)
                for (int y = cy + 30; y <= cy + 38; y++)
                    SetPixelSafe(pixels, x, y, dark);
            // 胸甲装饰（中心宝石）
            DrawCircle(pixels, cx, cy - 10, 8, new Color(0.9f, 0.8f, 0.3f), true);
            // 中线纹路
            DrawLine(pixels, cx, cy - 40, cx, cy + 28, dark, 3);
        }

        static void DrawHelmet(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.3f);
            Color dark = Color.Lerp(color, Color.black, 0.25f);
            // 头盔主体
            for (int y = -50; y <= 20; y++)
                for (int x = -44; x <= 44; x++)
                {
                    float dist = (float)(x * x) / (44 * 44) + (float)(y * y) / (50 * 50);
                    if (y < 0 && dist <= 1f)
                        SetPixelSafe(pixels, cx + x, cy + y, color);
                    else if (y >= 0 && Mathf.Abs(x) <= 44)
                        SetPixelSafe(pixels, cx + x, cy + y, color);
                }
            // 高光
            DrawEllipse(pixels, cx - 12, cy - 24, 16, 20, highlight, true);
            // 面罩横条
            for (int x = cx - 36; x <= cx + 36; x++)
                for (int y = cy + 4; y <= cy + 16; y++)
                    SetPixelSafe(pixels, x, y, dark);
            // 面罩缝隙
            for (int x = cx - 28; x <= cx + 28; x++)
                for (int y = cy + 8; y <= cy + 12; y++)
                    SetPixelSafe(pixels, x, y, new Color(0.05f, 0.05f, 0.08f));
            // 顶部装饰
            DrawLine(pixels, cx, cy - 50, cx, cy - 64, dark, 6);
            // 鼻梁
            DrawLine(pixels, cx, cy - 4, cx, cy + 20, dark, 6);
        }

        static void DrawPants(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.25f);
            Color dark = Color.Lerp(color, Color.black, 0.2f);
            // 腰带
            for (int x = cx - 40; x <= cx + 40; x++)
                for (int y = cy - 50; y <= cy - 36; y++)
                    SetPixelSafe(pixels, x, y, dark);
            // 腰带扣
            DrawCircle(pixels, cx, cy - 44, 6, new Color(0.9f, 0.8f, 0.3f), true);
            // 左裤腿
            for (int y = cy - 36; y <= cy + 56; y++)
            {
                float t = (y - (cy - 36)) / 92f;
                int halfWidth = (int)Mathf.Lerp(36, 20, t);
                int legCx = cx - 16;
                for (int x = legCx - halfWidth / 2; x <= legCx + halfWidth / 2; x++)
                    SetPixelSafe(pixels, x, y, color);
            }
            // 右裤腿
            for (int y = cy - 36; y <= cy + 56; y++)
            {
                float t = (y - (cy - 36)) / 92f;
                int halfWidth = (int)Mathf.Lerp(36, 20, t);
                int legCx = cx + 16;
                for (int x = legCx - halfWidth / 2; x <= legCx + halfWidth / 2; x++)
                    SetPixelSafe(pixels, x, y, color);
            }
            // 褶皱线
            DrawLine(pixels, cx - 24, cy - 30, cx - 20, cy + 40, highlight, 4);
            // 膝盖阴影
            DrawEllipse(pixels, cx - 16, cy + 10, 12, 10, dark, true);
            DrawEllipse(pixels, cx + 16, cy + 10, 12, 10, dark, true);
        }

        static void DrawRing(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.4f);
            // 环体
            for (int y = -36; y <= 36; y++)
                for (int x = -44; x <= 44; x++)
                {
                    float outer = (float)(x * x) / (44 * 44) + (float)(y * y) / (36 * 36);
                    float inner = (float)(x * x) / (30 * 30) + (float)(y * y) / (24 * 24);
                    if (outer <= 1f && inner >= 1f)
                    {
                        Color c = y < -10 ? highlight : color;
                        SetPixelSafe(pixels, cx + x, cy + y, c);
                    }
                }
            // 宝石
            DrawCircle(pixels, cx, cy - 36, 14, Color.Lerp(color, new Color(1f, 1f, 1f), 0.3f), true);
            DrawCircle(pixels, cx - 4, cy - 40, 4, new Color(1f, 1f, 1f, 0.7f), true);
            // 宝石座
            DrawLine(pixels, cx - 16, cy - 28, cx + 16, cy - 28, Color.Lerp(color, Color.black, 0.2f), 4);
        }

        static void DrawNecklace(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.3f);
            Color chainColor = new Color(0.8f, 0.75f, 0.5f);
            // 链条（抛物线）
            for (int i = -50; i <= 50; i++)
            {
                float t = i / 50f;
                int nx = cx + i;
                int ny = cy - 30 + (int)(24 * t * t);
                for (int dy = 0; dy <= 3; dy++)
                {
                    SetPixelSafe(pixels, nx, ny + dy, chainColor);
                }
            }
            // 吊坠
            int pendY = cy + 20;
            for (int y = -24; y <= 24; y++)
            {
                int halfW = y < 0 ? (24 + y) : (24 - y);
                halfW = Mathf.Max(halfW * 16 / 24, 0);
                for (int x = -halfW; x <= halfW; x++)
                {
                    Color c = (x < 0 && y < 0) ? highlight : color;
                    SetPixelSafe(pixels, cx + x, pendY + y, c);
                }
            }
            // 吊坠中线
            DrawLine(pixels, cx, pendY - 20, cx, pendY + 20, Color.Lerp(color, Color.white, 0.2f), 2);
            // 链扣
            DrawCircle(pixels, cx, cy - 6, 6, chainColor, true);
        }

        static void DrawPotion(Color[] pixels, int cx, int cy, Color color)
        {
            Color highlight = Color.Lerp(color, Color.white, 0.4f);
            Color glassColor = new Color(0.85f, 0.9f, 0.95f, 0.6f);
            // 瓶身
            DrawEllipse(pixels, cx, cy + 16, 36, 40, color, true);
            // 高光
            DrawEllipse(pixels, cx - 12, cy + 4, 12, 20, highlight, true);
            // 反光点
            DrawCircle(pixels, cx + 16, cy + 6, 6, new Color(1f, 1f, 1f, 0.4f), true);
            // 瓶颈
            for (int y = cy - 24; y <= cy - 4; y++)
            {
                int halfW = 10;
                for (int x = cx - halfW; x <= cx + halfW; x++)
                    SetPixelSafe(pixels, x, y, Color.Lerp(color, glassColor, 0.3f));
            }
            // 瓶口下沿
            for (int x = cx - 14; x <= cx + 14; x++)
                for (int y = cy - 36; y <= cy - 24; y++)
                    SetPixelSafe(pixels, x, y, new Color(0.5f, 0.4f, 0.3f));
            // 瓶塞
            for (int x = cx - 10; x <= cx + 10; x++)
                for (int y = cy - 44; y <= cy - 34; y++)
                    SetPixelSafe(pixels, x, y, new Color(0.6f, 0.5f, 0.35f));
            // 气泡
            DrawCircle(pixels, cx + 8, cy + 24, 4, new Color(1f, 1f, 1f, 0.35f), true);
            DrawCircle(pixels, cx - 6, cy + 36, 2, new Color(1f, 1f, 1f, 0.3f), true);
        }

        static void AddCornerGlow(Color[] pixels, Color glowColor)
        {
            Color glow = new Color(glowColor.r, glowColor.g, glowColor.b, 0.15f);
            int glowSize = 40;
            for (int y = 0; y < glowSize; y++)
                for (int x = 0; x < glowSize; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y) / glowSize;
                    if (dist < 1f)
                    {
                        Color c = new Color(glow.r, glow.g, glow.b, glow.a * (1 - dist));
                        SetPixelSafe(pixels, x + 10, y + 10, c);
                        SetPixelSafe(pixels, ICON_SIZE - 10 - x, y + 10, c);
                        SetPixelSafe(pixels, x + 10, ICON_SIZE - 10 - y, c);
                        SetPixelSafe(pixels, ICON_SIZE - 10 - x, ICON_SIZE - 10 - y, c);
                    }
                }
        }
    }
}
