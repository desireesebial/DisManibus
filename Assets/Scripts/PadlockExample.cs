using UnityEngine;
using UnityEngine.UI;

public class PadlockExample : MonoBehaviour
{
    [Header("Example Setup")]
    public CombinationPadlock mainPadlock;
    public PuzzleHint[] hints;
    public PuzzleManager puzzleManager;
    
    [Header("UI References")]
    public GameObject padlockUIPrefab;
    public GameObject hintUIPrefab;
    
    [Header("Audio Clips")]
    public AudioClip buttonClick;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip unlockSound;
    
    void Start()
    {
        SetupExamplePuzzle();
    }
    
    void SetupExamplePuzzle()
    {
        // Create padlock UI if not exists
        if (mainPadlock != null && mainPadlock.padlockUI == null)
        {
            CreatePadlockUI();
        }
        
        // Create hint UIs if not exists
        if (hints != null)
        {
            foreach (var hint in hints)
            {
                if (hint != null && hint.hintUI == null)
                {
                    CreateHintUI(hint);
                }
            }
        }
        
        // Setup puzzle manager
        if (puzzleManager != null)
        {
            SetupPuzzleManager();
        }
        
        // Configure audio
        SetupAudio();
    }
    
    void CreatePadlockUI()
    {
        // Create canvas if not exists
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PuzzleCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create padlock UI panel
        GameObject padlockUI = new GameObject("PadlockUI");
        padlockUI.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = padlockUI.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 500);
        rect.anchoredPosition = Vector2.zero;
        
        Image panelImage = padlockUI.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Create title
        CreateText(padlockUI, "Title", "Combination Lock", 24, new Vector2(0, 200));
        
        // Create combination display
        Text comboDisplay = CreateText(padlockUI, "ComboDisplay", "----", 36, new Vector2(0, 120));
        
        // Create message text
        Text messageText = CreateText(padlockUI, "Message", "", 16, new Vector2(0, 80));
        
        // Create number buttons
        Button[] numberButtons = new Button[10];
        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;
            float x = (col - 1) * 80;
            float y = 20 - row * 80;
            numberButtons[i] = CreateButton(padlockUI, (i + 1).ToString(), new Vector2(x, y));
        }
        numberButtons[9] = CreateButton(padlockUI, "0", new Vector2(0, -60));
        
        // Create action buttons
        Button enterButton = CreateButton(padlockUI, "Enter", new Vector2(-60, -140));
        Button clearButton = CreateButton(padlockUI, "Clear", new Vector2(60, -140));
        Button closeButton = CreateButton(padlockUI, "Close", new Vector2(0, -200));
        
        // Assign to padlock
        mainPadlock.padlockUI = padlockUI;
        mainPadlock.combinationDisplay = comboDisplay;
        mainPadlock.messageText = messageText;
        mainPadlock.numberButtons = numberButtons;
        mainPadlock.enterButton = enterButton;
        mainPadlock.clearButton = clearButton;
        mainPadlock.closeButton = closeButton;
    }
    
    void CreateHintUI(PuzzleHint hint)
    {
        // Create canvas if not exists
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PuzzleCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create hint UI panel
        GameObject hintUI = new GameObject("HintUI");
        hintUI.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = hintUI.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300, 400);
        rect.anchoredPosition = Vector2.zero;
        
        Image panelImage = hintUI.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Create title
        CreateText(hintUI, "Title", "Hint", 24, new Vector2(0, 150));
        
        // Create hint text
        Text hintText = CreateText(hintUI, "HintText", hint.hintText, 16, new Vector2(0, 80));
        
        // Create combination text
        Text comboText = CreateText(hintUI, "ComboText", "", 16, new Vector2(0, 40));
        
        // Create buttons
        Button revealButton = CreateButton(hintUI, "Reveal Hint", new Vector2(0, -20));
        Button closeButton = CreateButton(hintUI, "Close", new Vector2(0, -80));
        
        // Assign to hint
        hint.hintUI = hintUI;
        hint.hintTextDisplay = hintText;
        hint.combinationTextDisplay = comboText;
        hint.revealButton = revealButton;
        hint.closeButton = closeButton;
    }
    
    Text CreateText(GameObject parent, string name, string text, int fontSize, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300, 50);
        rect.anchoredPosition = position;
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        return textComponent;
    }
    
    Button CreateButton(GameObject parent, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(80, 40);
        rect.anchoredPosition = position;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Create text for button
        Text buttonText = CreateText(buttonObj, "Text", text, 16, Vector2.zero);
        buttonText.color = Color.white;
        
        return button;
    }
    
    void SetupPuzzleManager()
    {
        // Add padlocks to manager
        if (mainPadlock != null)
        {
            puzzleManager.padlocks.Add(mainPadlock);
        }
        
        // Setup hints
        if (hints != null && hints.Length > 0)
        {
            puzzleManager.puzzleHints = new string[hints.Length];
            for (int i = 0; i < hints.Length; i++)
            {
                if (hints[i] != null)
                {
                    puzzleManager.puzzleHints[i] = hints[i].hintText;
                }
            }
        }
        
        // Setup events
        puzzleManager.onAllPuzzlesComplete.AddListener(OnPuzzleComplete);
        puzzleManager.onPuzzleProgress.AddListener(OnPuzzleProgress);
    }
    
    void SetupAudio()
    {
        if (mainPadlock != null)
        {
            mainPadlock.buttonClickSound = buttonClick;
            mainPadlock.correctSound = correctSound;
            mainPadlock.incorrectSound = incorrectSound;
            mainPadlock.unlockSound = unlockSound;
        }
        
        if (hints != null)
        {
            foreach (var hint in hints)
            {
                if (hint != null)
                {
                    // You can assign audio clips here if needed
                }
            }
        }
    }
    
    void OnPuzzleComplete()
    {
        Debug.Log("Example puzzle completed!");
        
        // You can add custom completion logic here
        // For example: open a door, trigger a cutscene, etc.
    }
    
    void OnPuzzleProgress()
    {
        Debug.Log("Puzzle progress made!");
        
        // You can add custom progress logic here
        // For example: play a sound, show a message, etc.
    }
    
    // Example method to set up a simple office safe puzzle
    [ContextMenu("Setup Office Safe Puzzle")]
    void SetupOfficeSafePuzzle()
    {
        // Set combination to a memorable number
        if (mainPadlock != null)
        {
            mainPadlock.SetCombination("2024");
        }
        
        // Setup hints
        if (hints != null && hints.Length > 0)
        {
            hints[0].SetHintText("Check the calendar on the desk...");
            hints[0].SetCombinationHint("The year is written on the calendar");
        }
        
        Debug.Log("Office safe puzzle setup complete! Combination: 2024");
    }
    
    // Example method to set up a sequential door puzzle
    [ContextMenu("Setup Sequential Door Puzzle")]
    void SetupSequentialDoorPuzzle()
    {
        if (puzzleManager != null)
        {
            puzzleManager.requireAllPadlocks = true;
            puzzleManager.sequentialUnlocking = true;
            puzzleManager.enableHints = true;
            puzzleManager.hintDelay = 60f; // 1 minute before hints
        }
        
        Debug.Log("Sequential door puzzle setup complete!");
    }
} 