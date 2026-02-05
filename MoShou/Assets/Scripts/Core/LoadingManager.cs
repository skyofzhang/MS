using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MoShou.Core
{
    /// <summary>
    /// Loading screen manager - handles scene transitions with loading UI
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject loadingCanvas;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Text tipText;
        [SerializeField] private Image loadingIcon;

        [Header("Loading Settings")]
        [SerializeField] private float minimumLoadTime = 0.5f;
        [SerializeField] private float iconRotationSpeed = 180f;

        [Header("Loading Tips")]
        [SerializeField] private string[] loadingTips = new string[]
        {
            "Tip: Equip better gear to increase your stats!",
            "Tip: Use skills wisely - they have cooldowns.",
            "Tip: Defeating enemies drops gold and items.",
            "Tip: Visit the shop to upgrade your equipment.",
            "Tip: Complete stages to unlock new areas.",
            "Tip: Critical hits deal 150% damage!",
            "Tip: Higher difficulty stages give better rewards."
        };

        private bool isLoading = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Hide loading UI initially
                if (loadingCanvas != null)
                    loadingCanvas.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Load a scene with loading screen
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (!isLoading)
            {
                StartCoroutine(LoadSceneAsync(sceneName));
            }
        }

        /// <summary>
        /// Load scene asynchronously with progress display
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;

            // Show loading UI
            if (loadingCanvas != null)
                loadingCanvas.SetActive(true);

            // Show random tip
            ShowRandomTip();

            // Reset progress
            if (progressBar != null)
                progressBar.value = 0;
            if (progressText != null)
                progressText.text = "0%";

            float startTime = Time.realtimeSinceStartup;

            // Start async load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Update progress
            while (!asyncLoad.isDone)
            {
                // Progress goes from 0 to 0.9 during loading
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                // Update UI
                if (progressBar != null)
                    progressBar.value = progress;
                if (progressText != null)
                    progressText.text = $"{(int)(progress * 100)}%";

                // Rotate loading icon
                if (loadingIcon != null)
                    loadingIcon.transform.Rotate(0, 0, -iconRotationSpeed * Time.deltaTime);

                // Check if loading is complete
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum load time for smooth transition
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime < minimumLoadTime)
                    {
                        yield return new WaitForSecondsRealtime(minimumLoadTime - elapsedTime);
                    }

                    // Set progress to 100%
                    if (progressBar != null)
                        progressBar.value = 1f;
                    if (progressText != null)
                        progressText.text = "100%";

                    yield return new WaitForSecondsRealtime(0.1f);

                    // Allow scene activation
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Hide loading UI
            if (loadingCanvas != null)
                loadingCanvas.SetActive(false);

            isLoading = false;
        }

        /// <summary>
        /// Show a random loading tip
        /// </summary>
        private void ShowRandomTip()
        {
            if (tipText == null || loadingTips == null || loadingTips.Length == 0)
                return;

            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }

        /// <summary>
        /// Load scene immediately (no loading screen)
        /// </summary>
        public void LoadSceneImmediate(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Check if currently loading
        /// </summary>
        public bool IsLoading => isLoading;
    }
}
