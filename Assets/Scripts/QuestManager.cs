using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Central quest manager — Davy Jones quest chain.
/// You are Davy Jones, cursed to die and be reborn. Break the loop.
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

    // ─── QUEST CHAIN: THE CURSE OF DAVY JONES ──

    void BuildQuestChain()
    {
        // Q0 — The Awakening: impossible wave → die → rebirth dialogue
        var q0 = new Quest
        {
            id = "the_awakening",
            title = "The Awakening",
            description = "Something stirs in the deep...",
            questType = Quest.QuestType.Main,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.Survive,
                    description = "Survive the onslaught",
                    requiredCount = 1 // completed by first death
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("???", "Captain... can you hear me?"),
                new DialogueLine("???", "The sea is angry. Monsters are everywhere."),
                new DialogueLine("???", "Fight them. Or try to.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("???", "...You died."),
                new DialogueLine("???", "But you're still here, aren't you?"),
                new DialogueLine("Voice of the Deep", "You are Davy Jones."),
                new DialogueLine("Voice of the Deep", "Cursed to sail these waters. Cursed to die. Cursed to return."),
                new DialogueLine("Voice of the Deep", "Again and again, the sea spits you back."),
                new DialogueLine("Voice of the Deep", "But there is a way to break the cycle..."),
                new DialogueLine("Voice of the Deep", "Find the ones who can help you. Build your crew."),
                new DialogueLine("Voice of the Deep", "And face what waits in The Maelstrom."),
                new DialogueLine("Voice of the Deep", "Only then will Davy Jones be free.")
            }
        };

        // Q1 — Set Sail: sail to Trader's Cove
        var q1 = new Quest
        {
            id = "set_sail",
            title = "Set Sail",
            description = "Navigate to Trader's Cove. Someone there might know more about the curse.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "the_awakening",
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
                new DialogueLine("Voice of the Deep", "Trader's Cove lies to the northwest. The harbormaster there knows things."),
                new DialogueLine("Voice of the Deep", "Follow the compass. Die as many times as you must — you'll always come back.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Harbormaster", "By the gods... Davy Jones himself, in my port."),
                new DialogueLine("Harbormaster", "I've heard the legends. You're trapped in this cursed loop, aren't you?"),
                new DialogueLine("Harbormaster", "Our gunsmith was taken by the sea beasts. Free him — he can arm your ship properly."),
                new DialogueLine("Harbormaster", "Maybe with a real crew, you can reach The Maelstrom and end this.")
            }
        };

        // Q2 — Rescue the Gunsmith: defeat enemies
        var q2 = new Quest
        {
            id = "rescue_gunsmith",
            title = "Rescue the Gunsmith",
            description = "The gunsmith is held captive by sea creatures. Defeat them to free him.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "set_sail",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.DefeatEnemies,
                    description = "Defeat sea creatures (0/10)",
                    requiredCount = 10
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Harbormaster", "The creatures dragged the gunsmith east. He's still alive — I can feel it."),
                new DialogueLine("Harbormaster", "You can die trying. Literally. But you'll be back, won't you?")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Gunsmith", "I thought I was done for! You're... wait. Davy Jones?"),
                new DialogueLine("Gunsmith", "The cursed captain himself saved me? I must be dreaming."),
                new DialogueLine("Gunsmith", "Dream or not — my cannons are yours. I'll upgrade your weapons."),
                new DialogueLine("Gunsmith", "My apprentice, the Gunner, headed to the Naval Outpost. He'd be useful to you.")
            }
        };

        // Q3 — The Pilot: recruit helmsman → speed boost
        var q3 = new Quest
        {
            id = "the_pilot",
            title = "The Pilot",
            description = "Find a skilled helmsman at the Naval Outpost to navigate the cursed waters.",
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
                new DialogueLine("Gunsmith", "There's a helmsman at the Naval Outpost — fastest sailor I've ever seen."),
                new DialogueLine("Gunsmith", "If anyone can navigate through The Maelstrom, it's him.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Helmsman", "Davy Jones? I thought you were a myth."),
                new DialogueLine("Helmsman", "A cursed captain trying to break free? That's the most exciting thing I've heard in years."),
                new DialogueLine("Helmsman", "I'm your pilot now. You'll feel the ship respond better already.")
            }
        };

        // Q4 — The Gunner: defeat enemies → fire rate boost
        var q4 = new Quest
        {
            id = "the_gunner",
            title = "The Gunner",
            description = "The gunsmith's apprentice is stranded in dangerous waters. Clear the beasts.",
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
                new DialogueLine("Helmsman", "Captain, something's wrong in the waters nearby. Screams."),
                new DialogueLine("Helmsman", "Could be the Gunner the smithy mentioned. Let's clear them out!")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Gunner", "You saved me! The sea beasts had me cornered."),
                new DialogueLine("Gunner", "Wait — Davy Jones? THE Davy Jones?"),
                new DialogueLine("Gunner", "I don't care if you're cursed. You saved my life. My cannons are yours, captain."),
                new DialogueLine("Gunner", "I'll make sure we fire faster and hit harder.")
            }
        };

        // Q5 — Break the Loop: boss fight
        var q5 = new Quest
        {
            id = "break_the_loop",
            title = "Break the Loop",
            description = "Your crew is assembled. Sail to The Maelstrom and confront your destiny.",
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
                    description = "Defeat the Kraken (0/1)",
                    requiredCount = 1
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Voice of the Deep", "You've built a crew. You've died and returned more times than any mortal could."),
                new DialogueLine("Voice of the Deep", "The Maelstrom awaits. The source of your curse dwells within."),
                new DialogueLine("Helmsman", "We're with you, captain. To the end."),
                new DialogueLine("Gunner", "All cannons loaded. Let's break this curse."),
                new DialogueLine("Gunsmith", "Give 'em hell, Jones.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Voice of the Deep", "...It's done."),
                new DialogueLine("Voice of the Deep", "The loop is broken. The curse lifts."),
                new DialogueLine("Davy Jones", "I'm... free?"),
                new DialogueLine("Helmsman", "Captain! Look — the sea is calm!"),
                new DialogueLine("Gunner", "We did it. We actually did it!"),
                new DialogueLine("Voice of the Deep", "The seas will remember Davy Jones. Not as a curse — but as a legend."),
                new DialogueLine("Voice of the Deep", "Sail on, captain. The ocean is finally yours.")
            }
        };

        allQuests.Add(q0);
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

    /// <summary>
    /// Called by GameManager.OnPlayerDeath when the player dies and respawns.
    /// Completes the Awakening quest's Survive objective on first death.
    /// Returns true if it triggered dialogue (caller should NOT show its own).
    /// </summary>
    public bool ReportPlayerDeath(int deathCount, System.Action onAllDialogueFinished = null)
    {
        if (activeQuest == null)
            return false;

        // The Awakening quest completes when the player dies for the first time
        if (activeQuest.id == "the_awakening")
        {
            var obj = activeQuest.GetCurrentObjective();
            if (obj != null && !obj.isComplete && obj.type == QuestObjective.ObjectiveType.Survive)
            {
                obj.AddProgress(1);
                OnObjectiveProgress?.Invoke(activeQuest, obj);
                CheckQuestCompletion(onAllDialogueFinished);
                return true; // dialogue will be shown by CheckQuestCompletion
            }
        }
        return false;
    }

    // ─── INTERNAL ───────────────────────────────

    /// <param name="onSequenceComplete">Optional callback fired after ALL chained dialogues finish.</param>
    void CheckQuestCompletion(System.Action onSequenceComplete = null)
    {
        if (activeQuest == null || !activeQuest.IsComplete())
        {
            onSequenceComplete?.Invoke();
            return;
        }

        activeQuest.state = Quest.QuestState.Complete;
        Debug.Log($"Quest complete: {activeQuest.title}");

        // Apply rewards and reveal new locations
        ApplyQuestRewards(activeQuest);
        DiscoverQuestLocations(activeQuest);

        // Show completion dialogue
        DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI != null && activeQuest.completeDialogue.Count > 0)
        {
            Quest completed = activeQuest;
            dialogueUI.ShowDialogue(completed.completeDialogue, () =>
            {
                OnQuestCompleted?.Invoke(completed);
                ActivateNextQuest(onSequenceComplete);
            });
        }
        else
        {
            OnQuestCompleted?.Invoke(activeQuest);
            ActivateNextQuest(onSequenceComplete);
        }
    }

    void ActivateNextQuest(System.Action onSequenceComplete = null)
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

            // Show start dialogue, firing the sequence callback when done
            DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
            if (dialogueUI != null && q.startDialogue.Count > 0)
                dialogueUI.ShowDialogue(q.startDialogue, () => onSequenceComplete?.Invoke());
            else
                onSequenceComplete?.Invoke();

            return;
        }

        // No next quest found — still fire the callback
        onSequenceComplete?.Invoke();
    }

    /// <summary>
    /// Grants gameplay rewards when a quest is completed.
    /// </summary>
    void ApplyQuestRewards(Quest q)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        switch (q.id)
        {
            case "the_awakening":
                gm.hasAwakened = true;
                Debug.Log("Reward: Davy Jones has awakened.");
                break;
            case "rescue_gunsmith":
                // Unlock cannon upgrades (damage boost)
                gm.damageMultiplier += 0.5f;
                Debug.Log("Reward: Cannon upgrade — damage +50%.");
                break;
            case "the_pilot":
                // Speed boost from helmsman
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                    player.moveSpeed *= 1.5f;
                Debug.Log("Reward: Helmsman recruited — speed +50%.");
                break;
            case "the_gunner":
                // Fire rate boost
                var cannon = FindFirstObjectByType<CannonController>();
                if (cannon != null)
                    cannon.fireRate *= 0.7f;
                Debug.Log("Reward: Gunner recruited — fire rate improved.");
                break;
            case "break_the_loop":
                Debug.Log("The curse is broken. Davy Jones is free.");
                break;
        }
    }

    /// <summary>
    /// Reveals locations on the compass/minimap when quests are completed.
    /// </summary>
    void DiscoverQuestLocations(Quest completedQuest)
    {
        var locMgr = LocationManager.Instance;
        if (locMgr == null) return;

        switch (completedQuest.id)
        {
            case "the_awakening":
                locMgr.DiscoverLocation("traders_cove");
                break;
            case "rescue_gunsmith":
                locMgr.DiscoverLocation("naval_outpost");
                break;
            case "the_gunner":
                locMgr.DiscoverLocation("boss_arena");
                break;
        }
    }
}
