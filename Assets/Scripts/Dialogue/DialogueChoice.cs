using UnityEngine;
using UnityEngine.Localization;

[System.Serializable] // Позволяет отображать класс в инспекторе
public class DialogueChoice
{
    // Текст варианта ответа игрока
    public LocalizedString choiceText;

    // Следующий узел диалога
    public DialogueNode nextNode;

    // Тип события
    public DialogueEventType eventType;

    // ID события (например ID квеста)
    public string eventID;
}