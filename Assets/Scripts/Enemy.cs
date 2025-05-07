using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    private float _currentHealth;

    protected virtual void Awake()
    {
        _currentHealth = maxHealth;
    }
    
    public virtual void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0f)
            Die();
    }

 
    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}