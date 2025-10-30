using UnityEngine;
using TMPro;

/// <summary>
/// Simple universal nametag - just attach to any GameObject!
/// Shows a floating name when player is nearby and looking at the object.
/// Zero setup required - it just works!
/// </summary>
public class SimpleNametag : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name to display above this object")]
    public string displayName = "Object Name";

    [Tooltip("Color of the name text")]
    public Color nameColor = Color.white;

    [Tooltip("How far the player can see this nametag")]
    public float viewDistance = 10f;

    [Header("Optional")]
    [Tooltip("Height above object (adjust if needed)")]
    public float heightOffset = 2f;

    [Tooltip("Text size")]
    public float textSize = 24f;

    // Private stuff (you don't need to touch these)
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI nameText;
    private Transform player;
    private Camera playerCamera;
    private float currentAlpha = 0f;

    void Start()
    {
        FindPlayer();
        CreateNametag();
        ValidateComponents();
    }

    void Update()
    {
        // Safety checks
        if (player == null || canvas == null || canvasGroup == null || playerCamera == null) return;

        // Check if player is close and looking at us
        float distance = Vector3.Distance(transform.position, player.position);
        bool isClose = distance < viewDistance;
        bool isLookingAt = false;

        if (isClose)
        {
            // Check if player is facing this object
            Vector3 toObject = (transform.position - playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, toObject);
            isLookingAt = angle < 40f; // Cone of vision
        }

        // Show/hide nametag smoothly
        float targetAlpha = (isClose && isLookingAt) ? 1f : 0f;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * 5f);
        canvasGroup.alpha = currentAlpha;

        // Make text bigger when close, smaller when far
        if (currentAlpha > 0.01f)
        {
            // Clamp scale to prevent invalid values
            float normalizedDistance = Mathf.Clamp01(distance / viewDistance);
            float scale = Mathf.Lerp(1.5f, 0.7f, normalizedDistance);
            scale = Mathf.Clamp(scale, 0.3f, 2f);
            canvas.transform.localScale = Vector3.one * scale;

            // Always face the camera (proper billboard rotation)
            Vector3 directionToCamera = playerCamera.transform.position - canvas.transform.position;
            canvas.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    void FindPlayer()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            // Try to find camera
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                // Fallback: find camera in player children
                playerCamera = player.GetComponentInChildren<Camera>();
            }

            if (playerCamera == null)
            {
                Debug.LogWarning($"[SimpleNametag] No camera found for nametag on '{name}'. Nametag will not display.");
            }
        }
        else
        {
            Debug.LogWarning($"[SimpleNametag] No player found (needs 'Player' tag) for nametag on '{name}'.");
        }
    }

    void CreateNametag()
    {
        // Create canvas for the floating text
        GameObject canvasObj = new GameObject("Nametag");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.up * heightOffset;
        canvasObj.transform.localRotation = Quaternion.identity;

        // Setup Canvas component
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000; // Render above other UI

        // Setup CanvasGroup for fading
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Setup RectTransform and scale (CRITICAL: WorldSpace canvas needs tiny scale!)
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200, 50);
        canvasRect.localScale = Vector3.one * 0.01f; // CRITICAL FIX!

        // Create the text object
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;

        // Setup TextMeshPro
        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = displayName;
        nameText.fontSize = textSize;
        nameText.color = nameColor;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.overflowMode = TextOverflowModes.Overflow;
        nameText.enableWordWrapping = false;

        // Setup text RectTransform to fill canvas
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
    }

    void ValidateComponents()
    {
        // Check if all components were created successfully
        if (canvas == null || canvasGroup == null || nameText == null)
        {
            Debug.LogError($"[SimpleNametag] Failed to create nametag components on '{name}'!");
        }
    }

    // Optional: Change name at runtime
    public void SetName(string newName)
    {
        displayName = newName;
        if (nameText != null) nameText.text = newName;
    }

    // Optional: Change color at runtime
    public void SetColor(Color newColor)
    {
        nameColor = newColor;
        if (nameText != null) nameText.color = newColor;
    }

    // Show detection range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}
