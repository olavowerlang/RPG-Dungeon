using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Menu Principal")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitGameButton;

    [Header("HUD")]
    [SerializeField] private GameObject healthImages;

    [Header("Game Over")]
    // Mudança aqui: Referenciamos o PAINEL inteiro, não só o botão
    [SerializeField] private GameObject gameOverPanel;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();

        // Garante que o Game Over comece escondido, só por segurança
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
        // Ao ativar o painel, o fundo escuro, o texto e o botão aparecem juntos
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        // Reinicia a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo..."); // Útil para testar no Editor
        Application.Quit();
    }
}