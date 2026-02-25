using UnityEngine;

public class Chest : MonoBehaviour
{
    private bool IsOpened;
    private bool IsPlayerNear;
    private bool IsPlayerCanOpen;

    [SerializeField] private int chestEXP = 10;
    [SerializeField] private int chestGold = 10;
}
