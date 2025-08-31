using UnityEngine;

[CreateAssetMenu(fileName = "HelpDullahanQuest", menuName = "Scriptable Objects/Quests/Help Dullahan Quest")]
public class HelpDullahanQuest : Quest
{
    void Awake()
    {
        questID = "help_dullahan_quest";
        questTitle = "Help Dullahan";
        questDescription = "Find Dullahan's real head and attach it to his body to help him find peace.";
        questType = QuestType.Collection;
        priority = QuestPriority.High;
        requiredProgress = 1;
        experienceReward = 100;
        isRepeatable = false;
        isHidden = false;
    }
}

[CreateAssetMenu(fileName = "EscapeQuest", menuName = "Scriptable Objects/Quests/Escape Quest")]
public class EscapeQuest : Quest
{
    void Awake()
    {
        questID = "escape_quest";
        questTitle = "Escape to Floor 1";
        questDescription = "Leave Floor 2 through the exit door to continue your journey.";
        questType = QuestType.Exploration;
        priority = QuestPriority.Normal;
        requiredProgress = 1;
        experienceReward = 50;
        isRepeatable = false;
        isHidden = false;
    }
}

[CreateAssetMenu(fileName = "FindRealHeadQuest", menuName = "Scriptable Objects/Quests/Find Real Head Quest")]
public class FindRealHeadQuest : Quest
{
    void Awake()
    {
        questID = "find_real_head_quest";
        questTitle = "Find the Real Head";
        questDescription = "Search for Dullahan's real head among the three heads scattered in the area.";
        questType = QuestType.Collection;
        priority = QuestPriority.High;
        requiredProgress = 1;
        experienceReward = 75;
        isRepeatable = false;
        isHidden = false;
    }
}

[CreateAssetMenu(fileName = "AttachHeadQuest", menuName = "Scriptable Objects/Quests/Attach Head Quest")]
public class AttachHeadQuest : Quest
{
    void Awake()
    {
        questID = "attach_head_quest";
        questTitle = "Attach the Real Head";
        questDescription = "Bring the real head to Dullahan's body and attach it to complete the ritual.";
        questType = QuestType.Interaction;
        priority = QuestPriority.Critical;
        requiredProgress = 1;
        experienceReward = 150;
        isRepeatable = false;
        isHidden = false;
    }
}
