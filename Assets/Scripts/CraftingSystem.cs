using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance;

    [Header("Recipes — all fuse ingredient into sword")]
    public List<CraftingRecipe> recipes = new();

    [Header("Upgrade Increments")]
    public float speedIncrement = 2.5f;
    public float dashIncrement = 5f;
    public int damageIncrement = 1;
    public float knockbackIncrement = 3.5f;
    public float dashStaminaIncrease = 1f;

    private PlayerStats _playerStats;
    private DamageDealer[] _damageDealers;
    private Health _playerHealth;

    public event System.Action<string> OnFuseSuccess;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _playerStats = FindObjectOfType<PlayerStats>();

        if (_playerStats != null)
        {
            _damageDealers = _playerStats.GetDamageDealers();
            _playerHealth  = _playerStats.GetComponent<Health>();
        }
    }

    public CraftingRecipe GetRecipeForIngredient(ItemData ingredient)
    {
        foreach (var recipe in recipes)
            if (recipe != null && recipe.ingredient == ingredient)
                return recipe;
        return null;
    }

    public bool TryFuse(ItemData ingredient)
    {
        if (InventoryManager.Instance == null) return false;
        if (!InventoryManager.Instance.HasSword())
        {
            Debug.Log("No sword equipped — cannot fuse.");
            return false;
        }

        CraftingRecipe recipe = GetRecipeForIngredient(ingredient);
        if (recipe == null)
        {
            Debug.Log($"{ingredient.itemName} has no fusion recipe.");
            return false;
        }

        if (!InventoryManager.Instance.HasIngredient(ingredient, recipe.quantity))
        {
            Debug.Log("Not enough ingredients.");
            return false;
        }

        InventoryManager.Instance.RemoveItem(ingredient, recipe.quantity);
        ApplyBuff(recipe.buffType, recipe.buffValue);

        AudioManager.Instance?.PlayFusion();
        OnFuseSuccess?.Invoke(recipe.resultDescription);
        Debug.Log($"Fused {ingredient.itemName} into sword — {recipe.resultDescription}");
        return true;
    }

    public void ApplyDirectBuff(BuffType buffType) => ApplyBuff(buffType, -1f);

    // value < 0 means "use the CraftingSystem's own increment field" (shop / direct buff path)
    // value >= 0 means "use this exact amount" (recipe craft path)
    private void ApplyBuff(BuffType buffType, float value = -1f)
    {
        if (_playerStats == null)
        {
            _playerStats = FindObjectOfType<PlayerStats>();
            if (_playerStats != null)
            {
                _damageDealers = _playerStats.GetDamageDealers();
                _playerHealth  = _playerStats.GetComponent<Health>();
            }
        }

        switch (buffType)
        {
            case BuffType.Damage:
                if (_playerStats != null)
                    _playerStats.AddDamage(value >= 0f ? Mathf.RoundToInt(value) : damageIncrement);
                break;

            case BuffType.DashSpeed:
                if (_playerStats != null)
                    _playerStats.dashForce += value >= 0f ? value : dashIncrement;
                break;

            case BuffType.Knockback:
                if (_playerStats != null)
                {
                    _playerStats.knockbackForce += value >= 0f ? value : knockbackIncrement;
                    _playerStats.PushToDealers();
                }
                break;

            case BuffType.MoveSpeed:
                if (_playerStats != null)
                    _playerStats.speed += value >= 0f ? value : speedIncrement;
                break;

            case BuffType.MaxHP:
                if (_playerHealth != null)
                    _playerHealth.AddMaxHP(1);
                break;

            case BuffType.DashStamina:
                if (_playerStats != null)
                    _playerStats.maxDashStamina += value >= 0f ? value : dashStaminaIncrease;
                break;
        }
    }

    public void ResetBuffs()
    {
        if (_playerStats != null)
        {
            _playerStats.speed          = _playerStats.baseSpeed;
            _playerStats.dashForce      = _playerStats.baseDashForce;
            _playerStats.damage         = _playerStats.baseDamage;
            _playerStats.knockbackForce = _playerStats.baseKnockback;
            _playerStats.maxDashStamina = _playerStats.baseMaxDashStamina;
            _playerStats.PushToDealers();
        }

        if (_playerHealth != null && _playerStats != null)
            _playerHealth.ScaleMaxHP(_playerStats.baseMaxHP);
    }
}
