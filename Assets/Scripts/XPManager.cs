using System;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private float xpStorage = 0;

    private int _xpLimit;
    private PlayerStats _stats;
    private Health _health;

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
        _health = GetComponent<Health>();
        _xpLimit = _stats.startingXPLimit;
    }

    private void Start()
    {
        // Garante que a UI mostre os valores corretos ao iniciar
        UpdateUI();
    }

    public void GainXP(float amount)
    {
        xpStorage += amount;
        LevelUpCheck();
    }

    private void LevelUpCheck()
    {
        while (xpStorage >= _xpLimit)
        {
            xpStorage -= _xpLimit;
            level++;
            _xpLimit *= 2;

            _stats.AddDamage(_stats.damagePerLevel);
            _health?.HealFull();

            if(UIManager.Instance != null)
                UIManager.Instance.ShowLevelUpMessage(level);

            Debug.Log($"LEVEL UP! Level: {level}. +{_stats.damagePerLevel} DMG");
        }
        // Atualiza a UI após o loop (para garantir que a barra esvazie se subiu de nível)
        UpdateUI();
    }

    // Função auxiliar para não repetir código
    private void UpdateUI()
    {
        if(UIManager.Instance != null)
            UIManager.Instance.UpdateXPUI(xpStorage, _xpLimit, level);
    }
}