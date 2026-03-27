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

    private void Awake()
    {
        ResolveReferences();
        DisableCollider(attackLeft);
        DisableCollider(attackRight);
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (gameInput != null)
            gameInput.OnAttack += Attack;
    }

    private void OnDisable()
    {
        if (gameInput != null)
            gameInput.OnAttack -= Attack;
    }

    private void Update()
    {
        if (gameInput == null)
            return;

        _ = gameInput.MoveVector;
    }

    private void ResolveReferences()
    {
        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInParent<SpriteRenderer>();

        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();
    }

    private void Attack()
    {
        if (attackLeft == null || attackRight == null || spriteRenderer == null)
            return;

        UpdateCombo();
        ActivateAttackTrigger();
        OnAttackCombo?.Invoke();
    }

    private void UpdateCombo()
    {
        attackCombo = attackCombo == 1 ? 2 : 1;
    }

    private void ActivateAttackTrigger()
    {
        Transform trigger = spriteRenderer.flipX ? attackLeft : attackRight;
        StartCoroutine(AttackRoutine(trigger));
    }

    private IEnumerator AttackRoutine(Transform trigger)
    {
        if (trigger == null)
            yield break;

        Collider2D col = trigger.GetComponent<Collider2D>();
        if (col == null)
            yield break;

        col.enabled = true;
        yield return new WaitForSeconds(0.15f);
        col.enabled = false;
    }

    private void DisableCollider(Transform obj)
    {
        if (obj == null)
            return;

        Collider2D col = obj.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }
}