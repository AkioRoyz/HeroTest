using System;
using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    public static RewardSystem Instance;

    public static event Action<RewardData> OnRewardGiven;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GiveReward(RewardData reward)
    {
        OnRewardGiven?.Invoke(reward);
    }
}