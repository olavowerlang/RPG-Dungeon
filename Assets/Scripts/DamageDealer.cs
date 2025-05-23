using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private readonly HashSet<Health> _hitThisSwing = new();

    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>(); 
    }
    /* limpa ao ligar E ao desligar, não importa como o hit-box é controlado */
    public void BeginSwing()
    {
        _hitThisSwing.Clear();
        _col.enabled = true;
    }
    public void EndSwing()
    {
        _col.enabled = false;
    }
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