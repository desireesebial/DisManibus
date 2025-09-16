using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI Prompt (Optional)")]
    [Tooltip("Optional UI.Text for showing interact prompts.")]
    public Text promptText;
    [Tooltip("Text shown when player is near chest.")]
    public string promptInteractText = "Press E to interact";
    [Tooltip("Text shown when chest is locked and interaction fails.")]
    public string promptLockedText = "Chest is Locked";
    [Tooltip("Show the proximity prompt when within interact range (or inside trigger).")]
    public bool showProximityPrompt = true;
    [Tooltip("Seconds to show the locked message after failed interaction.")]
    public float lockedMessageDuration = 1.5f;

    // Runtime
    private Camera _playerCamera;
    private DullahanHeadInventory _inventory;
    private bool _playerInTrigger = false;
    private bool _isLocked;
    private bool _isOpen;
    private Quaternion _closedLocalRotation;
    private Quaternion _openLocalRotation;
    private Coroutine _animateRoutine;
    private int _effectiveLayerMask;
    private Coroutine _lockedMessageRoutine;

    [Header("Debug / Reliability")]
    [Tooltip("If true, draws a debug ray from the camera center when pressing the interact key.")]
    public bool debugDrawRay = false;
    [Tooltip("Radius for sphere cast fallback if thin ray misses. Set 0 to disable.")]
    public float sphereCastRadius = 0.15f;
    [Tooltip("If true, disables Animator on lid while animating to avoid override.")]
    public bool disableAnimatorOnLid = true;

    // Cached lid components for diagnosing overrides
    private Animator _lidAnimator;
    private Rigidbody _lidRigidbody;
    private HingeJoint _lidHingeJoint;
    private bool _physicsOverrideWarned = false;

    void Awake()
    {
        if (chestRoot == null) chestRoot = transform;
        if (lidTransform == null || lidTransform == transform)
        {
            // Try to auto-find a likely lid child if not explicitly assigned
            Transform found = transform.Find("SM_TreasureChest_Lid");
            if (found == null)
            {
                foreach (Transform t in GetComponentsInChildren<Transform>(true))
                {
                    if (t == transform) continue;
                    if (t.name.IndexOf("lid", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = t;
                        break;
                    }
                }
            }
            if (found != null) lidTransform = found;
            if (lidTransform == null) lidTransform = transform;
        }

        _closedLocalRotation = lidTransform.localRotation;
        _openLocalRotation = _closedLocalRotation * Quaternion.Euler(openLocalEulerOffset);

        _isLocked = startLocked;
        if (lockVisual != null) lockVisual.SetActive(_isLocked);

        // Cache lid components and log selection
        _lidAnimator = lidTransform != null ? lidTransform.GetComponent<Animator>() : null;
        _lidRigidbody = lidTransform != null ? lidTransform.GetComponent<Rigidbody>() : null;
        _lidHingeJoint = lidTransform != null ? lidTransform.GetComponent<HingeJoint>() : null;
        if (lidTransform == transform)
        {
            Debug.LogWarning("TreasureChestController: lidTransform is the root. Assign the actual lid object for visible rotation.");
        }
        Debug.Log($"TreasureChestController: Lid='{(lidTransform!=null?lidTransform.name:"null")}', HasRB={_lidRigidbody!=null}, HasHinge={_lidHingeJoint!=null}, HasAnimator={_lidAnimator!=null}");
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
        
        // Resolve effective raycast layer mask; default to Physics.DefaultRaycastLayers if mask is empty (Nothing)
        _effectiveLayerMask = interactMask.value != 0 ? interactMask.value : Physics.DefaultRaycastLayers;
        if (interactMask.value == 0)
        {
            Debug.LogWarning("TreasureChestController: InteractMask is 'Nothing'. Using Physics.DefaultRaycastLayers.");
        }
        
        Debug.Log($"TreasureChestController initialized. UseTrigger: {useTrigger}, InteractRange: {interactRange}, InteractMask: {interactMask}, EffectiveMask: {_effectiveLayerMask}");
        if (lidTransform != null)
        {
            var closedEuler = _closedLocalRotation.eulerAngles;
            var openEuler = _openLocalRotation.eulerAngles;
            Debug.Log($"TreasureChestController: ClosedEuler={closedEuler}, OpenEuler={openEuler}, Offset={openLocalEulerOffset}");
        }
    }

    void Update()
    {
        if (_playerCamera == null) return;

        // Update the on-screen prompt based on proximity
        UpdatePromptUI();

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
        if (debugDrawRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow, 0.5f);
        }

        bool didHit = Physics.Raycast(ray, out hit, interactRange, _effectiveLayerMask, QueryTriggerInteraction.Collide);
        if (!didHit && sphereCastRadius > 0f)
        {
            didHit = Physics.SphereCast(ray, sphereCastRadius, out hit, interactRange, _effectiveLayerMask, QueryTriggerInteraction.Collide);
        }

        if (didHit)
        {
            Transform t = hit.collider.transform;
            var controller = t.GetComponentInParent<TreasureChestController>();
            Debug.Log($"Raycast hit: {hit.collider.name} on layer {hit.collider.gameObject.layer}. Distance: {hit.distance:F2}");
            
            if (controller != null)
            {
                Debug.Log("Hit detected! Interacting with chest controller.");
                if (controller != this) controller.Interact(); else Interact();
            }
            else
            {
                Debug.Log("Hit object is not a chest (no TreasureChestController in parents)");
            }
        }
        else
        {
            Debug.Log($"No raycast/spherecast hit within range {interactRange} on mask {_effectiveLayerMask}");
        }
    }

    private void UpdatePromptUI()
    {
        if (!showProximityPrompt || promptText == null)
        {
            return;
        }

        bool shouldShow = false;
        if (useTrigger)
        {
            // Require being inside trigger and looking at the chest
            shouldShow = _playerInTrigger && IsLookingAtChest();
        }
        else
        {
            // Raycast aim only (like notes function)
            shouldShow = IsLookingAtChest();
        }

        if (shouldShow)
        {
            if (_lockedMessageRoutine == null)
            {
                promptText.text = promptInteractText;
            }
            if (!promptText.enabled) promptText.enabled = true;
        }
        else
        {
            if (promptText.enabled) promptText.enabled = false;
        }
    }

    private bool IsLookingAtChest()
    {
        if (_playerCamera == null) return false;
        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, interactRange, _effectiveLayerMask, QueryTriggerInteraction.Collide);
        if (!didHit && sphereCastRadius > 0f)
        {
            didHit = Physics.SphereCast(ray, sphereCastRadius, out hit, interactRange, _effectiveLayerMask, QueryTriggerInteraction.Collide);
        }
        if (!didHit) return false;

        Transform t = hit.collider.transform;
        var controller = t.GetComponentInParent<TreasureChestController>();
        return controller == this;
    }

    private IEnumerator ShowLockedPrompt()
    {
        if (promptText == null) yield break;

        // Force show locked text
        promptText.text = promptLockedText;
        promptText.enabled = true;

        float timer = 0f;
        while (timer < lockedMessageDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _lockedMessageRoutine = null;
        // After locked message, either show normal prompt (if still in proximity) or hide
        UpdatePromptUI();
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
            // Show temporary locked prompt
            if (promptText != null)
            {
                if (_lockedMessageRoutine != null) StopCoroutine(_lockedMessageRoutine);
                _lockedMessageRoutine = StartCoroutine(ShowLockedPrompt());
            }
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
        // Prevent Animator from overriding transform if desired
        if (disableAnimatorOnLid && _lidAnimator != null && _lidAnimator.enabled)
        {
            Debug.Log("TreasureChestController: Disabling lid Animator to allow manual rotation.");
            _lidAnimator.enabled = false;
        }
        _animateRoutine = StartCoroutine(Animate(open));
    }

    private IEnumerator Animate(bool open)
    {
        _isOpen = open;
        Quaternion from = open ? _closedLocalRotation : _openLocalRotation;
        Quaternion to = open ? _openLocalRotation : _closedLocalRotation;
        float duration = Mathf.Max(0.01f, animationDuration);
        float t = 0f;
        if (!_physicsOverrideWarned && (_lidRigidbody != null || _lidHingeJoint != null))
        {
            Debug.LogWarning("TreasureChestController: Lid has Rigidbody/HingeJoint. Physics may override transform rotation. Consider making the lid kinematic or removing joints, or drive the joint instead.");
            _physicsOverrideWarned = true;
        }
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


