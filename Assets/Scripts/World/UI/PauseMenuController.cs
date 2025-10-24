using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace World.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference pauseAction;
        [SerializeField] private KeyCode fallbackToggleKey = KeyCode.Escape;

        [Header("UI")]
        [SerializeField] private GameObject pauseMenuRoot;
        [SerializeField] private GameObject firstSelected;
        [SerializeField] private Image borderOverlay;

        [Header("Gameplay UI to Hide")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject healthUI;
        [SerializeField] private GameObject dialogueUI;

        [Header("Behaviour")]
        [SerializeField] private bool startPaused;
        [SerializeField] private bool allowResume = true;
        [SerializeField] private bool lockCursorWhenResumed = true;
        [SerializeField] private float timeScaleWhilePaused = 0f;

        [Header("Navigation")]
        [SerializeField] private string mainMenuSceneName;

        [Header("Events")]
        [SerializeField] private UnityEvent onPaused;
        [SerializeField] private UnityEvent onResumed;

        private bool isPaused;
        private float cachedTimeScale = 1f;

        private void Awake()
        {
            if (pauseMenuRoot != null)
            {
                pauseMenuRoot.SetActive(false);
            }

            if (borderOverlay != null)
            {
                borderOverlay.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (pauseAction != null)
            {
                pauseAction.action.performed += HandlePauseAction;
                pauseAction.action.Enable();
            }

            if (startPaused)
            {
                Pause();
            }
        }

        private void OnDisable()
        {
            if (pauseAction != null)
            {
                pauseAction.action.performed -= HandlePauseAction;
                pauseAction.action.Disable();
            }
        }

        private void Update()
        {
            if (pauseAction == null && Input.GetKeyDown(fallbackToggleKey))
            {
                TogglePause();
            }
        }

        private void HandlePauseAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (isPaused)
            {
                return;
            }

            cachedTimeScale = Time.timeScale;
            Time.timeScale = timeScaleWhilePaused;

            if (pauseMenuRoot != null)
            {
                pauseMenuRoot.SetActive(true);
            }

            if (borderOverlay != null)
            {
                borderOverlay.gameObject.SetActive(true);
            }

            // Hide gameplay UI
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            if (healthUI != null)
            {
                healthUI.SetActive(false);
            }

            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
            }

            if (firstSelected != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            isPaused = true;
            onPaused?.Invoke();
        }

        public void Resume()
        {
            if (!isPaused || !allowResume)
            {
                return;
            }

            Time.timeScale = cachedTimeScale;

            if (pauseMenuRoot != null)
            {
                pauseMenuRoot.SetActive(false);
            }

            if (borderOverlay != null)
            {
                borderOverlay.gameObject.SetActive(false);
            }

            // Show gameplay UI
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }

            if (healthUI != null)
            {
                healthUI.SetActive(true);
            }

            if (dialogueUI != null)
            {
                dialogueUI.SetActive(true);
            }

            if (lockCursorWhenResumed)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            isPaused = false;
            onResumed?.Invoke();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void ExitToMainMenu()
        {
            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                Debug.LogWarning($"{nameof(PauseMenuController)}: Main menu scene name is not set.", this);
                return;
            }

            Time.timeScale = cachedTimeScale;

            if (pauseMenuRoot != null)
            {
                pauseMenuRoot.SetActive(false);
            }

            if (borderOverlay != null)
            {
                borderOverlay.gameObject.SetActive(false);
            }

            // Show gameplay UI
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }

            if (healthUI != null)
            {
                healthUI.SetActive(true);
            }

            if (dialogueUI != null)
            {
                dialogueUI.SetActive(true);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            isPaused = false;
            onResumed?.Invoke();

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        public void ForceResume()
        {
            isPaused = true;
            Resume();
        }

        public bool IsPaused()
        {
            return isPaused;
        }
    }
}

