using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Displays rotating tips/hints during loading screens
/// Attach to a TextMeshProUGUI component to show helpful game tips while loading
/// </summary>
public class LoadingTips : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The TextMeshProUGUI component to display tips. If not assigned, will try to get from this GameObject.")]
    [SerializeField] private TextMeshProUGUI tipsText;

    [Header("Tip Settings")]
    [Tooltip("Time in seconds before changing to next tip")]
    [SerializeField] private float tipChangeInterval = 3f;

    [Tooltip("Shuffle tips randomly instead of sequential order")]
    [SerializeField] private bool randomizeTips = true;

    [Tooltip("Fade transition between tips")]
    [SerializeField] private bool useFadeTransition = true;

    [Tooltip("Fade transition duration in seconds")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Tips Content")]
    [Tooltip("Array of tips to display. Add your own custom tips here!")]
    [SerializeField] private string[] tips = new string[]
    {
        "Tip: Use your flashlight wisely... battery is limited",
        "Tip: Listen carefully for enemy sounds around you",
        "Tip: Some rooms contain valuable clues to progress",
        "Tip: Not all enemies can be avoided... choose your path carefully",
        "Tip: Your stamina depletes when sprinting",
        "Tip: Find safe spots to catch your breath",
        "Did you know: The mansion has multiple floors to explore",
        "Did you know: Different enemies have different behaviors",
        "Hint: Pay attention to environmental sounds",
        "Hint: Sometimes the best action is to hide"
    };

    private int currentIndex = 0;
    private Coroutine rotationCoroutine;

    private void Awake()
    {
        // Auto-find TextMeshProUGUI if not assigned
        if (tipsText == null)
        {
            tipsText = GetComponent<TextMeshProUGUI>();

            if (tipsText == null)
            {
                Debug.LogError($"[LoadingTips] No TextMeshProUGUI component found on {gameObject.name}");
            }
        }
    }

    private void OnEnable()
    {
        if (tipsText != null && tips != null && tips.Length > 0)
        {
            StartRotation();
        }
    }

    private void OnDisable()
    {
        StopRotation();
    }

    /// <summary>
    /// Starts rotating through tips
    /// </summary>
    public void StartRotation()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        // Initialize with random tip if randomization is enabled
        if (randomizeTips)
        {
            currentIndex = Random.Range(0, tips.Length);
        }
        else
        {
            currentIndex = 0;
        }

        rotationCoroutine = StartCoroutine(RotateTips());
    }

    /// <summary>
    /// Stops rotating through tips
    /// </summary>
    public void StopRotation()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    private IEnumerator RotateTips()
    {
        while (true)
        {
            if (tips == null || tips.Length == 0)
            {
                Debug.LogWarning("[LoadingTips] No tips defined in array!");
                yield break;
            }

            // Display current tip
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeToTip(tips[currentIndex]));
            }
            else
            {
                tipsText.text = tips[currentIndex];
            }

            // Wait before changing to next tip
            yield return new WaitForSecondsRealtime(tipChangeInterval);

            // Move to next tip
            if (randomizeTips)
            {
                // Pick a different random tip
                int newIndex = currentIndex;
                if (tips.Length > 1) // Only randomize if there's more than one tip
                {
                    while (newIndex == currentIndex)
                    {
                        newIndex = Random.Range(0, tips.Length);
                    }
                }
                currentIndex = newIndex;
            }
            else
            {
                // Sequential order
                currentIndex = (currentIndex + 1) % tips.Length;
            }
        }
    }

    private IEnumerator FadeToTip(string newTip)
    {
        if (tipsText == null) yield break;

        Color originalColor = tipsText.color;
        float halfFade = fadeDuration * 0.5f;

        // Fade out
        float elapsed = 0f;
        while (elapsed < halfFade)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / halfFade);
            tipsText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Change text
        tipsText.text = newTip;

        // Fade in
        elapsed = 0f;
        while (elapsed < halfFade)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / halfFade);
            tipsText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Ensure fully visible
        tipsText.color = originalColor;
    }

    /// <summary>
    /// Adds a new tip to the tips array at runtime
    /// </summary>
    public void AddTip(string newTip)
    {
        if (string.IsNullOrEmpty(newTip)) return;

        // Expand array and add new tip
        string[] newTips = new string[tips.Length + 1];
        tips.CopyTo(newTips, 0);
        newTips[tips.Length] = newTip;
        tips = newTips;
    }

    /// <summary>
    /// Sets a completely new tips array at runtime
    /// </summary>
    public void SetTips(string[] newTips)
    {
        if (newTips == null || newTips.Length == 0)
        {
            Debug.LogWarning("[LoadingTips] Attempted to set empty tips array!");
            return;
        }

        tips = newTips;
        currentIndex = 0;
    }

    /// <summary>
    /// Immediately shows a specific tip by text
    /// </summary>
    public void ShowSpecificTip(string tipText)
    {
        if (tipsText != null && !string.IsNullOrEmpty(tipText))
        {
            tipsText.text = tipText;
        }
    }
}
