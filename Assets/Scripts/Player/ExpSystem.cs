using System;
using Unity.VisualScripting;
using UnityEngine;

public class ExpSystem : MonoBehaviour
{
    [SerializeField] private int xpToNextLvl = 100;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int maxLvl = 50;
    [SerializeField] private int currentLvl = 1;
    [SerializeField] private int percentUp = 10;
    [SerializeField] private int testAddXp = 100;

    public event Action<int> OnLevelChange;
    public event Action<int> OnXpAdd;

    public int XpToNextLvl => xpToNextLvl;
    public int CurrentLvl => currentLvl;
    public int MaxLvl => maxLvl;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AddXP(testAddXp);
        }
    }

    private void AddXP(int amount)
    {
        if (currentLvl == maxLvl)
        {
            return;
        }

        currentXP += amount;

        while (currentXP >= xpToNextLvl)
        {
            currentXP -= xpToNextLvl;
            LvlUp();

            if (currentLvl == maxLvl)
            {
                currentXP = xpToNextLvl;
                break;
            }
        }

        OnXpAdd?.Invoke(currentXP);

    }

    private void LvlUp()
    {
        if (currentLvl == maxLvl)
        {
            return;
        }

        currentLvl++;

        if (currentLvl < maxLvl)
        {
            xpToNextLvl = Mathf.RoundToInt(xpToNextLvl * (1 + percentUp / 100f));
        }

        OnLevelChange?.Invoke(currentLvl);
    }
}
