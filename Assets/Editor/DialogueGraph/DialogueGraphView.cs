using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class DialogueGraphView : GraphView
{
    public DialogueGraphView()
    {
        // Растягиваем GraphView на всё окно
        style.flexGrow = 1;

        // Возможность зума
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        // Перемещение по графу
        this.AddManipulator(new ContentDragger());

        // Перетаскивание элементов
        this.AddManipulator(new SelectionDragger());

        // Выделение рамкой
        this.AddManipulator(new RectangleSelector());

        // Добавляем сетку
        GridBackground grid = new GridBackground();
        grid.StretchToParentSize();

        // Добавляем возможность соединять порты линиями
        EdgeConnector<Edge> edgeConnector = new EdgeConnector<Edge>(new EdgeConnectorListener());
        this.AddManipulator(edgeConnector);

        Insert(0, grid);

        nodeCreationRequest = context =>
        {
            CreateNode(context.screenMousePosition);
        };
    }

    public void CreateNode(Vector2 mousePosition)
    {
        // Создаём новый DialogueNode в памяти
        DialogueNode newNode = new DialogueNode();

        // Создаём визуальный узел с этим DialogueNode
        DialogueNodeView nodeView = new DialogueNodeView(newNode);

        Vector2 localPos = contentViewContainer.WorldToLocal(mousePosition);
        nodeView.SetPosition(new Rect(localPos.x, localPos.y, 200, 150));

        AddElement(nodeView);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }
}