using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Individual torch/candle for sequential lighting puzzle
/// Each torch can only be lit when the previous one in sequence is already lit
/// NOTE: Use TorchInteractionPrompt component for UI prompts
/// </summary>
public class SequentialTorch : MonoBehaviour
{
    [Header("Torch Settings")]
    [Tooltip("Sequence number (1, 2, 3, etc.) - determines lighting order")]
    public int sequenceNumber = 1;

    [Tooltip("How close player needs to be to interact")]
    public float interactionDistance = 3f;

    [Tooltip("Max angle between player look direction and torch for interaction (degrees)")]
    public float lookAngleThreshold = 45f;

    [Header("Visual Components")]
    [Tooltip("The flame GameObject (should be disabled initially)")]
    public GameObject flameObject;

    [Tooltip("The light component for illumination")]
    public Light torchLight;

    [Tooltip("Particle system for fire effects")]
    public ParticleSystem fireParticles;

    [Tooltip("Particle system for ready-to-light effect")]
    public ParticleSystem readyParticles;

    [Header("Connection Trail")]
    [Tooltip("Particle system for the line connecting to previous torch")]
    public ParticleSystem connectionTrail;

    [Tooltip("Duration for trail animation from previous torch (seconds)")]
    public float trailAnimationDuration = 1.5f;

    [Tooltip("How long to wait before fading out the trail (seconds)")]
    public float trailFadeDelay = 2f;

    [Tooltip("Duration for trail fade out (seconds)")]
    public float trailFadeDuration = 1.5f;

    [Header("Audio")]
    public AudioClip lightSound;
    public AudioClip wrongSequenceSound;
    public AudioClip readySound;

    // State
    private bool isLit = false;
    private bool isReadyToLight = false;
    private Transform player;
    private Camera playerCamera;
    private AudioSource audioSource;
    private SequentialTorchManager puzzleManager;

    // Visual feedback
    private Renderer torchRenderer;
    private Color originalColor;
    private Color readyColor = Color.yellow;
    private Color litColor = Color.orange;

    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            player = playerObj.transform;

            // Get player camera - check common locations
            playerCamera = playerObj.GetComponentInChildren<Camera>();
            if (!playerCamera)
            {
                playerCamera = Camera.main;
            }
        }

        // Find puzzle manager
        puzzleManager = FindObjectOfType<SequentialTorchManager>();

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

        // Visual setup
        torchRenderer = GetComponent<Renderer>();
        if (torchRenderer) originalColor = torchRenderer.material.color;

        // Initially unlit
        SetTorchState(false, false);

        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} initialized (camera found: {playerCamera != null})");
    }

    void Update()
    {
        if (!player || !playerCamera || isLit)
        {
            return;
        }

        // Check if player is looking at and close to the torch
        bool canInteract = IsPlayerLookingAtTorch();

        // Handle F key press
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            TryLightTorch();
        }
    }

    /// <summary>
    /// Checks if player is looking at the torch and within interaction distance
    /// </summary>
    bool IsPlayerLookingAtTorch()
    {
        // Check distance first (cheaper check)
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > interactionDistance)
        {
            return false;
        }

        // Get direction from camera to torch
        Vector3 directionToTorch = (transform.position - playerCamera.transform.position).normalized;
        Vector3 cameraForward = playerCamera.transform.forward;

        // Calculate angle between camera forward and direction to torch
        float angle = Vector3.Angle(cameraForward, directionToTorch);

        // Check if within look angle threshold
        if (angle > lookAngleThreshold)
        {
            return false;
        }

        // Optional: Raycast to ensure line of sight (prevents interaction through walls)
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, directionToTorch, out hit, distance + 0.5f))
        {
            // Check if we hit this torch or its parent
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }

    void TryLightTorch()
    {
        // Safety check: already lit
        if (isLit)
        {
            Debug.LogWarning($"[SequentialTorch] Torch {sequenceNumber} is already lit. Ignoring interaction.");
            return;
        }

        // Safety check: no puzzle manager assigned
        if (puzzleManager == null)
        {
            Debug.LogError($"[SequentialTorch] Torch {sequenceNumber} has no puzzle manager assigned! Cannot verify sequence.");
            return;
        }

        // Check if this torch is ready to be lit (asks manager if sequence is correct)
        if (!puzzleManager.CanLightTorch(sequenceNumber))
        {
            // Wrong sequence - show feedback
            Debug.Log($"[SequentialTorch] Torch {sequenceNumber} - Wrong sequence detected!");
            ShowWrongSequenceFeedback();
            return;
        }

        // All checks passed - light the torch
        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} - Lighting now...");
        LightTorch();
    }

    public void LightTorch()
    {
        // Double-check to prevent lighting already-lit torch
        if (isLit)
        {
            Debug.LogWarning($"[SequentialTorch] LightTorch() called on torch {sequenceNumber}, but it's already lit. Ignoring.");
            return;
        }

        Debug.Log($"[SequentialTorch] 🔥 Torch {sequenceNumber} LIT!");

        // Set lit state
        isLit = true;
        SetTorchState(true, false);

        // Play sound
        if (lightSound && audioSource != null)
        {
            audioSource.PlayOneShot(lightSound);
        }

        // Notify puzzle manager
        if (puzzleManager != null)
        {
            puzzleManager.OnTorchLit(sequenceNumber);
        }
        else
        {
            Debug.LogWarning($"[SequentialTorch] Torch {sequenceNumber} lit, but no puzzle manager to notify!");
        }
    }

    public void ExtinguishTorch()
    {
        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} extinguished");

        isLit = false;
        isReadyToLight = false;
        SetTorchState(false, false);
    }

    void ShowWrongSequenceFeedback()
    {
        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} - Wrong sequence!");

        // Play wrong sound
        if (wrongSequenceSound) audioSource.PlayOneShot(wrongSequenceSound);

        // Notify puzzle manager
        if (puzzleManager) puzzleManager.OnWrongSequenceAttempted(sequenceNumber);

        // Visual feedback - red flash
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        if (torchRenderer)
        {
            Color original = torchRenderer.material.color;
            torchRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            torchRenderer.material.color = original;
        }
    }

    public void SetReadyToLight(bool ready)
    {
        if (isLit)
        {
            return;
        }

        isReadyToLight = ready;
        SetTorchState(false, ready);

        // Play ready sound
        if (ready && readySound) audioSource.PlayOneShot(readySound);

        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} ready to light: {ready}");
    }

    /// <summary>
    /// Creates a visual connection line from the previous torch to this torch
    /// </summary>
    public void CreateConnectionLine(SequentialTorch previousTorch)
    {
        if (!connectionTrail || !previousTorch)
        {
            Debug.LogWarning($"[SequentialTorch] Cannot create connection line - missing connectionTrail or previousTorch");
            return;
        }

        Debug.Log($"[SequentialTorch] Creating connection line from torch {previousTorch.sequenceNumber} to {sequenceNumber}");

        // Start the trail animation coroutine
        StartCoroutine(AnimateTrailFromTorch(previousTorch.transform.position, transform.position));
    }

    /// <summary>
    /// Animates particle trail from start position to end position, then fades it out
    /// </summary>
    IEnumerator AnimateTrailFromTorch(Vector3 startPos, Vector3 endPos)
    {
        if (!connectionTrail) yield break;

        // Calculate distance and determine number of particle bursts needed
        float distance = Vector3.Distance(startPos, endPos);
        int particleBurstCount = Mathf.Max(10, Mathf.CeilToInt(distance * 2)); // More particles for longer distances

        // Enable the particle system
        connectionTrail.gameObject.SetActive(true);

        // Get the emission and main modules
        var emission = connectionTrail.emission;
        var main = connectionTrail.main;

        // Disable automatic emission - we'll manually emit particles
        emission.enabled = false;

        // Store all emitted particles' data for fading
        List<Vector3> trailPositions = new List<Vector3>();

        // Animate from start to end position, emitting particles along the way
        float elapsed = 0f;
        int lastBurstIndex = -1;

        while (elapsed < trailAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / trailAnimationDuration;

            // Smooth interpolation
            t = Mathf.SmoothStep(0f, 1f, t);

            // Calculate current position along the path
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);

            // Determine which burst index we should be at
            int currentBurstIndex = Mathf.FloorToInt(t * particleBurstCount);

            // Emit particles if we've reached a new position
            if (currentBurstIndex > lastBurstIndex && currentBurstIndex < particleBurstCount)
            {
                // Position particle system at current location
                connectionTrail.transform.position = currentPos;

                // Emit a burst of particles
                connectionTrail.Emit(3); // Emit 3-5 particles per position

                trailPositions.Add(currentPos);
                lastBurstIndex = currentBurstIndex;
            }

            yield return null;
        }

        // Emit final particles at end position
        connectionTrail.transform.position = endPos;
        connectionTrail.Emit(5);
        trailPositions.Add(endPos);

        Debug.Log($"[SequentialTorch] Trail animation completed for torch {sequenceNumber} (emitted particles at {trailPositions.Count} positions over {distance:F2} units)");

        // Wait before fading
        yield return new WaitForSeconds(trailFadeDelay);

        // Fade out the trail by reducing alpha over time
        float fadeElapsed = 0f;
        Color originalColor = main.startColor.color;

        while (fadeElapsed < trailFadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float fadeFactor = 1f - (fadeElapsed / trailFadeDuration);

            // Reduce alpha in the main module
            Color fadedColor = originalColor;
            fadedColor.a = originalColor.a * fadeFactor;
            main.startColor = fadedColor;

            yield return null;
        }

        // Stop and disable the particle system
        connectionTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        connectionTrail.gameObject.SetActive(false);

        // Restore original settings for next time
        emission.enabled = false; // Keep manual emission
        main.startColor = originalColor;

        Debug.Log($"[SequentialTorch] Trail faded out for torch {sequenceNumber}");
    }

    void SetTorchState(bool lit, bool ready)
    {
        // Flame object
        if (flameObject) flameObject.SetActive(lit);

        // Light component
        if (torchLight) torchLight.enabled = lit;

        // Fire particles
        if (fireParticles)
        {
            if (lit && !fireParticles.isPlaying)
                fireParticles.Play();
            else if (!lit && fireParticles.isPlaying)
                fireParticles.Stop();
        }

        // Ready particles
        if (readyParticles)
        {
            if (ready && !lit && !readyParticles.isPlaying)
                readyParticles.Play();
            else if ((!ready || lit) && readyParticles.isPlaying)
                readyParticles.Stop();
        }

        // Torch color
        if (torchRenderer)
        {
            if (lit)
                torchRenderer.material.color = litColor;
            else if (ready)
                torchRenderer.material.color = readyColor;
            else
                torchRenderer.material.color = originalColor;
        }
    }

    public bool IsLit => isLit;
    public int SequenceNumber => sequenceNumber;

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isReadyToLight ? Color.green : (isLit ? Color.orange : Color.gray);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Draw look direction cone (approximate)
        if (Application.isPlaying && playerCamera != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 directionToTorch = (transform.position - playerCamera.transform.position).normalized;
            Gizmos.DrawLine(playerCamera.transform.position, transform.position);
        }

        #if UNITY_EDITOR
        // Draw sequence number
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Torch {sequenceNumber}");
        #endif
    }
}
