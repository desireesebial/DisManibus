using UnityEngine;

/// <summary>
/// Swaps the sitting kuchi model to standing kuchi model after the video cutscene plays.
/// Monitors the VideoTrigger's hasPlayed flag and performs an instant model swap.
/// </summary>
public class KuchiModelSwap : MonoBehaviour
{
    [Header("Kuchi Model References")]
    [Tooltip("The sitting kuchi GameObject (will be disabled after cutscene)")]
    public GameObject sittingKuchi;

    [Tooltip("The standing kuchi GameObject (will be enabled after cutscene)")]
    public GameObject standingKuchi;

    [Header("Video Trigger Reference")]
    [Tooltip("Reference to the VideoTrigger component that plays the cutscene")]
    public VideoTrigger videoTrigger;

    private bool hasSwapped = false;

    void Update()
    {
        // Check if video has finished playing and we haven't swapped yet
        if (!hasSwapped && videoTrigger != null && videoTrigger.hasPlayed)
        {
            SwapKuchiModels();
            hasSwapped = true;
        }
    }

    /// <summary>
    /// Swaps the kuchi models - disables sitting, enables standing
    /// </summary>
    private void SwapKuchiModels()
    {
        Debug.Log("[KuchiModelSwap] Video cutscene finished - swapping models");

        // Disable sitting kuchi
        if (sittingKuchi != null)
        {
            sittingKuchi.SetActive(false);
            Debug.Log("[KuchiModelSwap] Disabled sitting kuchi");
        }
        else
        {
            Debug.LogWarning("[KuchiModelSwap] Sitting kuchi reference is null!");
        }

        // Enable standing kuchi
        if (standingKuchi != null)
        {
            standingKuchi.SetActive(true);
            Debug.Log("[KuchiModelSwap] Enabled standing kuchi");
        }
        else
        {
            Debug.LogWarning("[KuchiModelSwap] Standing kuchi reference is null!");
        }

        Debug.Log("[KuchiModelSwap] Model swap complete");
    }

    /// <summary>
    /// Validates references in the editor
    /// </summary>
    void OnValidate()
    {
        if (sittingKuchi == null)
            Debug.LogWarning("[KuchiModelSwap] Sitting Kuchi reference not assigned!");

        if (standingKuchi == null)
            Debug.LogWarning("[KuchiModelSwap] Standing Kuchi reference not assigned!");

        if (videoTrigger == null)
            Debug.LogWarning("[KuchiModelSwap] Video Trigger reference not assigned!");
    }
}
