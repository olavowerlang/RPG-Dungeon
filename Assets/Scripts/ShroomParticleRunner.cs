using System.Collections;
using UnityEngine;

/// <summary>
/// Spawned at shroom death to delay the particle burst independently of the shroom's lifetime.
/// </summary>
public class ShroomParticleRunner : MonoBehaviour
{
    public void Run(Vector3 position)
    {
        StartCoroutine(Routine(position));
    }

    private IEnumerator Routine(Vector3 position)
    {
        yield return new WaitForSeconds(0.4f);
        GenericEnemyHitEffect.SpawnDeathParticlesAt(position);
        Destroy(gameObject);
    }
}
