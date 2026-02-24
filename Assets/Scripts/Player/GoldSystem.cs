using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GoldSystem : MonoBehaviour
{
    [SerializeField] private int goldAmount = 0;
    public event Action OnGoldChange;

    public int GoldAmount => goldAmount;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) //ТЕСТОВЫЙ КОД УДАЛИТЬ ПОСЛЕ
        {
            GoldAdd(10);
        }

        else if (Input.GetKeyDown(KeyCode.B)) //ТЕСТОВЫЙ КОД УДАЛИТЬ ПОСЛЕ
        {
            GoldSpend(5);
        }
    }

    private void GoldAdd(int amount)
    {
        goldAmount = goldAmount + amount;
        OnGoldChange?.Invoke();
    }

    private void GoldSpend(int amount)
    {
        goldAmount = goldAmount - amount;
        OnGoldChange?.Invoke();
    }
}


