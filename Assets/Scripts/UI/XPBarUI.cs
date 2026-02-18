using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    [SerializeField] Image xpBar;
    [SerializeField] ExpSystem expSystem;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] TextMeshProUGUI lvlText;

    private int curLvl;

    private void Awake()
    {
        xpBar.fillAmount = 0;
        expText.text = "0/" + expSystem.XpToNextLvl;
        lvlText.text = "1";
    }

    private void OnEnable()
    {
        expSystem.OnXpAdd += XPupdate;
    }

    private void OnDisable()
    {
        expSystem.OnXpAdd -= XPupdate;
    }

    private void XPupdate(int currentXP)
    {
        int xpToNextLvl = expSystem.XpToNextLvl;
        float fill = (float)currentXP / xpToNextLvl;
        fill = Mathf.Clamp01(fill);
        xpBar.fillAmount = fill;
        expText.text = currentXP + "/" + xpToNextLvl;
        curLvl = expSystem.CurrentLvl;
        lvlText.text = curLvl.ToString();
    }
}
