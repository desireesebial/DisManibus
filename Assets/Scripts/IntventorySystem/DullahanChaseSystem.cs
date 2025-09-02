using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DullahanChaseSystem : MonoBehaviour
{
    [Header("Chase Settings")]
    public float maxChaseSpeed = 8f;
    public float minChaseSpeed = 3f;
    public float maxDetectionRange = 20f;
    public float minDetectionRange = 5f;
    public float intensityUpdateRate = 0.1f;
    
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolRadius = 15f;
    public float patrolWaitTime = 3f;
    public bool useWaypointPatrol = false;
    public Transform[] patrolWaypoints;
    
    [Header("Dullahan References")]
    public Transform dullahanTransform;
    public NavMeshAgent dullahanAgent;
    public Animator dullahanAnimator;
    
    [Header("Player References")]
    public Transform playerTransform;
    public FirstPersonController playerController;
    
    [Header("Chase Intensity")]
    public float currentIntensity = 0f;
    public float maxIntensity = 1f;
    public float intensityDecayRate = 0.5f;
    
    [Header("Integration")]
    public DullahanAudioManager audioManager;
    public DullahanChaseEventManager eventManager;
    
    [Header("Visual Effects")]
    public Light dullahanLight;
    public ParticleSystem chaseParticles;
    public Material dullahanMaterial;
    public Color normalColor = Color.white;
    public Color chaseColor = Color.red;
    
    private bool isChasing = false;
    private bool isInitialized = false;
    private Vector3 lastPlayerPosition;
    private float distanceToPlayer;
    
    void Start()
    {
        InitializeChaseSystem();
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        if (isChasing)
        {
            UpdateChase();
        }
        else
        {
            UpdatePatrol();
        }
        
        UpdateVisualEffects();
    }
    
    private void InitializeChaseSystem()
    {
        // Find references if not assigned
        if (dullahanTransform == null)
        {
            GameObject dullahan = GameObject.FindGameObjectWithTag("Dullahan");
            if (dullahan != null)
                dullahanTransform = dullahan.transform;
        }
        
        if (dullahanAgent == null && dullahanTransform != null)
        {
            dullahanAgent = dullahanTransform.GetComponent<NavMeshAgent>();
        }
        
        if (dullahanAnimator == null && dullahanTransform != null)
        {
            dullahanAnimator = dullahanTransform.GetComponent<Animator>();
        }
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        if (playerController == null && playerTransform != null)
        {
            playerController = playerTransform.GetComponent<FirstPersonController>();
        }
        
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
            
        if (eventManager == null)
            eventManager = FindObjectOfType<DullahanChaseEventManager>();
        
        // Initialize chase settings
        if (dullahanAgent != null)
        {
            dullahanAgent.speed = minChaseSpeed;
            dullahanAgent.stoppingDistance = 2f;
        }
        
        isInitialized = true;
        Debug.Log("Dullahan Chase System initialized");
    }
    
    public void StartChase()
    {
        if (!isInitialized) return;
        
        isChasing = true;
        currentIntensity = 0f;
        
        // Set chase animation
        if (dullahanAnimator != null)
        {
            dullahanAnimator.SetBool("IsChasing", true);
        }
        
        // Start chase audio
        if (audioManager != null)
        {
            audioManager.StartChase();
        }
        
        Debug.Log("Dullahan chase started");
    }
    
    public void EndChase()
    {
        if (!isInitialized) return;
        
        isChasing = false;
        currentIntensity = 0f;
        
        // Set patrol animation
        if (dullahanAnimator != null)
        {
            dullahanAnimator.SetBool("IsChasing", false);
        }
        
        // Stop chase audio
        if (audioManager != null)
        {
            audioManager.EndChase();
        }
        
        // Return to patrol behavior
        if (dullahanAgent != null)
        {
            dullahanAgent.speed = patrolSpeed;
            // Set initial patrol destination
            Vector3 patrolPoint = GetRandomPatrolPoint();
            dullahanAgent.SetDestination(patrolPoint);
        }
        
        Debug.Log("Dullahan chase ended - returning to patrol");
    }
    
    public void StartPatrol()
    {
        if (!isInitialized) return;
        
        isChasing = false;
        currentIntensity = 0f;
        
        // Set patrol animation
        if (dullahanAnimator != null)
        {
            dullahanAnimator.SetBool("IsChasing", false);
        }
        
        // Set patrol speed and destination
        if (dullahanAgent != null)
        {
            dullahanAgent.speed = patrolSpeed;
            Vector3 patrolPoint = GetRandomPatrolPoint();
            dullahanAgent.SetDestination(patrolPoint);
        }
        
        Debug.Log("Dullahan patrol started");
    }
    
    private void UpdateChase()
    {
        if (playerTransform == null || dullahanTransform == null) return;
        
        // Calculate distance to player
        distanceToPlayer = Vector3.Distance(dullahanTransform.position, playerTransform.position);
        
        // Update chase intensity based on distance
        UpdateChaseIntensity();
        
        // Move towards player
        if (dullahanAgent != null)
        {
            dullahanAgent.SetDestination(playerTransform.position);
            
            // Update speed based on intensity
            float targetSpeed = Mathf.Lerp(minChaseSpeed, maxChaseSpeed, currentIntensity);
            dullahanAgent.speed = Mathf.Lerp(dullahanAgent.speed, targetSpeed, Time.deltaTime * 2f);
        }
        
        // Update audio intensity
        if (audioManager != null)
        {
            audioManager.SetChaseIntensity(currentIntensity);
        }
        
        // Check if player is caught
        if (distanceToPlayer <= 2f)
        {
            OnPlayerCaught();
        }
    }
    
    private void UpdatePatrol()
    {
        // Implement patrol behavior here
        if (dullahanAgent != null && !dullahanAgent.hasPath)
        {
            if (useWaypointPatrol && patrolWaypoints.Length > 0)
            {
                // Use waypoint-based patrol
                SetNextWaypointDestination();
            }
            else
            {
                // Use random patrol within radius
                Vector3 randomPoint = GetRandomPatrolPoint();
                dullahanAgent.SetDestination(randomPoint);
            }
        }
    }
    
    private void SetNextWaypointDestination()
    {
        if (patrolWaypoints.Length == 0) return;
        
        // Simple waypoint cycling
        int currentWaypointIndex = Random.Range(0, patrolWaypoints.Length);
        Transform waypoint = patrolWaypoints[currentWaypointIndex];
        
        if (waypoint != null)
        {
            dullahanAgent.SetDestination(waypoint.position);
        }
    }
    
    private void UpdateChaseIntensity()
    {
        // Calculate intensity based on distance
        float normalizedDistance = Mathf.Clamp01((distanceToPlayer - minDetectionRange) / (maxDetectionRange - minDetectionRange));
        float targetIntensity = 1f - normalizedDistance;
        
        // Smoothly update intensity
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * intensityUpdateRate);
        currentIntensity = Mathf.Clamp(currentIntensity, 0f, maxIntensity);
        
        // Decay intensity when far from player
        if (distanceToPlayer > maxDetectionRange)
        {
            currentIntensity -= intensityDecayRate * Time.deltaTime;
            currentIntensity = Mathf.Max(0f, currentIntensity);
        }
    }
    
    private void UpdateVisualEffects()
    {
        // Update Dullahan light
        if (dullahanLight != null)
        {
            float intensity = Mathf.Lerp(0f, 2f, currentIntensity);
            dullahanLight.intensity = intensity;
            dullahanLight.color = Color.Lerp(normalColor, chaseColor, currentIntensity);
        }
        
        // Update particle effects
        if (chaseParticles != null)
        {
            var emission = chaseParticles.emission;
            emission.rateOverTime = currentIntensity * 50f;
        }
        
        // Update material color
        if (dullahanMaterial != null)
        {
            dullahanMaterial.color = Color.Lerp(normalColor, chaseColor, currentIntensity);
        }
    }
    
    private Vector3 GetRandomPatrolPoint()
    {
        // Generate a random point within patrol area
        Vector3 randomPoint = dullahanTransform.position + Random.insideUnitSphere * patrolRadius;
        
        // Ensure point is on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return dullahanTransform.position;
    }
    
    private void OnPlayerCaught()
    {
        Debug.Log("Player caught by Dullahan!");
        
        // Play catch sound
        if (audioManager != null)
        {
            // You can add a player caught sound method
            // audioManager.PlayPlayerCaughtSound();
        }
        
        // Handle player death/respawn
        // This could trigger game over, respawn player, etc.
        HandlePlayerDeath();
    }
    
    private void HandlePlayerDeath()
    {
        // You can implement player death logic here
        // For example: respawn player, reduce health, restart phase, etc.
        
        Debug.Log("Player death handled");
        
        // Example: Restart current phase
        if (eventManager != null)
        {
            eventManager.ResetEvent();
        }
    }
    
    // Public methods for external control
    public void SetChaseSpeed(float minSpeed, float maxSpeed)
    {
        minChaseSpeed = minSpeed;
        maxChaseSpeed = maxSpeed;
    }
    
    public float GetCurrentChaseSpeed()
    {
        // Return the current calculated chase speed based on intensity
        return Mathf.Lerp(minChaseSpeed, maxChaseSpeed, currentIntensity);
    }
    
    public void SetDetectionRange(float minRange, float maxRange)
    {
        minDetectionRange = minRange;
        maxDetectionRange = maxRange;
    }
    
    public float GetChaseIntensity()
    {
        return currentIntensity;
    }
    
    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }
    
    public void SetChaseIntensity(float intensity)
    {
        currentIntensity = Mathf.Clamp(intensity, 0f, maxIntensity);
    }
    
    public float GetDistanceToPlayer()
    {
        return distanceToPlayer;
    }
    
    public bool IsChasing()
    {
        return isChasing;
    }
    
    public void SetChaseTarget(Transform target)
    {
        playerTransform = target;
    }
    
    public void SetDullahanTransform(Transform dullahan)
    {
        dullahanTransform = dullahan;
        if (dullahan != null)
        {
            dullahanAgent = dullahan.GetComponent<NavMeshAgent>();
            dullahanAnimator = dullahan.GetComponent<Animator>();
        }
    }
}
