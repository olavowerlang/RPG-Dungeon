using System;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    // Variables you can adjust in the Inspector
    [SerializeField] private int level = 1;
    [SerializeField] private int xpLimit = 10;
    [SerializeField] private int xpStorage = 0;
    
    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    // This is the method the enemy will call
    // It's public so other scripts can access it
    public void GainXP(int amount)
    {
        xpStorage += amount;
        Debug.Log($"Gained {amount} XP! Total: {xpStorage}/{xpLimit}");
        
        // Checks for level up (using 'while' in case of multiple level ups)
        LevelUpCheck();
    }

    private void LevelUpCheck()
    {
        // Use 'while' in case the gained XP is enough for > 1 level
        while (xpStorage >= xpLimit)
        {
            // 1. Subtract the limit (keeping the remaining XP)
            xpStorage -= xpLimit;

            // 2. Increase the level
            level++;
            
            // 3. Double the XP limit (as you requested)
            xpLimit *= 2; // (same as xpLimit = xpLimit + xpLimit)

            _player.speed += 5f;

            Debug.Log($"LEVEL UP! Level: {level}. Next level at {xpLimit} XP. speed is now {_player.speed}");
            
            
        }
    }
}