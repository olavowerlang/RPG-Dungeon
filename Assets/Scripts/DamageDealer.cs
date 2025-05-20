using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    // "tipo de lista" que não repete
    private readonly HashSet<Health> _hitThisSwing = new();

    // reseta a lista toda vez que a animação liga o GameObject da hit-box
    private void OnEnable() => _hitThisSwing.Clear();

    // alvo entrou DEPOIS de ligar a hit-box
    private void OnTriggerEnter2D(Collider2D col) => TryHit(col);

    // alvo já estava dentro no exato frame em que ligamos a hit-box
    private void OnTriggerStay2D(Collider2D col)
    {
        // só tenta se ainda não marcou esse alvo
        if (!_hitThisSwing.Contains(col.GetComponentInParent<Health>()))
            TryHit(col);
    }

    private void TryHit(Collider2D col)
    {
        var hp = col.GetComponentInParent<Health>();   // procura Health no próprio objeto ou nos pais
        if (hp == null) return;                           // não tem vida → ignora

        if (_hitThisSwing.Add(hp))                        // true se é a 1.ª vez que acerta esse alvo
            hp.TakeDamage(damage);                        // aplica o dano
    }
}