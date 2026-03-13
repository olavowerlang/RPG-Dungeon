using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Menu Principal")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button quitGameButton;

    [Header("HUD - Vida")]
    [SerializeField] private GameObject healthImages;
    
    [Header("HUD - XP e Level")]
    [SerializeField] private Slider xpSlider; 
    [SerializeField] private TextMeshProUGUI levelText; // <--- NOVO: Arraste o texto do Nível aqui (ex: "Lvl 1")
    [SerializeField] private GameObject levelUpPanel; 
    [SerializeField] private TextMeshProUGUI levelUpText; 

    [Header("HUD - Gold")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private UnityEngine.UI.Image goldIcon;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;

    private GameManager _gameManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this); 
        else Instance = this; 
    }
    
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        healthImages.SetActive(false);
        xpSlider.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (goldText != null) goldText.gameObject.SetActive(false);
        if (goldIcon != null) goldIcon.gameObject.SetActive(false);

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(GoldManager.Instance.Gold);
        }
    }

    // Chamado quando aperta "Start Game"
    public void HideMainMenu()
    {
        startGameButton.gameObject.SetActive(false);
        quitGameButton.gameObject.SetActive(false);

        // --- CORREÇÃO: Mostra o HUD agora ---
        healthImages.gameObject.SetActive(true);
        xpSlider.gameObject.SetActive(true);
        if (levelText != null) levelText.gameObject.SetActive(true);
        if (goldText != null)
        {
            goldText.gameObject.SetActive(true);
            UpdateGoldUI(GoldManager.Instance != null ? GoldManager.Instance.Gold : 0);
        }
        if (goldIcon != null) goldIcon.gameObject.SetActive(true);

        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged += UpdateGoldUI;
    }

    // --- Atualiza Barra e Texto do Nível ---
    public void UpdateXPUI(int currentXp, int targetXp, int currentLevel)
    {
        // Atualiza a Barra
        float progress = (float)currentXp / targetXp;
        xpSlider.value = progress;

        // Atualiza o Texto ao lado da barra
        if (levelText != null)
            levelText.text = "Lvl " + currentLevel;
    }

    public void ShowLevelUpMessage(int newLevel)
    {
        StartCoroutine(LevelUpRoutine(newLevel));
    }

    private IEnumerator LevelUpRoutine(int level)
    {
        levelUpText.text = $"LEVEL UP! + SPD";
        levelUpPanel.SetActive(true);
        yield return new WaitForSeconds(3f);
        levelUpPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
    }

    public void UpdateGoldUI(int amount)
    {
        if (goldText != null)
            goldText.text = amount.ToString();
    }

    private void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= UpdateGoldUI;
    }
}