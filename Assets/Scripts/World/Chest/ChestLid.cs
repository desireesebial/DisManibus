using System.Collections;
using UnityEngine;

/// <summary>
/// Handles opening and closing a chest lid by rotating a target transform.
/// Call Open/Close/Toggle from other scripts (e.g., ChestPadlock) or events.
/// </summary>
public class ChestLid : MonoBehaviour
{
    [Header("Lid Target")]
    [Tooltip("Transform of the lid to rotate. If null, uses this GameObject's transform.")]
    public Transform lidTransform;

    [Header("Animation")]
    [Tooltip("Local rotation offset applied when opening (degrees). Commonly around -70 on X for a back-hinged lid.")]
    public Vector3 openLocalEulerOffset = new Vector3(-70f, 0f, 0f);
    [Tooltip("Time in seconds to fully open/close the lid.")]
    public float animationDuration = 0.6f;
    [Tooltip("Curve for opening/closing interpolation (0..1).")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("State")]
    [SerializeField]
    private bool startOpen = false;

    private Quaternion _closedLocalRotation;
    private Quaternion _openLocalRotation;
    private Coroutine _animateRoutine;
    private bool _isOpen;

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        if (lidTransform == null)
        {
            lidTransform = transform;
        }

        _closedLocalRotation = lidTransform.localRotation;
        _openLocalRotation = _closedLocalRotation * Quaternion.Euler(openLocalEulerOffset);

        if (startOpen)
        {
            lidTransform.localRotation = _openLocalRotation;
            _isOpen = true;
        }
        else
        {
            lidTransform.localRotation = _closedLocalRotation;
            _isOpen = false;
        }
    }

    private void OnValidate()
    {
        if (lidTransform == null)
        {
            lidTransform = transform;
        }
    }

    public void Open()
    {
        if (_isOpen)
            return;
        PlayAnimation(true);
    }

    public void Close()
    {
        if (!_isOpen)
            return;
        PlayAnimation(false);
    }

    public void Toggle()
    {
        PlayAnimation(!_isOpen);
    }

    private void PlayAnimation(bool open)
    {
        if (_animateRoutine != null)
        {
            StopCoroutine(_animateRoutine);
        }
        _animateRoutine = StartCoroutine(AnimateLid(open));
    }

    private IEnumerator AnimateLid(bool open)
    {
        _isOpen = open;

        Quaternion from = open ? _closedLocalRotation : _openLocalRotation;
        Quaternion to = open ? _openLocalRotation : _closedLocalRotation;

        float duration = Mathf.Max(0.01f, animationDuration);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = animationCurve != null ? animationCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            lidTransform.localRotation = Quaternion.Slerp(from, to, eased);
            yield return null;
        }

        lidTransform.localRotation = to;
        _animateRoutine = null;
    }
}


