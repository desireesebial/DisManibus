using UnityEngine;
using System.Collections;

namespace DisManibus.World.Lighting
{
    /// <summary>
    /// Makes a light randomly turn off for brief periods to create a scary atmosphere.
    /// Each light operates independently with random timing.
    /// Only blinks when player is in range (unless excluded).
    /// </summary>
    public class BlinkLight : MonoBehaviour
    {
        [Header("Blink Settings")]
        [Tooltip("Minimum duration the light stays off (in seconds)")]
        [Range(0.01f, 5f)]
        public float minOffDuration = 0.1f;

        [Tooltip("Maximum duration the light stays off (in seconds)")]
        [Range(0.1f, 10f)]
        public float maxOffDuration = 1f;

        [Tooltip("Minimum time between blinks (in seconds)")]
        [Range(0.01f, 10f)]
        public float minTimeBetweenBlinks = 0.1f;

        [Tooltip("Maximum time between blinks (in seconds)")]
        [Range(0.1f, 15f)]
        public float maxTimeBetweenBlinks = 3f;

        [Header("Player Detection")]
        [Tooltip("Enable proximity-based blinking (only blink when player is nearby)")]
        public bool enablePlayerDetection = true;

        [Tooltip("Distance at which light will blink when player is nearby")]
        [Range(5f, 50f)]
        public float playerDetectionRange = 27.5f;

        [Tooltip("Check this to exclude this light from blinking (for puzzle/chase lights)")]
        public bool excludeFromBlinking = false;

        [Header("References")]
        [Tooltip("The light to blink. If not set, will auto-find on this GameObject")]
        public Light targetLight;

        private float originalIntensity;
        private bool isBlinking = false;
        private Transform player;
        private bool isPlayerInRange = false;

        private void Start()
        {
            // Auto-find light if not assigned
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
            }

            if (targetLight == null)
            {
                Debug.LogError($"BlinkLight on {gameObject.name}: No Light component found!", this);
                enabled = false;
                return;
            }

            // Store original intensity
            originalIntensity = targetLight.intensity;

            // Find player if proximity detection is enabled
            if (enablePlayerDetection && !excludeFromBlinking)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                else
                {
                    Debug.LogWarning($"BlinkLight on {gameObject.name}: Player not found! Proximity detection will not work.", this);
                }
            }

            // If excluded from blinking, keep light on permanently
            if (excludeFromBlinking)
            {
                targetLight.intensity = originalIntensity;
                return; // Don't start blinking routine
            }

            // Start the blinking coroutine with a random initial delay
            float initialDelay = Random.Range(0f, maxTimeBetweenBlinks);
            StartCoroutine(BlinkRoutine(initialDelay));
        }

        /// <summary>
        /// Check if player is within detection range
        /// </summary>
        private bool CheckPlayerProximity()
        {
            if (!enablePlayerDetection || player == null)
            {
                return true; // Always allow blinking if detection is disabled or no player
            }

            float distance = Vector3.Distance(transform.position, player.position);
            return distance <= playerDetectionRange;
        }

        private IEnumerator BlinkRoutine(float initialDelay)
        {
            // Wait for initial random delay
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                // Update player proximity status
                isPlayerInRange = CheckPlayerProximity();

                // Only blink if player is in range (or detection is disabled)
                if (isPlayerInRange)
                {
                    // Turn off the light
                    isBlinking = true;
                    targetLight.intensity = 0f;

                    // Stay off for random duration
                    float offDuration = Random.Range(minOffDuration, maxOffDuration);
                    yield return new WaitForSeconds(offDuration);

                    // Turn the light back on
                    targetLight.intensity = originalIntensity;
                    isBlinking = false;

                    // Wait before next blink
                    float waitTime = Random.Range(minTimeBetweenBlinks, maxTimeBetweenBlinks);
                    yield return new WaitForSeconds(waitTime);
                }
                else
                {
                    // Player out of range - keep light fully on
                    targetLight.intensity = originalIntensity;
                    isBlinking = false;

                    // Check again after a short delay
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        private void OnDisable()
        {
            // Restore light when disabled
            if (targetLight != null && isBlinking)
            {
                targetLight.intensity = originalIntensity;
            }
        }

        private void OnValidate()
        {
            // Ensure min is not greater than max
            if (minOffDuration > maxOffDuration)
            {
                maxOffDuration = minOffDuration;
            }

            if (minTimeBetweenBlinks > maxTimeBetweenBlinks)
            {
                maxTimeBetweenBlinks = minTimeBetweenBlinks;
            }
        }
    }
}
