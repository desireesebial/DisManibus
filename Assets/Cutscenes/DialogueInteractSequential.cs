using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueInteractSequential : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;   // Drag your DialogueManager here
    [TextArea(2, 4)] public string[] lines;   // The dialogue lines to show (in order)

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode closeKey = KeyCode.Escape;
    public bool pausePlayerMovement = true;   // Optional, pauses gameplay while reading

    bool inRange = false;
    bool isTalking = false;
    bool isTyping = false;
    int currentLine = 0;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
    }

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

        // Start or advance dialogue
        if (Input.GetKeyDown(interactKey))
        {
            if (!isTalking)
                StartDialogue();
            else if (!isTyping)
                NextLine();
        }

        // Optional close key
        if (isTalking && Input.GetKeyDown(closeKey))
        {
            EndDialogue();
        }
    }

    void StartDialogue()
    {
        if (lines.Length == 0) return;

        isTalking = true;
        currentLine = 0;

        if (pausePlayerMovement)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ShowCurrentLine();
    }

    void NextLine()
    {
        currentLine++;
        if (currentLine >= lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    void ShowCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine(lines[currentLine]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueManager.ShowLine(line);
        yield return new WaitUntil(() => dialogueManager.lineFinished);
        isTyping = false;
    }

    void EndDialogue()
    {
        isTalking = false;
        dialogueManager.ClearLine();

        if (pausePlayerMovement)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
