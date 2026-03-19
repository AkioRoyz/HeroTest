using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue_", menuName = "Game/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Settings")]
    [SerializeField] private string dialogueId;

    [Tooltip("≈сли true, этот диалог можно запускать много раз.")]
    [SerializeField] private bool repeatable = true;

    [Header("Conditions")]
    [SerializeField] private List<DialogueConditionData> conditions = new();

    [Header("Nodes")]
    [SerializeField] private List<DialogueNodeData> nodes = new();

    public string DialogueId => dialogueId;
    public bool Repeatable => repeatable;
    public IReadOnlyList<DialogueConditionData> Conditions => conditions;
    public IReadOnlyList<DialogueNodeData> Nodes => nodes;

    public DialogueNodeData GetNode(int index)
    {
        if (index < 0 || index >= nodes.Count)
        {
            Debug.LogWarning($"DialogueData: invalid node index {index} in dialogue {name}");
            return null;
        }

        return nodes[index];
    }

    public int GetStartNodeIndex()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].IsStartNode)
                return i;
        }

        Debug.LogWarning($"DialogueData: start node was not found in dialogue {name}");
        return -1;
    }
}