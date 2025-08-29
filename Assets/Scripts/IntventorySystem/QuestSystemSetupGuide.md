# 🎯 **Quest System Setup Guide - Reusable Across Scenes**

## ✅ **What Makes It Reusable**

- **Singleton Pattern**: Automatically persists between scenes
- **DontDestroyOnLoad**: Quest progress saved across level transitions
- **Auto-Setup**: Finds UI elements automatically
- **Modular Design**: Each quest is a separate ScriptableObject
- **Event-Driven**: Loose coupling with other systems

## 🚀 **Quick Setup for New Scenes**

### **Step 1: Create Quest System (One Time Only)**

The Quest System uses a **Singleton pattern**, so you only need to create it once in your first scene:

```csharp
// In your first scene (e.g., Main Menu or Level 1)
// The QuestSystem will automatically persist to all other scenes
```

### **Step 2: Create Quest ScriptableObjects**

1. **Right-click in Project** → **Create** → **ScriptableObject** → **Quest**
2. **Fill in quest details**:
   - `questID`: Unique identifier (e.g., "find_dullahan_head")
   - `questTitle`: Display name (e.g., "Find Dullahan's Head")
   - `questDescription`: Quest details
   - `requiredProgress`: Number of steps needed
   - `questType`: Type of quest (Collection, Kill, Explore, etc.)
   - `questIcon`: Optional icon sprite

### **Step 3: Create UI Elements (Per Scene)**

Each scene needs its own UI elements, but the Quest System will find them automatically:

#### **Quest Popup UI:**
```
Canvas
└── QuestPopup (GameObject)
    ├── Background (Image)
    ├── QuestPopupTitle (TextMeshProUGUI)
    ├── QuestPopupDescription (TextMeshProUGUI)
    └── QuestPopupIcon (Image)
```

#### **Quest Log UI:**
```
Canvas
└── QuestLogUI (GameObject)
    ├── Background (Image)
    ├── QuestLogButton (Button)
    └── Content (Transform)
        └── QuestLogEntryPrefab (Prefab)
```

### **Step 4: Use in Your Scripts**

```csharp
// Start a quest
QuestSystem.Instance.StartQuest(myQuest);

// Update quest progress
QuestSystem.Instance.UpdateQuestProgress(myQuest, 2);

// Complete a quest
QuestSystem.Instance.CompleteQuest(myQuest);

// Check quest status
bool hasActiveQuests = QuestSystem.Instance.HasActiveQuests();
List<Quest> activeQuests = QuestSystem.Instance.GetActiveQuests();
```

## 📋 **Complete Example Implementation**

### **Scene 1: Main Menu**
```csharp
public class MainMenuManager : MonoBehaviour
{
    [Header("Quests")]
    public Quest tutorialQuest;
    public Quest mainQuest;
    
    void Start()
    {
        // Start tutorial quest
        QuestSystem.Instance.StartQuest(tutorialQuest);
    }
    
    public void StartGame()
    {
        // Quest progress persists when loading new scene
        SceneManager.LoadScene("Level1");
    }
}
```

### **Scene 2: Level 1**
```csharp
public class Level1Manager : MonoBehaviour
{
    [Header("Quests")]
    public Quest findKeyQuest;
    public Quest defeatEnemyQuest;
    
    void Start()
    {
        // Check if tutorial was completed
        if (QuestSystem.Instance.GetCompletedQuests().Contains(tutorialQuest))
        {
            // Start level-specific quests
            QuestSystem.Instance.StartQuest(findKeyQuest);
        }
    }
    
    public void OnKeyFound()
    {
        // Update quest progress
        QuestSystem.Instance.UpdateQuestProgress(findKeyQuest, 1);
    }
    
    public void OnEnemyDefeated()
    {
        // Complete quest
        QuestSystem.Instance.CompleteQuest(defeatEnemyQuest);
    }
}
```

### **Scene 3: Level 2**
```csharp
public class Level2Manager : MonoBehaviour
{
    void Start()
    {
        // All quests from previous scenes are still active/completed
        // You can check their status and continue them
        List<Quest> activeQuests = QuestSystem.Instance.GetActiveQuests();
        
        foreach (Quest quest in activeQuests)
        {
            Debug.Log($"Active quest: {quest.questTitle}");
        }
    }
}
```

## 🎮 **Quest Types and Examples**

### **Collection Quest**
```csharp
public class ItemCollector : MonoBehaviour
{
    public Quest collectionQuest;
    public int itemsCollected = 0;
    
    public void CollectItem()
    {
        itemsCollected++;
        QuestSystem.Instance.UpdateQuestProgress(collectionQuest, itemsCollected);
        
        if (itemsCollected >= collectionQuest.requiredProgress)
        {
            QuestSystem.Instance.CompleteQuest(collectionQuest);
        }
    }
}
```

### **Kill Quest**
```csharp
public class EnemyManager : MonoBehaviour
{
    public Quest killQuest;
    public int enemiesKilled = 0;
    
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        QuestSystem.Instance.UpdateQuestProgress(killQuest, enemiesKilled);
    }
}
```

### **Exploration Quest**
```csharp
public class CheckpointTrigger : MonoBehaviour
{
    public Quest explorationQuest;
    public int checkpointIndex;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestSystem.Instance.UpdateQuestProgress(explorationQuest, checkpointIndex + 1);
        }
    }
}
```

## 🔧 **Advanced Features**

### **Quest Dependencies**
```csharp
public class QuestManager : MonoBehaviour
{
    public Quest prerequisiteQuest;
    public Quest dependentQuest;
    
    public void StartDependentQuest()
    {
        // Only start if prerequisite is completed
        if (QuestSystem.Instance.GetCompletedQuests().Contains(prerequisiteQuest))
        {
            QuestSystem.Instance.StartQuest(dependentQuest);
        }
    }
}
```

### **Quest Rewards**
```csharp
public class QuestRewardManager : MonoBehaviour
{
    public void OnQuestCompleted(Quest quest)
    {
        switch (quest.questID)
        {
            case "find_dullahan_head":
                GivePlayerLantern();
                break;
            case "defeat_boss":
                UnlockNextLevel();
                break;
        }
    }
    
    private void GivePlayerLantern()
    {
        // Give player lantern item
        FindObjectOfType<DullahanHeadInventory>().GiveLantern();
    }
}
```

### **Quest Events**
```csharp
public class QuestEventHandler : MonoBehaviour
{
    void Start()
    {
        // Subscribe to quest events
        QuestSystem.Instance.OnQuestStarted += OnQuestStarted;
        QuestSystem.Instance.OnQuestCompleted += OnQuestCompleted;
    }
    
    void OnQuestStarted(Quest quest)
    {
        Debug.Log($"Quest started: {quest.questTitle}");
        // Play quest start sound, show UI, etc.
    }
    
    void OnQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest completed: {quest.questTitle}");
        // Give rewards, unlock achievements, etc.
    }
}
```

## 🎨 **UI Customization**

### **Custom Quest Popup**
```csharp
public class CustomQuestUI : MonoBehaviour
{
    public void ShowCustomQuestPopup(Quest quest)
    {
        // Your custom popup logic
        QuestSystem.Instance.ShowQuestPopup(quest, "Custom Message!");
    }
}
```

### **Quest Log Integration**
```csharp
public class QuestLogManager : MonoBehaviour
{
    void Update()
    {
        // Toggle quest log with custom key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            QuestSystem.Instance.ToggleQuestLog();
        }
    }
}
```

## 💾 **Save/Load System**

The Quest System automatically saves quest progress using `PlayerPrefs`. For more advanced save systems:

```csharp
public class AdvancedQuestSave : MonoBehaviour
{
    public void SaveQuestData()
    {
        // Get quest data
        List<Quest> activeQuests = QuestSystem.Instance.GetActiveQuests();
        List<Quest> completedQuests = QuestSystem.Instance.GetCompletedQuests();
        
        // Save to your custom save system
        SaveSystem.SaveQuests(activeQuests, completedQuests);
    }
    
    public void LoadQuestData()
    {
        // Load from your custom save system
        var questData = SaveSystem.LoadQuests();
        
        // Restore quest state
        foreach (var quest in questData.activeQuests)
        {
            QuestSystem.Instance.StartQuest(quest);
        }
    }
}
```

## 🐛 **Troubleshooting**

### **Quest System Not Found**
```csharp
// The Quest System will auto-create if not found
QuestSystem questSystem = QuestSystem.Instance;
```

### **UI Elements Not Found**
- Ensure UI elements have the correct names
- The system will log warnings if elements are missing
- UI elements are optional - the system works without them

### **Quest Progress Not Saving**
- Check if `autoSaveQuests` is enabled
- Ensure `persistBetweenScenes` is true
- Verify quest IDs are unique

## 🎯 **Best Practices**

1. **Use Unique Quest IDs**: Avoid conflicts between scenes
2. **Test Quest Flow**: Ensure quests work across scene transitions
3. **Handle Quest Dependencies**: Check prerequisites before starting quests
4. **Use Events**: Subscribe to quest events for game logic
5. **Save Progress**: The system auto-saves, but you can add custom save logic

## 🚀 **Ready to Use!**

The Quest System is now fully reusable across all your scenes! Just create your Quest ScriptableObjects and start using `QuestSystem.Instance` in your scripts. The system will handle everything else automatically! 🎮✨
