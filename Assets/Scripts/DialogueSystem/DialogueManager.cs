using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private DialogueUI dialogueUI;

    private DialogueData currentDialogue;
    private DialogueContext currentContext;
    private int currentNodeIndex = -1;
    private int selectedChoiceIndex = 0;
    private bool isDialogueActive = false;

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

        int startNodeIndex = dialogueData.GetStartNodeIndex();
        if (startNodeIndex < 0)
        {
            Debug.LogWarning($"Dialogue {dialogueData.name} has no valid start node.");
            return false;
        }

        currentDialogue = dialogueData;
        currentContext = new DialogueContext(source);
        currentNodeIndex = startNodeIndex;
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

    private void ShowCurrentNode()
    {
        if (!isDialogueActive || currentDialogue == null)
        {
            CloseDialogue();
            return;
        }

        DialogueNodeData node = currentDialogue.GetNode(currentNodeIndex);
        if (node == null)
        {
            Debug.LogWarning("Current dialogue node is null. Closing dialogue.");
            CloseDialogue();
            return;
        }

        selectedChoiceIndex = 0;

        RefreshCurrentNodeUI(node);
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

            for (int i = 0; i < node.Choices.Count; i++)
            {
                string choiceText = await ResolveLocalizedString(node.Choices[i].ChoiceText);
                choiceStrings.Add(choiceText);
            }

            if (choiceStrings.Count == 0)
            {
                Debug.LogWarning("Choice node has no choices. Closing dialogue.");
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
        if (!isDialogueActive || currentDialogue == null)
            return;

        DialogueNodeData node = currentDialogue.GetNode(currentNodeIndex);
        if (node == null || node.NodeType != DialogueNodeType.Choice)
            return;

        int choiceCount = node.Choices.Count;
        if (choiceCount <= 0)
            return;

        selectedChoiceIndex--;
        if (selectedChoiceIndex < 0)
        {
            selectedChoiceIndex = choiceCount - 1;
        }

        RefreshChoiceSelection(node);
    }

    private void HandleDialogueDown()
    {
        if (!isDialogueActive || currentDialogue == null)
            return;

        DialogueNodeData node = currentDialogue.GetNode(currentNodeIndex);
        if (node == null || node.NodeType != DialogueNodeType.Choice)
            return;

        int choiceCount = node.Choices.Count;
        if (choiceCount <= 0)
            return;

        selectedChoiceIndex++;
        if (selectedChoiceIndex >= choiceCount)
        {
            selectedChoiceIndex = 0;
        }

        RefreshChoiceSelection(node);
    }

    private async void RefreshChoiceSelection(DialogueNodeData node)
    {
        if (dialogueUI == null || node == null || node.NodeType != DialogueNodeType.Choice)
            return;

        List<string> choiceStrings = new List<string>();

        for (int i = 0; i < node.Choices.Count; i++)
        {
            string choiceText = await ResolveLocalizedString(node.Choices[i].ChoiceText);
            choiceStrings.Add(choiceText);
        }

        dialogueUI.SetChoices(choiceStrings, selectedChoiceIndex);
    }

    private void HandleDialogueSelect()
    {
        if (!isDialogueActive || currentDialogue == null)
            return;

        DialogueNodeData node = currentDialogue.GetNode(currentNodeIndex);
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
            if (node.Choices.Count == 0)
            {
                CloseDialogue();
                return;
            }

            if (selectedChoiceIndex < 0 || selectedChoiceIndex >= node.Choices.Count)
            {
                Debug.LogWarning("Selected choice index is out of range.");
                CloseDialogue();
                return;
            }

            DialogueChoiceData selectedChoice = (DialogueChoiceData)node.Choices[selectedChoiceIndex];
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

        currentNodeIndex = nodeIndex;
        selectedChoiceIndex = 0;
        ShowCurrentNode();
    }
}