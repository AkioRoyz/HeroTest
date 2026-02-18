using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] GameInput gameInput;
    [SerializeField] PlayerAttackSystem playerAttackSystem;

    private bool IsRunning;

    private void Update()
    {
        FlipSprite();
        RunningAnimation();
    }

    private void OnEnable()
    {
        playerHealth.OnTakeDamage += TakeDamage;
        playerAttackSystem.OnAttackCombo += AttackAnimation;
    }

    private void OnDisable()
    {
        playerHealth.OnTakeDamage -= TakeDamage;
        playerAttackSystem.OnAttackCombo -= AttackAnimation;
    }

    private void TakeDamage()
    {
        animator.SetTrigger("Hit");
    }
    private void FlipSprite()
    {
        Vector2 move = gameInput.moveVector;

        if (move.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (move.x < -0.01f)
            spriteRenderer.flipX = true;
    }

    private void RunningAnimation()
    {
        Vector2 move = gameInput.moveVector;

        bool isRunning = move.magnitude > 0.01f;
        animator.SetBool("IsRunning", isRunning);
    }

    private void AttackAnimation()
    {
        animator.SetTrigger("Attack");
        animator.SetInteger("AttackCombo", playerAttackSystem.AttackCombo);
    }
}
