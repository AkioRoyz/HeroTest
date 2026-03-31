using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerMoving playerMoving;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private StatsSystem statsSystem;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerTargetingSystem playerTargetingSystem;

    [Header("Attack Hitboxes")]
    [SerializeField] private PlayerCombatHitbox leftHitbox;
    [SerializeField] private PlayerCombatHitbox rightHitbox;

    [Header("Basic Attack Settings")]
    [SerializeField] private bool lockMovementDuringAttack = true;
    [SerializeField] private float attackFailSafeDuration = 0.70f;

    [Header("Damage Formula")]
    [SerializeField] private float strengthDamageMultiplier = 1.00f;
    [SerializeField] private int flatDamageBonus = 0;
    [SerializeField] private float randomDamageMinMultiplier = 0.90f;
    [SerializeField] private float randomDamageMaxMultiplier = 1.10f;

    [Header("Damage Metadata")]
    [SerializeField] private float knockbackForce = 0f;
    [SerializeField] private float stunDuration = 0f;
    [SerializeField] private bool canBeBlocked = true;
    [SerializeField] private bool canBeParried = true;
    [SerializeField] private string attackId = "player_basic_attack_01";

    [Header("Facing")]
    [SerializeField] private Vector2 defaultFacingDirection = Vector2.right;
    [SerializeField] private float minHorizontalInputToChangeFacing = 0.05f;

    private readonly HashSet<int> hitTargetIds = new HashSet<int>();

    private bool isAttackInProgress;
    private bool isHitboxOpen;

    private Vector2 lastNonZeroMoveDirection = Vector2.right;
    private PlayerAttackSide lastFacingSide = PlayerAttackSide.Right;
    private PlayerAttackSide currentAttackSide = PlayerAttackSide.Right;

    private Coroutine attackFailSafeCoroutine;

    public event Action<PlayerAttackSide> OnBasicAttackStarted;
    public event Action<PlayerAttackSide> OnBasicAttackFinished;

    public bool IsAttackInProgress => isAttackInProgress;
    public bool IsHitboxOpen => isHitboxOpen;
    public bool ShouldLockVisualFacing => isAttackInProgress;
    public PlayerAttackSide CurrentAttackSide => isAttackInProgress ? currentAttackSide : lastFacingSide;

    public Vector2 CurrentAttackDirection
    {
        get
        {
            return CurrentAttackSide == PlayerAttackSide.Left ? Vector2.left : Vector2.right;
        }
    }

    private void Awake()
    {
        ResolveReferences();
        DisableAllHitboxesImmediate();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (gameInput != null)
            gameInput.OnAttackStarted += HandleAttackStarted;
    }

    private void OnDisable()
    {
        if (gameInput != null)
            gameInput.OnAttackStarted -= HandleAttackStarted;

        ForceStopAllCombatState();
    }

    private void Update()
    {
        UpdateLastFacingFromInput();
    }

    private void ResolveReferences()
    {
        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();

        if (playerAnimation == null)
            playerAnimation = GetComponent<PlayerAnimation>();

        if (playerMoving == null)
            playerMoving = GetComponent<PlayerMoving>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (statsSystem == null)
            statsSystem = FindFirstObjectByType<StatsSystem>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerTargetingSystem == null)
            playerTargetingSystem = GetComponent<PlayerTargetingSystem>();
    }

    public void RegisterHitbox(PlayerCombatHitbox hitbox)
    {
        if (hitbox == null)
            return;

        if (hitbox.Side == PlayerAttackSide.Left)
            leftHitbox = hitbox;
        else
            rightHitbox = hitbox;
    }

    private void UpdateLastFacingFromInput()
    {
        if (gameInput == null)
            return;

        Vector2 move = gameInput.MoveVector;

        if (move.sqrMagnitude > 0.0001f)
            lastNonZeroMoveDirection = move.normalized;

        if (move.x > minHorizontalInputToChangeFacing)
            lastFacingSide = PlayerAttackSide.Right;
        else if (move.x < -minHorizontalInputToChangeFacing)
            lastFacingSide = PlayerAttackSide.Left;
    }

    private void HandleAttackStarted()
    {
        TryStartBasicAttack();
    }

    public bool TryStartBasicAttack()
    {
        if (!CanStartBasicAttack())
            return false;

        currentAttackSide = ResolveAttackSideForNewAttack();
        isAttackInProgress = true;
        isHitboxOpen = false;
        hitTargetIds.Clear();

        DisableAllHitboxesImmediate();

        if (lockMovementDuringAttack && playerMoving != null)
            playerMoving.SetExternalMovementBlocked(true);

        StartAttackFailSafeTimer();

        if (playerAnimation != null)
            playerAnimation.PlayBasicAttack(currentAttackSide);

        OnBasicAttackStarted?.Invoke(currentAttackSide);
        return true;
    }

    private bool CanStartBasicAttack()
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return false;

        if (isAttackInProgress)
            return false;

        if (playerHealth != null && !playerHealth.IsAlive)
            return false;

        return true;
    }

    private PlayerAttackSide ResolveAttackSideForNewAttack()
    {
        PlayerAttackSide targetFallbackSide = lastFacingSide;

        if (playerTargetingSystem != null &&
            playerTargetingSystem.TryGetPreferredAttackSide(targetFallbackSide, out PlayerAttackSide targetedSide))
        {
            return targetedSide;
        }

        if (gameInput != null)
        {
            Vector2 move = gameInput.MoveVector;

            if (move.x > minHorizontalInputToChangeFacing)
                return PlayerAttackSide.Right;

            if (move.x < -minHorizontalInputToChangeFacing)
                return PlayerAttackSide.Left;
        }

        if (lastNonZeroMoveDirection.x > minHorizontalInputToChangeFacing)
            return PlayerAttackSide.Right;

        if (lastNonZeroMoveDirection.x < -minHorizontalInputToChangeFacing)
            return PlayerAttackSide.Left;

        if (spriteRenderer != null)
            return spriteRenderer.flipX ? PlayerAttackSide.Left : PlayerAttackSide.Right;

        return defaultFacingDirection.x < 0f ? PlayerAttackSide.Left : PlayerAttackSide.Right;
    }

    public void AnimationEvent_OpenBasicAttackHitbox()
    {
        if (!isAttackInProgress)
            return;

        isHitboxOpen = true;

        PlayerCombatHitbox activeHitbox = GetActiveHitboxForCurrentAttack();
        if (activeHitbox != null)
            activeHitbox.SetHitboxActive(true);
    }

    public void AnimationEvent_CloseBasicAttackHitbox()
    {
        isHitboxOpen = false;
        DisableAllHitboxesImmediate();
    }

    public void AnimationEvent_EndBasicAttack()
    {
        FinishAttack();
    }

    public void NotifyHitboxTriggered(PlayerCombatHitbox hitbox, Collider2D other)
    {
        if (!isAttackInProgress)
            return;

        if (!isHitboxOpen)
            return;

        if (hitbox == null || other == null)
            return;

        if (hitbox != GetActiveHitboxForCurrentAttack())
            return;

        if (!TryGetCombatReceiver(other, out ICombatReceiver receiver))
            return;

        if (receiver == null || !receiver.IsAlive)
            return;

        if (receiver.Team == CombatTeam.Player)
            return;

        Transform receiverTransform = receiver.Transform;
        if (receiverTransform == null)
            return;

        int targetId = receiverTransform.GetInstanceID();
        if (hitTargetIds.Contains(targetId))
            return;

        hitTargetIds.Add(targetId);

        Vector3 hitPoint = hitbox.GetBestHitPoint(other);
        DamageInfo damageInfo = BuildDamageInfo(hitPoint);

        receiver.ReceiveDamage(damageInfo);
    }

    private DamageInfo BuildDamageInfo(Vector3 hitPoint)
    {
        int strength = statsSystem != null ? Mathf.Max(1, statsSystem.Strength) : 1;

        float minMultiplier = Mathf.Min(randomDamageMinMultiplier, randomDamageMaxMultiplier);
        float maxMultiplier = Mathf.Max(randomDamageMinMultiplier, randomDamageMaxMultiplier);

        float randomizedMultiplier = UnityEngine.Random.Range(minMultiplier, maxMultiplier);
        int calculatedDamage = Mathf.RoundToInt((strength * strengthDamageMultiplier + flatDamageBonus) * randomizedMultiplier);
        calculatedDamage = Mathf.Max(1, calculatedDamage);

        return new DamageInfo(
            source: gameObject,
            sourceTeam: CombatTeam.Player,
            damage: calculatedDamage,
            direction: CurrentAttackDirection,
            hitPoint: hitPoint,
            knockbackForce: knockbackForce,
            stunDuration: stunDuration,
            canBeBlocked: canBeBlocked,
            canBeParried: canBeParried,
            ignoresInvulnerability: false,
            isCritical: false,
            isSpecial: false,
            hitReactionType: HitReactionType.Light,
            attackId: attackId
        );
    }

    private PlayerCombatHitbox GetActiveHitboxForCurrentAttack()
    {
        return currentAttackSide == PlayerAttackSide.Left ? leftHitbox : rightHitbox;
    }

    private void DisableAllHitboxesImmediate()
    {
        if (leftHitbox != null)
            leftHitbox.ForceDisable();

        if (rightHitbox != null)
            rightHitbox.ForceDisable();
    }

    private void FinishAttack()
    {
        if (!isAttackInProgress && !isHitboxOpen)
            return;

        StopAttackFailSafeTimer();

        isAttackInProgress = false;
        isHitboxOpen = false;
        hitTargetIds.Clear();

        DisableAllHitboxesImmediate();

        if (lockMovementDuringAttack && playerMoving != null)
            playerMoving.SetExternalMovementBlocked(false);

        OnBasicAttackFinished?.Invoke(currentAttackSide);
    }

    private void ForceStopAllCombatState()
    {
        StopAttackFailSafeTimer();

        isAttackInProgress = false;
        isHitboxOpen = false;
        hitTargetIds.Clear();

        DisableAllHitboxesImmediate();

        if (playerMoving != null)
            playerMoving.SetExternalMovementBlocked(false);
    }

    private void StartAttackFailSafeTimer()
    {
        StopAttackFailSafeTimer();
        attackFailSafeCoroutine = StartCoroutine(AttackFailSafeRoutine());
    }

    private void StopAttackFailSafeTimer()
    {
        if (attackFailSafeCoroutine != null)
        {
            StopCoroutine(attackFailSafeCoroutine);
            attackFailSafeCoroutine = null;
        }
    }

    private IEnumerator AttackFailSafeRoutine()
    {
        yield return new WaitForSeconds(attackFailSafeDuration);
        attackFailSafeCoroutine = null;
        FinishAttack();
    }

    private bool TryGetCombatReceiver(Collider2D other, out ICombatReceiver receiver)
    {
        MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is ICombatReceiver combatReceiver)
            {
                receiver = combatReceiver;
                return true;
            }
        }

        receiver = null;
        return false;
    }

    [ContextMenu("Debug/Start Basic Attack")]
    private void DebugStartBasicAttack()
    {
        TryStartBasicAttack();
    }
}