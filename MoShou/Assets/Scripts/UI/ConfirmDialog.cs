using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace MoShou.UI
{
    /// <summary>
    /// 通用确认弹窗系统
    /// 实现策划案UI-10~UI-14的确认弹窗功能
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        private static ConfirmDialog _instance;
        public static ConfirmDialog Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 尝试查找
                    _instance = FindObjectOfType<ConfirmDialog>();
                    if (_instance == null)
                    {
                        // 创建新实例
                        CreateInstance();
                    }
                }
                return _instance;
            }
        }

        [Header("UI引用")]
        private GameObject dialogContainer;
        private Image backgroundOverlay;
        private Image dialogPanel;
        private Text titleText;
        private Text messageText;
        private Button confirmButton;
        private Button cancelButton;
        private Text confirmButtonText;
        private Text cancelButtonText;

        // 回调
        private Action onConfirm;
        private Action onCancel;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                CreateDialogUI();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 创建单例实例
        /// </summary>
        static void CreateInstance()
        {
            GameObject go = new GameObject("ConfirmDialog");
            _instance = go.AddComponent<ConfirmDialog>();
            DontDestroyOnLoad(go);
        }

        /// <summary>
        /// 创建弹窗UI
        /// </summary>
        void CreateDialogUI()
        {
            // 创建Canvas
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 确保在最上层

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // 背景容器
            dialogContainer = new GameObject("DialogContainer");
            dialogContainer.transform.SetParent(transform, false);
            RectTransform containerRect = dialogContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // 半透明遮罩
            GameObject overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(dialogContainer.transform, false);
            RectTransform overlayRect = overlayGO.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            backgroundOverlay = overlayGO.AddComponent<Image>();
            backgroundOverlay.color = new Color(0, 0, 0, 0.7f);

            // 点击遮罩关闭（可选）
            Button overlayBtn = overlayGO.AddComponent<Button>();
            overlayBtn.onClick.AddListener(OnCancelClick);

            // 弹窗面板
            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(dialogContainer.transform, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 350);

            dialogPanel = panelGO.AddComponent<Image>();
            dialogPanel.color = new Color(0.15f, 0.15f, 0.2f, 0.98f);

            // 面板边框
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(panelGO.transform, false);
            RectTransform borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-3, -3);
            borderRect.offsetMax = new Vector2(3, 3);
            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.7f, 0.55f, 0.25f, 0.9f);
            borderImg.raycastTarget = false;
            borderGO.transform.SetAsFirstSibling();

            // 标题
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform, false);
            RectTransform titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(-40, 50);

            titleText = titleGO.AddComponent<Text>();
            titleText.text = "确认";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.9f, 0.7f);
            titleText.font = GetDefaultFont();

            Outline titleOutline = titleGO.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0, 0, 0, 0.5f);
            titleOutline.effectDistance = new Vector2(1, -1);

            // 消息内容
            GameObject msgGO = new GameObject("Message");
            msgGO.transform.SetParent(panelGO.transform, false);
            RectTransform msgRect = msgGO.AddComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0, 0.35f);
            msgRect.anchorMax = new Vector2(1, 0.8f);
            msgRect.offsetMin = new Vector2(30, 0);
            msgRect.offsetMax = new Vector2(-30, 0);

            messageText = msgGO.AddComponent<Text>();
            messageText.text = "确定要执行此操作吗？";
            messageText.fontSize = 26;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.color = Color.white;
            messageText.font = GetDefaultFont();

            // 按钮容器
            GameObject buttonsGO = new GameObject("Buttons");
            buttonsGO.transform.SetParent(panelGO.transform, false);
            RectTransform buttonsRect = buttonsGO.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0, 0);
            buttonsRect.anchorMax = new Vector2(1, 0.3f);
            buttonsRect.offsetMin = new Vector2(30, 20);
            buttonsRect.offsetMax = new Vector2(-30, -10);

            HorizontalLayoutGroup hlg = buttonsGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 40;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 取消按钮
            cancelButton = CreateButton(buttonsGO.transform, "CancelButton", "取消",
                new Color(0.4f, 0.4f, 0.45f), OnCancelClick);

            // 确认按钮
            confirmButton = CreateButton(buttonsGO.transform, "ConfirmButton", "确认",
                new Color(0.3f, 0.6f, 0.3f), OnConfirmClick);

            // 获取按钮文本引用
            cancelButtonText = cancelButton.GetComponentInChildren<Text>();
            confirmButtonText = confirmButton.GetComponentInChildren<Text>();

            // 默认隐藏
            dialogContainer.SetActive(false);
        }

        Button CreateButton(Transform parent, string name, string text, Color color, UnityAction onClick)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);

            var layout = btnGO.AddComponent<LayoutElement>();
            layout.preferredWidth = 200;
            layout.preferredHeight = 60;

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = color;

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);

            // 按钮颜色过渡
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;

            // 边框
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(btnGO.transform, false);
            RectTransform borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2, -2);
            borderRect.offsetMax = new Vector2(2, 2);
            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.8f, 0.7f, 0.5f, 0.5f);
            borderImg.raycastTarget = false;
            borderGO.transform.SetAsFirstSibling();

            // 文本
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
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
            btnText.font = GetDefaultFont();

            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            return btn;
        }

        void OnConfirmClick()
        {
            Hide();
            onConfirm?.Invoke();
        }

        void OnCancelClick()
        {
            Hide();
            onCancel?.Invoke();
        }

        /// <summary>
        /// 显示确认弹窗
        /// </summary>
        public void Show(string title, string message, Action confirmCallback = null, Action cancelCallback = null,
            string confirmText = "确认", string cancelText = "取消")
        {
            if (dialogContainer == null)
            {
                CreateDialogUI();
            }

            titleText.text = title;
            messageText.text = message;
            onConfirm = confirmCallback;
            onCancel = cancelCallback;

            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;
            if (cancelButtonText != null)
                cancelButtonText.text = cancelText;

            dialogContainer.SetActive(true);

            // 暂停游戏时间（但UI动画继续）
            // Time.timeScale = 0f; // 注释掉，让调用者决定是否暂停
        }

        /// <summary>
        /// 显示简单确认弹窗
        /// </summary>
        public void ShowConfirm(string message, Action onConfirm)
        {
            Show("确认", message, onConfirm, null);
        }

        /// <summary>
        /// 显示警告弹窗
        /// </summary>
        public void ShowWarning(string message, Action onConfirm)
        {
            Show("警告", message, onConfirm, null, "继续", "取消");

            // 设置警告颜色
            if (titleText != null)
                titleText.color = new Color(1f, 0.7f, 0.3f);
        }

        /// <summary>
        /// 显示删除确认弹窗
        /// </summary>
        public void ShowDeleteConfirm(string itemName, Action onConfirm)
        {
            Show("删除确认", $"确定要删除 \"{itemName}\" 吗？\n此操作无法撤销！",
                onConfirm, null, "删除", "取消");

            // 设置删除颜色
            if (confirmButton != null)
            {
                var img = confirmButton.GetComponent<Image>();
                if (img != null) img.color = new Color(0.7f, 0.2f, 0.2f);
            }
        }

        /// <summary>
        /// 显示出售确认弹窗
        /// </summary>
        public void ShowSellConfirm(string itemName, int price, Action onConfirm)
        {
            Show("出售确认", $"确定要出售 \"{itemName}\" 吗？\n将获得 {price} 金币",
                onConfirm, null, "出售", "取消");
        }

        /// <summary>
        /// 显示购买确认弹窗
        /// </summary>
        public void ShowBuyConfirm(string itemName, int price, Action onConfirm)
        {
            Show("购买确认", $"确定要购买 \"{itemName}\" 吗？\n需要花费 {price} 金币",
                onConfirm, null, "购买", "取消");
        }

        /// <summary>
        /// 显示关卡开始确认弹窗
        /// </summary>
        public void ShowStageConfirm(int stageNum, string stageName, int recommendLevel, Action onConfirm)
        {
            Show("开始挑战",
                $"关卡: {stageName}\n推荐等级: Lv.{recommendLevel}\n\n确定要挑战此关卡吗？",
                onConfirm, null, "开始挑战", "返回");
        }

        /// <summary>
        /// 显示退出确认弹窗
        /// </summary>
        public void ShowExitConfirm(Action onConfirm)
        {
            Show("退出确认", "确定要退出游戏吗？\n当前进度将自动保存。",
                onConfirm, null, "退出", "继续游戏");
        }

        /// <summary>
        /// 隐藏弹窗
        /// </summary>
        public void Hide()
        {
            if (dialogContainer != null)
            {
                dialogContainer.SetActive(false);
            }

            // 恢复游戏时间
            // Time.timeScale = 1f;
        }

        /// <summary>
        /// 是否正在显示
        /// </summary>
        public bool IsShowing => dialogContainer != null && dialogContainer.activeSelf;

        Font GetDefaultFont()
        {
            string[] fontNames = { "LegacyRuntime.ttf", "Arial.ttf", "Liberation Sans" };
            foreach (string fontName in fontNames)
            {
                Font font = Resources.GetBuiltinResource<Font>(fontName);
                if (font != null) return font;
            }
            return Font.CreateDynamicFontFromOSFont("Arial", 14);
        }
    }
}
