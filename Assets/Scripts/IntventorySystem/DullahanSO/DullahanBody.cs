using UnityEngine;
using System.Collections;

public class DullahanBody : MonoBehaviour
{
    [Header("Body Settings")]
    public int requiredHeadID = 1; // ID of the real head
    public string bodyName = "Dullahan Body";
    public bool hasHead = false;
    
    [Header("Door Settings")]
    public Door finalDoor;
    public int doorKeyID = 999; // Special key ID for the final door
    
    [Header("Visual Effects")]
    public GameObject headAttachmentPoint;
    public GameObject headVisual;
    public Light bodyLight;
    public Material bodyMaterial;
    public Material activeBodyMaterial;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip headAttachedSound;
    public AudioClip doorUnlockSound;
    public AudioClip ambientSound;
    
    [Header("Particle Effects")]
    public ParticleSystem headAttachEffect;
    public ParticleSystem doorUnlockEffect;
    
    [Header("Animation")]
    public Animator bodyAnimator;
    public string attachHeadTrigger = "AttachHead";
    public string activateTrigger = "Activate";
    
    [Header("UI")]
    public GameObject interactionUI;
    public TMPro.TextMeshProUGUI interactionTextUI;
    public string attachText = "Press E to attach head";
    public string completedText = "Head attached - Door unlocked!";
    
    [Header("Game Completion")]
    public bool gameCompleted = false;
    public float completionDelay = 2f;
    public string nextSceneName = "MainMenu";
    
    private bool playerInRange = false;
    private Transform player;
    private PlayerInventory playerInventory;
    private DullahanAudioManager audioManager;
    private bool isAttaching = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerInventory = player.GetComponent<PlayerInventory>();
        audioManager = FindObjectOfType<DullahanAudioManager>();
        
        // Setup audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Setup visual effects
        SetupVisualEffects();
        
        // Setup UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
            
        // Hide head visual initially
        if (headVisual != null)
            headVisual.SetActive(false);
    }
    
    void Update()
    {
        if (hasHead || gameCompleted) return;
        
        CheckPlayerDistance();
        HandleInteraction();
    }
    
    void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= 3f;
        
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
        if (!playerInRange || isAttaching) return;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryAttachHead();
        }
    }
    
    void TryAttachHead()
    {
        if (hasHead || playerInventory == null) return;
        
        // Check if player has the required head
        KeyItemsSO requiredHead = FindRequiredHead();
        
        if (requiredHead != null)
        {
            StartCoroutine(AttachHead(requiredHead));
        }
        else
        {
            Debug.Log("You need the real Dullahan head to attach to the body!");
            // Could play a "wrong head" sound here
        }
    }
    
    KeyItemsSO FindRequiredHead()
    {
        if (playerInventory.inventoryList == null) return null;
        
        foreach (KeyItemsSO item in playerInventory.inventoryList)
        {
            if (item.itemID == requiredHeadID)
            {
                return item;
            }
        }
        
        return null;
    }
    
    IEnumerator AttachHead(KeyItemsSO headItem)
    {
        isAttaching = true;
        
        // Remove head from inventory
        playerInventory.inventoryList.Remove(headItem);
        
        // Play attach sound
        if (audioManager != null)
        {
            audioManager.PlayHeadAttachSound();
        }
        else if (headAttachedSound != null)
        {
            audioSource.PlayOneShot(headAttachedSound);
        }
        
        // Show head visual
        if (headVisual != null)
            headVisual.SetActive(true);
        
        // Play particle effect
        if (headAttachEffect != null)
            headAttachEffect.Play();
        
        // Trigger animation
        if (bodyAnimator != null)
            bodyAnimator.SetTrigger(attachHeadTrigger);
        
        // Wait for animation
        yield return new WaitForSeconds(1f);
        
        // Mark as having head
        hasHead = true;
        
        // Update visual effects
        UpdateBodyVisuals();
        
        // Unlock the door
        UnlockFinalDoor();
        
        // Show completion message
        ShowCompletionMessage();
        
        // Wait before completing game
        yield return new WaitForSeconds(completionDelay);
        
        // Complete the game
        CompleteGame();
        
        isAttaching = false;
    }
    
    void UnlockFinalDoor()
    {
        if (finalDoor == null) return;
        
        // Create a special key for the door
        KeyItemsSO doorKey = ScriptableObject.CreateInstance<KeyItemsSO>();
        doorKey.itemName = "Dullahan's Key";
        doorKey.itemID = doorKeyID;
        doorKey.description = "The key to freedom";
        doorKey.item_type = itemType.Keys;
        
        // Add key to player inventory
        if (playerInventory != null)
        {
            playerInventory.inventoryList.Add(doorKey);
        }
        
        // Play unlock sound
        if (audioManager != null)
        {
            audioManager.PlayDoorUnlockSound();
        }
        else if (doorUnlockSound != null)
        {
            audioSource.PlayOneShot(doorUnlockSound);
        }
        
        // Play unlock effect
        if (doorUnlockEffect != null)
            doorUnlockEffect.Play();
        
        Debug.Log("Final door unlocked! The Dullahan's head has been restored.");
    }
    
    void UpdateBodyVisuals()
    {
        // Change material
        if (bodyMaterial != null && activeBodyMaterial != null)
        {
            Renderer bodyRenderer = GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material = activeBodyMaterial;
            }
        }
        
        // Activate light
        if (bodyLight != null)
        {
            bodyLight.enabled = true;
            StartCoroutine(PulseLight());
        }
        
        // Trigger activation animation
        if (bodyAnimator != null)
            bodyAnimator.SetTrigger(activateTrigger);
    }
    
    IEnumerator PulseLight()
    {
        if (bodyLight == null) yield break;
        
        float originalIntensity = bodyLight.intensity;
        
        while (hasHead)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.3f + 1f;
            bodyLight.intensity = originalIntensity * pulse;
            yield return null;
        }
    }
    
    void ShowCompletionMessage()
    {
        if (interactionUI != null && interactionTextUI != null)
        {
            interactionTextUI.text = completedText;
            interactionUI.SetActive(true);
            
            // Hide after 5 seconds
            StartCoroutine(HideCompletionMessage(5f));
        }
    }
    
    IEnumerator HideCompletionMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    void CompleteGame()
    {
        gameCompleted = true;
        
        Debug.Log("Congratulations! You have completed the game!");
        
        // Could trigger game completion events here
        // - Show credits
        // - Save game state
        // - Load next scene
        
        // For now, just log completion
        // In a real implementation, you might want to:
        // SceneManager.LoadScene(nextSceneName);
    }
    
    void SetupVisualEffects()
    {
        // Setup light
        if (bodyLight != null)
        {
            bodyLight.enabled = false;
        }
        
        // Setup materials
        if (bodyMaterial != null)
        {
            Renderer bodyRenderer = GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material = bodyMaterial;
            }
        }
    }
    
    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionTextUI != null)
                interactionTextUI.text = attachText;
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
    
    // Public methods for external access
    public bool HasHead()
    {
        return hasHead;
    }
    
    public bool IsGameCompleted()
    {
        return gameCompleted;
    }
    
    public void SetFinalDoor(Door door)
    {
        finalDoor = door;
    }
}
