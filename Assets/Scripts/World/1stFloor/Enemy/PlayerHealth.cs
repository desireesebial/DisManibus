using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Simple player health system for Kuchisake-onna encounters
/// Attach this to your player GameObject
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Death Settings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool reloadSceneOnDeath = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Disable player controls
        var playerController = GetComponent<MonoBehaviour>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Start respawn sequence
        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (reloadSceneOnDeath)
        {
            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Respawn at checkpoint
            Respawn();
        }
    }

    void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        // Move to respawn point
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        // Re-enable player controls
        var playerController = GetComponent<MonoBehaviour>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Reset enemy
        var enemy = FindObjectOfType<KuchisakeOnnaController>();
        if (enemy != null)
        {
            enemy.ResetEnemy();
        }

        // Reset UI
        var questionUI = FindObjectOfType<KuchisakeQuestionUI>();
        if (questionUI != null)
        {
            questionUI.ResetUI();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}

