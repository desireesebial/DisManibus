using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarSliderUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealthSystem playerHealthSystem;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image sliderFillImage;

    [Header("Color Settings")]
    [Tooltip("Gradient from green (healthy) -> orange (injured) -> red (critical)")]
    [SerializeField] private bool useColorGradient = true;
    [SerializeField] private Color healthyColor = new Color(0.1f, 0.8f, 0.1f); // Green
    [SerializeField] private Color injuredColor = new Color(1f, 0.65f, 0.1f); // Orange
    [SerializeField] private Color criticalColor = new Color(0.9f, 0.1f, 0.1f); // Red

    [Header("Shake Settings")]
    [SerializeField] private bool enableShake = true;
    [SerializeField] private float shakeIntensity = 15f;
    [SerializeField] private float shakeDuration = 0.4f;

    [Header("Animation Settings")]
    [Tooltip("Enable smooth transition animation when health changes")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionDuration = 0.4f;

    // Private variables
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private Coroutine transitionCoroutine;
    private int lastKnownHealth;
    private float targetSliderValue;
    private Color targetColor;

    private void Awake()
    {
        // Get RectTransform for shake effect
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }

        // Find references if not assigned
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();
            if (playerHealthSystem == null)
            {
                Debug.LogError("[HealthBarSliderUI] PlayerHealthSystem not found!");
            }
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
            if (healthSlider == null)
            {
                Debug.LogError("[HealthBarSliderUI] Slider component not found!");
            }
        }

        // Configure slider to prevent user interaction and glitching
        if (healthSlider != null)
        {
            healthSlider.interactable = false; // Prevent user from dragging the slider
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.wholeNumbers = false;
        }

        // Get fill image if not assigned
        if (sliderFillImage == null && healthSlider != null)
        {
            sliderFillImage = healthSlider.fillRect?.GetComponent<Image>();
            if (sliderFillImage == null)
            {
                Debug.LogWarning("[HealthBarSliderUI] Fill Image not found. Color gradient will not work.");
            }
        }
    }

    private void Start()
    {
        // Subscribe to health changed event
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged.AddListener(OnHealthChanged);

            // Initialize with current health
            lastKnownHealth = playerHealthSystem.CurrentHealth;
            UpdateHealthBar(playerHealthSystem.CurrentHealth);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged.RemoveListener(OnHealthChanged);
        }
    }

    private void OnHealthChanged(int newHealth)
    {
        // Check if this was damage (health decreased)
        bool tookDamage = newHealth < lastKnownHealth;

        // Update the health bar
        UpdateHealthBar(newHealth);

        // Trigger shake effect if player took damage
        if (tookDamage && enableShake)
        {
            TriggerShake();
        }

        // Update last known health
        lastKnownHealth = newHealth;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        if (healthSlider == null || playerHealthSystem == null) return;

        // Calculate health percentage (0.0 to 1.0)
        float healthPercentage = (float)currentHealth / playerHealthSystem.MaxHealth;

        // Store target values
        targetSliderValue = healthPercentage;
        targetColor = GetHealthColor(healthPercentage);

        if (smoothTransition)
        {
            // Use smooth animation
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(SmoothTransitionCoroutine());
        }
        else
        {
            // Instant update
            healthSlider.value = healthPercentage;
            if (useColorGradient && sliderFillImage != null)
            {
                sliderFillImage.color = targetColor;
            }
        }
    }

    private Color GetHealthColor(float healthPercentage)
    {
        // Smooth gradient: Green (100%-70%) -> Orange (70%-30%) -> Red (30%-0%)
        if (healthPercentage > 0.7f)
        {
            // Healthy range: Green to slightly less green
            return Color.Lerp(injuredColor, healthyColor, (healthPercentage - 0.7f) / 0.3f);
        }
        else if (healthPercentage > 0.3f)
        {
            // Injured range: Orange transition
            return Color.Lerp(criticalColor, injuredColor, (healthPercentage - 0.3f) / 0.4f);
        }
        else
        {
            // Critical range: Red with slight darkening as it approaches zero
            return Color.Lerp(criticalColor * 0.7f, criticalColor, healthPercentage / 0.3f);
        }
    }

    private void TriggerShake()
    {
        if (rectTransform == null) return;

        // Stop existing shake coroutine if running
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            rectTransform.anchoredPosition = originalPosition;
        }

        // Start new shake
        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Calculate shake offset with decreasing intensity over time
            float progress = elapsed / shakeDuration;
            float currentIntensity = shakeIntensity * (1f - progress);

            // Random offset
            float xOffset = Random.Range(-1f, 1f) * currentIntensity;
            float yOffset = Random.Range(-1f, 1f) * currentIntensity;

            // Apply shake
            rectTransform.anchoredPosition = originalPosition + new Vector3(xOffset, yOffset, 0);

            elapsed += Time.unscaledDeltaTime; // Use unscaled time to work even when game is paused
            yield return null;
        }

        // Reset to original position
        rectTransform.anchoredPosition = originalPosition;
        shakeCoroutine = null;
    }

    private IEnumerator SmoothTransitionCoroutine()
    {
        if (healthSlider == null) yield break;

        float startValue = healthSlider.value;
        Color startColor = sliderFillImage != null ? sliderFillImage.color : Color.white;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / transitionDuration;

            // Use smooth step for more natural easing
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Lerp slider value
            healthSlider.value = Mathf.Lerp(startValue, targetSliderValue, smoothProgress);

            // Lerp color if gradient is enabled
            if (useColorGradient && sliderFillImage != null)
            {
                sliderFillImage.color = Color.Lerp(startColor, targetColor, smoothProgress);
            }

            yield return null;
        }

        // Ensure we reach the exact target values
        healthSlider.value = targetSliderValue;
        if (useColorGradient && sliderFillImage != null)
        {
            sliderFillImage.color = targetColor;
        }

        transitionCoroutine = null;
    }

    // Public methods for runtime configuration
    public void SetShakeEnabled(bool enabled)
    {
        enableShake = enabled;
    }

    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }

    public void SetShakeDuration(float duration)
    {
        shakeDuration = duration;
    }

    public void SetColorGradientEnabled(bool enabled)
    {
        useColorGradient = enabled;
        if (playerHealthSystem != null)
        {
            UpdateHealthBar(playerHealthSystem.CurrentHealth);
        }
    }

    public void SetSmoothTransitionEnabled(bool enabled)
    {
        smoothTransition = enabled;
    }

    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0.1f, duration); // Minimum 0.1s to prevent too fast animations
    }

    // Manual update method (useful for testing or special cases)
    public void ForceUpdateHealthBar()
    {
        if (playerHealthSystem != null)
        {
            UpdateHealthBar(playerHealthSystem.CurrentHealth);
        }
    }
}
