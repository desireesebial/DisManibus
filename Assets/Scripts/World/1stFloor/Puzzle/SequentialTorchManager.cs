using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    
    // State
    private int currentSequenceIndex = 0;
    private bool puzzleComplete = false;
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

        // Make first torch ready
        if (torches.Length > 0)
        {
            torches[0].SetReadyToLight(true);
            currentSequenceIndex = 0;
        }

        // Reset state
        puzzleComplete = false;

        // Hide completion effects
        if (completionParticles && completionParticles.isPlaying)
            completionParticles.Stop();

        if (completionLight)
            completionLight.enabled = false;
    }
    
    public bool CanLightTorch(int sequenceNumber)
    {
        if (puzzleComplete) return false;
        
        // Check if this is the next torch in sequence
        return sequenceNumber == currentSequenceIndex + 1;
    }
    
    public void OnTorchLit(int sequenceNumber)
    {
        if (puzzleComplete) return;
        
        Debug.Log($"[SequentialTorchManager] Torch {sequenceNumber} lit!");
        
        // Find the torch that was lit
        SequentialTorch litTorch = null;
        foreach (var torch in torches)
        {
            if (torch.SequenceNumber == sequenceNumber)
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

        // Update progress
        currentSequenceIndex++;
        
        // Check if puzzle is complete
        if (currentSequenceIndex >= torches.Length)
        {
            CompletePuzzle();
        }
        else
        {
            // Make next torch ready
            if (currentSequenceIndex < torches.Length)
            {
                torches[currentSequenceIndex].SetReadyToLight(true);
                Debug.Log($"[SequentialTorchManager] Torch {torches[currentSequenceIndex].SequenceNumber} is now ready to light");
            }
        }
    }
    
    void CompletePuzzle()
    {
        // Guard against completing puzzle multiple times
        if (puzzleComplete)
        {
            Debug.LogWarning("[SequentialTorchManager] Puzzle already completed!");
            return;
        }

        Debug.Log("[SequentialTorchManager] 🎉 PUZZLE COMPLETED!");

        puzzleComplete = true;
        
        // Play completion sound
        if (puzzleCompleteSound) audioSource.PlayOneShot(puzzleCompleteSound);
        
        // Play completion particles
        if (completionParticles) completionParticles.Play();
        
        // Turn on completion light
        if (completionLight) completionLight.enabled = true;
        
        // Unlock door
        if (rewardDoor) rewardDoor.UnlockDoor();
        
        // Spawn reward items
        if (rewardItems != null && rewardSpawnPoint)
        {
            foreach (var item in rewardItems)
            {
                if (item)
                {
                    Instantiate(item, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
                }
            }
        }
        
        // Enable/activate reward GameObjects
        if (rewardGameObjects != null)
        {
            foreach (var rewardObj in rewardGameObjects)
            {
                if (rewardObj)
                {
                    rewardObj.SetActive(true);
                    Debug.Log($"[SequentialTorchManager] Enabled reward GameObject: {rewardObj.name}");
                }
            }
        }
        
        // Make reward GameObjects visible
        if (rewardVisibleObjects != null)
        {
            foreach (var visibleObj in rewardVisibleObjects)
            {
                if (visibleObj)
                {
                    // Enable the GameObject if it's disabled
                    if (!visibleObj.activeInHierarchy)
                    {
                        visibleObj.SetActive(true);
                    }
                    
                    // Make sure it's visible (in case it was hidden by other means)
                    Renderer[] renderers = visibleObj.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        if (renderer)
                        {
                            renderer.enabled = true;
                        }
                    }
                    
                    Debug.Log($"[SequentialTorchManager] Made visible reward GameObject: {visibleObj.name}");
                }
            }
        }
        
        // Start celebration coroutine
        StartCoroutine(CelebrationSequence());
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
        Debug.Log("[SequentialTorchManager] Resetting puzzle...");

        // Play reset sound
        if (puzzleResetSound) audioSource.PlayOneShot(puzzleResetSound);

        // Extinguish and reset all torches
        foreach (var torch in torches)
        {
            if (torch != null)
            {
                torch.ExtinguishTorch();
            }
        }

        // Reset state
        currentSequenceIndex = 0;
        puzzleComplete = false;

        // Make first torch ready again
        if (torches.Length > 0)
        {
            torches[0].SetReadyToLight(true);
        }

        // Hide completion effects
        if (completionParticles && completionParticles.isPlaying)
            completionParticles.Stop();

        if (completionLight)
            completionLight.enabled = false;
    }
    
    public void OnWrongSequenceAttempted(int sequenceNumber)
    {
        if (!resetOnWrongSequence) return;
        
        Debug.Log($"[SequentialTorchManager] Wrong sequence attempted on torch {sequenceNumber} - resetting puzzle");
        
        // Start reset coroutine
        StartCoroutine(ResetAfterDelay());
    }
    
    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetPuzzle();
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
