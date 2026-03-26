using UnityEngine;

/// <summary>
/// Place on a trigger collider in the scene to switch music when the player enters.
/// Set the collider to Is Trigger. Works for any area — shop room, combat zone, etc.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MusicZone : MonoBehaviour
{
    public enum Zone { StartArea, Combat, Shop, Boss, Cave, Stop }

    [SerializeField] private Zone zone = Zone.Combat;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!GameManager.HasStarted) return;

        switch (zone)
        {
            case Zone.StartArea:
                AudioManager.Instance?.PlayStartAreaMusic();
                AudioManager.Instance?.PlayStartAreaAmbient();
                break;
            case Zone.Combat:
                AudioManager.Instance?.PlayCombatMusic();
                AudioManager.Instance?.StopAmbient();
                break;
            case Zone.Shop:
                AudioManager.Instance?.PlayShopMusic();
                AudioManager.Instance?.StopAmbient();
                break;
            case Zone.Boss:
                AudioManager.Instance?.PlayBossMusic();
                AudioManager.Instance?.StopAmbient();
                break;
            case Zone.Cave:
                AudioManager.Instance?.StopMusic();
                AudioManager.Instance?.StopAmbient();
                AudioManager.Instance?.PlayCaveEntrance();
                break;
            case Zone.Stop:
                AudioManager.Instance?.StopMusic();
                AudioManager.Instance?.StopAmbient();
                break;
        }
    }
}
