using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Image healthBar;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] TextMeshProUGUI healthText;

    private void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        playerHealth.OnHealthChange += UpdateUI;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChange -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (playerHealth.MaxHealth <= 0) return;

        float fill = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        fill = Mathf.Clamp01(fill);

        healthBar.fillAmount = fill;
        healthText.text = playerHealth.CurrentHealth + "/" + playerHealth.MaxHealth;
    }
}


