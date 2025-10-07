using UnityEngine;
using UnityEngine.Playables;

public class TimelineInteract : MonoBehaviour
{
    public PlayableDirector director;
    public KeyCode interactKey = KeyCode.E;
    public bool playOnce = true;

    bool inRange;
    bool played;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) inRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) inRange = false;
    }

    void Update()
    {
        if (inRange && Input.GetKeyDown(interactKey) && (!playOnce || !played))
        {
            played = true;
            director.Play();
        }
    }
}
