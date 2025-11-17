using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace HealthSystem
{
    public class DirectionalDamageIndicator : MonoBehaviour
    {
        [Header("Blood Splatter Images")]
        [Tooltip("Blood splatter image positioned at top edge of screen")]
        public Image topSplatter;

        [Tooltip("Blood splatter image positioned at bottom edge of screen")]
        public Image bottomSplatter;

        [Tooltip("Blood splatter image positioned at left edge of screen")]
        public Image leftSplatter;

        [Tooltip("Blood splatter image positioned at right edge of screen")]
        public Image rightSplatter;

        [Header("Indicator Settings")]
        [Tooltip("How long the indicator stays visible before fading (in seconds)")]
        [Range(1f, 5f)]
        public float fadeOutDuration = 2.5f;

        [Tooltip("Maximum alpha value for minimum damage (1 HP)")]
        [Range(0.3f, 0.8f)]
        public float minDamageAlpha = 0.5f;

        [Tooltip("Maximum alpha value for high damage (2-3 HP)")]
        [Range(0.6f, 1f)]
        public float maxDamageAlpha = 0.9f;

        [Tooltip("Scale multiplier for high damage intensity")]
        [Range(1f, 1.5f)]
        public float highDamageScale = 1.2f;

        [Header("References")]
        [Tooltip("Reference to the player's camera for direction calculation")]
        public Camera playerCamera;

        [Tooltip("Reference to the player transform")]
        public Transform playerTransform;

        // Current active fade coroutine
        private Coroutine currentFadeCoroutine;

        // Currently active splatter image
        private Image currentActiveSplatter;

        private void Awake()
        {
            // Validate references
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                {
                    Debug.LogError("DirectionalDamageIndicator: No camera found! Please assign playerCamera.");
                }
            }

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    Debug.LogError("DirectionalDamageIndicator: No player transform found!");
                }
            }

            // Initialize all splatters as invisible
            SetSplatterAlpha(topSplatter, 0f);
            SetSplatterAlpha(bottomSplatter, 0f);
            SetSplatterAlpha(leftSplatter, 0f);
            SetSplatterAlpha(rightSplatter, 0f);
        }

        /// <summary>
        /// Displays a directional damage indicator based on enemy position
        /// </summary>
        /// <param name="enemyPosition">World position of the enemy that dealt damage</param>
        /// <param name="damageAmount">Amount of damage taken (used for intensity scaling)</param>
        public void ShowDamageIndicator(Vector3 enemyPosition, int damageAmount)
        {
            if (playerTransform == null || playerCamera == null)
            {
                Debug.LogWarning("DirectionalDamageIndicator: Missing references, cannot show indicator");
                return;
            }

            // Cancel any existing fade coroutine (only show most recent damage)
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);

                // Instantly hide the previous splatter
                if (currentActiveSplatter != null)
                {
                    SetSplatterAlpha(currentActiveSplatter, 0f);
                    ResetSplatterScale(currentActiveSplatter);
                }
            }

            // Calculate direction from player to enemy
            Vector3 directionToEnemy = (enemyPosition - playerTransform.position).normalized;

            // Convert to camera space to get relative direction
            Vector3 localDirection = playerTransform.InverseTransformDirection(directionToEnemy);

            // Determine which edge to show based on direction
            Image splatterToShow = GetSplatterFromDirection(localDirection);

            if (splatterToShow == null)
            {
                Debug.LogWarning("DirectionalDamageIndicator: No splatter image assigned for this direction");
                return;
            }

            // Calculate alpha based on damage amount
            float targetAlpha = CalculateAlphaFromDamage(damageAmount);

            // Calculate scale based on damage amount
            float targetScale = damageAmount > 1 ? highDamageScale : 1f;

            // Set the active splatter
            currentActiveSplatter = splatterToShow;

            // Apply scale for high damage
            if (damageAmount > 1)
            {
                splatterToShow.transform.localScale = Vector3.one * targetScale;
            }

            // Start fade out animation
            currentFadeCoroutine = StartCoroutine(FadeOutSplatter(splatterToShow, targetAlpha, targetScale));
        }

        /// <summary>
        /// Determines which splatter image to show based on direction vector
        /// </summary>
        private Image GetSplatterFromDirection(Vector3 localDirection)
        {
            // Use the strongest component to determine primary direction
            float absX = Mathf.Abs(localDirection.x);
            float absZ = Mathf.Abs(localDirection.z);

            // Determine if horizontal or vertical direction is stronger
            if (absX > absZ)
            {
                // Left or Right
                if (localDirection.x > 0)
                {
                    return rightSplatter; // Enemy is to the right
                }
                else
                {
                    return leftSplatter; // Enemy is to the left
                }
            }
            else
            {
                // Forward or Back
                if (localDirection.z > 0)
                {
                    return topSplatter; // Enemy is in front (show indicator at top/front)
                }
                else
                {
                    return bottomSplatter; // Enemy is behind (show indicator at bottom/back)
                }
            }
        }

        /// <summary>
        /// Calculates the alpha value based on damage amount
        /// </summary>
        private float CalculateAlphaFromDamage(int damage)
        {
            if (damage <= 1)
            {
                return minDamageAlpha;
            }
            else
            {
                // Scale up for higher damage
                return maxDamageAlpha;
            }
        }

        /// <summary>
        /// Fades out the splatter over time
        /// </summary>
        private IEnumerator FadeOutSplatter(Image splatter, float startAlpha, float scale)
        {
            if (splatter == null) yield break;

            float elapsedTime = 0f;

            // Fade in quickly
            float fadeInTime = 0.1f;
            while (elapsedTime < fadeInTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, startAlpha, elapsedTime / fadeInTime);
                SetSplatterAlpha(splatter, alpha);
                yield return null;
            }

            // Ensure we hit the target alpha
            SetSplatterAlpha(splatter, startAlpha);

            // Hold at full alpha briefly
            float holdTime = 0.3f;
            yield return new WaitForSeconds(holdTime);

            // Fade out gradually
            elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
                SetSplatterAlpha(splatter, alpha);
                yield return null;
            }

            // Ensure completely invisible
            SetSplatterAlpha(splatter, 0f);

            // Reset scale
            ResetSplatterScale(splatter);

            // Clear current active reference
            if (currentActiveSplatter == splatter)
            {
                currentActiveSplatter = null;
            }

            currentFadeCoroutine = null;
        }

        /// <summary>
        /// Helper method to set alpha of a splatter image
        /// </summary>
        private void SetSplatterAlpha(Image splatter, float alpha)
        {
            if (splatter != null)
            {
                Color color = splatter.color;
                color.a = alpha;
                splatter.color = color;
            }
        }

        /// <summary>
        /// Helper method to reset scale of a splatter image
        /// </summary>
        private void ResetSplatterScale(Image splatter)
        {
            if (splatter != null)
            {
                splatter.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Public method to manually hide all indicators (useful for testing or special cases)
        /// </summary>
        public void HideAllIndicators()
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }

            SetSplatterAlpha(topSplatter, 0f);
            SetSplatterAlpha(bottomSplatter, 0f);
            SetSplatterAlpha(leftSplatter, 0f);
            SetSplatterAlpha(rightSplatter, 0f);

            ResetSplatterScale(topSplatter);
            ResetSplatterScale(bottomSplatter);
            ResetSplatterScale(leftSplatter);
            ResetSplatterScale(rightSplatter);

            currentActiveSplatter = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Test method for debugging in the editor
        /// </summary>
        [ContextMenu("Test Top Indicator")]
        private void TestTopIndicator()
        {
            if (playerTransform != null)
            {
                Vector3 testPos = playerTransform.position + playerTransform.forward * 5f;
                ShowDamageIndicator(testPos, 1);
            }
        }

        [ContextMenu("Test Bottom Indicator")]
        private void TestBottomIndicator()
        {
            if (playerTransform != null)
            {
                Vector3 testPos = playerTransform.position - playerTransform.forward * 5f;
                ShowDamageIndicator(testPos, 1);
            }
        }

        [ContextMenu("Test Left Indicator")]
        private void TestLeftIndicator()
        {
            if (playerTransform != null)
            {
                Vector3 testPos = playerTransform.position - playerTransform.right * 5f;
                ShowDamageIndicator(testPos, 1);
            }
        }

        [ContextMenu("Test Right Indicator")]
        private void TestRightIndicator()
        {
            if (playerTransform != null)
            {
                Vector3 testPos = playerTransform.position + playerTransform.right * 5f;
                ShowDamageIndicator(testPos, 1);
            }
        }

        [ContextMenu("Test High Damage")]
        private void TestHighDamage()
        {
            if (playerTransform != null)
            {
                Vector3 testPos = playerTransform.position + playerTransform.forward * 5f;
                ShowDamageIndicator(testPos, 2);
            }
        }
#endif
    }
}
