using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;    // drag your new cutscene director here
    public bool playOnce = true;

    bool _played;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playOnce && _played) return;

        _played = true;
        director.Play();  // 🎬 starts your Timeline cutscene
    }
}
