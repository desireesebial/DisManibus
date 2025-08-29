using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerHealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invulnerabilityTime = 1f;
    public int currentHealth;

    [Header("Health UI")]
    public GameObject healthUI;
    public Image[] healthBars = new Image[3];
    public TextMeshProUGUI healthText;

    [Header("Camera Effects")]
    public Camera playerCamera;
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.3f;
    public float damageFlashDuration = 0.2f;
    public Color damageFlashColor = Color.red;

    [Header("Player Controller")]
    public FirstPersonController playerController;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip healSound;
    public AudioClip criticalHealthSound;

    [Header("Post-Processing")]
    public MonoBehaviour postProcessVolume; // Changed from PostProcessVolume to MonoBehaviour for compatibility
    public float blurIntensity = 1f;
    public float blurDuration = 5f; // How long blur lasts for critical health

    // Events
    public System.Action<int> OnHealthChanged;
    public System.Action OnCriticalHealth;
    public System.Action OnPlayerDeath;

    // Private variables
    private bool isInvulnerable = false;
    private Vector3 originalCameraPosition;
    private bool isBlurActive = false;
    private Coroutine blurCoroutine;
    private Coroutine damageFlashCoroutine;

    void Start()
    {
        InitializeHealthSystem();
    }

    void Update()
    {
        HandleDebugInput();
        HandleCriticalHealthBlur();
    }

    private void InitializeHealthSystem()
    {
        // Set initial health
        currentHealth = maxHealth;

        // Find references if not assigned
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerController == null)
            playerController = GetComponent<FirstPersonController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Store original camera position
        if (playerCamera != null)
            originalCameraPosition = playerCamera.transform.localPosition;

        // Store original flash color (not needed for alpha-based flash)
        // CanvasGroup uses alpha for transparency, not color

        // Update UI
        UpdateHealthUI();

        Debug.Log($"Player Health System initialized. Health: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        // Reduce health
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Trigger events
        OnHealthChanged?.Invoke(currentHealth);

        // Visual and audio feedback
        StartCoroutine(CameraShake());
        StartDamageFlash();
        PlayDamageSound();

        // Apply debuffs based on health state
        ApplyHealthDebuffs();

        // Check for critical health
        if (currentHealth == 1)
        {
            OnCriticalHealth?.Invoke();
            StartCriticalHealthBlur();
        }

        // Check for death
        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
            HandlePlayerDeath();
        }

        // Start invulnerability
        StartCoroutine(InvulnerabilityFrames());

        // Update UI
        UpdateHealthUI();

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }

    public void Heal(int healAmount)
    {
        if (currentHealth >= maxHealth) return;

        // Increase health
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);

        // Trigger events
        OnHealthChanged?.Invoke(currentHealth);

        // Audio feedback
        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }

        // Remove debuffs if healed
        RemoveHealthDebuffs();

        // Stop critical health effects if healed above 1
        if (currentHealth > 1 && isBlurActive)
        {
            StopCriticalHealthBlur();
        }

        // Update UI
        UpdateHealthUI();

        Debug.Log($"Player healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }

    public void RestoreFullHealth()
    {
        int healAmount = maxHealth - currentHealth;
        Heal(healAmount);
    }

    private void ApplyHealthDebuffs()
    {
        if (playerController == null) return;

        switch (currentHealth)
        {
            case 2: // Minor injury
                playerController.walkSpeed = 4f; // Reduced from default
                playerController.mouseSensitivity = 1.5f; // Reduced from default
                Debug.Log("Applied minor injury debuffs: Reduced speed and sensitivity");
                break;

            case 1: // Critical injury
                playerController.walkSpeed = 3f; // Further reduced
                playerController.mouseSensitivity = 1f; // Further reduced
                Debug.Log("Applied critical injury debuffs: Major speed and sensitivity reduction");
                break;
        }
    }

    private void RemoveHealthDebuffs()
    {
        if (playerController == null) return;

        // Restore original values (adjust these to match your FirstPersonController defaults)
        playerController.walkSpeed = 6f; // Restore to default
        playerController.mouseSensitivity = 2f; // Restore to default
        Debug.Log("Removed health debuffs: Restored speed and sensitivity");
    }

    private IEnumerator CameraShake()
    {
        if (playerCamera == null) yield break;

        float elapsed = 0f;
        Vector3 originalPos = playerCamera.transform.localPosition;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }

    private void StartDamageFlash()
    {
        if (damageFlashCoroutine != null)
            StopCoroutine(damageFlashCoroutine);

        damageFlashCoroutine = StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        if (healthUI == null) yield break;

        CanvasGroup canvasGroup = healthUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = healthUI.AddComponent<CanvasGroup>();
        }

        // Flash red by changing alpha
        canvasGroup.alpha = 0.3f; // Reduce alpha for flash effect
        yield return new WaitForSeconds(damageFlashDuration);

        // Return to normal
        canvasGroup.alpha = 1f;
    }

    private void StartCriticalHealthBlur()
    {
        if (blurCoroutine != null)
            StopCoroutine(blurCoroutine);

        blurCoroutine = StartCoroutine(CriticalHealthBlur());
    }

    private void StopCriticalHealthBlur()
    {
        if (blurCoroutine != null)
        {
            StopCoroutine(blurCoroutine);
            blurCoroutine = null;
        }

        if (postProcessVolume != null)
        {
            // Disable blur effect
            postProcessVolume.enabled = false;
        }

        isBlurActive = false;
    }

    private IEnumerator CriticalHealthBlur()
    {
        isBlurActive = true;

        while (currentHealth == 1)
        {
            // Enable blur effect if post process volume is assigned
            if (postProcessVolume != null)
            {
                postProcessVolume.enabled = true;
            }

            // Wait for blur duration
            yield return new WaitForSeconds(blurDuration);

            // If still at critical health, continue the cycle
            if (currentHealth == 1)
            {
                // Play critical health sound
                if (audioSource != null && criticalHealthSound != null)
                {
                    audioSource.PlayOneShot(criticalHealthSound);
                }
            }
        }

        // Stop blur when health is restored
        StopCriticalHealthBlur();
    }

    private void HandleCriticalHealthBlur()
    {
        // This is called every frame to check if we need to start/stop blur
        if (currentHealth == 1 && !isBlurActive)
        {
            StartCriticalHealthBlur();
        }
        else if (currentHealth > 1 && isBlurActive)
        {
            StopCriticalHealthBlur();
        }
    }

    private void PlayDamageSound()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("Player has died!");
        
        // Disable player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // You can add death screen, respawn logic, or scene reload here
        // For now, just log the death
    }

    private void UpdateHealthUI()
    {
        // Update health bars
        for (int i = 0; i < healthBars.Length; i++)
        {
            if (healthBars[i] != null)
            {
                healthBars[i].fillAmount = (i < currentHealth) ? 1f : 0f;
            }
        }

        // Update health text
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }

    private void HandleDebugInput()
    {
        // Debug keys for testing
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(1);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RestoreFullHealth();
        }
    }

    private IEnumerator InvulnerabilityFrames()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    // Public getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsInvulnerable() => isInvulnerable;
    public bool IsCriticalHealth() => currentHealth == 1;
    public bool IsDead() => currentHealth <= 0;
}
