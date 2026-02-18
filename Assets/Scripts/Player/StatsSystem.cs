using System;
using UnityEngine;

public class StatsSystem : MonoBehaviour
{
    [SerializeField] private int strength = 15;
    [SerializeField] private int mana = 7;
    [SerializeField] private int defence = 5;
    private int strengthStep = 5;
    private int manaStep = 4;
    private int defenceStep = 3;

    public event Action OnStatsUpdate;

    public int Strength => strength;
    public int Mana => mana;
    public int Defence => defence;

    [SerializeField] ExpSystem expSystem;

    private void OnEnable()
    {
        expSystem.OnLevelChange += StatsUpdate;
    }

    private void OnDisable()
    {
        expSystem.OnLevelChange -= StatsUpdate;
    }

    private void StatsUpdate(int currentLvl)
    {
        strength = strength + strengthStep * (currentLvl - 1);
        mana = mana + manaStep * (currentLvl - 1);
        defence = defence + defenceStep * (currentLvl - 1);
        OnStatsUpdate?.Invoke();
    }
}
