using UnityEngine;

public class DialogueRelay : MonoBehaviour
{
    public DialogueManager dialogue;
    public void ShowLine(string s) => dialogue.ShowLine(s);
    public void ClearLine() => dialogue.ClearLine();
}
