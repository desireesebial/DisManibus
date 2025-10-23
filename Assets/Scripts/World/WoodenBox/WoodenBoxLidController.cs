using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sliding wooden box lid controller (matches pencil/slider box style).
/// - Raycast interaction from player camera (press E to open/close).
/// - Lid slides along a single local axis by a fixed distance.
/// - Clean transform-based animation with easing; no physics required.
///
/// Setup:
/// - Add this script to the box root (e.g., WoodenBox).
/// - Assign `lidTransform` to the sliding lid object.
/// - Configure `slideAxisLocal` (relative to the lid's parent) and `openDistance`.
/// - Optionally assign `promptText` and audio clips.
/// </summary>
public class WoodenBoxLidController : MonoBehaviour
{
    [Header("Hierarchy References")]
    public Transform boxRoot;
    public Transform lidTransform; // e.g., SM_WoodenBox_Lid

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 3.0f;
    public LayerMask interactMask = -1;
    public bool showProximityPrompt = true;

    [Header("UI Prompt (Optional)")]
    public Text promptText;
    public string promptInteractText = "Press E to open";

    [Header("Sliding Motion")]
    [Tooltip("Local-space axis along which the lid slides (relative to lid's parent).")]
    public Vector3 slideAxisLocal = new Vector3(1f, 0f, 0f);
    [Tooltip("Distance (in local units) the lid travels when opened.")]
    public float openDistance = 0.20f;
    [Tooltip("Seconds to complete an open/close slide.")]
    public float slideDuration = 0.45f;
    [Tooltip("Easing curve for the slide motion (0..1).")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    [Header("Lock System (Optional)")]
    [Tooltip("Is the box locked? Requires a key to open.")]
    public bool isLocked = false;
    [Tooltip("The specific key required to unlock this box (by keyId from KeySO).")]
    public string requiredKeyId = "";
    [Tooltip("Should the key be consumed (removed from inventory) when unlocking?")]
    public bool consumeKeyOnUnlock = false;
    [Tooltip("Reference to the player's head inventory for key checking.")]
    public DullahanHeadInventory headInventory;

    [Header("Lock Audio")]
    public AudioClip unlockClip;
    public AudioClip lockedClip; // plays when trying to open while locked

    // Runtime
    private Camera _playerCamera;
    private bool _isOpen;
    private Vector3 _closedLocalPosition;
    private Vector3 _openLocalPosition;
    private Coroutine _animateRoutine;
    private int _effectiveLayerMask;

    void Awake()
    {
        if (boxRoot == null) boxRoot = transform;
        if (lidTransform == null || lidTransform == transform)
        {
            // try auto-find lid by name contains "lid"
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t == transform) continue;
                if (t.name.IndexOf("lid", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lidTransform = t;
                    break;
                }
            }
            if (lidTransform == null) lidTransform = transform;
        }

        // Cache closed/open positions in local space
        _closedLocalPosition = lidTransform.localPosition;
        Vector3 axis = slideAxisLocal.sqrMagnitude > 0.0001f ? slideAxisLocal.normalized : Vector3.right;
        _openLocalPosition = _closedLocalPosition + axis * openDistance;
    }

    void Start()
    {
        if (FirstPersonController.Instance != null)
        {
            _playerCamera = FirstPersonController.Instance.PlayerCamera;
        }
        if (_playerCamera == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var fpc = player.GetComponent<FirstPersonController>();
                if (fpc != null && fpc.playerCamera != null) _playerCamera = fpc.playerCamera;
            }
        }
        if (_playerCamera == null) _playerCamera = Camera.main;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        _effectiveLayerMask = interactMask.value != 0 ? interactMask.value : Physics.DefaultRaycastLayers;

        // Find head inventory if not assigned (for lock system)
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
    }

    void Update()
    {
        if (_playerCamera == null) return;

        if (showProximityPrompt && promptText != null)
        {
            bool lookingAtBox = IsLookingAtBox();
            promptText.enabled = lookingAtBox;

            if (lookingAtBox)
            {
                // Update prompt text based on lock state
                promptText.text = GetInteractionPromptText();
            }
        }

        if (Input.GetKeyDown(interactKey))
        {
            TryRaycastInteract();
        }
    }

    private string GetInteractionPromptText()
    {
        // If box is locked
        if (isLocked)
        {
            if (headInventory != null)
            {
                KeySO selectedKey = headInventory.GetSelectedKey();
                if (selectedKey != null && selectedKey.keyId == requiredKeyId)
                {
                    // Correct key selected
                    return $"Press {interactKey} to unlock";
                }
                else if (selectedKey != null)
                {
                    // Wrong key selected
                    return "Wrong key";
                }
                else
                {
                    // No key selected
                    return "Locked - Need key";
                }
            }
            else
            {
                return "Locked";
            }
        }
        else
        {
            // Box is unlocked, show normal prompt
            return promptInteractText;
        }
    }

    private bool IsLookingAtBox()
    {
        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, interactRange, _effectiveLayerMask, QueryTriggerInteraction.Collide);
        if (!didHit) return false;
        var controller = hit.collider.transform.GetComponentInParent<WoodenBoxLidController>();
        return controller == this;
    }

    private void TryRaycastInteract()
    {
        if (!IsLookingAtBox()) return;

        // If box is locked, try to unlock it first
        if (isLocked)
        {
            TryUnlockBox();
        }
        else
        {
            // Box is unlocked, normal toggle behavior
            Toggle();
        }
    }

    private void TryUnlockBox()
    {
        // Check if we need a key
        if (string.IsNullOrEmpty(requiredKeyId))
        {
            Debug.LogWarning($"[WoodenBox] Box is locked but no requiredKeyId specified! Unlocking anyway.");
            UnlockBox();
            return;
        }

        // Check if player has the inventory
        if (headInventory == null)
        {
            Debug.LogWarning($"[WoodenBox] Box is locked but no headInventory found! Cannot check for keys.");
            PlayClip(lockedClip);
            return;
        }

        // Check if player has the correct key selected
        KeySO selectedKey = headInventory.GetSelectedKey();
        if (selectedKey != null && selectedKey.keyId == requiredKeyId)
        {
            // Correct key is selected!
            Debug.Log($"[WoodenBox] Correct key selected: {selectedKey.keyName}. Unlocking box!");
            UnlockBox();

            // Consume key if enabled
            if (consumeKeyOnUnlock)
            {
                headInventory.RemoveSelectedKeyIfKey();
                Debug.Log($"[WoodenBox] Key consumed: {selectedKey.keyName}");
            }
        }
        else
        {
            // No key or wrong key selected
            if (selectedKey != null)
            {
                Debug.Log($"[WoodenBox] Wrong key selected: {selectedKey.keyName} (need {requiredKeyId})");
            }
            else
            {
                Debug.Log($"[WoodenBox] No key selected. Need key: {requiredKeyId}");
            }
            PlayClip(lockedClip);
        }
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        DriveLid(true);
        PlayClip(openClip);
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        DriveLid(false);
        PlayClip(closeClip);
    }

    public void Toggle()
    {
        if (_isOpen) Close(); else Open();
    }

    /// <summary>
    /// Unlocks the box (plays unlock sound and automatically opens it).
    /// </summary>
    public void UnlockBox()
    {
        if (!isLocked)
        {
            Debug.Log("[WoodenBox] Box is already unlocked!");
            return;
        }

        Debug.Log("[WoodenBox] Box unlocked!");
        isLocked = false;

        // Play unlock sound
        PlayClip(unlockClip);

        // Automatically open the box after unlocking for smooth UX
        if (!_isOpen)
        {
            Open();
        }
    }

    /// <summary>
    /// Locks the box (can be called from other scripts).
    /// </summary>
    public void LockBox()
    {
        isLocked = true;
        Debug.Log("[WoodenBox] Box locked!");
    }

    /// <summary>
    /// Returns whether the box is currently locked.
    /// </summary>
    public bool IsLocked()
    {
        return isLocked;
    }

    /// <summary>
    /// Sets the required key ID at runtime.
    /// </summary>
    public void SetRequiredKey(string keyId)
    {
        requiredKeyId = keyId;
        Debug.Log($"[WoodenBox] Required key set to: {keyId}");
    }

    /// <summary>
    /// Returns whether the box is currently open.
    /// </summary>
    public bool IsOpen()
    {
        return _isOpen;
    }

    private void DriveLid(bool open)
    {
        // Transform-based slide
        if (_animateRoutine != null) StopCoroutine(_animateRoutine);
        _animateRoutine = StartCoroutine(AnimateSlide(open));
    }

    private IEnumerator AnimateSlide(bool open)
    {
        Vector3 from = open ? _closedLocalPosition : _openLocalPosition;
        Vector3 to = open ? _openLocalPosition : _closedLocalPosition;
        float t = 0f;
        float duration = Mathf.Max(0.01f, slideDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float e = slideCurve != null ? slideCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            lidTransform.localPosition = Vector3.LerpUnclamped(from, to, e);
            yield return null;
        }
        lidTransform.localPosition = to;
        _animateRoutine = null;
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (lidTransform == null) return;
        // Draw slide path for easy alignment
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);

        Vector3 localClosed = Application.isPlaying ? _closedLocalPosition : lidTransform.localPosition;
        Vector3 axis = slideAxisLocal.sqrMagnitude > 0.0001f ? slideAxisLocal.normalized : Vector3.right;
        Vector3 localOpen = localClosed + axis * openDistance;

        Transform parent = lidTransform.parent != null ? lidTransform.parent : lidTransform;
        Vector3 worldClosed = parent.TransformPoint(localClosed);
        Vector3 worldOpen = parent.TransformPoint(localOpen);

        Gizmos.DrawSphere(worldClosed, 0.01f);
        Gizmos.DrawSphere(worldOpen, 0.01f);
        Gizmos.DrawLine(worldClosed, worldOpen);
    }
#endif
}


