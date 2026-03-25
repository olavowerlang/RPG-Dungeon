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
    [SerializeField] private DialogueData ngPlusPreTransformDialogue;

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

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;
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
        if (_player == null) return;
        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist <= wakeUpRange)
            StartCoroutine(WakeUpRoutine());
    }

    private IEnumerator WakeUpRoutine()
    {
        _state = PigState.WakingUp;

        if (pigAnimator != null)
        {
            pigAnimator.SetTrigger(wakeUpTrigger);

            // Wait for WakeUp animation to start
            yield return null;
            yield return new WaitUntil(() =>
                pigAnimator.GetCurrentAnimatorStateInfo(0).IsName(wakeUpStateName));

            // Wait for it to finish
            yield return new WaitUntil(() =>
                pigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        }

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

        bool isNGPlus = NGPlusManager.Instance != null && NGPlusManager.Instance.GameCleared;
        DialogueData dialogue = isNGPlus && ngPlusPreTransformDialogue != null
            ? ngPlusPreTransformDialogue
            : preTransformDialogue;

        if (dialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(dialogue, () => transformationSequence.StartTransformation());
        else
            transformationSequence.StartTransformation();
    }
}
