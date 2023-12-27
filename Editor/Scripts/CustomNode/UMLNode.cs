using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    private UMLView m_umlView;
    private VisualElement root;
    private VisualElement divider;
    private Label dependencyInfoLabel;

    public UMLNode(Node ownerNode, UMLView mUmlView, ScriptInfo scriptInfo) : base(
        "Packages/com.lucecita.sdk/Editor/Uxml/NodeView.uxml")
    {
        this.ownerNode = ownerNode;
        this.title = ownerNode.title;
        this.m_umlView = mUmlView;
        this.scriptInfo = scriptInfo;
        this.border = this.Q<VisualElement>(UMLNodeView.k_border);
        this.divider = this.Q<VisualElement>(UMLNodeView.k_divider);
        this.dependencyInfoLabel = this.Q<Label>(UMLNodeView.k_labelDependencyInfo);


        root = UMLGenerator.root;
    }

    public void ShowDependencyInfoLabel(DependencyType dependencyType)
    {
        switch (dependencyType)
        {
            case DependencyType.Parent:
                dependencyInfoLabel.text = "의 부모";
                dependencyInfoLabel.style.color = new StyleColor(Color.black);
                break;

            case DependencyType.Child:
                dependencyInfoLabel.text = "의 자식";
                dependencyInfoLabel.style.color = new StyleColor(Color.yellow);
                break;

            case DependencyType.Referring:
                dependencyInfoLabel.text = "에게 참조되어짐";
                dependencyInfoLabel.style.color = new StyleColor(Color.blue);
                break;

            case DependencyType.Referenced:
                dependencyInfoLabel.text = "를 참조";
                dependencyInfoLabel.style.color = new StyleColor(Color.green);
                break;

            case DependencyType.ReferringAndReferenced:
                dependencyInfoLabel.text = "를 서로 참조";
                dependencyInfoLabel.style.color = new StyleColor(Color.red);
                break;
        }

        dependencyInfoLabel.style.display = DisplayStyle.Flex;
    }

    public void HideDependencyInfoLabel()
    {
        dependencyInfoLabel.style.display = DisplayStyle.None;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Analyze With GPT", OnClickAnalyzeWithGPT());
        evt.menu.AppendAction("Show Dependencies about this Node", OnClickShowDependencies());
    }

    private Action<DropdownMenuAction> OnClickAnalyzeWithGPT()
    {
        return (action) => AnalyzeScript();
    }

    private Action<DropdownMenuAction> OnClickShowDependencies()
    {
        return (action) => ShowDependencies();
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
        string ilCode = CodeReader.GetTypeCode(scriptType);

        GPTStreamHandler.apiKey = apiKey;
        GPTStreamHandler.RequestData requestData = new GPTStreamHandler.RequestData();
        GPTStreamHandler.Message systemMessage = new GPTStreamHandler.Message() { role = "system", content = "너는 유니티 C# 코드 분석가야." };
        GPTStreamHandler.Message systemMessage2 = new GPTStreamHandler.Message() { role = "system", content = "너는 IL 코드와 C# 코드를 해석하여 기능을 요약할거야" };
        GPTStreamHandler.Message systemMessage3 = new GPTStreamHandler.Message() { role = "system", content = "너는 답변할 때 1. 요약 2. 피드백 2개로 나누어 답변할 거야." };
        // GPTStreamHandler.Message systemMessage4 = new GPTStreamHandler.Message() { role = "system", content = "너는 답변할 때 변수와 함수를 나열하지 않을 거야"};
        // GPTStreamHandler.Message systemMessage5 = new GPTStreamHandler.Message() { role = "system", content = "너는 답변할 때 들여쓰기에 대해 피드백 할 필요 없어"};
        // GPTStreamHandler.Message systemMessage6 = new GPTStreamHandler.Message() { role = "system", content = "너는 답변할 때 클래스 코드를 나열하지 않을 거야"};
        GPTStreamHandler.Message systemMessage7 = new GPTStreamHandler.Message() { role = "system", content = "너는 답변할 때 네임스페이스는 생략할 거야" };
        // GPTStreamHandler.Message systemMessage8 = new GPTStreamHandler.Message() { role = "system", content = "500자 이내로 답변해줘"};
        GPTStreamHandler.Message requestMessage = new GPTStreamHandler.Message() { role = "user", content = ilCode };
        requestData.messages = new List<GPTStreamHandler.Message>() { systemMessage, systemMessage2, systemMessage3, systemMessage7, requestMessage };
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

    private void ShowDependencies()
    {
        EditorCoroutineUtility.StartCoroutine(m_umlView.PopulateView(new List<ScriptInfo>() { scriptInfo }, ViewOption.All), this);
    }


    public override void Select(VisualElement selectionContainer, bool additive)
    {
        base.Select(selectionContainer, additive);
        foreach (UMLNode node in m_umlView.nodes)
        {
            // 종속 정보 라벨 Hide
            node.HideDependencyInfoLabel();

            node.border.style.borderTopColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderBottomColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderLeftColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderRightColor = new Color(1f, 0.7921569f, 0.1843137f);
            node.border.style.borderTopWidth = 1;
            node.border.style.borderBottomWidth = 1;
            node.border.style.borderLeftWidth = 1;
            node.border.style.borderRightWidth = 1;

            bool isReferring = node.scriptInfo.referringScriptInfos.Exists(info => info == this.scriptInfo);
            bool isReferenced = node.scriptInfo.referencedScriptInfos.Exists(info => info == this.scriptInfo);
            if ((isReferring || isReferenced) && node != this)
            {
                // 종속 정보 라벨 Show
                DependencyType dependencyType = GetDependencyType(node.scriptInfo);
                node.ShowDependencyInfoLabel(dependencyType);
            }

            if (node.edges.Count == 0)
                continue;

            foreach (var edge in node.edges)
            {
                bool isDependenceNode = edge.TargetNode == this && node != this;
                foreach (var line in edge.edgeElements)
                {
                    // 참조받는 라인 포커스
                    if (isDependenceNode)
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

    private DependencyType GetDependencyType(ScriptInfo scriptInfo)
    {
        bool isReferenced = scriptInfo.referencedScriptInfos.Exists(info => info == this.scriptInfo);
        bool isReferring = scriptInfo.referringScriptInfos.Exists(info => info == this.scriptInfo);

        if (isReferring && isReferenced)
        {
            return DependencyType.ReferringAndReferenced;
        }
        else if (isReferring)
        {
            return DependencyType.Referring;
        }
        else if (isReferenced)
        {
            return DependencyType.Referenced;
        }
        else
        {
            return DependencyType.None;
        }
    }

    public void ConnectInput(List<UMLNode> nodes)
    {
        var targetInfos = scriptInfo.referringScriptInfos.Where(info => nodes.Any(node =>
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