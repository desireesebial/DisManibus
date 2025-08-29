using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanternPickable : MonoBehaviour, IPickable
{
    [Header("Lantern Settings")]
    public bool isPickedUp = false;
    public LanternSO lanternData;
    
    [Header("Visual Components")]
    public GameObject lanternModel;
    public Light lanternLight;
    public ParticleSystem lanternParticles;
    public Material lanternMaterial;
    public Color onColor = Color.yellow;
    public Color offColor = Color.gray;
    
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;
    public LayerMask playerLayer = 1;
    
    [Header("UI")]
    public GameObject pickupPrompt;
    public TextMeshProUGUI promptText;
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    [Header("Integration")]
    public DullahanHeadInventory headInventory;
    public AudioManager audioManager; // Reference to your existing audio system
    
    // Private variables
    private bool playerInRange = false;
    private Transform playerTransform;
    private bool isLanternOn = false;
    private Color originalMaterialColor;
    
    void Start()
    {
        InitializeLantern();
    }
    
    void Update()
    {
        if (isPickedUp) return;
        
        CheckPlayerProximity();
        HandleInput();
        HandleLanternEffects();
    }
    
    private void InitializeLantern()
    {
        // Find references if not assigned
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
            
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        
        // Store original material color
        if (lanternMaterial != null)
            originalMaterialColor = lanternMaterial.color;
        
        // Setup initial state
        SetLanternState(false);
        
        // Setup UI
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
            
        if (promptText != null && lanternData != null)
            promptText.text = lanternData.toggleMessage;
        
        Debug.Log("Lantern Pickable initialized");
    }
    
    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= pickupRange;
        
        // Show/hide pickup prompt
        if (pickupPrompt != null)
            pickupPrompt.SetActive(playerInRange && !isPickedUp);
        
        // Debug visualization
        if (playerInRange != wasInRange)
        {
            Debug.Log($"Player {(playerInRange ? "entered" : "left")} lantern pickup range");
        }
    }
    
    private void HandleInput()
    {
        if (!playerInRange || isPickedUp) return;
        
        if (Input.GetKeyDown(pickupKey))
        {
            PickupLantern();
        }
    }
    
    private void HandleLanternEffects()
    {
        if (isPickedUp) return;
        
        // Optional: Add ambient effects like flickering or pulsing
        if (lanternLight != null)
        {
            // Subtle flickering effect
            float flicker = Mathf.Sin(Time.time * 2f) * 0.1f + 0.9f;
            lanternLight.intensity = 1f * flicker;
        }
    }
    
    public void PickupLantern()
    {
        if (isPickedUp) return;
        
        isPickedUp = true;
        
        // Add to inventory FIRST
        if (headInventory != null)
        {
            headInventory.GiveLantern(lanternData);
        }
        else
        {
            Debug.LogError("No DullahanHeadInventory found! Lantern cannot be added to inventory.");
        }
        
        // Play pickup sound
        if (audioSource != null && lanternData != null && lanternData.pickupSound != null)
        {
            audioSource.PlayOneShot(lanternData.pickupSound);
        }
        
        // Play audio manager sound (using your existing system)
        if (audioManager != null && lanternData != null && lanternData.pickupSound != null)
        {
            audioManager.PlayRandomized(audioSource, lanternData.pickupSound, 1f);
        }
        
        // Hide pickup prompt
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
        
        // Hide lantern model
        if (lanternModel != null)
            lanternModel.SetActive(false);
            
        if (lanternLight != null)
            lanternLight.enabled = false;
            
        if (lanternParticles != null)
            lanternParticles.Stop();
        
        Debug.Log($"Lantern picked up: {(lanternData != null ? lanternData.lanternName : "Unknown Lantern")}");
        
        // Optional: Destroy the GameObject after a delay to allow sound to play
        Destroy(gameObject, 1f);
    }
    
    private void SetLanternState(bool state)
    {
        isLanternOn = state;
        
        if (lanternLight != null)
            lanternLight.enabled = state;
            
        if (lanternParticles != null)
        {
            if (state)
                lanternParticles.Play();
            else
                lanternParticles.Stop();
        }
        
        if (lanternMaterial != null)
            lanternMaterial.color = state ? onColor : offColor;
    }
    
    public void ToggleLantern()
    {
        if (isPickedUp) return;
        
        SetLanternState(!isLanternOn);
        
        // Play sound
        if (audioSource != null && lanternData != null)
        {
            AudioClip soundToPlay = isLanternOn ? lanternData.toggleOnSound : lanternData.toggleOffSound;
            if (soundToPlay != null)
                audioSource.PlayOneShot(soundToPlay);
        }
        
        Debug.Log($"Lantern toggled: {(isLanternOn ? "ON" : "OFF")}");
    }
    
    // IPickable interface implementation
    public void PickItem()
    {
        PickupLantern();
    }
    
    // Public getters
    public bool IsPickedUp() => isPickedUp;
    public bool IsPlayerInRange() => playerInRange;
    public bool IsLanternOn() => isLanternOn;
    public string GetLanternName() => lanternData != null ? lanternData.lanternName : "Unknown Lantern";
    public string GetLanternDescription() => lanternData != null ? lanternData.description : "No description available";
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
    
    void OnDrawGizmos()
    {
        if (playerInRange)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}
