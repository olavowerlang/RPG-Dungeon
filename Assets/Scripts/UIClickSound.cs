using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add to any Button to play the UI click sound automatically.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => AudioManager.Instance?.PlayGenericClick());
    }
}
