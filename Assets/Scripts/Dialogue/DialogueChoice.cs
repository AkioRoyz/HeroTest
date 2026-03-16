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

    // Условие показа варианта
    public DialogueConditionType conditionType;

    // ID условия (например ID квеста)
    public string conditionID;
}