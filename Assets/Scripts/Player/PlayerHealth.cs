using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    [SerializeField] StatsSystem statsSystem;

    public event Action OnHealthChange;
    public event Action OnTakeDamage;

    void Awake()
    {
        maxHealth = statsSystem.Strength * 10;
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        statsSystem.OnStatsUpdate += UpgradeHealth;
    }

    private void OnDisable()
    {
        statsSystem.OnStatsUpdate -= UpgradeHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) //ТЕСТОВЫЙ КОД УДАЛИТЬ ПОСЛЕ
        {
            TestDamage(10);
        }
    }

    private void TestDamage(int amount)
    {
        currentHealth -= amount;
        OnHealthChange?.Invoke();
        OnTakeDamage?.Invoke();
    }

    private void UpgradeHealth()
    {
        maxHealth = statsSystem.Strength * 10;
        currentHealth = maxHealth;
        OnHealthChange?.Invoke();
    }
}
