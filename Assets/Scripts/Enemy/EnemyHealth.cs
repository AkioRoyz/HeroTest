using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;

    public static event Action<EnemyReward> OnEnemyDie;
    public event Action OnEnemyHealthChange;

    [SerializeField] private int deathGold = 5;
    [SerializeField] private int deathEXP = 10;

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

    public struct EnemyReward
    {
        public int Exp;
        public int Gold;

        public EnemyReward(int exp, int gold)
        {
            Exp = exp; Gold = gold;
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
        OnEnemyDie?.Invoke(new EnemyReward(deathEXP, deathGold));
        Destroy(gameObject);
    }
}
