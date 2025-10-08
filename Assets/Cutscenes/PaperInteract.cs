using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PaperInteract : MonoBehaviour
{
    [Header("References")]
    public GameObject paperCanvas;        // The Canvas that displays the paper image
    public Sprite paperSprite;            // The image to show when opened
    public UnityEngine.UI.Image paperImage; // Reference to the Image inside the Canvas

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode closeKey = KeyCode.Escape;
    public bool pausePlayerMovement = true;

    bool inRange = false;
    bool isOpen = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!TryGetComponent<Rigidbody>(out var rb))
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Start()
    {
        if (paperCanvas)
            paperCanvas.SetActive(false);
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
            Debug.LogWarning("PaperInteract missing references!", this);
            return;
        }

        paperImage.sprite = paperSprite;
        paperCanvas.SetActive(true);
        isOpen = true;

        // Optional: pause player movement & unlock cursor
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
