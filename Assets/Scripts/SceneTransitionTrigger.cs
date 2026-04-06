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
    [SerializeField] private bool requirePigDialogue   = false;
    [SerializeField] private bool requireMushroomTalk  = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bool isNGPlus = NGPlusManager.Instance != null && NGPlusManager.Instance.IsNGPlus;
        if (!isNGPlus)
        {
            if (requirePigDialogue  && !PigShopkeeper.MainDialogueDone) return;
            if (requireMushroomTalk && !MushroomQuestPig.TalkDone) return;
        }

        NGPlusManager.Instance?.SnapshotForTransition();
        SceneManager.LoadScene(targetScene);
    }
}
