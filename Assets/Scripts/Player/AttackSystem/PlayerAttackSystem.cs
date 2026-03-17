using System;
using System.Collections;
using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    private int attackCombo = 1;
    public int AttackCombo => attackCombo;
    public event Action OnAttackCombo;

    [SerializeField] private GameInput gameInput;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Attack Triggers")]
    [SerializeField] private Transform attackLeft;
    [SerializeField] private Transform attackRight;

    private Vector2 lastMoveDirection = Vector2.down;

    private void Awake()
    {
        DisableCollider(attackLeft);
        DisableCollider(attackRight);
    }

    private void OnEnable()
    {
        gameInput.OnAttack += Attack;
    }

    private void OnDisable()
    {
        gameInput.OnAttack -= Attack;
    }

    private void Update()
    {
        Vector2 move = gameInput.MoveVector;

        if (move != Vector2.zero)
            lastMoveDirection = move;
    }

    private void Attack()
    {
        UpdateCombo();
        ActivateAttackTrigger();
        OnAttackCombo?.Invoke();
    }

    private void UpdateCombo()
    {
        if (attackCombo == 1)
            attackCombo++;
        else
            attackCombo = 1;
    }

    private void ActivateAttackTrigger()
    {
        Transform trigger = null;

        if (spriteRenderer.flipX)
            trigger = attackLeft;
        else
            trigger = attackRight;

        StartCoroutine(AttackRoutine(trigger));
    }

    private IEnumerator AttackRoutine(Transform trigger)
    {
        Collider2D col = trigger.GetComponent<Collider2D>();

        col.enabled = true;
        yield return new WaitForSeconds(0.15f);
        col.enabled = false;
    }

    private void DisableCollider(Transform obj)
    {
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }
}