using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class NPCDialogue : MonoBehaviour
{
    // Диалог, который будет запускать NPC
    [SerializeField] private Dialogue dialogue;

    [SerializeField] private LocalizedString npcName;
    [SerializeField] private TMP_Text nameText;

    // Ссылка на DialogueManager
    private DialogueManager dialogueManager;

    private void Awake()
    {
        npcName.StringChanged += UpdateName;
        npcName.RefreshString();
    }

    private void Start()
    {
        // Ищем DialogueManager на сцене
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    private void Update()
    {
        // Нажатие клавиши E запускает диалог
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    private void UpdateName(string value)
    {
        nameText.text = value;
    }

    private void OnDisable()
    {
        npcName.StringChanged -= UpdateName;
    }

    void StartDialogue()
    {
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogue, npcName);
        }
    }
}