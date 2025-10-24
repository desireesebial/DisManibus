using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Player Camera Control")]
    [SerializeField] private FirstPersonController playerController;

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

        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        Debug.Log("[PauseMenu] Resume() called");
        isPaused = false;

        SetCanvasVisible(pauseGroup, false);
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(controlsGroup, false);

        // Hide border overlay
        Debug.Log($"[PauseMenu] borderOverlay is null? {borderOverlay == null}");
        if (borderOverlay != null)
        {
            Debug.Log($"[PauseMenu] Hiding border: {borderOverlay.gameObject.name}, currently active: {borderOverlay.gameObject.activeSelf}");
            borderOverlay.gameObject.SetActive(false);
            Debug.Log($"[PauseMenu] Border hidden. Now active: {borderOverlay.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[PauseMenu] borderOverlay is NULL! Cannot hide border.");
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

    // ---------- settings ----------
    public void OpenSettings()
    {
        // 1) Hide Pause (no input, no draw)
        SetCanvasVisible(pauseGroup, false);
        if (pauseGroup) pauseGroup.blocksRaycasts = false;

        // 2) Make sure Settings root is active
        if (settingsPanel) settingsPanel.SetActive(true);

        // 3) Make Settings visible & clickable
        SetCanvasVisible(settingsGroup, true);

        // 4) FORCE settings to render on top of pause
        var settingsCanvas = settingsPanel.GetComponentInParent<Canvas>();
        if (!settingsCanvas) settingsCanvas = settingsPanel.AddComponent<Canvas>();
        var pauseCanvas = pauseRoot ? pauseRoot.GetComponentInParent<Canvas>() : null;

        settingsCanvas.overrideSorting = true;
        settingsCanvas.sortingOrder = 1000;
        if (pauseCanvas && pauseCanvas.overrideSorting && settingsCanvas.sortingOrder <= pauseCanvas.sortingOrder)
            settingsCanvas.sortingOrder = pauseCanvas.sortingOrder + 1;

        // 5) Ensure a GraphicRaycaster exists so clicks work
        if (!settingsPanel.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>())
            settingsPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 6) If your Settings has a background covering the screen, it can block clicks.
        //    Make sure the INTERACTIVE container is on top.
        var panel = settingsPanel.transform.Find("Panel Settings");
        if (panel) panel.SetAsLastSibling();

        // 7) Last safeguard: if any child CanvasGroups exist, open them too
        foreach (var g in settingsPanel.GetComponentsInChildren<CanvasGroup>(true))
        {
            g.alpha = 1f;
            g.interactable = true;
            g.blocksRaycasts = true;
        }

        Debug.Log("[PauseMenu] OpenSettings → forced visible & on top");
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

        Debug.Log($"[PauseMenu] AutoWire → pauseRoot={pauseRoot}, settingsPanel={settingsPanel}, controlsPanel={controlsPanel}, pauseGroup={pauseGroup}, settingsGroup={settingsGroup}, controlsGroup={controlsGroup}");
    }
}
