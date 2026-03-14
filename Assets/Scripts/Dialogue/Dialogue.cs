using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    // Первый узел диалога
    public DialogueNode startNode;
}