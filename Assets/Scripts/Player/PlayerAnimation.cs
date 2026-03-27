using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerAttackSystem playerAttackSystem;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (playerHealth != null)
            playerHealth.OnTakeDamage += TakeDamage;

        if (playerAttackSystem != null)
            playerAttackSystem.OnAttackCombo += AttackAnimation;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnTakeDamage -= TakeDamage;

        if (playerAttackSystem != null)
            playerAttackSystem.OnAttackCombo -= AttackAnimation;
    }

    private void Update()
    {
        if (gameInput == null || animator == null || spriteRenderer == null)
            return;

        FlipSprite();
        RunningAnimation();
    }

    private void ResolveReferences()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();

        if (playerAttackSystem == null)
            playerAttackSystem = GetComponentInChildren<PlayerAttackSystem>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void TakeDamage()
    {
        animator.SetTrigger("Hit");
    }

    private void FlipSprite()
    {
        Vector2 move = gameInput.MoveVector;

        if (move.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (move.x < -0.01f)
            spriteRenderer.flipX = true;
    }

    private void RunningAnimation()
    {
        Vector2 move = gameInput.MoveVector;
        bool isRunning = move.magnitude > 0.01f;
        animator.SetBool("IsRunning", isRunning);
    }

    private void AttackAnimation()
    {
        animator.SetTrigger("Attack");
        animator.SetInteger("AttackCombo", playerAttackSystem.AttackCombo);
    }
}