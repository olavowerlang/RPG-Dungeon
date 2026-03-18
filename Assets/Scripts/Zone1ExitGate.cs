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

        bool swordUnlocked = PlayerStats.Instance != null && PlayerStats.Instance.hasSword;

        if (!swordUnlocked)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue)
                DialogueManager.Instance.StartDialogue(pigWarningDialogue);
            return;
        }

        if (!_hasLeftOnce)
        {
            _hasLeftOnce = true;
            if (firstPig != null)
                firstPig.SetActive(false);
        }

        GetComponent<BoxCollider2D>().enabled = false;
    }
}
