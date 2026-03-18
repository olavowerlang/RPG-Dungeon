using System;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public bool IsInDialogue { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI continuePrompt;

    private DialogueLine[] _lines;
    private int _currentLine;
    private bool _justOpened;
    private Action _onComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsInDialogue) return;

        if (_justOpened) { _justOpened = false; return; }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            AdvanceDialogue();
    }

    public void StartDialogue(DialogueData data, Action onComplete = null)
    {
        if (IsInDialogue) return;

        _lines = data.lines;
        _currentLine = 0;
        IsInDialogue = true;
        _justOpened = true;
        _onComplete = onComplete;

        dialoguePanel.SetActive(true);
        ShowLine(_currentLine);
    }

    private void AdvanceDialogue()
    {
        _currentLine++;
        if (_currentLine >= _lines.Length)
            EndDialogue();
        else
            ShowLine(_currentLine);
    }

    private void ShowLine(int index)
    {
        speakerNameText.text = _lines[index].speakerName;
        dialogueText.text = _lines[index].text;

        bool isLast = index == _lines.Length - 1;
        continuePrompt.text = isLast ? "[E / Space] Close" : "[E / Space] Continue";
    }

    private void EndDialogue()
    {
        IsInDialogue = false;
        dialoguePanel.SetActive(false);
        _onComplete?.Invoke();
        _onComplete = null;
    }
}
