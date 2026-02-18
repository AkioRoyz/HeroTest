using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManaBarUI : MonoBehaviour
{
    [SerializeField] Image manaBar;
    [SerializeField] PlayerMana playerMana;
    [SerializeField] TextMeshProUGUI manaText;

    private void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        playerMana.OnManaChange += UpdateUI;
    }

    private void OnDisable()
    {
        playerMana.OnManaChange -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (playerMana.MaxMana <= 0) return;

        float fill = (float)playerMana.CurrentMana / playerMana.MaxMana;
        fill = Mathf.Clamp01(fill);

        manaBar.fillAmount = fill;
        manaText.text = playerMana.CurrentMana + "/" + playerMana.MaxMana;
    }
}
