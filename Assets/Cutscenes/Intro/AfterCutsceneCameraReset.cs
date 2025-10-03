using UnityEngine;
using UnityEngine.Playables;

public class AfterCutsceneCameraReset : MonoBehaviour
{
    [Header("Refs")]
    public PlayableDirector cutsceneDirector;   // Intro_Cutscene
    public GameObject cutsceneVcamGO;           // The VCutscene_Main GameObject
    public GameObject playerVcamGO;             // (optional) your gameplay vcam GameObject
    public Behaviour moveScript;                // your movement component (e.g., FirstPersonController)
    public Behaviour lookScript;                // your look component (mouse look)

    void Awake()
    {
        if (!cutsceneDirector) cutsceneDirector = GetComponent<PlayableDirector>();
        if (cutsceneDirector) cutsceneDirector.stopped += OnCutsceneStopped;
    }

    void OnDestroy()
    {
        if (cutsceneDirector) cutsceneDirector.stopped -= OnCutsceneStopped;
    }

    void OnCutsceneStopped(PlayableDirector d)
    {
        // Release cutscene camera
        if (cutsceneVcamGO) cutsceneVcamGO.SetActive(false);
        if (playerVcamGO) playerVcamGO.SetActive(true);

        // Re-enable controls
        if (moveScript) moveScript.enabled = true;
        if (lookScript) lookScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
