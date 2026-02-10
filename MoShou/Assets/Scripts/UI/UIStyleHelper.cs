using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MoShou.UI
{
    /// <summary>
    /// UI样式辅助工具 - 提供统一的UI创建方法
    /// 基于策划案UI风格规范
    /// </summary>
    public static class UIStyleHelper
    {
        // ========== 颜色常量 (基于Notion UI策划案) ==========
        public static class Colors
        {
            // 主题色 (Notion文档定义) - 直接使用RGB值避免静态初始化顺序问题
            public static readonly Color Primary = new Color(1f, 0.843f, 0f);           // #FFD700 金色
            public static readonly Color Secondary = new Color(0.298f, 0.686f, 0.314f); // #4CAF50 绿色
            public static readonly Color Danger = new Color(1f, 0.267f, 0.267f);        // #FF4444 红色
            public static readonly Color Info = new Color(0f, 0.749f, 1f);              // #00BFFF 蓝色

            // 主要颜色 (兼容旧代码)
            public static readonly Color Gold = new Color(1f, 0.85f, 0.2f);
            public static readonly Color GoldDark = new Color(0.85f, 0.65f, 0.15f);
            public static readonly Color Bronze = new Color(0.8f, 0.5f, 0.2f);

            // 品质颜色
            public static readonly Color Common = new Color(0.8f, 0.8f, 0.8f);
            public static readonly Color Uncommon = new Color(0.3f, 0.8f, 0.3f);
            public static readonly Color Rare = new Color(0.3f, 0.5f, 1f);
            public static readonly Color Epic = new Color(0.6f, 0.3f, 0.9f);
            public static readonly Color Legendary = new Color(1f, 0.5f, 0f);

            // 状态颜色
            public static readonly Color HealthGreen = new Color(0.3f, 0.8f, 0.3f);
            public static readonly Color HealthYellow = new Color(0.9f, 0.8f, 0.2f);
            public static readonly Color HealthRed = new Color(0.9f, 0.2f, 0.2f);
            public static readonly Color ManaBlue = new Color(0.3f, 0.5f, 0.9f);

            // UI背景
            public static readonly Color PanelBg = new Color(0.1f, 0.12f, 0.15f, 0.9f);
            public static readonly Color PanelBgDark = new Color(0.05f, 0.07f, 0.1f, 0.95f);
            public static readonly Color BorderGold = new Color(0.7f, 0.55f, 0.25f, 0.9f);

            // 按钮颜色
            public static readonly Color BtnPrimary = new Color(1f, 0.5f, 0.2f);
            public static readonly Color BtnPrimaryDark = new Color(0.9f, 0.35f, 0.1f);
            public static readonly Color BtnSecondary = new Color(0.4f, 0.45f, 0.55f);
            public static readonly Color BtnSecondaryDark = new Color(0.3f, 0.35f, 0.4f);
            public static readonly Color BtnDisabled = new Color(0.4f, 0.4f, 0.4f, 0.7f);

            // 辅助方法：Hex颜色转换
            public static Color HexToColor(string hex)
            {
                if (ColorUtility.TryParseHtmlString(hex, out Color color))
                    return color;
                return Color.white;
            }
        }

        // ========== 布局常量 (基于Notion UI策划案) ==========
        public static class Layout
        {
            // 参考分辨率
            public const float RefWidth = 1080f;
            public const float RefHeight = 1920f;

            // 基础单位
            public const float BaseUnit = 8f;

            // 页边距
            public const float PageMargin = 40f;

            // 按钮尺寸
            public const float BtnHeightLarge = 80f;
            public const float BtnHeightMedium = 64f;
            public const float BtnHeightSmall = 48f;
            public const float BtnWidthFull = 1000f;  // RefWidth - 2*PageMargin
            public const float BtnWidthHalf = 480f;

            // 间距
            public const float SpacingSmall = 8f;
            public const float SpacingMedium = 16f;
            public const float SpacingLarge = 24f;

            // 圆角
            public const float BorderRadiusSmall = 8f;
            public const float BorderRadiusMedium = 12f;
            public const float BorderRadiusLarge = 16f;
        }

        // ========== 字体获取 ==========
        public static Font GetDefaultFont()
        {
            string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
            foreach (string fontName in fontNames)
            {
                Font font = Resources.GetBuiltinResource<Font>(fontName);
                if (font != null) return font;
            }
            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }

        // ========== 面板创建 ==========

        /// <summary>
        /// 创建带边框的面板
        /// </summary>
        public static GameObject CreatePanel(Transform parent, string name, Vector2 size,
            Color bgColor, Color borderColor, float borderWidth = 3f)
        {
            GameObject panelGO = new GameObject(name);
            panelGO.transform.SetParent(parent, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = size;

            // 背景
            Image bgImg = panelGO.AddComponent<Image>();
            bgImg.color = bgColor;

            // 边框
            if (borderWidth > 0)
            {
                GameObject borderGO = new GameObject("Border");
                borderGO.transform.SetParent(panelGO.transform, false);
                RectTransform borderRect = borderGO.AddComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = new Vector2(-borderWidth, -borderWidth);
                borderRect.offsetMax = new Vector2(borderWidth, borderWidth);

                Image borderImg = borderGO.AddComponent<Image>();
                borderImg.color = borderColor;
                borderImg.raycastTarget = false;
                borderGO.transform.SetAsFirstSibling();
            }

            return panelGO;
        }

        /// <summary>
        /// 创建半透明毛玻璃效果面板
        /// </summary>
        public static GameObject CreateGlassPanel(Transform parent, string name, Vector2 size)
        {
            GameObject panelGO = CreatePanel(parent, name, size,
                Colors.PanelBg, Colors.BorderGold, 3f);

            // 添加内层
            GameObject innerGO = new GameObject("Inner");
            innerGO.transform.SetParent(panelGO.transform, false);
            RectTransform innerRect = innerGO.AddComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(5, 5);
            innerRect.offsetMax = new Vector2(-5, -5);

            Image innerImg = innerGO.AddComponent<Image>();
            innerImg.color = Colors.PanelBgDark;
            innerImg.raycastTarget = false;

            return panelGO;
        }

        // ========== 按钮创建 ==========

        /// <summary>
        /// 创建主要按钮（橙红渐变）
        /// </summary>
        public static Button CreatePrimaryButton(Transform parent, string name, string text,
            Vector2 size, UnityAction onClick)
        {
            return CreateStyledButton(parent, name, text, size,
                Colors.BtnPrimary, Colors.BtnPrimaryDark, onClick);
        }

        /// <summary>
        /// 创建次要按钮（灰色）
        /// </summary>
        public static Button CreateSecondaryButton(Transform parent, string name, string text,
            Vector2 size, UnityAction onClick)
        {
            return CreateStyledButton(parent, name, text, size,
                Colors.BtnSecondary, Colors.BtnSecondaryDark, onClick);
        }

        /// <summary>
        /// 创建自定义颜色按钮
        /// </summary>
        public static Button CreateStyledButton(Transform parent, string name, string text,
            Vector2 size, Color colorTop, Color colorBottom, UnityAction onClick,
            Sprite bgSprite = null)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            RectTransform btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = size;

            // 按钮背景
            Image btnImg = btnGO.AddComponent<Image>();
            if (bgSprite != null)
            {
                btnImg.sprite = bgSprite;
                btnImg.type = Image.Type.Sliced;
                btnImg.color = Color.white;
            }
            else
            {
                btnImg.color = colorTop;

                // 渐变层
                GameObject gradientGO = new GameObject("Gradient");
                gradientGO.transform.SetParent(btnGO.transform, false);
                RectTransform gradRect = gradientGO.AddComponent<RectTransform>();
                gradRect.anchorMin = new Vector2(0, 0);
                gradRect.anchorMax = new Vector2(1, 0.5f);
                gradRect.offsetMin = Vector2.zero;
                gradRect.offsetMax = Vector2.zero;

                Image gradImg = gradientGO.AddComponent<Image>();
                gradImg.color = new Color(colorBottom.r, colorBottom.g, colorBottom.b, 0.6f);
                gradImg.raycastTarget = false;
            }

            // 边框
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(btnGO.transform, false);
            RectTransform borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2, -2);
            borderRect.offsetMax = new Vector2(2, 2);

            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.8f, 0.7f, 0.5f, 0.6f);
            borderImg.raycastTarget = false;
            borderGO.transform.SetAsFirstSibling();

            // 高光
            GameObject hlGO = new GameObject("Highlight");
            hlGO.transform.SetParent(btnGO.transform, false);
            RectTransform hlRect = hlGO.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0, 0.6f);
            hlRect.anchorMax = new Vector2(1, 1);
            hlRect.offsetMin = new Vector2(5, 0);
            hlRect.offsetMax = new Vector2(-5, -3);

            Image hlImg = hlGO.AddComponent<Image>();
            hlImg.color = new Color(1, 1, 1, 0.15f);
            hlImg.raycastTarget = false;

            // Button组件
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            if (onClick != null)
                btn.onClick.AddListener(onClick);

            // 颜色过渡
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
            btn.colors = colors;

            // 文字
            CreateButtonText(btnGO.transform, text, (int)(size.y * 0.4f));

            return btn;
        }

        /// <summary>
        /// 创建按钮文字
        /// </summary>
        static void CreateButtonText(Transform parent, string text, int fontSize)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(parent, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            Text btnText = textGO.AddComponent<Text>();
            btnText.text = text;
            btnText.fontSize = fontSize;
            btnText.fontStyle = FontStyle.Bold;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.font = GetDefaultFont();

            // 描边
            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.1f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // 阴影
            Shadow shadow = textGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
        }

        // ========== 文字创建 ==========

        /// <summary>
        /// 创建标题文字（带描边和阴影）
        /// </summary>
        public static Text CreateTitleText(Transform parent, string name, string text,
            int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.fontStyle = FontStyle.Bold;
            textComp.alignment = alignment;
            textComp.color = color;
            textComp.font = GetDefaultFont();

            // 描边
            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.7f);
            outline.effectDistance = new Vector2(2, -2);

            // 阴影
            Shadow shadow = textGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);

            return textComp;
        }

        /// <summary>
        /// 创建普通文字
        /// </summary>
        public static Text CreateText(Transform parent, string name, string text,
            int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.alignment = alignment;
            textComp.color = color;
            textComp.font = GetDefaultFont();

            return textComp;
        }

        // ========== 进度条创建 ==========

        /// <summary>
        /// 创建进度条
        /// </summary>
        public static Slider CreateProgressBar(Transform parent, string name, Vector2 size,
            Color bgColor, Color fillColor, Color borderColor)
        {
            GameObject barGO = new GameObject(name);
            barGO.transform.SetParent(parent, false);
            RectTransform barRect = barGO.AddComponent<RectTransform>();
            barRect.sizeDelta = size;

            // 背景
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(barGO.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = bgColor;

            // 填充区域
            GameObject fillAreaGO = new GameObject("FillArea");
            fillAreaGO.transform.SetParent(barGO.transform, false);
            RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(3, 3);
            fillAreaRect.offsetMax = new Vector2(-3, -3);

            // 填充
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            RectTransform fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;

            // 边框
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(barGO.transform, false);
            RectTransform borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2, -2);
            borderRect.offsetMax = new Vector2(2, 2);

            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = borderColor;
            borderImg.raycastTarget = false;
            borderGO.transform.SetAsFirstSibling();

            // Slider组件
            Slider slider = barGO.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.interactable = false;

            return slider;
        }

        /// <summary>
        /// 创建血条
        /// </summary>
        public static Slider CreateHealthBar(Transform parent, string name, Vector2 size)
        {
            return CreateProgressBar(parent, name, size,
                new Color(0.2f, 0.1f, 0.1f, 0.9f),
                Colors.HealthGreen,
                new Color(0.6f, 0.5f, 0.3f, 0.8f));
        }

        /// <summary>
        /// 创建经验条
        /// </summary>
        public static Slider CreateExpBar(Transform parent, string name, Vector2 size)
        {
            return CreateProgressBar(parent, name, size,
                new Color(0.1f, 0.1f, 0.2f, 0.9f),
                Colors.ManaBlue,
                new Color(0.4f, 0.45f, 0.6f, 0.8f));
        }

        // ========== 图标创建 ==========

        /// <summary>
        /// 创建带边框的图标
        /// </summary>
        public static Image CreateIconWithBorder(Transform parent, string name, Vector2 size,
            Sprite icon, Color borderColor, float borderWidth = 3f)
        {
            GameObject containerGO = new GameObject(name);
            containerGO.transform.SetParent(parent, false);
            RectTransform containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.sizeDelta = size;

            // 边框
            Image borderImg = containerGO.AddComponent<Image>();
            borderImg.color = borderColor;

            // 图标
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(containerGO.transform, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(borderWidth, borderWidth);
            iconRect.offsetMax = new Vector2(-borderWidth, -borderWidth);

            Image iconImg = iconGO.AddComponent<Image>();
            if (icon != null)
                iconImg.sprite = icon;
            iconImg.color = Color.white;

            return iconImg;
        }

        /// <summary>
        /// 根据品质获取边框颜色
        /// </summary>
        public static Color GetQualityColor(int quality)
        {
            switch (quality)
            {
                case 0: return Colors.Common;
                case 1: return Colors.Uncommon;
                case 2: return Colors.Rare;
                case 3: return Colors.Epic;
                case 4: return Colors.Legendary;
                default: return Colors.Common;
            }
        }

        // ========== 星级评分 ==========

        /// <summary>
        /// 创建星级评分显示
        /// </summary>
        public static GameObject CreateStarRating(Transform parent, string name, int stars, int maxStars,
            Vector2 starSize, Sprite filledStar = null, Sprite emptyStar = null)
        {
            GameObject starsGO = new GameObject(name);
            starsGO.transform.SetParent(parent, false);

            var hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            for (int i = 0; i < maxStars; i++)
            {
                GameObject starGO = new GameObject($"Star_{i}");
                starGO.transform.SetParent(starsGO.transform, false);

                var layout = starGO.AddComponent<LayoutElement>();
                layout.preferredWidth = starSize.x;
                layout.preferredHeight = starSize.y;

                Image starImg = starGO.AddComponent<Image>();
                bool isFilled = i < stars;

                if (isFilled && filledStar != null)
                {
                    starImg.sprite = filledStar;
                    starImg.color = Color.white;
                }
                else if (!isFilled && emptyStar != null)
                {
                    starImg.sprite = emptyStar;
                    starImg.color = Color.white;
                }
                else
                {
                    starImg.color = isFilled ? Colors.Gold : new Color(0.3f, 0.3f, 0.3f, 0.6f);
                }
            }

            return starsGO;
        }
    }
}
