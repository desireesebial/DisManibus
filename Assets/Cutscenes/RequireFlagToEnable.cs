using UnityEngine;

public class RequireFlagToEnable : MonoBehaviour
{
    [Header("Gate")]
    public string requiredFlag = "quest_find_clues_started";

    [Header("What to enable when the flag is set")]
    public Behaviour[] behaviours; // e.g. DialogueInteract, NotePilePickable, ClueFromFlag
    public Collider[] colliders;   // any colliders that should only work after quest starts
    public GameObject[] objects;   // optional extra objects to show/enable

    bool unlocked;

    void Awake()
    {
        // Disable everything at start
        SetEnabled(false);
    }

    void Update()
    {
        if (unlocked) return;
        if (DialogueFlags.Has(requiredFlag))
        {
            unlocked = true;
            SetEnabled(true);
        }
    }

    void SetEnabled(bool on)
    {
        if (behaviours != null)
            foreach (var b in behaviours) if (b) b.enabled = on;

        if (colliders != null)
            foreach (var c in colliders) if (c) c.enabled = on;

        if (objects != null)
            foreach (var go in objects) if (go) go.SetActive(on);
    }
}
