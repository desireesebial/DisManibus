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
    
    [Header("Chase Cycle Settings")]
    public float chaseDuration = 180f; // 3 minutes per chase cycle
    public float restDuration = 60f; // 1 minute rest between chases
    public int maxChaseCycles = -1; // -1 = infinite cycles, or set a limit
    private int currentCycleCount = 0;
    
    [Header("Doors")]
    public doorscript[] exitDoors; // Multiple exit doors that open together after chase
    public doorscript realHeadDoor; // Door to real head room
    public int exitDoorKeyID = 999;
    public int realHeadDoorKeyID = 100;
    public bool exitDoorsLinked = true; // Whether exit doors should open simultaneously
    
    [Header("UI Elements")]
    public GameObject chaseUI; // UI shown during chase/rest
    public TextMeshProUGUI chaseText; // Text for chase/rest messages
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip chaseStartSound;
    public AudioClip chaseEndSound;
    public AudioClip restStartSound;

    [Header("Visual Effects")]
    public ParticleSystem chaseParticles;
    public ParticleSystem restParticles;
    
    [Header("Integration")]
    public DullahanChaseSystem dullahanChaseSystem;
    public DullahanAudioManager audioManager;
    public DullahanHeadInventory headInventory;
    public Floor2EndingEventManager floor2EventManager;
    
    [Header("State Management")]
    public EventState currentState = EventState.Waiting;
    public bool eventActive = true;
    public bool chaseActive = false;

    // Private variables
    private Transform player;
    private float stateTimer = 0f;
    
    public enum EventState
    {
        Waiting,     // Waiting for player to enter proximity
        Chasing,     // Active chase period (3 minutes)
        Resting,     // Dullahan resting period (1 minute)
        Exited       // Player left proximity/scene
    }
    
    void Start()
    {
        InitializeComponents();
        SetupUI();
        StartCoroutine(EventLoop());
    }

    void Update()
    {
        // Check if player left proximity during active chase/rest cycle
        if ((currentState == EventState.Chasing || currentState == EventState.Resting) && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > proximityRadius)
            {
                OnPlayerLeftProximity();
            }
        }
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
        // Hide chase UI initially
        if (chaseUI != null) chaseUI.SetActive(false);
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

                case EventState.Chasing:
                    yield return StartCoroutine(HandleChasingState());
                    // After chase completes, automatically transition to rest
                    if (currentState == EventState.Chasing)
                    {
                        currentCycleCount++;
                        currentState = EventState.Resting;
                    }
                    break;

                case EventState.Resting:
                    yield return StartCoroutine(HandleRestingState());
                    // After rest completes, automatically transition back to chase
                    if (currentState == EventState.Resting)
                    {
                        // Check if we've reached max cycles
                        if (maxChaseCycles > 0 && currentCycleCount >= maxChaseCycles)
                        {
                            currentState = EventState.Exited;
                        }
                        else
                        {
                            currentState = EventState.Chasing;
                        }
                    }
                    break;

                case EventState.Exited:
                    yield return StartCoroutine(HandleExitedState());
                    break;
            }

            yield return null;
        }
    }
    
    IEnumerator HandleWaitingState()
    {
        Debug.Log("[DullahanChase] Waiting for player to enter proximity...");

        // Wait for player to enter proximity
        while (currentState == EventState.Waiting)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= proximityRadius)
                {
                    Debug.Log("[DullahanChase] Player entered proximity! Starting chase immediately!");
                    currentCycleCount = 0; // Reset cycle count
                    currentState = EventState.Chasing; // Start chasing immediately
                    break;
                }
            }
            yield return null;
        }
    }
    
    IEnumerator HandleChasingState()
    {
        Debug.Log($"[DullahanChase] Starting chase cycle #{currentCycleCount + 1} - 3 minutes!");

        // Start the chase system
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartChase();
            chaseActive = true;
        }

        // Play chase start sound
        if (audioSource != null && chaseStartSound != null)
            audioSource.PlayOneShot(chaseStartSound);

        // Start chase particles
        if (chaseParticles != null)
            chaseParticles.Play();

        // Show chase UI
        if (chaseUI != null)
        {
            chaseUI.SetActive(true);
        }

        // Run chase timer (3 minutes = 180 seconds)
        stateTimer = 0f;
        while (stateTimer < chaseDuration && currentState == EventState.Chasing)
        {
            stateTimer += Time.deltaTime;

            // Update chase UI with countdown
            if (chaseText != null)
            {
                float remaining = chaseDuration - stateTimer;
                int minutes = Mathf.FloorToInt(remaining / 60f);
                int seconds = Mathf.FloorToInt(remaining % 60f);
                chaseText.text = $"DULLAHAN IS CHASING! Survive: {minutes}:{seconds:00}";
            }

            yield return null;
        }

        // End the chase
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
            chaseActive = false;
        }

        // Play chase end sound
        if (audioSource != null && chaseEndSound != null)
            audioSource.PlayOneShot(chaseEndSound);

        // Stop chase particles
        if (chaseParticles != null)
            chaseParticles.Stop();

        // Hide chase UI (will be shown again in rest state)
        if (chaseUI != null)
            chaseUI.SetActive(false);

        Debug.Log("[DullahanChase] Chase ended! Entering rest period...");
    }
    
    IEnumerator HandleRestingState()
    {
        Debug.Log("[DullahanChase] Dullahan is resting - 1 minute break!");

        // Stop Dullahan movement/chase
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase(); // Ensure chase is fully stopped
            chaseActive = false;
        }

        // Play rest start sound
        if (audioSource != null && restStartSound != null)
            audioSource.PlayOneShot(restStartSound);

        // Start rest particles
        if (restParticles != null)
            restParticles.Play();

        // Show rest UI
        if (chaseUI != null)
        {
            chaseUI.SetActive(true);
        }

        // Run rest timer (1 minute = 60 seconds)
        stateTimer = 0f;
        while (stateTimer < restDuration && currentState == EventState.Resting)
        {
            stateTimer += Time.deltaTime;

            // Update rest UI with countdown
            if (chaseText != null)
            {
                float remaining = restDuration - stateTimer;
                int seconds = Mathf.FloorToInt(remaining);
                chaseText.text = $"REST PERIOD - Next chase in: {seconds}s";
            }

            yield return null;
        }

        // Stop rest particles
        if (restParticles != null)
            restParticles.Stop();

        // Hide rest UI (will be shown again in chase state)
        if (chaseUI != null)
            chaseUI.SetActive(false);

        Debug.Log("[DullahanChase] Rest period ended! Next chase starting...");
    }
    
    IEnumerator HandleExitedState()
    {
        Debug.Log("[DullahanChase] Player left proximity or reached max cycles. Stopping chase...");

        // Stop all chase activity
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
            chaseActive = false;
        }

        // Hide all UI
        if (chaseUI != null)
            chaseUI.SetActive(false);

        // Stop all particles
        if (chaseParticles != null)
            chaseParticles.Stop();
        if (restParticles != null)
            restParticles.Stop();

        Debug.Log("[DullahanChase] Chase cycle ended. Waiting for player to re-enter...");

        // Wait a bit before resetting to Waiting state
        yield return new WaitForSeconds(5f);

        // Reset to waiting state
        currentCycleCount = 0;
        currentState = EventState.Waiting;
    }

    void OnPlayerLeftProximity()
    {
        Debug.Log("[DullahanChase] Player left proximity - stopping chase cycle");

        // Immediately transition to Exited state
        currentState = EventState.Exited;
    }
    
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw proximity radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);

        // Draw state info
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                $"State: {currentState}\nCycle: {currentCycleCount + 1}");
        }
    }
}