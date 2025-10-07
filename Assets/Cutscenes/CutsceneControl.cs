using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Linq;

public class CutsceneControl : MonoBehaviour
{
    public PlayableDirector director;

    // Drag the components you want disabled here (movement/look/input).
    public Behaviour[] behavioursToDisable;

    // Optional: auto-find by type names so you don't have to drag anything
    [Tooltip("Optional type names, e.g. FirstPersonController, PlayerInput, MouseLook")]
    public string[] extraTypeNames;
    List<Behaviour> _auto = new();

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        director.played += OnPlayed;
        director.stopped += OnStopped;

        // Auto-resolve extra type names (search in the whole scene or tag your player)
        if (extraTypeNames != null && extraTypeNames.Length > 0)
        {
            var allBehaviours = FindObjectsOfType<Behaviour>(true);
            foreach (var name in extraTypeNames)
            {
                var match = allBehaviours.FirstOrDefault(b => b && b.GetType().Name == name);
                if (match) _auto.Add(match);
            }
        }
    }

    void OnPlayed(PlayableDirector d)
    {
        foreach (var b in behavioursToDisable) if (b) b.enabled = false;
        foreach (var b in _auto) if (b) b.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnStopped(PlayableDirector d)
    {
        foreach (var b in behavioursToDisable) if (b) b.enabled = true;
        foreach (var b in _auto) if (b) b.enabled = true;
    }

    public void Play() => director?.Play();
}
