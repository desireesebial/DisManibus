using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI statusText;
    public GameObject deathMessageUI;
    public TextMeshProUGUI deathMessageText;

    [Header("Death Screen UI")]
    public Button retryButton; // Reload current scene
    public Button mainMenuButton; // Go to main menu
    public string mainMenuSceneName = "MainMenu";
    public bool pauseOnDeath = true;

    [Header("Health UI Colors")]
    public Color healthyBarColor = new Color(0.1f, 0.8f, 0.1f);
    public Color injuredBarColor = new Color(1f, 0.65f, 0.1f);
    public Color criticalBarColor = new Color(0.9f, 0.1f, 0.1f);

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

    [Header("Enemy Tags & Damage")]
    public string jenglotTag = "Jenglot";
    public int jenglotDamage = 1;
    public string kamatayanTag = "Kamatayan";
    public int kamatayanDamage = 1;
    public string dullahanTag = "Dullahan";
    public int dullahanDamage = 1;

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
    private float originalWalkSpeed;
    private float originalMouseSensitivity;
    private bool hasDied = false;

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

        // Cache original movement-related values from FirstPersonController
        if (playerController != null)
        {
            originalWalkSpeed = playerController.walkSpeed;
            originalMouseSensitivity = playerController.mouseSensitivity;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Store original camera position
        if (playerCamera != null)
            originalCameraPosition = playerCamera.transform.localPosition;

        // Store original flash color (not needed for alpha-based flash)
        // CanvasGroup uses alpha for transparency, not color

        // Update UI
        UpdateHealthUI();
        UpdateStatusUI();

        Debug.Log($"Player Health System initialized. Health: {currentHealth}/{maxHealth}");

        // Ensure death UI is hidden and time/cursor are normal at start
        if (deathMessageUI != null)
            deathMessageUI.SetActive(false);
        hasDied = false;
        if (pauseOnDeath)
            Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Wire death screen buttons
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RestartLevel);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
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
        UpdateStatusUI();

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
                playerController.walkSpeed = originalWalkSpeed * 0.8f;
                playerController.mouseSensitivity = originalMouseSensitivity * 0.75f;
                Debug.Log("Applied minor injury debuffs: Reduced speed and sensitivity");
                break;

            case 1: // Critical injury
                playerController.walkSpeed = originalWalkSpeed * 0.6f;
                playerController.mouseSensitivity = originalMouseSensitivity * 0.5f;
                Debug.Log("Applied critical injury debuffs: Major speed and sensitivity reduction");
                break;
        }
    }

    private void RemoveHealthDebuffs()
    {
        if (playerController == null) return;

        // Restore original values captured from FirstPersonController
        playerController.walkSpeed = originalWalkSpeed;
        playerController.mouseSensitivity = originalMouseSensitivity;
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
        if (hasDied) return;
        hasDied = true;
        Debug.Log("Player has died!");
        
        // Disable player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Stop effects
        StopCriticalHealthBlur();

        // Show death UI and pause/cursor handling
        ShowDeathScreen();
    }

    private void UpdateHealthUI()
    {
        // Update health bars
        for (int i = 0; i < healthBars.Length; i++)
        {
            if (healthBars[i] != null)
            {
                bool isActive = i < currentHealth;
                healthBars[i].fillAmount = isActive ? 1f : 0f;
                if (isActive)
                {
                    // Color bars based on current state
                    if (currentHealth >= maxHealth)
                        healthBars[i].color = healthyBarColor;
                    else if (currentHealth == 2)
                        healthBars[i].color = injuredBarColor;
                    else if (currentHealth == 1)
                        healthBars[i].color = criticalBarColor;
                }
            }
        }

        // Update health text
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }

    private void UpdateStatusUI()
    {
        // Update status text and toggle death UI when appropriate
        if (statusText != null)
        {
            if (currentHealth <= 0)
            {
                statusText.text = "Dead";
                statusText.color = criticalBarColor;
            }
            else if (currentHealth == 1)
            {
                statusText.text = "Critically Injured";
                statusText.color = criticalBarColor;
            }
            else if (currentHealth == 2)
            {
                statusText.text = "Injured";
                statusText.color = injuredBarColor;
            }
            else
            {
                statusText.text = "Healthy";
                statusText.color = healthyBarColor;
            }
        }

        // Do not auto-toggle death UI here; it's handled by ShowDeathScreen on death
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

    // Enemy contact handling
    private void OnTriggerEnter(Collider other)
    {
        TryApplyEnemyContactDamage(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryApplyEnemyContactDamage(collision.gameObject);
    }

    private void TryApplyEnemyContactDamage(GameObject other)
    {
        if (other == null || currentHealth <= 0) return;

        if (!isInvulnerable)
        {
            if (!string.IsNullOrEmpty(jenglotTag) && other.CompareTag(jenglotTag))
            {
                TakeDamage(Mathf.Max(1, jenglotDamage));
                return;
            }
            if (!string.IsNullOrEmpty(kamatayanTag) && other.CompareTag(kamatayanTag))
            {
                TakeDamage(Mathf.Max(1, kamatayanDamage));
                return;
            }
            if (!string.IsNullOrEmpty(dullahanTag) && other.CompareTag(dullahanTag))
            {
                TakeDamage(Mathf.Max(1, dullahanDamage));
                return;
            }
        }
    }

    // Death screen helpers
    private void ShowDeathScreen()
    {
        if (pauseOnDeath)
            Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deathMessageUI != null)
            deathMessageUI.SetActive(true);
        if (deathMessageText != null)
            deathMessageText.text = "You are dead";
    }

    public void RestartLevel()
    {
        if (pauseOnDeath)
            Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void ReturnToMainMenu()
    {
        if (pauseOnDeath)
            Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Main menu scene name is not set on PlayerHealthSystem.");
        }
    }
}
