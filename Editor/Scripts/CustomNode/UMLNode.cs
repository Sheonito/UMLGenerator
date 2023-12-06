using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using PlasticGui;
using UMLAutoGenerator;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class UMLNode : Node
{
    public ScriptInfo scriptInfo;
    private Node ownerNode;
    public List<UMLEdge> edges = new List<UMLEdge>();
    public VisualElement border;
    public Port inputPort;
    public Port outputPort;
    public Port leftPort;
    public Port rightPort;
    private UMLView m_umlView;
    private VisualElement root;

    public UMLNode(Node ownerNode, UMLView mUmlView, ScriptInfo scriptInfo) : base(
        "Packages/com.lucecita.sdk/Editor/Uxml/NodeView.uxml")
    {
        this.ownerNode = ownerNode;
        this.title = ownerNode.title;
        this.m_umlView = mUmlView;
        this.scriptInfo = scriptInfo;
        this.border = this.Q<VisualElement>("node-border");
        root = UMLGenerator.root;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Analyze With GPT", OnClickAnalyzeWithGPT());
    }

    private Action<DropdownMenuAction> OnClickAnalyzeWithGPT()
    {
        return (action) => AnalyzeScript();
    }

    private async void AnalyzeScript()
    {
        string apiKey = PlayerPrefs.GetString("UMLGeneratorAPIKey");
        if (string.IsNullOrEmpty(apiKey))
        {
            GPTKeyInputWindow.ShowWindow(AnalyzeScript);
            return;
        }

        TextField consoleTextField = root.Q<TextField>(UMLGeneratorView.k_consoleTextFieldGPT);
        ConsoleView consoleView = root.Q<ConsoleView>(UMLGeneratorView.k_consoleView);
        consoleView.ShowConsoleView();

        Type scriptType = scriptInfo.scriptType;
        string ilCode = CodeReader.GetILCode(scriptType);

        GPTStreamHandler.apiKey = apiKey;
        GPTStreamHandler.RequestData requestData = new GPTStreamHandler.RequestData();
        GPTStreamHandler.Message systemMessage = new GPTStreamHandler.Message()
        {
            role = "system",
            content = "너는 내가 짠 유니티 C#코드를 IL Code로 요청을 받을 것이고 그것을 클래스로 바꿀거야."
        };
        GPTStreamHandler.Message systemMessage2 = new GPTStreamHandler.Message()
        {
            role = "system",
            content = "코드에 대한 요약과 피드백을 줘. 반드시 카테고리를 요약과 피드백 2개로 나누어서 말해야만 해"
        };
        GPTStreamHandler.Message requestMessage = new GPTStreamHandler.Message() { role = "user", content = ilCode };
        requestData.messages = new List<GPTStreamHandler.Message>() { systemMessage, systemMessage2,requestMessage };
        GPTStreamHandler.Message responseMessage = new GPTStreamHandler.Message() { role = "system", content = null };
        CancellationTokenSource cts = new CancellationTokenSource();
        await foreach (var chunk in GPTStreamHandler.CreateCompletionRequestAsStream(requestData, cts.Token))
        {
            string role = chunk.choices[0].delta.role;

            if (role != null)
            {
                responseMessage.role = role;
            }

            responseMessage.content += chunk.choices[0].delta.content;
            consoleTextField.value = responseMessage.content;
        }
    }


    public override void Select(VisualElement selectionContainer, bool additive)
    {
        base.Select(selectionContainer, additive);
        foreach (UMLNode node in m_umlView.nodes)
        {
            node.border.style.borderTopColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderBottomColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderLeftColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderRightColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderTopWidth = 1;
            node.border.style.borderBottomWidth = 1;
            node.border.style.borderLeftWidth = 1;
            node.border.style.borderRightWidth = 1;

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
                        edge.style.top = edge.ParentNode.resolvedStyle.top - edge.TargetNode.resolvedStyle.top -
                            edge.TargetNode.border.resolvedStyle.height + 0.5f; // 0.5f를 더해야 하는 이유 찾아야 함
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

        border.style.borderTopColor = new Color(0.3840512f, 0.770284f, 0.8773585f);
        border.style.borderBottomColor = new Color(0.3840512f, 0.770284f, 0.8773585f);
        border.style.borderLeftColor = new Color(0.3840512f, 0.770284f, 0.8773585f);
        border.style.borderRightColor = new Color(0.3840512f, 0.770284f, 0.8773585f);
        border.style.borderTopWidth = 2;
        border.style.borderBottomWidth = 2;
        border.style.borderLeftWidth = 2;
        border.style.borderRightWidth = 2;

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