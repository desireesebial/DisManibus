using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace World.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        private const float SceneProgressCompleteThreshold = 0.9f;

        [Header("Visuals")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text statusText;
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private bool blockInputWhileVisible = true;
        [SerializeField] private bool hideOnStart = true;

        [Header("Behaviour")]
        [SerializeField] private float minimumDisplayTime = 0.35f;

        private static LoadingScreenController instance;
        private Coroutine fadeRoutine;
        private bool isVisible;
        private bool isLoading;
        private float cachedTimeScale = 1f;

        public static LoadingScreenController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<LoadingScreenController>();
                }
                return instance;
            }
        }

        public bool IsVisible => isVisible;
        public bool IsLoading => isLoading;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>();
            }

            if (hideOnStart)
            {
                SetVisible(false, true);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void Show(string statusMessage = null, bool instant = false)
        {
            if (statusMessage != null)
            {
                SetStatus(statusMessage);
            }

            if (isVisible && !instant)
            {
                return;
            }

            cachedTimeScale = Time.timeScale;
            Time.timeScale = 1f; // ensure loading logic continues regardless of pause

            SetVisible(true, instant);
            UpdateProgress(0f);
        }

        public void Hide(bool instant = false)
        {
            if (!isVisible && !instant)
            {
                return;
            }

            SetVisible(false, instant);

            Time.timeScale = cachedTimeScale;
        }

        public void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void UpdateProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(true);
                progressSlider.normalizedValue = Mathf.Clamp01(progress);
            }
        }

        public IEnumerator TrackAsyncOperation(AsyncOperation operation, string statusMessage = null)
        {
            if (operation == null)
            {
                yield break;
            }

            isLoading = true;
            Show(statusMessage);

            float elapsedRealtime = 0f;
            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                elapsedRealtime += Time.unscaledDeltaTime;
                float normalizedProgress = Mathf.Clamp01(operation.progress / SceneProgressCompleteThreshold);
                UpdateProgress(normalizedProgress);
                yield return null;
            }

            elapsedRealtime += Time.unscaledDeltaTime;
            UpdateProgress(1f);

            if (elapsedRealtime < minimumDisplayTime)
            {
                yield return new WaitForSecondsRealtime(minimumDisplayTime - elapsedRealtime);
            }

            Hide();
            isLoading = false;
        }

        private void SetVisible(bool visible, bool instant)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            if (instant || fadeDuration <= 0f)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                ApplyCanvasInteractableState(visible);
            }
            else
            {
                fadeRoutine = StartCoroutine(FadeCanvas(visible));
            }

            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(visible);
            }

            isVisible = visible;
        }

        private IEnumerator FadeCanvas(bool targetVisible)
        {
            float startingAlpha = canvasGroup.alpha;
            float targetAlpha = targetVisible ? 1f : 0f;
            float elapsed = 0f;

            ApplyCanvasInteractableState(true);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startingAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            ApplyCanvasInteractableState(targetVisible);
            fadeRoutine = null;
        }

        private void ApplyCanvasInteractableState(bool visible)
        {
            bool allowInteraction = visible && blockInputWhileVisible;
            canvasGroup.interactable = allowInteraction;
            canvasGroup.blocksRaycasts = allowInteraction;
        }
    }
}
