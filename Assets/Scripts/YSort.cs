using UnityEngine;

/// <summary>
/// Attach to any moving sprite (player, enemies, NPCs, item drops) so their
/// sorting order updates as they move up/down the screen.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        sr.sortingOrder = 10000 + Mathf.RoundToInt(-transform.position.y * 100);
    }
}
