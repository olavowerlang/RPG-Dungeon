using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image gameTitle;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private GameObject healthImages;
    
    
    private GameManager _gameManager;
  
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void HideMainMenu()
    {
        gameTitle.enabled = false;
        startGameButton.enabled = false;
        quitGameButton.enabled = false;
        
        startGameButton.image.enabled = false;
        quitGameButton.image.enabled = false;
        
        healthImages.gameObject.SetActive(true);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

   
}
