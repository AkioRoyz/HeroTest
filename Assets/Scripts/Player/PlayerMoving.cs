using System.Collections;
using UnityEngine;

public class PlayerMoving : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private PlayerHealth playerHealth;

    private float bonusSpeed;
    private bool isMovementBlocked;
    private Coroutine blockCoroutine;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (playerHealth != null)
            playerHealth.OnTakeDamage += BlockMovement;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnTakeDamage -= BlockMovement;
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
        if (isMovementBlocked || rb == null || gameInput == null)
            return;

        Vector2 move = gameInput.MoveVector;
        float currentSpeed = Mathf.Max(0f, baseSpeed + bonusSpeed);

        rb.MovePosition(rb.position + move * currentSpeed * Time.fixedDeltaTime);
    }

    public void AddSpeedBonus(float amount) => bonusSpeed += amount;
    public void RemoveSpeedBonus(float amount) => bonusSpeed -= amount;

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

    private void BlockMovement()
    {
        if (blockCoroutine != null)
            StopCoroutine(blockCoroutine);

        blockCoroutine = StartCoroutine(BlockMovementCoroutine());
    }

    private IEnumerator BlockMovementCoroutine()
    {
        isMovementBlocked = true;
        yield return new WaitForSeconds(0.5f);
        isMovementBlocked = false;
        blockCoroutine = null;
    }
}