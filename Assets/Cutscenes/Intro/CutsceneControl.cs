using UnityEngine;
using UnityEngine.Playables;

public class CutsceneControl : MonoBehaviour
{
    public PlayableDirector director;
    public MonoBehaviour[] scriptsToDisable; // FPS controller, mouse look, etc.

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        director.played += OnPlayed;
        director.stopped += OnStopped;
    }

    void OnPlayed(PlayableDirector d)
    {
        foreach (var s in scriptsToDisable) if (s) s.enabled = false;
    }

    void OnStopped(PlayableDirector d)
    {
        foreach (var s in scriptsToDisable) if (s) s.enabled = true;
    }
}
