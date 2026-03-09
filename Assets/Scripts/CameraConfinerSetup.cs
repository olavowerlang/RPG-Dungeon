using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

/// <summary>
/// Attach this to an empty GameObject.
/// Reads the Ground tilemap bounds and:
/// 1. Auto-configures CinemachineConfiner so the camera never shows outside the tilemap.
/// 2. Spawns 4 invisible wall colliders so the player can't walk off the tilemap.
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
public class CameraConfinerSetup : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private const float WallThickness = 1f;

    private Vector2 _debugMin, _debugMax;

    private void Awake()
    {
        groundTilemap.CompressBounds();

        BoundsInt cellBounds = groundTilemap.cellBounds;
        Vector3 worldMin = groundTilemap.CellToWorld(cellBounds.min);
        Vector3 worldMax = groundTilemap.CellToWorld(cellBounds.max);
        Vector2 min = new Vector2(worldMin.x, worldMin.y);
        Vector2 max = new Vector2(worldMax.x, worldMax.y);

        _debugMin = min;
        _debugMax = max;

        Debug.Log($"[CameraConfiner] cellBounds min={cellBounds.min} max={cellBounds.max}");
        Debug.Log($"[CameraConfiner] world min={min} max={max}");
        Debug.Log($"[CameraConfiner] size=({max.x - min.x} x {max.y - min.y})");

        SetupCameraConfiner(min, max);
        SpawnWalls(min, max);
    }

    private void OnDrawGizmos()
    {
        if (_debugMin == _debugMax) return;
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((_debugMin.x + _debugMax.x) * 0.5f, (_debugMin.y + _debugMax.y) * 0.5f, 0f);
        Vector3 size   = new Vector3(_debugMax.x - _debugMin.x, _debugMax.y - _debugMin.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }

    private void SetupCameraConfiner(Vector2 min, Vector2 max)
    {
        // PolygonCollider2D paths are in local space — zero out this transform first
        transform.position = Vector3.zero;

        var poly = GetComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.SetPath(0, new[]
        {
            new Vector2(min.x, min.y),
            new Vector2(min.x, max.y),
            new Vector2(max.x, max.y),
            new Vector2(max.x, min.y),
        });

        var confiner = virtualCamera.GetComponent<CinemachineConfiner>();
        if (confiner == null)
            confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();

        confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
        confiner.m_BoundingShape2D = poly;
        confiner.m_ConfineScreenEdges = true;
        confiner.InvalidatePathCache();
    }

    private void SpawnWalls(Vector2 min, Vector2 max)
    {
        float width  = max.x - min.x;
        float height = max.y - min.y;
        float cx     = (min.x + max.x) * 0.5f;
        float cy     = (min.y + max.y) * 0.5f;

        // bottom, top, left, right
        CreateWall("Wall_Bottom", new Vector2(cx, min.y - WallThickness * 0.5f), new Vector2(width, WallThickness));
        CreateWall("Wall_Top",    new Vector2(cx, max.y + WallThickness * 0.5f), new Vector2(width, WallThickness));
        CreateWall("Wall_Left",   new Vector2(min.x - WallThickness * 0.5f, cy), new Vector2(WallThickness, height));
        CreateWall("Wall_Right",  new Vector2(max.x + WallThickness * 0.5f, cy), new Vector2(WallThickness, height));
    }

    private void CreateWall(string wallName, Vector2 position, Vector2 size)
    {
        var go = new GameObject(wallName);
        go.transform.SetParent(transform);
        go.transform.position = position;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }
}
