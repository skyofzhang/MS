using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// UI资源生成器
/// 为MainMenu和StageSelect场景生成程序化UI资源
/// </summary>
public class UIResourceGenerator : EditorWindow
{
    [MenuItem("MoShou/生成UI资源/全部生成")]
    public static void GenerateAllUIResources()
    {
        GenerateMainMenuResources();
        GenerateStageSelectResources();
        GenerateCommonUIResources();

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("UI资源生成完成",
            "已生成以下资源:\n" +
            "✓ 主菜单背景、LOGO、按钮\n" +
            "✓ 选关界面背景、关卡按钮、锁定图标\n" +
            "✓ 通用UI元素\n\n" +
            "请在场景中应用这些资源",
            "确定");
    }

    [MenuItem("MoShou/生成UI资源/主菜单资源")]
    public static void GenerateMainMenuResources()
    {
        string outputPath = "Assets/Resources/Sprites/UI/MainMenu";
        EnsureDirectoryExists(outputPath);

        // 主菜单背景 (渐变深蓝色背景)
        CreateGradientTexture($"{outputPath}/UI_MainMenu_BG.png", 1080, 1920,
            new Color(0.05f, 0.08f, 0.15f), new Color(0.1f, 0.15f, 0.25f), true);

        // 游戏LOGO (带描边的标题框)
        CreateLogoTexture($"{outputPath}/UI_MainMenu_Logo.png", 800, 300);

        // 装饰框架
        CreateFrameTexture($"{outputPath}/UI_MainMenu_Frame.png", 600, 400,
            new Color(0.8f, 0.6f, 0.2f), 8);

        // 按钮 - 开始游戏
        CreateButtonTexture($"{outputPath}/UI_Btn_Start_Normal.png", 400, 80,
            new Color(0.2f, 0.5f, 0.8f), new Color(0.3f, 0.6f, 0.9f));
        CreateButtonTexture($"{outputPath}/UI_Btn_Start_Pressed.png", 400, 80,
            new Color(0.15f, 0.4f, 0.7f), new Color(0.2f, 0.5f, 0.8f));

        // 按钮 - 继续游戏
        CreateButtonTexture($"{outputPath}/UI_Btn_Continue_Normal.png", 400, 80,
            new Color(0.3f, 0.6f, 0.3f), new Color(0.4f, 0.7f, 0.4f));
        CreateButtonTexture($"{outputPath}/UI_Btn_Continue_Disabled.png", 400, 80,
            new Color(0.3f, 0.3f, 0.3f), new Color(0.4f, 0.4f, 0.4f));

        // 按钮 - 设置
        CreateButtonTexture($"{outputPath}/UI_Btn_Settings_Normal.png", 400, 80,
            new Color(0.5f, 0.4f, 0.3f), new Color(0.6f, 0.5f, 0.4f));

        // 按钮 - 退出
        CreateButtonTexture($"{outputPath}/UI_Btn_Quit_Normal.png", 400, 80,
            new Color(0.6f, 0.3f, 0.3f), new Color(0.7f, 0.4f, 0.4f));

        Debug.Log("[UIResourceGenerator] 主菜单资源生成完成");
    }

    [MenuItem("MoShou/生成UI资源/选关界面资源")]
    public static void GenerateStageSelectResources()
    {
        string outputPath = "Assets/Resources/Sprites/UI/StageSelect";
        EnsureDirectoryExists(outputPath);

        // 选关背景 (地图风格)
        CreateMapBackgroundTexture($"{outputPath}/UI_StageSelect_BG.png", 1080, 1920);

        // 章节标题背景
        CreateBannerTexture($"{outputPath}/UI_Chapter_Banner.png", 600, 100,
            new Color(0.4f, 0.3f, 0.2f));

        // 关卡按钮 - 已解锁
        CreateStageButtonTexture($"{outputPath}/UI_Stage_Unlocked.png", 120, 120,
            new Color(0.3f, 0.5f, 0.7f), false);

        // 关卡按钮 - 已通关
        CreateStageButtonTexture($"{outputPath}/UI_Stage_Cleared.png", 120, 120,
            new Color(0.3f, 0.7f, 0.3f), false);

        // 关卡按钮 - 锁定
        CreateStageButtonTexture($"{outputPath}/UI_Stage_Locked.png", 120, 120,
            new Color(0.4f, 0.4f, 0.4f), true);

        // 锁定图标
        CreateLockIconTexture($"{outputPath}/UI_Icon_Lock.png", 64, 64);

        // 星星 - 空
        CreateStarTexture($"{outputPath}/UI_Star_Empty.png", 48, 48, false);

        // 星星 - 满
        CreateStarTexture($"{outputPath}/UI_Star_Filled.png", 48, 48, true);

        // 返回按钮
        CreateBackButtonTexture($"{outputPath}/UI_Btn_Back.png", 80, 80);

        // 关卡信息面板背景
        CreatePanelTexture($"{outputPath}/UI_StageInfo_Panel.png", 500, 400,
            new Color(0.15f, 0.12f, 0.1f, 0.95f));

        Debug.Log("[UIResourceGenerator] 选关界面资源生成完成");
    }

    [MenuItem("MoShou/生成UI资源/通用UI资源")]
    public static void GenerateCommonUIResources()
    {
        string outputPath = "Assets/Resources/Sprites/UI/Common";
        EnsureDirectoryExists(outputPath);

        // 通用面板背景
        CreatePanelTexture($"{outputPath}/UI_Panel_Dark.png", 400, 300,
            new Color(0.1f, 0.1f, 0.12f, 0.95f));

        // 分割线
        CreateLineTexture($"{outputPath}/UI_Divider.png", 300, 4,
            new Color(0.5f, 0.4f, 0.3f));

        // 玩家信息框
        CreateFrameTexture($"{outputPath}/UI_PlayerInfo_Frame.png", 300, 80,
            new Color(0.6f, 0.5f, 0.3f), 4);

        Debug.Log("[UIResourceGenerator] 通用UI资源生成完成");
    }

    #region 纹理生成方法

    static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    static void CreateGradientTexture(string path, int width, int height, Color topColor, Color bottomColor, bool vertical)
    {
        Texture2D tex = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float t = vertical ? (float)y / height : (float)x / width;
                Color c = Color.Lerp(bottomColor, topColor, t);

                // 添加一些噪点纹理
                float noise = Random.Range(-0.02f, 0.02f);
                c = new Color(
                    Mathf.Clamp01(c.r + noise),
                    Mathf.Clamp01(c.g + noise),
                    Mathf.Clamp01(c.b + noise),
                    c.a
                );

                tex.SetPixel(x, y, c);
            }
        }

        // 添加边缘暗角
        AddVignette(tex, 0.3f);

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateLogoTexture(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        // 透明背景
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, clear);

        // 绘制装饰性边框
        Color gold = new Color(0.85f, 0.7f, 0.3f, 1f);
        Color darkGold = new Color(0.6f, 0.45f, 0.15f, 1f);

        // 外框
        DrawRoundedRect(tex, 10, 10, width - 20, height - 20, 20, gold, 6);

        // 内部渐变背景
        for (int y = 20; y < height - 20; y++)
        {
            for (int x = 20; x < width - 20; x++)
            {
                float t = (float)(y - 20) / (height - 40);
                Color bg = Color.Lerp(
                    new Color(0.12f, 0.08f, 0.05f, 0.95f),
                    new Color(0.08f, 0.05f, 0.03f, 0.95f),
                    t
                );
                tex.SetPixel(x, y, bg);
            }
        }

        // 添加装饰线
        DrawHorizontalLine(tex, 50, 60, width - 100, darkGold, 3);
        DrawHorizontalLine(tex, 50, height - 60, width - 100, darkGold, 3);

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateButtonTexture(string path, int width, int height, Color mainColor, Color highlightColor)
    {
        Texture2D tex = new Texture2D(width, height);

        // 圆角矩形按钮
        int radius = 15;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 检查是否在圆角范围内
                bool inButton = IsInRoundedRect(x, y, width, height, radius);

                if (inButton)
                {
                    // 渐变从上到下
                    float t = (float)y / height;
                    Color c = Color.Lerp(highlightColor, mainColor, t);

                    // 边缘描边
                    bool isEdge = !IsInRoundedRect(x, y, width - 4, height - 4, radius - 2) ||
                                  x < 2 || x >= width - 2 || y < 2 || y >= height - 2;
                    if (isEdge && IsInRoundedRect(x, y, width, height, radius))
                    {
                        c = Color.Lerp(c, Color.white, 0.3f);
                    }

                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }

        // 底部阴影效果
        for (int x = 0; x < width; x++)
        {
            for (int dy = 0; dy < 4; dy++)
            {
                int y = dy;
                if (IsInRoundedRect(x, y, width, height, radius))
                {
                    Color c = tex.GetPixel(x, y);
                    c = Color.Lerp(c, Color.black, 0.3f * (1 - (float)dy / 4));
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateFrameTexture(string path, int width, int height, Color frameColor, int thickness)
    {
        Texture2D tex = new Texture2D(width, height);

        Color clear = new Color(0, 0, 0, 0);
        Color inner = new Color(0.05f, 0.05f, 0.08f, 0.9f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isFrame = x < thickness || x >= width - thickness ||
                              y < thickness || y >= height - thickness;

                if (isFrame)
                {
                    // 渐变边框
                    float edge = Mathf.Min(
                        Mathf.Min(x, width - 1 - x),
                        Mathf.Min(y, height - 1 - y)
                    ) / (float)thickness;

                    Color c = Color.Lerp(frameColor * 0.6f, frameColor, edge);
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, inner);
                }
            }
        }

        // 角落装饰
        DrawCornerDecorations(tex, width, height, frameColor, thickness * 2);

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateMapBackgroundTexture(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        // 羊皮纸/地图风格背景
        Color baseColor = new Color(0.85f, 0.78f, 0.65f);
        Color darkColor = new Color(0.6f, 0.52f, 0.4f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 基础颜色加噪点
                float noise = Mathf.PerlinNoise(x * 0.01f, y * 0.01f);
                float noise2 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);

                Color c = Color.Lerp(darkColor, baseColor, noise * 0.5f + 0.5f);
                c = Color.Lerp(c, c * 0.9f, noise2 * 0.3f);

                tex.SetPixel(x, y, c);
            }
        }

        // 边缘做旧效果
        AddVignette(tex, 0.4f);

        // 添加一些污渍效果
        for (int i = 0; i < 20; i++)
        {
            int cx = Random.Range(50, width - 50);
            int cy = Random.Range(50, height - 50);
            int size = Random.Range(30, 80);
            float alpha = Random.Range(0.05f, 0.15f);

            for (int dy = -size; dy <= size; dy++)
            {
                for (int dx = -size; dx <= size; dx++)
                {
                    int px = cx + dx;
                    int py = cy + dy;
                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy) / size;
                        if (dist < 1)
                        {
                            Color c = tex.GetPixel(px, py);
                            c = Color.Lerp(c, darkColor, alpha * (1 - dist));
                            tex.SetPixel(px, py, c);
                        }
                    }
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateBannerTexture(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);

        Color darkColor = color * 0.6f;
        darkColor.a = 1f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 边缘渐变
                float edgeX = Mathf.Min(x, width - 1 - x) / (float)(width * 0.15f);
                edgeX = Mathf.Clamp01(edgeX);

                float t = (float)y / height;
                Color c = Color.Lerp(color, darkColor, t * 0.5f);
                c.a = edgeX;

                tex.SetPixel(x, y, c);
            }
        }

        // 顶部高光
        for (int x = 0; x < width; x++)
        {
            Color c = tex.GetPixel(x, height - 1);
            c = Color.Lerp(c, Color.white, 0.2f);
            tex.SetPixel(x, height - 1, c);
            tex.SetPixel(x, height - 2, Color.Lerp(tex.GetPixel(x, height - 2), Color.white, 0.1f));
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateStageButtonTexture(string path, int size, int _, Color color, bool locked)
    {
        Texture2D tex = new Texture2D(size, size);

        int radius = size / 2 - 5;
        int cx = size / 2;
        int cy = size / 2;

        Color clear = new Color(0, 0, 0, 0);
        Color darkColor = color * 0.5f;
        darkColor.a = 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                if (dist <= radius)
                {
                    // 径向渐变
                    float t = dist / radius;
                    Color c = Color.Lerp(color, darkColor, t * 0.5f);

                    // 顶部高光
                    if (y > cy)
                    {
                        float highlight = (float)(y - cy) / (size / 2);
                        c = Color.Lerp(c, Color.white, highlight * 0.2f);
                    }

                    tex.SetPixel(x, y, c);
                }
                else if (dist <= radius + 3)
                {
                    // 描边
                    tex.SetPixel(x, y, locked ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.8f, 0.7f, 0.4f));
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateLockIconTexture(string path, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        Color clear = new Color(0, 0, 0, 0);
        Color lockColor = new Color(0.3f, 0.3f, 0.35f);
        Color shackleColor = new Color(0.4f, 0.4f, 0.45f);

        // 清除背景
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, clear);

        int cx = width / 2;

        // 锁身 (下半部分矩形)
        int bodyTop = height / 2;
        int bodyBottom = 8;
        int bodyLeft = width / 4;
        int bodyRight = width - width / 4;

        for (int y = bodyBottom; y <= bodyTop; y++)
        {
            for (int x = bodyLeft; x <= bodyRight; x++)
            {
                tex.SetPixel(x, y, lockColor);
            }
        }

        // 锁环 (上半部分弧形)
        int shackleRadius = width / 4;
        int shackleThickness = 6;
        int shackleY = bodyTop + shackleRadius;

        for (int y = bodyTop; y < height - 5; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - shackleY) * (y - shackleY));
                if (dist >= shackleRadius - shackleThickness / 2 && dist <= shackleRadius + shackleThickness / 2)
                {
                    if (y >= shackleY) // 只画上半弧
                    {
                        tex.SetPixel(x, y, shackleColor);
                    }
                }
            }
        }

        // 锁孔
        int holeY = (bodyTop + bodyBottom) / 2;
        int holeRadius = 4;
        for (int dy = -holeRadius; dy <= holeRadius; dy++)
        {
            for (int dx = -holeRadius; dx <= holeRadius; dx++)
            {
                if (dx * dx + dy * dy <= holeRadius * holeRadius)
                {
                    tex.SetPixel(cx + dx, holeY + dy, new Color(0.15f, 0.15f, 0.18f));
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateStarTexture(string path, int size, int _, bool filled)
    {
        Texture2D tex = new Texture2D(size, size);

        Color clear = new Color(0, 0, 0, 0);
        Color starColor = filled ? new Color(1f, 0.85f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
        Color outlineColor = filled ? new Color(0.8f, 0.6f, 0.1f) : new Color(0.4f, 0.4f, 0.4f);

        // 清除背景
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // 简化的五角星绘制
        float cx = size / 2f;
        float cy = size / 2f;
        float outerR = size / 2f - 3;
        float innerR = outerR * 0.4f;

        // 使用填充多边形近似
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // 五角星的边界函数
                float starAngle = angle + Mathf.PI / 2; // 旋转让星星朝上
                float pointAngle = ((starAngle / (2 * Mathf.PI)) * 5 + 0.5f) % 1;
                float pointDist = Mathf.Abs(pointAngle - 0.5f) * 2; // 0到1
                float maxDist = Mathf.Lerp(outerR, innerR, pointDist);

                if (dist <= maxDist)
                {
                    tex.SetPixel(x, y, starColor);
                }
                else if (dist <= maxDist + 2)
                {
                    tex.SetPixel(x, y, outlineColor);
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateBackButtonTexture(string path, int size, int _)
    {
        Texture2D tex = new Texture2D(size, size);

        Color clear = new Color(0, 0, 0, 0);
        Color bgColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
        Color arrowColor = new Color(0.9f, 0.9f, 0.9f);

        int cx = size / 2;
        int cy = size / 2;
        int radius = size / 2 - 4;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                if (dist <= radius)
                {
                    tex.SetPixel(x, y, bgColor);
                }
                else if (dist <= radius + 3)
                {
                    tex.SetPixel(x, y, new Color(0.5f, 0.5f, 0.5f));
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }

        // 绘制左箭头
        int arrowSize = size / 3;
        int arrowX = cx - arrowSize / 4;

        for (int i = 0; i < arrowSize; i++)
        {
            int y1 = cy + i;
            int y2 = cy - i;
            int x1 = arrowX + i;

            if (x1 < size && y1 < size && y2 >= 0)
            {
                tex.SetPixel(x1, y1, arrowColor);
                tex.SetPixel(x1, y2, arrowColor);
                tex.SetPixel(x1 + 1, y1, arrowColor);
                tex.SetPixel(x1 + 1, y2, arrowColor);
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreatePanelTexture(string path, int width, int height, Color bgColor)
    {
        Texture2D tex = new Texture2D(width, height);

        int radius = 15;
        Color borderColor = new Color(0.4f, 0.35f, 0.25f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inPanel = IsInRoundedRect(x, y, width, height, radius);

                if (inPanel)
                {
                    bool isBorder = !IsInRoundedRect(x, y, width - 6, height - 6, radius - 3) ||
                                   x < 3 || x >= width - 3 || y < 3 || y >= height - 3;

                    if (isBorder && IsInRoundedRect(x, y, width, height, radius))
                    {
                        tex.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, bgColor);
                    }
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateLineTexture(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float alpha = 1f;
                // 两端渐变透明
                float edgeFade = Mathf.Min(x, width - 1 - x) / (width * 0.1f);
                alpha = Mathf.Clamp01(edgeFade);

                Color c = color;
                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        SaveTexture(tex, path);
    }

    #endregion

    #region 辅助方法

    static bool IsInRoundedRect(int x, int y, int width, int height, int radius)
    {
        // 检查四个角
        if (x < radius && y < radius)
        {
            return (x - radius) * (x - radius) + (y - radius) * (y - radius) <= radius * radius;
        }
        if (x >= width - radius && y < radius)
        {
            return (x - (width - radius - 1)) * (x - (width - radius - 1)) + (y - radius) * (y - radius) <= radius * radius;
        }
        if (x < radius && y >= height - radius)
        {
            return (x - radius) * (x - radius) + (y - (height - radius - 1)) * (y - (height - radius - 1)) <= radius * radius;
        }
        if (x >= width - radius && y >= height - radius)
        {
            return (x - (width - radius - 1)) * (x - (width - radius - 1)) + (y - (height - radius - 1)) * (y - (height - radius - 1)) <= radius * radius;
        }

        return x >= 0 && x < width && y >= 0 && y < height;
    }

    static void DrawRoundedRect(Texture2D tex, int left, int top, int width, int height, int radius, Color color, int thickness)
    {
        for (int t = 0; t < thickness; t++)
        {
            int l = left + t;
            int tp = top + t;
            int w = width - t * 2;
            int h = height - t * 2;
            int r = Mathf.Max(1, radius - t);

            // 上边
            for (int x = l + r; x < l + w - r; x++)
                tex.SetPixel(x, tp + h - 1, color);
            // 下边
            for (int x = l + r; x < l + w - r; x++)
                tex.SetPixel(x, tp, color);
            // 左边
            for (int y = tp + r; y < tp + h - r; y++)
                tex.SetPixel(l, y, color);
            // 右边
            for (int y = tp + r; y < tp + h - r; y++)
                tex.SetPixel(l + w - 1, y, color);

            // 四个角的弧
            DrawArc(tex, l + r, tp + h - r - 1, r, 90, 180, color);
            DrawArc(tex, l + w - r - 1, tp + h - r - 1, r, 0, 90, color);
            DrawArc(tex, l + r, tp + r, r, 180, 270, color);
            DrawArc(tex, l + w - r - 1, tp + r, r, 270, 360, color);
        }
    }

    static void DrawArc(Texture2D tex, int cx, int cy, int radius, int startAngle, int endAngle, Color color)
    {
        for (int angle = startAngle; angle <= endAngle; angle++)
        {
            float rad = angle * Mathf.Deg2Rad;
            int x = cx + Mathf.RoundToInt(Mathf.Cos(rad) * radius);
            int y = cy + Mathf.RoundToInt(Mathf.Sin(rad) * radius);
            if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
            {
                tex.SetPixel(x, y, color);
            }
        }
    }

    static void DrawHorizontalLine(Texture2D tex, int x, int y, int length, Color color, int thickness)
    {
        for (int dx = 0; dx < length; dx++)
        {
            for (int dy = 0; dy < thickness; dy++)
            {
                if (x + dx < tex.width && y + dy < tex.height)
                {
                    tex.SetPixel(x + dx, y + dy, color);
                }
            }
        }
    }

    static void DrawCornerDecorations(Texture2D tex, int width, int height, Color color, int size)
    {
        // 四个角落添加装饰
        Color brightColor = Color.Lerp(color, Color.white, 0.3f);

        // 左下角
        for (int i = 0; i < size; i++)
        {
            tex.SetPixel(i, 0, brightColor);
            tex.SetPixel(0, i, brightColor);
        }

        // 右下角
        for (int i = 0; i < size; i++)
        {
            tex.SetPixel(width - 1 - i, 0, brightColor);
            tex.SetPixel(width - 1, i, brightColor);
        }

        // 左上角
        for (int i = 0; i < size; i++)
        {
            tex.SetPixel(i, height - 1, brightColor);
            tex.SetPixel(0, height - 1 - i, brightColor);
        }

        // 右上角
        for (int i = 0; i < size; i++)
        {
            tex.SetPixel(width - 1 - i, height - 1, brightColor);
            tex.SetPixel(width - 1, height - 1 - i, brightColor);
        }
    }

    static void AddVignette(Texture2D tex, float strength)
    {
        int cx = tex.width / 2;
        int cy = tex.height / 2;
        float maxDist = Mathf.Sqrt(cx * cx + cy * cy);

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float vignette = dist / maxDist;
                vignette = Mathf.Pow(vignette, 2) * strength;

                Color c = tex.GetPixel(x, y);
                c = Color.Lerp(c, Color.black, vignette);
                tex.SetPixel(x, y, c);
            }
        }
    }

    static void SaveTexture(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);

        // 确保在下次AssetDatabase.Refresh时更新
        Debug.Log($"[UIResourceGenerator] 保存: {path}");
    }

    #endregion
}
