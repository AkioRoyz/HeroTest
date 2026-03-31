using System.Collections;
using UnityEngine;

public class PlayerMoving : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private PlayerHealth playerHealth;

    private float bonusSpeed;

    private bool isHitMovementBlocked;
    private bool isExternalMovementBlocked;
    private Coroutine hitBlockCoroutine;

    public bool IsMovementBlocked => isHitMovementBlocked || isExternalMovementBlocked;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (playerHealth != null)
            playerHealth.OnTakeDamage += BlockMovementFromHit;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnTakeDamage -= BlockMovementFromHit;

        isHitMovementBlocked = false;
        isExternalMovementBlocked = false;

        if (hitBlockCoroutine != null)
        {
            StopCoroutine(hitBlockCoroutine);
            hitBlockCoroutine = null;
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ResolveReferences()
    {
        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void Move()
    {
        if (rb == null || gameInput == null)
            return;

        if (IsMovementBlocked)
            return;

        Vector2 move = gameInput.MoveVector;
        float currentSpeed = Mathf.Max(0f, baseSpeed + bonusSpeed);

        rb.MovePosition(rb.position + move * currentSpeed * Time.fixedDeltaTime);
    }

    public void SetExternalMovementBlocked(bool value)
    {
        isExternalMovementBlocked = value;
    }

    public void AddSpeedBonus(float amount)
    {
        bonusSpeed += amount;
    }

    public void RemoveSpeedBonus(float amount)
    {
        bonusSpeed -= amount;
    }

    public void AddTemporarySpeedBonus(float amount, float duration)
    {
        StartCoroutine(TemporarySpeedBonusRoutine(amount, duration));
    }

    private IEnumerator TemporarySpeedBonusRoutine(float amount, float duration)
    {
        AddSpeedBonus(amount);
        yield return new WaitForSeconds(duration);
        RemoveSpeedBonus(amount);
    }

    private void BlockMovementFromHit()
    {
        if (hitBlockCoroutine != null)
            StopCoroutine(hitBlockCoroutine);

        hitBlockCoroutine = StartCoroutine(BlockMovementCoroutine());
    }

    private IEnumerator BlockMovementCoroutine()
    {
        isHitMovementBlocked = true;
        yield return new WaitForSeconds(0.5f);
        isHitMovementBlocked = false;
        hitBlockCoroutine = null;
    }
}