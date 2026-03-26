using UnityEngine;

/// <summary>
/// Place on a trigger collider in the scene to switch music when the player enters.
/// Set the collider to Is Trigger. Works for any area — shop room, combat zone, etc.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MusicZone : MonoBehaviour
{
    public enum Zone { StartArea, Combat, Shop, Boss, Stop }

    [SerializeField] private Zone zone = Zone.Combat;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (zone)
        {
            case Zone.StartArea: AudioManager.Instance?.PlayStartAreaMusic(); break;
            case Zone.Combat:    AudioManager.Instance?.PlayCombatMusic();    break;
            case Zone.Shop:      AudioManager.Instance?.PlayShopMusic();      break;
            case Zone.Boss:      AudioManager.Instance?.PlayBossMusic();      break;
            case Zone.Stop:      AudioManager.Instance?.StopMusic();          break;
        }
    }
}
