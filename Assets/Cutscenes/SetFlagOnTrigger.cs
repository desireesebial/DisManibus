using UnityEngine;

public class SetFlagOnTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string flagToSet;
    public bool once = true;
    public KeyCode interactKey = KeyCode.E;
    public bool mustLookAt = false;

    bool hasSet;
    bool inRange;

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
        if (!inRange) return;
        if (once && hasSet) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (mustLookAt)
            {
                var cam = Camera.main;
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 3f))
                    if (hit.collider.gameObject != gameObject) return;
            }

            DialogueFlags.Set(flagToSet);   // <— use DialogueFlags here
            hasSet = true;
        }
    }
}
