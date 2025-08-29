using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestSystem : MonoBehaviour
{
    [Header("Quest Management")]
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();
    
    [Header("Quest UI")]
    public GameObject questPopup;
    public TextMeshProUGUI questPopupTitle;
    public TextMeshProUGUI questPopupDescription;
    public Image questPopupIcon;
    public float popupDuration = 3f;
    
    [Header("Quest Log UI")]
    public GameObject questLogUI;
    public Transform questLogContent;
    public GameObject questLogEntryPrefab;
    public Button questLogToggleButton;
    public KeyCode questLogKey = KeyCode.J;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip questStartSound;
    public AudioClip questUpdateSound;
    public AudioClip questCompleteSound;
    
    [Header("Settings")]
    public bool autoSaveQuests = true;
    public bool showQuestNotifications = true;
    public bool persistBetweenScenes = true;
    
    // Events
    public System.Action<Quest> OnQuestStarted;
    public System.Action<Quest> OnQuestUpdated;
    public System.Action<Quest> OnQuestCompleted;
    
    private static QuestSystem instance;
    private bool isQuestLogOpen = false;
    private Coroutine popupCoroutine;
    
    // Singleton pattern for scene persistence
    public static QuestSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<QuestSystem>();
                if (instance == null)
                {
                    GameObject questSystemObj = new GameObject("QuestSystem");
                    instance = questSystemObj.AddComponent<QuestSystem>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (instance == null)
        {
            instance = this;
            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeQuestSystem();
    }
    
    void Start()
    {
        SetupUI();
        LoadQuests();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    private void InitializeQuestSystem()
    {
        // Find references if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Initialize lists
        if (activeQuests == null)
            activeQuests = new List<Quest>();
        if (completedQuests == null)
            completedQuests = new List<Quest>();
            
        Debug.Log("Quest System initialized");
    }
    
    private void SetupUI()
    {
        // Find UI elements if not assigned
        if (questPopup == null)
            questPopup = GameObject.Find("QuestPopup");
        if (questLogUI == null)
            questLogUI = GameObject.Find("QuestLogUI");
        if (questLogContent == null && questLogUI != null)
            questLogContent = questLogUI.transform.Find("Content");
        if (questLogToggleButton == null)
            questLogToggleButton = GameObject.Find("QuestLogButton")?.GetComponent<Button>();
            
        // Setup quest log button
        if (questLogToggleButton != null)
        {
            questLogToggleButton.onClick.AddListener(ToggleQuestLog);
        }
        
        // Hide UI initially
        if (questPopup != null)
            questPopup.SetActive(false);
        if (questLogUI != null)
            questLogUI.SetActive(false);
    }
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(questLogKey))
        {
            ToggleQuestLog();
        }
    }
    
    // Public methods for quest management
    public void StartQuest(Quest quest)
    {
        if (quest == null || activeQuests.Contains(quest) || completedQuests.Contains(quest))
            return;
            
        activeQuests.Add(quest);
        quest.isActive = true;
        quest.isCompleted = false;
        quest.currentProgress = 0;
        
        // Show quest popup
        if (showQuestNotifications)
            ShowQuestPopup(quest, "New Quest Started!");
            
        // Play sound
        if (audioSource != null && questStartSound != null)
            audioSource.PlayOneShot(questStartSound);
            
        // Trigger event
        OnQuestStarted?.Invoke(quest);
        
        // Update UI
        UpdateQuestLog();
        
        Debug.Log($"Quest started: {quest.questTitle}");
    }
    
    public void UpdateQuestProgress(Quest quest, int progress)
    {
        if (quest == null || !activeQuests.Contains(quest))
            return;
            
        quest.currentProgress = Mathf.Clamp(progress, 0, quest.requiredProgress);
        
        // Check if quest is completed
        if (quest.currentProgress >= quest.requiredProgress && !quest.isCompleted)
        {
            CompleteQuest(quest);
        }
        else
        {
            // Show progress update
            if (showQuestNotifications)
                ShowQuestPopup(quest, "Quest Updated!");
                
            // Play sound
            if (audioSource != null && questUpdateSound != null)
                audioSource.PlayOneShot(questUpdateSound);
                
            // Trigger event
            OnQuestUpdated?.Invoke(quest);
            
            // Update UI
            UpdateQuestLog();
        }
    }
    
    public void CompleteQuest(Quest quest)
    {
        if (quest == null || !activeQuests.Contains(quest))
            return;
            
        quest.isCompleted = true;
        quest.isActive = false;
        quest.currentProgress = quest.requiredProgress;
        
        // Move to completed list
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        
        // Show completion popup
        if (showQuestNotifications)
            ShowQuestPopup(quest, "Quest Completed!");
            
        // Play sound
        if (audioSource != null && questCompleteSound != null)
            audioSource.PlayOneShot(questCompleteSound);
            
        // Trigger event
        OnQuestCompleted?.Invoke(quest);
        
        // Update UI
        UpdateQuestLog();
        
        // Save quests
        if (autoSaveQuests)
            SaveQuests();
            
        Debug.Log($"Quest completed: {quest.questTitle}");
    }
    
    public void AbandonQuest(Quest quest)
    {
        if (quest == null || !activeQuests.Contains(quest))
            return;
            
        activeQuests.Remove(quest);
        quest.isActive = false;
        quest.currentProgress = 0;
        
        // Update UI
        UpdateQuestLog();
        
        Debug.Log($"Quest abandoned: {quest.questTitle}");
    }
    
    // UI Methods
    public void ShowQuestPopup(Quest quest, string message)
    {
        if (questPopup == null) return;
        
        // Stop existing popup
        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);
            
        // Setup popup content
        if (questPopupTitle != null)
            questPopupTitle.text = quest.questTitle;
        if (questPopupDescription != null)
            questPopupDescription.text = $"{message}\n{quest.questDescription}\nProgress: {quest.currentProgress}/{quest.requiredProgress}";
        if (questPopupIcon != null && quest.questIcon != null)
            questPopupIcon.sprite = quest.questIcon;
            
        // Show popup
        questPopup.SetActive(true);
        
        // Auto-hide after duration
        popupCoroutine = StartCoroutine(HideQuestPopupAfterDelay(popupDuration));
    }
    
    private System.Collections.IEnumerator HideQuestPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (questPopup != null)
            questPopup.SetActive(false);
    }
    
    public void ToggleQuestLog()
    {
        isQuestLogOpen = !isQuestLogOpen;
        
        if (questLogUI != null)
            questLogUI.SetActive(isQuestLogOpen);
            
        if (isQuestLogOpen)
            UpdateQuestLog();
    }
    
    private void UpdateQuestLog()
    {
        if (questLogContent == null) return;
        
        // Clear existing entries
        foreach (Transform child in questLogContent)
        {
            Destroy(child.gameObject);
        }
        
        // Add active quests
        foreach (Quest quest in activeQuests)
        {
            CreateQuestLogEntry(quest, false);
        }
        
        // Add completed quests
        foreach (Quest quest in completedQuests)
        {
            CreateQuestLogEntry(quest, true);
        }
    }
    
    private void CreateQuestLogEntry(Quest quest, bool isCompleted)
    {
        if (questLogEntryPrefab == null) return;
        
        GameObject entryObj = Instantiate(questLogEntryPrefab, questLogContent);
        QuestLogEntry entry = entryObj.GetComponent<QuestLogEntry>();
        
        if (entry != null)
        {
            entry.SetupQuestEntry(quest, isCompleted);
        }
    }
    
    // Save/Load methods
    private void SaveQuests()
    {
        // Save active quests
        for (int i = 0; i < activeQuests.Count; i++)
        {
            PlayerPrefs.SetString($"ActiveQuest_{i}", activeQuests[i].questID);
            PlayerPrefs.SetInt($"ActiveQuestProgress_{i}", activeQuests[i].currentProgress);
        }
        PlayerPrefs.SetInt("ActiveQuestCount", activeQuests.Count);
        
        // Save completed quests
        for (int i = 0; i < completedQuests.Count; i++)
        {
            PlayerPrefs.SetString($"CompletedQuest_{i}", completedQuests[i].questID);
        }
        PlayerPrefs.SetInt("CompletedQuestCount", completedQuests.Count);
        
        PlayerPrefs.Save();
        Debug.Log("Quests saved");
    }
    
    private void LoadQuests()
    {
        // Load active quests
        int activeCount = PlayerPrefs.GetInt("ActiveQuestCount", 0);
        for (int i = 0; i < activeCount; i++)
        {
            string questID = PlayerPrefs.GetString($"ActiveQuest_{i}", "");
            int progress = PlayerPrefs.GetInt($"ActiveQuestProgress_{i}", 0);
            
            // Find quest by ID (you'll need to implement this based on your quest loading system)
            Quest quest = FindQuestByID(questID);
            if (quest != null)
            {
                activeQuests.Add(quest);
                quest.isActive = true;
                quest.currentProgress = progress;
            }
        }
        
        // Load completed quests
        int completedCount = PlayerPrefs.GetInt("CompletedQuestCount", 0);
        for (int i = 0; i < completedCount; i++)
        {
            string questID = PlayerPrefs.GetString($"CompletedQuest_{i}", "");
            Quest quest = FindQuestByID(questID);
            if (quest != null)
            {
                completedQuests.Add(quest);
                quest.isCompleted = true;
                quest.isActive = false;
            }
        }
        
        UpdateQuestLog();
        Debug.Log("Quests loaded");
    }
    
    private Quest FindQuestByID(string questID)
    {
        // This method should be implemented based on how you store/load quest ScriptableObjects
        // You might want to use Resources.Load, or have a QuestDatabase
        return Resources.Load<Quest>($"Quests/{questID}");
    }
    
    // Public getters
    public List<Quest> GetActiveQuests() => activeQuests;
    public List<Quest> GetCompletedQuests() => completedQuests;
    public bool HasActiveQuests() => activeQuests.Count > 0;
    public bool HasCompletedQuests() => completedQuests.Count > 0;
    public int GetActiveQuestCount() => activeQuests.Count;
    public int GetCompletedQuestCount() => completedQuests.Count;
    
    // Scene management
    public void OnSceneLoaded()
    {
        SetupUI();
        UpdateQuestLog();
    }
    
    void OnDestroy()
    {
        if (autoSaveQuests)
            SaveQuests();
    }
}

[System.Serializable]
public class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questID;
    public string questTitle;
    public string questDescription;
    public Sprite questIcon;
    public QuestType questType;
    public QuestPriority priority = QuestPriority.Normal;
    
    [Header("Progress")]
    public int requiredProgress = 1;
    public int currentProgress = 0;
    
    [Header("Rewards")]
    public int experienceReward = 0;
    public string itemRewardID = "";
    
    [Header("State")]
    public bool isActive = false;
    public bool isCompleted = false;
    public float startTime = 0f;
    public float completionTime = 0f;
    
    [Header("Conditions")]
    public bool isRepeatable = false;
    public bool isHidden = false;
    public string[] prerequisiteQuestIDs;
}

public enum QuestType
{
    Collection,
    Elimination,
    Exploration,
    Interaction,
    Survival,
    Escort,
    Custom
}

public enum QuestPriority
{
    Low,
    Normal,
    High,
    Critical
}
