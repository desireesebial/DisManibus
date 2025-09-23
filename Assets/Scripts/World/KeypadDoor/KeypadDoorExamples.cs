using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example implementations and integration patterns for KeypadDoorController.
/// This script demonstrates various ways to use and extend the keypad system.
/// </summary>
public class KeypadDoorExamples : MonoBehaviour
{
    [Header("Example Configurations")]
    [SerializeField] private KeypadDoorController[] keypads;
    
    [Header("Dynamic Code Examples")]
    [SerializeField] private string[] possibleCodes = {"1234", "5678", "9876", "0000"};
    [SerializeField] private bool randomizeCodeOnStart = false;
    
    [Header("Multi-Door Examples")]
    [SerializeField] private doorscript[] linkedDoors;
    [SerializeField] private KeypadDoorController masterKeypad;
    
    [Header("Puzzle Integration")]
    [SerializeField] private bool requirePuzzleCompletion = false;
    [SerializeField] private string puzzleCompletionTag = "PuzzleComplete";

    void Start()
    {
        SetupExamples();
    }

    void SetupExamples()
    {
        // Example 1: Randomize keypad codes
        if (randomizeCodeOnStart)
        {
            RandomizeKeypads();
        }

        // Example 2: Setup event listeners
        SetupEventListeners();

        // Example 3: Setup multi-door system
        SetupMultiDoorSystem();
    }

    #region Example 1: Dynamic Code Management

    /// <summary>
    /// Randomize keypad codes from a predefined list
    /// </summary>
    public void RandomizeKeypads()
    {
        foreach (var keypad in keypads)
        {
            if (keypad != null && possibleCodes.Length > 0)
            {
                string randomCode = possibleCodes[Random.Range(0, possibleCodes.Length)];
                keypad.SetCode(randomCode);
                Debug.Log($"[KeypadExamples] Set {keypad.name} code to: {randomCode}");
            }
        }
    }

    /// <summary>
    /// Set a specific keypad's code
    /// </summary>
    public void SetKeypadCode(int keypadIndex, string newCode)
    {
        if (keypadIndex >= 0 && keypadIndex < keypads.Length && keypads[keypadIndex] != null)
        {
            keypads[keypadIndex].SetCode(newCode);
            Debug.Log($"[KeypadExamples] Updated keypad {keypadIndex} code to: {newCode}");
        }
    }

    /// <summary>
    /// Generate a random 4-digit code
    /// </summary>
    public string GenerateRandomCode(int length = 4)
    {
        string code = "";
        for (int i = 0; i < length; i++)
        {
            code += Random.Range(0, 10).ToString();
        }
        return code;
    }

    #endregion

    #region Example 2: Event System Integration

    /// <summary>
    /// Setup event listeners for keypad interactions
    /// </summary>
    void SetupEventListeners()
    {
        foreach (var keypad in keypads)
        {
            if (keypad != null)
            {
                // Add listeners for correct code
                keypad.onCorrectCode.AddListener(() => OnCorrectCode(keypad));
                
                // Add listeners for wrong code
                keypad.onWrongCode.AddListener(() => OnWrongCode(keypad));
            }
        }
    }

    void OnCorrectCode(KeypadDoorController keypad)
    {
        Debug.Log($"[KeypadExamples] Correct code entered on {keypad.name}!");
        
        // Example: Play success animation
        // PlaySuccessAnimation();
        
        // Example: Update game state
        // GameManager.Instance.OnKeypadSolved(keypad.name);
        
        // Example: Trigger other events
        // TriggerSecurityDisabled();
    }

    void OnWrongCode(KeypadDoorController keypad)
    {
        Debug.Log($"[KeypadExamples] Wrong code entered on {keypad.name}!");
        
        // Example: Increase security level
        // SecuritySystem.Instance.OnWrongCodeEntered();
        
        // Example: Play alarm sound
        // PlayAlarmSound();
        
        // Example: Track failed attempts
        // TrackFailedAttempt(keypad.name);
    }

    #endregion

    #region Example 3: Multi-Door System

    /// <summary>
    /// Setup a master keypad that controls multiple doors
    /// </summary>
    void SetupMultiDoorSystem()
    {
        if (masterKeypad != null)
        {
            masterKeypad.onCorrectCode.AddListener(OpenAllLinkedDoors);
        }
    }

    void OpenAllLinkedDoors()
    {
        Debug.Log("[KeypadExamples] Opening all linked doors...");
        
        foreach (var door in linkedDoors)
        {
            if (door != null)
            {
                door.UnlockDoor();
                door.OpenDoor();
                Debug.Log($"[KeypadExamples] Opened door: {door.name}");
            }
        }
    }

    #endregion

    #region Example 4: Puzzle Integration

    /// <summary>
    /// Check if puzzle requirements are met before allowing keypad use
    /// </summary>
    public bool IsPuzzleCompleted()
    {
        if (!requirePuzzleCompletion) return true;
        
        // Example: Check for puzzle completion object
        GameObject puzzleMarker = GameObject.FindGameObjectWithTag(puzzleCompletionTag);
        return puzzleMarker != null;
    }

    /// <summary>
    /// Custom keypad validation with puzzle requirement
    /// </summary>
    public void ValidateKeypadAccess(KeypadDoorController keypad)
    {
        if (!IsPuzzleCompleted())
        {
            Debug.Log("[KeypadExamples] Puzzle must be completed first!");
            // Show message to player
            // UIManager.Instance.ShowMessage("Complete the puzzle first!");
            return;
        }
        
        // Allow normal keypad operation
        Debug.Log("[KeypadExamples] Keypad access granted!");
    }

    #endregion

    #region Example 5: Advanced Features

    /// <summary>
    /// Create a timed keypad that changes codes periodically
    /// </summary>
    [System.Serializable]
    public class TimedKeypad
    {
        public KeypadDoorController keypad;
        public float changeInterval = 60f; // Change code every 60 seconds
        public string[] timedCodes = {"1111", "2222", "3333"};
        [System.NonSerialized] public float nextChangeTime;
        [System.NonSerialized] public int currentCodeIndex;
    }

    [SerializeField] private TimedKeypad[] timedKeypads;

    void Update()
    {
        // Handle timed keypads
        foreach (var timedKeypad in timedKeypads)
        {
            if (timedKeypad.keypad != null && Time.time >= timedKeypad.nextChangeTime)
            {
                ChangeTimedKeypadCode(timedKeypad);
            }
        }
    }

    void ChangeTimedKeypadCode(TimedKeypad timedKeypad)
    {
        if (timedKeypad.timedCodes.Length == 0) return;
        
        timedKeypad.currentCodeIndex = (timedKeypad.currentCodeIndex + 1) % timedKeypad.timedCodes.Length;
        string newCode = timedKeypad.timedCodes[timedKeypad.currentCodeIndex];
        
        timedKeypad.keypad.SetCode(newCode);
        timedKeypad.nextChangeTime = Time.time + timedKeypad.changeInterval;
        
        Debug.Log($"[KeypadExamples] Timed keypad code changed to: {newCode}");
    }

    #endregion

    #region Example 6: Save/Load Integration

    /// <summary>
    /// Save keypad states to PlayerPrefs
    /// </summary>
    public void SaveKeypadStates()
    {
        for (int i = 0; i < keypads.Length; i++)
        {
            if (keypads[i] != null)
            {
                string keypadKey = $"Keypad_{i}_Unlocked";
                bool isUnlocked = keypads[i].targetDoor != null && !keypads[i].targetDoor.IsLocked();
                PlayerPrefs.SetInt(keypadKey, isUnlocked ? 1 : 0);
            }
        }
        PlayerPrefs.Save();
        Debug.Log("[KeypadExamples] Keypad states saved!");
    }

    /// <summary>
    /// Load keypad states from PlayerPrefs
    /// </summary>
    public void LoadKeypadStates()
    {
        for (int i = 0; i < keypads.Length; i++)
        {
            if (keypads[i] != null && keypads[i].targetDoor != null)
            {
                string keypadKey = $"Keypad_{i}_Unlocked";
                bool wasUnlocked = PlayerPrefs.GetInt(keypadKey, 0) == 1;
                
                if (wasUnlocked)
                {
                    keypads[i].targetDoor.ForceUnlock();
                    Debug.Log($"[KeypadExamples] Restored unlocked state for keypad {i}");
                }
            }
        }
        Debug.Log("[KeypadExamples] Keypad states loaded!");
    }

    #endregion

    #region Context Menu Methods (for testing in editor)

    [ContextMenu("Randomize All Codes")]
    void TestRandomizeCodes()
    {
        RandomizeKeypads();
    }

    [ContextMenu("Open All Doors")]
    void TestOpenAllDoors()
    {
        OpenAllLinkedDoors();
    }

    [ContextMenu("Save Keypad States")]
    void TestSaveStates()
    {
        SaveKeypadStates();
    }

    [ContextMenu("Load Keypad States")]
    void TestLoadStates()
    {
        LoadKeypadStates();
    }

    #endregion
}
