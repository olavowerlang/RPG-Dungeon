using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Menu Principal")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitGameButton;

    [Header("HUD")]
    [SerializeField] private GameObject healthImages;

    [Header("Game Over")]
    // Muda aqui: Referenciamos o PAINEL inteiro, nao so o botao
    [SerializeField] private GameObject gameOverPanel;

    private GameManager _gameManager;

    private void Awake()
        {
            // Se já existir uma instância (e não for essa), destrói a duplicata
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            }
        }
    
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();

        // Garante que o Game Over comece escondido, so por seguranca
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void HideMainMenu()
    {
        startGameButton.gameObject.SetActive(false);
        quitGameButton.gameObject.SetActive(false);

        healthImages.gameObject.SetActive(true);
    }

    // Renomeei para ficar mais claro
    public void ShowGameOver()
    {
        // Ao ativar o painel, o fundo escuro, o texto e o botao aparecem juntos
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        // Reinicia a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo..."); // util para testar no Editor
        Application.Quit();
    }
}