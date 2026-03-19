using UnityEngine;
using UnityEngine.Localization;

public class NpcDialogueInteractable : MonoBehaviour, IDialogueSource
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Speaker")]
    [SerializeField] private DialogueSpeakerData speakerData;

    [Header("Interaction")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject interactionHintObject;

    private bool isPlayerInside;
    private int playerLayer;

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
        RefreshHint(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer)
            return;

        isPlayerInside = true;
        RefreshHint(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer)
            return;

        isPlayerInside = false;
        RefreshHint(false);
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

        if (DialogueManager.Instance.IsDialogueActive)
            return;

        bool started = DialogueManager.Instance.StartDialogue(dialogueData, this);

        if (started)
        {
            RefreshHint(false);
        }
    }

    private void RefreshHint(bool visible)
    {
        if (interactionHintObject != null)
        {
            interactionHintObject.SetActive(visible);
        }
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