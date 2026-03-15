using System;
using System.Collections.Generic;

/// <summary>
/// Data model for a single quest.
/// Quests have objectives that must all be completed to finish the quest.
/// </summary>
[Serializable]
public class Quest
{
    public enum QuestType { Main, Side }
    public enum QuestState { Locked, Active, Complete }

    public string id;
    public string title;
    public string description;
    public QuestType questType;
    public QuestState state = QuestState.Locked;

    /// <summary>ID of the quest that must be complete before this one can activate</summary>
    public string prerequisiteQuestId;

    /// <summary>Objectives that must all be completed</summary>
    public List<QuestObjective> objectives = new List<QuestObjective>();

    /// <summary>Dialogue lines shown when the quest is given</summary>
    public List<DialogueLine> startDialogue = new List<DialogueLine>();

    /// <summary>Dialogue lines shown when the quest is turned in</summary>
    public List<DialogueLine> completeDialogue = new List<DialogueLine>();

    public bool IsComplete()
    {
        foreach (var obj in objectives)
            if (!obj.isComplete) return false;
        return objectives.Count > 0;
    }

    public QuestObjective GetCurrentObjective()
    {
        foreach (var obj in objectives)
            if (!obj.isComplete) return obj;
        return null;
    }
}

[Serializable]
public class QuestObjective
{
    public enum ObjectiveType
    {
        TravelTo,       // reach a location
        DefeatEnemies,  // kill N enemies
        TalkTo,         // arrive at location + auto-dialogue
        Survive,        // survive N seconds
        CollectItems    // gather N of a resource
    }

    public ObjectiveType type;
    public string description;          // shown in tracker UI
    public string targetLocationId;     // for TravelTo / TalkTo
    public int requiredCount;           // for DefeatEnemies / CollectItems / Survive (seconds)
    public int currentCount;
    public bool isComplete;

    public void AddProgress(int amount = 1)
    {
        if (isComplete) return;
        currentCount += amount;
        if (currentCount >= requiredCount)
        {
            currentCount = requiredCount;
            isComplete = true;
        }
    }
}

[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;

    public DialogueLine() { }
    public DialogueLine(string speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
    }
}
