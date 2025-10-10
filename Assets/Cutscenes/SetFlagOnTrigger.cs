using UnityEngine;

public class SetFlagOnTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string flagToSet;
    public bool once = true;
    public KeyCode interactKey = KeyCode.E;
    public bool mustLookAt = false; // optional if you want raycast check

    bool hasSet;
    bool inRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = false;
    }

    void Update()
    {
        if (!inRange) return;
        if (once && hasSet) return;

        // Wait for player to actually press E
        if (Input.GetKeyDown(interactKey))
        {
            // optional: check if player is looking at this object (forward ray)
            if (mustLookAt)
            {
                Camera cam = Camera.main;
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 3f))
                {
                    if (hit.collider.gameObject != gameObject) return;
                }
            }

            FlagManager.SetFlag(flagToSet);
            hasSet = true;
        }
    }
}
