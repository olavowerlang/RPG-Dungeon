using System.Collections;
using UnityEngine;

/// <summary>
/// Plays periodic idle sounds for an enemy only when the player is nearby.
/// Globally limits how many of the same enemy type play idle at once —
/// so a room of skeletons doesn't become a wall of noise.
/// Add to enemy root. Set EnemyType to match the enemy.
/// </summary>
public class EnemyAmbientSound : MonoBehaviour
{
    public enum EnemyType { Skeleton, Shroom, Slime }

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private float     range       = 9f;
    [SerializeField] private float     minInterval = 4f;
    [SerializeField] private float     maxInterval = 10f;

    private const int MaxSimultaneous = 2;

    // Per-type global counters
    private static int _skeletonActive;
    private static int _shroomActive;
    private static int _slimeActive;

    private Transform _player;
    private float     _timer;
    private bool      _dead;
    private bool      _slotTaken; // tracks whether this instance currently holds a counter slot

    public static void ResetCounters()
    {
        _skeletonActive = 0;
        _shroomActive   = 0;
        _slimeActive    = 0;
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
        _timer  = Random.Range(minInterval, maxInterval);

        var health = GetComponent<Health>();
        if (health != null) health.OnDeath += () => _dead = true;
    }

    private void OnDestroy()
    {
        // If destroyed while holding a slot, release it so the counter doesn't leak
        if (_slotTaken)
        {
            DecrementCount();
            _slotTaken = false;
        }
    }

    private void Update()
    {
        if (_dead || _player == null) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = Random.Range(minInterval, maxInterval);

        if (Vector2.Distance(transform.position, _player.position) > range) return;
        if (GetCount() >= MaxSimultaneous) return;

        IncrementCount();
        _slotTaken = true;
        PlayIdle();
        StartCoroutine(ReleaseSlot());
    }

    private void PlayIdle()
    {
        switch (enemyType)
        {
            case EnemyType.Skeleton: AudioManager.Instance?.PlaySkeletonIdle(); break;
            case EnemyType.Shroom:   AudioManager.Instance?.PlayShroomIdle();   break;
            case EnemyType.Slime:    AudioManager.Instance?.PlaySlimeJump();    break;
        }
    }

    private IEnumerator ReleaseSlot()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        _slotTaken = false;
        DecrementCount();
    }

    private int GetCount()
    {
        switch (enemyType)
        {
            case EnemyType.Skeleton: return _skeletonActive;
            case EnemyType.Shroom:   return _shroomActive;
            default:                 return _slimeActive;
        }
    }

    private void IncrementCount()
    {
        switch (enemyType)
        {
            case EnemyType.Skeleton: _skeletonActive++; break;
            case EnemyType.Shroom:   _shroomActive++;   break;
            default:                 _slimeActive++;    break;
        }
    }

    private void DecrementCount()
    {
        switch (enemyType)
        {
            case EnemyType.Skeleton: _skeletonActive = Mathf.Max(0, _skeletonActive - 1); break;
            case EnemyType.Shroom:   _shroomActive   = Mathf.Max(0, _shroomActive   - 1); break;
            default:                 _slimeActive    = Mathf.Max(0, _slimeActive    - 1); break;
        }
    }
}
