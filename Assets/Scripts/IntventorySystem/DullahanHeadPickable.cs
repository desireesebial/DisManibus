using UnityEngine;
using System.Collections;

public class DullahanHeadPickable : MonoBehaviour, IPickable
{
    [Header("Head Settings")]
    public DullahanHeadSO headData;
    public bool isPickedUp = false;
    
    [Header("Visual Effects")]
    public Light headLight;
    public Renderer headRenderer;
    public Material originalMaterial;
    public Material glowMaterial;
    public GameObject headVisual; // Visual representation of the head
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip dropSound;
    
    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Press E to pick up head";
    
    [Header("UI")]
    public GameObject interactionUI;
    public TMPro.TextMeshProUGUI interactionTextUI;
    
    private bool playerInRange = false;
    private Transform player;
    private DullahanHeadInventory headInventory;
    private DullahanHeadEffectManager effectManager;
    private DullahanAudioManager audioManager;
    
    void Start()
    {
        // Find player and components
        FindPlayerAndComponents();
        
        // Setup visual effects
        SetupVisualEffects();
        
        // Setup audio
        SetupAudio();
        
        // Setup UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    void Update()
    {
        if (isPickedUp) return;
        
        CheckPlayerDistance();
        HandleInteraction();
    }
    
    private void FindPlayerAndComponents()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // Find head inventory
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
        
        // Find effect manager
        if (effectManager == null)
            effectManager = FindObjectOfType<DullahanHeadEffectManager>();
        
        // Find audio manager
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
    }
    
    void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        // Show/hide interaction UI
        if (playerInRange && !wasInRange)
        {
            ShowInteractionUI();
        }
        else if (!playerInRange && wasInRange)
        {
            HideInteractionUI();
        }
    }
    
    void HandleInteraction()
    {
        if (!playerInRange) return;
        
        if (Input.GetKeyDown(interactionKey))
        {
            PickupHead();
        }
    }
    
    public void PickupHead()
    {
        if (isPickedUp || headInventory == null) return;
        
        // Check if head inventory is full
        if (headInventory.inventoryList.Count >= headInventory.maxInventorySize)
        {
            Debug.Log("Head inventory is full!");
            return;
        }
        
        // Check if head data is valid
        if (headData == null)
        {
            Debug.LogError("Head has no ScriptableObject assigned!");
            return;
        }
        
        // Add to head inventory using compatibility method
        headInventory.AddToInventoryList(headData);
        
        // Apply effects if any
        if (headData.hasEffect && effectManager != null)
        {
            effectManager.ApplyHeadEffect(headData);
        }
        
        // Play pickup sound
        PlayPickupSound();
        
        // Hide interaction UI
        HideInteractionUI();
        
        // Mark as picked up
        isPickedUp = true;
        
        // Hide the head object
        gameObject.SetActive(false);
        
        Debug.Log($"Picked up {headData.headName}!");
    }
    
    // Implementation of IPickable interface
    public void PickItem()
    {
        PickupHead();
    }
    
    private void PlayPickupSound()
    {
        // Try audio manager first
        if (audioManager != null)
        {
            audioManager.PlayHeadPickupSound(headData.headType);
        }
        // Fallback to local audio source
        else if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        // Use head data audio if available
        else if (headData != null && headData.pickupSound != null)
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.PlayOneShot(headData.pickupSound);
        }
    }
    
    void SetupVisualEffects()
    {
        if (headData == null) return;
        
        // Setup glow effect
        if (headData.hasGlowEffect)
        {
            SetupGlowEffect();
        }
        
        // Setup material
        SetupMaterial();
    }
    
    private void SetupGlowEffect()
    {
        if (headLight == null)
        {
            headLight = gameObject.AddComponent<Light>();
        }
        
        if (headLight != null)
        {
            headLight.color = headData.headGlowColor;
            headLight.intensity = 1f;
            headLight.range = 3f;
            
            // Add pulsing effect
            StartCoroutine(PulseLight());
        }
    }
    
    private void SetupMaterial()
    {
        if (headRenderer == null)
            headRenderer = GetComponent<Renderer>();
            
        if (headRenderer != null && headData.headMaterial != null)
        {
            originalMaterial = headRenderer.material;
            headRenderer.material = headData.headMaterial;
        }
    }
    
    private void SetupAudio()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Set audio clips from head data
        if (headData != null)
        {
            pickupSound = headData.pickupSound;
            dropSound = headData.dropSound;
        }
    }
    
    IEnumerator PulseLight()
    {
        if (headLight == null) yield break;
        
        float originalIntensity = headLight.intensity;
        
        while (!isPickedUp)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.5f + 1f;
            headLight.intensity = originalIntensity * pulse;
            yield return null;
        }
    }
    
    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionTextUI != null)
                interactionTextUI.text = interactionText;
        }
    }
    
    void HideInteractionUI()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionUI();
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionUI();
        }
    }
}
