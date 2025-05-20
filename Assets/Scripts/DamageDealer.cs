using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private readonly HashSet<Health> _hitThisSwing = new();

    /* limpa ao ligar E ao desligar, não importa como o hit-box é controlado */
    private void OnEnable()  => _hitThisSwing.Clear();
    private void OnTriggerEnter2D(Collider2D col) => TryHit(col);

    private void OnTriggerStay2D(Collider2D col)
    {
        // só primeira vez que vê esse Health no swing atual
        if (!_hitThisSwing.Contains(col.GetComponentInParent<Health>()))
            TryHit(col);
    }

    private void TryHit(Collider2D col)
    {
        var hp = col.GetComponentInParent<Health>();
        if (hp == null) return;

        // Add() devolve true apenas se ainda não estava no HashSet
        if (_hitThisSwing.Add(hp))
            hp.TakeDamage(damage);
    }
}