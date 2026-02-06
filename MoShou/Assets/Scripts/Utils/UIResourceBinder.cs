using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MoShou.Utils
{
    /// <summary>
    /// UI资源绑定器 - 自动将美术资源应用到UI组件
    /// 解决问题：美术资源存在但UI组件没有使用
    /// 每次场景加载时重新绑定当前场景的UI（MainMenu/GameScene/StageSelect）
    /// </summary>
    public class UIResourceBinder : MonoBehaviour
    {
        public static UIResourceBinder Instance { get; private set; }

        // UI资源路径 (对应Resources/Sprites/UI/目录)
        private const string PATH_BUTTONS = "Sprites/UI/Buttons/";
        private const string PATH_HUD = "Sprites/UI/HUD/";
        private const string PATH_COMMON = "Sprites/UI/Common/";
        private const string PATH_SKILLS = "Sprites/UI/Skills/";
        private const string PATH_RESULT = "Sprites/UI/Result/";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            BindAllUIInScene();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindAllUIInScene();
        }

        /// <summary>
        /// 绑定场景中所有UI资源
        /// </summary>
        public void BindAllUIInScene()
        {
            Debug.Log("[UIResourceBinder] 开始绑定UI资源...");

            // 绑定所有按钮
            BindAllButtons();

            // 绑定所有Slider背景
            BindAllSliders();

            // 绑定所有Image背景
            BindAllBackgrounds();

            // 按名称绑定：暂停按钮、技能槽、金币图标等
            BindNamedImages();

            Debug.Log("[UIResourceBinder] UI资源绑定完成");
        }

        /// <summary>
        /// 按名称绑定特定UI元素（暂停按钮、技能槽、金币图标、波次/等级背景等）
        /// </summary>
        private void BindNamedImages()
        {
            var images = FindObjectsOfType<Image>(true);
            var pauseSprite = Resources.Load<Sprite>(PATH_BUTTONS + "UI_Btn_Pause");
            var closeSprite = Resources.Load<Sprite>(PATH_BUTTONS + "UI_Btn_Close");
            var goldIconSprite = Resources.Load<Sprite>(PATH_HUD + "UI_HUD_Gold_Icon");
            var levelBgSprite = Resources.Load<Sprite>(PATH_HUD + "UI_HUD_Level_BG");
            var waveBgSprite = Resources.Load<Sprite>(PATH_HUD + "UI_HUD_Wave_BG");
            var skillSlotBgSprite = Resources.Load<Sprite>(PATH_SKILLS + "UI_Skill_Slot_BG");
            var skillMultiShotSprite = Resources.Load<Sprite>(PATH_SKILLS + "UI_Skill_Icon_MultiShot");
            var skillPierceSprite = Resources.Load<Sprite>(PATH_SKILLS + "UI_Skill_Icon_Pierce");

            int count = 0;
            foreach (var img in images)
            {
                if (img.sprite != null) continue;
                string nameLower = img.name.ToLower();

                Sprite sprite = null;
                if (nameLower.Contains("pause") && pauseSprite != null)
                    sprite = pauseSprite;
                else if (nameLower.Contains("close") && closeSprite != null)
                    sprite = closeSprite;
                else if (nameLower.Contains("gold") && goldIconSprite != null)
                    sprite = goldIconSprite;
                else if ((nameLower.Contains("level") || nameLower.Contains("lv")) && nameLower.Contains("bg") && levelBgSprite != null)
                    sprite = levelBgSprite;
                else if (nameLower.Contains("wave") && waveBgSprite != null)
                    sprite = waveBgSprite;
                else if (nameLower.Contains("skill") && nameLower.Contains("slot") && skillSlotBgSprite != null)
                    sprite = skillSlotBgSprite;
                else if (nameLower.Contains("skill") && (nameLower.Contains("1") || nameLower.Contains("multishot")) && skillMultiShotSprite != null)
                    sprite = skillMultiShotSprite;
                else if (nameLower.Contains("skill") && (nameLower.Contains("2") || nameLower.Contains("pierce")) && skillPierceSprite != null)
                    sprite = skillPierceSprite;

                if (sprite != null)
                {
                    img.sprite = sprite;
                    img.type = Image.Type.Simple;
                    img.color = Color.white;
                    count++;
                }
            }
            if (count > 0)
                Debug.Log($"[UIResourceBinder] 按名称绑定了 {count} 个Image");
        }

        /// <summary>
        /// 绑定所有按钮图片
        /// </summary>
        private void BindAllButtons()
        {
            var buttons = FindObjectsOfType<Button>(true);
            var normalSprite = Resources.Load<Sprite>(PATH_BUTTONS + "UI_Btn_Primary_Normal");
            var pressedSprite = Resources.Load<Sprite>(PATH_BUTTONS + "UI_Btn_Primary_Pressed");
            var disabledSprite = Resources.Load<Sprite>(PATH_BUTTONS + "UI_Btn_Primary_Disabled");

            if (normalSprite == null)
            {
                Debug.LogWarning("[UIResourceBinder] 按钮图片未找到: UI_Btn_Primary_Normal");
                return;
            }

            int count = 0;
            foreach (var btn in buttons)
            {
                var image = btn.GetComponent<Image>();
                if (image != null && image.sprite == null)
                {
                    image.sprite = normalSprite;
                    image.type = Image.Type.Sliced;

                    // 配置按钮状态
                    var colors = btn.colors;
                    colors.normalColor = Color.white;
                    colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
                    colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
                    btn.colors = colors;

                    // 如果有SpriteState
                    var spriteState = btn.spriteState;
                    if (pressedSprite != null) spriteState.pressedSprite = pressedSprite;
                    if (disabledSprite != null) spriteState.disabledSprite = disabledSprite;
                    btn.spriteState = spriteState;

                    count++;
                }
            }
            Debug.Log($"[UIResourceBinder] 绑定了 {count} 个按钮");
        }

        /// <summary>
        /// 绑定所有Slider背景和填充
        /// </summary>
        private void BindAllSliders()
        {
            var sliders = FindObjectsOfType<Slider>(true);

            // 加载HP条资源
            var hpBgSprite = Resources.Load<Sprite>(PATH_HUD + "UI_HUD_HPBar_BG");
            var hpFillSprite = Resources.Load<Sprite>(PATH_HUD + "UI_HUD_HPBar_Fill");

            // 加载EXP条资源
            var expBgSprite = Resources.Load<Sprite>(PATH_HUD + "UI_HUD_EXPBar_BG");
            var expFillSprite = Resources.Load<Sprite>(PATH_HUD + "UI_HUD_EXPBar_Fill");

            int count = 0;
            foreach (var slider in sliders)
            {
                string sliderName = slider.name.ToLower();

                // 判断是HP条还是EXP条
                bool isHealth = sliderName.Contains("health") || sliderName.Contains("hp");
                bool isExp = sliderName.Contains("exp") || sliderName.Contains("experience");

                Sprite bgSprite = isHealth ? hpBgSprite : (isExp ? expBgSprite : hpBgSprite);
                Sprite fillSprite = isHealth ? hpFillSprite : (isExp ? expFillSprite : hpFillSprite);

                // 设置背景
                var bgImage = slider.GetComponent<Image>();
                if (bgImage != null && bgImage.sprite == null && bgSprite != null)
                {
                    bgImage.sprite = bgSprite;
                    bgImage.type = Image.Type.Sliced;
                }

                // 设置填充
                if (slider.fillRect != null && fillSprite != null)
                {
                    var fillImage = slider.fillRect.GetComponent<Image>();
                    if (fillImage != null && fillImage.sprite == null)
                    {
                        fillImage.sprite = fillSprite;
                        fillImage.type = Image.Type.Sliced;

                        // HP条用绿色，EXP条用蓝色
                        fillImage.color = isHealth ? new Color(0.2f, 0.8f, 0.2f) :
                                         isExp ? new Color(0.3f, 0.6f, 1f) : Color.white;
                    }
                }
                count++;
            }
            Debug.Log($"[UIResourceBinder] 绑定了 {count} 个滑动条");
        }

        /// <summary>
        /// 绑定所有Panel/Dialog背景
        /// </summary>
        private void BindAllBackgrounds()
        {
            var dialogBg = Resources.Load<Sprite>(PATH_COMMON + "UI_Common_Dialog_BG");

            if (dialogBg == null)
            {
                Debug.LogWarning("[UIResourceBinder] 对话框背景未找到");
                return;
            }

            // 查找所有Panel
            var panels = FindObjectsOfType<RectTransform>(true);
            int count = 0;

            foreach (var panel in panels)
            {
                string panelName = panel.name.ToLower();

                // 只处理明确是Panel/Dialog/Window的对象
                if (panelName.Contains("panel") || panelName.Contains("dialog") ||
                    panelName.Contains("window") || panelName.Contains("popup"))
                {
                    var image = panel.GetComponent<Image>();
                    if (image != null && image.sprite == null)
                    {
                        image.sprite = dialogBg;
                        image.type = Image.Type.Sliced;
                        image.color = new Color(1, 1, 1, 0.95f);
                        count++;
                    }
                }
            }
            Debug.Log($"[UIResourceBinder] 绑定了 {count} 个面板背景");
        }

        /// <summary>
        /// 加载技能图标
        /// </summary>
        public Sprite LoadSkillIcon(string skillName)
        {
            string path = PATH_SKILLS + "UI_Skill_Icon_" + skillName;
            return Resources.Load<Sprite>(path);
        }

        /// <summary>
        /// 加载HUD元素
        /// </summary>
        public Sprite LoadHUDElement(string elementName)
        {
            return Resources.Load<Sprite>(PATH_HUD + elementName);
        }

        /// <summary>
        /// 加载按钮图片
        /// </summary>
        public Sprite LoadButtonSprite(string buttonName)
        {
            return Resources.Load<Sprite>(PATH_BUTTONS + buttonName);
        }
    }
}
