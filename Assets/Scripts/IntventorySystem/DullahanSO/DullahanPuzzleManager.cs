using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DullahanPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Components")]
    public DullahanHeadPickable[] headPickables;
    public DullahanBody dullahanBody;
    public DullahanChaseSystem dullahanChase;
    public Door finalDoor;
    
    [Header("Puzzle Settings")]
    public bool puzzleActive = true;
    public bool puzzleCompleted = false;
    public float puzzleStartDelay = 5f;
    
    [Header("Head Spawn Settings")]
    public Transform[] headSpawnPoints;
    public GameObject[] headPrefabs;
    public DullahanHeadSO[] headData;
    
    [Header("Audio")]
    public DullahanAudioManager audioManager;
    
    [Header("UI")]
    public GameObject puzzleUI;
    public TMPro.TextMeshProUGUI puzzleText;
    public string startText = "Find the Dullahan's real head and return it to his body";
    public string progressText = "Heads found: {0}/3";
    public string completeText = "Puzzle completed! The door is unlocked!";
    
    [Header("Effects")]
    public ParticleSystem puzzleStartEffect;
    public ParticleSystem puzzleCompleteEffect;
    public Light[] puzzleLights;
    
    private List<DullahanHeadPickable> spawnedHeads = new List<DullahanHeadPickable>();
    private int headsFound = 0;
    private bool puzzleStarted = false;
    
    void Start()
    {
        // Setup puzzle components
        SetupPuzzleComponents();
        
        // Start puzzle after delay
        StartCoroutine(StartPuzzleAfterDelay());
    }
    
    void Update()
    {
        if (!puzzleActive || puzzleCompleted) return;
        
        UpdatePuzzleProgress();
        CheckPuzzleCompletion();
    }
    
    void SetupPuzzleComponents()
    {
        // Find components if not assigned
        if (dullahanChase == null)
            dullahanChase = FindObjectOfType<DullahanChaseSystem>();
            
        if (dullahanBody == null)
            dullahanBody = FindObjectOfType<DullahanBody>();
            
        if (finalDoor == null)
            finalDoor = FindObjectOfType<Door>();
            
        // Setup body reference to final door
        if (dullahanBody != null && finalDoor != null)
        {
            dullahanBody.SetFinalDoor(finalDoor);
        }
        
        // Find audio manager
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
    }
    
    IEnumerator StartPuzzleAfterDelay()
    {
        yield return new WaitForSeconds(puzzleStartDelay);
        
        StartPuzzle();
    }
    
    void StartPuzzle()
    {
        if (puzzleStarted) return;
        
        puzzleStarted = true;
        puzzleActive = true;
        
        // Spawn heads
        SpawnHeads();
        
        // Start puzzle ambient audio
        if (audioManager != null)
        {
            audioManager.StartPuzzleAmbient();
        }
        
        // Play start effect
        if (puzzleStartEffect != null)
            puzzleStartEffect.Play();
        
        // Show start message
        ShowPuzzleMessage(startText);
        
        // Activate puzzle lights
        ActivatePuzzleLights();
        
        Debug.Log("Dullahan puzzle started!");
    }
    
    void SpawnHeads()
    {
        if (headSpawnPoints.Length == 0 || headPrefabs.Length == 0 || headData.Length == 0) return;
        
        // Clear existing heads
        foreach (DullahanHeadPickable head in spawnedHeads)
        {
            if (head != null)
                Destroy(head.gameObject);
        }
        spawnedHeads.Clear();
        
        // Spawn new heads
        for (int i = 0; i < Mathf.Min(headSpawnPoints.Length, headPrefabs.Length, headData.Length); i++)
        {
            Transform spawnPoint = headSpawnPoints[i];
            GameObject headPrefab = headPrefabs[i];
            DullahanHeadSO headDataItem = headData[i];
            
            if (spawnPoint != null && headPrefab != null && headDataItem != null)
            {
                GameObject headObj = Instantiate(headPrefab, spawnPoint.position, spawnPoint.rotation);
                DullahanHeadPickable headPickable = headObj.GetComponent<DullahanHeadPickable>();
                
                if (headPickable != null)
                {
                    headPickable.headData = headDataItem;
                    spawnedHeads.Add(headPickable);
                }
            }
        }
        
        Debug.Log($"Spawned {spawnedHeads.Count} heads");
    }
    
    void UpdatePuzzleProgress()
    {
        int newHeadsFound = 0;
        
        // Count picked up heads
        foreach (DullahanHeadPickable head in spawnedHeads)
        {
            if (head != null && head.isPickedUp)
            {
                newHeadsFound++;
            }
        }
        
        // Update progress if changed
        if (newHeadsFound != headsFound)
        {
            headsFound = newHeadsFound;
            ShowPuzzleMessage(string.Format(progressText, headsFound));
            
            // Play pickup sound
            if (headsFound > 0)
            {
                // Could play different sounds for different heads
                Debug.Log($"Head {headsFound} picked up!");
            }
        }
    }
    
    void CheckPuzzleCompletion()
    {
        if (dullahanBody != null && dullahanBody.HasHead())
        {
            CompletePuzzle();
        }
    }
    
    void CompletePuzzle()
    {
        if (puzzleCompleted) return;
        
        puzzleCompleted = true;
        puzzleActive = false;
        
        // Play completion sound
        if (audioManager != null)
        {
            audioManager.PlayPuzzleCompleteSound();
            audioManager.StopPuzzleAmbient();
        }
        
        // Play completion effect
        if (puzzleCompleteEffect != null)
            puzzleCompleteEffect.Play();
        
        // Show completion message
        ShowPuzzleMessage(completeText);
        
        // Deactivate puzzle lights
        DeactivatePuzzleLights();
        
        // Stop Dullahan chase
        if (dullahanChase != null)
        {
            // Could implement a method to stop the chase
            Debug.Log("Dullahan chase should stop now");
        }
        
        Debug.Log("Dullahan puzzle completed!");
    }
    
    void ShowPuzzleMessage(string message)
    {
        if (puzzleUI != null && puzzleText != null)
        {
            puzzleText.text = message;
            puzzleUI.SetActive(true);
            
            // Hide after 5 seconds
            StartCoroutine(HidePuzzleMessage(5f));
        }
    }
    
    IEnumerator HidePuzzleMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (puzzleUI != null)
            puzzleUI.SetActive(false);
    }
    
    void ActivatePuzzleLights()
    {
        if (puzzleLights == null) return;
        
        foreach (Light light in puzzleLights)
        {
            if (light != null)
            {
                light.enabled = true;
                StartCoroutine(PulseLight(light));
            }
        }
    }
    
    void DeactivatePuzzleLights()
    {
        if (puzzleLights == null) return;
        
        foreach (Light light in puzzleLights)
        {
            if (light != null)
            {
                light.enabled = false;
            }
        }
    }
    
    IEnumerator PulseLight(Light light)
    {
        if (light == null) yield break;
        
        float originalIntensity = light.intensity;
        
        while (puzzleActive && !puzzleCompleted)
        {
            float pulse = Mathf.Sin(Time.time * 1.5f) * 0.3f + 1f;
            light.intensity = originalIntensity * pulse;
            yield return null;
        }
    }
    
    // Public methods for external access
    public bool IsPuzzleActive()
    {
        return puzzleActive;
    }
    
    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }
    
    public int GetHeadsFound()
    {
        return headsFound;
    }
    
    public int GetTotalHeads()
    {
        return spawnedHeads.Count;
    }
    
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        puzzleActive = true;
        headsFound = 0;
        puzzleStarted = false;
        
        // Respawn heads
        SpawnHeads();
        
        // Reset body
        if (dullahanBody != null)
        {
            // Would need to implement reset method in DullahanBody
            Debug.Log("Reset Dullahan body");
        }
        
        // Restart puzzle
        StartCoroutine(StartPuzzleAfterDelay());
    }
    
    public void SetPuzzleActive(bool active)
    {
        puzzleActive = active;
        
        if (!active)
        {
            DeactivatePuzzleLights();
        }
        else if (puzzleStarted)
        {
            ActivatePuzzleLights();
        }
    }
}
