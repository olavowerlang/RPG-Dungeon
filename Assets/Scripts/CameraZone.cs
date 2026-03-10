using UnityEngine;

/// <summary>
/// Place on an empty GameObject. Resize the BoxCollider2D to cover the zone area.
/// When the player walks in, the camera confiner updates to this zone.
///
/// Open Sides: check to remove the wall on that side (use for passages to other zones).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    [Header("Open Sides (no wall)")]
    [SerializeField] private bool openTop;
    [SerializeField] private bool openBottom;
    [SerializeField] private bool openLeft;
    [SerializeField] private bool openRight;

    private const float WallThickness = 1f;

    private void Awake()
    {
        var box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;

        Vector2 min, max;
        GetWorldBounds(box, out min, out max);

        float w  = max.x - min.x;
        float h  = max.y - min.y;
        float cx = (min.x + max.x) * 0.5f;
        float cy = (min.y + max.y) * 0.5f;

        if (!openBottom) SpawnWall("Wall_Bottom", new Vector2(cx, min.y - WallThickness * 0.5f), new Vector2(w, WallThickness));
        if (!openTop)    SpawnWall("Wall_Top",    new Vector2(cx, max.y + WallThickness * 0.5f), new Vector2(w, WallThickness));
        if (!openLeft)   SpawnWall("Wall_Left",   new Vector2(min.x - WallThickness * 0.5f, cy), new Vector2(WallThickness, h));
        if (!openRight)  SpawnWall("Wall_Right",  new Vector2(max.x + WallThickness * 0.5f, cy), new Vector2(WallThickness, h));
    }

    // Returns world-space path for CameraConfinerSetup's shared PolygonCollider2D
    public Vector2[] GetConfinerPath()
    {
        var box = GetComponent<BoxCollider2D>();
        Vector2 min, max;
        GetWorldBounds(box, out min, out max);
        return new[]
        {
            new Vector2(min.x, min.y),
            new Vector2(min.x, max.y),
            new Vector2(max.x, max.y),
            new Vector2(max.x, min.y),
        };
    }

    private void GetWorldBounds(BoxCollider2D box, out Vector2 min, out Vector2 max)
    {
        Vector2 c = (Vector2)transform.position + box.offset;
        Vector2 h = box.size * 0.5f;
        min = c - h;
        max = c + h;
    }

    private void SpawnWall(string wallName, Vector2 position, Vector2 size)
    {
        var go = new GameObject(wallName);
        go.transform.SetParent(transform);
        go.transform.position = position;
        go.AddComponent<BoxCollider2D>().size = size;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            CameraConfinerSetup.Instance.ActivateZone(this);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            CameraConfinerSetup.Instance.ActivateZone(this);
    }
}
