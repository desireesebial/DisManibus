using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.02f;

    // ✅ Add this:
    [HideInInspector] public bool lineFinished = false;

    public void ShowLine(string line)
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(string line)
    {
        lineFinished = false; // reset at start
        dialogueText.text = "";
        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        lineFinished = true; // ✅ mark done when finished typing
    }

    public void ClearLine()
    {
        dialogueText.text = "";
    }
}
