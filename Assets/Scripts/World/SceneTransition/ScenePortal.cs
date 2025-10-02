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
        [SerializeField] private bool requireHoldToActivate = false;
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
        [SerializeField] private bool preferTransitionManagerLoadingScreen = true;

        private bool playerInRange;
        private GameObject playerReference;
        private float holdTimer;
        private bool holdActivationTriggered;

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
            if (!requireHoldToActivate)
            {
                UpdateHoldProgressUI(0f);
                if (Input.GetKeyDown(interactKey))
                {
                    ActivatePortal();
                }
                return;
            }

            if (Input.GetKey(interactKey))
            {
                holdTimer += Time.deltaTime;
                float requiredTime = Mathf.Max(holdDuration, 0f);
                float progress = requiredTime <= 0f ? 1f : Mathf.Clamp01(holdTimer / requiredTime);
                UpdateHoldProgressUI(progress);

                if (!holdActivationTriggered && (requiredTime <= 0f || holdTimer >= requiredTime))
                {
                    holdActivationTriggered = true;
                    ActivatePortal();
                }
            }
            else
            {
                if (holdTimer > 0f || holdActivationTriggered)
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
            if (string.IsNullOrEmpty(targetSceneName) && targetSceneBuildIndex < 0)
            {
                Debug.LogError($"ScenePortal '{name}' cannot activate without a valid scene target.");
                onPortalFailed?.Invoke();
                return;
            }

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
                    SceneTransitionManager.Instance.LoadScene(targetSceneName, loadingMessage, preferTransitionManagerLoadingScreen);
                }
                else
                {
                    SceneTransitionManager.Instance.LoadScene(targetSceneBuildIndex, loadingMessage, preferTransitionManagerLoadingScreen);
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
            var operation = SceneManager.LoadSceneAsync(sceneName);

            if (LoadingScreenController.Instance != null)
            {
                yield return LoadingScreenController.Instance.TrackAsyncOperation(operation, "Loading...");
            }
            else
            {
                yield return operation;
            }
        }

        private System.Collections.IEnumerator LoadSceneWithFallback(int buildIndex)
        {
            var operation = SceneManager.LoadSceneAsync(buildIndex);

            if (LoadingScreenController.Instance != null)
            {
                yield return LoadingScreenController.Instance.TrackAsyncOperation(operation, "Loading...");
            }
            else
            {
                yield return operation;
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

            UpdateHoldProgressUI(0f);
        }

        private void UpdateHoldProgressUI(float progress)
        {
            if (!requireHoldToActivate)
            {
                progress = 0f;
            }

            float clampedProgress = Mathf.Clamp01(progress);

            if (holdProgressImage != null)
            {
                holdProgressImage.fillAmount = clampedProgress;
            }

            if (holdProgressSlider != null)
            {
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
