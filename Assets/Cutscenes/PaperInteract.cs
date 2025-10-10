using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PaperInteract : MonoBehaviour
{
    [Header("References")]
    public GameObject paperCanvas;             // Canvas that displays the paper image
    public Sprite paperSprite;                 // The sprite to show when opened
    public UnityEngine.UI.Image paperImage;    // The Image inside the canvas that renders the sprite

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode closeKey = KeyCode.Escape;
    public bool pausePlayerMovement = true;

    [Header("Clue UI / Pickup")]
    public bool showClueUIOnOpen = true;       // Show the "X/6" + code UI when opening the paper
    public bool registerClueOnFirstOpen = false; // If true, register a clue once when opened
    [Tooltip("0..(TotalClues-1). Set to -1 to disable.")]
    public int clueIndex = -1;                 // Which slot to fill (0-based)
    public char clueValue = '_';               // Digit/char to put in that slot
    public bool giveClueOnce = true;           // Only register once from this paper

    bool inRange = false;
    bool isOpen = false;
    bool clueGiven = false;

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
        if (paperCanvas)
            paperCanvas.SetActive(false);
        clueGiven = false;
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
        if (inRange && Input.GetKeyDown(interactKey) && !isOpen)
            OpenPaper();
        else if (isOpen && Input.GetKeyDown(closeKey))
            ClosePaper();
    }

    void OpenPaper()
    {
        if (!paperCanvas || !paperImage || !paperSprite)
        {
            Debug.LogWarning("[PaperInteract] Missing references.", this);
            return;
        }

        // Show paper
        paperImage.sprite = paperSprite;
        paperCanvas.SetActive(true);
        isOpen = true;

        // Show the Clue UI immediately if requested
        if (showClueUIOnOpen && ClueManager.Instance != null)
            ClueManager.Instance.ShowClueUI();

        // Optionally register a clue on the first open (no second press needed)
        if (registerClueOnFirstOpen && !clueGiven && ClueManager.Instance != null && clueIndex >= 0)
        {
            ClueManager.Instance.RegisterClue(clueIndex, clueValue);
            if (giveClueOnce) clueGiven = true;
        }

        // Optional: pause game and unlock cursor while reading
        if (pausePlayerMovement)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClosePaper()
    {
        if (!paperCanvas) return;

        paperCanvas.SetActive(false);
        isOpen = false;

        if (pausePlayerMovement)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
