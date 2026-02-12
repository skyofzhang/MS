using UnityEngine;
using UnityEngine.UI;

namespace MoShou.UI
{
    /// <summary>
    /// 商品卡片Prefab控制器
    /// 所有UI引用通过SerializeField绑定，由Editor脚本或Prefab设置
    /// </summary>
    public class ShopItemCardUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image iconFrame;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Image coinIcon;
        [SerializeField] private Text priceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyButtonText;

        private ShopItemData itemData;

        /// <summary>
        /// 填充商品数据并绑定购买事件
        /// </summary>
        public void Setup(ShopItemData data, Sprite itemIcon, System.Action<ShopItemData> onPurchase)
        {
            itemData = data;

            // 名称和描述
            if (nameText != null) nameText.text = data.name;
            if (descriptionText != null) descriptionText.text = data.description;

            // 价格
            if (priceText != null) priceText.text = data.price.ToString();

            // 物品图标
            if (iconImage != null)
            {
                if (itemIcon != null)
                {
                    iconImage.sprite = itemIcon;
                    iconImage.preserveAspect = true;
                    iconImage.color = Color.white;
                }
                else
                {
                    // 无icon时半透明色块
                    iconImage.color = new Color(
                        GetCategoryColor().r,
                        GetCategoryColor().g,
                        GetCategoryColor().b,
                        0.4f
                    );
                }
            }

            // 购买按钮
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => onPurchase?.Invoke(data));
            }
        }

        private Color GetCategoryColor()
        {
            if (itemData == null) return Color.gray;
            switch (itemData.category)
            {
                case ShopCategory.Weapon: return new Color(0.8f, 0.4f, 0.2f);
                case ShopCategory.Armor: return new Color(0.3f, 0.5f, 0.8f);
                case ShopCategory.Helmet: return new Color(0.5f, 0.5f, 0.7f);
                case ShopCategory.Pants: return new Color(0.4f, 0.6f, 0.5f);
                case ShopCategory.Ring: return new Color(0.7f, 0.5f, 0.8f);
                case ShopCategory.Necklace: return new Color(0.8f, 0.6f, 0.4f);
                case ShopCategory.Consumable: return new Color(0.3f, 0.8f, 0.3f);
                default: return Color.gray;
            }
        }
    }
}
