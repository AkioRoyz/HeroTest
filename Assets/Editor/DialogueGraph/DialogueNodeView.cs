using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class DialogueNodeView : Node
{
    public DialogueNode dialogueNode;

    public DialogueNodeView(DialogueNode node = null)
    {
        dialogueNode = node ?? new DialogueNode();

        // Название узла
        title = dialogueNode.dialogueText?.GetLocalizedString() ?? "Dialogue Node";

        // Входной порт
        Port input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        input.portName = "Input";
        inputContainer.Add(input);

        // Выходной порт
        Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        output.portName = "Output";
        outputContainer.Add(output);

        RefreshExpandedState();
        RefreshPorts();
    }
}