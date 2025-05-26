using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [SerializeField] private Health health;
    
    private GameObject _skelFighterInstance;
    
    [SerializeField] private GameObject[] skelFightersActive;
    
   // [SerializeField] private GameObject skelFighter;
    
    
    
    // Start is called before the first frame update
   
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }

    private void Start()
    {
        SpawnSkelFighter();
    }

    private void Update()
    {
        SpawnSkelFighter();
    }

    private void SpawnSkelFighter()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(-25f, 25f),   // X
            Random.Range(-13f, 13f),   // Y
            0f);

        foreach (var skelFighter in skelFightersActive )
        {
           _skelFighterInstance = Instantiate(skelFighter, spawnPos, Quaternion.identity);
           
        }
        
       // if (_skelFighterInstance == null)
          //  _skelFighterInstance = Instantiate(skelFighter, spawnPos, Quaternion.identity);
    }
}
