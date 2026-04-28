using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Freedom choice panel for NG+++.
/// Reuses NPCInteractionPanel for arrow-key navigation and cycling sound.
/// Shows an explanatory text above the two options.
/// </summary>
public class FreedomChoiceUI : MonoBehaviour
{
    public static FreedomChoiceUI Instance { get; private set; }

    [SerializeField] private GameObject          panel;
    [SerializeField] private TextMeshProUGUI     bodyText;
    [SerializeField] private NPCInteractionPanel interactionPanel;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void Show(Action onFree, Action onLoop)
    {
        if (interactionPanel == null)
        {
            Debug.LogError("[FreedomChoiceUI] interactionPanel not assigned — defaulting to loop.");
            onLoop?.Invoke();
            return;
        }

        if (panel != null) panel.SetActive(true);

        interactionPanel.Show(
            labels:    new[] { "Free him", "Break his will" },
            enabled:   new[] { true, true },
            onConfirm: index =>
            {
                if (panel != null) panel.SetActive(false);
                if (index == 0) onFree?.Invoke();
                else            onLoop?.Invoke();
            },
            onCancel: () =>
            {
                // Escape = same as "Keep going"
                if (panel != null) panel.SetActive(false);
                onLoop?.Invoke();
            }
        );
    }
}
