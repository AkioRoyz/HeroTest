using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Optional Runtime Providers")]
    [SerializeField] private MonoBehaviour questProviderBehaviour;

    [Header("Player Data")]
    [Tooltip("¬ременно: текущий уровень игрока. ѕозже заменим на реальную систему уровн€.")]
    [SerializeField] private int debugPlayerLevel = 1;

    private IDialogueQuestProvider questProvider;

    private DialogueData currentDialogue;
    private DialogueContext currentContext;
    private int currentNodeIndex = -1;
    private int selectedChoiceIndex = 0;
    private bool isDialogueActive = false;

    // –еально доступные ответы текущего choice-узла
    private readonly List<DialogueChoiceData> availableChoices = new();

    public bool IsDialogueActive => isDialogueActive;
    public DialogueData CurrentDialogue => currentDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        questProvider = questProviderBehaviour as IDialogueQuestProvider;
    }

    private void Start()
    {
        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.OnDialogueUp += HandleDialogueUp;
            gameInput.OnDialogueDown += HandleDialogueDown;
            gameInput.OnDialogueSelect += HandleDialogueSelect;
            gameInput.OnDialogueClose += HandleDialogueClose;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.OnDialogueUp -= HandleDialogueUp;
            gameInput.OnDialogueDown -= HandleDialogueDown;
            gameInput.OnDialogueSelect -= HandleDialogueSelect;
            gameInput.OnDialogueClose -= HandleDialogueClose;
        }
    }

    // ѕублична€ проверка: можно ли вообще стартовать этот диалог сейчас
    public bool CanStartDialogue(DialogueData dialogueData, IDialogueSource source = null)
    {
        if (dialogueData == null)
            return false;

        if (isDialogueActive)
            return false;

        DialogueContext context = BuildContext(source);

        // ѕровер€ем услови€ всего диалога
        if (!AreConditionsMet(dialogueData.Conditions, context))
            return false;

        // ѕровер€ем, есть ли вообще хот€ бы один валидный стартовый узел
        int startNodeIndex = dialogueData.GetStartNodeIndex();
        if (startNodeIndex < 0)
            return false;

        int validNodeIndex = FindFirstValidNodeFrom(startNodeIndex, dialogueData, context);
        return validNodeIndex >= 0;
    }

    public bool StartDialogue(DialogueData dialogueData, IDialogueSource source = null)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("StartDialogue called with null dialogueData.");
            return false;
        }

        if (isDialogueActive)
        {
            Debug.LogWarning("Cannot start dialogue: another dialogue is already active.");
            return false;
        }

        DialogueContext context = BuildContext(source);

        if (!AreConditionsMet(dialogueData.Conditions, context))
        {
            Debug.Log($"Dialogue {dialogueData.name} cannot start because dialogue conditions are not met.");
            return false;
        }

        int startNodeIndex = dialogueData.GetStartNodeIndex();
        if (startNodeIndex < 0)
        {
            Debug.LogWarning($"Dialogue {dialogueData.name} has no start node.");
            return false;
        }

        int firstValidNodeIndex = FindFirstValidNodeFrom(startNodeIndex, dialogueData, context);
        if (firstValidNodeIndex < 0)
        {
            Debug.LogWarning($"Dialogue {dialogueData.name} has no valid node to start from.");
            return false;
        }

        currentDialogue = dialogueData;
        currentContext = context;
        currentNodeIndex = firstValidNodeIndex;
        selectedChoiceIndex = 0;
        isDialogueActive = true;

        if (gameInput != null)
        {
            gameInput.SwitchToDialogueMode();
        }

        if (dialogueUI != null)
        {
            dialogueUI.Show();
        }

        ShowCurrentNode();
        return true;
    }

    public void CloseDialogue()
    {
        if (!isDialogueActive)
            return;

        currentDialogue = null;
        currentContext = null;
        currentNodeIndex = -1;
        selectedChoiceIndex = 0;
        isDialogueActive = false;
        availableChoices.Clear();

        if (dialogueUI != null)
        {
            dialogueUI.ClearChoices();
            dialogueUI.SetSpeakerName(string.Empty);
            dialogueUI.SetDialogueText(string.Empty);
            dialogueUI.SetPortrait(null);
            dialogueUI.Hide();
        }

        if (gameInput != null)
        {
            gameInput.SwitchToPlayerMode();
        }
    }

    private DialogueContext BuildContext(IDialogueSource source)
    {
        return new DialogueContext(source, debugPlayerLevel, questProvider);
    }

    private void ShowCurrentNode()
    {
        if (!isDialogueActive || currentDialogue == null)
        {
            CloseDialogue();
            return;
        }

        DialogueNodeData node = GetCurrentValidNode();
        if (node == null)
        {
            Debug.LogWarning("No valid current dialogue node was found. Closing dialogue.");
            CloseDialogue();
            return;
        }

        selectedChoiceIndex = 0;
        BuildAvailableChoices(node);
        RefreshCurrentNodeUI(node);
    }

    // ¬озвращает текущий узел, а если он невалиден Ч пытаетс€ найти следующий валидный
    private DialogueNodeData GetCurrentValidNode()
    {
        if (currentDialogue == null)
            return null;

        DialogueNodeData currentNode = currentDialogue.GetNode(currentNodeIndex);
        if (currentNode != null && AreConditionsMet(currentNode.Conditions, currentContext))
        {
            return currentNode;
        }

        int nextValidIndex = FindFirstValidNodeFrom(currentNodeIndex + 1, currentDialogue, currentContext);
        if (nextValidIndex < 0)
            return null;

        currentNodeIndex = nextValidIndex;
        return currentDialogue.GetNode(currentNodeIndex);
    }

    private void BuildAvailableChoices(DialogueNodeData node)
    {
        availableChoices.Clear();

        if (node == null || node.NodeType != DialogueNodeType.Choice)
            return;

        for (int i = 0; i < node.Choices.Count; i++)
        {
            DialogueChoiceData choice = node.Choices[i];

            if (AreConditionsMet(choice.Conditions, currentContext))
            {
                availableChoices.Add(choice);
            }
        }
    }

    private async void RefreshCurrentNodeUI(DialogueNodeData node)
    {
        if (dialogueUI == null || node == null)
            return;

        string speakerName = await ResolveSpeakerName(node);
        string dialogueText = await ResolveLocalizedString(node.DialogueText);
        Sprite portrait = ResolvePortrait(node);

        dialogueUI.SetSpeakerName(speakerName);
        dialogueUI.SetDialogueText(dialogueText);
        dialogueUI.SetPortrait(portrait);

        if (node.NodeType == DialogueNodeType.Choice)
        {
            List<string> choiceStrings = new List<string>();

            for (int i = 0; i < availableChoices.Count; i++)
            {
                string choiceText = await ResolveLocalizedString(availableChoices[i].ChoiceText);
                choiceStrings.Add(choiceText);
            }

            if (choiceStrings.Count == 0)
            {
                Debug.LogWarning("Choice node has no available choices. Closing dialogue.");
                CloseDialogue();
                return;
            }

            dialogueUI.SetChoices(choiceStrings, selectedChoiceIndex);
        }
        else
        {
            dialogueUI.ClearChoices();
        }
    }

    private async System.Threading.Tasks.Task<string> ResolveSpeakerName(DialogueNodeData node)
    {
        if (node == null)
            return string.Empty;

        LocalizedString localizedName = null;

        if (node.SpeakerMode == DialogueSpeakerMode.UseCustomName)
        {
            localizedName = node.CustomSpeakerName;
        }
        else
        {
            localizedName = currentContext?.GetSourceSpeakerName();
        }

        return await ResolveLocalizedString(localizedName);
    }

    private Sprite ResolvePortrait(DialogueNodeData node)
    {
        if (node == null)
            return null;

        if (node.SpeakerPortrait != null)
            return node.SpeakerPortrait;

        return currentContext?.GetSourcePortrait();
    }

    private async System.Threading.Tasks.Task<string> ResolveLocalizedString(LocalizedString localizedString)
    {
        if (localizedString == null)
            return string.Empty;

        try
        {
            return await localizedString.GetLocalizedStringAsync().Task;
        }
        catch
        {
            return string.Empty;
        }
    }

    private void HandleDialogueUp()
    {
        if (!isDialogueActive || availableChoices.Count == 0)
            return;

        selectedChoiceIndex--;
        if (selectedChoiceIndex < 0)
        {
            selectedChoiceIndex = availableChoices.Count - 1;
        }

        RefreshChoiceSelection();
    }

    private void HandleDialogueDown()
    {
        if (!isDialogueActive || availableChoices.Count == 0)
            return;

        selectedChoiceIndex++;
        if (selectedChoiceIndex >= availableChoices.Count)
        {
            selectedChoiceIndex = 0;
        }

        RefreshChoiceSelection();
    }

    private async void RefreshChoiceSelection()
    {
        if (dialogueUI == null)
            return;

        List<string> choiceStrings = new List<string>();

        for (int i = 0; i < availableChoices.Count; i++)
        {
            string choiceText = await ResolveLocalizedString(availableChoices[i].ChoiceText);
            choiceStrings.Add(choiceText);
        }

        dialogueUI.SetChoices(choiceStrings, selectedChoiceIndex);
    }

    private void HandleDialogueSelect()
    {
        if (!isDialogueActive || currentDialogue == null)
            return;

        DialogueNodeData node = GetCurrentValidNode();
        if (node == null)
        {
            CloseDialogue();
            return;
        }

        if (node.NodeType == DialogueNodeType.Line)
        {
            GoToNode(node.NextNodeIndex);
        }
        else if (node.NodeType == DialogueNodeType.Choice)
        {
            if (availableChoices.Count == 0)
            {
                CloseDialogue();
                return;
            }

            if (selectedChoiceIndex < 0 || selectedChoiceIndex >= availableChoices.Count)
            {
                Debug.LogWarning("Selected choice index is out of range.");
                CloseDialogue();
                return;
            }

            DialogueChoiceData selectedChoice = availableChoices[selectedChoiceIndex];
            GoToNode(selectedChoice.NextNodeIndex);
        }
    }

    private void HandleDialogueClose()
    {
        if (!isDialogueActive)
            return;

        CloseDialogue();
    }

    private void GoToNode(int nodeIndex)
    {
        if (nodeIndex < 0)
        {
            CloseDialogue();
            return;
        }

        if (currentDialogue == null)
        {
            CloseDialogue();
            return;
        }

        int validNodeIndex = FindFirstValidNodeFrom(nodeIndex, currentDialogue, currentContext);
        if (validNodeIndex < 0)
        {
            Debug.LogWarning($"Could not find a valid node starting from index {nodeIndex}. Dialogue will be closed.");
            CloseDialogue();
            return;
        }

        currentNodeIndex = validNodeIndex;
        selectedChoiceIndex = 0;
        ShowCurrentNode();
    }

    private int FindFirstValidNodeFrom(int startIndex, DialogueData dialogueData, DialogueContext context)
    {
        if (dialogueData == null)
            return -1;

        if (startIndex < 0)
            return -1;

        for (int i = startIndex; i < dialogueData.Nodes.Count; i++)
        {
            DialogueNodeData node = dialogueData.GetNode(i);

            if (node == null)
                continue;

            if (AreConditionsMet(node.Conditions, context))
            {
                return i;
            }
        }

        return -1;
    }

    private bool AreConditionsMet(IReadOnlyList<DialogueConditionData> conditions, DialogueContext context)
    {
        if (conditions == null || conditions.Count == 0)
            return true;

        for (int i = 0; i < conditions.Count; i++)
        {
            if (!IsConditionMet(conditions[i], context))
                return false;
        }

        return true;
    }

    private bool IsConditionMet(DialogueConditionData condition, DialogueContext context)
    {
        if (condition == null)
            return true;

        switch (condition.ConditionType)
        {
            case DialogueConditionType.None:
                return true;

            case DialogueConditionType.PlayerLevelAtLeast:
                return context != null && context.GetPlayerLevel() >= condition.RequiredLevel;

            case DialogueConditionType.HasItem:
                return InventorySystem.Instance != null &&
                       condition.RequiredItem != null &&
                       InventorySystem.Instance.HasItem(condition.RequiredItem, condition.RequiredItemAmount);

            case DialogueConditionType.DoesNotHaveItem:
                return InventorySystem.Instance != null &&
                       condition.RequiredItem != null &&
                       !InventorySystem.Instance.HasItem(condition.RequiredItem, condition.RequiredItemAmount);

            case DialogueConditionType.PlayOnce:
                return DialogueRuntimeState.Instance == null ||
                       !DialogueRuntimeState.Instance.HasPlayed(condition.OnceKey);

            case DialogueConditionType.QuestState:
                if (context == null || context.QuestProvider == null)
                    return false;

                return context.QuestProvider.IsQuestStateMatched(condition.QuestId, condition.RequiredQuestState);

            default:
                return true;
        }
    }

    public void SetDebugPlayerLevel(int level)
    {
        debugPlayerLevel = Mathf.Max(1, level);
    }
}