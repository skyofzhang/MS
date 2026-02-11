using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 物品图标生成器 - 为商城所有装备生成简约风格的图标
/// 每个装备按品质着色，带有装备类型符号
/// 使用方法: Unity菜单 -> Tools -> 生成物品图标
/// </summary>
public class ItemIconGenerator
{
    private const int ICON_SIZE = 128;
    private const string OUTPUT_PATH = "Assets/Resources/Sprites/Items";

    // 品质颜色
    static readonly Color WHITE_BG = new Color(0.35f, 0.35f, 0.38f);   // 白色品质
    static readonly Color GREEN_BG = new Color(0.2f, 0.45f, 0.25f);    // 绿色品质
    static readonly Color BLUE_BG = new Color(0.2f, 0.3f, 0.55f);      // 蓝色品质
    static readonly Color PURPLE_BG = new Color(0.4f, 0.2f, 0.55f);    // 紫色品质
    static readonly Color ORANGE_BG = new Color(0.55f, 0.35f, 0.15f);  // 橙色品质

    // 品质边框颜色
    static readonly Color WHITE_BORDER = new Color(0.6f, 0.6f, 0.6f);
    static readonly Color GREEN_BORDER = new Color(0.3f, 0.8f, 0.3f);
    static readonly Color BLUE_BORDER = new Color(0.3f, 0.5f, 1f);
    static readonly Color PURPLE_BORDER = new Color(0.7f, 0.3f, 1f);
    static readonly Color ORANGE_BORDER = new Color(1f, 0.6f, 0.1f);

    [MenuItem("Tools/生成物品图标")]
    public static void GenerateAllIcons()
    {
        // 确保输出目录存在
        if (!Directory.Exists(OUTPUT_PATH))
        {
            Directory.CreateDirectory(OUTPUT_PATH);
        }

        int count = 0;

        // ===== 武器 =====
        count += GenerateIcon("WPN_001", "剑", WHITE_BG, WHITE_BORDER, new Color(0.7f, 0.7f, 0.75f), IconShape.Sword);
        count += GenerateIcon("WPN_002", "铁", GREEN_BG, GREEN_BORDER, new Color(0.75f, 0.75f, 0.8f), IconShape.Sword);
        count += GenerateIcon("WPN_003", "钢", BLUE_BG, BLUE_BORDER, new Color(0.8f, 0.85f, 1f), IconShape.Sword);
        count += GenerateIcon("WPN_004", "弓", GREEN_BG, GREEN_BORDER, new Color(0.6f, 0.45f, 0.3f), IconShape.Bow);
        count += GenerateIcon("WPN_005", "影", PURPLE_BG, PURPLE_BORDER, new Color(0.3f, 0.1f, 0.4f), IconShape.Sword);
        count += GenerateIcon("WPN_006", "龙", ORANGE_BG, ORANGE_BORDER, new Color(1f, 0.85f, 0.3f), IconShape.Sword);
        count += GenerateIcon("WPN_007", "雷", PURPLE_BG, PURPLE_BORDER, new Color(0.4f, 0.7f, 1f), IconShape.Bow);

        // ===== 护甲 =====
        count += GenerateIcon("ARM_001", "布", WHITE_BG, WHITE_BORDER, new Color(0.65f, 0.55f, 0.45f), IconShape.Armor);
        count += GenerateIcon("ARM_002", "皮", GREEN_BG, GREEN_BORDER, new Color(0.6f, 0.4f, 0.25f), IconShape.Armor);
        count += GenerateIcon("ARM_003", "锁", BLUE_BG, BLUE_BORDER, new Color(0.6f, 0.65f, 0.7f), IconShape.Armor);
        count += GenerateIcon("ARM_004", "板", PURPLE_BG, PURPLE_BORDER, new Color(0.7f, 0.7f, 0.75f), IconShape.Armor);
        count += GenerateIcon("ARM_005", "银", PURPLE_BG, PURPLE_BORDER, new Color(0.8f, 0.85f, 0.9f), IconShape.Armor);
        count += GenerateIcon("ARM_006", "龙", ORANGE_BG, ORANGE_BORDER, new Color(0.2f, 0.6f, 0.3f), IconShape.Armor);

        // ===== 头盔 =====
        count += GenerateIcon("HLM_001", "帽", WHITE_BG, WHITE_BORDER, new Color(0.6f, 0.5f, 0.35f), IconShape.Helmet);
        count += GenerateIcon("HLM_002", "铁", GREEN_BG, GREEN_BORDER, new Color(0.6f, 0.6f, 0.65f), IconShape.Helmet);
        count += GenerateIcon("HLM_003", "鹫", BLUE_BG, BLUE_BORDER, new Color(0.8f, 0.75f, 0.5f), IconShape.Helmet);
        count += GenerateIcon("HLM_004", "冠", PURPLE_BG, PURPLE_BORDER, new Color(0.9f, 0.75f, 0.2f), IconShape.Helmet);

        // ===== 护腿 =====
        count += GenerateIcon("PNT_001", "布", WHITE_BG, WHITE_BORDER, new Color(0.55f, 0.5f, 0.4f), IconShape.Pants);
        count += GenerateIcon("PNT_002", "皮", GREEN_BG, GREEN_BORDER, new Color(0.55f, 0.4f, 0.25f), IconShape.Pants);
        count += GenerateIcon("PNT_003", "铁", BLUE_BG, BLUE_BORDER, new Color(0.6f, 0.6f, 0.7f), IconShape.Pants);
        count += GenerateIcon("PNT_004", "影", PURPLE_BG, PURPLE_BORDER, new Color(0.25f, 0.15f, 0.35f), IconShape.Pants);

        // ===== 戒指 =====
        count += GenerateIcon("RNG_001", "铜", WHITE_BG, WHITE_BORDER, new Color(0.7f, 0.5f, 0.3f), IconShape.Ring);
        count += GenerateIcon("RNG_002", "银", GREEN_BG, GREEN_BORDER, new Color(0.8f, 0.82f, 0.85f), IconShape.Ring);
        count += GenerateIcon("RNG_003", "火", BLUE_BG, BLUE_BORDER, new Color(1f, 0.4f, 0.15f), IconShape.Ring);
        count += GenerateIcon("RNG_004", "霜", PURPLE_BG, PURPLE_BORDER, new Color(0.4f, 0.7f, 1f), IconShape.Ring);

        // ===== 项链 =====
        count += GenerateIcon("NCK_001", "符", WHITE_BG, WHITE_BORDER, new Color(0.7f, 0.65f, 0.4f), IconShape.Necklace);
        count += GenerateIcon("NCK_002", "命", GREEN_BG, GREEN_BORDER, new Color(0.3f, 0.8f, 0.4f), IconShape.Necklace);
        count += GenerateIcon("NCK_003", "守", BLUE_BG, BLUE_BORDER, new Color(0.9f, 0.85f, 0.5f), IconShape.Necklace);
        count += GenerateIcon("NCK_004", "心", PURPLE_BG, PURPLE_BORDER, new Color(1f, 0.3f, 0.3f), IconShape.Necklace);

        // ===== 消耗品 =====
        count += GenerateIcon("CON_001", "小", WHITE_BG, WHITE_BORDER, new Color(0.9f, 0.2f, 0.2f), IconShape.Potion);
        count += GenerateIcon("CON_002", "中", GREEN_BG, GREEN_BORDER, new Color(0.9f, 0.3f, 0.3f), IconShape.Potion);
        count += GenerateIcon("CON_003", "大", BLUE_BG, BLUE_BORDER, new Color(1f, 0.2f, 0.2f), IconShape.Potion);
        count += GenerateIcon("CON_004", "攻", GREEN_BG, GREEN_BORDER, new Color(1f, 0.5f, 0.1f), IconShape.Potion);

        AssetDatabase.Refresh();

        // 自动设置所有生成的PNG为Sprite类型
        string[] pngFiles = Directory.GetFiles(OUTPUT_PATH, "*.png");
        foreach (string pngFile in pngFiles)
        {
            string assetPath = pngFile.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[ItemIconGenerator] 生成完成！共 {count} 个图标，路径: {OUTPUT_PATH}");
        EditorUtility.DisplayDialog("图标生成完成", $"共生成 {count} 个物品图标\n路径: {OUTPUT_PATH}\n已自动设置为Sprite类型", "确定");
    }

    enum IconShape
    {
        Sword, Bow, Armor, Helmet, Pants, Ring, Necklace, Potion
    }

    static int GenerateIcon(string id, string label, Color bgColor, Color borderColor, Color itemColor, IconShape shape)
    {
        Texture2D tex = new Texture2D(ICON_SIZE, ICON_SIZE, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[ICON_SIZE * ICON_SIZE];

        // 填充透明
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        int center = ICON_SIZE / 2;

        // 1. 绘制圆角矩形背景
        DrawRoundedRect(pixels, 4, 4, ICON_SIZE - 8, ICON_SIZE - 8, 12, bgColor);

        // 2. 绘制边框（3px粗）
        DrawRoundedRectBorder(pixels, 2, 2, ICON_SIZE - 4, ICON_SIZE - 4, 14, borderColor, 3);

        // 3. 绘制装备形状
        switch (shape)
        {
            case IconShape.Sword:
                DrawSword(pixels, center, center - 5, itemColor);
                break;
            case IconShape.Bow:
                DrawBow(pixels, center, center - 5, itemColor);
                break;
            case IconShape.Armor:
                DrawArmor(pixels, center, center - 2, itemColor);
                break;
            case IconShape.Helmet:
                DrawHelmet(pixels, center, center - 5, itemColor);
                break;
            case IconShape.Pants:
                DrawPants(pixels, center, center - 2, itemColor);
                break;
            case IconShape.Ring:
                DrawRing(pixels, center, center - 3, itemColor);
                break;
            case IconShape.Necklace:
                DrawNecklace(pixels, center, center - 5, itemColor);
                break;
            case IconShape.Potion:
                DrawPotion(pixels, center, center - 5, itemColor);
                break;
        }

        // 4. 品质光晕（角落发光效果）
        AddCornerGlow(pixels, borderColor);

        tex.SetPixels(pixels);
        tex.Apply();

        // 保存为PNG
        string filePath = Path.Combine(OUTPUT_PATH, $"{id}.png");
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);
        Object.DestroyImmediate(tex);

        return 1;
    }

    // ===== 绘制辅助方法 =====

    static void SetPixelSafe(Color[] pixels, int x, int y, Color color)
    {
        if (x >= 0 && x < ICON_SIZE && y >= 0 && y < ICON_SIZE)
        {
            int idx = y * ICON_SIZE + x;
            // Alpha混合
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
                // 检查是否在圆角内
                bool inside = true;
                // 左上角
                if (px < x + radius && py < y + radius)
                    inside = (px - x - radius) * (px - x - radius) + (py - y - radius) * (py - y - radius) <= radius * radius;
                // 右上角
                else if (px >= x + w - radius && py < y + radius)
                    inside = (px - x - w + radius) * (px - x - w + radius) + (py - y - radius) * (py - y - radius) <= radius * radius;
                // 左下角
                else if (px < x + radius && py >= y + h - radius)
                    inside = (px - x - radius) * (px - x - radius) + (py - y - h + radius) * (py - y - h + radius) <= radius * radius;
                // 右下角
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

            // Top & Bottom
            for (int px = cx + r; px < cx + cw - r; px++)
            {
                SetPixelSafe(pixels, px, cy, color);
                SetPixelSafe(pixels, px, cy + ch - 1, color);
            }
            // Left & Right
            for (int py = cy + r; py < cy + ch - r; py++)
            {
                SetPixelSafe(pixels, cx, py, color);
                SetPixelSafe(pixels, cx + cw - 1, py, color);
            }
            // Corners
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
        {
            for (int x = -r; x <= r; x++)
            {
                float dist = Mathf.Sqrt(x * x + y * y);
                if (filled ? dist <= r : Mathf.Abs(dist - r) < 1.5f)
                {
                    SetPixelSafe(pixels, cx + x, cy + y, color);
                }
            }
        }
    }

    static void DrawEllipse(Color[] pixels, int cx, int cy, int rx, int ry, Color color, bool filled = true)
    {
        for (int y = -ry; y <= ry; y++)
        {
            for (int x = -rx; x <= rx; x++)
            {
                float dist = (float)(x * x) / (rx * rx) + (float)(y * y) / (ry * ry);
                if (filled ? dist <= 1f : Mathf.Abs(dist - 1f) < 0.15f)
                {
                    SetPixelSafe(pixels, cx + x, cy + y, color);
                }
            }
        }
    }

    static void FillTriangle(Color[] pixels, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
    {
        int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
        int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
        int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
        int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));

        for (int py = minY; py <= maxY; py++)
        {
            for (int px = minX; px <= maxX; px++)
            {
                if (PointInTriangle(px, py, x0, y0, x1, y1, x2, y2))
                    SetPixelSafe(pixels, px, py, color);
            }
        }
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

    // ===== 装备形状绘制 =====

    static void DrawSword(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.4f);
        Color dark = Color.Lerp(color, Color.black, 0.3f);

        // 剑身（斜向右上方）
        DrawLine(pixels, cx - 15, cy + 20, cx + 15, cy - 25, color, 5);
        DrawLine(pixels, cx - 14, cy + 19, cx + 14, cy - 24, highlight, 2);

        // 剑尖
        FillTriangle(pixels, cx + 13, cy - 23, cx + 20, cy - 32, cx + 17, cy - 28, highlight);

        // 护手（横向）
        DrawLine(pixels, cx - 22, cy + 5, cx + 5, cy + 5, dark, 4);

        // 剑柄
        DrawLine(pixels, cx - 18, cy + 22, cx - 12, cy + 12, new Color(0.5f, 0.35f, 0.2f), 4);

        // 宝石（护手中心）
        DrawCircle(pixels, cx - 8, cy + 5, 3, new Color(1f, 0.3f, 0.2f), true);
    }

    static void DrawBow(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.3f);

        // 弓臂（弧形）- 用多段线模拟曲线
        for (int i = -30; i <= 30; i++)
        {
            float t = i / 30f;
            int bx = cx - 10 + (int)(18 * (1 - t * t));
            int by = cy + i;
            SetPixelSafe(pixels, bx, by, color);
            SetPixelSafe(pixels, bx + 1, by, color);
            SetPixelSafe(pixels, bx - 1, by, color);
            SetPixelSafe(pixels, bx, by, highlight);
        }

        // 弓弦
        DrawLine(pixels, cx - 10 + 18, cy - 30, cx - 10 + 18, cy + 30, new Color(0.8f, 0.8f, 0.7f), 1);

        // 箭
        DrawLine(pixels, cx - 5, cy + 15, cx + 25, cy - 20, new Color(0.6f, 0.5f, 0.3f), 2);
        // 箭头
        FillTriangle(pixels, cx + 23, cy - 18, cx + 30, cy - 28, cx + 28, cy - 15, new Color(0.7f, 0.7f, 0.75f));
        // 箭羽
        DrawLine(pixels, cx - 5, cy + 15, cx - 10, cy + 20, new Color(1f, 0.3f, 0.2f), 2);
        DrawLine(pixels, cx - 5, cy + 15, cx - 2, cy + 22, new Color(1f, 0.3f, 0.2f), 2);
    }

    static void DrawArmor(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.3f);
        Color dark = Color.Lerp(color, Color.black, 0.25f);

        // 躯干
        DrawEllipse(pixels, cx, cy, 22, 28, color, true);
        // 高光
        DrawEllipse(pixels, cx - 5, cy - 8, 10, 15, highlight, true);

        // 领口
        DrawEllipse(pixels, cx, cy - 26, 12, 6, dark, true);
        DrawEllipse(pixels, cx, cy - 26, 10, 4, new Color(0.15f, 0.15f, 0.2f), true);

        // 肩甲
        DrawEllipse(pixels, cx - 22, cy - 15, 10, 7, dark, true);
        DrawEllipse(pixels, cx + 22, cy - 15, 10, 7, dark, true);

        // 腰带
        for (int x = cx - 20; x <= cx + 20; x++)
            for (int y = cy + 15; y <= cy + 19; y++)
                SetPixelSafe(pixels, x, y, dark);

        // 装饰（中心宝石）
        DrawCircle(pixels, cx, cy - 5, 4, new Color(0.9f, 0.8f, 0.3f), true);
    }

    static void DrawHelmet(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.3f);
        Color dark = Color.Lerp(color, Color.black, 0.25f);

        // 头盔主体（半圆）
        for (int y = -25; y <= 10; y++)
        {
            for (int x = -22; x <= 22; x++)
            {
                float dist = (float)(x * x) / (22 * 22) + (float)(y * y) / (25 * 25);
                if (y < 0 && dist <= 1f)
                    SetPixelSafe(pixels, cx + x, cy + y, color);
                else if (y >= 0 && Mathf.Abs(x) <= 22)
                    SetPixelSafe(pixels, cx + x, cy + y, color);
            }
        }

        // 高光
        DrawEllipse(pixels, cx - 6, cy - 12, 8, 10, highlight, true);

        // 面罩/护目
        for (int x = cx - 18; x <= cx + 18; x++)
            for (int y = cy + 2; y <= cy + 8; y++)
                SetPixelSafe(pixels, x, y, dark);

        // 眼缝
        for (int x = cx - 14; x <= cx + 14; x++)
            for (int y = cy + 4; y <= cy + 6; y++)
                SetPixelSafe(pixels, x, y, new Color(0.05f, 0.05f, 0.08f));

        // 顶部装饰（竖线）
        DrawLine(pixels, cx, cy - 25, cx, cy - 32, dark, 3);

        // 护鼻
        DrawLine(pixels, cx, cy - 2, cx, cy + 10, dark, 3);
    }

    static void DrawPants(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.25f);
        Color dark = Color.Lerp(color, Color.black, 0.2f);

        // 腰部
        for (int x = cx - 20; x <= cx + 20; x++)
            for (int y = cy - 25; y <= cy - 18; y++)
                SetPixelSafe(pixels, x, y, dark);

        // 腰带扣
        DrawCircle(pixels, cx, cy - 22, 3, new Color(0.9f, 0.8f, 0.3f), true);

        // 左腿
        for (int y = cy - 18; y <= cy + 28; y++)
        {
            float t = (y - (cy - 18)) / 46f;
            int halfWidth = (int)Mathf.Lerp(18, 10, t);
            int legCx = cx - 8;
            for (int x = legCx - halfWidth / 2; x <= legCx + halfWidth / 2; x++)
                SetPixelSafe(pixels, x, y, color);
        }

        // 右腿
        for (int y = cy - 18; y <= cy + 28; y++)
        {
            float t = (y - (cy - 18)) / 46f;
            int halfWidth = (int)Mathf.Lerp(18, 10, t);
            int legCx = cx + 8;
            for (int x = legCx - halfWidth / 2; x <= legCx + halfWidth / 2; x++)
                SetPixelSafe(pixels, x, y, color);
        }

        // 高光（左腿内侧）
        DrawLine(pixels, cx - 12, cy - 15, cx - 10, cy + 20, highlight, 2);

        // 膝盖护甲
        DrawEllipse(pixels, cx - 8, cy + 5, 6, 5, dark, true);
        DrawEllipse(pixels, cx + 8, cy + 5, 6, 5, dark, true);
    }

    static void DrawRing(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.4f);

        // 戒指环（椭圆环）
        for (int y = -18; y <= 18; y++)
        {
            for (int x = -22; x <= 22; x++)
            {
                float outer = (float)(x * x) / (22 * 22) + (float)(y * y) / (18 * 18);
                float inner = (float)(x * x) / (15 * 15) + (float)(y * y) / (12 * 12);
                if (outer <= 1f && inner >= 1f)
                {
                    Color c = y < -5 ? highlight : color;
                    SetPixelSafe(pixels, cx + x, cy + y, c);
                }
            }
        }

        // 宝石（顶部）
        DrawCircle(pixels, cx, cy - 18, 7, Color.Lerp(color, new Color(1f, 1f, 1f), 0.3f), true);
        // 宝石高光
        DrawCircle(pixels, cx - 2, cy - 20, 2, new Color(1f, 1f, 1f, 0.7f), true);

        // 宝石底座
        DrawLine(pixels, cx - 8, cy - 14, cx + 8, cy - 14, Color.Lerp(color, Color.black, 0.2f), 2);
    }

    static void DrawNecklace(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.3f);
        Color chainColor = new Color(0.8f, 0.75f, 0.5f);

        // 链条（U形曲线）
        for (int i = -25; i <= 25; i++)
        {
            float t = i / 25f;
            int nx = cx + i;
            int ny = cy - 15 + (int)(12 * t * t);
            SetPixelSafe(pixels, nx, ny, chainColor);
            SetPixelSafe(pixels, nx, ny + 1, chainColor);
        }

        // 吊坠（菱形/宝石形）
        int pendY = cy + 10;
        for (int y = -12; y <= 12; y++)
        {
            int halfW = y < 0 ? (12 + y) : (12 - y);
            halfW = Mathf.Max(halfW * 8 / 12, 0);
            for (int x = -halfW; x <= halfW; x++)
            {
                Color c = (x < 0 && y < 0) ? highlight : color;
                SetPixelSafe(pixels, cx + x, pendY + y, c);
            }
        }

        // 吊坠装饰线
        DrawLine(pixels, cx, pendY - 10, cx, pendY + 10, Color.Lerp(color, Color.white, 0.2f), 1);

        // 顶部连接点
        DrawCircle(pixels, cx, cy - 3, 3, chainColor, true);
    }

    static void DrawPotion(Color[] pixels, int cx, int cy, Color color)
    {
        Color highlight = Color.Lerp(color, Color.white, 0.4f);
        Color glassColor = new Color(0.85f, 0.9f, 0.95f, 0.6f);

        // 瓶身（下部圆形）
        DrawEllipse(pixels, cx, cy + 8, 18, 20, color, true);
        // 高光
        DrawEllipse(pixels, cx - 6, cy + 2, 6, 10, highlight, true);

        // 玻璃反光
        DrawCircle(pixels, cx + 8, cy + 3, 3, new Color(1f, 1f, 1f, 0.4f), true);

        // 瓶颈
        for (int y = cy - 12; y <= cy - 2; y++)
        {
            int halfW = 5;
            for (int x = cx - halfW; x <= cx + halfW; x++)
                SetPixelSafe(pixels, x, y, Color.Lerp(color, glassColor, 0.3f));
        }

        // 瓶口
        for (int x = cx - 7; x <= cx + 7; x++)
            for (int y = cy - 18; y <= cy - 12; y++)
                SetPixelSafe(pixels, x, y, new Color(0.5f, 0.4f, 0.3f));

        // 软木塞
        for (int x = cx - 5; x <= cx + 5; x++)
            for (int y = cy - 22; y <= cy - 17; y++)
                SetPixelSafe(pixels, x, y, new Color(0.6f, 0.5f, 0.35f));

        // 气泡
        DrawCircle(pixels, cx + 4, cy + 12, 2, new Color(1f, 1f, 1f, 0.35f), true);
        DrawCircle(pixels, cx - 3, cy + 18, 1, new Color(1f, 1f, 1f, 0.3f), true);
    }

    static void AddCornerGlow(Color[] pixels, Color glowColor)
    {
        Color glow = new Color(glowColor.r, glowColor.g, glowColor.b, 0.15f);
        int glowSize = 20;

        // 四角添加微光
        for (int y = 0; y < glowSize; y++)
        {
            for (int x = 0; x < glowSize; x++)
            {
                float dist = Mathf.Sqrt(x * x + y * y) / glowSize;
                if (dist < 1f)
                {
                    Color c = new Color(glow.r, glow.g, glow.b, glow.a * (1 - dist));
                    // 左下角
                    SetPixelSafe(pixels, x + 5, y + 5, c);
                    // 右下角
                    SetPixelSafe(pixels, ICON_SIZE - 5 - x, y + 5, c);
                    // 左上角
                    SetPixelSafe(pixels, x + 5, ICON_SIZE - 5 - y, c);
                    // 右上角
                    SetPixelSafe(pixels, ICON_SIZE - 5 - x, ICON_SIZE - 5 - y, c);
                }
            }
        }
    }
}
