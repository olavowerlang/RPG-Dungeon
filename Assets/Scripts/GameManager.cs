using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [SerializeField] GameObject skelFighter;
    private GameObject _skelFighterInstance;
    private readonly List<Health> _enemiesAlive = new();
    //[SerializeField] private GameObject[] skelFightersActive;

    private int _waveNumber = 1;
    
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
        _waveNumber += 1;
    }

    private void Update()
    {
        if (_enemiesAlive.Count <= 0)
        {
            SpawnSkelFighter();
            _waveNumber += 1;
        }
           
    }

    private void SpawnSkelFighter()
    {
        for (int i = 0; i < _waveNumber; i++)
        { 
            Vector3 spawnPos = new Vector3(
                Random.Range(-25f, 25f),  
                Random.Range(-13f, 13f),   
                0f);
            
            var enemyInstance = Instantiate(skelFighter, spawnPos, Quaternion.identity);
            var health  = enemyInstance.GetComponent<Health>(); 
            _enemiesAlive.Add(health);     
        }
        
    }
    
    public void UnregisterEnemy(Health health)
    {
        _enemiesAlive.Remove(health);
    }
}
