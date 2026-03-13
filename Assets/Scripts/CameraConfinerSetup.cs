using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

[RequireComponent(typeof(PolygonCollider2D))]
public class CameraConfinerSetup : MonoBehaviour
{
    public static CameraConfinerSetup Instance { get; private set; }

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CameraZone startingZone;

    private const float WallThickness = 1f;
    private CinemachineConfiner _confiner;
    private PolygonCollider2D _sharedPoly;
    private CameraZone _activeZone;
    private CameraZone[] _allZones;
    private Transform _player;

    private void Awake()
    {
        Instance = this;
        transform.position = Vector3.zero;

        _sharedPoly = GetComponent<PolygonCollider2D>();
        _sharedPoly.isTrigger = true;

        _confiner = virtualCamera.GetComponent<CinemachineConfiner>();
        if (_confiner == null)
            _confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();

        _confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
        _confiner.m_BoundingShape2D = _sharedPoly;
        _confiner.m_ConfineScreenEdges = true;

        groundTilemap.CompressBounds();
        BoundsInt cellBounds = groundTilemap.cellBounds;
        Vector2 min = groundTilemap.CellToWorld(cellBounds.min);
        Vector2 max = groundTilemap.CellToWorld(cellBounds.max);
        SpawnWalls(min, max);

        if (startingZone != null)
            ApplyZone(startingZone);
    }

    private void Start()
    {
        _allZones = FindObjectsOfType<CameraZone>();
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;
    }

    private void Update()
    {
        if (_player == null || _allZones == null) return;

        foreach (var zone in _allZones)
        {
            if (zone.ContainsPoint(_player.position))
            {
                ApplyZone(zone);
                return;
            }
        }
    }

    private void ApplyZone(CameraZone zone)
    {
        if (zone == _activeZone) return;
        _activeZone = zone;
        _sharedPoly.SetPath(0, zone.GetConfinerPath());
        _confiner.InvalidatePathCache();
    }

    private void SpawnWalls(Vector2 min, Vector2 max)
    {
        float w  = max.x - min.x;
        float h  = max.y - min.y;
        float cx = (min.x + max.x) * 0.5f;
        float cy = (min.y + max.y) * 0.5f;

        CreateWall("Wall_Bottom", new Vector2(cx, min.y - WallThickness * 0.5f), new Vector2(w, WallThickness));
        CreateWall("Wall_Top",    new Vector2(cx, max.y + WallThickness * 0.5f), new Vector2(w, WallThickness));
        CreateWall("Wall_Left",   new Vector2(min.x - WallThickness * 0.5f, cy), new Vector2(WallThickness, h));
        CreateWall("Wall_Right",  new Vector2(max.x + WallThickness * 0.5f, cy), new Vector2(WallThickness, h));
    }

    private void CreateWall(string wallName, Vector2 position, Vector2 size)
    {
        var go = new GameObject(wallName);
        go.transform.SetParent(transform);
        go.transform.position = position;
        go.AddComponent<BoxCollider2D>().size = size;
    }
}
