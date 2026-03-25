using UnityEngine;

/// <summary>
/// Attach to the Bombshroom prefab.
/// On death: drops guaranteed mushroom + gold + one random item from the loot table.
/// </summary>
public class BombshroomDropper : MonoBehaviour
{
    [Header("Guaranteed Drops")]
    [SerializeField] private ItemData  mushroomItem;
    [SerializeField] private GameObject itemDropPrefab;

    [Header("Gold")]
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private int        minGold = 5;
    [SerializeField] private int        maxGold = 15;

    [Header("Random Item")]
    [SerializeField] private LootTable lootTable;

    [Header("XP")]
    [SerializeField] private float xpReward = 2f;

    private Health _health;
    private bool   _dropped;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (_health != null) _health.OnDeath += Drop;
    }

    private void OnDisable()
    {
        if (_health != null) _health.OnDeath -= Drop;
    }

    private void Drop()
    {
        if (_dropped) return;
        _dropped = true;

        Vector3 pos = transform.position;

        // Guaranteed mushroom
        if (mushroomItem != null && itemDropPrefab != null)
        {
            GameObject go = Instantiate(itemDropPrefab, pos + new Vector3(0.4f, 0.2f, 0f), Quaternion.identity);
            go.GetComponent<ItemDrop>()?.Init(mushroomItem);
        }

        // Gold
        if (goldDropPrefab != null)
        {
            int amount = Random.Range(minGold, maxGold + 1);
            GameObject go = Instantiate(goldDropPrefab, pos + new Vector3(-0.4f, 0.2f, 0f), Quaternion.identity);
            go.GetComponent<GoldDrop>()?.Init(amount);
        }

        // Random item from loot table
        if (lootTable != null && itemDropPrefab != null)
        {
            ItemData drop = lootTable.Roll();
            if (drop != null)
            {
                GameObject go = Instantiate(itemDropPrefab, pos + new Vector3(0f, 0.4f, 0f), Quaternion.identity);
                go.GetComponent<ItemDrop>()?.Init(drop);
            }
        }

        // XP
        XPManager xp = FindObjectOfType<XPManager>();
        if (xp != null) xp.GainXP(xpReward);
    }
}
