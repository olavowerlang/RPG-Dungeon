using UnityEngine;

public class GoldDrop : MonoBehaviour
{
    [SerializeField] private int amount = 5;

    public void Init(int goldAmount) => amount = goldAmount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GoldManager.Instance == null) return;
        GoldManager.Instance.AddGold(amount);
        Destroy(gameObject);
    }
}
