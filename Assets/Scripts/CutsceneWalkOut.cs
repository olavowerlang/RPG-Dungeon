using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneWalkOut : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Transform character1;
    [SerializeField] private Transform character2;
    [SerializeField] private Animator  animator1;
    [SerializeField] private Animator  animator2;

    [Header("Walk Settings")]
    [SerializeField] private float walkSpeed    = 1.5f;
    [SerializeField] private float walkDuration = 4f;

    [Header("UI")]
    [SerializeField] private Image           blackOverlay;
    [SerializeField] private TextMeshProUGUI creditsText;

    [Header("Timing")]
    [SerializeField] private float fadeInTime         = 1.5f;
    [SerializeField] private float fadeOutTime        = 2f;
    [SerializeField] private float creditsDisplayTime = 5f;

    [Header("Scene")]
    [SerializeField] private string mainSceneName = "Main Scene";

    private static readonly int IsWalkingHash  = Animator.StringToHash("isWalking");
    private static readonly int DirXHash       = Animator.StringToHash("DirX");
    private static readonly int DirYHash       = Animator.StringToHash("DirY");
    private static readonly int PlayerWalkHash = Animator.StringToHash("Player_Walk");

    private bool          _forcingWalk;
    private CloneAnimator _cloneAnim1;
    private CloneAnimator _cloneAnim2;
    private Animator[]    _anims1;
    private Animator[]    _anims2;

    private void Awake()
    {
        SetOverlayAlpha(1f);
        if (creditsText != null) creditsText.alpha = 0f;
    }

    private void Start()
    {
        // Collect ALL animators on each character — avoids picking the wrong one when there are multiple
        _anims1 = character1 != null ? character1.GetComponentsInChildren<Animator>(true) : new Animator[0];
        _anims2 = character2 != null ? character2.GetComponentsInChildren<Animator>(true) : new Animator[0];

        // Also keep inspector refs as fallback (backward-compat with scene wiring)
        if (animator1 == null && _anims1.Length > 0) animator1 = _anims1[0];
        if (animator2 == null && _anims2.Length > 0) animator2 = _anims2[0];

        // Find CloneAnimator on both characters (drives the correct internal Animator ref)
        if (character1 != null)
            _cloneAnim1 = character1.GetComponentInChildren<CloneAnimator>(true)
                       ?? character1.GetComponentInParent<CloneAnimator>(true);
        if (character2 != null)
            _cloneAnim2 = character2.GetComponentInChildren<CloneAnimator>(true)
                       ?? character2.GetComponentInParent<CloneAnimator>(true);

        // Stop coroutines AND disable — just disabling leaves coroutines running in Unity
        foreach (Transform ch in new[] { character1, character2 })
        {
            if (ch == null) continue;
            foreach (var s in ch.GetComponentsInChildren<PlayerAnimator>(true))
                { s.StopAllCoroutines(); s.enabled = false; }
            foreach (var s in ch.GetComponentsInParent<PlayerAnimator>(true))
                { s.StopAllCoroutines(); s.enabled = false; }
            foreach (var s in ch.GetComponentsInChildren<CloneAI>(true))
                { s.StopAllCoroutines(); s.enabled = false; }
            foreach (var s in ch.GetComponentsInParent<CloneAI>(true))
                { s.StopAllCoroutines(); s.enabled = false; }
        }

        // Unfreeze and rebind ALL animators on both characters
        foreach (var anim in _anims1) { if (anim != null) { anim.speed = 1f; anim.Rebind(); anim.Update(0f); } }
        foreach (var anim in _anims2) { if (anim != null) { anim.speed = 1f; anim.Rebind(); anim.Update(0f); } }

        _forcingWalk = true;
        StartCoroutine(CutsceneRoutine());
    }

    // Runs after ALL Updates — wins over PlayerAnimator/CloneAI every frame
    private void LateUpdate()
    {
        if (!_forcingWalk) return;
        ForceWalkDown(_anims1, _cloneAnim1);
        ForceWalkDown(_anims2, _cloneAnim2);
    }

    private void ForceWalkDown(Animator[] anims, CloneAnimator cloneAnim)
    {
        if (cloneAnim != null)
        {
            cloneAnim.SetWalking(true);
            cloneAnim.SetDirection(Vector2.down);
        }

        if (anims == null) return;
        foreach (var anim in anims)
        {
            if (anim == null) continue;
            anim.speed = 1f;
            anim.SetBool(IsWalkingHash, true);
            anim.SetFloat(DirXHash, 0f);
            anim.SetFloat(DirYHash, -1f);
            if (!anim.GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(PlayerWalkHash))
                anim.Play("Player_Walk", 0);
        }
    }

    private IEnumerator CutsceneRoutine()
    {
        float fadeElapsed = 0f;
        float walkElapsed = 0f;

        // Phase 1: walk + fade in simultaneously
        while (walkElapsed < walkDuration)
        {
            float dt    = Time.deltaTime;
            walkElapsed += dt;

            if (fadeElapsed < fadeInTime)
            {
                fadeElapsed += dt;
                SetOverlayAlpha(Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(fadeElapsed / fadeInTime)));
            }
            else
            {
                SetOverlayAlpha(0f);
            }

            if (character1 != null) character1.Translate(Vector3.down * walkSpeed * dt);
            if (character2 != null) character2.Translate(Vector3.down * walkSpeed * dt);
            yield return null;
        }
        SetOverlayAlpha(0f);

        // Phase 2: keep walking AND moving during fade to black — characters never visibly stop
        float fadeOut = 0f;
        while (fadeOut < fadeOutTime)
        {
            float dt = Time.unscaledDeltaTime;
            fadeOut += dt;
            SetOverlayAlpha(Mathf.SmoothStep(0f, 1f, fadeOut / fadeOutTime));
            if (character1 != null) character1.Translate(Vector3.down * walkSpeed * Time.deltaTime);
            if (character2 != null) character2.Translate(Vector3.down * walkSpeed * Time.deltaTime);
            yield return null;
        }
        SetOverlayAlpha(1f);

        // Screen is fully black — stop
        _forcingWalk = false;
        foreach (var anim in _anims1) if (anim != null) anim.SetBool(IsWalkingHash, false);
        foreach (var anim in _anims2) if (anim != null) anim.SetBool(IsWalkingHash, false);
        _cloneAnim1?.SetWalking(false);
        _cloneAnim2?.SetWalking(false);

        // Credits
        if (creditsText != null)
        {
            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                creditsText.alpha = Mathf.Clamp01(t / fadeOutTime);
                yield return null;
            }
            creditsText.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(creditsDisplayTime);
        SceneManager.LoadScene(mainSceneName);
    }

    private void SetOverlayAlpha(float a)
    {
        if (blackOverlay == null) return;
        Color c  = blackOverlay.color;
        c.a      = a;
        blackOverlay.color = c;
    }
}
