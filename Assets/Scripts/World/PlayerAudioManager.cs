using UnityEngine;

/// <summary>
/// Manages player audio for walking, sprinting, and exhaustion sounds.
/// Designed for 9-second looping audio clips with smooth state transitions.
/// </summary>
public class PlayerAudioManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the FirstPersonController (auto-assigned via singleton)")]
    private FirstPersonController playerController;

    [Header("Audio Settings")]
    [Tooltip("Volume fade out threshold when stamina recovers (0.0 to 1.0)")]
    [Range(0f, 1f)]
    public float breathingFadeThreshold = 0.3f; // 30% stamina

    [Tooltip("Input threshold for detecting movement (default: 0.1)")]
    [Range(0.01f, 0.5f)]
    public float movementInputThreshold = 0.1f;

    // Internal state tracking
    private enum MovementState
    {
        Idle,
        Walking,
        Sprinting
    }

    private MovementState currentState = MovementState.Idle;
    private bool isExhaustedAudioPlaying = false;

    // Audio components from FirstPersonController
    private AudioSource walkAudioSource;
    private AudioSource sprintAudioSource;
    private AudioSource breathingAudioSource;

    private AudioClip walkClip;
    private AudioClip sprintClip;
    private AudioClip breathingClip;

    void Start()
    {
        // Get reference to FirstPersonController via singleton
        playerController = FirstPersonController.Instance;

        if (playerController == null)
        {
            Debug.LogError("[PlayerAudioManager] FirstPersonController instance not found! Make sure FirstPersonController exists in the scene.", this);
            enabled = false;
            return;
        }

        // Cache audio component references from FirstPersonController
        walkAudioSource = playerController.footstepAudioSource;
        sprintAudioSource = playerController.sprintAudioSource;
        breathingAudioSource = playerController.breathingAudioSource;

        walkClip = playerController.walkFootstepSound;
        sprintClip = playerController.sprintFootstepSound;
        breathingClip = playerController.exhaustedBreathingSound;

        // Validate audio components
        ValidateAudioComponents();

        Debug.Log("[PlayerAudioManager] Initialized successfully for 9-second audio clips", this);
    }

    void Update()
    {
        if (playerController == null) return;

        // Update movement state and handle audio
        UpdateMovementAudio();
        UpdateExhaustionAudio();
    }

    /// <summary>
    /// Handles walking and sprinting audio based on player movement state
    /// </summary>
    private void UpdateMovementAudio()
    {
        // Detect movement input (WASD)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        bool hasMovementInput = (Mathf.Abs(horizontalInput) > movementInputThreshold ||
                                  Mathf.Abs(verticalInput) > movementInputThreshold);

        // Determine target state
        MovementState targetState = MovementState.Idle;

        if (hasMovementInput)
        {
            if (playerController.IsSprinting)
            {
                targetState = MovementState.Sprinting;
            }
            else
            {
                targetState = MovementState.Walking;
            }
        }

        // Only transition if state changed
        if (targetState != currentState)
        {
            TransitionToMovementState(targetState);
        }
    }

    /// <summary>
    /// Transitions between movement states (Idle, Walking, Sprinting)
    /// </summary>
    private void TransitionToMovementState(MovementState newState)
    {
        // Stop audio from previous state
        switch (currentState)
        {
            case MovementState.Walking:
                StopWalkAudio();
                break;
            case MovementState.Sprinting:
                StopSprintAudio();
                break;
        }

        // Start audio for new state
        switch (newState)
        {
            case MovementState.Walking:
                StartWalkAudio();
                break;
            case MovementState.Sprinting:
                StartSprintAudio();
                break;
            case MovementState.Idle:
                // Both audio sources already stopped
                Debug.Log("[PlayerAudioManager] Transitioned to Idle - all movement audio stopped", this);
                break;
        }

        currentState = newState;
    }

    /// <summary>
    /// Starts walk audio (9-second loop)
    /// </summary>
    private void StartWalkAudio()
    {
        if (walkAudioSource == null || walkClip == null)
        {
            Debug.LogWarning("[PlayerAudioManager] Cannot start walk audio - AudioSource or AudioClip is missing!", this);
            return;
        }

        walkAudioSource.clip = walkClip;
        walkAudioSource.loop = true;
        walkAudioSource.Play();
        Debug.Log("[PlayerAudioManager] Walk audio started (9-second loop)", this);
    }

    /// <summary>
    /// Stops walk audio
    /// </summary>
    private void StopWalkAudio()
    {
        if (walkAudioSource != null && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
            Debug.Log("[PlayerAudioManager] Walk audio stopped", this);
        }
    }

    /// <summary>
    /// Starts sprint audio (9-second loop)
    /// </summary>
    private void StartSprintAudio()
    {
        if (sprintAudioSource == null || sprintClip == null)
        {
            Debug.LogWarning("[PlayerAudioManager] Cannot start sprint audio - AudioSource or AudioClip is missing!", this);
            return;
        }

        sprintAudioSource.clip = sprintClip;
        sprintAudioSource.loop = true;
        sprintAudioSource.Play();
        Debug.Log("[PlayerAudioManager] Sprint audio started (9-second loop)", this);
    }

    /// <summary>
    /// Stops sprint audio
    /// </summary>
    private void StopSprintAudio()
    {
        if (sprintAudioSource != null && sprintAudioSource.isPlaying)
        {
            sprintAudioSource.Stop();
            Debug.Log("[PlayerAudioManager] Sprint audio stopped", this);
        }
    }

    /// <summary>
    /// Handles exhaustion/panting audio when stamina reaches 0
    /// </summary>
    private void UpdateExhaustionAudio()
    {
        if (playerController == null) return;

        bool isFatigued = playerController.IsFatigued;

        // Start breathing audio when exhausted
        if (isFatigued && !isExhaustedAudioPlaying)
        {
            StartExhaustionAudio();
        }
        // Update volume fade during recovery
        else if (isFatigued && isExhaustedAudioPlaying)
        {
            UpdateExhaustionVolumeFade();
        }
        // Stop breathing audio when recovered
        else if (!isFatigued && isExhaustedAudioPlaying)
        {
            StopExhaustionAudio();
        }
    }

    /// <summary>
    /// Starts exhaustion/panting audio (9-second loop)
    /// </summary>
    private void StartExhaustionAudio()
    {
        if (breathingAudioSource == null || breathingClip == null)
        {
            Debug.LogWarning("[PlayerAudioManager] Cannot start exhaustion audio - AudioSource or AudioClip is missing!", this);
            return;
        }

        breathingAudioSource.clip = breathingClip;
        breathingAudioSource.loop = true;
        breathingAudioSource.volume = 1f;
        breathingAudioSource.Play();
        isExhaustedAudioPlaying = true;
        Debug.Log("[PlayerAudioManager] Exhaustion/panting audio started - stamina depleted", this);
    }

    /// <summary>
    /// Updates breathing audio volume based on stamina recovery (fades out from 0% to 30%)
    /// </summary>
    private void UpdateExhaustionVolumeFade()
    {
        if (breathingAudioSource == null || playerController == null) return;

        // Calculate stamina percentage (0.0 to 1.0)
        float staminaPercent = playerController.SprintRemaining / playerController.sprintDuration;

        // Only fade if stamina is below threshold (default 30%)
        if (staminaPercent <= breathingFadeThreshold)
        {
            // Volume lerps from 1.0 (at 0% stamina) to 0.0 (at threshold%)
            float volumeFactor = 1f - (staminaPercent / breathingFadeThreshold);
            breathingAudioSource.volume = Mathf.Clamp01(volumeFactor);

            // Debug every 60 frames (~1 second at 60fps)
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[PlayerAudioManager] Exhaustion fade - Stamina: {staminaPercent:P0}, Volume: {breathingAudioSource.volume:F2}", this);
            }
        }
    }

    /// <summary>
    /// Stops exhaustion/panting audio
    /// </summary>
    private void StopExhaustionAudio()
    {
        if (breathingAudioSource != null && breathingAudioSource.isPlaying)
        {
            breathingAudioSource.Stop();
            breathingAudioSource.volume = 1f; // Reset volume for next time
            isExhaustedAudioPlaying = false;
            Debug.Log("[PlayerAudioManager] Exhaustion audio stopped - stamina recovered", this);
        }
    }

    /// <summary>
    /// Validates that all required audio components are assigned
    /// </summary>
    private void ValidateAudioComponents()
    {
        bool hasErrors = false;

        if (walkAudioSource == null)
        {
            Debug.LogError("[PlayerAudioManager] Walk AudioSource is not assigned in FirstPersonController!", this);
            hasErrors = true;
        }

        if (sprintAudioSource == null)
        {
            Debug.LogError("[PlayerAudioManager] Sprint AudioSource is not assigned in FirstPersonController!", this);
            hasErrors = true;
        }

        if (breathingAudioSource == null)
        {
            Debug.LogError("[PlayerAudioManager] Breathing AudioSource is not assigned in FirstPersonController!", this);
            hasErrors = true;
        }

        if (walkClip == null)
        {
            Debug.LogWarning("[PlayerAudioManager] Walk AudioClip is not assigned in FirstPersonController!", this);
        }

        if (sprintClip == null)
        {
            Debug.LogWarning("[PlayerAudioManager] Sprint AudioClip is not assigned in FirstPersonController!", this);
        }

        if (breathingClip == null)
        {
            Debug.LogWarning("[PlayerAudioManager] Breathing AudioClip is not assigned in FirstPersonController!", this);
        }

        if (!hasErrors)
        {
            Debug.Log("[PlayerAudioManager] All audio components validated successfully", this);
        }
    }

    /// <summary>
    /// Cleanup when component is disabled
    /// </summary>
    private void OnDisable()
    {
        // Stop all audio when disabled
        StopWalkAudio();
        StopSprintAudio();
        StopExhaustionAudio();
    }

    /// <summary>
    /// Debug info in Unity Inspector
    /// </summary>
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;

        // Optional: Display debug info in top-left corner
        // Uncomment to enable visual debugging
        /*
        GUILayout.BeginArea(new Rect(10, 150, 300, 100));
        GUILayout.Label($"Movement State: {currentState}");
        GUILayout.Label($"Exhausted Audio: {isExhaustedAudioPlaying}");
        if (playerController != null)
        {
            GUILayout.Label($"Stamina: {playerController.SprintRemaining:F1}/{playerController.sprintDuration:F1}");
        }
        GUILayout.EndArea();
        */
    }
}
