using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class NpcScript : MonoBehaviour
{
    [SerializeField] private LocalizedString npcName;
    [SerializeField] private TMP_Text nameText;

    private void Awake()
    {
        npcName.StringChanged += UpdateName;
        npcName.RefreshString();
    }

    private void UpdateName(string value)
    {
        nameText.text = value;
    }

    private void OnDisable()
    {
        npcName.StringChanged -= UpdateName;
    }
}
