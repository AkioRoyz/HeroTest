using UnityEngine;
using UnityEngine.Localization;

public class NpcDialogueInteractable : MonoBehaviour, IDialogueSource
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Speaker")]
    [SerializeField] private DialogueSpeakerData speakerData;

    [Header("NPC Quest Id")]
    [Tooltip("Уникальный ID NPC для квестов. Например: elder_npc, guard_01")]
    [SerializeField] private string npcId;

    [Header("NPC Role")]
    [SerializeField] private DialogueNpcRole npcRole = DialogueNpcRole.Regular;

    [Header("Quest")]
    [SerializeField] private string npcId;

    [Header("Interaction")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject interactionHintObject;

    private bool isPlayerInside;
    private int playerLayer;

    public DialogueNpcRole NpcRole => npcRole;
    public string NpcId => npcId;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.OnUse += HandleUsePressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.OnUse -= HandleUsePressed;
        }
    }

    private void Start()
    {
        RefreshHint();
    }

    private void Update()
    {
        if (isPlayerInside)
        {
            RefreshHint();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer)
            return;

        isPlayerInside = true;
        RefreshHint();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer)
            return;

        isPlayerInside = false;
        RefreshHint();
    }

    private void HandleUsePressed()
    {
        if (!isPlayerInside)
            return;

        if (dialogueData == null)
        {
            Debug.LogWarning($"{name}: dialogueData is missing.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager.Instance is missing.");
            return;
        }

        if (!DialogueManager.Instance.CanStartDialogue(dialogueData, this))
        {
            RefreshHint();
            return;
        }

        bool started = DialogueManager.Instance.StartDialogue(dialogueData, this);

        if (started)
        {
<<<<<<< HEAD
            if (QuestSystem.Instance != null && !string.IsNullOrWhiteSpace(npcId))
            {
                QuestSystem.Instance.RegisterNpcTalked(npcId);
=======
            if (QuestManager.Instance != null && !string.IsNullOrWhiteSpace(npcId))
            {
                QuestManager.Instance.NotifyNpcTalked(npcId);
>>>>>>> recovery
            }

            RefreshHint();
        }
    }

    private void RefreshHint()
    {
        if (interactionHintObject == null)
            return;

        bool shouldShow =
            isPlayerInside &&
            dialogueData != null &&
            DialogueManager.Instance != null &&
            DialogueManager.Instance.CanStartDialogue(dialogueData, this);

        interactionHintObject.SetActive(shouldShow);
    }

    public LocalizedString GetDialogueSpeakerName()
    {
        if (speakerData == null)
            return null;

        return speakerData.SpeakerName;
    }

    public Sprite GetDialoguePortrait()
    {
        if (speakerData == null)
            return null;

        return speakerData.Portrait;
    }
}