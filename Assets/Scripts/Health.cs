using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    private int _currentHp;
    
    private void Awake() => _currentHp = maxHP;

    public void TakeDamage(int dmg)
    {
        _currentHp -= dmg;
        if (_currentHp <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    
}
