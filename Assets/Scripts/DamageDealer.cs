using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var health = collision.GetComponentInParent<Health>();
        if (health != null)
            health.TakeDamage(damage);
        Debug.Log("Target took" + damage + " damage");
    }

    //permitir mudar dano em runtime (power-ups)
    public void SetDamage(int value) => damage = value;
}