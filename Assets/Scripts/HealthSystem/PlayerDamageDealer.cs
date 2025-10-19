using UnityEngine;

/// <summary>
/// Simple damage dealer component that can be attached to player weapons, projectiles, or any object that should deal damage to enemies.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerDamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private float destroyDelay = 0.1f;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSound;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    private void Start()
    {
        // Get audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        DealDamageToEnemy(other.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        DealDamageToEnemy(collision.gameObject);
    }
    
    private void DealDamageToEnemy(GameObject target)
    {
        // Check if target is an enemy
        if (target.CompareTag("Enemy") || target.CompareTag("Jenglot") || 
            target.CompareTag("Kamatayan") || target.CompareTag("Dullahan"))
        {
            // Try to get EnemyDamageController first
            EnemyDamageController enemyController = target.GetComponent<EnemyDamageController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(damageAmount);
            }
            else
            {
                // Try to get EnemyHealthComponent
                EnemyHealthComponent enemyHealth = target.GetComponent<EnemyHealthComponent>();
                if (enemyHealth != null)
                {
                    enemyHealth.ApplyDamage(damageAmount);
                }
            }
            
            // Play hit sound
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            
            // Show hit effect
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Destroy this object if configured to do so
            if (destroyOnHit)
            {
                Destroy(gameObject, destroyDelay);
            }
            
            Debug.Log($"Player dealt {damageAmount} damage to {target.name}");
        }
    }
    
    // Public methods to modify damage
    public void SetDamage(int newDamage)
    {
        damageAmount = newDamage;
    }
    
    public int GetDamage()
    {
        return damageAmount;
    }
    
    // Debug method
    [ContextMenu("Test Damage")]
    private void TestDamage()
    {
        Debug.Log($"PlayerDamageDealer on {gameObject.name} deals {damageAmount} damage");
    }
}
