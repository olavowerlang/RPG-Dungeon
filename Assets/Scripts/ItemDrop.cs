using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] public ItemData itemData;
    [SerializeField] private int quantity = 1;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Automatically use the item's icon as the world sprite
        if (_sr != null && itemData != null)
            _sr.sprite = itemData.icon;
    }

    // Called by EnemyAnimator after setting itemData
    public void Init(ItemData data, int qty = 1)
    {
        itemData = data;
        quantity = qty;

        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _sr.sprite = data.icon;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (InventoryManager.Instance == null) return;

        bool added = InventoryManager.Instance.AddItem(itemData, quantity);
        if (added)
            Destroy(gameObject);
    }
}