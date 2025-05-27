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
    //[SerializeField] private GameObject[] skelFightersActive;

    private int _waveNumber = 0;
    private bool _waveFinished = false;
    
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
        _waveNumber += 1;
        
        for (int i = 0; i < _waveNumber; i++)
        { 
            Vector3 spawnPos = new Vector3(
                Random.Range(-25f, 25f),  
                Random.Range(-13f, 13f),   
                0f);
            
            _skelFighterInstance = Instantiate(skelFighter, spawnPos, Quaternion.identity);
        }

        if (_waveFinished == false)
        {   
            _waveNumber += 1;
            _waveFinished = true;
        }
        
        _waveFinished = false;
        
       // if (_skelFighterInstance == null)
          //  _skelFighterInstance = Instantiate(skelFighter, spawnPos, Quaternion.identity);
    }
}
