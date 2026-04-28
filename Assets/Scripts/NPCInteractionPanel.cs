using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Arrow-key-navigable panel with selectable options.
/// Call Show() to open, Hide() to close.
/// Arrow keys navigate, E confirms, greyed options are skipped.
/// </summary>
public class NPCInteractionPanel : MonoBehaviour
{
    [Header("Option Labels (one TextMeshPro per option)")]
    [SerializeField] private TextMeshProUGUI[] optionTexts;

    [Header("Colors")]
    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color greyedColor   = new Color(0.45f, 0.45f, 0.45f, 1f);

    public static bool IsOpen { get; private set; }

    private string[]    _labels;
    private bool[]      _enabled;
    private Action<int> _onConfirm;
    private Action      _onCancel;
    private int         _selected;
    private bool        _isOpen;
    private float       _confirmCooldown;

    private Transform    _worldTarget;
    private RectTransform _rt;
    private Canvas        _rootCanvas;

    private void Awake()
    {
        _rt         = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
        if (optionTexts == null || optionTexts.Length == 0)
            optionTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
    }

    // Call before Show() to make the panel follow a world-space object (e.g. the NPC).
    public void SetWorldTarget(Transform target) => _worldTarget = target;

    private void LateUpdate()
    {
        if (!_isOpen || _worldTarget == null || Camera.main == null || _rt == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_worldTarget.position + Vector3.up * 1.2f);
        if (screenPos.z > 0f)
            _rt.position = screenPos;
    }

    private void Update()
    {
        if (!_isOpen) return;

        _confirmCooldown -= Time.unscaledDeltaTime;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            TryMove(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            TryMove(-1);

        if (_confirmCooldown <= 0f && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            Confirm();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
            _onCancel?.Invoke();
        }
    }

    /// <param name="labels">Display text for each option.</param>
    /// <param name="enabled">Whether each option is selectable (false = greyed out).</param>
    /// <param name="onConfirm">Called with the confirmed option index.</param>
    /// <param name="onCancel">Called if the panel is dismissed without selection (optional).</param>
    public void Show(string[] labels, bool[] enabled, Action<int> onConfirm, Action onCancel = null)
    {
        _labels    = labels;
        _enabled   = enabled;
        _onConfirm = onConfirm;
        _onCancel  = onCancel;
        _isOpen    = true;
        IsOpen     = true;
        _confirmCooldown = 0.18f;

        // Default to first enabled option
        _selected = 0;
        for (int i = 0; i < labels.Length; i++)
            if (enabled[i]) { _selected = i; break; }

        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        _isOpen      = false;
        IsOpen       = false;
        _worldTarget = null;
        gameObject.SetActive(false);
    }

    private void TryMove(int dir)
    {
        int next     = _selected + dir;
        int attempts = 0;

        while (attempts < _labels.Length)
        {
            if (next >= _labels.Length) next = 0;
            if (next < 0) next = _labels.Length - 1;

            if (_enabled[next]) { _selected = next; Refresh(); AudioManager.Instance?.PlayGenericClick(); return; }
            next += dir;
            attempts++;
        }
    }

    private void Confirm()
    {
        if (!_enabled[_selected]) return;
        int chosen = _selected;
        AudioManager.Instance?.PlayGenericClick();
        Hide();
        _onConfirm?.Invoke(chosen);
    }

    private void Refresh()
    {
        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i] == null) continue;
            bool hasLabel = i < _labels.Length;
            optionTexts[i].gameObject.SetActive(hasLabel);
            if (!hasLabel) continue;
            optionTexts[i].text  = _labels[i];
            optionTexts[i].alpha = 1f;
            optionTexts[i].color = !_enabled[i] ? greyedColor
                                 : i == _selected ? selectedColor
                                 : normalColor;
        }
    }
}
