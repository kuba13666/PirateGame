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
                new DialogueLine("Harbormaster", "Then hear this: two terrors haunt these waters. A green ghost-light in the northern fog that lures ships to their doom..."),
                new DialogueLine("Harbormaster", "...and a white island that appears on no chart and moves between sightings. They say a man still lives inside it."),
                new DialogueLine("Harbormaster", "Only with legends at your side will you survive The Maelstrom. But first — our gunsmith. The beasts wrecked his ship east of here. Free him, and he'll arm you properly.")
            }
        };

        // Q2 — Rescue the Gunsmith: find his wreck in the Hunting Grounds, clear the beasts
        var q2 = new Quest
        {
            id = "rescue_gunsmith",
            title = "Rescue the Gunsmith",
            description = "The gunsmith's ship was wrecked in the Hunting Grounds, east of Safe Harbor. Find the wreck and drive off the beasts.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "set_sail",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.TravelTo,
                    description = "Find the Gunsmith's wreck (east)",
                    targetLocationId = "gunsmith_wreck",
                    requiredCount = 1
                },
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.DefeatEnemies,
                    description = "Clear the beasts around the wreck (0/10)",
                    requiredCount = 10
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Harbormaster", "His ship ran aground east of here, in the Hunting Grounds. He's still alive — I can feel it."),
                new DialogueLine("Harbormaster", "You can die trying. Literally. But you'll be back, won't you?")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Gunsmith", "I thought I was done for! You're... wait. Davy Jones?"),
                new DialogueLine("Gunsmith", "The cursed captain himself saved me? I must be dreaming."),
                new DialogueLine("Gunsmith", "Dream or not — my cannons are yours. I'll upgrade your weapons."),
                new DialogueLine("Gunsmith", "One thing, Jones... my old friend Israel Hands vanished hunting that white island. The finest gunner who ever lived, swallowed by the sea."),
                new DialogueLine("Gunsmith", "And if you mean to enter The Maelstrom, find yourself a pilot first. No mortal hand can hold a wheel through that storm.")
            }
        };

        // Q3 — The Ghost Light: the Flying Dutchman miniboss → the legendary PILOT
        var q3 = new Quest
        {
            id = "ghost_light",
            title = "The Ghost Light",
            description = "A green light haunts the fog north-west, luring ships to their doom. Investigate Dutchman's Drift.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "rescue_gunsmith",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.TravelTo,
                    description = "Sail into Dutchman's Drift (north-west)",
                    targetLocationId = "dutchmans_drift",
                    requiredCount = 1
                },
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.DefeatBoss,
                    description = "Defeat the Flying Dutchman",
                    targetBossId = "flying_dutchman",
                    targetLocationId = "dutchmans_drift",
                    requiredCount = 1
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Harbormaster", "The ghost-light took another ship last night. Whatever burns green out there, it's no lighthouse."),
                new DialogueLine("Voice of the Deep", "A cursed soul, like you. Van der Decken swore to sail until doomsday — and the sea holds him to it."),
                new DialogueLine("Voice of the Deep", "The Dutchman cannot stop, Jones. Make him.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Van der Decken", "Three hundred years... and the sea finally lets me drop anchor. You did this, Jones?"),
                new DialogueLine("Van der Decken", "Then my wheel is yours. If breaking YOUR curse breaks mine, I'll steer you through hell itself."),
                new DialogueLine("Van der Decken", "And captain — the Maelstrom IS hell. No hand but mine can hold a wheel through it.")
            }
        };

        // Q4 — The White Island: Mocha Dick miniboss → the legendary GUNNER (Israel Hands)
        var q4 = new Quest
        {
            id = "white_island_hunt",
            title = "The White Island",
            description = "An island that appears on no chart drifts near the Forgotten Isle. Israel Hands vanished hunting it.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "ghost_light",
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.TravelTo,
                    description = "Find the White Island (south-east)",
                    targetLocationId = "white_island",
                    requiredCount = 1
                },
                new QuestObjective
                {
                    type = QuestObjective.ObjectiveType.DefeatBoss,
                    description = "Slay Mocha Dick, the cursed white whale",
                    targetBossId = "mocha_dick",
                    targetLocationId = "white_island",
                    requiredCount = 1
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Gunsmith", "Hands swore he'd kill the white whale or die trying. Knowing that stubborn old gunner, he managed neither."),
                new DialogueLine("Voice of the Deep", "The island that moves. Land upon it, and it will take you where the lost ones go."),
                new DialogueLine("Voice of the Deep", "Perhaps, Jones... that is the way in.")
            },
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("???", "*the dying whale convulses — and disgorges a man, furious and dripping*"),
                new DialogueLine("Israel Hands", "TWELVE YEARS! Twelve years in that stinking gut, and you kill my whale in ten minutes?!"),
                new DialogueLine("Israel Hands", "...Davy Jones, eh? Blackbeard shot me in the knee for less than what the sea's done to you."),
                new DialogueLine("Israel Hands", "Fine. My cannons are yours, captain. Let's go kill something bigger.")
            }
        };

        // Q5 — Into the Maelstrom: Biome 1 guardian boss (Kraken). NOTE: this
        // does NOT break the curse — that is the finale of the FINAL biome.
        // The Maelstrom is a passage; defeating its guardian opens the way
        // DOWN into the next cursed sea (Biome 2).
        var q5 = new Quest
        {
            id = "into_the_maelstrom",
            title = "Into the Maelstrom",
            description = "Your crew of legends is assembled. Van der Decken can steer through the storm wall. Sail to The Maelstrom and face its guardian.",
            questType = Quest.QuestType.Main,
            prerequisiteQuestId = "white_island_hunt",
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
                    type = QuestObjective.ObjectiveType.DefeatBoss,
                    description = "Slay the Kraken",
                    targetBossId = "kraken",
                    targetLocationId = "boss_arena",
                    requiredCount = 1
                }
            },
            startDialogue = new List<DialogueLine>
            {
                new DialogueLine("Voice of the Deep", "You've gathered legends. You've died and returned more times than any mortal could."),
                new DialogueLine("Voice of the Deep", "The Maelstrom awaits — and a leviathan coils at its mouth. Break past the Kraken, and the way opens."),
                new DialogueLine("Van der Decken", "Three hundred years of storms, captain. This one is just louder. I'll hold the wheel."),
                new DialogueLine("Israel Hands", "All cannons loaded. Let's carve a path through that thing."),
                new DialogueLine("Gunsmith", "Give 'em hell, Jones.")
            },
            // Cliffhanger, NOT the curse-break — that is the final biome's finale.
            completeDialogue = new List<DialogueLine>
            {
                new DialogueLine("Israel Hands", "It's dead! The Kraken's DEAD — captain, we did it!"),
                new DialogueLine("Van der Decken", "...Then why does the sea still pull at us?"),
                new DialogueLine("Voice of the Deep", "The Kraken was a gatekeeper. Did you think one beast could hold a curse this old?"),
                new DialogueLine("Voice of the Deep", "The Maelstrom is no exit, Davy Jones. It is a door."),
                new DialogueLine("Davy Jones", "The wheel — she won't answer! We're going down!"),
                new DialogueLine("Voice of the Deep", "Down into deeper waters. Sail on, captain. Worse things than the Kraken wait below."),
                new DialogueLine("Voice of the Deep", "*The whirlpool swallows the ship whole...*")
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
    /// Called by boss fight logic (BossArenaManager) when a boss dies.
    /// </summary>
    public void ReportBossDefeated(string bossId)
    {
        if (activeQuest == null) return;
        var obj = activeQuest.GetCurrentObjective();
        if (obj == null || obj.isComplete) return;
        if (obj.type == QuestObjective.ObjectiveType.DefeatBoss && obj.targetBossId == bossId)
        {
            obj.AddProgress(1);
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

        // Pause time during quest dialogue so the player can't die while reading
        Time.timeScale = 0f;

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
            {
                dialogueUI.ShowDialogue(q.startDialogue, () =>
                {
                    // Resume time after all dialogue is done (unless port is keeping it paused)
                    if (PortZone.GetActivePort() == null)
                        Time.timeScale = 1f;
                    onSequenceComplete?.Invoke();
                });
            }
            else
            {
                if (PortZone.GetActivePort() == null)
                    Time.timeScale = 1f;
                onSequenceComplete?.Invoke();
            }

            return;
        }

        // No next quest found — resume time and fire callback
        if (PortZone.GetActivePort() == null)
            Time.timeScale = 1f;
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
            case "ghost_light":
                // Van der Decken takes the helm: speed boost that persists
                // across re-equips (ShopManager multiplies by speedMultiplier)
                gm.speedMultiplier = 1.5f;
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                    player.moveSpeed *= 1.5f; // apply to the ship being sailed right now
                Debug.Log("Reward: Van der Decken at the helm — speed +50%.");
                break;
            case "white_island_hunt":
                // Israel Hands on the guns: fire rate boost that persists
                // across cannon rebuilds (ShopManager applies fireRateMultiplier)
                gm.fireRateMultiplier = 0.7f;
                foreach (var c in FindObjectsByType<CannonController>(FindObjectsSortMode.None))
                    c.fireRate *= 0.7f; // apply to currently mounted cannons
                Debug.Log("Reward: Israel Hands on the guns — fire rate +43%.");
                break;
            case "into_the_maelstrom":
                // Biome 1 cleared — the Maelstrom opens to Biome 2.
                // (Curse remains; "Break the Loop" is the final biome's finale.)
                gm.biome1Complete = true;
                Debug.Log("The Kraken falls — the Maelstrom drags you deeper. (Biome 2 to come.)");
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
            case "set_sail":
                locMgr.DiscoverLocation("gunsmith_wreck");
                break;
            case "rescue_gunsmith":
                // Naval Outpost has no quest purpose in the reframed chain yet
                // (its old "recruit the Pilot" role moved to Dutchman's Drift),
                // so don't flag it as a destination — only the next target.
                locMgr.DiscoverLocation("dutchmans_drift");
                break;
            case "ghost_light":
                locMgr.DiscoverLocation("white_island");
                break;
            case "white_island_hunt":
                locMgr.DiscoverLocation("boss_arena");
                break;
        }
    }
}
