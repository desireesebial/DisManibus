using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Health UI")]
    public Slider healthBar;
    public Text healthText;
    public Image damageFlash;
    
    [Header("Stamina UI")]
    public Slider staminaBar;
    public Text staminaText;
    
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button restartButton;
    public Button mainMenuButton;
    
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    
    void Start()
    {
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerMovement = player.GetComponent<PlayerMovement>();
            
            // Setup health UI
            if (playerHealth != null)
            {
                playerHealth.healthBar = healthBar;
                playerHealth.damageFlash = damageFlash;
            }
            
            // Setup stamina UI
            if (playerMovement != null)
            {
                playerMovement.staminaBar = staminaBar;
            }
        }
        
        // Setup game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    
    void Update()
    {
        UpdateHealthUI();
        UpdateStaminaUI();
        CheckGameOver();
    }
    
    void UpdateHealthUI()
    {
        if (playerHealth != null && healthText != null)
        {
            healthText.text = $"Health: {Mathf.Round(playerHealth.currentHealth)}/{playerHealth.maxHealth}";
        }
    }
    
    void UpdateStaminaUI()
    {
        if (playerMovement != null && staminaText != null)
        {
            staminaText.text = $"Stamina: {Mathf.Round(playerMovement.GetSprintTimeRemaining())}/{playerMovement.maxSprintTime}";
        }
    }
    
    void CheckGameOver()
    {
        if (playerHealth != null && playerHealth.IsDead())
        {
            ShowGameOver();
        }
    }
    
    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER\nYou have been defeated!";
            }
        }
    }
    
    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
    
    // Public method to show damage indicator
    public void ShowDamageIndicator()
    {
        if (damageFlash != null)
        {
            damageFlash.color = Color.red;
            Invoke("ClearDamageFlash", 0.2f);
        }
    }
    
    void ClearDamageFlash()
    {
        if (damageFlash != null)
        {
            damageFlash.color = Color.clear;
        }
    }
} 