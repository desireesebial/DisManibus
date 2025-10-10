using UnityEngine;

public class FlagActivator : MonoBehaviour
{
    [Header("Gate")]
    public string requiredFlag = "quest_find_clues_started";
    public bool once = true;
    public bool disableAtStart = true;

    [Header("Enable when flag is set")]
    public GameObject[] objectsToEnable;
    public Behaviour[] behavioursToEnable;
    public Collider[] collidersToEnable;

    bool activated;

    void Awake()
    {
        if (disableAtStart) SetEnabled(false);
    }

    void Update()
    {
        if (activated && once) return;

        // *** USE DialogueFlags ***
        bool isOn = DialogueFlags.Has(requiredFlag);
        if (isOn)
        {
            SetEnabled(true);
            activated = true;
            if (once) enabled = false;
        }
    }

    void SetEnabled(bool on)
    {
        if (objectsToEnable != null)
            foreach (var go in objectsToEnable) if (go) go.SetActive(on);

        if (behavioursToEnable != null)
            foreach (var b in behavioursToEnable) if (b) b.enabled = on;

        if (collidersToEnable != null)
            foreach (var c in collidersToEnable) if (c) c.enabled = on;
    }
}
