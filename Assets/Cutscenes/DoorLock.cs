using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorLock : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public bool mustLookAtDoor = false; // set true if you want a raycast check
    public float lookMaxAngle = 35f;    // used if mustLookAtDoor

    [Header("Opening")]
    public Animator animator;           // optional; set a trigger or bool
    public string openTrigger = "Open"; // Animator trigger name
    public Collider blockingCollider;   // collider that blocks the doorway; disable on open
    public AudioSource sfx;             // optional
    public AudioClip lockedClip;        // played when locked
    public AudioClip openClip;          // played when opened

    [Header("Requirements")]
    [Tooltip("If true, door can open once ItemTracker.IsComplete is true.")]
    public bool requireItemsQuestComplete = true;
    [Tooltip("Optional: require this DialogueFlags flag instead / in addition.")]
    public string requiredFlag = "items_complete";

    [Header("Feedback")]
    public DialogueManager dialogueManager; // optional: show 'Locked' line
    [TextArea] public string lockedLine = "It's locked. I should find the items first.";

    bool inRange;
    bool opened;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        if (!blockingCollider)
        {
            // try to find a non-trigger collider on the same object or a child
            var cols = GetComponentsInChildren<Collider>();
            foreach (var c in cols)
                if (!c.isTrigger) { blockingCollider = c; break; }
        }

        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) inRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) inRange = false;
    }

    void Update()
    {
        if (!inRange || opened) return;
        if (Input.GetKeyDown(interactKey))
        {
            if (CanOpen())
                OpenDoor();
            else
                TellLocked();
        }
    }

    bool CanOpen()
    {
        if (mustLookAtDoor && !PlayerLookingAtDoor()) return false;

        bool ok = true;

        if (requireItemsQuestComplete && ItemTracker.Instance != null)
            ok &= ItemTracker.Instance.IsComplete;

        if (!string.IsNullOrEmpty(requiredFlag))
            ok &= DialogueFlags.Has(requiredFlag);

        return ok;
    }

    bool PlayerLookingAtDoor()
    {
        var cam = Camera.main;
        if (!cam) return true;
        Vector3 toDoor = (transform.position - cam.transform.position).normalized;
        float angle = Vector3.Angle(cam.transform.forward, toDoor);
        return angle <= lookMaxAngle;
    }

    void OpenDoor()
    {
        opened = true;

        if (sfx && openClip) { sfx.PlayOneShot(openClip); }

        if (animator && !string.IsNullOrEmpty(openTrigger))
            animator.SetTrigger(openTrigger);

        if (blockingCollider)
            blockingCollider.enabled = false; // let the player through
    }

    void TellLocked()
    {
        if (sfx && lockedClip) sfx.PlayOneShot(lockedClip);
        if (dialogueManager && !string.IsNullOrWhiteSpace(lockedLine))
            dialogueManager.ShowLine(lockedLine);
    }
}
