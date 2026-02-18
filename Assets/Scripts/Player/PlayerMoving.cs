using System.Collections;
using UnityEngine;

public class PlayerMoving : MonoBehaviour
{
    [SerializeField] GameInput gameInput;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] PlayerHealth playerHealth;

    private float currentSpeed;
    private Coroutine blockCoroutine;

    private void Awake()
    {
        currentSpeed = speed;
    }

    private void OnEnable()
    {
        playerHealth.OnTakeDamage += BlockMovement;
    }

    private void OnDisable()
    {
        playerHealth.OnTakeDamage -= BlockMovement;
    }

    private void FixedUpdate()
    {
        Moving();
    }

    private void Moving()
    {
        Vector2 move = gameInput.moveVector;
        rb.MovePosition(rb.position + move * currentSpeed * Time.fixedDeltaTime);
    }

    private void BlockMovement()
    {
        if (blockCoroutine != null)
            StopCoroutine(blockCoroutine);

        blockCoroutine = StartCoroutine(BlockMovementCoroutine());
    }

    private IEnumerator BlockMovementCoroutine()
    {
        currentSpeed = 0f;
        yield return new WaitForSeconds(0.5f);
        currentSpeed = speed;
        blockCoroutine = null;
    }
}
