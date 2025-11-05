using UnityEngine;

public class ClueFromFlag : MonoBehaviour
{
    [Header("Watch this dialogue flag")]
    public string flagName;

    [Header("Which code slot/value to fill")]
    public int clueIndex;   // 0..5
    public char clueValue;  // e.g., '7'

    bool lastSeen = false;

    void Update()
    {
        if (string.IsNullOrEmpty(flagName)) return;

        bool now = DialogueFlags.Has(flagName);
        if (now && !lastSeen)   // false -> true edge
        {
            if (ClueManager.Instance)
                ClueManager.Instance.RegisterClue(clueIndex, clueValue);
        }
        lastSeen = now;
    }

    // Optional: allow external resets (e.g., from a restart button)
    public void ResetWatcher() { lastSeen = false; }
}
