using UnityEngine;

// Attach to the Player root GameObject
// Listens to Health.OnDeath and resets inventory + buffs
public class PlayerDeathHandler : MonoBehaviour
{
    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearInventory();

        if (CraftingSystem.Instance != null)
            CraftingSystem.Instance.ResetBuffs();
    }
}