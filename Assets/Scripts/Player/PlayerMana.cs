using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [SerializeField] private int maxMana;
    [SerializeField] private int currentMana;

    public int MaxMana => maxMana;
    public int CurrentMana => currentMana;

    [SerializeField] StatsSystem statsSystem;
    public event Action OnManaChange;

    void Awake()
    {
        maxMana = statsSystem.Mana * 10;
        currentMana = maxMana;
    }

    private void OnEnable()
    {
        statsSystem.OnStatsUpdate += UpgradeMana;
    }

    private void OnDisable()
    {
        statsSystem.OnStatsUpdate -= UpgradeMana;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            TestManaStore(10);
        }
    }

    private void TestManaStore(int amount)
    {
        currentMana -= amount;
        OnManaChange?.Invoke();
    }

    private void UpgradeMana()
    {
        maxMana = statsSystem.Mana * 10;
        currentMana = maxMana;
        OnManaChange?.Invoke();
    }
}
