using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerCombatController playerCombatController;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (playerHealth != null)
            playerHealth.OnTakeDamage += TakeDamage;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnTakeDamage -= TakeDamage;
    }

    private void Update()
    {
        if (gameInput == null || animator == null || spriteRenderer == null)
            return;

        UpdateFacingVisual();
        UpdateRunningAnimation();
    }

    private void ResolveReferences()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerCombatController == null)
            playerCombatController = GetComponent<PlayerCombatController>();
    }

    private void TakeDamage()
    {
        if (animator == null)
            return;

        animator.SetTrigger("Hit");
    }

    private void UpdateFacingVisual()
    {
        if (playerCombatController != null && playerCombatController.ShouldLockVisualFacing)
        {
            spriteRenderer.flipX = playerCombatController.CurrentAttackSide == PlayerAttackSide.Left;
            return;
        }

        Vector2 move = gameInput.MoveVector;

        if (move.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (move.x < -0.01f)
            spriteRenderer.flipX = true;
    }

    private void UpdateRunningAnimation()
    {
        Vector2 move = gameInput.MoveVector;
        bool isRunning = move.magnitude > 0.01f;

        if (playerCombatController != null && playerCombatController.IsAttackInProgress)
            isRunning = false;

        animator.SetBool("IsRunning", isRunning);
    }

    public void PlayComboAttack(int animationComboIndex, PlayerAttackSide attackSide)
    {
        if (animator == null)
            return;

        if (spriteRenderer != null)
            spriteRenderer.flipX = attackSide == PlayerAttackSide.Left;

        animator.SetBool("IsRunning", false);
        animator.ResetTrigger("Attack");
        animator.SetInteger("AttackCombo", Mathf.Max(1, animationComboIndex));
        animator.SetTrigger("Attack");
    }

    public void ResetCombatAnimationState()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("Attack");
        animator.SetInteger("AttackCombo", 0);
    }

    // Backward-compatible wrapper
    public void PlayBasicAttack(PlayerAttackSide attackSide)
    {
        PlayComboAttack(1, attackSide);
    }

    public void AnimationEvent_OpenCurrentAttackHitbox()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_OpenCurrentAttackHitbox();
    }

    public void AnimationEvent_CloseCurrentAttackHitbox()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_CloseCurrentAttackHitbox();
    }

    public void AnimationEvent_OpenComboInputWindow()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_OpenComboInputWindow();
    }

    public void AnimationEvent_CloseComboInputWindow()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_CloseComboInputWindow();
    }

    public void AnimationEvent_EndCurrentAttackStep()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_EndCurrentAttackStep();
    }

    public void AnimationEvent_OpenBasicAttackHitbox()
    {
        AnimationEvent_OpenCurrentAttackHitbox();
    }

    public void AnimationEvent_CloseBasicAttackHitbox()
    {
        AnimationEvent_CloseCurrentAttackHitbox();
    }

    public void AnimationEvent_EndBasicAttack()
    {
        AnimationEvent_EndCurrentAttackStep();
    }
}