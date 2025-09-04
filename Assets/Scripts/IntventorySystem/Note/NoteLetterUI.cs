using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoteLetterUI : MonoBehaviour
{
    [Header("UI Element Names (Must exist in Canvas)")]
    public string notePanelName = "NotePanel";
    public string paperImageName = "PaperImage";
    public string textContentName = "TextContent";
    public string closeButtonName = "CloseButton";
    
    [Header("Animation")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Input")]
    public KeyCode closeKey = KeyCode.Escape;
    
    // UI References (found automatically)
    private GameObject notePanel;
    private Image paperImage;
    private TextMeshProUGUI textContent;
    private Button closeButton;
    
    private CanvasGroup canvasGroup;
    private NoteLetterSO currentNote;
    private bool isVisible = false;
    
    // Player controller reference
    private FirstPersonController firstPersonController;
    
    void Start()
    {
        // Find UI elements in Canvas by name
        FindUIElements();
        
        // Find player controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            firstPersonController = player.GetComponent<FirstPersonController>();
        }
        
        // Get or add CanvasGroup for fade effects
        if (notePanel != null)
        {
            canvasGroup = notePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notePanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseNote);
        }
        
        // Hide initially
        HideNoteImmediate();
    }
    
    void FindUIElements()
    {
        // Find Canvas in scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene! Please create a Canvas first.");
            return;
        }
        
        // Find UI elements by name
        notePanel = FindUIElementByName(canvas.transform, notePanelName);
        if (notePanel == null)
        {
            Debug.LogError($"NotePanel with name '{notePanelName}' not found in Canvas!");
            return;
        }
        
        paperImage = notePanel.transform.Find(paperImageName)?.GetComponent<Image>();
        textContent = notePanel.transform.Find(textContentName)?.GetComponent<TextMeshProUGUI>();
        closeButton = notePanel.transform.Find(closeButtonName)?.GetComponent<Button>();
        
        // Log any missing elements
        if (paperImage == null) Debug.LogWarning($"PaperImage with name '{paperImageName}' not found!");
        if (textContent == null) Debug.LogWarning($"TextContent with name '{textContentName}' not found!");
        if (closeButton == null) Debug.LogWarning($"CloseButton with name '{closeButtonName}' not found!");
    }
    
    GameObject FindUIElementByName(Transform parent, string name)
    {
        // Search recursively through all children
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
            
            GameObject found = FindUIElementByName(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    void Update()
    {
        // Check for close input
        if (isVisible && Input.GetKeyDown(closeKey))
        {
            CloseNote();
        }
    }
    
    public void ShowNote(NoteLetterSO noteData)
    {
        if (noteData == null) return;
        
        currentNote = noteData;
        UpdateNoteDisplay();
        ShowNoteAnimated();
        
        // Ensure cursor is visible and unlocked for note interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void HideNote()
    {
        if (!isVisible) return;
        
        HideNoteAnimated();
    }
    
    void UpdateNoteDisplay()
    {
        if (currentNote == null) return;
        
        // Update text content - combine all information into one text element
        if (textContent != null)
        {
            string fullText = "";
            
            // Add title if it exists
            if (!string.IsNullOrEmpty(currentNote.noteTitle))
            {
                fullText += $"<b><size=24>{currentNote.noteTitle}</size></b>\n\n";
            }
            
            // Add main content
            if (!string.IsNullOrEmpty(currentNote.noteContent))
            {
                fullText += currentNote.noteContent;
            }
            
            // Add author and date at the bottom
            if (!string.IsNullOrEmpty(currentNote.author) || !string.IsNullOrEmpty(currentNote.date))
            {
                fullText += "\n\n";
                if (!string.IsNullOrEmpty(currentNote.author))
                {
                    fullText += $"<i>By: {currentNote.author}</i>";
                }
                if (!string.IsNullOrEmpty(currentNote.date))
                {
                    if (!string.IsNullOrEmpty(currentNote.author)) fullText += " | ";
                    fullText += $"<i>{currentNote.date}</i>";
                }
            }
            
            textContent.text = fullText;
            textContent.color = currentNote.textColor;
            
            // Update font if specified (uses TMP_FontAsset for TextMeshPro compatibility)
            if (currentNote.textFont != null)
            {
                textContent.font = currentNote.textFont;
            }
        }
        
        // Update paper texture
        if (paperImage != null && currentNote.paperTexture != null)
        {
            paperImage.sprite = currentNote.paperTexture;
        }
    }
    
    void ShowNoteAnimated()
    {
        if (notePanel != null)
        {
            notePanel.SetActive(true);
        }
        
        isVisible = true;
        StartCoroutine(FadeIn());
    }
    
    void HideNoteAnimated()
    {
        StartCoroutine(FadeOut());
    }
    
    void HideNoteImmediate()
    {
        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }
        
        isVisible = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            float alpha = fadeCurve.Evaluate(progress);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            
            yield return null;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            float alpha = 1f - fadeCurve.Evaluate(progress);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            
            yield return null;
        }
        
        HideNoteImmediate();
        
        // Re-lock cursor for FPS gameplay
        if (firstPersonController != null && firstPersonController.lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Notify the pickable object that the note is closed
        if (currentNote != null)
        {
            var pickable = FindObjectOfType<NoteLetterPickable>();
            if (pickable != null)
            {
                pickable.CloseNote();
            }
        }
    }
    
    public void CloseNote()
    {
        HideNote();
    }
    
    // Public method to check if note is currently visible
    public bool IsNoteVisible()
    {
        return isVisible;
    }
    
    // Public method to refresh UI element references (useful if UI changes)
    [ContextMenu("Refresh UI References")]
    public void RefreshUIReferences()
    {
        FindUIElements();
        Debug.Log("UI references refreshed!");
    }
}
