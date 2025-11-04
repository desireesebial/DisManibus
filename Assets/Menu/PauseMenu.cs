using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Primary pause menu controller with integrated Settings and Controls panel support.
/// Uses CanvasGroup visibility system for smooth transitions between panels.
///
/// USAGE:
/// - Use this controller when you need pause menu WITH settings/controls panels
/// - Automatically hides gameplay UI elements when paused
/// - Integrates with FirstPersonController for camera control
/// - Prevents pausing during cutscenes and when player is dead
///
/// ALTERNATIVE:
/// - For simpler pause menus without settings integration, see PauseMenuController
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels (auto-wires if left empty)")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private CanvasGroup pauseGroup;

    // Settings root (the whole SettingsMenu) + its CanvasGroup
    [SerializeField] private GameObject settingsPanel;   // should be the SettingsMenu root
    [SerializeField] private CanvasGroup settingsGroup;  // CanvasGroup on SettingsMenu root

    // Controls root + its CanvasGroup
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private CanvasGroup controlsGroup;

    [Header("Border Overlay")]
    [SerializeField] private Image borderOverlay;

    [Header("Gameplay UI to Hide")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject questUI;
    [SerializeField] private GameObject cluesUI;
    [SerializeField] private GameObject staminaUI;

    [Header("Player Camera Control")]
    [SerializeField] private FirstPersonController playerController;

    [Header("Player Health")]
    [SerializeField] private PlayerHealthSystem playerHealthSystem;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    void Awake() { AutoWire(); }

    void Start()
    {
        // Keep objects active but invisible (CanvasGroup alpha) so events work
        SafeSetActive(pauseRoot, true);
        SafeSetActive(settingsPanel, true);
        SafeSetActive(controlsPanel, true);

        SetCanvasVisible(pauseGroup, false);
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(controlsGroup, false);

        // Hide border overlay initially
        if (borderOverlay != null)
        {
            borderOverlay.gameObject.SetActive(false);
        }

        // Auto-find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<FirstPersonController>();
        }

        // Auto-find player health system if not assigned
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();
        }

        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't allow pausing during cutscenes
            if (CutsceneControl.IsPlayingCutscene)
            {
                return;
            }

            // Don't allow pausing when player is dead
            if (playerHealthSystem != null && playerHealthSystem.IsDead())
            {
                return;
            }

            if (!isPaused) { Pause(); return; }

            bool settingsOpen = settingsGroup && settingsGroup.alpha > 0f;
            bool controlsOpen = controlsPanel && controlsPanel.activeSelf && controlsGroup && controlsGroup.alpha > 0f;

            if (settingsOpen || controlsOpen) BackFromSubPanel();
            else Resume();
        }
    }

    // ---------- main pause ----------
    public void Pause()
    {
        // Don't allow pausing when player is dead
        if (playerHealthSystem != null && playerHealthSystem.IsDead())
        {
            return;
        }

        isPaused = true;

        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(controlsGroup, false);
        SafeSetActive(pauseRoot, true);
        SetCanvasVisible(pauseGroup, true);

        // Show border overlay
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

        if (questUI != null)
        {
            questUI.SetActive(false);
        }

        if (cluesUI != null)
        {
            cluesUI.SetActive(false);
        }

        if (staminaUI != null)
        {
            staminaUI.SetActive(false);
        }

        // Disable camera look
        if (playerController != null)
        {
            playerController.DisableCameraLook();
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseRoot) pauseRoot.transform.SetAsLastSibling();
    }

    public void Resume()
    {
        isPaused = false;

        SetCanvasVisible(pauseGroup, false);
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(controlsGroup, false);

        // Hide border overlay
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

        if (questUI != null)
        {
            questUI.SetActive(true);
        }

        if (cluesUI != null)
        {
            cluesUI.SetActive(true);
        }

        if (staminaUI != null)
        {
            staminaUI.SetActive(true);
        }

        // Enable camera look
        if (playerController != null)
        {
            playerController.EnableCameraLook();
        }

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Opens the settings panel and ensures it renders properly on top of the pause menu
    /// </summary>
    public void OpenSettings()
    {
        // Hide pause menu
        SetCanvasVisible(pauseGroup, false);
        if (pauseGroup) pauseGroup.blocksRaycasts = false;

        // Activate and show settings panel
        if (settingsPanel) settingsPanel.SetActive(true);
        SetCanvasVisible(settingsGroup, true);

        // Ensure settings renders on top of pause menu
        var settingsCanvas = settingsPanel.GetComponentInParent<Canvas>();
        if (!settingsCanvas) settingsCanvas = settingsPanel.AddComponent<Canvas>();
        var pauseCanvas = pauseRoot ? pauseRoot.GetComponentInParent<Canvas>() : null;

        settingsCanvas.overrideSorting = true;
        settingsCanvas.sortingOrder = 1000;
        if (pauseCanvas && pauseCanvas.overrideSorting && settingsCanvas.sortingOrder <= pauseCanvas.sortingOrder)
            settingsCanvas.sortingOrder = pauseCanvas.sortingOrder + 1;

        // Ensure GraphicRaycaster exists for click detection
        if (!settingsPanel.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>())
            settingsPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Ensure interactive panel is on top
        var panel = settingsPanel.transform.Find("Panel Settings");
        if (panel) panel.SetAsLastSibling();

        // Ensure all child CanvasGroups are properly visible and interactive
        foreach (var g in settingsPanel.GetComponentsInChildren<CanvasGroup>(true))
        {
            g.alpha = 1f;
            g.interactable = true;
            g.blocksRaycasts = true;
        }
    }


    public void CloseSettings()
    {
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(pauseGroup, true);
        if (pauseRoot) pauseRoot.transform.SetAsLastSibling();
    }

    // ---------- controls ----------
    public void OpenControls()
    {
        SetCanvasVisible(pauseGroup, false);
        SetCanvasVisible(settingsGroup, false);

        if (controlsPanel) controlsPanel.SetActive(true);
        SetCanvasVisible(controlsGroup, true);

        var panel = controlsPanel.transform.Find("Panel Controls");
        if (panel) panel.SetAsLastSibling();
    }

    public void BackFromSubPanel()
    {
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(controlsGroup, false);
        SetCanvasVisible(pauseGroup, true);
        if (pauseRoot) pauseRoot.transform.SetAsLastSibling();
    }

    public void GoToMainMenu()
    {
        // Hide border overlay
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

        if (questUI != null)
        {
            questUI.SetActive(true);
        }

        if (cluesUI != null)
        {
            cluesUI.SetActive(true);
        }

        if (staminaUI != null)
        {
            staminaUI.SetActive(true);
        }

        // Enable camera look
        if (playerController != null)
        {
            playerController.EnableCameraLook();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame() => Application.Quit();

    // ---------- helpers ----------
    static void SafeSetActive(GameObject go, bool v)
    {
        if (go && go.activeSelf != v) go.SetActive(v);
    }

    static void SetCanvasVisible(CanvasGroup g, bool visible)
    {
        if (!g) return;
        g.alpha = visible ? 1f : 0f;
        g.interactable = visible;
        g.blocksRaycasts = visible;
        if (!g.gameObject.activeSelf) g.gameObject.SetActive(true);
    }

    static CanvasGroup GetCanvasGroup(GameObject go) => go ? go.GetComponent<CanvasGroup>() : null;

    /// <summary>
    /// Automatically finds and wires up UI panels if not assigned in the Inspector
    /// </summary>
    void AutoWire()
    {
        if (!pauseRoot) pauseRoot = GameObject.Find("Pause Menu");
        if (!pauseGroup && pauseRoot) pauseGroup = pauseRoot.GetComponent<CanvasGroup>();

        // SETTINGS: use the SettingsMenu root + its CanvasGroup
        if (!settingsPanel)
        {
            var sm = GameObject.Find("SettingsMenu");
            if (sm) settingsPanel = sm;
        }
        if (!settingsGroup && settingsPanel) settingsGroup = settingsPanel.GetComponent<CanvasGroup>();

        // CONTROLS: root + its CanvasGroup
        if (!controlsPanel)
        {
            var c = GameObject.Find("Controls");
            if (c) controlsPanel = c;
        }
        if (!controlsGroup && controlsPanel) controlsGroup = controlsPanel.GetComponent<CanvasGroup>();
    }
}
