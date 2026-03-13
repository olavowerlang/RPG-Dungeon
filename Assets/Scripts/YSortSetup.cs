using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Attach to ONE empty GameObject in the scene.
/// Automatically handles Y-sorting for every sprite and tilemap — no per-object setup needed.
/// For moving objects (player, enemies) also attach YSort.cs to them.
/// </summary>
[ExecuteAlways]
public class YSortSetup : MonoBehaviour
{
    void OnEnable() => Apply();

    void Apply()
    {
        // Camera sort mode (play mode)
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transparencySortMode = TransparencySortMode.CustomAxis;
            cam.transparencySortAxis = new Vector3(0f, 1f, 0f);
        }

        // Every tilemap: Individual mode + TopLeft sort order (works in scene view too)
        foreach (TilemapRenderer tr in FindObjectsOfType<TilemapRenderer>())
        {
            tr.mode = TilemapRenderer.Mode.Individual;
            tr.sortOrder = TilemapRenderer.SortOrder.TopLeft;
        }

        // Every static sprite: explicit sortingOrder from Y (works in scene view too)
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>())
        {
            if (sr.GetComponent<YSort>() != null) continue;
            sr.sortingOrder = 10000 + Mathf.RoundToInt(-sr.transform.position.y * 100);
        }
    }
}
