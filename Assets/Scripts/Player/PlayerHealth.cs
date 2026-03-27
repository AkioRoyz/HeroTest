using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private StatsSystem statsSystem;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public event Action OnHealthChange;
    public event Action OnTakeDamage;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        RefreshHealthFromStats(true);
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (statsSystem != null)
        {
            statsSystem.OnStatsUpdate += OnStatsUpdated;
            statsSystem.OnLevelStatsUpdated += OnLevelStatsUpdated;
        }
    }

    private void OnDisable()
    {
        if (statsSystem != null)
        {
            statsSystem.OnStatsUpdate -= OnStatsUpdated;
            statsSystem.OnLevelStatsUpdated -= OnLevelStatsUpdated;
        }
    }

    private void ResolveReferences()
    {
        if (statsSystem == null)
            statsSystem = FindFirstObjectByType<StatsSystem>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChange?.Invoke();
        OnTakeDamage?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChange?.Invoke();
    }

    public void RestoreToFull()
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke();
    }

    private void OnStatsUpdated()
    {
        RefreshHealthFromStats(false);
    }

    private void OnLevelStatsUpdated()
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke();
    }

    private void RefreshHealthFromStats(bool firstInit)
    {
        if (statsSystem == null)
        {
            maxHealth = Mathf.Max(maxHealth, 1);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnHealthChange?.Invoke();
            return;
        }

        int oldMaxHealth = maxHealth;
        maxHealth = Mathf.Max(1, statsSystem.Strength * 10);

        if (firstInit || oldMaxHealth <= 0)
        {
            currentHealth = maxHealth;
        }
        else
        {
            int difference = maxHealth - oldMaxHealth;
            currentHealth += difference;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        OnHealthChange?.Invoke();
    }
}