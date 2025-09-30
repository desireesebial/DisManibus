using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class doorscript : MonoBehaviour
{
    private static int s_lastInteractFrame = -1; // prevents multiple doors handling the same E press
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;
    public bool isLocked = true;
    public int requiredKeyID = -1; // -1 means no key required
    public KeyItemsSO requiredKeyItem; // optional direct reference to key asset
    public string requiredKeySOId = ""; // empty means no SO key required
    public bool consumeKeyOnUnlock = false; // consume SO key when unlocking
    public bool useSelectedSOKeyOnly = false; // when true, only selected KeySO can unlock
    public bool consumeWrongSelectedKey = true; // consume wrong selected key
    public bool requireSelectedNumericKey = false; // when true, require selected numeric key in hand
    
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
            if (s_lastInteractFrame == Time.frameCount) return; // another door already handled this press
            s_lastInteractFrame = Time.frameCount;
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
        // If an SO key is configured, always use SO logic and selected key interaction
        if (!string.IsNullOrEmpty(requiredKeySOId))
        {
            if (headInventory == null)
            {
                PlayLockedSound();
                ShowLockedMessage();
                return;
            }

            var selectedKey = headInventory.GetSelectedKey();
            if (selectedKey != null)
            {
                if (selectedKey.keyId == requiredKeySOId)
                {
                    if (consumeKeyOnUnlock) headInventory.RemoveSelectedKeyIfKey();
                    UnlockDoor();
                    ToggleDoor();
                }
                else
                {
                    if (consumeWrongSelectedKey) headInventory.RemoveSelectedKeyIfKey();
                    PlayLockedSound();
                    ShowLockedMessage();
                }
                return;
            }
            // No selected key; fall through to locked feedback
            PlayLockedSound();
            ShowLockedMessage();
            return;
        }

        if (HasRequiredKey())
        {
            UnlockDoor();
            // Consume numeric key from PlayerInventory if configured and available
            if (consumeKeyOnUnlock && playerInventory != null)
            {
                int targetKeyId = requiredKeyItem != null ? requiredKeyItem.itemID : requiredKeyID;
                if (targetKeyId != -1)
                {
                    playerInventory.ConsumeKey(targetKeyId);
                }
            }
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
        int targetKeyId = requiredKeyItem != null ? requiredKeyItem.itemID : requiredKeyID;
        if (targetKeyId == -1 && string.IsNullOrEmpty(requiredKeySOId)) return true; // No key required
        
        // If an SO key is specified, only that key can satisfy the door
        if (!string.IsNullOrEmpty(requiredKeySOId))
        {
            if (headInventory != null)
            {
                return headInventory.HasKey(requiredKeySOId);
            }
            return false;
        }
        
        // Check player inventory for keys
        if (playerInventory != null)
        {
            if (requireSelectedNumericKey)
            {
                var current = playerInventory.GetCurrentItem();
                if (current != null && current.item_type == itemType.Keys && current.itemID == targetKeyId)
                    return true;
            }
            else
            {
                foreach (var item in playerInventory.inventoryList)
                {
                    if (item != null && item.item_type == itemType.Keys && item.itemID == targetKeyId)
                    {
                        return true;
                    }
                }
            }
        }
        
        // Check DullahanHeadInventory for SO-based keys
        if (headInventory != null && !string.IsNullOrEmpty(requiredKeySOId))
        {
            if (headInventory.HasKey(requiredKeySOId))
                return true;
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

    private IEnumerator AnimateDoor(bool open, bool propagate = true)
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
        
        // Animate all linked doors (non-recursive)
        if (propagate && linkedDoors != null && linkedDoors.Length > 0)
        {
            foreach (var linkedDoor in linkedDoors)
            {
                if (linkedDoor != null)
                {
                    linkedDoor.StopAllCoroutines();
                    linkedDoor.StartCoroutine(linkedDoor.AnimateDoor(open, false));
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
        
        // Update own state flag; linked doors manage their own state in their coroutine
        
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