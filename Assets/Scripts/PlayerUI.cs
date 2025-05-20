using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image[] healthImageList;
    
    private Health _health;
    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    // Update is called once per frame
    private void Update()
    {
        for (int i = 0; i < healthImageList.Length; i++) 
            healthImageList[i].enabled = i < _health.currentHp;
        
    }
}
