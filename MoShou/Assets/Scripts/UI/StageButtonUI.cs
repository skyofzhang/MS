using UnityEngine;
using UnityEngine.UI;
using MoShou.Core;

namespace MoShou.UI
{
    /// <summary>
    /// Stage button UI component
    /// </summary>
    public class StageButtonUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] public Button button;
        [SerializeField] public Text stageNumberText;
        [SerializeField] public Image lockIcon;
        [SerializeField] public Image[] starIcons;
        [SerializeField] public Image clearMark;

        private StageData stageData;
        private System.Action<StageData> onClickCallback;

        /// <summary>
        /// Initialize the stage button
        /// </summary>
        public void Initialize(StageData data, bool isUnlocked, bool isCleared, System.Action<StageData> onClick)
        {
            stageData = data;
            onClickCallback = onClick;

            // Get components if not assigned
            if (button == null)
                button = GetComponent<Button>();

            // Set up button
            if (button != null)
            {
                button.interactable = isUnlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }

            // Update visuals
            if (stageNumberText != null)
                stageNumberText.text = data.stageId.ToString();

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!isUnlocked);

            if (clearMark != null)
                clearMark.gameObject.SetActive(isCleared);

            // Update star display (difficulty)
            UpdateStars(data.difficulty);
        }

        /// <summary>
        /// Update star icons based on difficulty
        /// </summary>
        private void UpdateStars(int difficulty)
        {
            if (starIcons == null) return;

            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].gameObject.SetActive(i < difficulty);
            }
        }

        /// <summary>
        /// Button click handler
        /// </summary>
        private void OnClick()
        {
            onClickCallback?.Invoke(stageData);
        }
    }
}
