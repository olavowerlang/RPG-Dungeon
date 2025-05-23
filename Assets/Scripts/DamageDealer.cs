using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Configuração do Dano")]
    [SerializeField] private int damage = 1;
    [Tooltip("Selecione apenas as layers que este hit-box deve atingir (ex.: Player)")]
    [SerializeField] private LayerMask hitLayers;

    private readonly HashSet<Health> _hitSet = new();

    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>(); 
    }
    /* limpa ao ligar E ao desligar, não importa como o hit-box é controlado */
    public void BeginSwing()
    {
        _hitSet.Clear();
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
        if (!_hitSet.Contains(col.GetComponentInParent<Health>()))
            TryHit(col);
    }

    private void TryHit(Collider2D other)
    {
        int layer = other.gameObject.layer;
        if ((hitLayers.value & (1 << layer)) == 0)
            return;

        if (!other.TryGetComponent<Health>(out var hp))
            return;

        if (_hitSet.Add(hp))
            hp.TakeDamage(damage);
    }
}