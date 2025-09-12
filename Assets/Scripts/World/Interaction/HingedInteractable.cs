using System.Collections;
using UnityEngine;

/// <summary>
/// A unified door-style interactable for any hinged object (doors, chests, lids).
/// - Rotates a hinge transform open/closed with an animation curve.
/// - Optional lock using a KeySO. If locked, interaction tries player's selected key.
/// - Works with center-screen (crosshair) raycast like a typical door script.
/// </summary>
public class HingedInteractable : MonoBehaviour
{
    [Header("Hinge Target")]
    [Tooltip("Transform that pivots open/closed. Defaults to this transform.")]
    public Transform hingeTransform;
    [Tooltip("Local Euler offset from closed to open state (degrees). For a back-hinged lid often X = -70.")]
    public Vector3 openLocalEulerOffset = new Vector3(0f, 90f, 0f);
    [Tooltip("Start open at scene load.")]
    public bool startOpen = false;

    [Header("Animation")]
    public float animationDuration = 0.6f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Lock (Optional)")]
    public bool isLocked = false;
    [Tooltip("If set, only this key (or matching keyId) can unlock.")]
    public KeySO requiredKey;
    [Tooltip("Hide/disable this visual once unlocked.")]
    public GameObject lockVisual;
    [Tooltip("Remove key from inventory when used to unlock.")]
    public bool consumeKeyOnUnlock = false;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip unlockClip;
    public AudioClip deniedClip;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 3.0f;
    public LayerMask interactMask = -1;
    [Tooltip("Root of the object hierarchy. Colliders under this root are valid hits. Defaults to transform.root.")]
    public Transform objectRoot;

    // Runtime
    private Camera _playerCamera;
    private DullahanHeadInventory _inventory;
    private Quaternion _closedLocalRotation;
    private Quaternion _openLocalRotation;
    private bool _isOpen;
    private Coroutine _animateRoutine;

    public bool IsOpen => _isOpen;

    void Awake()
    {
        if (hingeTransform == null)
            hingeTransform = transform;
        if (objectRoot == null)
            objectRoot = transform.root;

        _closedLocalRotation = hingeTransform.localRotation;
        _openLocalRotation = _closedLocalRotation * Quaternion.Euler(openLocalEulerOffset);

        if (startOpen)
        {
            hingeTransform.localRotation = _openLocalRotation;
            _isOpen = true;
        }
        else
        {
            hingeTransform.localRotation = _closedLocalRotation;
            _isOpen = false;
        }
    }

    void Start()
    {
        // Resolve camera and inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var fpc = player.GetComponent<FirstPersonController>();
            if (fpc != null && fpc.playerCamera != null)
                _playerCamera = fpc.playerCamera;
            if (_playerCamera == null)
                _playerCamera = player.GetComponentInChildren<Camera>();
            _inventory = player.GetComponent<DullahanHeadInventory>();
        }

        if (_playerCamera == null)
            _playerCamera = Camera.main;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_playerCamera == null) return;
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
        {
            Transform hitT = hit.collider.transform;
            Transform root = objectRoot != null ? objectRoot : transform;
            if (hitT == root || hitT.IsChildOf(root))
            {
                InteractLogic();
            }
        }
    }

    private void InteractLogic()
    {
        if (isLocked)
        {
            // Attempt to unlock with selected key
            if (_inventory != null)
            {
                KeySO selectedKey = _inventory.GetSelectedKey();
                if (selectedKey != null && KeyMatches(selectedKey))
                {
                    UnlockInternal(selectedKey);
                    Open();
                    return;
                }
            }
            PlayDenied();
            return;
        }

        Toggle();
    }

    private bool KeyMatches(KeySO key)
    {
        if (requiredKey == null) return true; // Any key allowed when not specified
        if (key == requiredKey) return true;
        if (!string.IsNullOrEmpty(requiredKey.keyId) && key != null)
            return key.keyId == requiredKey.keyId;
        return false;
    }

    private void UnlockInternal(KeySO usedKey)
    {
        isLocked = false;
        if (lockVisual != null)
            lockVisual.SetActive(false);
        PlayClip(unlockClip);

        if (consumeKeyOnUnlock && usedKey != null && _inventory != null)
        {
            if (!string.IsNullOrEmpty(usedKey.keyId))
            {
                _inventory.ConsumeKey(usedKey.keyId);
            }
            else
            {
                _inventory.RemoveSelectedKeyIfKey();
            }
        }
    }

    public void Lock()
    {
        if (isLocked) return;
        isLocked = true;
        if (lockVisual != null)
            lockVisual.SetActive(true);
    }

    public void Unlock()
    {
        if (!isLocked) return;
        UnlockInternal(requiredKey);
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
            hingeTransform.localRotation = Quaternion.Slerp(from, to, e);
            yield return null;
        }
        hingeTransform.localRotation = to;
        _animateRoutine = null;
    }

    private void PlayDenied()
    {
        PlayClip(deniedClip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}


