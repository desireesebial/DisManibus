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
    }

    void Update()
    {
        if (_playerCamera == null) return;

        if (showProximityPrompt && promptText != null)
        {
            promptText.enabled = IsLookingAtBox();
            if (promptText.enabled) promptText.text = promptInteractText;
        }

        if (Input.GetKeyDown(interactKey))
        {
            TryRaycastInteract();
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
        Toggle();
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


