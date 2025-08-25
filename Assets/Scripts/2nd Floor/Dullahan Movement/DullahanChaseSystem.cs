using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using System.Collections;

public class DullahanChaseSystem : MonoBehaviour
{
    [Header("Chase Intensity Settings")]
    public float maxChaseSpeed = 8f;
    public float minChaseSpeed = 3f;
    public float maxIntensityDistance = 2f;
    public float minIntensityDistance = 15f;
    public float intensityMultiplier = 1.5f;
    
    [Header("Audio")]
    public DullahanAudioManager audioManager;
    
    [Header("Visual Effects")]
    public Light dullahanLight;
    public float maxLightIntensity = 3f;
    public float minLightIntensity = 0.5f;
    public Color normalLightColor = Color.white;
    public Color intenseLightColor = Color.red;
    public float lightFlickerSpeed = 10f;
    
    [Header("Player Effects")]
    public FirstPersonController playerController;
    public float maxScreenShake = 0.1f;
    public float maxFOVChange = 10f;
    public float normalFOV = 60f;
    
    [Header("Chase States")]
    public bool isChasing = false;
    public bool isNearPlayer = false;
    public float currentIntensity = 0f;
    
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private float originalPlayerFOV;
    private Vector3 originalPlayerPosition;
    private Camera playerCamera;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (playerController == null)
            playerController = player.GetComponent<FirstPersonController>();
            
        if (playerController != null)
        {
            playerCamera = playerController.playerCamera;
            originalPlayerFOV = playerController.fov;
        }
        
        // Find audio manager if not assigned
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
        
        // Initialize light
        if (dullahanLight != null)
        {
            dullahanLight.intensity = minLightIntensity;
            dullahanLight.color = normalLightColor;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Update chase state
        UpdateChaseState(distanceToPlayer);
        
        // Update intensity based on distance
        UpdateIntensity(distanceToPlayer);
        
        // Apply intensity effects
        ApplyIntensityEffects();
        
        // Update chase behavior
        UpdateChaseBehavior();
    }
    
    void UpdateChaseState(float distance)
    {
        bool wasChasing = isChasing;
        isChasing = distance <= minIntensityDistance;
        isNearPlayer = distance <= maxIntensityDistance;
        
        // Trigger chase start/end events
        if (isChasing && !wasChasing)
        {
            OnChaseStart();
        }
        else if (!isChasing && wasChasing)
        {
            OnChaseEnd();
        }
    }
    
    void UpdateIntensity(float distance)
    {
        if (!isChasing)
        {
            currentIntensity = 0f;
            return;
        }
        
        // Calculate intensity based on distance (closer = higher intensity)
        float normalizedDistance = Mathf.Clamp01((minIntensityDistance - distance) / (minIntensityDistance - maxIntensityDistance));
        currentIntensity = Mathf.Lerp(0f, 1f, normalizedDistance);
        currentIntensity = Mathf.Pow(currentIntensity, intensityMultiplier);
    }
    
    void ApplyIntensityEffects()
    {
        // Update agent speed
        if (agent != null)
        {
            float targetSpeed = Mathf.Lerp(minChaseSpeed, maxChaseSpeed, currentIntensity);
            agent.speed = targetSpeed;
        }
        
        // Update audio
        UpdateChaseAudio();
        
        // Update visual effects
        UpdateVisualEffects();
        
        // Update player effects
        UpdatePlayerEffects();
    }
    
    void UpdateChaseAudio()
    {
        if (audioManager != null)
        {
            audioManager.SetChaseIntensity(currentIntensity);
        }
    }
    
    void UpdateVisualEffects()
    {
        if (dullahanLight == null) return;
        
        // Update light intensity and color
        float targetIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, currentIntensity);
        Color targetColor = Color.Lerp(normalLightColor, intenseLightColor, currentIntensity);
        
        // Add flicker effect when near player
        if (isNearPlayer)
        {
            float flicker = Mathf.Sin(Time.time * lightFlickerSpeed) * 0.3f + 1f;
            targetIntensity *= flicker;
        }
        
        dullahanLight.intensity = Mathf.Lerp(dullahanLight.intensity, targetIntensity, Time.deltaTime * 5f);
        dullahanLight.color = Color.Lerp(dullahanLight.color, targetColor, Time.deltaTime * 3f);
    }
    
    void UpdatePlayerEffects()
    {
        if (playerController == null || playerCamera == null) return;
        
        // Screen shake effect
        if (isNearPlayer)
        {
            float shakeAmount = maxScreenShake * currentIntensity;
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0f
            );
            playerCamera.transform.localPosition = originalPlayerPosition + shakeOffset;
        }
        else
        {
            // Return to original position
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition, 
                originalPlayerPosition, 
                Time.deltaTime * 5f
            );
        }
        
        // FOV effect
        float targetFOV = normalFOV + (maxFOVChange * currentIntensity);
        playerController.fov = Mathf.Lerp(playerController.fov, targetFOV, Time.deltaTime * 2f);
    }
    
    void UpdateChaseBehavior()
    {
        if (!isChasing) return;
        
        // Set destination to player
        if (agent != null && player != null)
        {
            agent.SetDestination(player.position);
        }
        
        // Update animation
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.SetBool("IsChasing", true);
            animator.SetFloat("ChaseIntensity", currentIntensity);
        }
    }
    

    
    void OnChaseStart()
    {
        Debug.Log("Dullahan chase started!");
        
        // Start chase audio
        if (audioManager != null)
        {
            audioManager.StartChase();
        }
        
        // Store original player camera position
        if (playerCamera != null)
        {
            originalPlayerPosition = playerCamera.transform.localPosition;
        }
    }
    
    void OnChaseEnd()
    {
        Debug.Log("Dullahan chase ended!");
        
        // End chase audio
        if (audioManager != null)
        {
            audioManager.EndChase();
        }
        
        // Reset player effects
        if (playerController != null)
        {
            playerController.fov = originalPlayerFOV;
        }
        
        // Reset light
        if (dullahanLight != null)
        {
            dullahanLight.intensity = minLightIntensity;
            dullahanLight.color = normalLightColor;
        }
        
        // Update animation
        if (animator != null)
        {
            animator.SetBool("IsChasing", false);
        }
    }
    
    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }
    
    public bool IsChasing()
    {
        return isChasing;
    }
    
    public bool IsNearPlayer()
    {
        return isNearPlayer;
    }
}
