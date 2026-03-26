using UnityEngine;

/// <summary>
/// Blocks the Zone 7 cave entrance. No dialogue — player simply can't pass
/// until MushroomQuestPig.TalkDone is true (NPC3 first talk completed).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CaveExitGate : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        if (MushroomQuestPig.TalkDone)
            GetComponent<BoxCollider2D>().enabled = false;
    }
}
