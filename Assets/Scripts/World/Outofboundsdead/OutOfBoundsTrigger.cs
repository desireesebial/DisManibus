using UnityEngine;

namespace DisManibus.World
{
    /// <summary>
    /// Triggers player death when they fall out of bounds.
    /// Attach to a trigger collider positioned below the playable area.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class OutOfBoundsTrigger : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [SerializeField, Tooltip("Optional delay before triggering death (in seconds)")]
        private float deathDelay = 0f;

        [Header("Gizmo Settings")]
        [SerializeField, Tooltip("Color of the trigger zone in the editor")]
        private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

        private void Awake()
        {
            // Ensure this object has a trigger collider
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the player entered the trigger
            if (other.CompareTag("Player"))
            {
                TriggerPlayerDeath();
            }
        }

        private void TriggerPlayerDeath()
        {
            // Find the PlayerHealthSystem
            PlayerHealthSystem playerHealth = FindObjectOfType<PlayerHealthSystem>();

            if (playerHealth != null)
            {
                if (deathDelay > 0f)
                {
                    Invoke(nameof(ExecuteDeath), deathDelay);
                }
                else
                {
                    ExecuteDeath();
                }
            }
            else
            {
                Debug.LogError("OutOfBoundsTrigger: PlayerHealthSystem not found in scene!");
            }
        }

        private void ExecuteDeath()
        {
            PlayerHealthSystem playerHealth = FindObjectOfType<PlayerHealthSystem>();
            if (playerHealth != null)
            {
                // Apply fatal damage to trigger death through the health system
                playerHealth.ApplyDamage(999);
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize the trigger zone in the editor
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = gizmoColor;
                Gizmos.matrix = transform.localToWorldMatrix;

                if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}
