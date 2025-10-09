using UnityEngine;

public class ClueFromFlag : MonoBehaviour
{
    [Header("Watch this dialogue flag")]
    public string flagName;   // must match DialogueInteract.SetFlagWhenUsed

    [Header("Which code slot/value to fill")]
    public int clueIndex;     // 0..5 for a 6-digit code
    public char clueValue;    // e.g., '7'

    bool consumed;

    void Update()
    {
        if (consumed) return;
        if (string.IsNullOrEmpty(flagName)) return;

        if (DialogueFlags.Has(flagName))
        {
            if (ClueManager.Instance) ClueManager.Instance.RegisterClue(clueIndex, clueValue);
            consumed = true;  // fire only once
        }
    }
}
