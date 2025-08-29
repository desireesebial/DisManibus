using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestLogEntry : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questProgressText;
    public Image questIcon;
    public Image backgroundImage;
    public Button questButton;
    
    [Header("Visual States")]
    public Color activeQuestColor = Color.white;
    public Color completedQuestColor = Color.green;
    public Color failedQuestColor = Color.red;
    
    [Header("Progress Bar")]
    public Image progressBar;
    public Color progressBarColor = Color.blue;
    
    private Quest quest;
    private bool isCompleted;
    
    void Start()
    {
        SetupQuestButton();
    }
    
    private void SetupQuestButton()
    {
        if (questButton != null)
        {
            questButton.onClick.AddListener(OnQuestClicked);
        }
    }
    
    public void SetupQuestEntry(Quest questData, bool completed)
    {
        quest = questData;
        isCompleted = completed;
        
        if (quest == null) return;
        
        // Setup title
        if (questTitleText != null)
        {
            questTitleText.text = quest.questTitle;
        }
        
        // Setup description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = quest.questDescription;
        }
        
        // Setup progress
        if (questProgressText != null)
        {
            if (completed)
            {
                questProgressText.text = "Completed!";
            }
            else
            {
                questProgressText.text = $"{quest.currentProgress}/{quest.requiredProgress}";
            }
        }
        
        // Setup icon
        if (questIcon != null && quest.questIcon != null)
        {
            questIcon.sprite = quest.questIcon;
        }
        
        // Setup progress bar
        if (progressBar != null)
        {
            float progress = quest.requiredProgress > 0 ? (float)quest.currentProgress / quest.requiredProgress : 0f;
            progressBar.fillAmount = completed ? 1f : progress;
            progressBar.color = progressBarColor;
        }
        
        // Setup visual state
        UpdateVisualState();
    }
    
    private void UpdateVisualState()
    {
        if (backgroundImage == null) return;
        
        if (isCompleted)
        {
            backgroundImage.color = completedQuestColor;
        }
        else
        {
            backgroundImage.color = activeQuestColor;
        }
    }
    
    public void UpdateProgress()
    {
        if (quest == null) return;
        
        // Update progress text
        if (questProgressText != null)
        {
            if (isCompleted)
            {
                questProgressText.text = "Completed!";
            }
            else
            {
                questProgressText.text = $"{quest.currentProgress}/{quest.requiredProgress}";
            }
        }
        
        // Update progress bar
        if (progressBar != null)
        {
            float progress = quest.requiredProgress > 0 ? (float)quest.currentProgress / quest.requiredProgress : 0f;
            progressBar.fillAmount = isCompleted ? 1f : progress;
        }
    }
    
    private void OnQuestClicked()
    {
        if (quest == null) return;
        
        // You can add quest details popup or other functionality here
        Debug.Log($"Quest clicked: {quest.questTitle}");
        
        // Example: Show quest details
        ShowQuestDetails();
    }
    
    private void ShowQuestDetails()
    {
        // This could open a detailed quest window
        // For now, just log the details
        Debug.Log($"Quest Details:");
        Debug.Log($"  Title: {quest.questTitle}");
        Debug.Log($"  Description: {quest.questDescription}");
        Debug.Log($"  Type: {quest.questType}");
        Debug.Log($"  Progress: {quest.currentProgress}/{quest.requiredProgress}");
        Debug.Log($"  Completed: {quest.isCompleted}");
        Debug.Log($"  Active: {quest.isActive}");
    }
    
    public Quest GetQuest()
    {
        return quest;
    }
    
    public bool IsCompleted()
    {
        return isCompleted;
    }
    
    public void SetCompleted(bool completed)
    {
        isCompleted = completed;
        UpdateVisualState();
    }
}
