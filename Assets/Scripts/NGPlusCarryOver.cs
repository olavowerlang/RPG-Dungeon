using UnityEngine;

/// <summary>
/// Attach to the Player root. In Start(), restores carried-over stats from NGPlusManager
/// if this is a NG+ run. Runs after GoldManager.Awake so gold is safe to set.
/// </summary>
public class NGPlusCarryOver : MonoBehaviour
{
    private void Start()
    {
        NGPlusManager.Instance?.ApplyCarryOver(GetComponent<PlayerStats>());
    }
}
