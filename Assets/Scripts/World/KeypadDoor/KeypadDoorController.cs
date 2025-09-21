using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class KeypadDoorController : MonoBehaviour
{
    [Header("Code Settings")]
    public string correctCode = "1234";
    public int codeLength = 4;
    public bool autoCloseOnUnlock = true;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public string interactionPrompt = "Press E to use keypad";

    [Header("UI (auto-built if empty)")]
    public GameObject keypadUI;
    public Text codeDisplay;
    public Text messageText;
    public Button[] numberButtons; // 0..9
    public Button enterButton;
    public Button clearButton;
    public Button closeButton;
    public bool buildUIIfMissing = true;

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip unlockSound;

    [Header("Door")]
    public doorscript targetDoor;
    public bool openDoorOnUnlock = true;

    [Header("Events")]
    public UnityEvent onCorrectCode;
    public UnityEvent onWrongCode;

    private string _currentInput = "";
    private bool _isUIOpen = false;
    private AudioSource _audioSource;
    private Transform _player;
    private SimplePlayerMovement _playerMovement;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerMovement = playerObj.GetComponent<SimplePlayerMovement>();
        }

        if (buildUIIfMissing && keypadUI == null)
        {
            BuildKeypadUI();
        }

        SetupUIBindings();

        if (keypadUI != null)
        {
            keypadUI.SetActive(false);
        }

        UpdateCodeDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && IsPlayerInRange())
        {
            if (!_isUIOpen)
            {
                OpenUI();
            }
            else
            {
                CloseUI();
            }
        }

        if (_isUIOpen && !IsPlayerInRange())
        {
            CloseUI();
        }

        if (_isUIOpen)
        {
            HandleKeyboardInput();
        }
    }

    void HandleKeyboardInput()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                AddDigit(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Backspace();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckCode();
        }
    }

    void SetupUIBindings()
    {
        if (numberButtons != null && numberButtons.Length >= 10)
        {
            for (int i = 0; i < 10; i++)
            {
                int digit = i;
                numberButtons[i].onClick.AddListener(() => AddDigit(digit));
            }
        }

        if (enterButton != null)
        {
            enterButton.onClick.AddListener(CheckCode);
        }

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearInput);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseUI);
        }
    }

    void AddDigit(int digit)
    {
        if (_currentInput.Length >= Mathf.Max(1, codeLength)) return;
        _currentInput += digit.ToString();
        UpdateCodeDisplay();
        PlaySound(buttonClickSound);
    }

    void Backspace()
    {
        if (_currentInput.Length == 0) return;
        _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        UpdateCodeDisplay();
        PlaySound(buttonClickSound);
    }

    void ClearInput()
    {
        _currentInput = "";
        UpdateCodeDisplay();
        PlaySound(buttonClickSound);
    }

    void CheckCode()
    {
        if (_currentInput.Length != Mathf.Max(1, codeLength))
        {
            ShowMessage($"Enter {Mathf.Max(1, codeLength)}-digit code", Color.yellow);
            PlaySound(incorrectSound);
            return;
        }

        if (_currentInput == correctCode)
        {
            ShowMessage("Code accepted", Color.green);
            PlaySound(correctSound);
            onCorrectCode?.Invoke();

            if (targetDoor != null)
            {
                targetDoor.UnlockDoor();
                if (openDoorOnUnlock)
                {
                    targetDoor.OpenDoor();
                }
            }

            PlaySound(unlockSound);

            if (autoCloseOnUnlock)
            {
                Invoke("CloseUI", 1.5f);
            }
        }
        else
        {
            ShowMessage("Incorrect code", Color.red);
            PlaySound(incorrectSound);
            onWrongCode?.Invoke();
            ClearInput();
        }
    }

    void UpdateCodeDisplay()
    {
        if (codeDisplay == null) return;

        int targetLen = Mathf.Max(1, codeLength);
        string display = "";
        for (int i = 0; i < _currentInput.Length; i++) display += "*";
        for (int i = _currentInput.Length; i < targetLen; i++) display += "-";
        codeDisplay.text = display;
    }

    void ShowMessage(string message, Color color)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageText.color = color;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 3f);
    }

    void ClearMessage()
    {
        if (messageText == null) return;
        messageText.text = "";
    }

    void OpenUI()
    {
        if (keypadUI == null) return;
        keypadUI.SetActive(true);
        _isUIOpen = true;
        if (_playerMovement != null) _playerMovement.enabled = false;
        if (targetDoor != null) targetDoor.enabled = false; // prevent door from handling the same E press
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ClearInput();
        ClearMessage();
    }

    void CloseUI()
    {
        if (keypadUI == null) return;
        keypadUI.SetActive(false);
        _isUIOpen = false;
        if (_playerMovement != null) _playerMovement.enabled = true;
        if (targetDoor != null) targetDoor.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    bool IsPlayerInRange()
    {
        if (_player == null) return false;
        return Vector3.Distance(transform.position, _player.position) <= interactionDistance;
    }

    void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    public void SetCode(string newCode)
    {
        if (string.IsNullOrEmpty(newCode)) return;
        correctCode = newCode;
        codeLength = newCode.Length;
        UpdateCodeDisplay();
    }

    void BuildKeypadUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PuzzleCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject panel = new GameObject("KeypadUI");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 500);
        rect.anchoredPosition = Vector2.zero;
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        Text title = CreateText(panel, "Title", "Keypad", 24, new Vector2(0, 200));
        title.color = Color.white;

        codeDisplay = CreateText(panel, "CodeDisplay", "----", 36, new Vector2(0, 120));
        messageText = CreateText(panel, "Message", "", 16, new Vector2(0, 80));

        numberButtons = new Button[10];
        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;
            float x = (col - 1) * 80f;
            float y = 20f - row * 80f;
            numberButtons[i + 1] = CreateButton(panel, (i + 1).ToString(), new Vector2(x, y));
        }
        numberButtons[0] = CreateButton(panel, "0", new Vector2(0, -60f));

        enterButton = CreateButton(panel, "Enter", new Vector2(-60f, -140f));
        clearButton = CreateButton(panel, "Clear", new Vector2(60f, -140f));
        closeButton = CreateButton(panel, "Close", new Vector2(0f, -200f));

        keypadUI = panel;
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
        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleCenter;
        return textComp;
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
        Text buttonText = CreateText(buttonObj, "Text", text, 16, Vector2.zero);
        buttonText.color = Color.white;
        return button;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}


