using UnityEngine;

/// <summary>
/// Gate that requires a specific dialogue to be completed before unlocking the store.
/// </summary>
public class DialogueGate : ShopGate
{
    [SerializeField] private DialogueData dialogueData;

    private bool _cleared;

    public override bool IsCleared => _cleared;

    public override void Activate()
    {
        if (DialogueManager.Instance.IsInDialogue) return;
        DialogueManager.Instance.StartDialogue(dialogueData, () => _cleared = true);
    }
}
