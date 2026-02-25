using System;
using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    private int attackCombo = 1;
    public int AttackCombo => attackCombo;
    public event Action OnAttackCombo;
    [SerializeField] GameInput gameInput;

    private void OnEnable()
    {
        gameInput.OnAttack += Attack;
    }

    private void OnDisable()
    {
        gameInput.OnAttack -= Attack;
    }

    private void Attack()
    {
        if (attackCombo == 1)
        {
            attackCombo++;
        }

        else if (attackCombo >= 2)
        {
            attackCombo = 1;
        }
        OnAttackCombo?.Invoke();
    }
}
