using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    private Health _health;
    
    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    // Update is called once per frame
    public void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterEnemy(_health);
    }
}
