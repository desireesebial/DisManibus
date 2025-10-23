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

    [Header("Visibility Control")]
    public bool changeVisibilityOnInteract = false;
    public bool interactVisibilityState = true;
    public bool changeVisibilityOnCorrectCode = false;
    public bool correctCodeVisibilityState = true;
    public bool affectColliders = false;
    public GameObject[] visibilityTargets;
    
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;
    private bool _playerInRange = false;
    private bool _isAnimating = false;
    private readonly List<Renderer> _visibilityRenderers = new List<Renderer>();
    private readonly List<Collider> _visibilityColliders = new List<Collider>();
    private bool _visibilityCacheInitialized = false;

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
        {
            doorAudioSource = GetComponent<AudioSource>();
            if (doorAudioSource == null)
            {
                // Check if we have any audio clips assigned
                bool hasAudioClips = doorOpenSound != null || doorCloseSound != null || doorLockedSound != null;

                if (hasAudioClips)
                {
                    // Auto-create AudioSource since clips are assigned
                    Debug.Log($"[Door] {gameObject.name}: Audio clips assigned but no AudioSource. Creating one automatically.");
                    doorAudioSource = gameObject.AddComponent<AudioSource>();
                    doorAudioSource.playOnAwake = false;
                    doorAudioSource.spatialBlend = 1f; // 3D sound for doors
                    doorAudioSource.minDistance = 1f;
                    doorAudioSource.maxDistance = 15f;
                    Debug.Log($"[Door] {gameObject.name}: AudioSource created and configured.");
                }
                else
                {
                    // No audio clips assigned, silently skip (audio is optional)
                    Debug.Log($"[Door] {gameObject.name}: No AudioSource and no audio clips assigned. Audio disabled (this is fine).");
                }
            }
            else
            {
                Debug.Log($"[Door] {gameObject.name}: AudioSource found.");
            }
        }
            
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

        CacheVisibilityTargets();
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
                if (TryUnlockDoor())
                {
                    ApplyInteractVisibility();
                }
            }
            else
            {
                ToggleDoor();
                ApplyInteractVisibility();
            }
        }
    }

    private bool TryUnlockDoor()
    {
        // If an SO key is configured, always use SO logic and selected key interaction
        if (!string.IsNullOrEmpty(requiredKeySOId))
        {
            if (headInventory == null)
            {
                PlayLockedSound();
                ShowLockedMessage();
                return false;
            }

            var selectedKey = headInventory.GetSelectedKey();
            if (selectedKey != null)
            {
                if (selectedKey.keyId == requiredKeySOId)
                {
                    if (consumeKeyOnUnlock) headInventory.RemoveSelectedKeyIfKey();
                    UnlockDoor();
                    ToggleDoor();
                    return true;
                }
                else
                {
                    if (consumeWrongSelectedKey) headInventory.RemoveSelectedKeyIfKey();
                    PlayLockedSound();
                    ShowLockedMessage();
                    return false;
                }
            }
            // No selected key; fall through to locked feedback
            PlayLockedSound();
            ShowLockedMessage();
            return false;
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
            return true;
        }
        else
        {
            PlayLockedSound();
            ShowLockedMessage();
            return false;
        }

        return false;
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
        
        Debug.Log($"[Door] ToggleDoor called for {gameObject.name}. Current state: isOpen={isOpen}, isLocked={isLocked}");
        
        // Ensure the GameObject is active before starting a coroutine
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"Cannot start coroutine on inactive door: {gameObject.name}. Activating it first.");
            gameObject.SetActive(true);
            // Start with delay after activation
            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(ToggleDoorAfterActivation(!isOpen));
            return;
        }
        
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
            
        _currentCoroutine = StartCoroutine(AnimateDoor(!isOpen));
    }
    
    // Helper coroutine to wait a frame after activation
    private IEnumerator ToggleDoorAfterActivation(bool openState)
    {
        yield return null; // Wait one frame for GameObject to fully activate
        _currentCoroutine = StartCoroutine(AnimateDoor(openState));
    }

    public void OpenDoor()
    {
        if (!isOpen && !_isAnimating)
        {
            // Ensure the GameObject is active before starting a coroutine
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"Cannot start coroutine on inactive door: {gameObject.name}. Activating it first.");
                gameObject.SetActive(true);
                // Start with delay after activation
                if (_currentCoroutine != null)
                    StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(ToggleDoorAfterActivation(true));
                return;
            }
            
            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(AnimateDoor(true));
        }
    }

    public void CloseDoor()
    {
        if (isOpen && !_isAnimating)
        {
            // Ensure the GameObject is active before starting a coroutine
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"Cannot start coroutine on inactive door: {gameObject.name}. Activating it first.");
                gameObject.SetActive(true);
                // Start with delay after activation
                if (_currentCoroutine != null)
                    StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(ToggleDoorAfterActivation(false));
                return;
            }
            
            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(AnimateDoor(false));
        }
    }

    private IEnumerator AnimateDoor(bool open, bool propagate = true)
    {
        _isAnimating = true;
        
        Debug.Log($"[Door] AnimateDoor called for {gameObject.name}. Opening: {open}, isLocked: {isLocked}");
        
        Quaternion targetRotation = open ? _openRotation : _closedRotation;
        Quaternion startRotation = transform.rotation;
        float timeElapsed = 0f;
        float maxAnimationTime = 10f; // Safety timeout (10 seconds max)
        float animationStartTime = Time.time;
        
        // Safety check for openSpeed
        if (openSpeed <= 0)
        {
            Debug.LogWarning($"[Door] Invalid openSpeed: {openSpeed}. Setting to 2.");
            openSpeed = 2f;
        }
        
        // Play sound
        Debug.Log($"[Door] About to play sound for {gameObject.name}. Sound type: {(open ? "OPEN" : "CLOSE")}");
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
                if (linkedDoor != null && linkedDoor.gameObject != null)
                {
                    // Ensure linked door is active before starting coroutine
                    if (!linkedDoor.gameObject.activeInHierarchy)
                    {
                        Debug.LogWarning($"Linked door {linkedDoor.gameObject.name} is inactive. Activating it.");
                        linkedDoor.gameObject.SetActive(true);
                    }
                    
                    linkedDoor.StopAllCoroutines();
                    linkedDoor.StartCoroutine(linkedDoor.AnimateDoor(open, false));
                }
            }
        }
        
        // Animation loop with timeout protection
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            // Check for timeout to prevent infinite loops
            if (Time.time - animationStartTime > maxAnimationTime)
            {
                Debug.LogError($"[Door] Animation timeout for {gameObject.name}! Forcing completion.");
                break;
            }
            
            // Check for zero deltaTime (can happen on some systems)
            if (Time.deltaTime <= 0)
            {
                Debug.LogWarning($"[Door] Zero deltaTime detected. Skipping frame.");
                yield return null;
                continue;
            }
            
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
        if (doorAudioSource == null) 
        {
            Debug.LogWarning($"[Door] AudioSource is null for {gameObject.name}. Cannot play sound.");
            return;
        }
        
        AudioClip clipToPlay = opening ? doorOpenSound : doorCloseSound;
        if (clipToPlay != null)
        {
            Debug.Log($"[Door] Playing {(opening ? "open" : "close")} sound for {gameObject.name}");
            doorAudioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"[Door] {(opening ? "doorOpenSound" : "doorCloseSound")} is null for {gameObject.name}. Cannot play sound.");
        }
    }

    private void PlayLockedSound()
    {
        if (doorAudioSource == null)
        {
            Debug.LogWarning($"[Door] AudioSource is null for {gameObject.name}. Cannot play locked sound.");
            return;
        }
        
        if (doorLockedSound != null)
        {
            Debug.Log($"[Door] Playing locked sound for {gameObject.name}");
            doorAudioSource.PlayOneShot(doorLockedSound);
        }
        else
        {
            Debug.LogWarning($"[Door] doorLockedSound is null for {gameObject.name}. Cannot play locked sound.");
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
    
    // Public method to test door sounds
    public void TestDoorSounds()
    {
        Debug.Log($"[Door] Testing sounds for {gameObject.name}");
        Debug.Log($"[Door] AudioSource: {(doorAudioSource != null ? "Found" : "NULL")}");
        Debug.Log($"[Door] Open Sound: {(doorOpenSound != null ? doorOpenSound.name : "NULL")}");
        Debug.Log($"[Door] Close Sound: {(doorCloseSound != null ? doorCloseSound.name : "NULL")}");
        Debug.Log($"[Door] Locked Sound: {(doorLockedSound != null ? doorLockedSound.name : "NULL")}");
        
        if (doorAudioSource != null)
        {
            Debug.Log($"[Door] AudioSource Volume: {doorAudioSource.volume}");
            Debug.Log($"[Door] AudioSource Mute: {doorAudioSource.mute}");
            Debug.Log($"[Door] AudioSource Enabled: {doorAudioSource.enabled}");
        }
    }

    public void SetDoorVisibility(bool visible)
    {
        SetVisibilityState(visible);
    }

    public void ApplyKeypadVisibility()
    {
        if (changeVisibilityOnCorrectCode)
        {
            SetVisibilityState(correctCodeVisibilityState);
        }
    }

    public void RefreshVisibilityTargets()
    {
        CacheVisibilityTargets();
    }

    private void ApplyInteractVisibility()
    {
        if (changeVisibilityOnInteract)
        {
            SetVisibilityState(interactVisibilityState);
        }
    }

    private void CacheVisibilityTargets()
    {
        _visibilityRenderers.Clear();
        _visibilityColliders.Clear();

        if (visibilityTargets == null || visibilityTargets.Length == 0)
        {
            AddVisibilityComponents(gameObject);
        }
        else
        {
            foreach (var target in visibilityTargets)
            {
                AddVisibilityComponents(target);
            }
        }

        _visibilityCacheInitialized = true;
    }

    private void EnsureVisibilityCache()
    {
        if (!_visibilityCacheInitialized)
        {
            CacheVisibilityTargets();
        }
    }

    private void AddVisibilityComponents(GameObject target)
    {
        if (target == null) return;

        var renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer != null && !_visibilityRenderers.Contains(renderer))
            {
                _visibilityRenderers.Add(renderer);
            }
        }

        var colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (var collider in colliders)
        {
            if (collider != null && !_visibilityColliders.Contains(collider))
            {
                _visibilityColliders.Add(collider);
            }
        }
    }

    private void SetVisibilityState(bool visible)
    {
        EnsureVisibilityCache();

        foreach (var renderer in _visibilityRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }

        if (affectColliders)
        {
            foreach (var collider in _visibilityColliders)
            {
                if (collider != null)
                {
                    collider.enabled = visible;
                }
            }
        }
    }
}