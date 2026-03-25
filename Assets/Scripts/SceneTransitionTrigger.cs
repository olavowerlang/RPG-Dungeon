using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place on a trigger collider (e.g. at the cave entrance).
/// When the player walks in, loads the target scene.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SceneManager.LoadScene(targetScene);
    }
}
