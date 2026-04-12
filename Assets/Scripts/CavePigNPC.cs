using System.Collections;
using UnityEngine;

/// <summary>
/// Cave pig NPC — sleeps until player gets close, wakes up, then dialogue → transformation.
/// Requires a trigger Collider2D on this GameObject.
///
/// Animator setup needed:
///   - Default state: Sleep (looping)
///   - Trigger "WakeUp" → transitions to WakeUp state (play once)
///   - WakeUp exits to Idle automatically when done
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CavePigNPC : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData preTransformDialogue;
    [Tooltip("Index 0 = NG+,  1 = NG++,  2 = NG+++  (last slot reused for higher tiers)")]
    [SerializeField] private DialogueData[] ngPlusPreTransformDialogues;

    [Header("References")]
    [SerializeField] private TransformationSequence transformationSequence;
    [SerializeField] private GameObject             interactionPrompt;
    [SerializeField] private Animator               pigAnimator;

    [Header("Animator Parameters")]
    [SerializeField] private string wakeUpTrigger  = "WakeUp";
    [SerializeField] private string wakeUpStateName = "WakeUp"; // exact state name to wait for

    [Header("Detection")]
    [SerializeField] private float wakeUpRange = 4f; // distance at which pig wakes (can differ from collider size)

    // ── State ─────────────────────────────────────────────────────────────────

    private enum PigState { Sleeping, WakingUp, Idle, DialogueDone }
    private PigState _state = PigState.Sleeping;

    private Transform _player;
    private bool _canCheckWake;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private IEnumerator Start()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            _player = playerGO.transform;
        else
            Debug.LogError("CavePigNPC: No GameObject tagged 'Player' found!", this);

        if (pigAnimator == null)
            Debug.LogError("CavePigNPC: pigAnimator is not assigned in the Inspector!", this);

        yield return new WaitForSeconds(0.5f);
        _canCheckWake = true;
        Debug.Log("CavePigNPC: ready to check wake distance.");
    }

    private void Update()
    {
        switch (_state)
        {
            case PigState.Sleeping:
                CheckWakeDistance();
                break;

            case PigState.Idle:
                UpdateIdlePrompt();
                break;
        }
    }

    // ── Logic ─────────────────────────────────────────────────────────────────

    private void CheckWakeDistance()
    {
        if (!_canCheckWake || _player == null) return;
        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist <= wakeUpRange)
        {
            Debug.Log($"CavePigNPC: player in range ({dist:F2}), starting WakeUpRoutine.");
            StartCoroutine(WakeUpRoutine());
        }
    }

    private IEnumerator WakeUpRoutine()
    {
        _state = PigState.WakingUp;
        Debug.Log("CavePigNPC: WakeUpRoutine started.");

        if (pigAnimator != null)
        {
            Debug.Log($"CavePigNPC: firing trigger '{wakeUpTrigger}'. Current state: {pigAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash}");
            pigAnimator.SetTrigger(wakeUpTrigger);

            // Give the animator two frames to process the trigger and start transitioning
            yield return null;
            yield return null;

            Debug.Log($"CavePigNPC: after 2 frames, IsName('{wakeUpStateName}') = {pigAnimator.GetCurrentAnimatorStateInfo(0).IsName(wakeUpStateName)}");

            // Wait for WakeUp state to begin (in case transition takes a bit longer)
            float waitTimeout = 1f;
            while (!pigAnimator.GetCurrentAnimatorStateInfo(0).IsName(wakeUpStateName) && waitTimeout > 0f)
            {
                waitTimeout -= Time.deltaTime;
                yield return null;
            }

            if (waitTimeout <= 0f)
                Debug.LogWarning($"CavePigNPC: timed out waiting for '{wakeUpStateName}' state. Check the Animator Controller setup.");
            else
                Debug.Log("CavePigNPC: WakeUp state confirmed, waiting for animation to finish.");

            // Wait for the WakeUp animation to finish
            yield return new WaitUntil(() =>
                pigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f
                || !pigAnimator.GetCurrentAnimatorStateInfo(0).IsName(wakeUpStateName));
        }

        Debug.Log("CavePigNPC: now Idle.");
        _state = PigState.Idle;
    }

    private void UpdateIdlePrompt()
    {
        if (DialogueManager.Instance == null) return;

        bool canInteract = _player != null &&
                           Vector2.Distance(transform.position, _player.position) <= wakeUpRange &&
                           !DialogueManager.Instance.IsInDialogue;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);

        if (canInteract && Input.GetKeyDown(KeyCode.E))
            StartPreTransformDialogue();
    }

    private void StartPreTransformDialogue()
    {
        _state = PigState.DialogueDone;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        DialogueData dialogue = NGPlusManager.PickDialogue(preTransformDialogue, ngPlusPreTransformDialogues);

        if (dialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(dialogue, () => transformationSequence.StartTransformation());
        else
            transformationSequence.StartTransformation();
    }
}
