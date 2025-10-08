using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Kamatayan Hallucination Effect - 4th Floor Event
/// Makes Kamatayan stand idle at a specific position, waiting for the player.
/// When the player sees it (gets close or looks at it), it disappears like a hallucination.
/// Creates a creepy psychological horror effect.
/// 
/// SETUP:
/// 1. Place Kamatayan GameObject where you want it to stand
/// 2. Add this script directly to the Kamatayan GameObject
/// 3. Assign the Player (or leave empty to auto-find by tag "Player")
/// 4. Configure detection settings (distance/line of sight)
/// 5. (Optional) Add events to trigger sounds/effects when it disappears
/// </summary>
public class KamatayanHallucinationEffect : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("The player GameObject (auto-finds by tag 'Player' if not assigned)")]
    public Transform player;

    [Tooltip("Distance at which Kamatayan can detect the player")]
    public float detectionRange = 15f;

    [Tooltip("Should Kamatayan only disappear when player is looking at it?")]
    public bool requireLineOfSight = true;

    [Tooltip("Field of view angle from player's camera to consider 'looking at' Kamatayan")]
    public float playerViewAngle = 45f;

    [Header("Disappear Effect")]
    [Tooltip("Time it takes to fade out (0 = instant)")]
    public float fadeOutTime = 0.5f;

    [Tooltip("Should there be a delay before checking for player detection?")]
    public float initialDelay = 0.5f;

    [Header("What Happens After")]
    [Tooltip("Should Kamatayan permanently disappear after being seen?")]
    public bool permanentDisappear = true;

    [Tooltip("If not permanent, how long before it reappears?")]
    public float reappearDelay = 10f;

    [Tooltip("Should Kamatayan resume roaming AI after reappearing?")]
    public bool enableRoamingAfterReappear = false;

    [Header("Events")]
    [Tooltip("Called when player detects Kamatayan and it starts to disappear")]
    public UnityEvent OnHallucinationFade;

    [Tooltip("Called after Kamatayan fully disappears")]
    public UnityEvent OnHallucinationGone;

    // Internal references
    private KamatayanController kamatayanController;
    private Renderer[] kamatayanRenderers;
    private Transform cameraTransform;
    private bool hasDisappeared = false;
    private bool isWaiting = true;
    private Vector3 waitPosition;

    void Start()
    {
        // Store the position where Kamatayan should wait
        waitPosition = transform.position;

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("[KamatayanHallucination] Auto-found Player by tag");
            }
        }

        // Get camera
        if (player != null)
        {
            cameraTransform = Camera.main?.transform;
        }

        // Get references
        kamatayanController = GetComponent<KamatayanController>();
        kamatayanRenderers = GetComponentsInChildren<Renderer>();

        // Disable AI so Kamatayan stands still and waits
        if (kamatayanController != null)
        {
            kamatayanController.enabled = false;
        }

        // Start detection after initial delay
        if (initialDelay > 0)
        {
            StartCoroutine(StartDetectionAfterDelay());
        }
    }

    void Update()
    {
        if (!isWaiting || hasDisappeared || player == null)
            return;

        // Check if player is close enough
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange)
            return;

        // Check if player is looking at Kamatayan (if required)
        if (requireLineOfSight)
        {
            if (!IsPlayerLookingAtKamatayan())
                return;
        }

        // Player detected! Disappear like a hallucination
        TriggerHallucinationDisappear();
    }

    bool IsPlayerLookingAtKamatayan()
    {
        if (cameraTransform == null)
            return false;

        // Direction from camera to Kamatayan
        Vector3 directionToKamatayan = (transform.position - cameraTransform.position).normalized;

        // Camera forward direction
        Vector3 cameraForward = cameraTransform.forward;

        // Calculate angle
        float angle = Vector3.Angle(cameraForward, directionToKamatayan);

        return angle < playerViewAngle;
    }

    void TriggerHallucinationDisappear()
    {
        if (hasDisappeared)
            return;

        hasDisappeared = true;
        isWaiting = false;

        Debug.Log("[KamatayanHallucination] Player detected - Disappearing like a hallucination!");

        // Trigger event
        OnHallucinationFade?.Invoke();

        // Start fade out
        if (fadeOutTime > 0)
        {
            StartCoroutine(FadeOutAndDisappear());
        }
        else
        {
            CompleteDisappear();
        }
    }

    IEnumerator StartDetectionAfterDelay()
    {
        isWaiting = false;
        yield return new WaitForSeconds(initialDelay);
        isWaiting = true;
    }

    IEnumerator FadeOutAndDisappear()
    {
        // Simple fade out by reducing alpha (works if materials support transparency)
        float elapsed = 0f;
        Material[] originalMaterials = new Material[kamatayanRenderers.Length];

        // Store original materials and enable transparency
        for (int i = 0; i < kamatayanRenderers.Length; i++)
        {
            if (kamatayanRenderers[i] != null && kamatayanRenderers[i].material != null)
            {
                originalMaterials[i] = kamatayanRenderers[i].material;
                kamatayanRenderers[i].material.SetFloat("_Mode", 2); // Fade mode
                kamatayanRenderers[i].material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                kamatayanRenderers[i].material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                kamatayanRenderers[i].material.SetInt("_ZWrite", 0);
                kamatayanRenderers[i].material.DisableKeyword("_ALPHATEST_ON");
                kamatayanRenderers[i].material.EnableKeyword("_ALPHABLEND_ON");
                kamatayanRenderers[i].material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                kamatayanRenderers[i].material.renderQueue = 3000;
            }
        }

        // Fade out
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeOutTime);

            foreach (Renderer renderer in kamatayanRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    Color color = renderer.material.color;
                    color.a = alpha;
                    renderer.material.color = color;
                }
            }

            yield return null;
        }

        CompleteDisappear();
    }

    void CompleteDisappear()
    {
        // Hide completely
        foreach (Renderer renderer in kamatayanRenderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }

        // Trigger gone event
        OnHallucinationGone?.Invoke();

        // Handle reappear if not permanent
        if (!permanentDisappear)
        {
            StartCoroutine(ReappearAfterDelay());
        }
    }

    IEnumerator ReappearAfterDelay()
    {
        yield return new WaitForSeconds(reappearDelay);

        // Show again
        foreach (Renderer renderer in kamatayanRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
                // Reset alpha
                Color color = renderer.material.color;
                color.a = 1f;
                renderer.material.color = color;
            }
        }

        // Reset position
        transform.position = waitPosition;

        // Enable roaming AI if requested
        if (enableRoamingAfterReappear && kamatayanController != null)
        {
            kamatayanController.enabled = true;
            Debug.Log("[KamatayanHallucination] Kamatayan reappeared and is now roaming");
        }
        else
        {
            // Reset to wait again
            hasDisappeared = false;
            isWaiting = true;
            Debug.Log("[KamatayanHallucination] Kamatayan reappeared and is waiting again");
        }
    }

    /// <summary>
    /// Manually trigger the hallucination disappear effect
    /// </summary>
    public void ManualTriggerDisappear()
    {
        TriggerHallucinationDisappear();
    }

    /// <summary>
    /// Reset the hallucination to start waiting again
    /// </summary>
    public void ResetHallucination()
    {
        hasDisappeared = false;
        isWaiting = true;
        transform.position = waitPosition;

        // Show visuals
        foreach (Renderer renderer in kamatayanRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
                Color color = renderer.material.color;
                color.a = 1f;
                renderer.material.color = color;
            }
        }

        Debug.Log("[KamatayanHallucination] Reset - Kamatayan waiting again");
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        // Draw detection range
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw line to player if in range
        if (player != null && Application.isPlaying)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= detectionRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw field of view cone for line of sight
        if (requireLineOfSight && cameraTransform != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            // This is a simplified representation
            Vector3 leftBoundary = Quaternion.Euler(0, -playerViewAngle, 0) * cameraTransform.forward * detectionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, playerViewAngle, 0) * cameraTransform.forward * detectionRange;
            
            Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + leftBoundary);
            Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + rightBoundary);
        }
    }
}


