using UnityEngine;

public class DialogueTester : MonoBehaviour
{
    [SerializeField] private DialogueData testDialogue;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(testDialogue);
            }
        }
    }
}