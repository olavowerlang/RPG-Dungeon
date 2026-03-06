using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance;

    [Header("Recipes — all fuse ingredient into sword")]
    public List<CraftingRecipe> recipes = new();

    // Base values for reset on death
    private float _baseSpeed;
    private float _baseAttackPush;
    private float _baseDashForce;
    private int _baseDamage;

    private PlayerController _playerController;
    private DamageDealer _damageDealer;

    public event System.Action<string> OnFuseSuccess;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _playerController = FindObjectOfType<PlayerController>();
        _damageDealer = FindObjectOfType<DamageDealer>();

        if (_playerController != null)
        {
            _baseSpeed = _playerController.speed;
            _baseAttackPush = _playerController.attackPushForce;
            _baseDashForce = _playerController.dashForce;
        }

        if (_damageDealer != null)
            _baseDamage = _damageDealer.Damage;
    }

    // Returns the recipe that matches this ingredient, or null
    public CraftingRecipe GetRecipeForIngredient(ItemData ingredient)
    {
        foreach (var recipe in recipes)
            if (recipe.ingredient == ingredient)
                return recipe;
        return null;
    }

    // Try to fuse selected ingredient into the sword
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

        OnFuseSuccess?.Invoke(recipe.resultDescription);
        Debug.Log($"Fused {ingredient.itemName} into sword — {recipe.resultDescription}");
        return true;
    }

    private void ApplyBuff(BuffType buffType, float value)
    {
        if (_playerController == null)
            _playerController = FindObjectOfType<PlayerController>();
        if (_damageDealer == null)
            _damageDealer = FindObjectOfType<DamageDealer>();

        switch (buffType)
        {
            case BuffType.Damage:
                if (_damageDealer != null)
                    _damageDealer.Damage += (int)value;
                break;

            case BuffType.DashSpeed:
                if (_playerController != null)
                    _playerController.dashForce += value;
                break;

            case BuffType.Knockback:
                if (_playerController != null)
                    _playerController.attackPushForce += value;
                break;

            case BuffType.MoveSpeed:
                if (_playerController != null)
                    _playerController.speed += value;
                break;

            case BuffType.Range:
                if (_damageDealer != null)
                {
                    Vector3 scale = _damageDealer.transform.localScale;
                    scale *= (1f + value);
                    _damageDealer.transform.localScale = scale;
                }
                break;
        }
    }

    public void ResetBuffs()
    {
        if (_playerController != null)
        {
            _playerController.speed = _baseSpeed;
            _playerController.attackPushForce = _baseAttackPush;
            _playerController.dashForce = _baseDashForce;
        }

        if (_damageDealer != null)
        {
            _damageDealer.Damage = _baseDamage;
            _damageDealer.transform.localScale = Vector3.one;
        }
    }
}