using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //[SerializeField] private Image gameTitle;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private GameObject healthImages;
    
    private GameManager _gameManager;
  
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void HideMainMenu()
    {
        //gameTitle.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
        quitGameButton.gameObject.SetActive(false);
        
        healthImages.gameObject.SetActive(true);

    }

    public void ShowRestartButton()
    {
        restartGameButton.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

   
}
