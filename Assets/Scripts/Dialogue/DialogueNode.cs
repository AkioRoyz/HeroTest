using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class DialogueNode
{
    // Имя персонажа
    public string speakerName;

    // Портрет персонажа
    public Sprite portrait;

    // Текст реплики
    public LocalizedString dialogueText;

    // Варианты ответа
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}