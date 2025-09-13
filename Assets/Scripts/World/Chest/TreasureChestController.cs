using System.Collections;
using UnityEngine;

/// <summary>
/// Unified controller for your chest hierarchy:
/// - Root: TreasureChest
/// - Lid:  SM_TreasureChest_Lid (hinge rotates here)
/// - Optional Lock: SM_TreasureChest_Lock (visual hidden when unlocked)
/// Works with crosshair raycast or trigger proximity.
/// Requires: Player tagged object with FirstPersonController and DullahanHeadInventory.
/// </summary>
public class TreasureChestController : MonoBehaviour
{
    [Header("Hierarchy References")]
    [Tooltip("Top-level root of the chest. All child colliders under this root are valid for interaction.")]
    public Transform chestRoot;
    [Tooltip("Transform of the lid that rotates open/closed. For your chest, assign SM_TreasureChest_Lid.")]
    public Transform lidTransform;
    [Tooltip("Optional: lock visual GameObject (e.g., SM_TreasureChest_Lock). Hidden when unlocked.")]
    public GameObject lockVisual;

    [Header("Lock (Optional)")]
    public bool startLocked = false;
    public KeySO requiredKey; // If null, any key unlocks; if set, must match by reference or keyId
    public bool consumeKeyOnUnlock = false;

    [Header("Animation")]
    [Tooltip("Local euler offset to apply when opening. For back hinge lids try X=-70.")]
    public Vector3 openLocalEulerOffset = new Vector3(-70f, 0f, 0f);
    public float animationDuration = 0.6f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 3.0f;
    public LayerMask interactMask = -1;
    public bool useTrigger = false;
    public string playerTag = "Player";

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip unlockClip;
    public AudioClip deniedClip;

    // Runtime
    private Camera _playerCamera;
    private DullahanHeadInventory _inventory;
    private bool _playerInTrigger = false;
    private bool _isLocked;
    private bool _isOpen;
    private Quaternion _closedLocalRotation;
    private Quaternion _openLocalRotation;
    private Coroutine _animateRoutine;

    void Awake()
    {
        if (chestRoot == null) chestRoot = transform;
        if (lidTransform == null) lidTransform = transform;

        _closedLocalRotation = lidTransform.localRotation;
        _openLocalRotation = _closedLocalRotation * Quaternion.Euler(openLocalEulerOffset);

        _isLocked = startLocked;
        if (lockVisual != null) lockVisual.SetActive(_isLocked);
    }

    void Start()
    {
        // Use FirstPersonController singleton reference
        if (FirstPersonController.Instance != null)
        {
            _playerCamera = FirstPersonController.Instance.PlayerCamera;
            _inventory = FirstPersonController.Instance.GetComponent<DullahanHeadInventory>();
            Debug.Log($"TreasureChestController: Found FPC Instance. Camera: {_playerCamera != null}, Inventory: {_inventory != null}");
        }
        else
        {
            // Fallback to old method
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var fpc = player.GetComponent<FirstPersonController>();
                if (fpc != null && fpc.playerCamera != null)
                    _playerCamera = fpc.playerCamera;
                if (_playerCamera == null)
                    _playerCamera = player.GetComponentInChildren<Camera>();
                _inventory = player.GetComponent<DullahanHeadInventory>();
                Debug.Log($"TreasureChestController: Using fallback method. Camera: {_playerCamera != null}, Inventory: {_inventory != null}");
            }
        }
        
        if (_playerCamera == null) 
        {
            _playerCamera = Camera.main;
            Debug.LogWarning("TreasureChestController: No player camera found, using Camera.main");
        }
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        Debug.Log($"TreasureChestController initialized. UseTrigger: {useTrigger}, InteractRange: {interactRange}, InteractMask: {interactMask}");
    }

    void Update()
    {
        if (_playerCamera == null) return;

        if (useTrigger)
        {
            if (_playerInTrigger && Input.GetKeyDown(interactKey))
            {
                Interact();
            }
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            TryRaycastInteract();
        }
    }

    private void TryRaycastInteract()
    {
        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
        {
            Transform t = hit.collider.transform;
            Transform root = chestRoot != null ? chestRoot : transform;
            Debug.Log($"Raycast hit: {hit.collider.name} on layer {hit.collider.gameObject.layer}. Distance: {hit.distance:F2}");
            Debug.Log($"Checking if {t.name} is child of {root.name}");
            
            if (t == root || t.IsChildOf(root))
            {
                Debug.Log("Hit detected! Calling Interact()");
                Interact();
            }
            else
            {
                Debug.Log("Hit object is not part of chest hierarchy");
            }
        }
        else
        {
            Debug.Log($"No raycast hit within range {interactRange} on mask {interactMask}");
        }
    }

    private void Interact()
    {
        Debug.Log($"Interact called! IsLocked: {_isLocked}, IsOpen: {_isOpen}");
        
        if (_isLocked)
        {
            Debug.Log("Chest is locked, checking for key...");
            // Try unlock using selected key
            if (_inventory != null)
            {
                KeySO key = _inventory.GetSelectedKey();
                Debug.Log($"Selected key: {(key != null ? key.keyName : "null")}");
                if (key != null && KeyMatches(key))
                {
                    Debug.Log("Key matches! Unlocking chest...");
                    UnlockInternal(key);
                    Open();
                    return;
                }
                else
                {
                    Debug.Log("Key doesn't match or no key selected");
                }
            }
            else
            {
                Debug.Log("No inventory found");
            }
            PlayClip(deniedClip);
            return;
        }

        Debug.Log("Chest is unlocked, toggling...");
        Toggle();
    }

    private bool KeyMatches(KeySO key)
    {
        if (requiredKey == null) return true;
        if (key == requiredKey) return true;
        if (!string.IsNullOrEmpty(requiredKey.keyId) && key != null)
            return key.keyId == requiredKey.keyId;
        return false;
    }

    private void UnlockInternal(KeySO usedKey)
    {
        _isLocked = false;
        if (lockVisual != null) lockVisual.SetActive(false);
        PlayClip(unlockClip);
        if (consumeKeyOnUnlock && usedKey != null && _inventory != null)
        {
            if (!string.IsNullOrEmpty(usedKey.keyId)) _inventory.ConsumeKey(usedKey.keyId);
            else _inventory.RemoveSelectedKeyIfKey();
        }
    }

    public void Open()
    {
        if (_isOpen) return;
        PlayAnimation(true);
        PlayClip(openClip);
    }

    public void Close()
    {
        if (!_isOpen) return;
        PlayAnimation(false);
        PlayClip(closeClip);
    }

    public void Toggle()
    {
        if (_isOpen) Close(); else Open();
    }

    private void PlayAnimation(bool open)
    {
        if (_animateRoutine != null) StopCoroutine(_animateRoutine);
        _animateRoutine = StartCoroutine(Animate(open));
    }

    private IEnumerator Animate(bool open)
    {
        _isOpen = open;
        Quaternion from = open ? _closedLocalRotation : _openLocalRotation;
        Quaternion to = open ? _openLocalRotation : _closedLocalRotation;
        float duration = Mathf.Max(0.01f, animationDuration);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float e = animationCurve != null ? animationCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            lidTransform.localRotation = Quaternion.Slerp(from, to, e);
            yield return null;
        }
        lidTransform.localRotation = to;
        _animateRoutine = null;
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            _playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useTrigger) return;
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            _playerInTrigger = false;
        }
    }
}


