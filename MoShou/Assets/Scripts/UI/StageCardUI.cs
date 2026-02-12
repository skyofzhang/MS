using UnityEngine;
using UnityEngine.UI;

namespace MoShou.UI
{
    /// <summary>
    /// 关卡卡片UI控制器 - 挂载到StageCard Prefab上
    /// 所有子元素引用通过Inspector拖入
    /// </summary>
    public class StageCardUI : MonoBehaviour
    {
        [Header("卡片背景")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Button cardButton;

        [Header("左侧缩略图")]
        [SerializeField] private Image thumbnail;

        [Header("文字")]
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text stageInfoText;

        [Header("星级（已通关显示）")]
        [SerializeField] private GameObject starsRoot;
        [SerializeField] private Image[] starImages; // 3个

        [Header("激活按钮（未通关已解锁显示）")]
        [SerializeField] private GameObject goButtonRoot;
        [SerializeField] private Button goButton;

        [Header("锁定图标（未解锁显示）")]
        [SerializeField] private GameObject lockRoot;

        // 卡片状态颜色 - 对应9-slice金色帧的tint
        private static readonly Color ColorCurrent = Color.white;
        private static readonly Color ColorCleared = new Color(0.85f, 0.85f, 0.8f);
        private static readonly Color ColorUnlocked = new Color(0.9f, 0.9f, 0.85f);
        private static readonly Color ColorLocked = new Color(0.5f, 0.5f, 0.55f);

        // 缩略图灰色（锁定状态）
        private static readonly Color ThumbLockedTint = new Color(0.3f, 0.3f, 0.3f, 0.7f);

        /// <summary>
        /// 初始化卡片数据和状态
        /// </summary>
        /// <param name="stageNum">关卡编号</param>
        /// <param name="displayName">显示名称</param>
        /// <param name="infoLine">第二行信息文字</param>
        /// <param name="isLocked">是否锁定</param>
        /// <param name="isCleared">是否已通关</param>
        /// <param name="isCurrent">是否当前关卡</param>
        /// <param name="starCount">已获星数(0-3)</param>
        /// <param name="thumbSprite">区域缩略图(可为null)</param>
        /// <param name="starFilled">填充星sprite(可为null)</param>
        /// <param name="starEmpty">空星sprite(可为null)</param>
        /// <param name="onClick">点击回调</param>
        public void Setup(int stageNum, string displayName, string infoLine,
            bool isLocked, bool isCleared, bool isCurrent,
            int starCount, Sprite thumbSprite,
            Sprite starFilled, Sprite starEmpty,
            System.Action onClick)
        {
            // 卡片名
            gameObject.name = $"StageCard_{stageNum}";

            // === 背景颜色（状态区分）===
            if (cardBackground != null)
            {
                if (isCurrent)
                    cardBackground.color = ColorCurrent;
                else if (isCleared)
                    cardBackground.color = ColorCleared;
                else if (!isLocked)
                    cardBackground.color = ColorUnlocked;
                else
                    cardBackground.color = ColorLocked;
            }

            // === 按钮交互 ===
            if (cardButton != null)
            {
                cardButton.interactable = !isLocked;
                cardButton.onClick.RemoveAllListeners();
                if (!isLocked && onClick != null)
                {
                    cardButton.onClick.AddListener(() => onClick());
                }
            }

            // === 缩略图 ===
            if (thumbnail != null)
            {
                if (thumbSprite != null)
                {
                    thumbnail.sprite = thumbSprite;
                    thumbnail.color = isLocked ? ThumbLockedTint : Color.white;
                }
                else
                {
                    thumbnail.sprite = null;
                    thumbnail.color = isLocked
                        ? new Color(0.2f, 0.2f, 0.22f, 0.6f)
                        : new Color(0.3f, 0.4f, 0.3f, 0.8f);
                }
            }

            // === 关卡名 ===
            if (stageNameText != null)
            {
                stageNameText.text = $"关卡 {stageNum}: {displayName}";
                if (isLocked)
                    stageNameText.color = new Color(0.45f, 0.45f, 0.48f);
                else if (isCurrent)
                    stageNameText.color = UIStyleHelper.Colors.Gold;
                else
                    stageNameText.color = new Color(0.95f, 0.9f, 0.8f);
            }

            // === 信息行 ===
            if (stageInfoText != null)
            {
                stageInfoText.text = infoLine;
                stageInfoText.color = isLocked
                    ? new Color(0.4f, 0.4f, 0.42f)
                    : new Color(0.6f, 0.58f, 0.52f);
            }

            // === 星级（仅已通关显示）===
            if (starsRoot != null)
            {
                starsRoot.SetActive(isCleared);
                if (isCleared && starImages != null)
                {
                    int clampedStars = Mathf.Clamp(starCount, 0, 3);
                    for (int i = 0; i < starImages.Length; i++)
                    {
                        if (starImages[i] == null) continue;
                        if (starFilled != null)
                        {
                            starImages[i].sprite = (i < clampedStars) ? starFilled : starEmpty;
                            starImages[i].color = (i < clampedStars)
                                ? new Color(1f, 1f, 0.9f)
                                : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                        }
                        else
                        {
                            starImages[i].color = (i < clampedStars)
                                ? new Color(1f, 0.9f, 0.4f)
                                : new Color(0.4f, 0.4f, 0.4f, 0.5f);
                        }
                    }
                }
            }

            // === 激活按钮（未通关已解锁）===
            if (goButtonRoot != null)
            {
                goButtonRoot.SetActive(!isLocked && !isCleared);
                if (!isLocked && !isCleared && goButton != null)
                {
                    goButton.onClick.RemoveAllListeners();
                    if (onClick != null)
                    {
                        goButton.onClick.AddListener(() => onClick());
                    }
                }
            }

            // === 锁定图标 ===
            if (lockRoot != null)
            {
                lockRoot.SetActive(isLocked);
            }
        }
    }
}
