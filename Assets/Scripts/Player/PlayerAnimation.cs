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
            playerHealth = FindFirstObjectByType<PlayerHealth>();

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

    public void PlayBasicAttack(PlayerAttackSide attackSide)
    {
        if (animator == null)
            return;

        if (spriteRenderer != null)
            spriteRenderer.flipX = attackSide == PlayerAttackSide.Left;

        animator.SetBool("IsRunning", false);
        animator.SetInteger("AttackCombo", 1);
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");
    }

    // Animation Events

    public void AnimationEvent_OpenBasicAttackHitbox()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_OpenBasicAttackHitbox();
    }

    public void AnimationEvent_CloseBasicAttackHitbox()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_CloseBasicAttackHitbox();
    }

    public void AnimationEvent_EndBasicAttack()
    {
        if (playerCombatController != null)
            playerCombatController.AnimationEvent_EndBasicAttack();
    }
}