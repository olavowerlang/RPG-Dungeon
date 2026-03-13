using UnityEngine;

/// <summary>
/// Base class for all pig shop gates.
/// Add a subclass component to MainNPCInstance2 to require a condition before the store opens.
/// PigShopkeeper evaluates gates in the order of its serialized array — first uncleared gate
/// is activated. Once all gates are cleared, the store opens normally.
/// </summary>
public abstract class ShopGate : MonoBehaviour
{
    public abstract bool IsCleared { get; }

    /// <summary>
    /// Called when the player tries to open the store and this gate is not yet cleared.
    /// Should show dialogue, prompt, or do whatever the gate requires.
    /// </summary>
    public abstract void Activate();
}
