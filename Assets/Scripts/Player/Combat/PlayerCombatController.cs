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
    [SerializeField] private float preWindowInputBufferDuration = 0.35f;
    [SerializeField] private bool allowRetargetBetweenComboSteps = true;

    [Header("Facing")]
    [SerializeField] private Vector2 defaultFacingDirection = Vector2.right;
    [SerializeField] private float minHorizontalInputToChangeFacing = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool enableCombatDebugLogs = true;

    private readonly HashSet<int> hitTargetIds = new HashSet<int>();

    private bool isAttackInProgress;
    private bool isHitboxOpen;
    private bool comboInputWindowOpen;
    private bool queuedNextComboStep;
    private bool preWindowBufferedInputActive;
    private float preWindowBufferedInputExpireTime;

    private int currentComboStepIndex = -1;
    private int lastProcessedEndEventFrame = -1;
    private int lastProcessedOpenHitboxEventFrame = -1;
    private int lastProcessedCloseHitboxEventFrame = -1;
    private int lastProcessedOpenComboWindowEventFrame = -1;
    private int lastProcessedCloseComboWindowEventFrame = -1;

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
        {
            DebugLog($"BUFFER EXPIRED | step={CurrentComboStepNumber}");
            ClearBufferedInput();
        }
    }

    private void HandlePlayerDied()
    {
        DebugLog("PLAYER DIED -> ForceStopAllCombatState");
        ForceStopAllCombatState();
    }

    private void HandleAttackStarted()
    {
        DebugLog(
            $"INPUT AttackStarted | inProgress={isAttackInProgress} | step={CurrentComboStepNumber} | " +
            $"windowOpen={comboInputWindowOpen} | queued={queuedNextComboStep} | buffered={preWindowBufferedInputActive}"
        );

        if (!isAttackInProgress)
        {
            TryStartComboFromBeginning();
            return;
        }

        TryBufferComboContinuationInput();
    }

    public bool TryStartComboFromBeginning()
    {
        DebugLog("TryStartComboFromBeginning()");
        return StartComboStep(0, true);
    }

    private bool StartComboStep(int stepIndex, bool fromNeutral)
    {
        if (!IsValidComboStepIndex(stepIndex))
        {
            DebugLog($"StartComboStep FAILED | invalid stepIndex={stepIndex}");
            return false;
        }

        if (fromNeutral && !CanStartAttackFromNeutral())
        {
            DebugLog($"StartComboStep FAILED | step={stepIndex + 1} | CanStartAttackFromNeutral=false");
            return false;
        }

        PlayerComboStepDefinition step = comboSteps[stepIndex];
        if (step == null)
        {
            DebugLog($"StartComboStep FAILED | step={stepIndex + 1} | step definition is null");
            return false;
        }

        step.Sanitize();

        currentComboStepIndex = stepIndex;
        lastProcessedEndEventFrame = -1;
        lastProcessedOpenHitboxEventFrame = -1;
        lastProcessedCloseHitboxEventFrame = -1;
        lastProcessedOpenComboWindowEventFrame = -1;
        lastProcessedCloseComboWindowEventFrame = -1;

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

        float failSafe = GetResolvedFailSafeDuration(step);
        StartAttackFailSafeTimer(failSafe);

        DebugLog(
            $"START STEP {stepIndex + 1} | animCombo={step.animationComboIndex} | attackId={step.attackId} | " +
            $"side={currentAttackSide} | failSafe={failSafe:F2}"
        );

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
        {
            DebugLog($"BUFFER INPUT IGNORED | no next combo step after step={CurrentComboStepNumber}");
            return;
        }

        if (comboInputWindowOpen)
        {
            if (!queuedNextComboStep)
            {
                queuedNextComboStep = true;
                DebugLog($"QUEUE NEXT STEP IMMEDIATE | currentStep={CurrentComboStepNumber} -> nextStep={CurrentComboStepNumber + 1}");
            }
            else
            {
                DebugLog($"QUEUE NEXT STEP IGNORED | already queued | currentStep={CurrentComboStepNumber}");
            }

            return;
        }

        preWindowBufferedInputActive = true;
        preWindowBufferedInputExpireTime = Time.time + Mathf.Max(0.01f, preWindowInputBufferDuration);

        DebugLog(
            $"BUFFER INPUT STORED | currentStep={CurrentComboStepNumber} | expiresAt={preWindowBufferedInputExpireTime:F2} | " +
            $"bufferDuration={preWindowInputBufferDuration:F2}"
        );
    }

    private PlayerAttackSide ResolveAttackSideForNewAttack()
    {
        PlayerAttackSide targetFallbackSide = lastFacingSide;

        if (playerTargetingSystem != null &&
            playerTargetingSystem.TryGetPreferredAttackSide(targetFallbackSide, out PlayerAttackSide targetedSide))
        {
            DebugLog($"ResolveAttackSide -> targeting picked {targetedSide}");
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

    public void HandleAnimationEvent_OpenCurrentAttackHitbox()
    {
        if (!isAttackInProgress)
        {
            DebugLog("EVENT OpenHitbox IGNORED | no attack in progress");
            return;
        }

        if (lastProcessedOpenHitboxEventFrame == Time.frameCount)
        {
            DebugLog($"EVENT OpenHitbox IGNORED | duplicate same frame | frame={Time.frameCount}");
            return;
        }

        lastProcessedOpenHitboxEventFrame = Time.frameCount;
        isHitboxOpen = true;

        PlayerCombatHitbox activeHitbox = GetActiveHitboxForCurrentAttack();
        if (activeHitbox != null)
            activeHitbox.SetHitboxActive(true);

        DebugLog($"EVENT OpenHitbox | step={CurrentComboStepNumber} | side={currentAttackSide}");
    }

    public void HandleAnimationEvent_CloseCurrentAttackHitbox()
    {
        if (!isAttackInProgress && !isHitboxOpen)
        {
            DebugLog("EVENT CloseHitbox IGNORED | no attack / already closed");
            return;
        }

        if (lastProcessedCloseHitboxEventFrame == Time.frameCount)
        {
            DebugLog($"EVENT CloseHitbox IGNORED | duplicate same frame | frame={Time.frameCount}");
            return;
        }

        lastProcessedCloseHitboxEventFrame = Time.frameCount;
        isHitboxOpen = false;
        DisableAllHitboxesImmediate();

        DebugLog($"EVENT CloseHitbox | step={CurrentComboStepNumber}");
    }

    public void HandleAnimationEvent_OpenComboInputWindow()
    {
        if (!isAttackInProgress)
        {
            DebugLog("EVENT OpenComboWindow IGNORED | no attack in progress");
            return;
        }

        if (lastProcessedOpenComboWindowEventFrame == Time.frameCount)
        {
            DebugLog($"EVENT OpenComboWindow IGNORED | duplicate same frame | frame={Time.frameCount}");
            return;
        }

        lastProcessedOpenComboWindowEventFrame = Time.frameCount;
        comboInputWindowOpen = true;

        DebugLog(
            $"EVENT OpenComboWindow | step={CurrentComboStepNumber} | buffered={preWindowBufferedInputActive} | " +
            $"hasNext={HasNextComboStep()}"
        );

        if (HasNextComboStep() && HasValidBufferedInput())
        {
            queuedNextComboStep = true;
            ClearBufferedInput();

            DebugLog($"BUFFER PROMOTED TO QUEUE | currentStep={CurrentComboStepNumber} -> nextStep={CurrentComboStepNumber + 1}");
        }
    }

    public void HandleAnimationEvent_CloseComboInputWindow()
    {
        if (!isAttackInProgress)
        {
            DebugLog("EVENT CloseComboWindow IGNORED | no attack in progress");
            return;
        }

        if (lastProcessedCloseComboWindowEventFrame == Time.frameCount)
        {
            DebugLog($"EVENT CloseComboWindow IGNORED | duplicate same frame | frame={Time.frameCount}");
            return;
        }

        lastProcessedCloseComboWindowEventFrame = Time.frameCount;
        comboInputWindowOpen = false;
        ClearBufferedInput();

        DebugLog($"EVENT CloseComboWindow | step={CurrentComboStepNumber} | queued={queuedNextComboStep}");
    }

    public void HandleAnimationEvent_EndCurrentAttackStep()
    {
        if (!isAttackInProgress)
        {
            DebugLog("EVENT EndStep IGNORED | no attack in progress");
            return;
        }

        if (lastProcessedEndEventFrame == Time.frameCount)
        {
            DebugLog($"EVENT EndStep IGNORED | duplicate same frame | frame={Time.frameCount}");
            return;
        }

        lastProcessedEndEventFrame = Time.frameCount;

        StopAttackFailSafeTimer();

        isHitboxOpen = false;
        comboInputWindowOpen = false;
        DisableAllHitboxesImmediate();
        ClearBufferedInput();

        int finishedStepIndex = currentComboStepIndex;
        PlayerAttackSide finishedSide = currentAttackSide;

        DebugLog(
            $"EVENT EndStep | finishedStep={finishedStepIndex + 1} | queuedNext={queuedNextComboStep} | hasNext={HasNextComboStep()}"
        );

        OnComboStepFinished?.Invoke(finishedStepIndex, finishedSide);

        if (queuedNextComboStep && HasNextComboStep())
        {
            int nextStepIndex = currentComboStepIndex + 1;
            queuedNextComboStep = false;

            DebugLog($"CHAIN TO NEXT STEP | {finishedStepIndex + 1} -> {nextStepIndex + 1}");

            if (!StartComboStep(nextStepIndex, false))
            {
                DebugLog("CHAIN FAILED -> FinishCombo()");
                FinishCombo();
            }

            return;
        }

        DebugLog("NO CHAIN -> FinishCombo()");
        FinishCombo();
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

        DebugLog(
            $"HIT CONFIRMED | step={CurrentComboStepNumber} | attackId={damageInfo.AttackId} | " +
            $"damage={damageInfo.Damage} | target={receiverTransform.name}"
        );

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

        DebugLog($"FinishCombo() | comboWasActive={comboWasActive} | lastStep={CurrentComboStepNumber}");

        isAttackInProgress = false;
        isHitboxOpen = false;
        comboInputWindowOpen = false;
        queuedNextComboStep = false;
        currentComboStepIndex = -1;
        lastProcessedEndEventFrame = -1;
        lastProcessedOpenHitboxEventFrame = -1;
        lastProcessedCloseHitboxEventFrame = -1;
        lastProcessedOpenComboWindowEventFrame = -1;
        lastProcessedCloseComboWindowEventFrame = -1;

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

        DebugLog("ForceStopAllCombatState()");

        isAttackInProgress = false;
        isHitboxOpen = false;
        comboInputWindowOpen = false;
        queuedNextComboStep = false;
        currentComboStepIndex = -1;
        lastProcessedEndEventFrame = -1;
        lastProcessedOpenHitboxEventFrame = -1;
        lastProcessedCloseHitboxEventFrame = -1;
        lastProcessedOpenComboWindowEventFrame = -1;
        lastProcessedCloseComboWindowEventFrame = -1;

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

        DebugLog($"StartFailSafeTimer | duration={duration:F2} | step={CurrentComboStepNumber}");
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

        DebugLog($"FAILSAFE TRIGGERED | step={CurrentComboStepNumber}");

        if (isAttackInProgress)
            HandleAnimationEvent_EndCurrentAttackStep();
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

    private void DebugLog(string message)
    {
        if (!enableCombatDebugLogs)
            return;

        Debug.Log($"[PlayerCombat DEBUG] {message}", this);
    }
}