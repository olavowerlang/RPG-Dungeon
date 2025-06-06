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
    private readonly List<Health> _enemiesAlive = new();
    
    private int _waveNumber = 1;
    private bool _spawningWave = false;

    private Camera _cam;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
            
        _cam = Camera.main;
    }

    private void Start()
    {
        _spawningWave = true;
        StartCoroutine(SpawnWave());
    }

    private void Update()
    {
        if (!_spawningWave && _enemiesAlive.Count == 0)
        {
            _spawningWave = true;
            StartCoroutine(SpawnWave());
            _waveNumber += Random.Range(1,3);
        }
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < _waveNumber; i++)
        {
            Vector2 pos = GetOffscreenPosition();
            var go = Instantiate(skelFighter, pos, Quaternion.identity);
            _enemiesAlive.Add(go.GetComponent<Health>());
            yield return new WaitForSeconds(0.1f);
        }
        _spawningWave = false;
    }

    Vector2 GetOffscreenPosition(float margin = 2f)
    {
        float halfH = _cam.orthographicSize;    
        float halfW = halfH * _cam.aspect;  

        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: return new Vector2(-halfW - margin, Random.Range(-halfH, halfH));
            case 1: return new Vector2( halfW + margin, Random.Range(-halfH, halfH));
            case 2: return new Vector2(Random.Range(-halfW, halfW),  halfH + margin);
            default:return new Vector2(Random.Range(-halfW, halfW), -halfH - margin);
        }
    }

    public void UnregisterEnemy(Health health)
    {
        _enemiesAlive.Remove(health);
    }
}
