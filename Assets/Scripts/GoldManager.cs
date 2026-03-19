using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [SerializeField] private int startingGold = 0;
    [SerializeField] private int debugAddGold = 0;

    public int Gold { get; private set; }
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Gold = startingGold;
        OnGoldChanged?.Invoke(Gold);
    }

    private void Update()
    {
        if (debugAddGold != 0)
        {
            AddGold(debugAddGold);
            debugAddGold = 0;
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}
