using System.Collections.Generic;
using UnityEngine;

namespace DisManibus.World.FirstFloor.Enemy
{
    /// <summary>
    /// Trigger-based safe zone detection for Kuchisake Onna enemy.
    /// When player enters this trigger, they are considered in a safe zone.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SafeZoneTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";

        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool showGizmos = true;

        // Static tracking of all active safe zones
        private static HashSet<SafeZoneTrigger> activeSafeZones = new HashSet<SafeZoneTrigger>();

        // Static property to check if player is in any safe zone
        public static bool IsPlayerInAnySafeZone => activeSafeZones.Count > 0;

        // Track if player is currently in this specific zone
        private bool playerInZone = false;

        private Collider triggerCollider;

        void Awake()
        {
            triggerCollider = GetComponent<Collider>();

            // Ensure collider is configured as trigger
            if (!triggerCollider.isTrigger)
            {
                Debug.LogWarning($"SafeZoneTrigger on {gameObject.name}: Collider was not set as trigger. Auto-configuring...", this);
                triggerCollider.isTrigger = true;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                playerInZone = true;
                activeSafeZones.Add(this);

                if (enableLogging)
                {
                    Debug.Log($"Player entered safe zone: {gameObject.name} (Total active zones: {activeSafeZones.Count})", this);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                playerInZone = false;
                activeSafeZones.Remove(this);

                if (enableLogging)
                {
                    Debug.Log($"Player exited safe zone: {gameObject.name} (Total active zones: {activeSafeZones.Count})", this);
                }
            }
        }

        void OnDisable()
        {
            // Clean up if this zone is disabled while player is inside
            if (playerInZone)
            {
                activeSafeZones.Remove(this);
                playerInZone = false;
            }
        }

        void OnDestroy()
        {
            // Clean up when destroyed
            activeSafeZones.Remove(this);
        }

        // Reset method is called when component is added or reset in editor
        void Reset()
        {
            // Auto-configure the collider as a trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            // Set color based on whether player is in this zone
            Gizmos.color = playerInZone ? new Color(0f, 1f, 0f, 0.3f) : new Color(1f, 1f, 0f, 0.2f);

            // Draw the trigger bounds
            if (col is BoxCollider boxCol)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);

                // Draw wireframe outline
                Gizmos.color = playerInZone ? Color.green : Color.yellow;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(transform.TransformPoint(sphereCol.center), sphereCol.radius * transform.lossyScale.x);

                // Draw wireframe outline
                Gizmos.color = playerInZone ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(transform.TransformPoint(sphereCol.center), sphereCol.radius * transform.lossyScale.x);
            }
            else if (col is CapsuleCollider capsuleCol)
            {
                // For capsule, draw a wire sphere at center as approximation
                Gizmos.DrawWireSphere(transform.TransformPoint(capsuleCol.center), capsuleCol.radius * transform.lossyScale.x);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            // Draw brighter version when selected
            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = playerInZone ? Color.green : Color.yellow;

            if (col is BoxCollider boxCol)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(sphereCol.center), sphereCol.radius * transform.lossyScale.x);
            }
        }

        // Debug helper to get active zone count
        public static int GetActiveZoneCount() => activeSafeZones.Count;

        // Debug helper to check if player is in this specific zone
        public bool IsPlayerInThisZone() => playerInZone;
    }
}
