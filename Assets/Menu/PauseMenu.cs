using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels (auto-wires if left empty)")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private CanvasGroup pauseGroup;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CanvasGroup settingsGroup;  // NEW: for visibility control
    [SerializeField] private GameObject controlsPanel;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // add this field with your others
    [SerializeField] private CanvasGroup controlsGroup; // CanvasGroup on your Controls panel


    private bool isPaused;

    void Awake() { AutoWire(); }

    void Start()
    {
        SafeSetActive(pauseRoot, true);
        SafeSetActive(settingsPanel, true);
        SafeSetActive(controlsPanel, true);

        SetCanvasVisible(pauseGroup, false);
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(controlsGroup, false);   // <-- hide controls via CanvasGroup

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

            // if paused already, ESC goes back from subpanel or resumes
            if ((settingsPanel && settingsGroup.alpha > 0) || (controlsPanel && controlsPanel.activeSelf))
                BackFromSubPanel();
            else
                Resume();
        }
    }

    // ---------- main pause ----------
    public void Pause()
    {
        Debug.Log("[PauseMenu] Pause()");
        isPaused = true;

        SetCanvasVisible(settingsGroup, false);
        SafeSetActive(controlsPanel, false);
        SafeSetActive(pauseRoot, true);
        SetCanvasVisible(pauseGroup, true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseRoot) pauseRoot.transform.SetAsLastSibling();
    }

    public void Resume()
    {
        Debug.Log("[PauseMenu] Resume()");
        isPaused = false;

        SetCanvasVisible(pauseGroup, false);
        SetCanvasVisible(settingsGroup, false);
        SafeSetActive(controlsPanel, false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ---------- settings ----------
    public void OpenSettings()
    {
        Debug.Log("[PauseMenu] OpenSettings()");

        // hide pause, show settings
        SetCanvasVisible(pauseGroup, false);
        if (pauseGroup) pauseGroup.blocksRaycasts = false;

        if (settingsPanel) settingsPanel.SetActive(true);
        SetCanvasVisible(settingsGroup, true);

        // ensure everything inside Settings is visible & interactable
        foreach (CanvasGroup g in settingsPanel.GetComponentsInChildren<CanvasGroup>(true))
        {
            g.alpha = 1f;
            g.interactable = true;
            g.blocksRaycasts = true;
        }

        // make sure interactive panel renders above background
        var panel = settingsPanel.transform.Find("Panel Settings");
        if (panel) panel.SetAsLastSibling();

        Debug.Log("[PauseMenu] Settings opened successfully.");
    }

    public void CloseSettings()
    {
        Debug.Log("[PauseMenu] CloseSettings()");
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(pauseGroup, true);
        if (pauseGroup) pauseGroup.transform.SetAsLastSibling();
    }

    // ---------- controls ----------
    public void OpenControls()
    {
        Debug.Log("[PauseMenu] OpenControls()");
        SetCanvasVisible(pauseGroup, false);
        SetCanvasVisible(settingsGroup, false);

        if (controlsPanel) controlsPanel.SetActive(true);
        SetCanvasVisible(controlsGroup, true);    // <-- show controls via CanvasGroup

        // make sure interactive bits render above any background
        var panel = controlsPanel.transform.Find("Panel Controls");
        if (panel) panel.SetAsLastSibling();
    }


    public void BackFromSubPanel()
    {
        Debug.Log("[PauseMenu] BackFromSubPanel()");
        SetCanvasVisible(settingsGroup, false);
        SetCanvasVisible(controlsGroup, false);   // <-- hide controls
        SetCanvasVisible(pauseGroup, true);
        if (pauseRoot) pauseRoot.transform.SetAsLastSibling();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

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

    static CanvasGroup GetCanvasGroup(GameObject go)
    {
        return go ? go.GetComponent<CanvasGroup>() : null;
    }

    void AutoWire()
    {
        if (!pauseRoot) pauseRoot = GameObject.Find("Pause Menu");

        if (!settingsPanel)
        {
            var sm = GameObject.Find("SettingsMenu");
            if (sm)
            {
                var child = sm.transform.Find("Panel Settings");
                settingsPanel = child ? child.gameObject : sm;
            }
        }

        if (!controlsPanel)
        {
            var c = GameObject.Find("Controls"); // adjust if your root is named differently
            if (c) controlsPanel = c;
        }

        if (!pauseGroup && pauseRoot) pauseGroup = pauseRoot.GetComponent<CanvasGroup>();
        if (!settingsGroup && settingsPanel) settingsGroup = settingsPanel.GetComponent<CanvasGroup>();
        if (!controlsGroup && controlsPanel) controlsGroup = controlsPanel.GetComponent<CanvasGroup>(); // <-- new

        Debug.Log($"[PauseMenu] AutoWire → pauseRoot={pauseRoot}, settingsPanel={settingsPanel}, controlsPanel={controlsPanel}, pauseGroup={pauseGroup}, settingsGroup={settingsGroup}, controlsGroup={controlsGroup}");
    }
}