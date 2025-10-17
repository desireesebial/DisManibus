using UnityEngine;
using System.Collections;

/// <summary>
/// Simple, robust head placement puzzle for Dullahan's body
/// Press F near Dullahan to place head from inventory
/// </summary>
public class SimpleHeadPlacement : MonoBehaviour
{
    [Header("Required Setup")]
    [Tooltip("ID of the correct head (usually 1 for real head)")]
    public int correctHeadID = 1;
    
    [Tooltip("Where to attach the head visually")]
    public Transform headAttachPoint;
    
    [Tooltip("Pre-placed head model (will be shown when correct head placed)")]
    public GameObject attachedHeadModel;
    
    [Header("Interaction")]
    [Tooltip("How close player needs to be to interact")]
    public float interactionDistance = 5f;
    
    [Tooltip("UI text for interaction prompt")]
    public TMPro.TextMeshProUGUI interactionText;
    
    [Header("Wrong Head Behavior")]
    [Tooltip("Show wrong head briefly before it disappears")]
    public bool showWrongHeadBriefly = true;
    
    [Tooltip("How long to show wrong head")]
    public float wrongHeadDuration = 2f;
    
    [Header("Dullahan Freeze (Optional)")]
    [Tooltip("Freeze Dullahan when player picks up a head")]
    public bool freezeDullahanWithHead = true;
    
    [Tooltip("Start with Dullahan frozen")]
    public bool startFrozen = false;
    
    [Header("Audio (Optional)")]
    public AudioClip correctHeadSound;
    public AudioClip wrongHeadSound;
    
    [Header("Rewards (Optional)")]
    public Door rewardDoor;
    public GameObject[] rewardItems;
    
    // Private references
    private Transform player;
    private DullahanHeadInventory inventory;
    private DullahanChaseSystem chaseSystem;
    private UnityEngine.AI.NavMeshAgent dullahanAgent;
    private AudioSource audioSource;
    private GameObject currentWrongHead;
    
    // State
    private bool puzzleComplete = false;
    private bool playerHoldingHead = false;
    private bool isDullahanFrozen = false;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
        
        // Find inventory
        inventory = FindObjectOfType<DullahanHeadInventory>();
        
        // Find Dullahan components
        GameObject dullahanObj = GameObject.FindGameObjectWithTag("Dullahan");
        if (dullahanObj)
        {
            chaseSystem = dullahanObj.GetComponent<DullahanChaseSystem>();
            dullahanAgent = dullahanObj.GetComponent<UnityEngine.AI.NavMeshAgent>();
        }
        
        // Audio
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Hide attached head initially
        if (attachedHeadModel) attachedHeadModel.SetActive(false);
        
        // Hide interaction text
        if (interactionText) interactionText.gameObject.SetActive(false);
        
        // Start frozen if needed
        if (startFrozen) FreezeDullahan();
        
        Debug.Log("[SimpleHeadPlacement] Initialized successfully");
    }
    
    void Update()
    {
        if (puzzleComplete || !player || !inventory) return;
        
        // Check if player is holding a head
        bool nowHoldingHead = inventory.GetCurrentHead() != null;
        
        // Freeze/unfreeze Dullahan based on head holding
        if (freezeDullahanWithHead)
        {
            if (nowHoldingHead && !playerHoldingHead)
            {
                FreezeDullahan();
            }
            else if (!nowHoldingHead && playerHoldingHead)
            {
                UnfreezeDullahan();
            }
        }
        
        playerHoldingHead = nowHoldingHead;
        
        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactionDistance;
        
        // Show/hide interaction prompt
        if (interactionText)
        {
            bool showPrompt = inRange && playerHoldingHead;
            interactionText.gameObject.SetActive(showPrompt);
            
            if (showPrompt)
            {
                DullahanHeadSO currentHead = inventory.GetCurrentHead();
                if (currentHead != null)
                {
                    bool isCorrect = currentHead.headID == correctHeadID;
                    interactionText.text = isCorrect ? 
                        "Press F to attach head" : 
                        "Press F to attach head (this might not be right...)";
                    interactionText.color = isCorrect ? Color.green : Color.yellow;
                }
            }
        }
        
        // Handle F key press
        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            TryPlaceHead();
        }
    }
    
    void TryPlaceHead()
    {
        if (!inventory) return;
        
        // Get currently selected head
        DullahanHeadSO head = inventory.GetCurrentHead();
        
        if (head == null)
        {
            Debug.Log("[SimpleHeadPlacement] No head selected");
            return;
        }
        
        Debug.Log($"[SimpleHeadPlacement] Trying to place head: {head.headName} (ID: {head.headID})");
        
        // Check if correct
        if (head.headID == correctHeadID)
        {
            PlaceCorrectHead(head);
        }
        else
        {
            PlaceWrongHead(head);
        }
    }
    
    void PlaceCorrectHead(DullahanHeadSO head)
    {
        Debug.Log("[SimpleHeadPlacement] ✓ CORRECT HEAD PLACED!");
        
        // Remove from inventory
        inventory.RemoveSelectedHeadIfHead();
        
        // Show attached head
        if (attachedHeadModel)
        {
            attachedHeadModel.SetActive(true);
            CleanupComponents(attachedHeadModel);
        }
        else if (head.headPrefab && headAttachPoint)
        {
            GameObject headInstance = Instantiate(head.headPrefab, headAttachPoint);
            headInstance.transform.localPosition = Vector3.zero;
            headInstance.transform.localRotation = Quaternion.identity;
            headInstance.transform.localScale = Vector3.one;
            CleanupComponents(headInstance);
        }
        
        // Play sound
        if (correctHeadSound) audioSource.PlayOneShot(correctHeadSound);
        
        // Mark complete
        puzzleComplete = true;
        
        // Unfreeze Dullahan
        UnfreezeDullahan();
        
        // Grant rewards
        if (rewardDoor) rewardDoor.UnlockDoor();
        
        if (rewardItems != null)
        {
            foreach (var item in rewardItems)
            {
                if (item) Instantiate(item, headAttachPoint.position, Quaternion.identity);
            }
        }
        
        // Hide prompt
        if (interactionText) interactionText.gameObject.SetActive(false);
        
        // Notify event managers
        Floor2EndingEventManager eventManager = FindObjectOfType<Floor2EndingEventManager>();
        if (eventManager) eventManager.OnRealHeadAttached();
        
        Debug.Log("[SimpleHeadPlacement] Puzzle completed successfully!");
    }
    
    void PlaceWrongHead(DullahanHeadSO head)
    {
        Debug.Log($"[SimpleHeadPlacement] ✗ WRONG HEAD: {head.headName}");
        
        // Remove from inventory
        inventory.RemoveSelectedHeadIfHead();
        
        // Play sound
        if (wrongHeadSound) audioSource.PlayOneShot(wrongHeadSound);
        
        // Show wrong head briefly
        if (showWrongHeadBriefly && head.headPrefab && headAttachPoint)
        {
            StartCoroutine(ShowWrongHeadTemporarily(head));
        }
        
        // Apply effects
        DullahanHeadEffectManager effectManager = FindObjectOfType<DullahanHeadEffectManager>();
        if (effectManager && head.hasEffect)
        {
            effectManager.ApplyHeadEffect(head);
        }
        
        // Unfreeze Dullahan (punishment - starts chasing again)
        UnfreezeDullahan();
    }
    
    IEnumerator ShowWrongHeadTemporarily(DullahanHeadSO head)
    {
        // Create temporary head
        currentWrongHead = Instantiate(head.headPrefab, headAttachPoint);
        currentWrongHead.transform.localPosition = Vector3.zero;
        currentWrongHead.transform.localRotation = Quaternion.identity;
        currentWrongHead.transform.localScale = Vector3.one;
        
        // Make it non-interactive
        CleanupComponents(currentWrongHead);
        
        Debug.Log($"[SimpleHeadPlacement] Showing wrong head for {wrongHeadDuration} seconds...");
        
        // Wait
        yield return new WaitForSeconds(wrongHeadDuration);
        
        // Remove it
        if (currentWrongHead)
        {
            Destroy(currentWrongHead);
            currentWrongHead = null;
        }
        
        Debug.Log("[SimpleHeadPlacement] Wrong head removed");
    }
    
    void FreezeDullahan()
    {
        if (isDullahanFrozen) return;
        
        Debug.Log("[SimpleHeadPlacement] Freezing Dullahan");
        
        if (chaseSystem) chaseSystem.EndChase();
        if (dullahanAgent)
        {
            dullahanAgent.isStopped = true;
            dullahanAgent.velocity = Vector3.zero;
        }
        
        isDullahanFrozen = true;
    }
    
    void UnfreezeDullahan()
    {
        if (!isDullahanFrozen) return;
        
        Debug.Log("[SimpleHeadPlacement] Unfreezing Dullahan");
        
        if (dullahanAgent) dullahanAgent.isStopped = false;
        
        if (chaseSystem)
        {
            if (puzzleComplete)
                chaseSystem.StartPatrol();
            else
                chaseSystem.StartChase();
        }
        
        isDullahanFrozen = false;
    }
    
    void CleanupComponents(GameObject obj)
    {
        // Remove all interactive components recursively
        Rigidbody[] rbs = obj.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs) if (rb) Destroy(rb);
        
        Collider[] cols = obj.GetComponentsInChildren<Collider>(true);
        foreach (var col in cols) if (col) Destroy(col);
        
        DullahanHeadPickable[] pickables = obj.GetComponentsInChildren<DullahanHeadPickable>(true);
        foreach (var pickable in pickables) if (pickable) Destroy(pickable);
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
