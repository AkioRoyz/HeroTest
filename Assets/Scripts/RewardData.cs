using System;

[Serializable]
public struct RewardData
{
    public int Exp;
    public int Gold;

    public RewardData(int exp, int gold)
    {
        Exp = exp;
        Gold = gold;
    }
}