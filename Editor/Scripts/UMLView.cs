using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class UMLView : GraphView
{
    public new class UxmlFactory : UxmlFactory<UMLView, GraphView.UxmlTraits>
    {
    }
    

    public List<UMLNode> nodes = new List<UMLNode>();

    public UMLView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.lucecita.sdk/Editor/USS/UMLGenerator.uss");
        styleSheets.Add(styleSheet);

        SpawnPos = Vector2.zero;
    }
    
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node)
            .ToList();
    }

    // 실제 visualElement 생성
    public IEnumerator PopulateView(List<ScriptInfo> scriptInfoList)
    {
        nodes.Clear();
        DeleteElements(graphElements);

        scriptInfoList.ForEach(GetNode);
        scriptInfoList.ForEach(GetReferencedNode);
        yield return new WaitForEndOfFrame();
        // nodes.ForEach(CreatePort);
        nodes.ForEach(node => node.ConnectInput(nodes));
    }

    private void GetNode(ScriptInfo scriptInfo)
    {
        UMLNode umlNode = CreateScriptNode(scriptInfo.scriptName,scriptInfo);
    }

    private void GetReferencedNode(ScriptInfo scriptInfo)
    {
        int referenceTypeCount = scriptInfo.referenceScriptInfos.Count;
        if (referenceTypeCount > 0)
        {
            for (int i = 0; i < referenceTypeCount; i++)
            {
                CreateScriptNode(scriptInfo.referenceScriptInfos[i].scriptName,scriptInfo.referenceScriptInfos[i]);
            }
        }
    }

    private void CreatePort(UMLNode umlNode)
    {
        umlNode.CreateInputPorts();
        // scriptNode.CreateLeftPorts();
        // scriptNode.CreateRightPorts();
        umlNode.CreateOutputPorts();
    }

    private UMLNode CreateScriptNode(string scriptName,ScriptInfo info)
    {
        scriptName = scriptName.Replace("`1", "<T>");
        bool isContained = nodes.Exists(node => node.title == scriptName);
        if (!isContained)
        {
            Node node = new Node();
            node.title = scriptName;
            UMLNode umlNode = new UMLNode(node,this,info);
            nodes.Add(umlNode);
            AddElement(umlNode);

            // 노드 라벨 생성
            // Label nodeLabel = new Label("Label");
            // nodeLabel.style.backgroundColor = new StyleColor(Color.grey);
            // scriptNode.Add(nodeLabel);

            EditorCoroutineUtility.StartCoroutine(UpdateNodePosition(umlNode), this);

            return umlNode;
        }
        return null;
    }

    private Node preNode;
    private Vector2 SpawnPos;
    public static float nodeSpacing = 40;

    // 임시
    private IEnumerator UpdateNodePosition(Node node)
    {
        yield return new WaitForEndOfFrame();

        if (SpawnPos == Vector2.zero)
        {
            float width = nodeSpacing;
            float height = (this.resolvedStyle.height - node.resolvedStyle.height) / 2;
            SpawnPos = new Vector2(width, height);
        }
        else
        {
            float width = SpawnPos.x + preNode.resolvedStyle.width + nodeSpacing;
            float height = SpawnPos.y;
            float WidthThreshold = resolvedStyle.width - node.resolvedStyle.width - nodeSpacing;
            if (width > WidthThreshold)
            {
                float nextRowX = nodeSpacing;
                float nextRowY = height + preNode.resolvedStyle.height + nodeSpacing;
                SpawnPos = new Vector2(nextRowX, nextRowY);
            }
            else
            {
                SpawnPos = new Vector2(width, height);
            }
        }

        StyleLength leftPos = new StyleLength(new Length(SpawnPos.x));
        StyleLength topPos = new StyleLength(new Length(SpawnPos.y));
        node.style.left = leftPos;
        node.style.top = topPos;

        preNode = node;
    }
}