using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EdgeConnectorListener : IEdgeConnectorListener
{
    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        // Можно не реализовывать для простого редактора
    }

    public void OnDrop(GraphView graphView, Edge edge)
    {
        // Подключаем линию к портам
        edge.input.Connect(edge);
        edge.output.Connect(edge);

        graphView.AddElement(edge);
    }
}