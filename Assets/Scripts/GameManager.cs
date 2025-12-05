using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int _enemiesKilled = 0; // Contador de mortes

    [SerializeField] GameObject skelFighter;
    private readonly List<Health> _enemiesAlive = new();
    
    
    private int _waveNumber = 1;
    [FormerlySerializedAs("_spawningWave")] public bool spawningWave = false;

    private Camera _cam;
    
    public bool gameStarted = false;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

    }

    private void Update()
    {
        if (!spawningWave && _enemiesAlive.Count == 0 && gameStarted)
        {
            spawningWave = true;
            //StartCoroutine(SpawnWave());
            _waveNumber += Random.Range(1, 4);
        }

         
    }

    public void StartGame()
    {
        gameStarted = true;
        spawningWave = true;
        //StartCoroutine(nameof(SpawnWave));
    }

    public IEnumerator SpawnWave()
    {
        for (int i = 0; i < _waveNumber; i++)
        {
            Vector2 pos = GetOffscreenPosition();
            var skelFighterInstance = Instantiate(skelFighter, pos, Quaternion.identity);
            _enemiesAlive.Add(skelFighterInstance.GetComponent<Health>());
            yield return new WaitForSeconds(0.1f);
        }
        
        spawningWave = false;
    }

    Vector2 GetOffscreenPosition(float margin = 2f)
    {
        Camera cam = Camera.main;          // pega da cena atual
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;  

        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: return new Vector2(-halfW - margin, Random.Range(-halfH, halfH));
            case 1: return new Vector2( halfW + margin, Random.Range(-halfH, halfH));
            case 2: return new Vector2(Random.Range(-halfW, halfW),  halfH + margin);
            default:return new Vector2(Random.Range(-halfW, halfW), -halfH - margin);
        }
    }

    //public void UnregisterEnemy(Health health)
    //{
    //    _enemiesAlive.Remove(health);
    //}
    public void UnregisterEnemy(Health health)
    {
        // Remove da lista de vivos
        _enemiesAlive.Remove(health);

        // Aumenta a contagem de mortos
        _enemiesKilled++;

        // Verifica se matou 4 (ou mais)
        if (_enemiesKilled >= 4)
        {
            Debug.Log("Vitória!");

            // Para o spawn de novas ondas para não bugar a tela de vitória
            gameStarted = false;
            spawningWave = false;

            // Chama a UI (Garanta que esse método existe no seu UIManager)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVictory();
            }
        }
    }
}
