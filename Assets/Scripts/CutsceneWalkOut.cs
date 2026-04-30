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

    private bool       _forcingWalk;
    private Animator[] _anims1;
    private Animator[] _anims2;

    private void Awake()
    {
        SetOverlayAlpha(1f);
        if (creditsText != null) creditsText.alpha = 0f;
    }

    private void Start()
    {
        _anims1 = Gather(character1);
        _anims2 = Gather(character2);

        // Kill every script that touches animation or movement on both characters
        foreach (Transform ch in new[] { character1, character2 })
        {
            if (ch == null) continue;
            DisableAll<PlayerAnimator>(ch);
            DisableAll<PlayerController>(ch);
            DisableAll<PlayerInput>(ch);
            DisableAll<CloneAI>(ch);
            DisableAll<CloneAnimator>(ch);
        }

        // Unfreeze and reset all animators
        foreach (var anim in _anims1) Reset(anim);
        foreach (var anim in _anims2) Reset(anim);

        _forcingWalk = true;
        StartCoroutine(CutsceneRoutine());
    }

    private void LateUpdate()
    {
        if (!_forcingWalk) return;
        ForceWalkDown(_anims1);
        ForceWalkDown(_anims2);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Animator[] Gather(Transform root)
    {
        if (root == null) return new Animator[0];
        return root.GetComponentsInChildren<Animator>(true);
    }

    private static void DisableAll<T>(Transform root) where T : MonoBehaviour
    {
        foreach (var c in root.GetComponentsInChildren<T>(true))
            { c.StopAllCoroutines(); c.enabled = false; }
        foreach (var c in root.GetComponentsInParent<T>(true))
            { c.StopAllCoroutines(); c.enabled = false; }
    }

    private static void Reset(Animator anim)
    {
        if (anim == null) return;
        anim.speed = 1f;
        anim.Rebind();
        anim.Update(0f);
    }

    private static void ForceWalkDown(Animator[] anims)
    {
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

    // ── cutscene ─────────────────────────────────────────────────────────────

    private IEnumerator CutsceneRoutine()
    {
        float fadeElapsed = 0f;
        float walkElapsed = 0f;

        // Phase 1: walk + fade in simultaneously
        while (walkElapsed < walkDuration)
        {
            float dt = Time.deltaTime;
            walkElapsed  += dt;
            fadeElapsed  += dt;
            SetOverlayAlpha(Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(fadeElapsed / fadeInTime)));
            if (character1 != null) character1.Translate(Vector3.down * walkSpeed * dt);
            if (character2 != null) character2.Translate(Vector3.down * walkSpeed * dt);
            yield return null;
        }
        SetOverlayAlpha(0f);

        // Phase 2: keep walking during fade to black
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

        // Screen black — stop walking
        _forcingWalk = false;
        foreach (var anim in _anims1) if (anim != null) anim.SetBool(IsWalkingHash, false);
        foreach (var anim in _anims2) if (anim != null) anim.SetBool(IsWalkingHash, false);

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
        Color c = blackOverlay.color;
        c.a = a;
        blackOverlay.color = c;
    }
}
