using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class UMLNode : Node
{
    private ScriptInfo scriptInfo;
    private Node ownerNode;
    public List<UMLEdge> edges = new List<UMLEdge>();
    public VisualElement border;
    public Port inputPort;
    public Port outputPort;
    public Port leftPort;
    public Port rightPort;
    private UMLView m_umlView;

    public UMLNode(Node ownerNode, UMLView mUmlView, ScriptInfo scriptInfo) : base("Packages/com.lucecita.sdk/Editor/Uxml/NodeView.uxml")
    {
        this.ownerNode = ownerNode;
        this.title = ownerNode.title;
        this.m_umlView = mUmlView;
        this.scriptInfo = scriptInfo;
        this.border = this.Q<VisualElement>("node-border");
    }

    public override void Select(VisualElement selectionContainer, bool additive)
    {
        base.Select(selectionContainer, additive);

        foreach (UMLNode node in m_umlView.nodes)
        {
            if (node.edges.Count == 0)
                continue;

            foreach (var edge in node.edges)
            {
                foreach (var line in edge.edgeElements)
                {
                    // 참조받는 라인 포커스
                    if (edge.TargetNode == this && node != this)
                    {
                        edge.TargetNode.Add(edge);
                        edge.style.top = edge.ParentNode.resolvedStyle.top - edge.TargetNode.resolvedStyle.top - edge.TargetNode.border.resolvedStyle.height + 0.5f; // 0.5f를 더해야 하는 이유 찾아야 함
                        edge.style.left = edge.ParentNode.resolvedStyle.left - edge.TargetNode.resolvedStyle.left;

                        line.Select();
                    }
                    else
                    {
                        if (edge.parent != edge.ParentNode)
                        {
                            edge.BackOriginEdgeParent();
                        }

                        line.UnSelect();
                    }
                }
            }
        }

        edges.ForEach(edge => edge.edgeElements.ForEach(line => line.Select()));
    }

    public void CreateInputPorts()
    {
        inputPort = ownerNode.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi,
            typeof(UMLNode));

        if (inputPort != null)
        {
            inputPort.portName = "";
            inputPort.style.flexDirection = FlexDirection.Column;
            inputContainer.Add(inputPort);
        }
    }

    public void CreateOutputPorts()
    {
        outputPort =
            ownerNode.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(UMLNode));

        if (outputPort != null)
        {
            outputPort.portName = "";
            outputPort.style.flexDirection = FlexDirection.ColumnReverse;
            outputContainer.Add(outputPort);
        }
    }

    public void CreateLeftPorts()
    {
        leftPort = ownerNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi,
            typeof(UMLNode));

        if (leftPort != null)
        {
            leftPort.portName = "";
            leftPort.style.flexDirection = FlexDirection.ColumnReverse;
            VisualElement left = this.Q<VisualElement>("left");
            left.Add(leftPort);
        }
    }

    public void CreateRightPorts()
    {
        rightPort = ownerNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
            typeof(UMLNode));

        if (rightPort != null)
        {
            rightPort.portName = "";
            rightPort.style.flexDirection = FlexDirection.ColumnReverse;
            VisualElement right = this.Q<VisualElement>("right");
            right.Add(rightPort);
        }
    }

    public void ConnectInput(List<UMLNode> nodes)
    {
        var targetInfos = scriptInfo.referenceScriptInfos.Where(info => nodes.Any(node =>
            EqualScriptName(node.title, info.scriptName) &&
            EqualScriptName(ownerNode.title, info.scriptName) == false));

        foreach (var info in targetInfos)
        {
            UMLNode targetNode = nodes.FirstOrDefault(node => EqualScriptName(node.title, info.scriptName));
            UMLEdge umlEdge = new UMLEdge(targetNode, this, m_umlView);
            edges.Add(umlEdge);
            Insert(0, umlEdge);
        }
    }

    private bool EqualScriptName(string value1, string value2)
    {
        if (value1.Contains("<T>"))
        {
            value1 = value1.Replace("<T>", "`1");
        }
        else if (value1.Contains("`1"))
        {
            value1 = value1.Replace("`1", "<T>");
        }

        if (value1 == value2)
            return true;

        else
            return false;
    }
}