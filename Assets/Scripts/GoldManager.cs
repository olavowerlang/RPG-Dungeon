using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    public int Gold { get; private set; }
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
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
