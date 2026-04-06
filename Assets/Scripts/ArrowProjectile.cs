using UnityEngine;

/// <summary>
/// Fired by SkeletonArcher. Travels in a fixed direction, hits player or walls.
/// Call Init(direction) immediately after instantiating.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockback = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Vector2 _direction;
    private bool _initialized;
    private Rigidbody2D _rb;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Start()
    {
        Destroy(gameObject, lifetime);
        if (NGPlusManager.Instance != null && NGPlusManager.Instance.IsNGPlus)
            damage += Mathf.Min(NGPlusManager.Instance.NGPlusCount - 1, 2);
    }

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _initialized = true;
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;
        _rb.velocity = _direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (other.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeHit(damage, _direction, knockback);
            Destroy(gameObject);
            return;
        }

        if ((obstacleLayer.value & (1 << other.gameObject.layer)) != 0)
            Destroy(gameObject);
    }
}
