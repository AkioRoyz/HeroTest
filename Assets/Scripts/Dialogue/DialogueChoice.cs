using UnityEngine;
using UnityEngine.Localization;

[System.Serializable] // ѕозвол€ет отображать класс в инспекторе
public class DialogueChoice
{
    // “екст варианта ответа игрока
    public LocalizedString choiceText;

    // —ледующий узел диалога
    public DialogueNode nextNode;

    // ID событи€ диалога (если нужно)
    public string eventID;
}