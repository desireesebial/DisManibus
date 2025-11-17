using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerHealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private int currentHealth = 3;

    [Header("Health UI")]
    public GameObject healthUI;
    public Image[] healthBars = new Image[3];
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI statusText;

    [Header("Death Screen UI")]
    public GameObject deathMessageUI;
    public RawImage deathBackground; // Death screen background as RawImage
    public Image deathMessageImage; // Death message as image (instead of text)
    public TextMeshProUGUI deathMessageText; // Alternative text-based death message
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

    [Header("Directional Damage Indicator")]
    [Tooltip("Reference to the directional damage indicator component")]
    public HealthSystem.DirectionalDamageIndicator damageIndicator;

    [Header("Player Controller")]
    public FirstPersonController playerController;

    [Header("Gameplay UI to Hide on Death")]
    [Tooltip("Stamina/Sprint bar UI")]
    public GameObject staminaUI;

    [Tooltip("Quest objectives UI")]
    public GameObject questUI;

    [Tooltip("Item tracker/clues UI")]
    public GameObject itemTrackerUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip healSound;
    public AudioClip criticalHealthSound;

    [Header("Groaning Audio")]
    [Tooltip("Array of groaning sounds played when player takes damage (drag and drop audio clips)")]
    public AudioClip[] groaningSounds;

    [Header("Knockback Settings")]
    [Tooltip("Force applied when player is knocked back by damage")]
    public float knockbackForce = 5f;

    [Tooltip("Maximum distance player will be pushed back (for wall collision prevention)")]
    public float knockbackDistance = 2f;

    [Tooltip("Upward force component added to knockback for more dynamic feel")]
    public float knockbackUpwardForce = 2f;

    [Tooltip("Layer mask for detecting walls during knockback (prevents pushing through walls)")]
    public LayerMask wallLayerMask;

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

    // Events (Unity Events for better integration)
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnCriticalHealth;
    public UnityEvent OnPlayerDeath;

    // Properties (inspired by Ilumisoft pattern)
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public bool IsAlive => currentHealth > 0;

    // Private variables
    private bool isInvulnerable = false;
    private Vector3 originalCameraPosition;
    private bool isBlurActive = false;
    private Coroutine blurCoroutine;
    private Coroutine damageFlashCoroutine;
    private float originalWalkSpeed;
    private float originalMouseSensitivity;
    private bool hasDied = false;
    private Rigidbody playerRigidbody; // For knockback force application

    void Start()
    {
        InitializeHealthSystem();
    }

    void Update()
    {
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

        // Get Rigidbody for knockback system
        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody>();

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
        if (deathBackground != null)
            deathBackground.gameObject.SetActive(false);
        if (deathMessageImage != null)
            deathMessageImage.gameObject.SetActive(false);
        if (deathMessageText != null)
            deathMessageText.gameObject.SetActive(false);
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);
        
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

    public void ApplyDamage(int damage)
    {
        if (isInvulnerable || !IsAlive)
        {
            Debug.Log($"Damage blocked: invulnerable={isInvulnerable}, isAlive={IsAlive}");
            return;
        }

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        int healthChange = currentHealth - previousHealth;

        Debug.Log($"ApplyDamage called: damage={damage}, previousHealth={previousHealth}, currentHealth={currentHealth}, healthChange={healthChange}");

        if (Mathf.Abs(healthChange) > 0)
        {
            OnHealthChanged?.Invoke(currentHealth);

            // Visual and audio feedback - ALWAYS play when damage is applied
            StartCoroutine(CameraShake());
            StartDamageFlash();
            PlayDamageSound(); // Groaning audio plays here
            Debug.Log("Playing damage sound and visual effects");

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
        else
        {
            Debug.LogWarning($"Health did not change! This should not happen. damage={damage}, health={currentHealth}");
        }
    }

    /// <summary>
    /// Applies damage to the player and shows directional damage indicator
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    /// <param name="enemyPosition">World position of the enemy that dealt damage</param>
    public void ApplyDamage(int damage, Vector3 enemyPosition)
    {
        // Apply the damage using the standard method
        ApplyDamage(damage);

        // Show directional damage indicator if available
        if (damageIndicator != null && IsAlive)
        {
            damageIndicator.ShowDamageIndicator(enemyPosition, damage);
        }
    }

    // Keep the old method for backward compatibility
    public void TakeDamage(int damage)
    {
        ApplyDamage(damage);
    }

    public void AddHealth(int healAmount)
    {
        if (!IsAlive || currentHealth >= maxHealth) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        int healthChange = currentHealth - previousHealth;
        
        if (healthChange > 0)
        {
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
    }

    // Keep the old method for backward compatibility
    public void Heal(int healAmount)
    {
        AddHealth(healAmount);
    }

    public void SetHealth(int health)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        
        int healthChange = currentHealth - previousHealth;
        
        if (Mathf.Abs(healthChange) > 0)
        {
            OnHealthChanged?.Invoke(currentHealth);
            
            // Apply or remove debuffs based on new health
            RemoveHealthDebuffs();
            ApplyHealthDebuffs();
            
            // Handle critical health effects
            if (currentHealth == 1 && !isBlurActive)
            {
                OnCriticalHealth?.Invoke();
                StartCriticalHealthBlur();
            }
            else if (currentHealth > 1 && isBlurActive)
            {
                StopCriticalHealthBlur();
            }
            
            // Handle death
            if (currentHealth <= 0 && IsAlive == false)
            {
                OnPlayerDeath?.Invoke();
                HandlePlayerDeath();
            }
            
            // Update UI
            UpdateHealthUI();
            UpdateStatusUI();
        }
    }

    public void RestoreFullHealth()
    {
        SetHealth(maxHealth);
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
        if (audioSource == null) return;

        // Prioritize groaning sounds if available for variation
        if (groaningSounds != null && groaningSounds.Length > 0)
        {
            // Filter out null clips
            AudioClip[] validClips = System.Array.FindAll(groaningSounds, clip => clip != null);

            if (validClips.Length > 0)
            {
                // Select random groaning sound
                int randomIndex = Random.Range(0, validClips.Length);
                audioSource.PlayOneShot(validClips[randomIndex]);
                return;
            }
        }

        // Fall back to single damage sound if no groaning sounds available
        if (damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }

    private void ApplyKnockback(Vector3 enemyPosition)
    {
        // Safety check - ensure we have a rigidbody
        if (playerRigidbody == null)
        {
            Debug.LogWarning("Cannot apply knockback: playerRigidbody is null");
            return;
        }

        // Calculate knockback direction (away from enemy)
        Vector3 playerPosition = transform.position;
        Vector3 knockbackDirection = (playerPosition - enemyPosition).normalized;

        // Flatten the direction to horizontal plane (optional - prevents vertical knockback from flying enemies)
        knockbackDirection.y = 0;
        knockbackDirection.Normalize();

        // Raycast to check for walls in the knockback direction
        float safeKnockbackDistance = knockbackDistance;
        RaycastHit hit;

        // Cast from player position in knockback direction
        if (Physics.Raycast(playerPosition, knockbackDirection, out hit, knockbackDistance, wallLayerMask))
        {
            // Wall detected - reduce knockback distance to prevent going through walls
            // Use half the distance to the wall to be safe
            safeKnockbackDistance = hit.distance * 0.5f;
            Debug.Log($"Wall detected at {hit.distance}m. Reducing knockback to {safeKnockbackDistance}m");

            // If wall is too close (less than 0.5m), don't apply horizontal knockback
            if (safeKnockbackDistance < 0.5f)
            {
                Debug.Log("Wall too close - skipping horizontal knockback");
                knockbackDirection = Vector3.zero;
            }
        }

        // Apply knockback force
        if (knockbackDirection != Vector3.zero)
        {
            // Horizontal knockback
            Vector3 knockbackForceVector = knockbackDirection * knockbackForce;

            // Add upward component for more dynamic feel
            knockbackForceVector.y = knockbackUpwardForce;

            // Apply impulse force for instant impact
            playerRigidbody.AddForce(knockbackForceVector, ForceMode.Impulse);

            Debug.Log($"Applied knockback: direction={knockbackDirection}, force={knockbackForce}, distance={safeKnockbackDistance}");
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
        // Update health bars (optional - only if array is assigned)
        if (healthBars != null && healthBars.Length > 0)
        {
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
        TryApplyEnemyContactDamage(other.gameObject, other.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryApplyEnemyContactDamage(collision.gameObject, collision.transform.position);
    }

    private void TryApplyEnemyContactDamage(GameObject other, Vector3 enemyPosition)
    {
        Debug.Log($"TryApplyEnemyContactDamage called: enemy={other?.name}, tag={other?.tag}, isInvulnerable={isInvulnerable}, currentHealth={currentHealth}");

        if (other == null || currentHealth <= 0)
        {
            Debug.Log($"Early return: other is null={other == null}, health <= 0={currentHealth <= 0}");
            return;
        }

        if (!isInvulnerable)
        {
            bool damageApplied = false;
            int damageAmount = 0;

            if (!string.IsNullOrEmpty(jenglotTag) && other.CompareTag(jenglotTag))
            {
                damageAmount = Mathf.Max(1, jenglotDamage);
                Debug.Log($"Jenglot hit detected! Applying {damageAmount} damage");
                ApplyDamage(damageAmount, enemyPosition);
                damageApplied = true;
            }
            else if (!string.IsNullOrEmpty(kamatayanTag) && other.CompareTag(kamatayanTag))
            {
                damageAmount = Mathf.Max(1, kamatayanDamage);
                Debug.Log($"Kamatayan hit detected! Applying {damageAmount} damage");
                ApplyDamage(damageAmount, enemyPosition);
                damageApplied = true;
            }
            else if (!string.IsNullOrEmpty(dullahanTag) && other.CompareTag(dullahanTag))
            {
                damageAmount = Mathf.Max(1, dullahanDamage);
                Debug.Log($"Dullahan hit detected! Applying {damageAmount} damage");
                ApplyDamage(damageAmount, enemyPosition);
                damageApplied = true;
            }
            else
            {
                Debug.Log($"No matching enemy tag found. Object tag: '{other.tag}', Expected tags: '{jenglotTag}', '{kamatayanTag}', '{dullahanTag}'");
            }

            // Apply knockback if damage was applied
            if (damageApplied)
            {
                Debug.Log($"Damage applied, now applying knockback from position {enemyPosition}");
                ApplyKnockback(enemyPosition);
            }
        }
        else
        {
            Debug.Log("Player is invulnerable, damage blocked");
        }
    }

    // Death screen helpers
    private void ShowDeathScreen()
    {
        ShowDeathScreen(null);
    }

    /// <summary>
    /// Shows the death screen with optional custom message
    /// </summary>
    /// <param name="customMessage">Custom death message (null = use default)</param>
    public void ShowDeathScreen(string customMessage)
    {
        if (pauseOnDeath)
            Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide all gameplay UI elements
        if (healthUI != null)
        {
            healthUI.SetActive(false);
            Debug.Log("[PlayerHealthSystem] Hiding healthUI on death");
        }
        if (staminaUI != null)
        {
            staminaUI.SetActive(false);
            Debug.Log("[PlayerHealthSystem] Hiding staminaUI on death");
        }
        if (questUI != null)
        {
            questUI.SetActive(false);
            Debug.Log("[PlayerHealthSystem] Hiding questUI on death");
        }
        if (itemTrackerUI != null)
        {
            itemTrackerUI.SetActive(false);
            Debug.Log("[PlayerHealthSystem] Hiding itemTrackerUI on death");
        }

        // Show death screen UI
        if (deathMessageUI != null)
            deathMessageUI.SetActive(true);

        // Show death background RawImage
        if (deathBackground != null)
            deathBackground.gameObject.SetActive(true);

        // Show death message (either image or text)
        if (deathMessageImage != null)
        {
            deathMessageImage.gameObject.SetActive(true);
            // Death message image is already set in inspector
            // Note: If using image-based death screen, custom messages won't show
        }
        else if (deathMessageText != null)
        {
            deathMessageText.gameObject.SetActive(true);
            // Use custom message if provided, otherwise use default
            deathMessageText.text = !string.IsNullOrEmpty(customMessage) ? customMessage : "You are dead";
        }

        // Show buttons
        if (retryButton != null)
            retryButton.gameObject.SetActive(true);
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);
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

    public void ApplySavedHealth(int savedCurrentHealth, int savedMaxHealth)
    {
        maxHealth = Mathf.Max(1, savedMaxHealth);
        currentHealth = Mathf.Clamp(savedCurrentHealth, 0, maxHealth);

        // Reset debuffs, effects, and controller states
        RemoveHealthDebuffs();
        ApplyHealthDebuffs();

        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
            HandlePlayerDeath();
        }
        else
        {
            hasDied = false;
            StopCriticalHealthBlur();
            if (pauseOnDeath)
                Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (deathMessageUI != null)
                deathMessageUI.SetActive(false);
            if (deathBackground != null)
                deathBackground.gameObject.SetActive(false);
            if (deathMessageImage != null)
                deathMessageImage.gameObject.SetActive(false);
            if (deathMessageText != null)
                deathMessageText.gameObject.SetActive(false);
            if (retryButton != null)
                retryButton.gameObject.SetActive(false);
            if (mainMenuButton != null)
                mainMenuButton.gameObject.SetActive(false);
            if (playerController != null && !playerController.enabled)
                playerController.enabled = true;

            // Restore gameplay UI elements
            if (healthUI != null)
            {
                healthUI.SetActive(true);
                Debug.Log("[PlayerHealthSystem] Restoring healthUI");
            }
            if (staminaUI != null)
            {
                staminaUI.SetActive(true);
                Debug.Log("[PlayerHealthSystem] Restoring staminaUI");
            }
            if (questUI != null)
            {
                questUI.SetActive(true);
                Debug.Log("[PlayerHealthSystem] Restoring questUI");
            }
            if (itemTrackerUI != null)
            {
                itemTrackerUI.SetActive(true);
                Debug.Log("[PlayerHealthSystem] Restoring itemTrackerUI");
            }
        }

        // Update UI and status
        UpdateHealthUI();
        UpdateStatusUI();

        if (currentHealth == 1)
        {
            StartCriticalHealthBlur();
        }
    }
}
