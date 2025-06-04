using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Configuração do Dano")]
    [SerializeField] private int damage = 1;
    [Tooltip("Selecione apenas as layers que este hit-box deve atingir (ex.: Player)")]
    [SerializeField] private LayerMask hitLayers;

    /* ← agora guarda a interface, não o Health */
    private readonly HashSet<IDamageable> _hitSet = new();

    private Collider2D _col;

    private void Awake() => _col = GetComponent<Collider2D>();

    /* limpa a lista e liga o collider */
    public void BeginSwing()
    {
        _hitSet.Clear();
        _col.enabled = true;
    }

    public void EndSwing() => _col.enabled = false;

    private void OnTriggerEnter2D(Collider2D col) => TryHit(col);

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.TryGetComponent<IDamageable>(out var dmg) && !_hitSet.Contains(dmg))
            TryHit(col);
    }
    
    private void TryHit(Collider2D other)
    {

        int layer = other.gameObject.layer;
        if ((hitLayers.value & (1 << layer)) == 0) return;

        if (!other.TryGetComponent<IDamageable>(out var target)) return;

        //garante um único hit por swing
        if (!_hitSet.Add(target)) return;

        //calcula direção p/ knockback e dispara TakeHit
        Vector2 dir = (other.transform.position - transform.position).normalized;
        target.TakeHit(damage, dir);
    }
}