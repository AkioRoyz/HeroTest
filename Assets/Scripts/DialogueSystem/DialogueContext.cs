using UnityEngine;
using UnityEngine.Localization;

public class DialogueContext
{
    // Кто запустил диалог.
    // Это может быть NPC, trigger zone или другой объект.
    public IDialogueSource Source { get; private set; }

    public DialogueContext(IDialogueSource source)
    {
        Source = source;
    }

    public LocalizedString GetSourceSpeakerName()
    {
        if (Source == null)
            return null;

        return Source.GetDialogueSpeakerName();
    }

    public Sprite GetSourcePortrait()
    {
        if (Source == null)
            return null;

        return Source.GetDialoguePortrait();
    }
}