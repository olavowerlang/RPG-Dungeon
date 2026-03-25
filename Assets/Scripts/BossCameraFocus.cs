using System.Collections;
using Cinemachine;
using UnityEngine;

/// <summary>
/// Manages the brief camera focus on the clone during the Phase 2 pause.
/// Assign the Phase 2 virtual camera (pointed at the clone) in the Inspector.
/// That camera should have a lower priority than the player-follow camera at start.
/// When Focus() is called it bumps priority above the player cam; Unfocus() restores it.
/// </summary>
public class BossCameraFocus : MonoBehaviour
{
    public static BossCameraFocus Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera phase2Cam;   // drag the clone-focus vcam here
    [SerializeField] private int                      focusPriority   = 20;  // above player cam (usually 10)
    [SerializeField] private int                      defaultPriority = 5;
    [SerializeField] private float                    blendInTime     = 0.6f; // wait after raising priority before dialogue
    [SerializeField] private float                    blendOutTime    = 0.6f; // wait after lowering priority

    private void Awake()
    {
        Instance = this;
        if (phase2Cam != null)
            phase2Cam.Priority = defaultPriority;
    }

    /// <summary>
    /// Blends camera to the clone, waits, then calls onReady so dialogue can start.
    /// </summary>
    public void Focus(System.Action onReady)
    {
        StartCoroutine(FocusRoutine(onReady));
    }

    /// <summary>
    /// Blends camera back to the player, then calls onDone.
    /// </summary>
    public void Unfocus(System.Action onDone)
    {
        StartCoroutine(UnfocusRoutine(onDone));
    }

    private IEnumerator FocusRoutine(System.Action onReady)
    {
        if (phase2Cam != null) phase2Cam.Priority = focusPriority;
        yield return new WaitForSeconds(blendInTime);
        onReady?.Invoke();
    }

    private IEnumerator UnfocusRoutine(System.Action onDone)
    {
        if (phase2Cam != null) phase2Cam.Priority = defaultPriority;
        yield return new WaitForSeconds(blendOutTime);
        onDone?.Invoke();
    }
}
