using UnityEngine;
using System.Collections;

namespace DisManibus.World.Lighting
{
    /// <summary>
    /// Makes a light randomly turn off for 1-5 seconds to create a scary atmosphere.
    /// Each light operates independently with random timing.
    /// </summary>
    public class BlinkLight : MonoBehaviour
    {
        [Header("Blink Settings")]
        [Tooltip("Minimum duration the light stays off (in seconds)")]
        [Range(0.5f, 5f)]
        public float minOffDuration = 1f;

        [Tooltip("Maximum duration the light stays off (in seconds)")]
        [Range(1f, 10f)]
        public float maxOffDuration = 5f;

        [Tooltip("Minimum time between blinks (in seconds)")]
        [Range(0.5f, 10f)]
        public float minTimeBetweenBlinks = 0.5f;

        [Tooltip("Maximum time between blinks (in seconds)")]
        [Range(1f, 15f)]
        public float maxTimeBetweenBlinks = 3f;

        [Header("References")]
        [Tooltip("The light to blink. If not set, will auto-find on this GameObject")]
        public Light targetLight;

        private float originalIntensity;
        private bool isBlinking = false;

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

            // Start the blinking coroutine with a random initial delay
            float initialDelay = Random.Range(0f, maxTimeBetweenBlinks);
            StartCoroutine(BlinkRoutine(initialDelay));
        }

        private IEnumerator BlinkRoutine(float initialDelay)
        {
            // Wait for initial random delay
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                // Turn off the light
                isBlinking = true;
                targetLight.intensity = 0f;

                // Stay off for random duration (1-5 seconds)
                float offDuration = Random.Range(minOffDuration, maxOffDuration);
                yield return new WaitForSeconds(offDuration);

                // Turn the light back on
                targetLight.intensity = originalIntensity;
                isBlinking = false;

                // Wait before next blink
                float waitTime = Random.Range(minTimeBetweenBlinks, maxTimeBetweenBlinks);
                yield return new WaitForSeconds(waitTime);
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
