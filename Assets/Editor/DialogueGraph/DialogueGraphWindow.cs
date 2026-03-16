using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphWindow : EditorWindow
{
    private DialogueGraphView graphView;

    [MenuItem("Tools/Dialogue Graph")]
    public static void OpenWindow()
    {
        GetWindow<DialogueGraphWindow>("Dialogue Graph");
    }

    private void OnEnable()
    {
        CreateGraphView();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    void CreateGraphView()
    {
        graphView = new DialogueGraphView();

        graphView.StretchToParentSize();

        rootVisualElement.Add(graphView);
    }
}