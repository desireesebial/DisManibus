using UnityEngine;

/// <summary>
/// Makes a UI element continuously follow another UI element's position and anchoring.
/// Useful when UI elements are in different parents but need to move together.
/// </summary>
[ExecuteAlways] // Works in both edit and play mode
public class UIAnchorFollower : MonoBehaviour
{
    [Header("Target to Follow")]
    [Tooltip("The UI element (RectTransform) to follow")]
    public RectTransform targetToFollow;

    [Header("Offset from Target")]
    [Tooltip("Horizontal offset from target (in pixels)")]
    public float offsetX = 0f;

    [Tooltip("Vertical offset from target (in pixels)")]
    public float offsetY = -50f; // Default: below target

    [Header("Options")]
    [Tooltip("Copy the target's anchor settings")]
    public bool matchAnchors = true;

    [Tooltip("Copy the target's pivot settings")]
    public bool matchPivot = true;

    [Tooltip("Update every frame (keep checked for responsive UI)")]
    public bool updateContinuously = true;

    [Tooltip("Show debug lines in Scene view")]
    public bool showDebugLine = true;

    private RectTransform myRectTransform;

    void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (myRectTransform == null)
        {
            myRectTransform = GetComponent<RectTransform>();
        }

        // Initial update
        UpdatePosition();
    }

    void Update()
    {
        if (updateContinuously)
        {
            UpdatePosition();
        }
    }

    void LateUpdate()
    {
        // Also update in LateUpdate to ensure it happens after all other UI updates
        if (updateContinuously)
        {
            UpdatePosition();
        }
    }

    public void UpdatePosition()
    {
        if (targetToFollow == null || myRectTransform == null)
            return;

        // Match anchors if enabled
        if (matchAnchors)
        {
            myRectTransform.anchorMin = targetToFollow.anchorMin;
            myRectTransform.anchorMax = targetToFollow.anchorMax;
        }

        // Match pivot if enabled
        if (matchPivot)
        {
            myRectTransform.pivot = targetToFollow.pivot;
        }

        // Calculate position with offset
        Vector2 targetPosition = targetToFollow.anchoredPosition;
        myRectTransform.anchoredPosition = new Vector2(
            targetPosition.x + offsetX,
            targetPosition.y + offsetY
        );
    }

    void OnDrawGizmos()
    {
        if (showDebugLine && targetToFollow != null && myRectTransform != null)
        {
            // Draw a line connecting this UI to the target
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(myRectTransform.position, targetToFollow.position);
        }
    }

    // Context menu for manual update
    [ContextMenu("Update Position Now")]
    void ManualUpdate()
    {
        if (myRectTransform == null)
        {
            myRectTransform = GetComponent<RectTransform>();
        }
        UpdatePosition();
        Debug.Log($"[UIAnchorFollower] Updated {gameObject.name} to follow {targetToFollow?.name}");
    }
}
