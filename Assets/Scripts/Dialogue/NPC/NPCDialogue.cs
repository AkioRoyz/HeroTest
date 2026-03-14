using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    // Диалог, который будет запускать NPC
    [SerializeField] private Dialogue dialogue;

    // Ссылка на DialogueManager
    private DialogueManager dialogueManager;

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

    void StartDialogue()
    {
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogue);
        }
    }
}