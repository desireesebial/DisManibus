using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

/// <summary>
/// Handles delayed playback of cutscenes to ensure proper scene initialization.
/// This prevents issues where the player falls through the floor due to physics/collider
/// initialization race conditions when cutscenes auto-play on scene load.
///
/// USAGE:
/// 1. Attach this script to the same GameObject as your PlayableDirector
/// 2. Set the PlayableDirector's "Initial State" to "Paused" in the Inspector
/// 3. Assign the PlayableDirector reference (or leave empty to auto-find)
/// 4. Adjust the startup delay as needed (default 0.1 seconds)
/// </summary>
public class CutsceneDelayedStart : MonoBehaviour
{
    [Header("Cutscene Setup")]
    [Tooltip("The PlayableDirector to start after delay. If not assigned, will auto-find on same GameObject.")]
    public PlayableDirector director;

    [Tooltip("Delay in seconds before starting the cutscene. Allows time for physics and colliders to initialize.")]
    [Range(0.05f, 1f)]
    public float startupDelay = 0.1f;

    [Tooltip("Should the cutscene play automatically on scene start?")]
    public bool playOnStart = true;

    [Header("Debug")]
    [Tooltip("Enable debug logging to track cutscene startup")]
    public bool enableDebugLogs = true;

    void Awake()
    {
        if (enableDebugLogs)
            Debug.Log($"[CutsceneDelayedStart] Awake on {gameObject.name}");

        // Auto-find director if not assigned
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
            if (director == null)
            {
                Debug.LogError($"[CutsceneDelayedStart] No PlayableDirector found on {gameObject.name}! Please assign one in the Inspector.");
                enabled = false;
                return;
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log($"[CutsceneDelayedStart] Auto-found PlayableDirector on {gameObject.name}");
            }
        }

        // Ensure director is paused initially
        if (director.state == PlayState.Playing)
        {
            Debug.LogWarning($"[CutsceneDelayedStart] PlayableDirector on {gameObject.name} was already playing! Pausing it for delayed start.");
            director.Pause();
        }
    }

    void Start()
    {
        if (playOnStart)
        {
            if (enableDebugLogs)
                Debug.Log($"[CutsceneDelayedStart] Starting delayed cutscene playback coroutine with {startupDelay}s delay");

            StartCoroutine(StartCutsceneAfterDelay());
        }
    }

    /// <summary>
    /// Starts the cutscene after waiting for the specified delay.
    /// This ensures the scene is fully initialized, physics is ready, and colliders are active.
    /// </summary>
    private IEnumerator StartCutsceneAfterDelay()
    {
        if (enableDebugLogs)
            Debug.Log($"[CutsceneDelayedStart] Waiting {startupDelay} seconds for scene initialization...");

        // Wait for specified delay
        yield return new WaitForSeconds(startupDelay);

        // Additional safety: wait one more fixed update to ensure physics is ready
        yield return new WaitForFixedUpdate();

        // Verify director is still valid and not already playing
        if (director == null)
        {
            Debug.LogError("[CutsceneDelayedStart] PlayableDirector became null during delay!");
            yield break;
        }

        if (director.state == PlayState.Playing)
        {
            Debug.LogWarning("[CutsceneDelayedStart] PlayableDirector is already playing, skipping delayed start");
            yield break;
        }

        // Start the cutscene
        if (enableDebugLogs)
            Debug.Log($"[CutsceneDelayedStart] Scene initialized - Starting cutscene playback now");

        director.Play();

        if (enableDebugLogs)
            Debug.Log($"[CutsceneDelayedStart] Cutscene started successfully. State: {director.state}");
    }

    /// <summary>
    /// Manually trigger the cutscene playback.
    /// Useful for triggering cutscenes from other scripts or events.
    /// </summary>
    public void PlayCutscene()
    {
        if (director == null)
        {
            Debug.LogError("[CutsceneDelayedStart] Cannot play cutscene - PlayableDirector is null!");
            return;
        }

        if (enableDebugLogs)
            Debug.Log("[CutsceneDelayedStart] Manually triggering cutscene playback");

        director.Play();
    }

    /// <summary>
    /// Manually trigger the cutscene with a custom delay.
    /// </summary>
    /// <param name="customDelay">Delay in seconds before starting</param>
    public void PlayCutsceneWithDelay(float customDelay)
    {
        StartCoroutine(StartCutsceneWithCustomDelay(customDelay));
    }

    private IEnumerator StartCutsceneWithCustomDelay(float delay)
    {
        if (enableDebugLogs)
            Debug.Log($"[CutsceneDelayedStart] Starting cutscene with custom delay of {delay}s");

        yield return new WaitForSeconds(delay);
        yield return new WaitForFixedUpdate();

        if (director != null)
        {
            director.Play();
        }
    }
}
