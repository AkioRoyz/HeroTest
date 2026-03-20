using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class DialogueChoiceData
{
    [Header("Choice Text")]
    [SerializeField] private LocalizedString choiceText;

    [Header("Next Node")]
    [Tooltip("»ндекс узла, в который перейдЄт диалог после выбора этого ответа. -1 = завершить диалог.")]
    [SerializeField] private int nextNodeIndex = -1;

    [Header("Conditions")]
    [SerializeField] private List<DialogueConditionData> conditions = new();

    [Header("Unavailable Behaviour")]
    [SerializeField] private DialogueChoiceUnavailableMode unavailableMode = DialogueChoiceUnavailableMode.Hide;

    [Tooltip("“екст, который показываетс€, если ответ недоступен и выбран режим ShowDisabled. ≈сли не задан, будет показан обычный текст ответа.")]
    [SerializeField] private LocalizedString disabledChoiceText;

    [Header("On Select Actions")]
    [SerializeField] private List<DialogueActionData> onSelectActions = new();

    public LocalizedString ChoiceText => choiceText;
    public int NextNodeIndex => nextNodeIndex;
    public IReadOnlyList<DialogueConditionData> Conditions => conditions;
    public DialogueChoiceUnavailableMode UnavailableMode => unavailableMode;
    public LocalizedString DisabledChoiceText => disabledChoiceText;
    public IReadOnlyList<DialogueActionData> OnSelectActions => onSelectActions;
}