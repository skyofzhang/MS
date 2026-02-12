using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单血条控制器 - 使用Image.fillAmount
/// </summary>
public class SimpleHealthBar : MonoBehaviour
{
    public Image fillImage;

    public void SetValue(float normalized)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(normalized);
        }
    }
}
