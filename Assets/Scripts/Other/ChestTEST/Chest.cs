using System;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private bool IsOpened;
    private bool IsPlayerCanOpen;
    private int playerLayer;

    [SerializeField] private int chestEXP = 10;
    [SerializeField] private int chestGold = 10;
    [SerializeField] private Animator chestAnimator;
    [SerializeField] GameInput gameInput;
    [SerializeField] GameObject triggerImageZone;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            IsPlayerCanOpen = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            IsPlayerCanOpen = false;
        }
    }

    private void OnEnable()
    {
        gameInput.OnUse += ChestOpened;
    }

    private void OnDisable()
    {
        gameInput.OnUse -= ChestOpened;
    }

    private void ChestOpened()
    {
        if (!IsPlayerCanOpen || IsOpened)
        {
            return;
        }


        RewardData reward = new RewardData(chestEXP, chestGold);
        RewardSystem.Instance.GiveReward(reward);
        IsOpened = true;
        chestAnimator.SetBool("IsOpened", true);
        triggerImageZone.SetActive(false);
    }
}
