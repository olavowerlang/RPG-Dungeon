using UnityEngine;

/// <summary>
/// Blocks enemies from entering zone 7. Player passes through freely.
/// Setup: BoxCollider2D with Is Trigger = ON.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyBarrier : MonoBehaviour
{
    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        Rigidbody2D rb = other.attachedRigidbody;

        // Find which side of the barrier the enemy entered from and snap them back
        Vector2 barrierPos  = transform.position;
        Vector2 enemyPos    = other.transform.position;
        Vector2 pushDir     = (enemyPos - barrierPos).normalized;

        // Snap to just outside the barrier bounds
        Bounds bounds = _col.bounds;
        Vector2 snapped = enemyPos;

        // Dominant axis = whichever side they crossed
        if (Mathf.Abs(pushDir.x) > Mathf.Abs(pushDir.y))
            snapped.x = pushDir.x > 0 ? bounds.max.x + 0.1f : bounds.min.x - 0.1f;
        else
            snapped.y = pushDir.y > 0 ? bounds.max.y + 0.1f : bounds.min.y - 0.1f;

        if (rb != null)
        {
            rb.MovePosition(snapped);
            rb.velocity = Vector2.zero;
        }
        else
        {
            other.transform.position = snapped;
        }
    }
}
