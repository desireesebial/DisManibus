using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager; // drag your DialogueManager object here
    [TextArea(2, 4)]
    public string[] lines;                  // the lines you want to show

    bool hasPlayed;

    void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;
        if (other.CompareTag("Player"))
        {
            hasPlayed = true;
            StartCoroutine(PlayDialogue());
        }
    }

    System.Collections.IEnumerator PlayDialogue()
    {
        foreach (string line in lines)
        {
            dialogueManager.ShowLine(line);
            yield return new WaitUntil(() => dialogueManager.lineFinished); // waits for typewriter to finish
            yield return new WaitForSeconds(1f); // optional pause
        }
        dialogueManager.ClearLine();
    }
}
