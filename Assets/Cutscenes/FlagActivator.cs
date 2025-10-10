using UnityEngine;

public class FlagActivator : MonoBehaviour
{
    [Header("Gate")]
    public string requiredFlag = "quest_find_clues_started";
    public bool once = true;            // activate once then stop checking
    public bool disableAtStart = true;  // turn things OFF at start

    [Header("What to toggle ON when the flag is set")]
    public GameObject[] objectsToEnable;   // whole objects (prompts, meshes, etc.)
    public Behaviour[] behavioursToEnable; // scripts/components (DialogueInteract, PaperInteract, etc.)
    public Collider[] collidersToEnable;   // colliders you want active only after the flag

    bool activated;

    void Awake()
    {
        if (disableAtStart)
            SetEnabled(false);
    }

    void Update()
    {
        if (activated && once) return;

        // If you're using FlagManager from earlier:
        bool isOn = FlagManager.GetFlag(requiredFlag);

        // If you prefer DialogueFlags instead, comment the line above and use:
        // bool isOn = DialogueFlags.Has(requiredFlag);

        if (isOn)
        {
            SetEnabled(true);
            activated = true;
            if (once) enabled = false; // stop Update() if one-shot
        }
    }

    void SetEnabled(bool on)
    {
        if (objectsToEnable != null)
            foreach (var go in objectsToEnable)
                if (go) go.SetActive(on);

        if (behavioursToEnable != null)
            foreach (var b in behavioursToEnable)
                if (b) b.enabled = on;

        if (collidersToEnable != null)
            foreach (var c in collidersToEnable)
                if (c) c.enabled = on;
    }
}
