using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Animates loading text with animated dots (Loading. -> Loading.. -> Loading... -> repeat)
/// Attach this to a TextMeshProUGUI component for professional loading screen effect
/// </summary>
public class LoadingTextAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The TextMeshProUGUI component to animate. If not assigned, will try to get from this GameObject.")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Animation Settings")]
    [Tooltip("Base text without dots (e.g., 'Loading')")]
    [SerializeField] private string baseText = "Loading";

    [Tooltip("Time between dot changes in seconds")]
    [SerializeField] private float dotInterval = 0.5f;

    [Tooltip("Maximum number of dots to display")]
    [SerializeField] private int maxDots = 3;

    [Header("Optional Suffix")]
    [Tooltip("Optional text to append after dots (e.g., '%' for 'Loading... 45%')")]
    [SerializeField] private string suffix = "";

    private int currentDots = 0;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        // Auto-find TextMeshProUGUI if not assigned
        if (loadingText == null)
        {
            loadingText = GetComponent<TextMeshProUGUI>();

            if (loadingText == null)
            {
                Debug.LogError($"[LoadingTextAnimator] No TextMeshProUGUI component found on {gameObject.name}");
            }
        }
    }

    private void OnEnable()
    {
        if (loadingText != null)
        {
            StartAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    /// <summary>
    /// Starts the dot animation
    /// </summary>
    public void StartAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateDots());
    }

    /// <summary>
    /// Stops the dot animation
    /// </summary>
    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator AnimateDots()
    {
        currentDots = 0;

        while (true)
        {
            // Cycle through 0 to maxDots
            currentDots = (currentDots % (maxDots + 1));

            // Build string with appropriate number of dots
            string dots = new string('.', currentDots);

            // Update text
            if (loadingText != null)
            {
                loadingText.text = baseText + dots + suffix;
            }

            currentDots++;

            // Use unscaled time so animation continues even if game is paused
            yield return new WaitForSecondsRealtime(dotInterval);
        }
    }

    /// <summary>
    /// Updates the base text during runtime
    /// </summary>
    public void SetBaseText(string newBaseText)
    {
        baseText = newBaseText;
    }

    /// <summary>
    /// Updates the suffix text during runtime (useful for percentages)
    /// </summary>
    public void SetSuffix(string newSuffix)
    {
        suffix = newSuffix;
    }
}
