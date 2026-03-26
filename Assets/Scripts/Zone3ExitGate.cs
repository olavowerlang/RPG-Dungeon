using UnityEngine;

/// <summary>
/// Blocks Zone 3 exit until the player has bought the dash AND completed
/// the main dialogue with NPC2 (PigShopkeeper Instance 2).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Zone3ExitGate : MonoBehaviour
{
    [Header("Blocking Dialogue (optional)")]
    [SerializeField] private DialogueData blockedDialogue;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        bool hasDash   = PlayerStats.Instance != null && PlayerStats.Instance.hasDash;
        bool talkDone  = PigShopkeeper.Instance2TalkDone;

        if (!hasDash || !talkDone)
        {
            if (blockedDialogue != null && DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue)
                DialogueManager.Instance.StartDialogue(blockedDialogue);
            return;
        }

        GetComponent<BoxCollider2D>().enabled = false;
    }
}
