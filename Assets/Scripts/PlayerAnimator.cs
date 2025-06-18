using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int LightAttackTrigger1 = Animator.StringToHash("LightAttackTrigger1");
    private static readonly int LightAttackTrigger2 = Animator.StringToHash("LightAttackTrigger2");

    private static readonly int Die = Animator.StringToHash("Die");
    //private static readonly int LightAttackTrigger3 = Animator.StringToHash("LightAttackTrigger3");

    private bool _lightAttackDone  = false; 
    private bool _lightAttack2Done = false; 
    private bool _deathTriggered;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private Transform playerVisual;
    
    private DamageDealer[] _hitboxes;
    private PlayerController _playerController;
    private PlayerHitEffect _playerHitEffect;
    private Health _health;

    public UnityEvent onDeathAnimEnd;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _hitboxes = GetComponentsInChildren<DamageDealer>(true);
        _health = GetComponentInParent<Health>();
        _playerHitEffect= GetComponentInParent<PlayerHitEffect>();
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        _animator.SetBool(IsWalking, _playerController.IsWalking());
        DefineSpriteDirection();
        
        if (_health.IsDead && !_deathTriggered)
        {
            _deathTriggered = true;   
            _animator.SetTrigger(Die);
            _playerHitEffect.enabled = false;
            StartCoroutine(WaitForDeathAnim());
        }
        
    }

    private void DefineSpriteDirection()
    {
        if (_playerController.InputDirection.x != 0)
        {
            float dirX = _playerController.LastMovementDirection.x;

            bool faceLeft = dirX < 0;
            
            Vector3 scale = playerVisual.localScale;
            scale.x = faceLeft ? -2f : 2f;
            playerVisual.localScale = scale;
        }
    }
  
    /* ------------------ COMBO ------------------ */

    public void TriggerLightAttack()
    {
       
        if (!_lightAttackDone && !_lightAttack2Done)
        {
            _animator.SetTrigger(LightAttackTrigger1);
        }
        
        else if (_lightAttackDone && !_lightAttack2Done)
        {
            _animator.SetTrigger(LightAttackTrigger2);
        }
       
        // else if (_lightAttackDone && _lightAttack2Done)
        // {
        //     _animator.SetTrigger(LightAttackTrigger3);
        //
        //     // Reset imediato – impede retrigger
        //     _lightAttackDone  = false;
        //     _lightAttack2Done = false;
        // }
    }
    private IEnumerator WaitForDeathAnim()
    {
        // espera entrar na state "Player_Death"
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Death"));
        
        Rigidbody2D _rb = GetComponentInParent<Rigidbody2D>();
        yield return new WaitForFixedUpdate();

        _playerController.enabled = false;    // trava movimento

        // espera terminar (normalizedTime vai de 0-1)
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);
        
        _rb.velocity = Vector2.zero;          // zera física
        _animator.speed = 0f;                 // pausa Animator
        
        onDeathAnimEnd?.Invoke();
        
    }

    /* ---------- Animation Events ---------- */

    public void EnableHitbox(int i) => _hitboxes[i].BeginSwing();
    public void DisableHitbox(int i) => _hitboxes[i].EndSwing();

    // Janela para emendar golpe 2
    public void LightAttack2Window() => _lightAttackDone = true;

    // Golpe 1 terminou sem combo
    public void LightAttack1Ended()
    {
        _lightAttackDone  = false;
        _lightAttack2Done = false; // segurança
    }

    // Golpe 2 terminou sem combo
    public void LightAttack2Ended()
    {
        _lightAttackDone  = false;
        _lightAttack2Done = false; // obrigatório
    }

    // // Janela para emendar golpe 3
    // public void LightAttack3Window() => _lightAttack2Done = true;
    //
    // // Golpe 3 terminou
    // public void LightAttack3Ended()
    // {
    //     _lightAttackDone  = false;
    //     _lightAttack2Done = false;
    // }
}
