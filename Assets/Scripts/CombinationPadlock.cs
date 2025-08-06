using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CombinationPadlock : MonoBehaviour
{
    [Header("Padlock Settings")]
    public string correctCombination = "1234";
    public bool isLocked = true;
    public bool isSolved = false;
    
    [Header("UI References")]
    public GameObject padlockUI;
    public Text combinationDisplay;
    public Text messageText;
    public Button[] numberButtons;
    public Button enterButton;
    public Button clearButton;
    public Button closeButton;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip unlockSound;
    
    [Header("Events")]
    public UnityEvent onPadlockUnlocked;
    public UnityEvent onPadlockOpened;
    
    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public string interactionPrompt = "Press E to interact with padlock";
    
    private string currentCombination = "";
    private AudioSource audioSource;
    private bool isUIOpen = false;
    private Transform player;
    private SimplePlayerMovement playerMovement;
    
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
        
        // Setup UI
        SetupUI();
        
        // Hide UI initially
        if (padlockUI != null)
        {
            padlockUI.SetActive(false);
        }
        
        // Update display
        UpdateCombinationDisplay();
    }
    
    void Update()
    {
        // Check for interaction input
        if (Input.GetKeyDown(interactKey) && IsPlayerInRange())
        {
            if (!isUIOpen && !isSolved)
            {
                OpenPadlockUI();
            }
            else if (isUIOpen)
            {
                ClosePadlockUI();
            }
        }
        
        // Close UI if player moves too far away
        if (isUIOpen && !IsPlayerInRange())
        {
            ClosePadlockUI();
        }
    }
    
    void SetupUI()
    {
        // Setup number buttons (0-9)
        for (int i = 0; i < numberButtons.Length && i < 10; i++)
        {
            int number = i;
            numberButtons[i].onClick.AddListener(() => AddNumber(number));
        }
        
        // Setup other buttons
        if (enterButton != null)
        {
            enterButton.onClick.AddListener(CheckCombination);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearCombination);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePadlockUI);
        }
    }
    
    void AddNumber(int number)
    {
        if (currentCombination.Length < 4)
        {
            currentCombination += number.ToString();
            UpdateCombinationDisplay();
            PlaySound(buttonClickSound);
        }
    }
    
    void ClearCombination()
    {
        currentCombination = "";
        UpdateCombinationDisplay();
        PlaySound(buttonClickSound);
    }
    
    void CheckCombination()
    {
        if (currentCombination.Length != 4)
        {
            ShowMessage("Please enter a 4-digit code", Color.yellow);
            PlaySound(incorrectSound);
            return;
        }
        
        if (currentCombination == correctCombination)
        {
            // Correct combination!
            isLocked = false;
            isSolved = true;
            ShowMessage("Correct! Padlock unlocked!", Color.green);
            PlaySound(correctSound);
            
            // Trigger events
            onPadlockUnlocked?.Invoke();
            
            // Close UI after a short delay
            Invoke("ClosePadlockUI", 2f);
        }
        else
        {
            // Wrong combination
            ShowMessage("Incorrect code. Try again.", Color.red);
            PlaySound(incorrectSound);
            ClearCombination();
        }
    }
    
    void UpdateCombinationDisplay()
    {
        if (combinationDisplay != null)
        {
            // Show asterisks for entered digits
            string display = "";
            for (int i = 0; i < currentCombination.Length; i++)
            {
                display += "*";
            }
            
            // Add dashes for remaining digits
            for (int i = currentCombination.Length; i < 4; i++)
            {
                display += "-";
            }
            
            combinationDisplay.text = display;
        }
    }
    
    void ShowMessage(string message, Color color)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
            
            // Clear message after 3 seconds
            Invoke("ClearMessage", 3f);
        }
    }
    
    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
    
    void OpenPadlockUI()
    {
        if (padlockUI != null)
        {
            padlockUI.SetActive(true);
            isUIOpen = true;
            
            // Disable player movement
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }
            
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Clear any previous input
            ClearCombination();
            ClearMessage();
        }
    }
    
    void ClosePadlockUI()
    {
        if (padlockUI != null)
        {
            padlockUI.SetActive(false);
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
    
    // Public method to open the padlock (for story progression)
    public void UnlockPadlock()
    {
        isLocked = false;
        isSolved = true;
        onPadlockUnlocked?.Invoke();
    }
    
    // Public method to set a new combination
    public void SetCombination(string newCombination)
    {
        if (newCombination.Length == 4 && int.TryParse(newCombination, out _))
        {
            correctCombination = newCombination;
        }
    }
    
    // Public method to reset the padlock
    public void ResetPadlock()
    {
        isLocked = true;
        isSolved = false;
        currentCombination = "";
        UpdateCombinationDisplay();
    }
    
    // Gizmos for interaction range visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
} 