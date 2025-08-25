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
    private PlayerInventory playerInventory;
    private DullahanHeadEffectManager effectManager;
    private DullahanAudioManager audioManager;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerInventory = player.GetComponent<PlayerInventory>();
        effectManager = FindObjectOfType<DullahanHeadEffectManager>();
        audioManager = FindObjectOfType<DullahanAudioManager>();
        
        // Setup visual effects
        SetupVisualEffects();
        
        // Setup audio
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
        if (isPickedUp || playerInventory == null) return;
        
        // Check if player has inventory space
        if (playerInventory.inventoryList.Count >= playerInventory.maxInventorySize)
        {
            Debug.Log("Inventory is full!");
            return;
        }
        
        // Add to inventory
        if (headData != null)
        {
            // Convert DullahanHeadSO to KeyItemsSO for inventory compatibility
            KeyItemsSO inventoryItem = CreateInventoryItem(headData);
            playerInventory.inventoryList.Add(inventoryItem);
            
            // Apply effects if any
            if (headData.hasEffect && effectManager != null)
            {
                effectManager.ApplyHeadEffect(headData);
            }
            
            // Play pickup sound
            if (audioManager != null)
            {
                audioManager.PlayHeadPickupSound(headData.headType);
            }
            else if (pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            
            // Hide interaction UI
            HideInteractionUI();
            
            // Mark as picked up
            isPickedUp = true;
            
            // Hide the head object
            gameObject.SetActive(false);
            
            Debug.Log($"Picked up {headData.headName}!");
        }
    }
    
    // Implementation of IPickable interface
    public void PickItem()
    {
        PickupHead();
    }
    
    KeyItemsSO CreateInventoryItem(DullahanHeadSO headSO)
    {
        // Create a temporary KeyItemsSO for inventory compatibility
        KeyItemsSO item = ScriptableObject.CreateInstance<KeyItemsSO>();
        item.itemName = headSO.headName;
        item.itemID = headSO.headID;
        item.description = headSO.description;
        item.item_sprite = headSO.headSprite;
        item.item_type = itemType.Document; // Use Document type for heads
        item.cooldown = 0f;
        
        return item;
    }
    
    void SetupVisualEffects()
    {
        if (headData == null) return;
        
        // Setup glow effect
        if (headData.hasGlowEffect)
        {
            if (headLight == null)
            {
                headLight = gameObject.AddComponent<Light>();
            }
            
            headLight.color = headData.headGlowColor;
            headLight.intensity = 1f;
            headLight.range = 3f;
            
            // Add pulsing effect
            StartCoroutine(PulseLight());
        }
        
        // Setup material
        if (headRenderer == null)
            headRenderer = GetComponent<Renderer>();
            
        if (headRenderer != null && headData.headMaterial != null)
        {
            originalMaterial = headRenderer.material;
            headRenderer.material = headData.headMaterial;
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
