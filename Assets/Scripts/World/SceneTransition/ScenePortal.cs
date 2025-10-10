using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using World.UI;

namespace World.SceneTransition
{
    [RequireComponent(typeof(Collider))]
    public class ScenePortal : MonoBehaviour
    {
        [Header("Destination Scene")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private int targetSceneBuildIndex = -1;

        [Header("Interaction")]
        [SerializeField] private bool useTrigger = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool saveOnTransition = true;
        [SerializeField] private bool requireHoldToActivate = true;
        [SerializeField, Min(0f)] private float holdDuration = 1.5f;

        [Header("Interaction UI")]
        [SerializeField] private GameObject interactionUIPanel;
        [SerializeField] private TMP_Text interactionLabel;
        [SerializeField] private Image holdProgressImage;
        [SerializeField] private Slider holdProgressSlider;
        [SerializeField] private string tapPromptFormat = "Press {0} to enter";
        [SerializeField] private string holdPromptFormat = "Hold {0} to enter";
        [SerializeField] private bool showHoldDurationInPrompt = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onPortalActivated;
        [SerializeField] private UnityEvent onPortalFailed;

        [Header("Loading Screen")]
        [SerializeField] private string loadingMessage = "Loading...";

        private bool playerInRange;
        private GameObject playerReference;
        private float holdTimer;
        private bool holdActivationTriggered;
        private bool isActivating;

        private void Start()
        {
            ResetHoldState();
            HideInteractionUI();
            UpdatePromptText();
        }

        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void Awake()
        {
            if (string.IsNullOrEmpty(targetSceneName) && targetSceneBuildIndex < 0)
            {
                Debug.LogWarning($"ScenePortal '{name}' has no target scene defined.");
            }

            if (useTrigger)
            {
                Collider collider = GetComponent<Collider>();
                if (!collider.isTrigger)
                {
                    Debug.LogWarning($"ScenePortal '{name}' collider should be set to trigger. Auto-enabling.");
                    collider.isTrigger = true;
                }
            }

            UpdatePromptText();
        }

        private void Update()
        {
            if (!useTrigger && playerReference != null)
            {
                HandleInteractionInput();
            }
            else if (useTrigger && playerInRange)
            {
                HandleInteractionInput();
            }
        }

        private void HandleInteractionInput()
        {
            // Don't accept input if already activating
            if (isActivating)
            {
                return;
            }

            if (!requireHoldToActivate)
            {
                UpdateHoldProgressUI(0f);
                if (Input.GetKeyDown(interactKey))
                {
                    ActivatePortal();
                }
                return;
            }

            // Handle hold-to-activate
            if (Input.GetKey(interactKey))
            {
                holdTimer += Time.deltaTime;
                
                // Calculate progress - ensure it matches the actual hold time
                float progress = Mathf.Clamp01(holdTimer / holdDuration);
                UpdateHoldProgressUI(progress);

                // Check if hold duration is complete
                if (!holdActivationTriggered && holdTimer >= holdDuration)
                {
                    holdActivationTriggered = true;
                    UpdateHoldProgressUI(1f); // Ensure slider shows complete
                    ActivatePortal();
                }
            }
            else
            {
                // Reset if player releases the key before completing
                if (holdTimer > 0f)
                {
                    ResetHoldState();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!useTrigger) return;

            if (other.CompareTag(playerTag))
            {
                playerInRange = true;
                playerReference = other.gameObject;
                ShowInteractionUI();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!useTrigger) return;

            if (other.CompareTag(playerTag))
            {
                playerInRange = false;
                if (playerReference == other.gameObject)
                {
                    playerReference = null;
                    ResetHoldState();
                }
                HideInteractionUI();
            }
        }

        public void ActivatePortal()
        {
            // Prevent multiple activations
            if (isActivating)
            {
                return;
            }

            if (string.IsNullOrEmpty(targetSceneName) && targetSceneBuildIndex < 0)
            {
                Debug.LogError($"ScenePortal '{name}' cannot activate without a valid scene target.");
                onPortalFailed?.Invoke();
                return;
            }

            // Mark as activating to prevent further input
            isActivating = true;
            Debug.Log($"ScenePortal '{name}' activating - transitioning to scene");

            if (saveOnTransition)
            {
                var saveManager = FindAnyObjectByType<SaveLoad.SaveManager>();
                if (saveManager != null)
                {
                    saveManager.SaveGame();
                }
                else
                {
                    Debug.LogWarning("ScenePortal could not find SaveManager to save game before transitioning");
                }
            }

            onPortalActivated?.Invoke();

            if (SceneTransitionManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    SceneTransitionManager.Instance.LoadScene(targetSceneName, loadingMessage);
                }
                else
                {
                    SceneTransitionManager.Instance.LoadScene(targetSceneBuildIndex, loadingMessage);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    StartCoroutine(LoadSceneWithFallback(targetSceneName));
                }
                else
                {
                    StartCoroutine(LoadSceneWithFallback(targetSceneBuildIndex));
                }
            }
        }

        // Public API for buttons or other triggers
        public void SetPlayerReference(GameObject player)
        {
            playerReference = player;
            if (!useTrigger)
            {
                if (playerReference != null)
                {
                    ShowInteractionUI();
                }
                else
                {
                    HideInteractionUI();
                }
            }

            if (player == null)
            {
                ResetHoldState();
            }
        }

        public void SetTargetScene(string sceneName)
        {
            targetSceneName = sceneName;
        }

        public void SetTargetScene(int buildIndex)
        {
            targetSceneBuildIndex = buildIndex;
        }

        private System.Collections.IEnumerator LoadSceneWithFallback(string sceneName)
        {
            float startTime = Time.realtimeSinceStartup;
            var operation = SceneManager.LoadSceneAsync(sceneName);

            if (LoadingScreenController.Instance != null)
            {
                yield return LoadingScreenController.Instance.TrackAsyncOperation(operation, loadingMessage);
            }
            else
            {
                Debug.LogWarning($"ScenePortal '{name}': LoadingScreenController not found - loading without loading screen");
                yield return operation;
                
                // Ensure minimum loading time of 2 seconds even without loading screen
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                float minTime = 2.0f;
                if (elapsedTime < minTime)
                {
                    yield return new WaitForSecondsRealtime(minTime - elapsedTime);
                }
            }
        }

        private System.Collections.IEnumerator LoadSceneWithFallback(int buildIndex)
        {
            float startTime = Time.realtimeSinceStartup;
            var operation = SceneManager.LoadSceneAsync(buildIndex);

            if (LoadingScreenController.Instance != null)
            {
                yield return LoadingScreenController.Instance.TrackAsyncOperation(operation, loadingMessage);
            }
            else
            {
                Debug.LogWarning($"ScenePortal '{name}': LoadingScreenController not found - loading without loading screen");
                yield return operation;
                
                // Ensure minimum loading time of 2 seconds even without loading screen
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                float minTime = 2.0f;
                if (elapsedTime < minTime)
                {
                    yield return new WaitForSecondsRealtime(minTime - elapsedTime);
                }
            }
        }

        private void ResetHoldState()
        {
            holdTimer = 0f;
            holdActivationTriggered = false;
            UpdateHoldProgressUI(0f);
        }

        private void ShowInteractionUI()
        {
            if (interactionUIPanel != null && !interactionUIPanel.activeSelf)
            {
                interactionUIPanel.SetActive(true);
            }

            UpdatePromptText();
            UpdateHoldProgressUI(0f);
        }

        private void HideInteractionUI()
        {
            if (interactionUIPanel != null && interactionUIPanel.activeSelf)
            {
                interactionUIPanel.SetActive(false);
            }

            // Explicitly hide progress UI elements
            if (holdProgressImage != null && holdProgressImage.gameObject.activeSelf)
            {
                holdProgressImage.gameObject.SetActive(false);
            }
            if (holdProgressSlider != null && holdProgressSlider.gameObject.activeSelf)
            {
                holdProgressSlider.gameObject.SetActive(false);
            }
        }

        private void UpdateHoldProgressUI(float progress)
        {
            if (!requireHoldToActivate || progress <= 0f)
            {
                // Hide progress UI when not using hold-to-activate or progress is 0
                if (holdProgressImage != null && holdProgressImage.gameObject.activeSelf)
                {
                    holdProgressImage.gameObject.SetActive(false);
                }
                if (holdProgressSlider != null && holdProgressSlider.gameObject.activeSelf)
                {
                    holdProgressSlider.gameObject.SetActive(false);
                }
                return;
            }

            // Show and update progress UI when using hold-to-activate
            float clampedProgress = Mathf.Clamp01(progress);

            if (holdProgressImage != null)
            {
                if (!holdProgressImage.gameObject.activeSelf)
                {
                    holdProgressImage.gameObject.SetActive(true);
                }
                holdProgressImage.fillAmount = clampedProgress;
            }

            if (holdProgressSlider != null)
            {
                if (!holdProgressSlider.gameObject.activeSelf)
                {
                    holdProgressSlider.gameObject.SetActive(true);
                }
                holdProgressSlider.normalizedValue = clampedProgress;
            }
        }

        private void UpdatePromptText()
        {
            if (interactionLabel == null) return;

            string keyName = interactKey.ToString();

            if (requireHoldToActivate)
            {
                string prompt = string.Format(holdPromptFormat, keyName);
                if (showHoldDurationInPrompt && holdDuration > 0f)
                {
                    prompt = $"{prompt} ({holdDuration:0.#}s)";
                }
                interactionLabel.text = prompt;
            }
            else
            {
                interactionLabel.text = string.Format(tapPromptFormat, keyName);
            }
        }

        private void OnValidate()
        {
            holdDuration = Mathf.Max(0f, holdDuration);
            UpdatePromptText();
            UpdateHoldProgressUI(0f);
        }
    }
}
