using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class doorscript : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;
    public bool isLocked = true;
    public int requiredKeyID = -1; // -1 means no key required
    
    [Header("Multiple Doors")]
    public doorscript[] linkedDoors; // Doors that should open/close together
    public bool autoLinkDoors = false; // Automatically link doors with similar names
    
    [Header("Door Components")]
    public Animator doorAnimator;
    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    public AudioClip doorLockedSound;
    
    [Header("UI")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;
    public string openText = "Press E to open";
    public string closeText = "Press E to close";
    public string lockedText = "Door is locked";
    
    [Header("Integration")]
    public DullahanHeadInventory headInventory;
    public PlayerInventory playerInventory;
    
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;
    private bool _playerInRange = false;
    private bool _isAnimating = false;

    void Start()
    {
        InitializeDoor();
    }

    void Update()
    {
        HandlePlayerInteraction();
        UpdateUI();
    }

    private void InitializeDoor()
    {
        // Set initial rotations
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        
        // Find references if not assigned
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
            
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
            
        if (doorAudioSource == null)
            doorAudioSource = GetComponent<AudioSource>();
            
        // Auto-link doors if enabled
        if (autoLinkDoors)
        {
            AutoLinkDoors();
        }
            
        // Set initial state
        if (isOpen)
        {
            transform.rotation = _openRotation;
        }
        else
        {
            transform.rotation = _closedRotation;
        }
        
        // Hide UI initially
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void HandlePlayerInteraction()
    {
        if (!_playerInRange || _isAnimating) return;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isLocked)
            {
                TryUnlockDoor();
            }
            else
            {
                ToggleDoor();
            }
        }
    }

    private void TryUnlockDoor()
    {
        if (HasRequiredKey())
        {
            UnlockDoor();
            ToggleDoor();
        }
        else
        {
            PlayLockedSound();
            ShowLockedMessage();
        }
    }

    private bool HasRequiredKey()
    {
        if (requiredKeyID == -1) return true; // No key required
        
        // Check player inventory for keys
        if (playerInventory != null)
        {
            foreach (var item in playerInventory.inventoryList)
            {
                if (item != null && item.item_type == itemType.Keys && item.itemID == requiredKeyID)
                {
                    return true;
                }
            }
        }
        
        // Check head inventory for special keys (if needed)
        if (headInventory != null)
        {
            // You can add special head-based unlocking logic here
            // For example, if a specific head acts as a key
        }
        
        return false;
    }

    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log($"Door {gameObject.name} unlocked!");
    }

    public void LockDoor()
    {
        isLocked = true;
        Debug.Log($"Door {gameObject.name} locked!");
    }

    public void ToggleDoor()
    {
        if (_isAnimating) return;
        
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
            
        _currentCoroutine = StartCoroutine(AnimateDoor(!isOpen));
    }

    public void OpenDoor()
    {
        if (!isOpen && !_isAnimating)
        {
            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(AnimateDoor(true));
        }
    }

    public void CloseDoor()
    {
        if (isOpen && !_isAnimating)
        {
            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(AnimateDoor(false));
        }
    }

    private IEnumerator AnimateDoor(bool open)
    {
        _isAnimating = true;
        
        Quaternion targetRotation = open ? _openRotation : _closedRotation;
        Quaternion startRotation = transform.rotation;
        float timeElapsed = 0f;
        
        // Play sound
        PlayDoorSound(open);
        
        // Trigger animation if available
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsOpen", open);
        }
        
        // Animate all linked doors
        if (linkedDoors != null && linkedDoors.Length > 0)
        {
            foreach (var linkedDoor in linkedDoors)
            {
                if (linkedDoor != null)
                {
                    linkedDoor.isOpen = open;
                    if (linkedDoor.doorAnimator != null)
                    {
                        linkedDoor.doorAnimator.SetBool("IsOpen", open);
                    }
                }
            }
        }
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            timeElapsed += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, timeElapsed);
            yield return null;
        }
        
        transform.rotation = targetRotation;
        isOpen = open;
        
        // Update linked doors
        if (linkedDoors != null)
        {
            foreach (var linkedDoor in linkedDoors)
            {
                if (linkedDoor != null)
                {
                    linkedDoor.isOpen = open;
                    linkedDoor.transform.rotation = open ? linkedDoor._openRotation : linkedDoor._closedRotation;
                }
            }
        }
        
        _isAnimating = false;
        Debug.Log($"Door {gameObject.name} {(open ? "opened" : "closed")}!");
    }

    private void PlayDoorSound(bool opening)
    {
        if (doorAudioSource == null) return;
        
        AudioClip clipToPlay = opening ? doorOpenSound : doorCloseSound;
        if (clipToPlay != null)
        {
            doorAudioSource.PlayOneShot(clipToPlay);
        }
    }

    private void PlayLockedSound()
    {
        if (doorAudioSource != null && doorLockedSound != null)
        {
            doorAudioSource.PlayOneShot(doorLockedSound);
        }
    }

    private void ShowLockedMessage()
    {
        if (interactionText != null)
        {
            interactionText.text = lockedText;
            StartCoroutine(ResetInteractionText());
        }
    }

    private IEnumerator ResetInteractionText()
    {
        yield return new WaitForSeconds(2f);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (interactionUI == null) return;
        
        if (_playerInRange && !_isAnimating)
        {
            interactionUI.SetActive(true);
            
            if (interactionText != null)
            {
                if (isLocked)
                {
                    interactionText.text = lockedText;
                }
                else
                {
                    interactionText.text = isOpen ? closeText : openText;
                }
            }
        }
        else
        {
            interactionUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }

    // Public methods for external control
    public bool IsOpen() => isOpen;
    public bool IsLocked() => isLocked;
    public bool IsAnimating() => _isAnimating;
    
    // Method to set required key ID
    public void SetRequiredKeyID(int keyID)
    {
        requiredKeyID = keyID;
    }
    
    // Method to unlock without animation (for puzzle completion)
    public void ForceUnlock()
    {
        isLocked = false;
        Debug.Log($"Door {gameObject.name} force unlocked!");
    }
    
    // Method to open without animation (for puzzle completion)
    public void ForceOpen()
    {
        isOpen = true;
        transform.rotation = _openRotation;
        
        // Update linked doors
        if (linkedDoors != null)
        {
            foreach (var linkedDoor in linkedDoors)
            {
                if (linkedDoor != null)
                {
                    linkedDoor.isOpen = true;
                    linkedDoor.transform.rotation = linkedDoor._openRotation;
                }
            }
        }
    }
    
    // Method to close without animation
    public void ForceClose()
    {
        isOpen = false;
        transform.rotation = _closedRotation;
        
        // Update linked doors
        if (linkedDoors != null)
        {
            foreach (var linkedDoor in linkedDoors)
            {
                if (linkedDoor != null)
                {
                    linkedDoor.isOpen = false;
                    linkedDoor.transform.rotation = linkedDoor._closedRotation;
                }
            }
        }
    }
    
    // Auto-link doors with similar names
    private void AutoLinkDoors()
    {
        if (linkedDoors != null && linkedDoors.Length > 0) return; // Already linked
        
        doorscript[] allDoors = FindObjectsOfType<doorscript>();
        List<doorscript> similarDoors = new List<doorscript>();
        
        string baseName = gameObject.name.ToLower();
        
        foreach (var door in allDoors)
        {
            if (door != this && door.gameObject.name.ToLower().Contains(baseName))
            {
                similarDoors.Add(door);
            }
        }
        
        if (similarDoors.Count > 0)
        {
            linkedDoors = similarDoors.ToArray();
            Debug.Log($"Auto-linked {linkedDoors.Length} doors for {gameObject.name}");
        }
    }
    
    // Public method to set linked doors
    public void SetLinkedDoors(doorscript[] doors)
    {
        linkedDoors = doors;
    }
}