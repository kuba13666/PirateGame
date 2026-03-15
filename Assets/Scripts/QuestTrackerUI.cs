using UnityEngine;
using TMPro;

/// <summary>
/// Small HUD element (top-left, below health) showing current quest name + objective.
/// Listens to QuestManager events to stay updated.
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI objectiveText;
    public GameObject trackerPanel;

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestActivated += OnQuestActivated;
            QuestManager.Instance.OnObjectiveProgress += OnObjectiveProgress;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;

            // Show current quest if one is already active
            Quest active = QuestManager.Instance.GetActiveQuest();
            if (active != null)
                UpdateDisplay(active);
            else
                HideTracker();
        }
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestActivated -= OnQuestActivated;
            QuestManager.Instance.OnObjectiveProgress -= OnObjectiveProgress;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    void OnQuestActivated(Quest q)
    {
        UpdateDisplay(q);
    }

    void OnObjectiveProgress(Quest q, QuestObjective obj)
    {
        UpdateDisplay(q);
    }

    void OnQuestCompleted(Quest q)
    {
        // Brief delay then show next quest or hide
        Quest next = QuestManager.Instance.GetActiveQuest();
        if (next != null)
            UpdateDisplay(next);
        else
            HideTracker();
    }

    void UpdateDisplay(Quest q)
    {
        if (trackerPanel != null) trackerPanel.SetActive(true);

        if (questTitleText != null)
            questTitleText.text = q.title;

        if (objectiveText != null)
        {
            QuestObjective obj = q.GetCurrentObjective();
            objectiveText.text = obj != null ? obj.description : "Complete!";
        }
    }

    void HideTracker()
    {
        if (trackerPanel != null) trackerPanel.SetActive(false);
    }
}
