using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Central quest manager — owns the quest chain, tracks progress, fires events.
/// Other systems (PortZone, enemy death, etc.) call into this to report progress.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    /// <summary>Fired when a quest becomes active</summary>
    public event Action<Quest> OnQuestActivated;
    /// <summary>Fired when an objective makes progress</summary>
    public event Action<Quest, QuestObjective> OnObjectiveProgress;
    /// <summary>Fired when a quest is completed</summary>
    public event Action<Quest> OnQuestCompleted;

    private readonly List<Quest> allQuests = new List<Quest>();
    private Quest activeQuest;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        BuildQuestChain();
    }

    void Start()
    {
        // Auto-activate the first quest
        ActivateNextQuest();
    }

    // ─── QUEST CHAIN DEFINITION ─────────────────

    void BuildQuestChain()
    {
        // Q1 — Set Sail (tutorial: sail to Trader's Cove)
        var q1 = new Quest
        {
            id = "set_sail",
            title = "Set Sail",
            description = "Navigate to Trader's Cove to find the harbormaster.",
            questType = Quest.QuestType.Main,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.TravelTo,
                    description = "Sail to Trader's Cove",
                    targetLocationId = "traders_cove",
                    requiredCount = 1
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Old Sailor", "The seas aren't safe anymore, captain."),
                new DialogueLine("Old Sailor", "Head to Trader's Cove — the harbormaster there may know what's going on."),
                new DialogueLine("Old Sailor", "Follow the compass, and watch for monsters!")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Harbormaster", "You made it! We've been expecting someone brave enough to help."),
                new DialogueLine("Harbormaster", "Our gunsmith was captured by pirates east of here."),
                new DialogueLine("Harbormaster", "Without him, we can't arm our ships. Will you rescue him?")
            }
        };

        // Q2 — Rescue the Gunsmith (defeat enemy wave)
        var q2 = new Quest
        {
            id = "rescue_gunsmith",
            title = "Rescue the Gunsmith",
            description = "Defeat the pirates holding the gunsmith prisoner.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "set_sail",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.DefeatEnemies,
                    description = "Defeat pirates (0/10)",
                    requiredCount = 10
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Harbormaster", "The pirates took the gunsmith to the waters east of here."),
                new DialogueLine("Harbormaster", "Defeat them and bring him back!")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Gunsmith", "Freedom at last! Thank you, captain!"),
                new DialogueLine("Gunsmith", "I'll set up shop at the Cove. Come see me for cannon upgrades."),
                new DialogueLine("Gunsmith", "Oh — my apprentice, the Gunner, went to the Naval Outpost. You should find him.")
            }
        };

        // Q3 — The Pilot (travel to Naval Outpost, recruit helmsman)
        var q3 = new Quest
        {
            id = "the_pilot",
            title = "The Pilot",
            description = "Sail to the Naval Outpost to recruit a skilled helmsman.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "rescue_gunsmith",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.TravelTo,
                    description = "Sail to the Naval Outpost",
                    targetLocationId = "naval_outpost",
                    requiredCount = 1
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Gunsmith", "The Naval Outpost to the south has a pilot — best helmsman on the seas."),
                new DialogueLine("Gunsmith", "Recruit him and your ship will handle like a dream.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Helmsman", "A captain in need? Say no more — I've been itching for adventure."),
                new DialogueLine("Helmsman", "I'll be your pilot. You'll feel the difference immediately.")
            }
        };

        // Q4 — The Gunner (defeat enemies near outpost, rescue apprentice)
        var q4 = new Quest
        {
            id = "the_gunner",
            title = "The Gunner",
            description = "Find the gunsmith's apprentice and rescue him from danger.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "the_pilot",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.DefeatEnemies,
                    description = "Clear the waters (0/15)",
                    requiredCount = 15
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Helmsman", "I heard strange sounds coming from the waters nearby."),
                new DialogueLine("Helmsman", "Could be the Gunner the smithy mentioned — let's clear the beasts!")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Gunner", "Captain! The gunsmith sent you? I owe you my life."),
                new DialogueLine("Gunner", "My cannons don't miss. Let me join your crew.")
            }
        };

        // Q5 — The Final Threat (sail to boss arena, defeat boss)
        var q5 = new Quest
        {
            id = "final_threat",
            title = "The Final Threat",
            description = "All crew assembled. Confront the menace in The Maelstrom.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "the_gunner",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.TravelTo,
                    description = "Enter The Maelstrom",
                    targetLocationId = "boss_arena",
                    requiredCount = 1
                },
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.DefeatEnemies,
                    description = "Defeat the Sea Serpent (0/1)",
                    requiredCount = 1
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Helmsman", "Captain, we're as strong as we'll ever be."),
                new DialogueLine("Gunner", "All cannons loaded and ready, sir."),
                new DialogueLine("Old Sailor", "The Maelstrom lies to the north. End this threat once and for all.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Helmsman", "We did it, captain! The seas are safe again!"),
                new DialogueLine("Gunner", "What a fight! The old man would be proud."),
                new DialogueLine("Old Sailor", "A legend is born. Well done, captain.")
            }
        };

        allQuests.Add(q1);
        allQuests.Add(q2);
        allQuests.Add(q3);
        allQuests.Add(q4);
        allQuests.Add(q5);
    }

    // ─── PUBLIC API ─────────────────────────────

    public Quest GetActiveQuest() => activeQuest;

    public Quest GetQuest(string id)
    {
        foreach (var q in allQuests)
            if (q.id == id) return q;
        return null;
    }

    /// <summary>
    /// Called by PortZone / LocationManager when player reaches a location.
    /// </summary>
    public void ReportLocationReached(string locationId)
    {
        if (activeQuest == null) return;
        var obj = activeQuest.GetCurrentObjective();
        if (obj == null || obj.isComplete) return;
        if (obj.type == QuestObjective.ObjectiveType.TravelTo && obj.targetLocationId == locationId)
        {
            obj.AddProgress(1);
            OnObjectiveProgress?.Invoke(activeQuest, obj);
            CheckQuestCompletion();
        }
    }

    /// <summary>
    /// Called by enemy death logic when an enemy is killed.
    /// </summary>
    public void ReportEnemyKilled()
    {
        if (activeQuest == null) return;
        var obj = activeQuest.GetCurrentObjective();
        if (obj == null || obj.isComplete) return;
        if (obj.type == QuestObjective.ObjectiveType.DefeatEnemies)
        {
            obj.AddProgress(1);
            obj.description = $"Defeat enemies ({obj.currentCount}/{obj.requiredCount})";
            OnObjectiveProgress?.Invoke(activeQuest, obj);
            CheckQuestCompletion();
        }
    }

    // ─── INTERNAL ───────────────────────────────

    void CheckQuestCompletion()
    {
        if (activeQuest == null || !activeQuest.IsComplete()) return;

        activeQuest.state = Quest.QuestState.Complete;
        Debug.Log($"Quest complete: {activeQuest.title}");

        // Show completion dialogue
        DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI != null && activeQuest.completeDialogue.Count > 0)
        {
            Quest completed = activeQuest;
            dialogueUI.ShowDialogue(completed.completeDialogue, () =>
            {
                OnQuestCompleted?.Invoke(completed);
                ActivateNextQuest();
            });
        }
        else
        {
            OnQuestCompleted?.Invoke(activeQuest);
            ActivateNextQuest();
        }
    }

    void ActivateNextQuest()
    {
        activeQuest = null;

        foreach (var q in allQuests)
        {
            if (q.state != Quest.QuestState.Locked) continue;

            // Check prerequisite
            if (!string.IsNullOrEmpty(q.prerequisiteQuestId))
            {
                Quest prereq = GetQuest(q.prerequisiteQuestId);
                if (prereq == null || prereq.state != Quest.QuestState.Complete)
                    continue;
            }

            // Activate this quest
            q.state = Quest.QuestState.Active;
            activeQuest = q;
            Debug.Log($"Quest activated: {q.title}");

            OnQuestActivated?.Invoke(q);

            // Show start dialogue
            DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
            if (dialogueUI != null && q.startDialogue.Count > 0)
                dialogueUI.ShowDialogue(q.startDialogue, null);

            break;
        }
    }
}
