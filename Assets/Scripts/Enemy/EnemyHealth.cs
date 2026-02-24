using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;

    public event Action<int, int> OnEnemyDie;
    public event Action OnEnemyHealthChange;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) //ТЕСТОВЫЙ КОД УДАЛИТЬ ПОСЛЕ
        {
            TakeDamage(10);
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        OnEnemyHealthChange?.Invoke();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnEnemyDie?.Invoke(10,5);
        Destroy(gameObject);
    }
}
