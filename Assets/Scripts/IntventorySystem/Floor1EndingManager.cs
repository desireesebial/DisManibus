using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Floor1EndingManager : MonoBehaviour
{
    [Header("Ending Settings")]
    public bool endingTriggered = false;
    public Transform endingTriggerPoint; // Point where Floor 1 completion is detected
    
    [Header("Ending Scenes")]
    public string goodEndingSceneName = "GoodEnding";
    public string badEndingSceneName = "BadEnding";
    public string creditsSceneName = "Credits";
    
    [Header("Ending UI")]
    public GameObject goodEndingUI;
    public GameObject badEndingUI;
    public TextMeshProUGUI endingText;
    public Button continueButton;
    
    [Header("Ending Audio")]
    public AudioSource audioSource;
    public AudioClip goodEndingMusic;
    public AudioClip badEndingMusic;
    public AudioClip endingTransitionSound;
    
    [Header("Ending Text")]
    public string goodEndingText = "Good Ending: You helped Dullahan find peace. Your kindness has been rewarded.";
    public string badEndingText = "Bad Ending: You chose to abandon Dullahan. Your selfishness has consequences.";
    
    [Header("Integration")]
    public QuestSystem questSystem;
    public PlayerHealthSystem playerHealth;
    
    [Header("Debug")]
    public bool debugMode = false;
    public KeyCode triggerEndingKey = KeyCode.E;
    public KeyCode forceGoodEndingKey = KeyCode.G;
    public KeyCode forceBadEndingKey = KeyCode.B;
    
    private bool playerInEndingArea = false;
    private bool endingSequenceStarted = false;
    private Transform playerTransform;
    
    void Start()
    {
        InitializeEndingManager();
    }
    
    void Update()
    {
        if (endingTriggered || endingSequenceStarted) return;
        
        HandleEndingDetection();
        HandleDebugInput();
    }
    
    private void InitializeEndingManager()
    {
        // Find references if not assigned
        if (questSystem == null)
            questSystem = FindObjectOfType<QuestSystem>();
            
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealthSystem>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        
        // Setup ending trigger point if not assigned
        if (endingTriggerPoint == null)
        {
            endingTriggerPoint = transform;
        }
        
        // Setup audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // Setup UI
        if (goodEndingUI != null)
            goodEndingUI.SetActive(false);
        if (badEndingUI != null)
            badEndingUI.SetActive(false);
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        
        Debug.Log("Floor 1 Ending Manager initialized - Waiting for completion trigger");
    }
    
    private void HandleEndingDetection()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(endingTriggerPoint.position, playerTransform.position);
        bool wasInEndingArea = playerInEndingArea;
        playerInEndingArea = distanceToPlayer <= 3f; // Small trigger area
        
        // Player just entered ending area
        if (playerInEndingArea && !wasInEndingArea)
        {
            OnPlayerEnteredEndingArea();
        }
    }
    
    private void OnPlayerEnteredEndingArea()
    {
        if (endingSequenceStarted) return;
        
        Debug.Log("Player entered Floor 1 ending area - Checking Floor 2 choice");
        endingSequenceStarted = true;
        
        // Check the player's choice from Floor 2
        bool choseGoodEnding = Floor2EndingEventManager.GetEndingChoice();
        
        if (choseGoodEnding)
        {
            TriggerGoodEnding();
        }
        else
        {
            TriggerBadEnding();
        }
    }
    
    private void TriggerGoodEnding()
    {
        endingTriggered = true;
        
        Debug.Log("Triggering Good Ending - Player helped Dullahan in Floor 2");
        
        // Show good ending UI
        if (goodEndingUI != null)
            goodEndingUI.SetActive(true);
        if (endingText != null)
            endingText.text = goodEndingText;
        
        // Play good ending music
        if (audioSource != null && goodEndingMusic != null)
        {
            audioSource.clip = goodEndingMusic;
            audioSource.Play();
        }
        
        // Complete any relevant quests
        CompleteGoodEndingQuests();
        
        // Start ending sequence
        StartCoroutine(GoodEndingSequence());
    }
    
    private void TriggerBadEnding()
    {
        endingTriggered = true;
        
        Debug.Log("Triggering Bad Ending - Player abandoned Dullahan in Floor 2");
        
        // Show bad ending UI
        if (badEndingUI != null)
            badEndingUI.SetActive(true);
        if (endingText != null)
            endingText.text = badEndingText;
        
        // Play bad ending music
        if (audioSource != null && badEndingMusic != null)
        {
            audioSource.clip = badEndingMusic;
            audioSource.Play();
        }
        
        // Complete any relevant quests
        CompleteBadEndingQuests();
        
        // Start ending sequence
        StartCoroutine(BadEndingSequence());
    }
    
    private void CompleteGoodEndingQuests()
    {
        if (questSystem != null)
        {
            // Complete any active quests related to good ending
            List<Quest> activeQuests = questSystem.GetActiveQuests();
            foreach (Quest quest in activeQuests)
            {
                if (quest.questTitle.Contains("Help") || quest.questTitle.Contains("Dullahan"))
                {
                    questSystem.CompleteQuest(quest);
                }
            }
        }
    }
    
    private void CompleteBadEndingQuests()
    {
        if (questSystem != null)
        {
            // Complete any active quests related to bad ending
            List<Quest> activeQuests = questSystem.GetActiveQuests();
            foreach (Quest quest in activeQuests)
            {
                if (quest.questTitle.Contains("Escape") || quest.questTitle.Contains("Leave"))
                {
                    questSystem.CompleteQuest(quest);
                }
            }
        }
    }
    
    private IEnumerator GoodEndingSequence()
    {
        // Wait for player to read the ending text
        yield return new WaitForSeconds(5f);
        
        // Show continue button
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
        
        // Wait for player to click continue or auto-continue after delay
        yield return new WaitForSeconds(10f);
        
        // Load good ending scene
        LoadGoodEndingScene();
    }
    
    private IEnumerator BadEndingSequence()
    {
        // Wait for player to read the ending text
        yield return new WaitForSeconds(5f);
        
        // Show continue button
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
        
        // Wait for player to click continue or auto-continue after delay
        yield return new WaitForSeconds(10f);
        
        // Load bad ending scene
        LoadBadEndingScene();
    }
    
    private void OnContinueButtonClicked()
    {
        // Play transition sound
        if (audioSource != null && endingTransitionSound != null)
        {
            audioSource.PlayOneShot(endingTransitionSound);
        }
        
        // Load appropriate ending scene based on choice
        bool choseGoodEnding = Floor2EndingEventManager.GetEndingChoice();
        
        if (choseGoodEnding)
        {
            LoadGoodEndingScene();
        }
        else
        {
            LoadBadEndingScene();
        }
    }
    
    private void LoadGoodEndingScene()
    {
        Debug.Log("Loading Good Ending scene");
        
        if (!string.IsNullOrEmpty(goodEndingSceneName))
        {
            // Use SceneTransitionManager if available
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(goodEndingSceneName);
            }
            else
            {
                SceneManager.LoadScene(goodEndingSceneName);
            }
        }
        else
        {
            // Fallback to credits scene
            LoadCreditsScene();
        }
    }
    
    private void LoadBadEndingScene()
    {
        Debug.Log("Loading Bad Ending scene");
        
        if (!string.IsNullOrEmpty(badEndingSceneName))
        {
            // Use SceneTransitionManager if available
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(badEndingSceneName);
            }
            else
            {
                SceneManager.LoadScene(badEndingSceneName);
            }
        }
        else
        {
            // Fallback to credits scene
            LoadCreditsScene();
        }
    }
    
    private void LoadCreditsScene()
    {
        Debug.Log("Loading Credits scene");
        
        if (!string.IsNullOrEmpty(creditsSceneName))
        {
            // Use SceneTransitionManager if available
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(creditsSceneName);
            }
            else
            {
                SceneManager.LoadScene(creditsSceneName);
            }
        }
    }
    
    private void HandleDebugInput()
    {
        if (!debugMode) return;
        
        if (Input.GetKeyDown(triggerEndingKey))
        {
            Debug.Log("Debug: Triggering ending check");
            OnPlayerEnteredEndingArea();
        }
        
        if (Input.GetKeyDown(forceGoodEndingKey))
        {
            Debug.Log("Debug: Forcing good ending");
            Floor2EndingEventManager.ClearEndingChoice();
            Floor2EndingEventManager.GetEndingChoice(); // This will return false, so we need to set it manually
            PlayerPrefs.SetInt("Floor2EndingChoice", 1);
            PlayerPrefs.Save();
            TriggerGoodEnding();
        }
        
        if (Input.GetKeyDown(forceBadEndingKey))
        {
            Debug.Log("Debug: Forcing bad ending");
            Floor2EndingEventManager.ClearEndingChoice();
            Floor2EndingEventManager.GetEndingChoice(); // This will return false, so we need to set it manually
            PlayerPrefs.SetInt("Floor2EndingChoice", 0);
            PlayerPrefs.Save();
            TriggerBadEnding();
        }
    }
    
    // Public methods for external control
    public void ForceGoodEnding()
    {
        if (!endingTriggered)
        {
            PlayerPrefs.SetInt("Floor2EndingChoice", 1);
            PlayerPrefs.Save();
            TriggerGoodEnding();
        }
    }
    
    public void ForceBadEnding()
    {
        if (!endingTriggered)
        {
            PlayerPrefs.SetInt("Floor2EndingChoice", 0);
            PlayerPrefs.Save();
            TriggerBadEnding();
        }
    }
    
    public bool IsEndingTriggered()
    {
        return endingTriggered;
    }
    
    public bool GetCurrentEndingChoice()
    {
        return Floor2EndingEventManager.GetEndingChoice();
    }
    
    // Method to reset ending state (useful for testing)
    public void ResetEnding()
    {
        endingTriggered = false;
        endingSequenceStarted = false;
        playerInEndingArea = false;
        
        // Hide UI
        if (goodEndingUI != null)
            goodEndingUI.SetActive(false);
        if (badEndingUI != null)
            badEndingUI.SetActive(false);
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
        
        Debug.Log("Floor 1 ending state reset");
    }
}
