using TMPro;
using UnityEngine;

public class GoldAmount : MonoBehaviour
{
    [SerializeField] GoldSystem goldSystem;
    [SerializeField] TextMeshProUGUI goldText;

    private int goldAmount;

    private void OnEnable()
    {
        goldSystem.OnGoldChange += GoldTextChange;
    }

    private void OnDisable()
    {
        goldSystem.OnGoldChange -= GoldTextChange;
    }

    private void GoldTextChange()
    {
        goldAmount = goldSystem.GoldAmount;
        goldText.text = goldAmount.ToString();
    }
}
