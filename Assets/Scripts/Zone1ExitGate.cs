using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Zone1ExitGate : MonoBehaviour
{
    [Header("Blocking Dialogue")]
    [SerializeField] private DialogueData pigWarningDialogue;

    [Header("First pig — hidden on first exit")]
    [SerializeField] private GameObject firstPig;

    private bool _hasLeftOnce;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        bool isNGPlus  = NGPlusManager.Instance != null && NGPlusManager.Instance.IsNGPlus;
        int  ngCount   = NGPlusManager.Instance != null ? NGPlusManager.Instance.NGPlusCount : 0;

        // NG++: always open, no requirements
        if (isNGPlus && ngCount >= 2)
        {
            OpenGate();
            return;
        }

        // NG+1: zone 1 dialogue must happen before the player can leave
        if (isNGPlus && ngCount == 1)
        {
            if (!NPCDialogue.Zone1MainDone)
            {
                if (pigWarningDialogue != null && DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue)
                    DialogueManager.Instance.StartDialogue(pigWarningDialogue);
                return;
            }
            OpenGate();
            return;
        }

        // Normal run: requires sword
        bool swordUnlocked = PlayerStats.Instance != null && PlayerStats.Instance.hasSword;
        if (!swordUnlocked)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue)
                DialogueManager.Instance.StartDialogue(pigWarningDialogue);
            return;
        }

        OpenGate();
    }

    private void OpenGate()
    {
        if (!_hasLeftOnce)
        {
            _hasLeftOnce = true;
            if (firstPig != null) firstPig.SetActive(false);
        }
        GetComponent<BoxCollider2D>().enabled = false;
    }
}
