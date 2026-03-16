using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class DialogueNode
{
    // Тип того кто говорит
    public DialogueSpeakerType speakerType;

    // Портрет персонажа
    public Sprite portrait;

    // Текст реплики
    public LocalizedString dialogueText;

    // Варианты ответа
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}