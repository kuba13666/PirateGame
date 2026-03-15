using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Bottom-screen dialogue panel that shows speaker name + text line by line.
/// Player taps/clicks to advance. Fires callback when all lines are read.
/// Uses unscaled time so it works while the game is paused (in port).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI continueHint;

    private readonly Queue<DialogueLine> lineQueue = new Queue<DialogueLine>();
    private Action onComplete;
    private bool isShowing;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isShowing) return;

        // Advance on click / tap (use unscaledTime so it works when paused)
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextLine();
        }
    }

    /// <summary>
    /// Begin showing a sequence of dialogue lines.
    /// </summary>
    public void ShowDialogue(List<DialogueLine> lines, Action onFinished)
    {
        if (lines == null || lines.Count == 0)
        {
            onFinished?.Invoke();
            return;
        }

        lineQueue.Clear();
        foreach (var line in lines)
            lineQueue.Enqueue(line);

        onComplete = onFinished;
        isShowing = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowNextLine();
    }

    void ShowNextLine()
    {
        if (lineQueue.Count == 0)
        {
            CloseDialogue();
            return;
        }

        DialogueLine line = lineQueue.Dequeue();

        if (speakerText != null)
            speakerText.text = line.speaker;
        if (bodyText != null)
            bodyText.text = line.text;
        if (continueHint != null)
            continueHint.text = lineQueue.Count > 0 ? "[Click to continue]" : "[Click to close]";
    }

    void CloseDialogue()
    {
        isShowing = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        var cb = onComplete;
        onComplete = null;
        cb?.Invoke();
    }
}
