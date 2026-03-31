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

    [Header("Combo Settings")]
    [SerializeField] private PlayerComboStepDefinition[] comboSteps;
    [SerializeField] private bool lockMovementDuringAttack = true;
    [SerializeField] private float defaultAttackFailSafeDuration = 0.70f;
    [SerializeField] private float preWindowInputBufferDuration = 0.20f;
    [SerializeField] private bool allowRetargetBetweenComboSteps = true;

    [Header("Facing")]
    [SerializeField] private Vector2 defaultFacingDirection = Vector2.right;
    [SerializeField] private float minHorizontalInputToChangeFacing = 0.05f;

    private readonly HashSet<int> hitTargetIds = new HashSet<int>();

    private bool isAttackInProgress;
    private bool isHitboxOpen;
    private bool comboInputWindowOpen;
    private bool queuedNextComboStep;
    private bool preWindowBufferedInputActive;
    private float preWindowBufferedInputExpireTime;

    private int currentComboStepIndex = -1;
    private int lastProcessedEndEventFrame = -1;

    private Vector2 lastNonZeroMoveDirection = Vector2.right;
    private PlayerAttackSide lastFacingSide = PlayerAttackSide.Right;
    private PlayerAttackSide currentAttackSide = PlayerAttackSide.Right;

    private Coroutine attackFailSafeCoroutine;

    public event Action<int, PlayerAttackSide> OnComboStepStarted;
    public event Action<int, PlayerAttackSide> OnComboStepFinished;
    public event Action OnComboFinished;

    public bool IsAttackInProgress => isAttackInProgress;
    public bool IsHitboxOpen => isHitboxOpen;
    public bool IsComboInputWindowOpen => comboInputWindowOpen;
    public bool HasQueuedNextComboStep => queuedNextComboStep;
    public bool ShouldLockVisualFacing => isAttackInProgress;
    public int CurrentComboStepNumber => currentComboStepIndex >= 0 ? currentComboStepIndex + 1 : 0;
    public int CurrentComboStepIndex => currentComboStepIndex;

    public PlayerAttackSide CurrentAttackSide => isAttackInProgress ? currentAttackSide : lastFacingSide;

    public Vector2 CurrentAttackDirection
    {
        get
        {
            return CurrentAttackSide == PlayerAttackSide.Left ? Vector2.left : Vector2.right;
        }
    }

    private void Reset()
    {
        EnsureDefaultComboStepsIfNeeded();
        SanitizeComboSteps();
    }

    private void OnValidate()
    {
        EnsureDefaultComboStepsIfNeeded();
        SanitizeComboSteps();

        defaultAttackFailSafeDuration = Mathf.Max(0.05f, defaultAttackFailSafeDuration);
        preWindowInputBufferDuration = Mathf.Max(0.01f, preWindowInputBufferDuration);
        minHorizontalInputToChangeFacing = Mathf.Max(0f, minHorizontalInputToChangeFacing);
    }

    private void Awake()
    {
        ResolveReferences();
        EnsureDefaultComboStepsIfNeeded();
        SanitizeComboSteps();
        DisableAllHitboxesImmediate();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (gameInput != null)
            gameInput.OnAttackStarted += HandleAttackStarted;

        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (gameInput != null)
            gameInput.OnAttackStarted -= HandleAttackStarted;

        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;

        ForceStopAllCombatState();
    }

    private void Update()
    {
        UpdateLastFacingFromInput();
        UpdateBufferedInputExpiration();
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
            playerHealth = GetComponent<PlayerHealth>();

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

    private void UpdateBufferedInputExpiration()
    {
        if (!preWindowBufferedInputActive)
            return;

        if (Time.time > preWindowBufferedInputExpireTime)
            ClearBufferedInput();
    }

    private void HandlePlayerDied()
    {
        ForceStopAllCombatState();
    }

    private void HandleAttackStarted()
    {
        if (!isAttackInProgress)
        {
            TryStartComboFromBeginning();
            return;
        }

        TryBufferComboContinuationInput();
    }

    public bool TryStartComboFromBeginning()
    {
        return StartComboStep(0, true);
    }

    private bool StartComboStep(int stepIndex, bool fromNeutral)
    {
        if (!IsValidComboStepIndex(stepIndex))
            return false;

        if (fromNeutral && !CanStartAttackFromNeutral())
            return false;

        PlayerComboStepDefinition step = comboSteps[stepIndex];
        if (step == null)
            return false;

        step.Sanitize();

        currentComboStepIndex = stepIndex;
        lastProcessedEndEventFrame = -1;

        if (fromNeutral || allowRetargetBetweenComboSteps)
            currentAttackSide = ResolveAttackSideForNewAttack();

        isAttackInProgress = true;
        isHitboxOpen = false;
        comboInputWindowOpen = false;
        queuedNextComboStep = false;

        ClearBufferedInput();
        hitTargetIds.Clear();
        DisableAllHitboxesImmediate();

        if (lockMovementDuringAttack && playerMoving != null)
            playerMoving.SetExternalMovementBlocked(true);

        StartAttackFailSafeTimer(GetResolvedFailSafeDuration(step));

        if (playerAnimation != null)
            playerAnimation.PlayComboAttack(step.animationComboIndex, currentAttackSide);

        OnComboStepStarted?.Invoke(currentComboStepIndex, currentAttackSide);
        return true;
    }

    private bool CanStartAttackFromNeutral()
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return false;

        if (isAttackInProgress)
            return false;

        if (playerHealth != null && !playerHealth.IsAlive)
            return false;

        if (comboSteps == null || comboSteps.Length == 0)
            return false;

        return true;
    }

    private void TryBufferComboContinuationInput()
    {
        if (!isAttackInProgress)
            return;

        if (!HasNextComboStep())
            return;

        if (comboInputWindowOpen)
        {
            queuedNextComboStep = true;
            return;
        }

        preWindowBufferedInputActive = true;
        preWindowBufferedInputExpireTime = Time.time + Mathf.Max(0.01f, preWindowInputBufferDuration);
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

    public void AnimationEvent_OpenCurrentAttackHitbox()
    {
        if (!isAttackInProgress)
            return;

        isHitboxOpen = true;

        PlayerCombatHitbox activeHitbox = GetActiveHitboxForCurrentAttack();
        if (activeHitbox != null)
            activeHitbox.SetHitboxActive(true);
    }

    public void AnimationEvent_CloseCurrentAttackHitbox()
    {
        isHitboxOpen = false;
        DisableAllHitboxesImmediate();
    }

    public void AnimationEvent_OpenComboInputWindow()
    {
        if (!isAttackInProgress)
            return;

        comboInputWindowOpen = true;

        if (HasNextComboStep() && HasValidBufferedInput())
        {
            queuedNextComboStep = true;
            ClearBufferedInput();
        }
    }

    public void AnimationEvent_CloseComboInputWindow()
    {
        comboInputWindowOpen = false;
        ClearBufferedInput();
    }

    public void AnimationEvent_EndCurrentAttackStep()
    {
        if (!isAttackInProgress)
            return;

        if (lastProcessedEndEventFrame == Time.frameCount)
            return;

        lastProcessedEndEventFrame = Time.frameCount;

        StopAttackFailSafeTimer();

        isHitboxOpen = false;
        comboInputWindowOpen = false;
        DisableAllHitboxesImmediate();
        ClearBufferedInput();

        int finishedStepIndex = currentComboStepIndex;
        PlayerAttackSide finishedSide = currentAttackSide;

        OnComboStepFinished?.Invoke(finishedStepIndex, finishedSide);

        if (queuedNextComboStep && HasNextComboStep())
        {
            int nextStepIndex = currentComboStepIndex + 1;
            queuedNextComboStep = false;
            StartComboStep(nextStepIndex, false);
            return;
        }

        FinishCombo();
    }

    // Backward-compatible wrappers
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
        PlayerComboStepDefinition step = GetCurrentComboStep();
        if (step == null)
        {
            step = CreateFallbackComboStep();
            step.Sanitize();
        }

        int strength = statsSystem != null ? Mathf.Max(1, statsSystem.Strength) : 1;

        float minMultiplier = Mathf.Min(step.randomDamageMinMultiplier, step.randomDamageMaxMultiplier);
        float maxMultiplier = Mathf.Max(step.randomDamageMinMultiplier, step.randomDamageMaxMultiplier);

        float randomizedMultiplier = UnityEngine.Random.Range(minMultiplier, maxMultiplier);
        int calculatedDamage = Mathf.RoundToInt((strength * step.strengthDamageMultiplier + step.flatDamageBonus) * randomizedMultiplier);
        calculatedDamage = Mathf.Max(1, calculatedDamage);

        return new DamageInfo(
            source: gameObject,
            sourceTeam: CombatTeam.Player,
            damage: calculatedDamage,
            direction: CurrentAttackDirection,
            hitPoint: hitPoint,
            knockbackForce: step.knockbackForce,
            stunDuration: step.stunDuration,
            canBeBlocked: step.canBeBlocked,
            canBeParried: step.canBeParried,
            ignoresInvulnerability: false,
            isCritical: false,
            isSpecial: false,
            hitReactionType: step.hitReactionType,
            attackId: step.attackId
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

    private void FinishCombo()
    {
        StopAttackFailSafeTimer();

        bool comboWasActive = isAttackInProgress || currentComboStepIndex >= 0;

        isAttackInProgress = false;
        isHitboxOpen = false;
        comboInputWindowOpen = false;
        queuedNextComboStep = false;
        currentComboStepIndex = -1;
        lastProcessedEndEventFrame = -1;

        ClearBufferedInput();
        hitTargetIds.Clear();
        DisableAllHitboxesImmediate();

        if (lockMovementDuringAttack && playerMoving != null)
            playerMoving.SetExternalMovementBlocked(false);

        if (playerAnimation != null)
            playerAnimation.ResetCombatAnimationState();

        if (comboWasActive)
            OnComboFinished?.Invoke();
    }

    private void ForceStopAllCombatState()
    {
        StopAttackFailSafeTimer();

        isAttackInProgress = false;
        isHitboxOpen = false;
        comboInputWindowOpen = false;
        queuedNextComboStep = false;
        currentComboStepIndex = -1;
        lastProcessedEndEventFrame = -1;

        ClearBufferedInput();
        hitTargetIds.Clear();
        DisableAllHitboxesImmediate();

        if (playerMoving != null)
            playerMoving.SetExternalMovementBlocked(false);

        if (playerAnimation != null)
            playerAnimation.ResetCombatAnimationState();
    }

    private void StartAttackFailSafeTimer(float duration)
    {
        StopAttackFailSafeTimer();
        attackFailSafeCoroutine = StartCoroutine(AttackFailSafeRoutine(duration));
    }

    private void StopAttackFailSafeTimer()
    {
        if (attackFailSafeCoroutine != null)
        {
            StopCoroutine(attackFailSafeCoroutine);
            attackFailSafeCoroutine = null;
        }
    }

    private IEnumerator AttackFailSafeRoutine(float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.05f, duration));
        attackFailSafeCoroutine = null;

        if (isAttackInProgress)
            AnimationEvent_EndCurrentAttackStep();
    }

    private bool HasNextComboStep()
    {
        return IsValidComboStepIndex(currentComboStepIndex + 1);
    }

    private bool HasValidBufferedInput()
    {
        return preWindowBufferedInputActive && Time.time <= preWindowBufferedInputExpireTime;
    }

    private void ClearBufferedInput()
    {
        preWindowBufferedInputActive = false;
        preWindowBufferedInputExpireTime = -1f;
    }

    private bool IsValidComboStepIndex(int index)
    {
        return comboSteps != null && index >= 0 && index < comboSteps.Length && comboSteps[index] != null;
    }

    private PlayerComboStepDefinition GetCurrentComboStep()
    {
        if (!IsValidComboStepIndex(currentComboStepIndex))
            return null;

        return comboSteps[currentComboStepIndex];
    }

    private float GetResolvedFailSafeDuration(PlayerComboStepDefinition step)
    {
        if (step == null)
            return defaultAttackFailSafeDuration;

        if (step.attackFailSafeDuration <= 0f)
            return defaultAttackFailSafeDuration;

        return step.attackFailSafeDuration;
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

    private void EnsureDefaultComboStepsIfNeeded()
    {
        if (comboSteps != null && comboSteps.Length > 0)
            return;

        comboSteps = new PlayerComboStepDefinition[3];
        comboSteps[0] = CreateDefaultComboStep(1, "player_light_01", 1.00f, 0, 0.70f);
        comboSteps[1] = CreateDefaultComboStep(2, "player_light_02", 1.10f, 1, 0.72f);
        comboSteps[2] = CreateDefaultComboStep(3, "player_light_03", 1.25f, 2, 0.78f);
    }

    private void SanitizeComboSteps()
    {
        if (comboSteps == null)
            return;

        for (int i = 0; i < comboSteps.Length; i++)
        {
            if (comboSteps[i] == null)
                comboSteps[i] = CreateDefaultComboStep(i + 1, $"player_light_0{i + 1}", 1.00f + (0.10f * i), i, defaultAttackFailSafeDuration);

            comboSteps[i].Sanitize();
        }
    }

    private PlayerComboStepDefinition CreateDefaultComboStep(int animationComboIndex, string attackId, float strengthMultiplier, int flatBonus, float failSafe)
    {
        PlayerComboStepDefinition step = new PlayerComboStepDefinition();
        step.animationComboIndex = Mathf.Max(1, animationComboIndex);
        step.attackId = attackId;
        step.attackFailSafeDuration = Mathf.Max(0.05f, failSafe);
        step.strengthDamageMultiplier = strengthMultiplier;
        step.flatDamageBonus = flatBonus;
        step.randomDamageMinMultiplier = 0.90f;
        step.randomDamageMaxMultiplier = 1.10f;
        step.knockbackForce = 0f;
        step.stunDuration = 0f;
        step.canBeBlocked = true;
        step.canBeParried = true;
        step.hitReactionType = HitReactionType.Light;
        step.Sanitize();
        return step;
    }

    private PlayerComboStepDefinition CreateFallbackComboStep()
    {
        return CreateDefaultComboStep(1, "player_light_fallback", 1f, 0, defaultAttackFailSafeDuration);
    }

    [ContextMenu("Debug/Start Combo From Beginning")]
    private void DebugStartCombo()
    {
        TryStartComboFromBeginning();
    }
}