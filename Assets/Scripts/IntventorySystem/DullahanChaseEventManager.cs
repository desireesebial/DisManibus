using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DullahanChaseEventManager : MonoBehaviour
{
    [Header("Proximity Settings")]
    public float proximityRadius = 10f;
    public Transform proximityCenter; // Center point for proximity detection
    public bool playerInProximity = false;
    public bool proximityTriggered = false;
    
    [Header("Chase Settings")]
    public float chaseDuration = 60f; // 60 seconds chase
    public float headSpawnDelay = 2f; // Delay before heads spawn after chase
    public float headCollectionTime = 90f; // 90 seconds to collect heads
    
    [Header("Doors")]
    public doorscript[] exitDoors; // Multiple exit doors that open together after chase
    public doorscript realHeadDoor; // Door to real head room
    public int exitDoorKeyID = 999;
    public int realHeadDoorKeyID = 100;
    public bool exitDoorsLinked = true; // Whether exit doors should open simultaneously
    
    [Header("Player Choice")]
    public bool playerChoseToHelp = false;
    public bool choiceMade = false;
    public float choiceTimeLimit = 30f; // Time limit for player choice
    private float choiceTimer = 0f;
    
    [Header("UI Elements")]
    public GameObject proximityUI; // UI shown when player enters proximity
    public GameObject choiceUI; // UI for player choice
    public GameObject chaseUI; // UI shown during chase
    public GameObject headCollectionUI; // UI shown during head collection
    public TextMeshProUGUI proximityText;
    public TextMeshProUGUI choiceText;
    public TextMeshProUGUI chaseText;
    public TextMeshProUGUI headCollectionText;
    public Button helpButton;
    public Button leaveButton;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip proximitySound;
    public AudioClip choiceSound;
    public AudioClip chaseStartSound;
    public AudioClip chaseEndSound;
    public AudioClip headSpawnSound;
    public AudioClip doorOpenSound;
    
    [Header("Visual Effects")]
    public Light proximityLight;
    public ParticleSystem proximityParticles;
    public ParticleSystem chaseParticles;
    public ParticleSystem headSpawnParticles;
    
    [Header("Head Spawning")]
    public Transform[] headSpawnPoints; // Points where heads will spawn
    public GameObject realHeadPrefab;
    public GameObject fakeHead1Prefab;
    public GameObject fakeHead2Prefab;
    public float headSpawnRadius = 2f;
    
    [Header("Integration")]
    public DullahanChaseSystem dullahanChaseSystem;
    public DullahanAudioManager audioManager;
    public DullahanHeadInventory headInventory;
    public Floor2EndingEventManager floor2EventManager;
    
    [Header("State Management")]
    public EventState currentState = EventState.Waiting;
    public bool eventActive = true;
    public bool chaseActive = false;
    public bool headsSpawned = false;
    public bool headsCollected = false;
    
    // Private variables
    private Transform player;
    private float chaseTimer = 0f;
    private float headCollectionTimer = 0f;
    private List<GameObject> spawnedHeads = new List<GameObject>();
    
    public enum EventState
    {
        Waiting,
        Proximity,
        Choice,
        Chase,
        HeadCollection,
        Completed
    }
    
    void Start()
    {
        InitializeComponents();
        SetupUI();
        StartCoroutine(EventLoop());
    }
    
    void InitializeComponents()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // Find components
        if (dullahanChaseSystem == null)
            dullahanChaseSystem = FindObjectOfType<DullahanChaseSystem>();
            
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
            
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
            
        if (floor2EventManager == null)
            floor2EventManager = FindObjectOfType<Floor2EndingEventManager>();
        
        // Setup audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    void SetupUI()
    {
        // Hide all UI initially
        if (proximityUI != null) proximityUI.SetActive(false);
        if (choiceUI != null) choiceUI.SetActive(false);
        if (chaseUI != null) chaseUI.SetActive(false);
        if (headCollectionUI != null) headCollectionUI.SetActive(false);
        
        // Setup button events
        if (helpButton != null)
            helpButton.onClick.AddListener(OnHelpButtonClicked);
        
        if (leaveButton != null)
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
    }
    
    IEnumerator EventLoop()
    {
        while (eventActive)
        {
            switch (currentState)
            {
                case EventState.Waiting:
                    yield return StartCoroutine(HandleWaitingState());
                    break;
                case EventState.Proximity:
                    yield return StartCoroutine(HandleProximityState());
                    break;
                case EventState.Choice:
                    yield return StartCoroutine(HandleChoiceState());
                    break;
                case EventState.Chase:
                    yield return StartCoroutine(HandleChaseState());
                    break;
                case EventState.HeadCollection:
                    yield return StartCoroutine(HandleHeadCollectionState());
                    break;
                case EventState.Completed:
                    yield return StartCoroutine(HandleCompletedState());
                    break;
            }
            
            yield return null;
        }
    }
    
    IEnumerator HandleWaitingState()
    {
        // Wait for player to enter proximity
        while (currentState == EventState.Waiting)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= proximityRadius)
                {
                    currentState = EventState.Proximity;
                    break;
                }
            }
            yield return null;
        }
    }
    
    IEnumerator HandleProximityState()
    {
        Debug.Log("[DullahanChaseEventManager] Player entered proximity");
        
        // Show proximity UI
        if (proximityUI != null)
        {
            proximityUI.SetActive(true);
            if (proximityText != null)
                proximityText.text = "You sense a powerful presence nearby...";
        }
        
        // Play proximity sound
        if (audioSource != null && proximitySound != null)
            audioSource.PlayOneShot(proximitySound);
        
        // Start proximity effects
        StartProximityEffects();
        
        // Wait for player to make choice
        yield return new WaitForSeconds(2f);
        
        currentState = EventState.Choice;
    }
    
    IEnumerator HandleChoiceState()
    {
        Debug.Log("[DullahanChaseEventManager] Player choice phase");
        
        // Show choice UI
        if (choiceUI != null)
        {
            choiceUI.SetActive(true);
            if (choiceText != null)
                choiceText.text = "Will you help Dullahan find his real head?";
        }
        
        // Play choice sound
        if (audioSource != null && choiceSound != null)
            audioSource.PlayOneShot(choiceSound);
        
        // Wait for player choice or timeout
        choiceTimer = 0f;
        while (currentState == EventState.Choice && !choiceMade && choiceTimer < choiceTimeLimit)
        {
            choiceTimer += Time.deltaTime;
            yield return null;
        }
        
        // Handle timeout
        if (!choiceMade)
        {
            Debug.Log("[DullahanChaseEventManager] Player choice timeout - defaulting to help");
            playerChoseToHelp = true;
            choiceMade = true;
        }
        
        // Hide choice UI
        if (choiceUI != null) choiceUI.SetActive(false);
        
        if (playerChoseToHelp)
        {
            currentState = EventState.Chase;
        }
        else
        {
            // Player chose to leave - open exit doors
            OpenExitDoors();
            currentState = EventState.Completed;
        }
    }
    
    IEnumerator HandleChaseState()
    {
        Debug.Log("[DullahanChaseEventManager] Chase phase started");
        
        // Show chase UI
        if (chaseUI != null)
        {
            chaseUI.SetActive(true);
            if (chaseText != null)
                chaseText.text = "Dullahan is chasing you! Survive for " + chaseDuration + " seconds!";
        }
        
        // Start chase
        if (dullahanChaseSystem != null)
            dullahanChaseSystem.StartChase();
        
        // Play chase start sound
        if (audioSource != null && chaseStartSound != null)
            audioSource.PlayOneShot(chaseStartSound);
        
        // Start chase effects
        StartChaseEffects();
        
        // Run chase timer
        chaseTimer = 0f;
        while (currentState == EventState.Chase && chaseTimer < chaseDuration)
        {
            chaseTimer += Time.deltaTime;
            
            // Update chase UI
            if (chaseText != null)
            {
                float remainingTime = chaseDuration - chaseTimer;
                chaseText.text = "Dullahan is chasing you! " + Mathf.Ceil(remainingTime) + " seconds remaining!";
            }
            
            yield return null;
        }
        
        // End chase
        if (dullahanChaseSystem != null)
            dullahanChaseSystem.EndChase();
        
        // Hide chase UI
        if (chaseUI != null) chaseUI.SetActive(false);
        
        // Play chase end sound
        if (audioSource != null && chaseEndSound != null)
            audioSource.PlayOneShot(chaseEndSound);
        
        // Stop chase effects
        StopChaseEffects();
        
        // Wait for head spawn delay
        yield return new WaitForSeconds(headSpawnDelay);
        
        // Spawn heads
        SpawnHeads();
        
        currentState = EventState.HeadCollection;
    }
    
    IEnumerator HandleHeadCollectionState()
    {
        Debug.Log("[DullahanChaseEventManager] Head collection phase started");
        
        // Show head collection UI
        if (headCollectionUI != null)
        {
            headCollectionUI.SetActive(true);
            if (headCollectionText != null)
                headCollectionText.text = "Collect the heads! You have " + headCollectionTime + " seconds!";
        }
        
        // Start head collection timer
        headCollectionTimer = 0f;
        while (currentState == EventState.HeadCollection && headCollectionTimer < headCollectionTime)
        {
            headCollectionTimer += Time.deltaTime;
            
            // Update head collection UI
            if (headCollectionText != null)
            {
                float remainingTime = headCollectionTime - headCollectionTimer;
                headCollectionText.text = "Collect the heads! " + Mathf.Ceil(remainingTime) + " seconds remaining!";
            }
            
            // Check if all heads collected
            if (CheckAllHeadsCollected())
            {
                headsCollected = true;
                break;
            }
            
            yield return null;
        }
        
        // Hide head collection UI
        if (headCollectionUI != null) headCollectionUI.SetActive(false);
        
        if (headsCollected)
        {
            Debug.Log("[DullahanChaseEventManager] All heads collected!");
            // Open real head door
            if (realHeadDoor != null)
                realHeadDoor.UnlockDoor();
        }
        else
        {
            Debug.Log("[DullahanChaseEventManager] Time's up! Opening exit doors.");
            // Open exit doors
            OpenExitDoors();
        }
        
        currentState = EventState.Completed;
    }
    
    IEnumerator HandleCompletedState()
    {
        Debug.Log("[DullahanChaseEventManager] Event completed");
        
        // Clean up spawned heads
        CleanupSpawnedHeads();
        
        // Notify floor 2 event manager
        if (floor2EventManager != null)
        {
            if (headsCollected)
                floor2EventManager.OnRealHeadAttached();
            else
            {
                // Player chose to leave - this would need to be implemented in Floor2EndingEventManager
                Debug.Log("[DullahanChaseEventManager] Player chose to leave - notification not implemented");
            }
        }
        
        // Deactivate event
        eventActive = false;
        
        yield return null;
    }
    
    void SpawnHeads()
    {
        Debug.Log("[DullahanChaseEventManager] Spawning heads");
        
        // Spawn real head
        if (realHeadPrefab != null && headSpawnPoints.Length > 0)
        {
            Transform spawnPoint = headSpawnPoints[0];
            Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * headSpawnRadius;
            spawnPosition.y = spawnPoint.position.y;
            
            GameObject realHead = Instantiate(realHeadPrefab, spawnPosition, Quaternion.identity);
            spawnedHeads.Add(realHead);
        }
        
        // Spawn fake heads
        if (fakeHead1Prefab != null && headSpawnPoints.Length > 1)
        {
            Transform spawnPoint = headSpawnPoints[1];
            Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * headSpawnRadius;
            spawnPosition.y = spawnPoint.position.y;
            
            GameObject fakeHead1 = Instantiate(fakeHead1Prefab, spawnPosition, Quaternion.identity);
            spawnedHeads.Add(fakeHead1);
        }
        
        if (fakeHead2Prefab != null && headSpawnPoints.Length > 2)
        {
            Transform spawnPoint = headSpawnPoints[2];
            Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * headSpawnRadius;
            spawnPosition.y = spawnPoint.position.y;
            
            GameObject fakeHead2 = Instantiate(fakeHead2Prefab, spawnPosition, Quaternion.identity);
            spawnedHeads.Add(fakeHead2);
        }
        
        // Play head spawn sound
        if (audioSource != null && headSpawnSound != null)
            audioSource.PlayOneShot(headSpawnSound);
        
        // Start head spawn particles
        if (headSpawnParticles != null)
            headSpawnParticles.Play();
        
        headsSpawned = true;
    }
    
    bool CheckAllHeadsCollected()
    {
        // Check if all spawned heads have been collected
        foreach (GameObject head in spawnedHeads)
        {
            if (head != null && head.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
    }
    
    void CleanupSpawnedHeads()
    {
        foreach (GameObject head in spawnedHeads)
        {
            if (head != null)
                Destroy(head);
        }
        spawnedHeads.Clear();
    }
    
    void OpenExitDoors()
    {
        Debug.Log("[DullahanChaseEventManager] Opening exit doors");
        
        if (exitDoors != null)
        {
            foreach (doorscript door in exitDoors)
            {
                if (door != null)
                    door.UnlockDoor();
            }
        }
        
        // Play door open sound
        if (audioSource != null && doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound);
    }
    
    void StartProximityEffects()
    {
        if (proximityLight != null)
            proximityLight.enabled = true;
        
        if (proximityParticles != null)
            proximityParticles.Play();
    }
    
    void StartChaseEffects()
    {
        if (chaseParticles != null)
            chaseParticles.Play();
    }
    
    void StopChaseEffects()
    {
        if (chaseParticles != null)
            chaseParticles.Stop();
    }
    
    public void OnHelpButtonClicked()
    {
        Debug.Log("[DullahanChaseEventManager] Player chose to help");
        playerChoseToHelp = true;
        choiceMade = true;
    }
    
    public void OnLeaveButtonClicked()
    {
        Debug.Log("[DullahanChaseEventManager] Player chose to leave");
        playerChoseToHelp = false;
        choiceMade = true;
    }
    
    public void OnChaseStarted()
    {
        Debug.Log("[DullahanChaseEventManager] Chase started by external system");
    }
    
    public void OnChaseEnded()
    {
        Debug.Log("[DullahanChaseEventManager] Chase ended by external system");
    }
    
    public void OnRealHeadAttachedToBody()
    {
        Debug.Log("[DullahanChaseEventManager] Real head attached to body");
        headsCollected = true;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw proximity radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);
        
        // Draw head spawn points
        if (headSpawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in headSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, headSpawnRadius);
                }
            }
        }
    }
}