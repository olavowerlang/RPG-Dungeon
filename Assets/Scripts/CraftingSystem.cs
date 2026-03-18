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

    // Base values for reset on death
    private float _baseSpeed;
    private float _baseAttackPush;
    private float _baseDashForce;
    private int _baseDamage;
    private float _baseKnockback;

    private PlayerStats _playerStats;
    private DamageDealer[] _damageDealers;
    private Health _playerHealth;

    public event System.Action<string> OnFuseSuccess;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _playerStats = FindObjectOfType<PlayerStats>();

        if (_playerStats != null)
        {
            _damageDealers = _playerStats.GetDamageDealers();
            _playerHealth  = _playerStats.GetComponent<Health>();
            _baseSpeed        = _playerStats.speed;
            _baseAttackPush   = _playerStats.attackPushForce;
            _baseDashForce    = _playerStats.dashForce;
            _baseDamage       = _playerStats.damage;
            _baseKnockback    = _playerStats.knockbackForce;
        }
    }

    public CraftingRecipe GetRecipeForIngredient(ItemData ingredient)
    {
        foreach (var recipe in recipes)
            if (recipe.ingredient == ingredient)
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
        ApplyBuff(recipe.buffType);

        OnFuseSuccess?.Invoke(recipe.resultDescription);
        Debug.Log($"Fused {ingredient.itemName} into sword — {recipe.resultDescription}");
        return true;
    }

    public void ApplyDirectBuff(BuffType buffType) => ApplyBuff(buffType);

    private void ApplyBuff(BuffType buffType)
    {
        if (_playerStats == null)
        {
            _playerStats = FindObjectOfType<PlayerStats>();
            if (_playerStats != null)
                _damageDealers = _playerStats.GetDamageDealers();
        }

        switch (buffType)
        {
            case BuffType.Damage:
                foreach (var dd in _damageDealers)
                    if (dd != null) dd.Damage += damageIncrement;
                break;

            case BuffType.DashSpeed:
                if (_playerStats != null)
                    _playerStats.dashForce += dashIncrement;
                break;

            case BuffType.Knockback:
                foreach (var dd in _damageDealers)
                    if (dd != null) dd.knockbackForce += knockbackIncrement;
                break;

            case BuffType.MoveSpeed:
                if (_playerStats != null)
                    _playerStats.speed += speedIncrement;
                break;

            case BuffType.MaxHP:
                if (_playerHealth != null)
                    _playerHealth.AddMaxHP(1);
                break;
        }
    }

    public void ResetBuffs()
    {
        if (_playerStats != null)
        {
            _playerStats.speed          = _baseSpeed;
            _playerStats.attackPushForce = _baseAttackPush;
            _playerStats.dashForce      = _baseDashForce;
        }

        if (_damageDealers != null)
        {
            foreach (var dd in _damageDealers)
            {
                if (dd == null) continue;
                dd.Damage          = _baseDamage;
                dd.knockbackForce  = _baseKnockback;
            }
        }
    }
}
