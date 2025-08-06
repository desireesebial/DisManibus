using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public List<CombinationPadlock> padlocks = new List<CombinationPadlock>();
    public bool requireAllPadlocks = false;
    public bool sequentialUnlocking = false;
    
    [Header("Puzzle Progression")]
    public int currentPuzzleIndex = 0;
    public bool puzzleComplete = false;
    
    [Header("Events")]
    public UnityEvent onAllPuzzlesComplete;
    public UnityEvent onPuzzleProgress;
    public UnityEvent onPuzzleFailed;
    
    [Header("Hints System")]
    public bool enableHints = true;
    public float hintDelay = 30f; // Seconds before showing hint
    public string[] puzzleHints;
    
    [Header("Audio")]
    public AudioClip puzzleCompleteSound;
    public AudioClip hintSound;
    
    private AudioSource audioSource;
    private Dictionary<CombinationPadlock, bool> padlockStatus = new Dictionary<CombinationPadlock, bool>();
    private float[] hintTimers;
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize padlock status tracking
        InitializePadlocks();
        
        // Setup hint timers
        if (enableHints && puzzleHints != null)
        {
            hintTimers = new float[puzzleHints.Length];
            for (int i = 0; i < hintTimers.Length; i++)
            {
                hintTimers[i] = hintDelay;
            }
        }
    }
    
    void Update()
    {
        // Update hint timers
        if (enableHints && !puzzleComplete)
        {
            UpdateHintTimers();
        }
        
        // Check puzzle completion
        CheckPuzzleCompletion();
    }
    
    void InitializePadlocks()
    {
        padlockStatus.Clear();
        
        foreach (var padlock in padlocks)
        {
            if (padlock != null)
            {
                padlockStatus[padlock] = false;
                
                // Subscribe to padlock unlock events
                padlock.onPadlockUnlocked.AddListener(() => OnPadlockUnlocked(padlock));
            }
        }
    }
    
    void OnPadlockUnlocked(CombinationPadlock padlock)
    {
        if (padlockStatus.ContainsKey(padlock))
        {
            padlockStatus[padlock] = true;
            Debug.Log($"Padlock unlocked: {padlock.name}");
            
            // Check if we need to unlock the next padlock in sequence
            if (sequentialUnlocking)
            {
                UnlockNextPadlock();
            }
            
            // Trigger progress event
            onPuzzleProgress?.Invoke();
        }
    }
    
    void UnlockNextPadlock()
    {
        // Find the next locked padlock
        for (int i = 0; i < padlocks.Count; i++)
        {
            if (padlocks[i] != null && !padlockStatus[padlocks[i]])
            {
                // This is the next padlock to unlock
                currentPuzzleIndex = i;
                break;
            }
        }
    }
    
    void CheckPuzzleCompletion()
    {
        if (puzzleComplete) return;
        
        bool allUnlocked = true;
        int unlockedCount = 0;
        
        foreach (var kvp in padlockStatus)
        {
            if (kvp.Value)
            {
                unlockedCount++;
            }
            else
            {
                allUnlocked = false;
            }
        }
        
        // Check completion conditions
        if (requireAllPadlocks && allUnlocked)
        {
            CompletePuzzle();
        }
        else if (!requireAllPadlocks && unlockedCount > 0)
        {
            // For non-sequential puzzles, complete when at least one is unlocked
            CompletePuzzle();
        }
    }
    
    void CompletePuzzle()
    {
        puzzleComplete = true;
        Debug.Log("All puzzles completed!");
        
        // Play completion sound
        if (audioSource != null && puzzleCompleteSound != null)
        {
            audioSource.PlayOneShot(puzzleCompleteSound);
        }
        
        // Trigger completion event
        onAllPuzzlesComplete?.Invoke();
    }
    
    void UpdateHintTimers()
    {
        for (int i = 0; i < hintTimers.Length; i++)
        {
            if (hintTimers[i] > 0)
            {
                hintTimers[i] -= Time.deltaTime;
                
                if (hintTimers[i] <= 0)
                {
                    ShowHint(i);
                }
            }
        }
    }
    
    void ShowHint(int hintIndex)
    {
        if (hintIndex < puzzleHints.Length && !string.IsNullOrEmpty(puzzleHints[hintIndex]))
        {
            Debug.Log($"Hint {hintIndex + 1}: {puzzleHints[hintIndex]}");
            
            // Play hint sound
            if (audioSource != null && hintSound != null)
            {
                audioSource.PlayOneShot(hintSound);
            }
            
            // You can extend this to show hints in UI
            // For now, it just logs to console
        }
    }
    
    // Public methods for external control
    
    public void ResetAllPuzzles()
    {
        puzzleComplete = false;
        currentPuzzleIndex = 0;
        
        foreach (var padlock in padlocks)
        {
            if (padlock != null)
            {
                padlock.ResetPadlock();
                padlockStatus[padlock] = false;
            }
        }
        
        // Reset hint timers
        if (hintTimers != null)
        {
            for (int i = 0; i < hintTimers.Length; i++)
            {
                hintTimers[i] = hintDelay;
            }
        }
    }
    
    public void UnlockAllPadlocks()
    {
        foreach (var padlock in padlocks)
        {
            if (padlock != null)
            {
                padlock.UnlockPadlock();
            }
        }
    }
    
    public void SetCombination(int padlockIndex, string combination)
    {
        if (padlockIndex >= 0 && padlockIndex < padlocks.Count && padlocks[padlockIndex] != null)
        {
            padlocks[padlockIndex].SetCombination(combination);
        }
    }
    
    public bool IsPadlockUnlocked(int padlockIndex)
    {
        if (padlockIndex >= 0 && padlockIndex < padlocks.Count && padlocks[padlockIndex] != null)
        {
            return padlockStatus[padlocks[padlockIndex]];
        }
        return false;
    }
    
    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var kvp in padlockStatus)
        {
            if (kvp.Value) count++;
        }
        return count;
    }
    
    public float GetProgressPercentage()
    {
        if (padlocks.Count == 0) return 0f;
        
        int unlocked = GetUnlockedCount();
        return (float)unlocked / padlocks.Count * 100f;
    }
    
    // Method to add a padlock dynamically
    public void AddPadlock(CombinationPadlock padlock)
    {
        if (padlock != null && !padlocks.Contains(padlock))
        {
            padlocks.Add(padlock);
            padlockStatus[padlock] = false;
            padlock.onPadlockUnlocked.AddListener(() => OnPadlockUnlocked(padlock));
        }
    }
    
    // Method to remove a padlock
    public void RemovePadlock(CombinationPadlock padlock)
    {
        if (padlock != null && padlocks.Contains(padlock))
        {
            padlocks.Remove(padlock);
            padlockStatus.Remove(padlock);
        }
    }
} 