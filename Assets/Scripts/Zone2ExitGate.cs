using UnityEngine;

/// <summary>
/// Place a solid BoxCollider2D strip across the exit of Zone 2.
/// Blocks the player until all assigned skeletons in this zone are dead.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Zone2ExitGate : MonoBehaviour
{
    [Header("Blocking Dialogue")]
    [SerializeField] private DialogueData blockedDialogue;

    [Header("Skeletons that must all be killed to open this gate")]
    [SerializeField] private GameObject[] zone2Enemies;

    private bool AllEnemiesDead()
    {
        foreach (var enemy in zone2Enemies)
            if (enemy != null) return false;
        return true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        if (!AllEnemiesDead())
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue)
                DialogueManager.Instance.StartDialogue(blockedDialogue);
            return;
        }

        GetComponent<BoxCollider2D>().enabled = false;
    }
}
