using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth;

    [Header("Quest Identity")]
    [SerializeField] private string enemyTypeId;
    [SerializeField] private string enemyUniqueId;

    [Header("Death Reward")]
    [SerializeField] private int deathGold = 5;
    [SerializeField] private int deathEXP = 10;

    public event Action OnEnemyHealthChange;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public string EnemyTypeId => enemyTypeId;
    public string EnemyUniqueId => enemyUniqueId;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnEnemyHealthChange?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.NotifyEnemyKilled(enemyTypeId, enemyUniqueId);
        }

        if (RewardSystem.Instance != null)
        {
            RewardData reward = new RewardData(deathEXP, deathGold);
            RewardSystem.Instance.GiveReward(reward);
        }
        else
        {
            Debug.LogWarning("EnemyHealth: RewardSystem.Instance is missing.");
        }

        Destroy(gameObject);
    }
}