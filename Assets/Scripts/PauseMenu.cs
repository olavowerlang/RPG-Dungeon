using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool _paused;

    private void Update()
    {
        if (!GameManager.HasStarted) return;
        if (StoreManager.Instance != null && StoreManager.Instance.IsStoreOpen) return;
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            SetPaused(!_paused);
    }

    private void SetPaused(bool paused)
    {
        _paused = paused;
        Time.timeScale = paused ? 0f : 1f;
        pausePanel.SetActive(paused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
