using System;
using UnityEngine;

/// <summary>
/// Shown after Humberto's freedom offer dialogue in NG+++.
/// Two buttons: let him go (true ending) or keep the loop (continue fighting).
/// Wire OnFreePressed / OnLoopPressed to the respective button OnClick events in the Inspector.
/// </summary>
public class FreedomChoiceUI : MonoBehaviour
{
    public static FreedomChoiceUI Instance { get; private set; }

    [SerializeField] private GameObject panel;

    private Action _onFree;
    private Action _onLoop;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void Show(Action onFree, Action onLoop)
    {
        _onFree = onFree;
        _onLoop = onLoop;
        if (panel != null) panel.SetActive(true);
    }

    // Wire these to the buttons in the Inspector
    public void OnFreePressed()
    {
        if (panel != null) panel.SetActive(false);
        _onFree?.Invoke();
    }

    public void OnLoopPressed()
    {
        if (panel != null) panel.SetActive(false);
        _onLoop?.Invoke();
    }
}
