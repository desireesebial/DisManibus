using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DisManibus.World.SceneTransition;

/// <summary>
/// Defines the ending type for this scene
/// </summary>
public enum EndingType
{
    None,           // No special ending behavior
    GoodEnding,     // Good ending: enemy disappears
    BadEnding       // Bad ending: enemy becomes enraged
}

/// <summary>
/// Manages the sequential torch lighting puzzle
/// Controls which torches can be lit and handles puzzle completion
/// </summary>
public class SequentialTorchManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("All torches in the puzzle (should be in sequence order)")]
    public SequentialTorch[] torches;
    
    [Tooltip("How long to wait before resetting puzzle on wrong sequence")]
    public float resetDelay = 3f;
    
    [Tooltip("Reset puzzle when wrong sequence is attempted")]
    public bool resetOnWrongSequence = true;
    
    [Header("Rewards")]
    [Tooltip("Door to unlock when puzzle is completed")]
    public Door rewardDoor;
    
    [Tooltip("Items to spawn when puzzle is completed")]
    public GameObject[] rewardItems;
    
    [Tooltip("Where to spawn reward items")]
    public Transform rewardSpawnPoint;
    
    [Tooltip("GameObjects to enable/activate when puzzle is completed")]
    public GameObject[] rewardGameObjects;
    
    [Tooltip("GameObjects to make visible when puzzle is completed")]
    public GameObject[] rewardVisibleObjects;
    
    [Header("Audio")]
    public AudioClip puzzleCompleteSound;
    public AudioClip puzzleResetSound;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for puzzle completion")]
    public ParticleSystem completionParticles;

    [Tooltip("Light to turn on when puzzle is complete")]
    public Light completionLight;

    [Header("Ending Behavior")]
    [Tooltip("Ending type for this scene - affects Kuchisake Onna behavior after puzzle completion")]
    public EndingType endingType = EndingType.None;
    
    // State
    private int currentSequenceIndex = 0;
    private bool puzzleComplete = false;
    private bool isResetting = false; // Prevent multiple simultaneous resets
    private List<int> lightingOrder = new List<int>(); // Track the order torches were lit
    private AudioSource audioSource;
    
    void Start()
    {
        // Audio
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Initialize puzzle
        InitializePuzzle();
        
        Debug.Log($"[SequentialTorchManager] Puzzle initialized with {torches.Length} torches");
    }
    
    void InitializePuzzle()
    {
        // Validate torches array
        if (torches == null || torches.Length == 0)
        {
            Debug.LogError("[SequentialTorchManager] Torches array is null or empty! Please assign torches in the inspector.");
            return;
        }

        // Remove null entries from torches array
        torches = System.Array.FindAll(torches, torch => torch != null);

        if (torches.Length == 0)
        {
            Debug.LogError("[SequentialTorchManager] All torch references are null! Please assign valid torches.");
            return;
        }

        // Sort torches by sequence number
        System.Array.Sort(torches, (a, b) => a.SequenceNumber.CompareTo(b.SequenceNumber));

        // Validate sequence numbers
        bool validSequence = true;
        for (int i = 0; i < torches.Length; i++)
        {
            int expectedSequenceNumber = i + 1; // Sequence should be 1, 2, 3, ...
            if (torches[i].SequenceNumber != expectedSequenceNumber)
            {
                Debug.LogWarning($"[SequentialTorchManager] Torch at index {i} has sequence number {torches[i].SequenceNumber}, expected {expectedSequenceNumber}. " +
                    $"Puzzle may not work correctly. Please set sequence numbers to 1, 2, 3, ... in order.");
                validSequence = false;
            }
        }

        if (!validSequence)
        {
            Debug.LogError("[SequentialTorchManager] Sequence numbers are not configured correctly! They should be 1, 2, 3, ... with no gaps or duplicates.");
        }

        // Reset all torches
        foreach (var torch in torches)
        {
            torch.ExtinguishTorch();
        }

        // Reset state (all torches are now interactable - no "ready" concept)
        currentSequenceIndex = 0;
        puzzleComplete = false;

        // Hide completion effects
        if (completionParticles && completionParticles.isPlaying)
            completionParticles.Stop();

        if (completionLight)
            completionLight.enabled = false;
    }
    
    public bool CanLightTorch(int sequenceNumber)
    {
        // Puzzle already complete - no more lighting allowed
        if (puzzleComplete)
        {
            Debug.LogWarning($"[SequentialTorchManager] Cannot light torch {sequenceNumber} - puzzle already complete");
            return false;
        }

        // Validate sequence number is within bounds
        if (sequenceNumber < 1 || sequenceNumber > torches.Length)
        {
            Debug.LogError($"[SequentialTorchManager] Invalid sequence number {sequenceNumber}. Must be between 1 and {torches.Length}");
            return false;
        }

        // Check if this torch is already lit (prevent duplicate lighting)
        if (lightingOrder.Contains(sequenceNumber))
        {
            Debug.LogWarning($"[SequentialTorchManager] Torch {sequenceNumber} is already lit. Cannot light again.");
            return false;
        }

        // Allow any torch to be lit (no sequential restriction)
        Debug.Log($"[SequentialTorchManager] Allowing torch {sequenceNumber} to be lit (current order: {string.Join("→", lightingOrder)})");
        return true;
    }
    
    public void OnTorchLit(int sequenceNumber)
    {
        // Safety check: puzzle already complete
        if (puzzleComplete)
        {
            Debug.LogWarning($"[SequentialTorchManager] OnTorchLit called for torch {sequenceNumber}, but puzzle is already complete. Ignoring.");
            return;
        }

        // Safety check: torch already in lighting order (shouldn't happen)
        if (lightingOrder.Contains(sequenceNumber))
        {
            Debug.LogWarning($"[SequentialTorchManager] Torch {sequenceNumber} already in lighting order! Ignoring duplicate.");
            return;
        }

        // Add torch to lighting order
        lightingOrder.Add(sequenceNumber);
        currentSequenceIndex = lightingOrder.Count;

        Debug.Log($"[SequentialTorchManager] ✓ Torch {sequenceNumber} lit! Order so far: [{string.Join("→", lightingOrder)}] ({lightingOrder.Count}/{torches.Length})");

        // Find the torch that was lit
        SequentialTorch litTorch = null;
        foreach (var torch in torches)
        {
            if (torch != null && torch.SequenceNumber == sequenceNumber)
            {
                litTorch = torch;
                break;
            }
        }

        if (litTorch == null)
        {
            Debug.LogError($"[SequentialTorchManager] Could not find torch with sequence number {sequenceNumber}");
            return;
        }

        // Verify the torch is actually lit (double-check state consistency)
        if (!litTorch.IsLit)
        {
            Debug.LogError($"[SequentialTorchManager] Torch {sequenceNumber} reported as lit, but IsLit flag is false! State inconsistency detected.");
            return;
        }

        // Create connection line from previously lit torch (if this isn't the first torch)
        if (lightingOrder.Count > 1)
        {
            // Find the previous torch in lighting order (not sequence order)
            int previousSequenceNumber = lightingOrder[lightingOrder.Count - 2];
            SequentialTorch previousTorch = null;
            foreach (var torch in torches)
            {
                if (torch != null && torch.SequenceNumber == previousSequenceNumber)
                {
                    previousTorch = torch;
                    break;
                }
            }

            if (previousTorch != null && previousTorch.IsLit)
            {
                litTorch.CreateConnectionLine(previousTorch);
                Debug.Log($"[SequentialTorchManager] Created connection line from torch {previousSequenceNumber} to torch {sequenceNumber}");
            }
            else
            {
                Debug.LogWarning($"[SequentialTorchManager] Could not create connection line: previous torch ({previousSequenceNumber}) is {(previousTorch == null ? "null" : "not lit")}");
            }
        }

        // Check if all torches are now lit
        if (lightingOrder.Count >= torches.Length)
        {
            Debug.Log($"[SequentialTorchManager] All {torches.Length} torches lit! Validating sequence...");
            ValidateFinalSequence();
        }
    }

    void ValidateFinalSequence()
    {
        // Build the correct sequence [1, 2, 3, 4, 5, ...]
        List<int> correctSequence = new List<int>();
        for (int i = 1; i <= torches.Length; i++)
        {
            correctSequence.Add(i);
        }

        // Compare lighting order with correct sequence
        bool isCorrect = true;
        if (lightingOrder.Count != correctSequence.Count)
        {
            isCorrect = false;
        }
        else
        {
            for (int i = 0; i < lightingOrder.Count; i++)
            {
                if (lightingOrder[i] != correctSequence[i])
                {
                    isCorrect = false;
                    break;
                }
            }
        }

        // Display results
        string actualOrder = string.Join("→", lightingOrder);
        string expectedOrder = string.Join("→", correctSequence);

        if (isCorrect)
        {
            Debug.Log($"[SequentialTorchManager] ✓✓✓ CORRECT SEQUENCE! [{actualOrder}] matches expected [{expectedOrder}]");
            CompletePuzzle();
        }
        else
        {
            Debug.LogWarning($"[SequentialTorchManager] ❌❌❌ WRONG SEQUENCE! Lit: [{actualOrder}], Expected: [{expectedOrder}]. Resetting puzzle...");
            StartCoroutine(ResetAfterDelay());
        }
    }

    void CompletePuzzle()
    {
        // Guard against completing puzzle multiple times
        if (puzzleComplete)
        {
            Debug.LogWarning("[SequentialTorchManager] CompletePuzzle() called, but puzzle is already completed! Ignoring.");
            return;
        }

        // Verify all torches are actually lit (sanity check)
        int litCount = 0;
        foreach (var torch in torches)
        {
            if (torch != null && torch.IsLit)
            {
                litCount++;
            }
        }

        if (litCount != torches.Length)
        {
            Debug.LogError($"[SequentialTorchManager] CRITICAL ERROR: CompletePuzzle() called, but only {litCount}/{torches.Length} torches are lit! Aborting completion.");
            return;
        }

        Debug.Log("[SequentialTorchManager] 🎉🎉🎉 PUZZLE COMPLETED! 🎉🎉🎉");

        // Mark puzzle as complete
        puzzleComplete = true;

        // Unlock all portals across all scenes
        PortalManager.UnlockAllPortals();

        // Play completion sound
        if (puzzleCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(puzzleCompleteSound);
            Debug.Log("[SequentialTorchManager] ♫ Playing completion sound");
        }

        // Play completion particles
        if (completionParticles != null)
        {
            completionParticles.Play();
            Debug.Log("[SequentialTorchManager] ✨ Playing completion particles");
        }

        // Turn on completion light
        if (completionLight != null)
        {
            completionLight.enabled = true;
            Debug.Log("[SequentialTorchManager] 💡 Enabled completion light");
        }

        // Unlock door
        if (rewardDoor != null)
        {
            rewardDoor.UnlockDoor();
            Debug.Log("[SequentialTorchManager] 🚪 Unlocked reward door");
        }

        // Spawn reward items
        if (rewardItems != null && rewardItems.Length > 0)
        {
            if (rewardSpawnPoint != null)
            {
                int spawnedCount = 0;
                foreach (var item in rewardItems)
                {
                    if (item != null)
                    {
                        Instantiate(item, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
                        spawnedCount++;
                    }
                    else
                    {
                        Debug.LogWarning("[SequentialTorchManager] Null reward item in array, skipping");
                    }
                }
                Debug.Log($"[SequentialTorchManager] 📦 Spawned {spawnedCount} reward items at {rewardSpawnPoint.name}");
            }
            else
            {
                Debug.LogWarning("[SequentialTorchManager] Reward items assigned, but no reward spawn point! Cannot spawn items.");
            }
        }

        // Enable/activate reward GameObjects
        if (rewardGameObjects != null && rewardGameObjects.Length > 0)
        {
            int enabledCount = 0;
            foreach (var rewardObj in rewardGameObjects)
            {
                if (rewardObj != null)
                {
                    bool wasActive = rewardObj.activeInHierarchy;
                    rewardObj.SetActive(true);
                    enabledCount++;
                    Debug.Log($"[SequentialTorchManager] ✓ Enabled reward GameObject: '{rewardObj.name}' (was {(wasActive ? "active" : "inactive")})");
                }
                else
                {
                    Debug.LogWarning("[SequentialTorchManager] Null reward GameObject in array, skipping");
                }
            }
            Debug.Log($"[SequentialTorchManager] Enabled {enabledCount} reward GameObjects");
        }

        // Make reward GameObjects visible (from invisible to visible)
        if (rewardVisibleObjects != null && rewardVisibleObjects.Length > 0)
        {
            int visibleCount = 0;
            foreach (var visibleObj in rewardVisibleObjects)
            {
                if (visibleObj != null)
                {
                    // Enable the GameObject if it's disabled
                    bool wasActive = visibleObj.activeInHierarchy;
                    if (!wasActive)
                    {
                        visibleObj.SetActive(true);
                        Debug.Log($"[SequentialTorchManager] Activated previously inactive GameObject: '{visibleObj.name}'");
                    }

                    // Make sure all renderers are enabled (make visible)
                    Renderer[] renderers = visibleObj.GetComponentsInChildren<Renderer>(true); // Include inactive children
                    int enabledRenderers = 0;
                    foreach (var renderer in renderers)
                    {
                        if (renderer != null)
                        {
                            renderer.enabled = true;
                            enabledRenderers++;
                        }
                    }

                    visibleCount++;
                    Debug.Log($"[SequentialTorchManager] ✓ Made visible reward GameObject: '{visibleObj.name}' ({enabledRenderers} renderers enabled)");
                }
                else
                {
                    Debug.LogWarning("[SequentialTorchManager] Null reward visible GameObject in array, skipping");
                }
            }
            Debug.Log($"[SequentialTorchManager] 👁 Made {visibleCount} GameObjects visible");
        }

        // Start celebration coroutine
        StartCoroutine(CelebrationSequence());

        // Trigger ending-specific behavior for Kuchisake Onna
        TriggerEndingBehavior();
    }

    /// <summary>
    /// Triggers ending-specific behavior for Kuchisake Onna based on endingType
    /// </summary>
    void TriggerEndingBehavior()
    {
        // Find Kuchisake Onna in the scene
        KuchisakeOnnaController enemy = FindObjectOfType<KuchisakeOnnaController>();

        if (enemy == null)
        {
            Debug.LogWarning("[SequentialTorchManager] No KuchisakeOnnaController found in scene. Ending behavior skipped.");
            return;
        }

        // Trigger behavior based on ending type
        switch (endingType)
        {
            case EndingType.None:
                Debug.Log("[SequentialTorchManager] EndingType is None. No special enemy behavior triggered.");
                break;

            case EndingType.GoodEnding:
                Debug.Log("[SequentialTorchManager] GOOD ENDING: Making Kuchisake Onna disappear...");
                enemy.DisappearEnemy();
                break;

            case EndingType.BadEnding:
                Debug.Log("[SequentialTorchManager] BAD ENDING: Enraging Kuchisake Onna...");
                enemy.EnrageEnemy();
                break;
        }
    }
    
    IEnumerator CelebrationSequence()
    {
        // Store original intensities for all lit torches
        Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
        for (int i = 0; i < currentSequenceIndex; i++)
        {
            if (torches[i].torchLight)
            {
                originalIntensities[torches[i].torchLight] = torches[i].torchLight.intensity;
            }
        }

        // Flash all lit torches
        for (int i = 0; i < 3; i++)
        {
            // Brighten torches
            for (int j = 0; j < currentSequenceIndex; j++)
            {
                if (torches[j].torchLight && originalIntensities.ContainsKey(torches[j].torchLight))
                {
                    torches[j].torchLight.intensity = originalIntensities[torches[j].torchLight] * 1.5f;
                }
            }

            yield return new WaitForSeconds(0.3f);

            // Return to normal
            for (int j = 0; j < currentSequenceIndex; j++)
            {
                if (torches[j].torchLight && originalIntensities.ContainsKey(torches[j].torchLight))
                {
                    torches[j].torchLight.intensity = originalIntensities[torches[j].torchLight];
                }
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
    
    public void ResetPuzzle()
    {
        Debug.Log("[SequentialTorchManager] 🔄 Resetting puzzle to initial state...");

        // Play reset sound
        if (puzzleResetSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(puzzleResetSound);
        }

        // Extinguish and reset all torches with validation
        int extinguishedCount = 0;
        if (torches != null && torches.Length > 0)
        {
            foreach (var torch in torches)
            {
                if (torch != null)
                {
                    torch.ExtinguishTorch();
                    extinguishedCount++;
                }
                else
                {
                    Debug.LogWarning("[SequentialTorchManager] Null torch found in torches array during reset");
                }
            }
            Debug.Log($"[SequentialTorchManager] Extinguished {extinguishedCount}/{torches.Length} torches");
        }
        else
        {
            Debug.LogError("[SequentialTorchManager] Cannot reset - torches array is null or empty!");
        }

        // Reset state variables
        currentSequenceIndex = 0;
        puzzleComplete = false;

        // Clear lighting order
        lightingOrder.Clear();
        Debug.Log("[SequentialTorchManager] Cleared lighting order history. All torches are now interactable.");

        // Hide completion effects
        if (completionParticles != null && completionParticles.isPlaying)
        {
            completionParticles.Stop();
            Debug.Log("[SequentialTorchManager] Stopped completion particles");
        }

        if (completionLight != null)
        {
            completionLight.enabled = false;
            Debug.Log("[SequentialTorchManager] Disabled completion light");
        }

        Debug.Log("[SequentialTorchManager] ✓ Puzzle reset complete");
    }
    
    public void OnWrongSequenceAttempted(int sequenceNumber)
    {
        // Check if reset on wrong sequence is enabled
        if (!resetOnWrongSequence)
        {
            Debug.LogWarning($"[SequentialTorchManager] Wrong sequence attempted on torch {sequenceNumber}, but reset is disabled. Ignoring.");
            return;
        }

        // Prevent multiple simultaneous reset attempts
        if (isResetting)
        {
            Debug.LogWarning($"[SequentialTorchManager] Reset already in progress. Ignoring repeated wrong sequence attempt.");
            return;
        }

        // Calculate what the expected torch was
        int expectedSequenceNumber = currentSequenceIndex + 1;

        Debug.LogWarning($"[SequentialTorchManager] ❌ WRONG SEQUENCE! Player attempted torch {sequenceNumber}, but expected torch {expectedSequenceNumber}. Resetting puzzle in {resetDelay} seconds...");

        // Start reset coroutine
        StartCoroutine(ResetAfterDelay());
    }

    IEnumerator ResetAfterDelay()
    {
        isResetting = true;

        Debug.Log($"[SequentialTorchManager] Waiting {resetDelay} seconds before reset...");
        yield return new WaitForSeconds(resetDelay);

        ResetPuzzle();

        isResetting = false;
    }
    
    // Public getters for other scripts
    public bool IsPuzzleComplete => puzzleComplete;
    public int CurrentSequenceIndex => currentSequenceIndex;
    public int TotalTorches => torches.Length;
    public int LitTorchesCount => currentSequenceIndex;
    
    // Debug methods
    [ContextMenu("Reset Puzzle")]
    public void DebugResetPuzzle()
    {
        ResetPuzzle();
    }
    
    [ContextMenu("Complete Puzzle")]
    public void DebugCompletePuzzle()
    {
        Debug.Log("[SequentialTorchManager] DEBUG: Force completing puzzle...");

        // Light all torches visually without triggering events
        foreach (var torch in torches)
        {
            if (!torch.IsLit)
            {
                torch.SetReadyToLight(true);
                // Get the torch reference and set its state directly
                torch.LightTorch();
            }
        }

        // Set state directly to match all torches being lit
        currentSequenceIndex = torches.Length;

        // Complete the puzzle
        if (!puzzleComplete)
        {
            CompletePuzzle();
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (torches == null) return;
        
        // Draw connections between torches
        Gizmos.color = Color.blue;
        for (int i = 0; i < torches.Length - 1; i++)
        {
            if (torches[i] && torches[i + 1])
            {
                Gizmos.DrawLine(torches[i].transform.position, torches[i + 1].transform.position);
            }
        }
        
        // Draw reward spawn point
        if (rewardSpawnPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rewardSpawnPoint.position, 1f);
        }
    }
}
