using UnityEngine;

public class DialogueInteract : MonoBehaviour
{
    public DialogueManager dialogueManager;
    [TextArea(2, 4)]
    public string[] lines;
    public KeyCode interactKey = KeyCode.E;

    bool playerInRange;
    bool talking;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey) && !talking)
            StartCoroutine(StartDialogue());
    }

    System.Collections.IEnumerator StartDialogue()
    {
        talking = true;
        foreach (string line in lines)
        {
            dialogueManager.ShowLine(line);
            yield return new WaitUntil(() => dialogueManager.lineFinished);
            yield return new WaitForSeconds(0.5f);
        }
        dialogueManager.ClearLine();
        talking = false;
    }
}
