using UnityEngine;

/// <summary>
/// Place one of these in every scene.
/// Tells AudioManager what audio to start when this scene loads.
/// Fires in Start() — after OnSceneLoaded has already cleared all audio.
/// </summary>
public class SceneAudioStarter : MonoBehaviour
{
    public enum SceneAudioType { StartArea, Cave, Combat, Shop, None }

    [SerializeField] private SceneAudioType audioType = SceneAudioType.StartArea;

    private void Start()
    {
        switch (audioType)
        {
            case SceneAudioType.StartArea:
                AudioManager.Instance?.PlayStartAreaAmbient();
                break;
            case SceneAudioType.Cave:
                AudioManager.Instance?.PlayCaveEntrance();
                break;
            case SceneAudioType.Combat:
                AudioManager.Instance?.PlayCombatMusic();
                break;
            case SceneAudioType.Shop:
                AudioManager.Instance?.PlayShopMusic();
                break;
            case SceneAudioType.None:
                break;
        }
    }
}
