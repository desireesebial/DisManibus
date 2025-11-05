using UnityEngine;

public class CluePickup : MonoBehaviour
{
    [Header("Clue Settings")]
    public int clueIndex;    // 0–5
    public char clueValue;   // '7', '4', etc.

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    bool inRange;

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
        if (inRange && Input.GetKeyDown(interactKey))
        {
            ClueManager.Instance.RegisterClue(clueIndex, clueValue);
            gameObject.SetActive(false); // optional: disappear once found
        }
    }
}
