using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class VideoInteractable : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("The video clip to play when player interacts")]
    public VideoClip videoClip;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Press E to watch video";

    [Header("Crosshair Detection")]
    public bool useCrosshairDetection = true;
    public float crosshairDetectionRange = 10f;
    public LayerMask objectLayerMask = -1;

    [Header("Crosshair Feedback")]
    public bool changeCrosshairColor = true;
    public Color crosshairHoverColor = Color.yellow;

    [Header("UI References")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionTextUI;

    [Header("Playback Options")]
    [Tooltip("If true, video can only be watched once. If false, can be watched multiple times")]
    public bool playOnce = false;

    [Header("Skip Options")]
    [Tooltip("Determines if and how the player can skip the video")]
    public SkipMode skipMode = SkipMode.ESCToSkip;

    [Header("Optional References")]
    [Tooltip("Optional: Reference to FirstPersonController. If not set, will find it automatically")]
    public FirstPersonController playerController;

    // Internal state
    private bool playerInRange = false;
    private bool isPlayingVideo = false;
    private bool hasPlayed = false;

    // Player references
    private Rigidbody playerRigidbody;
    private Camera playerCamera;
    private Image crosshairObject;
    private Color originalCrosshairColor;
    private bool wasHovering = false;

    // Video playback
    private GameObject videoCanvas;
    private VideoPlayer videoPlayer;
    private RawImage videoDisplay;

    // Store original game state to restore after video
    private bool originalPlayerCanMove;
    private bool originalCameraCanMove;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    private float originalTimeScale;

    // Cached collider reference
    private Collider objectCollider;

    public enum SkipMode
    {
        NoSkipping,
        ESCToSkip,
        SpaceToSkip,
        AnyKeyToSkip
    }

    void Start()
    {
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerCamera = player.GetComponentInChildren<Camera>();

            // Get crosshair reference from FirstPersonController
            if (playerController != null)
            {
                crosshairObject = playerController.GetComponentInChildren<Image>();
                if (crosshairObject != null)
                {
                    originalCrosshairColor = crosshairObject.color;
                }
            }
        }

        // Collider Validation
        objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            objectCollider = GetComponentInChildren<Collider>();
        }

        if (useCrosshairDetection && objectCollider == null)
        {
            Debug.LogError($"[VideoInteractable] {gameObject.name}: Crosshair detection is enabled but NO COLLIDER found! Please add a collider to this GameObject.");
        }
        else if (objectCollider != null)
        {
            // Check if collider's layer matches the layer mask
            int colliderLayer = objectCollider.gameObject.layer;
            bool layerMatches = (objectLayerMask == -1) || ((objectLayerMask.value & (1 << colliderLayer)) != 0);

            if (!layerMatches)
            {
                Debug.LogWarning($"[VideoInteractable] {gameObject.name}: Collider is on layer '{LayerMask.LayerToName(colliderLayer)}' which doesn't match objectLayerMask! Raycast detection may fail.");
            }
        }

        // UI Validation
        if (interactionUI == null)
        {
            Debug.LogWarning($"[VideoInteractable] {gameObject.name}: interactionUI not assigned! Interaction prompt won't show.");
        }
        else
        {
            // Try to auto-find TextMeshProUGUI if not assigned
            if (interactionTextUI == null)
            {
                interactionTextUI = interactionUI.GetComponentInChildren<TextMeshProUGUI>();
            }

            interactionUI.SetActive(false);
        }

        // Validate video clip
        if (videoClip == null)
        {
            Debug.LogError($"[VideoInteractable] {gameObject.name}: No video clip assigned!");
        }
    }

    void Update()
    {
        // Don't check for interaction while video is playing
        if (isPlayingVideo)
        {
            HandleVideoSkip();
            return;
        }

        CheckPlayerDistance();
        HandleInteraction();
    }

    void CheckPlayerDistance()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool wasInRange = playerInRange;

        if (useCrosshairDetection)
        {
            playerInRange = IsObjectUnderCrosshair() && distance <= crosshairDetectionRange;
        }
        else
        {
            playerInRange = distance <= interactionRange;
        }

        // Show/hide UI and crosshair feedback
        if (playerInRange && !wasInRange)
        {
            // Check if already played and playOnce is true
            if (playOnce && hasPlayed)
            {
                return; // Don't show prompt if already played
            }

            ShowInteractionUI();
            ChangeCrosshairColor(true);
        }
        else if (!playerInRange && wasInRange)
        {
            HideInteractionUI();
            ChangeCrosshairColor(false);
        }
    }

    void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            // Check if already played
            if (playOnce && hasPlayed)
            {
                Debug.Log($"[VideoInteractable] {gameObject.name}: Video already played (playOnce is enabled)");
                return;
            }

            PlayVideo();
        }
    }

    void HandleVideoSkip()
    {
        if (!isPlayingVideo || videoPlayer == null) return;

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
            Debug.Log($"[VideoInteractable] {gameObject.name}: Video skipped by player");
            StopVideo();
        }
    }

    bool IsObjectUnderCrosshair()
    {
        if (playerCamera == null) return false;

        // Cast ray from center of screen (crosshair position)
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, crosshairDetectionRange, objectLayerMask))
        {
            // Check if the hit object is this object or its child
            return hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform);
        }

        return false;
    }

    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionTextUI != null)
            {
                interactionTextUI.text = interactionText;
            }
        }
    }

    void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void ChangeCrosshairColor(bool isHovering)
    {
        if (!changeCrosshairColor || crosshairObject == null) return;

        if (isHovering && !wasHovering)
        {
            crosshairObject.color = crosshairHoverColor;
            wasHovering = true;
        }
        else if (!isHovering && wasHovering)
        {
            crosshairObject.color = originalCrosshairColor;
            wasHovering = false;
        }
    }

    void PlayVideo()
    {
        if (videoClip == null)
        {
            Debug.LogError($"[VideoInteractable] {gameObject.name}: Cannot play video - no video clip assigned!");
            return;
        }

        Debug.Log($"[VideoInteractable] {gameObject.name}: Playing video '{videoClip.name}'");

        hasPlayed = true;
        isPlayingVideo = true;

        // Hide interaction UI
        HideInteractionUI();
        ChangeCrosshairColor(false);

        // Save current game state
        SaveGameState();

        // Pause game and disable player controls
        PauseGame();

        // Create UI overlay for video
        CreateVideoUI();

        // Setup and play video
        SetupVideoPlayer();
    }

    void SaveGameState()
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

    void PauseGame()
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
    }

    void CreateVideoUI()
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

    void SetupVideoPlayer()
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

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log($"[VideoInteractable] {gameObject.name}: Video finished playing");
        StopVideo();
    }

    void StopVideo()
    {
        if (!isPlayingVideo) return;

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

    void RestoreGameState()
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

        Debug.Log($"[VideoInteractable] {gameObject.name}: Game state restored");
    }

    void OnDestroy()
    {
        // Clean up if this object is destroyed while video is playing
        if (isPlayingVideo)
        {
            StopVideo();
        }
    }

    // Public method to reset (useful for testing or quest systems)
    public void ResetInteractable()
    {
        hasPlayed = false;
        Debug.Log($"[VideoInteractable] {gameObject.name}: Interactable reset");
    }

    // Visual debugging in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw interaction range sphere (simple distance detection)
        if (!useCrosshairDetection)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        // Draw crosshair detection range sphere
        if (useCrosshairDetection)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, crosshairDetectionRange);
        }

        // Draw collider bounds if available
        if (objectCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(objectCollider.bounds.center, objectCollider.bounds.size);
        }
        else
        {
            // Warning: No collider found
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}
