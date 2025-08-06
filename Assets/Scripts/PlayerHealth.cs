using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives System")]
    public int maxLives = 3;
    public int currentLives;
    
    [Header("UI")]
    public Text livesText;
    public Text attemptsText;
    
    void Start()
    {
        currentLives = maxLives;
        UpdateUI();
    }
    
    public void TakeDamage(float damage)
    {
        // Any damage = lose a life immediately
        LoseLife();
    }
    
    void LoseLife()
    {
        currentLives--;
        Debug.Log("Player hit! Lives remaining: " + currentLives);
        
        if (currentLives <= 0)
        {
            // No more lives - game over
            Die();
        }
        else
        {
            // Restart scene for next attempt
            RestartScene();
        }
    }
    
    void RestartScene()
    {
        Debug.Log("Restarting scene... Lives left: " + currentLives);
        
        // Restart the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    void UpdateUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + currentLives + "/" + maxLives;
        
        if (attemptsText != null)
            attemptsText.text = "Attempts: " + (maxLives - currentLives + 1) + "/" + maxLives;
    }
    
    void Die()
    {
        Debug.Log("Game Over! All 3 attempts used!");
        // Disable player movement
        GetComponent<SimplePlayerMovement>().enabled = false;
        // Load menu scene after 2 seconds
        Invoke("GameOver", 2f);
    }
    
    void GameOver()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public bool IsDead()
    {
        return currentLives <= 0;
    }
} 