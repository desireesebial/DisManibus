using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using World.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("UI References")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private Image fadeImage;

    [Header("Loading Screen")]
    [SerializeField] private bool useLoadingScreenByDefault = true;
    [SerializeField] private string defaultLoadingMessage = "Loading...";
    [SerializeField] private float minimumLoadingTime = 2.0f;
    
    private static SceneTransitionManager instance;
    private bool isTransitioning = false;
    
    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SceneTransitionManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SceneTransitionManager");
                    instance = go.AddComponent<SceneTransitionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupTransitionCanvas();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Start()
    {
        // Ensure we start with a fade in
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
    }
    
    private void SetupTransitionCanvas()
    {
        if (transitionCanvas == null)
        {
            // Create transition canvas if it doesn't exist
            GameObject canvasGO = new GameObject("TransitionCanvas");
            transitionCanvas = canvasGO.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 999; // Ensure it's on top
            
            // Add canvas scaler
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add graphic raycaster
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create fade image
            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(canvasGO.transform, false);
            
            fadeImage = imageGO.AddComponent<Image>();
            fadeImage.color = Color.black;
            fadeImage.raycastTarget = false;
            
            // Set image to fill screen
            RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Create canvas group
            fadeCanvasGroup = imageGO.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            
            DontDestroyOnLoad(canvasGO);
        }
        else if (fadeCanvasGroup == null && fadeImage != null)
        {
            fadeCanvasGroup = fadeImage.GetComponent<CanvasGroup>();
            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = fadeImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
    
    public void LoadScene(string sceneName, string loadingMessage = null, bool? useLoadingScreenOverride = null)
    {
        if (!isTransitioning)
        {
            bool useLoadingScreen = useLoadingScreenOverride ?? useLoadingScreenByDefault;
            StartCoroutine(LoadSceneCoroutine(sceneName, loadingMessage, useLoadingScreen));
        }
    }
    
    public void LoadScene(int sceneIndex, string loadingMessage = null, bool? useLoadingScreenOverride = null)
    {
        if (!isTransitioning)
        {
            bool useLoadingScreen = useLoadingScreenOverride ?? useLoadingScreenByDefault;
            StartCoroutine(LoadSceneCoroutine(sceneIndex, loadingMessage, useLoadingScreen));
        }
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName, string loadingMessage, bool useLoadingScreen)
    {
        isTransitioning = true;
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        yield return HandleAsyncOperation(operation, loadingMessage, useLoadingScreen);

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }
    
    private IEnumerator LoadSceneCoroutine(int sceneIndex, string loadingMessage, bool useLoadingScreen)
    {
        isTransitioning = true;
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        yield return HandleAsyncOperation(operation, loadingMessage, useLoadingScreen);

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }
    
    private IEnumerator HandleAsyncOperation(AsyncOperation operation, string loadingMessage, bool useLoadingScreen)
    {
        if (operation == null)
        {
            Debug.LogError("SceneTransitionManager failed to start async load operation.");
            yield break;
        }

        float startTime = Time.realtimeSinceStartup;

        if (useLoadingScreen)
        {
            if (LoadingScreenController.Instance != null)
            {
                string status = string.IsNullOrWhiteSpace(loadingMessage) ? defaultLoadingMessage : loadingMessage;
                Debug.Log($"SceneTransitionManager: Using LoadingScreenController with message '{status}'");
                yield return LoadingScreenController.Instance.TrackAsyncOperation(operation, status);
            }
            else
            {
                Debug.LogWarning("SceneTransitionManager: LoadingScreenController not found! Loading without loading screen.");
                yield return operation;
                
                // Ensure minimum loading time even without loading screen UI
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                if (elapsedTime < minimumLoadingTime)
                {
                    yield return new WaitForSecondsRealtime(minimumLoadingTime - elapsedTime);
                }
            }
        }
        else
        {
            Debug.Log("SceneTransitionManager: Loading without loading screen (useLoadingScreen = false)");
            yield return operation;
        }

        // Give new scene a frame to initialise before fading in
        yield return null;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            float alpha = fadeCurve.Evaluate(progress);
            
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            float alpha = 1f - fadeCurve.Evaluate(progress);
            
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;
    }
    
    // Public method to check if transition is in progress
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    // Public method to force fade out (useful for immediate transitions)
    public void ForceFadeOut()
    {
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;
    }
    
    // Public method to force fade in
    public void ForceFadeIn()
    {
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;
    }
}
