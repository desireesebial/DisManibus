using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestLogEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questProgressText;
    public Image questIconImage;
    public Image backgroundImage;
    
    [Header("Colors")]
    public Color activeQuestColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    public Color completedQuestColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    public Color normalTextColor = Color.white;
    public Color completedTextColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
    
    private Quest currentQuest;
    private bool isCompleted;
    
    public void SetupQuestEntry(Quest quest, bool completed)
    {
        currentQuest = quest;
        isCompleted = completed;
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (currentQuest == null) return;
        
        // Update title
        if (questTitleText != null)
        {
            questTitleText.text = currentQuest.questTitle;
            questTitleText.color = isCompleted ? completedTextColor : normalTextColor;
        }
        
        // Update description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = currentQuest.questDescription;
            questDescriptionText.color = isCompleted ? completedTextColor : normalTextColor;
        }
        
        // Update progress
        if (questProgressText != null)
        {
            if (isCompleted)
            {
                questProgressText.text = "COMPLETED";
                questProgressText.color = completedTextColor;
            }
            else
            {
                questProgressText.text = $"Progress: {currentQuest.currentProgress}/{currentQuest.requiredProgress}";
                questProgressText.color = normalTextColor;
            }
        }
        
        // Update icon
        if (questIconImage != null && currentQuest.questIcon != null)
        {
            questIconImage.sprite = currentQuest.questIcon;
            questIconImage.color = isCompleted ? completedTextColor : Color.white;
        }
        
        // Update background
        if (backgroundImage != null)
        {
            backgroundImage.color = isCompleted ? completedQuestColor : activeQuestColor;
        }
    }
    
    // Public method to refresh the entry (useful for dynamic updates)
    public void RefreshEntry()
    {
        UpdateUI();
    }
    
    // Public getters
    public Quest GetQuest()
    {
        return currentQuest;
    }
    
    public bool IsCompleted()
    {
        return isCompleted;
    }
}

