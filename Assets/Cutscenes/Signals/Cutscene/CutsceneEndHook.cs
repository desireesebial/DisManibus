using UnityEngine;
using UnityEngine.Playables;

public class CutsceneEndHook : MonoBehaviour
{
    public PlayableDirector director;                 // on Cutscene_Player
    public AfterCutsceneCameraReset reset;            // same object that has your reset script

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        director.stopped += OnStopped;
    }

    void OnStopped(PlayableDirector d)
    {
        reset?.OnCutsceneEnd();
    }
}
