using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace World.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        private const float SceneProgressCompleteThreshold = 0.9f;

        [Header("Visuals")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private bool blockInputWhileVisible = true;
        [SerializeField] private bool hideOnStart = true;

        [Header("Behaviour")]
        [SerializeField] private float minimumDisplayTime = 2.0f;

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

            float startTime = Time.realtimeSinceStartup;

            // Prevent scene from activating until we're ready
            // This keeps the scene at 90% progress until we explicitly allow it
            operation.allowSceneActivation = false;

            // Phase 1: Wait for scene to load to 90% (0.9 is when Unity considers async load "done")
            while (operation.progress < 0.9f)
            {
                float normalizedProgress = Mathf.Clamp01(operation.progress / SceneProgressCompleteThreshold);
                UpdateProgress(normalizedProgress);
                yield return null;
            }

            // Phase 2: Scene is at 90%, now ensure minimum display time is met
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            float remainingTime = minimumDisplayTime - elapsedTime;

            if (remainingTime > 0)
            {
                // Smoothly fill progress from 90% to 100% during the remaining minimum time
                // This gives visual feedback that something is happening
                float startProgress = 0.9f;
                float fillTime = 0f;

                while (fillTime < remainingTime)
                {
                    fillTime += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(fillTime / remainingTime);
                    float displayProgress = Mathf.Lerp(startProgress, 1f, t);
                    UpdateProgress(displayProgress);
                    yield return null;
                }
            }

            // Ensure progress bar shows 100% completion
            UpdateProgress(1f);

            // Small delay so user can see 100% completion before scene switches
            yield return new WaitForSecondsRealtime(0.2f);

            // NOW allow scene to activate (happens almost instantly since it's been ready at 90%)
            operation.allowSceneActivation = true;

            // Wait for activation to complete
            while (!operation.isDone)
            {
                yield return null;
            }

            // Hide loading screen after scene is fully loaded and activated
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
