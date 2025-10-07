using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueInteract : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;     // Drag your DialogueManager here

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] lines;                      // Lines to randomly pick from

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public bool mustFinishBeforeNext = true;    // Wait until typing is done before next press
    public bool avoidImmediateRepeat = true;    // Don’t repeat the same line twice in a row

    private bool inRange = false;
    private bool typing = false;
    private int lastIndex = -1;

    void Reset()
    {
        // Makes sure the collider is a trigger and has a kinematic rigidbody for triggers to work
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!TryGetComponent<Rigidbody>(out var rb))
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
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
        if (mustFinishBeforeNext && typing) return;

        if (Input.GetKeyDown(interactKey))
        {
            ShowRandomLine();
        }
    }

    void ShowRandomLine()
    {
        if (lines == null || lines.Length == 0) return;

        int index = Random.Range(0, lines.Length);

        // Avoid repeating the same line twice in a row (optional)
        if (avoidImmediateRepeat && lines.Length > 1)
        {
            while (index == lastIndex)
                index = Random.Range(0, lines.Length);
        }

        lastIndex = index;
        StartCoroutine(TypeAndUnlock(lines[index]));
    }

    IEnumerator TypeAndUnlock(string line)
    {
        typing = true;
        dialogueManager.ShowLine(line);
        yield return new WaitUntil(() => dialogueManager.lineFinished);
        typing = false;
    }
}
