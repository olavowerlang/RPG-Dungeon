using System;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int xpLimit = 10;
    [SerializeField] private int xpStorage = 0;
    
    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        // Garante que a UI mostre os valores corretos ao iniciar
        UpdateUI();
    }

    public void GainXP(int amount)
    {
        xpStorage += amount;
        
        LevelUpCheck();
        
        UpdateUI();
    }

    private void LevelUpCheck()
    {
        while (xpStorage >= xpLimit)
        {
            xpStorage -= xpLimit;
            level++;
            xpLimit *= 2; 

            _player.speed += 5f;

            if(UIManager.Instance != null)
                UIManager.Instance.ShowLevelUpMessage(level);

            Debug.Log($"LEVEL UP! Level: {level}. + SPD");
        }
        // Atualiza a UI após o loop (para garantir que a barra esvazie se subiu de nível)
        UpdateUI();
    }

    // Função auxiliar para não repetir código
    private void UpdateUI()
    {
        if(UIManager.Instance != null) 
            UIManager.Instance.UpdateXPUI(xpStorage, xpLimit, level);
    }
}