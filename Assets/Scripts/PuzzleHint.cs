using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PuzzleHint : MonoBehaviour
{
    [Header("Hint Settings")]
    public string hintText = "Look for clues in the environment...";
    public string combinationHint = "";
    public bool isRevealed = false;
    public bool canBeRevealed = true;
    
    [Header("Display Settings")]
    public GameObject hintUI;
    public Text hintTextDisplay;
    public Text combinationTextDisplay;
    public Button closeButton;
    public Button revealButton;
    
    [Header("Interaction")]
    public float interactionDistance = 2f;
    public KeyCode interactKey = KeyCode.E;
    public string interactionPrompt = "Press E to examine";
    
    [Header("Visual Effects")]
    public GameObject glowEffect;
    public Material normalMaterial;
    public Material highlightedMaterial;
    public Renderer objectRenderer;
    
    [Header("Audio")]
    public AudioClip examineSound;
    public AudioClip revealSound;
    
    [Header("Events")]
    public UnityEvent onHintRevealed;
    public UnityEvent onHintExamined;
    
    private AudioSource audioSource;
    private bool isUIOpen = false;
    private Transform player;
    private SimplePlayerMovement playerMovement;
    private Material originalMaterial;
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<SimplePlayerMovement>();
        }
        
        // Setup renderer
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
        
        // Setup UI
        SetupUI();
        
        // Hide UI initially
        if (hintUI != null)
        {
            hintUI.SetActive(false);
        }
        
        // Setup glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }
    
    void Update()
    {
        // Check for interaction input
        if (Input.GetKeyDown(interactKey) && IsPlayerInRange())
        {
            if (!isUIOpen)
            {
                OpenHintUI();
            }
            else
            {
                CloseHintUI();
            }
        }
        
        // Close UI if player moves too far away
        if (isUIOpen && !IsPlayerInRange())
        {
            CloseHintUI();
        }
        
        // Update visual effects
        UpdateVisualEffects();
    }
    
    void SetupUI()
    {
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseHintUI);
        }
        
        // Setup reveal button
        if (revealButton != null)
        {
            revealButton.onClick.AddListener(RevealHint);
            revealButton.gameObject.SetActive(canBeRevealed && !isRevealed);
        }
        
        // Update text displays
        UpdateTextDisplays();
    }
    
    void UpdateTextDisplays()
    {
        if (hintTextDisplay != null)
        {
            hintTextDisplay.text = hintText;
        }
        
        if (combinationTextDisplay != null)
        {
            if (isRevealed && !string.IsNullOrEmpty(combinationHint))
            {
                combinationTextDisplay.text = "Combination: " + combinationHint;
                combinationTextDisplay.gameObject.SetActive(true);
            }
            else
            {
                combinationTextDisplay.gameObject.SetActive(false);
            }
        }
    }
    
    void OpenHintUI()
    {
        if (hintUI != null)
        {
            hintUI.SetActive(true);
            isUIOpen = true;
            
            // Disable player movement
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }
            
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Play examine sound
            PlaySound(examineSound);
            
            // Trigger examine event
            onHintExamined?.Invoke();
        }
    }
    
    void CloseHintUI()
    {
        if (hintUI != null)
        {
            hintUI.SetActive(false);
            isUIOpen = false;
            
            // Re-enable player movement
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
            
            // Hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void RevealHint()
    {
        if (canBeRevealed && !isRevealed)
        {
            isRevealed = true;
            
            // Update UI
            UpdateTextDisplays();
            
            if (revealButton != null)
            {
                revealButton.gameObject.SetActive(false);
            }
            
            // Play reveal sound
            PlaySound(revealSound);
            
            // Trigger reveal event
            onHintRevealed?.Invoke();
            
            Debug.Log($"Hint revealed: {combinationHint}");
        }
    }
    
    void UpdateVisualEffects()
    {
        bool inRange = IsPlayerInRange();
        
        // Update glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(inRange);
        }
        
        // Update material
        if (objectRenderer != null)
        {
            if (inRange && highlightedMaterial != null)
            {
                objectRenderer.material = highlightedMaterial;
            }
            else if (originalMaterial != null)
            {
                objectRenderer.material = originalMaterial;
            }
        }
    }
    
    bool IsPlayerInRange()
    {
        if (player == null) return false;
        
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= interactionDistance;
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public methods for external control
    
    public void SetHintText(string newHintText)
    {
        hintText = newHintText;
        UpdateTextDisplays();
    }
    
    public void SetCombinationHint(string newCombinationHint)
    {
        combinationHint = newCombinationHint;
        UpdateTextDisplays();
    }
    
    public void ForceReveal()
    {
        if (!isRevealed)
        {
            RevealHint();
        }
    }
    
    public void ResetHint()
    {
        isRevealed = false;
        UpdateTextDisplays();
        
        if (revealButton != null)
        {
            revealButton.gameObject.SetActive(canBeRevealed);
        }
    }
    
    public bool IsRevealed()
    {
        return isRevealed;
    }
    
    // Gizmos for interaction range visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
} 