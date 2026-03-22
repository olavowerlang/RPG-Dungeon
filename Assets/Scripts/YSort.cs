using UnityEngine;

/// <summary>
/// Attach to the root of any moving object (player, enemies, item drops).
/// Finds the SpriteRenderer in children — do NOT add this to the visual child directly.
/// </summary>
public class YSort : MonoBehaviour
{
    SpriteRenderer _sr;

    void Awake() => _sr = GetComponentInChildren<SpriteRenderer>(true);

    void LateUpdate()
    {
        if (_sr != null)
            _sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}
