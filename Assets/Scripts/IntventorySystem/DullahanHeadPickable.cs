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

        // Auto-spawn head visual if not assigned but headPrefab exists
        AutoSpawnHeadVisual();

        // Setup visual effects
        SetupVisualEffects();

        // Setup audio
        SetupAudio();

        // Setup UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void AutoSpawnHeadVisual()
    {
        // If headVisual is already assigned, no need to spawn
        if (headVisual != null)
        {
            Debug.Log($"[DullahanHeadPickable] Head visual already assigned for {gameObject.name}");
            return;
        }

        // Check if a visual already exists in children (prevent re-spawning)
        Transform existingVisual = transform.Find($"{headData?.headName}_Visual");
        if (existingVisual != null)
        {
            Debug.Log($"[DullahanHeadPickable] Found existing visual {existingVisual.name}, using it instead of spawning new one");
            headVisual = existingVisual.gameObject;
            CleanupHeadVisualComponents(headVisual);
            return;
        }

        // If we have headData with a prefab, spawn it
        if (headData != null && headData.headPrefab != null)
        {
            Debug.Log($"[DullahanHeadPickable] Auto-spawning head visual for {headData.headName}");

            // Instantiate the head visual as a child
            headVisual = Instantiate(headData.headPrefab, transform);
            headVisual.name = $"{headData.headName}_Visual";
            headVisual.transform.localPosition = Vector3.zero;
            headVisual.transform.localRotation = Quaternion.identity;
            headVisual.transform.localScale = Vector3.one;

            // Clean up interactive components from the spawned visual
            CleanupHeadVisualComponents(headVisual);

            // Get renderer from spawned visual
            if (headRenderer == null)
            {
                headRenderer = headVisual.GetComponent<Renderer>();
                if (headRenderer == null)
                {
                    headRenderer = headVisual.GetComponentInChildren<Renderer>();
                }
            }

            Debug.Log($"[DullahanHeadPickable] ✅ Head visual spawned successfully for {headData.headName}");
        }
        else
        {
            Debug.LogWarning($"[DullahanHeadPickable] ⚠️ Cannot auto-spawn visual for {gameObject.name}: " +
                           $"headData={headData != null}, headPrefab={headData?.headPrefab != null}");
        }
    }

    private void CleanupHeadVisualComponents(GameObject visual)
    {
        if (visual == null) return;

        Debug.Log($"[DullahanHeadPickable] Cleaning up interactive components from spawned visual: {visual.name}");

        // Remove ALL DullahanHeadPickable components from the spawned visual and its children
        // This prevents duplicates and ensures only the parent is pickable
        DullahanHeadPickable[] childPickables = visual.GetComponentsInChildren<DullahanHeadPickable>(true);
        foreach (var pickable in childPickables)
        {
            if (pickable != this) // Don't destroy self
            {
                Debug.Log($"[DullahanHeadPickable] Removing DullahanHeadPickable from {pickable.gameObject.name}");
                Destroy(pickable);
            }
        }

        // Remove colliders that might cause interaction issues
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
        {
            Debug.Log($"[DullahanHeadPickable] Removing Collider from {col.gameObject.name}");
            Destroy(col);
        }

        // Remove rigidbodies that might cause physics issues
        Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rigidbodies)
        {
            Debug.Log($"[DullahanHeadPickable] Removing Rigidbody from {rb.gameObject.name}");
            Destroy(rb);
        }

        Debug.Log($"[DullahanHeadPickable] ✅ Visual cleanup complete for {visual.name}");
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

        // Check if head data is valid
        if (headData == null)
        {
            Debug.LogError("Head has no ScriptableObject assigned!");
            return;
        }

        // SAFEGUARD: Check if this exact head is already in inventory (prevent duplicates)
        bool alreadyInInventory = false;
        foreach (var slot in headInventory.inventorySlots)
        {
            if (slot.isOccupied &&
                slot.itemType == DullahanHeadInventory.InventorySlot.ItemType.Head &&
                slot.headItem != null &&
                slot.headItem.headID == headData.headID)
            {
                // Check if it's the same instance or duplicate
                if (slot.headItem == headData)
                {
                    Debug.LogWarning($"[DullahanHeadPickable] ⚠️ Head {headData.headName} (ID: {headData.headID}) is already in inventory! Preventing duplicate pickup.");
                    alreadyInInventory = true;
                    break;
                }
            }
        }

        if (alreadyInInventory)
        {
            // Mark as picked up and hide to prevent re-pickup
            isPickedUp = true;
            if (headVisual != null) headVisual.SetActive(false);
            gameObject.SetActive(false);
            return;
        }

        // Check if head inventory is full
        if (headInventory.inventoryList.Count >= headInventory.maxInventorySize)
        {
            Debug.Log("Head inventory is full!");
            return;
        }

        Debug.Log($"[DullahanHeadPickable] Picking up {headData.headName} (ID: {headData.headID})");

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

        // Hide the spawned head visual if it exists
        if (headVisual != null)
        {
            headVisual.SetActive(false);
        }

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
