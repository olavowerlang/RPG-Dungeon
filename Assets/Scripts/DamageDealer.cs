using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Configuração do Dano")]
    [SerializeField] private int damage = 1;
    public float knockbackForce = 0f;
    [Tooltip("Selecione apenas as layers que este hit-box deve atingir (ex.: Player)")]
    [SerializeField] private LayerMask hitLayers;

    public int Damage
    {
        get => damage;
        set => damage = value;
    }

    public void AddHitLayer(int layer) => hitLayers |= 1 << layer;

    /* ← agora guarda a interface, não o Health */
    private readonly HashSet<IDamageable> _hitSet = new();

    private Collider2D _col;

    private void Awake() => _col = GetComponent<Collider2D>();

    private Collider2D Col
    {
        get
        {
            if (_col == null) _col = GetComponent<Collider2D>();
            return _col;
        }
    }

    /* limpa a lista e liga o collider */
    public void BeginSwing()
    {
        _hitSet.Clear();
        if (Col != null)
            Col.enabled = true;
        else
            Debug.LogError($"DamageDealer on '{gameObject.name}': no Collider2D found — hitbox cannot enable! Check the hitbox child has a Collider2D component.", this);
    }

    public void EndSwing() { if (Col != null) Col.enabled = false; }

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

        var target = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        //garante um único hit por swing
        if (!_hitSet.Add(target)) return;

        //calcula direção p/ knockback e dispara TakeHit
        Vector2 dir = (other.transform.position - transform.position).normalized;
        target.TakeHit(damage, dir, knockbackForce);
    }
}