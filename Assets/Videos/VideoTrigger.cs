using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoTrigger : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("The video clip to play when player enters the trigger")]
    public VideoClip videoClip;

    [Header("Playback Options")]
    [Tooltip("If true, video plays only once. If false, plays every time player enters trigger")]
    public bool playOnce = true;

    [Header("Skip Options")]
    [Tooltip("Determines if and how the player can skip the video")]
    public SkipMode skipMode = SkipMode.ESCToSkip;

    [Header("Optional References")]
    [Tooltip("Optional: Reference to the player GameObject. If not set, will search for 'Player' tag")]
    public GameObject player;

    [Tooltip("Optional: Reference to FirstPersonController. If not set, will find it automatically")]
    public FirstPersonController playerController;

    // Internal variables
    public bool hasPlayed = false; // Public so KuchisakeOnna can monitor it
    private bool isPlayingVideo = false;
    private GameObject videoCanvas;
    private VideoPlayer videoPlayer;
    private RawImage videoDisplay;

    // Store original game state to restore after video
    private bool originalPlayerCanMove;
    private bool originalCameraCanMove;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    private float originalTimeScale;

    public enum SkipMode
    {
        NoSkipping,
        ESCToSkip,
        SpaceToSkip,
        AnyKeyToSkip
    }

    private void Reset()
    {
        // Ensure the collider is set to trigger when component is added
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void Awake()
    {
        // Find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player entering the trigger
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Check if we should play the video
        if (playOnce && hasPlayed)
        {
            return;
        }

        // Check if video is already playing
        if (isPlayingVideo)
        {
            return;
        }

        // Check if video clip is assigned
        if (videoClip == null)
        {
            Debug.LogWarning($"VideoTrigger on '{name}': No video clip assigned!");
            return;
        }

        // Store player reference if not set
        if (player == null)
        {
            player = other.gameObject;
        }

        // Play the video
        PlayVideo();
    }

    private void PlayVideo()
    {
        Debug.Log($"VideoTrigger: Playing video '{videoClip.name}'");

        hasPlayed = true;
        isPlayingVideo = true;

        // Save current game state
        SaveGameState();

        // Pause game and disable player controls
        PauseGame();

        // Create UI overlay for video
        CreateVideoUI();

        // Setup and play video
        SetupVideoPlayer();
    }

    private void SaveGameState()
    {
        // Save time scale
        originalTimeScale = Time.timeScale;

        // Save cursor state
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // Save player controller state
        if (playerController != null)
        {
            originalPlayerCanMove = playerController.playerCanMove;
            originalCameraCanMove = playerController.cameraCanMove;
        }
    }

    private void PauseGame()
    {
        // Pause time
        Time.timeScale = 0f;

        // Show and unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player movement and camera
        if (playerController != null)
        {
            playerController.playerCanMove = false;
            playerController.cameraCanMove = false;
        }
        else
        {
            Debug.LogWarning("VideoTrigger: FirstPersonController not found! Player controls may not be disabled properly.");
        }
    }

    private void CreateVideoUI()
    {
        // Create canvas for video overlay
        videoCanvas = new GameObject("VideoCanvas");
        Canvas canvas = videoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it's on top

        CanvasScaler scaler = videoCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        videoCanvas.AddComponent<GraphicRaycaster>();

        // Create black background panel
        GameObject bgPanel = new GameObject("Background");
        bgPanel.transform.SetParent(videoCanvas.transform, false);
        RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = Color.black;

        // Create RawImage for video display
        GameObject videoObj = new GameObject("VideoDisplay");
        videoObj.transform.SetParent(videoCanvas.transform, false);
        RectTransform videoRect = videoObj.AddComponent<RectTransform>();
        videoRect.anchorMin = Vector2.zero;
        videoRect.anchorMax = Vector2.one;
        videoRect.sizeDelta = Vector2.zero;
        videoDisplay = videoObj.AddComponent<RawImage>();
        videoDisplay.color = Color.white;
    }

    private void SetupVideoPlayer()
    {
        // Create VideoPlayer component on the canvas
        videoPlayer = videoCanvas.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        // Create render texture for video
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.Create();
        videoPlayer.targetTexture = renderTexture;
        videoDisplay.texture = renderTexture;

        // Subscribe to video end event
        videoPlayer.loopPointReached += OnVideoFinished;

        // Start playing
        videoPlayer.Play();
    }

    private void Update()
    {
        // Only check for skip input while video is playing
        if (!isPlayingVideo || videoPlayer == null)
        {
            return;
        }

        // Check skip input based on skip mode
        bool shouldSkip = false;

        switch (skipMode)
        {
            case SkipMode.ESCToSkip:
                shouldSkip = Input.GetKeyDown(KeyCode.Escape);
                break;

            case SkipMode.SpaceToSkip:
                shouldSkip = Input.GetKeyDown(KeyCode.Space);
                break;

            case SkipMode.AnyKeyToSkip:
                shouldSkip = Input.anyKeyDown;
                break;

            case SkipMode.NoSkipping:
                // No skipping allowed
                break;
        }

        if (shouldSkip)
        {
            Debug.Log("VideoTrigger: Video skipped by player");
            StopVideo();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("VideoTrigger: Video finished playing");
        StopVideo();
    }

    private void StopVideo()
    {
        if (!isPlayingVideo)
        {
            return;
        }

        // Stop video player
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.loopPointReached -= OnVideoFinished;

            // Clean up render texture
            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
                Destroy(videoPlayer.targetTexture);
            }
        }

        // Destroy video UI
        if (videoCanvas != null)
        {
            Destroy(videoCanvas);
        }

        // Restore game state
        RestoreGameState();

        isPlayingVideo = false;
    }

    private void RestoreGameState()
    {
        // Restore time scale
        Time.timeScale = originalTimeScale;

        // Restore cursor state
        Cursor.lockState = originalCursorLockMode;
        Cursor.visible = originalCursorVisible;

        // Restore player controller state
        if (playerController != null)
        {
            playerController.playerCanMove = originalPlayerCanMove;
            playerController.cameraCanMove = originalCameraCanMove;
        }

        Debug.Log("VideoTrigger: Game state restored");
    }

    private void OnDestroy()
    {
        // Clean up if this object is destroyed while video is playing
        if (isPlayingVideo)
        {
            StopVideo();
        }
    }

    // Public method to reset the trigger (useful for testing or quest systems)
    public void ResetTrigger()
    {
        hasPlayed = false;
        Debug.Log("VideoTrigger: Trigger reset");
    }
}
