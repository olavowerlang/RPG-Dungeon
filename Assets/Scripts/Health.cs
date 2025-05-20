using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 3;
    public int currentHp;
    
    private void Awake() => currentHp = maxHP;

    public void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    
}
