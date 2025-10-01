using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

        [Header("Events")]
        [SerializeField] private UnityEvent onPortalActivated;
        [SerializeField] private UnityEvent onPortalFailed;

        private bool playerInRange;
        private GameObject playerReference;

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
            if (Input.GetKeyDown(interactKey))
            {
                ActivatePortal();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!useTrigger) return;

            if (other.CompareTag(playerTag))
            {
                playerInRange = true;
                playerReference = other.gameObject;
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
                }
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
                    SceneTransitionManager.Instance.LoadScene(targetSceneName);
                }
                else
                {
                    SceneTransitionManager.Instance.LoadScene(targetSceneBuildIndex);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    SceneManager.LoadScene(targetSceneName);
                }
                else
                {
                    SceneManager.LoadScene(targetSceneBuildIndex);
                }
            }
        }

        // Public API for buttons or other triggers
        public void SetPlayerReference(GameObject player)
        {
            playerReference = player;
        }

        public void SetTargetScene(string sceneName)
        {
            targetSceneName = sceneName;
        }

        public void SetTargetScene(int buildIndex)
        {
            targetSceneBuildIndex = buildIndex;
        }
    }
}
