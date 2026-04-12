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
    [SerializeField] private GameObject logoImage;

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
    public static bool HudUnlocked { get; private set; } // persists across scene loads

    // ── Game-over intercept (used by CloneAI to speak before the panel shows) ─
    private static System.Action _pendingGameOver;
    private static bool          _gameOverSuppressed;

    public static void SuppressNextGameOver() => _gameOverSuppressed = true;
    public static void ResumeGameOver()
    {
        _gameOverSuppressed = false;
        _pendingGameOver?.Invoke();
        _pendingGameOver = null;
    }

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

        // Hide logo, hearts and gold before start — only on the initial load, not scene transitions
        bool isNGPlus = NGPlusManager.Instance != null && NGPlusManager.Instance.IsNGPlus;

        if (logoImage != null) logoImage.SetActive(!isNGPlus && !HudUnlocked);

        bool showHUD = isNGPlus || HudUnlocked;
        healthImages.SetActive(showHUD);
        if (goldText != null) goldText.gameObject.SetActive(showHUD);
        if (goldIcon != null) goldIcon.gameObject.SetActive(showHUD);

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(GoldManager.Instance.Gold);
        }
    }

    // Wire the Start button to this instead of HideMainMenu + StartGame separately
    public void OnStartPressed()
    {
        AudioManager.Instance?.PlayGameStart();
        HideMainMenu();
        _gameManager?.StartGame();
    }

    // Chamado quando aperta "Start Game"
    public void HideMainMenu()
    {
        HudUnlocked = true;
        startGameButton.gameObject.SetActive(false);
        quitGameButton.gameObject.SetActive(false);
        if (logoImage != null) logoImage.SetActive(false);

        // --- CORREÇÃO: Mostra o HUD agora ---
        healthImages.gameObject.SetActive(true);
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
    public void UpdateXPUI(float currentXp, int targetXp, int currentLevel)
    {
        // Atualiza a Barra
        float progress = currentXp / targetXp;
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
        levelUpText.text = "LEVEL UP! +STRENGTH";
        PositionLevelUpPanel();
        levelUpPanel.SetActive(true);
        AudioManager.Instance?.PlayLevelUp();
        yield return new WaitForSeconds(3f);
        levelUpPanel.SetActive(false);
    }

    private void PositionLevelUpPanel()
    {
        if (healthImages == null || levelUpPanel == null) return;

        var ps = PlayerStats.Instance;
        if (ps == null) return;
        var health = ps.GetComponent<Health>();
        if (health == null) return;

        int maxHp = health.MaxHP;
        if (maxHp <= 0 || maxHp > healthImages.transform.childCount) return;

        var lastHeart = healthImages.transform.GetChild(maxHp - 1) as RectTransform;
        var panelRect  = levelUpPanel.GetComponent<RectTransform>();
        if (lastHeart == null || panelRect == null) return;

        // Place panel just to the right of the last heart's center + half its width
        float halfW = lastHeart.rect.width * lastHeart.lossyScale.x * 0.5f;
        panelRect.position = new Vector3(
            lastHeart.position.x + halfW + 10f,
            lastHeart.position.y,
            panelRect.position.z
        );
    }

    public void ShowGameOver()
    {
        if (_gameOverSuppressed)
        {
            _pendingGameOver = () => Instance?.ShowGameOver();
            return;
        }
        gameOverPanel.SetActive(true);
        AudioManager.Instance?.PlayGameOver();
    }

    public void RestartGame()
    {
        _gameOverSuppressed = false;
        _pendingGameOver    = null;
        HudUnlocked = false;
        SceneManager.LoadScene("Main Scene");
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