using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UMLEdge : VisualElement
{
    private UMLView m_umlView;
    public UMLNode ParentNode { get; private set; }
    public UMLNode TargetNode { get; private set; }
    public List<UMLEdgeElement> edgeElements;
    private List<Vector2> points;
    private const float lineSize = 3;
    private const string uiFile = "Packages/com.lucecita.sdk/Editor/Uxml/UMLEdge.uxml";
    private bool isTargetUpNeighbor;

    public UMLEdge(UMLNode targetNode, UMLNode parentNode, UMLView umlView)
    {
        edgeElements = new List<UMLEdgeElement>();
        points = new List<Vector2>();

        this.ParentNode = parentNode;
        this.TargetNode = targetNode;
        m_umlView = umlView;
        EditorCoroutineUtility.StartCoroutine(CreateLines(targetNode), this);
    }

    private IEnumerator CreateLines(UMLNode targetNode)
    {
        yield return new WaitForEndOfFrame();

        // 디버깅
        // Debug.Log("targetNode: " + targetNode.title);
        // Debug.Log("parentNode: " + ParentNode.title);

        CreateFirstLine();
        CreateSecondLine();
        CreateThirdLine();

        if (!isTargetUpNeighbor)
        {
            CreateFirthLine();
            CreateFifthLine();
        }

        CreateDirectionImage();

        m_umlView.RegisterCallback<MouseUpEvent>(SelectClickedLine);
    }

    public void BackOriginEdgeParent()
    {
        ParentNode.Add(this);
        this.SendToBack();
        style.top = 0;
        style.left = 0;
    }

    public void MoveEdgeParent(UMLNode umlNode)
    {
    }


    private void SelectClickedLine(MouseUpEvent evt)
    {
        Vector2 mousePos = evt.mousePosition;
        VisualElement visualElement = UMLGenerator.root.panel.Pick(mousePos);
        if (visualElement.parent == this)
        {
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
                        if (edge.parent != edge.ParentNode)
                        {
                            edge.BackOriginEdgeParent();
                        }

                        line.UnSelect();
                    }
                }
            }

            m_umlView.AddElement(ParentNode);
            ParentNode.Insert(ParentNode.edges.Count - 1, this);

            edgeElements.ForEach(e => e.Select());
        }
    }

    private void CreateDirectionImage()
    {
        VisualTreeAsset edgeTreeAsset = EditorGUIUtility.Load(uiFile) as VisualTreeAsset;
        UMLEdgeElement direction = edgeTreeAsset.CloneTree().Q<UMLEdgeElement>("direction");
        direction.elementType = UMLEdgeElementType.Direction;
        edgeElements.Add(direction);
        Add(direction);

        if (isTargetUpNeighbor)
        {
            direction.style.top = edgeElements[2].top - 2;
            direction.style.left = edgeElements[2].left - 7.5f;
        }
        else
        {
            direction.style.top = edgeElements[4].top - 2;
            direction.style.left = edgeElements[4].left - 7.5f;
        }
    }

    private void CreateFirstLine()
    {
        UMLEdgeElement line = GetLine();
        edgeElements.Add(line);
        Add(line);

        float top = line.resolvedStyle.top - (UMLView.nodeSpacing / 2) - lineSize;
        float height = (UMLView.nodeSpacing / 2) + lineSize;
        line.style.left = Length.Percent(50);
        line.style.top = top;
        line.style.width = new Length(lineSize);
        line.style.height = new Length(height);

        line.top = top;
        line.left = ParentNode.resolvedStyle.width / 2;
        line.Width = lineSize;
        line.height = height;
    }


    private void CreateSecondLine()
    {
        UMLEdgeElement line = GetLine();
        edgeElements.Add(line);
        Add(line);

        float targetTop = TargetNode.resolvedStyle.top;
        float parentTop = ParentNode.resolvedStyle.top;
        float topDiff = parentTop - targetTop;
        isTargetUpNeighbor = targetTop < parentTop && topDiff < (ParentNode.resolvedStyle.height * 2);

        float targetLeft = TargetNode.style.left.value.value;
        float parentLeft = ParentNode.style.left.value.value;
        float parentWidth = ParentNode.resolvedStyle.width;
        float targetWidth = TargetNode.resolvedStyle.width;
        bool isTargetRight = targetLeft > parentLeft;
        float xDiff;

        if (isTargetRight) // 타겟은 오른쪽
        {
            xDiff = targetLeft - parentLeft;
            float width;
            if (isTargetUpNeighbor)
            {
                width = xDiff;
            }
            else
            {
                width = xDiff + targetWidth - (parentWidth / 2) + (UMLView.nodeSpacing / 2) + lineSize;
            }

            line.style.left = Length.Percent(50);
            line.style.width = new Length(width);

            line.left = line.style.left.value.value;
            line.Width = width;
        }
        else // 타겟은 왼쪽
        {
            xDiff = parentLeft - targetLeft;
            float width;
            if (isTargetUpNeighbor)
            {
                width = xDiff;
            }
            else
            {
                width = xDiff + targetWidth - (parentWidth / 2) + (UMLView.nodeSpacing / 2);
            }

            line.style.left = -width + (parentWidth / 2); // left는 노드 맨 왼쪽이기 때문에 + parentWidth / 2
            line.style.width = new Length(width);

            line.left = line.style.left.value.value;
            line.Width = width;
        }

        line.style.height = new Length(lineSize);
        StyleLength topPos = new StyleLength(new Length(-25));
        line.style.top = topPos;


        line.top = line.style.top.value.value;
        line.height = line.style.height.value.value;
    }

    private void CreateThirdLine()
    {
        UMLEdgeElement line = GetLine();
        edgeElements.Add(line);
        Add(line);

        UMLEdgeElement secondLine = edgeElements[1];

        float targetTop = TargetNode.resolvedStyle.top;
        float parentTop = ParentNode.resolvedStyle.top;

        float targetLeft = TargetNode.style.left.value.value;
        float parentLeft = ParentNode.style.left.value.value;
        bool isTargetRight = targetLeft > parentLeft;

        // 타겟 노드가 부모 노드 바로 위에 있을 때
        if (isTargetUpNeighbor)
        {
            if (isTargetRight)
            {
                line.style.left = edgeElements[1].Width + (TargetNode.border.resolvedStyle.width / 2);
                line.left = edgeElements[1].Width + (TargetNode.border.resolvedStyle.width / 2);
            }
            else
            {
                line.style.left = secondLine.left - lineSize;
                line.left = secondLine.left - lineSize;
            }

            float height = (parentTop - targetTop) -
                           (TargetNode.border.resolvedStyle.height + UMLView.nodeSpacing / 2) - 1.5f;
            line.style.width = new Length(lineSize);
            line.style.height = new Length(height + 1f);
            line.style.top = secondLine.top - (UMLView.nodeSpacing / 2) - 1.5f;
            line.top = secondLine.top - (UMLView.nodeSpacing / 2) - 1.5f;
            line.Width = lineSize;
            line.height = height + 1;
        }
        else
        {
            float height;
            // 부모 노드가 타겟 노드보다 위에 있을 때
            if (targetTop > parentTop)
            {
                height = (targetTop - parentTop) + TargetNode.resolvedStyle.height + UMLView.nodeSpacing;
                line.style.top = secondLine.top;
                line.top = secondLine.top;
            }
            // 부모 노드가 타겟 노드와 같은 Y 축에 있을 때
            else if (targetTop == parentTop)
            {
                height = (parentTop - targetTop) + TargetNode.resolvedStyle.height + UMLView.nodeSpacing;
                line.style.top = -(parentTop - targetTop + edgeElements[0].height + 1.5f);
                line.top = -(parentTop - targetTop + edgeElements[0].height + 1.5f);
            }
            // 부모 노드가 타겟 노드보다 아래에 있을 때
            else
            {
                height = (parentTop - targetTop - TargetNode.border.resolvedStyle.height - UMLView.nodeSpacing -
                          lineSize - 1.5f);
                line.style.top = -(parentTop - targetTop - TargetNode.border.resolvedStyle.height -
                    edgeElements[0].height + 1.5f);
                line.top = -(parentTop - targetTop - TargetNode.border.resolvedStyle.height - edgeElements[0].height +
                             1.5f);
            }

            if (isTargetRight)
            {
                // targetNode.resolvedStyle.width가 targetNode의 가운데로 위치됨
                line.style.left = targetLeft - parentLeft + TargetNode.resolvedStyle.width + (UMLView.nodeSpacing / 2) -
                                  lineSize;
                line.left = targetLeft - parentLeft + TargetNode.resolvedStyle.width + (UMLView.nodeSpacing / 2) -
                            lineSize;
            }
            else
            {
                line.style.left = secondLine.left - lineSize;
                line.left = secondLine.left - lineSize;
            }

            line.style.width = new Length(lineSize);
            line.style.height = new Length(height);
            line.Width = lineSize;
            line.height = height;
        }
    }

    private void CreateFirthLine()
    {
        UMLEdgeElement line = GetLine();
        edgeElements.Add(line);
        Add(line);

        UMLEdgeElement thirdLine = edgeElements[2];

        float targetX = TargetNode.worldTransform.GetPosition().x;
        float lineX = thirdLine.worldTransform.GetPosition().x + thirdLine.left;
        bool isTargetRight = targetX > lineX;
        line.isTargetRight = isTargetRight;

        if (isTargetRight)
        {
            line.style.left = thirdLine.left;
            line.left = thirdLine.left;
            line.style.width = (UMLView.nodeSpacing / 2) + (TargetNode.resolvedStyle.width / 2) + 1.5f;
            line.Width = (UMLView.nodeSpacing / 2) + (TargetNode.resolvedStyle.width / 2);
        }
        else
        {
            line.style.left = thirdLine.left - ((UMLView.nodeSpacing / 2) + (TargetNode.resolvedStyle.width / 2));
            line.left = thirdLine.left - ((UMLView.nodeSpacing / 2) + (TargetNode.resolvedStyle.width / 2));
            line.style.width = (UMLView.nodeSpacing / 2) + (TargetNode.resolvedStyle.width / 2) + lineSize;
            line.Width = (UMLView.nodeSpacing / 2) + (TargetNode.resolvedStyle.width / 2) + lineSize;
        }

        float targetTop = TargetNode.resolvedStyle.top;
        float parentTop = ParentNode.resolvedStyle.top;
        bool isTargetUp = targetTop < parentTop;

        if (isTargetUp)
        {
            line.style.top = thirdLine.top - 1;
            line.top = thirdLine.top - 1;
        }
        else
        {
            line.style.top = thirdLine.top + thirdLine.height - 1;
            line.top = thirdLine.top + thirdLine.height - 1;
        }


        line.style.height = lineSize;
        line.height = lineSize;
    }

    private void CreateFifthLine()
    {
        UMLEdgeElement line = GetLine();
        edgeElements.Add(line);
        Add(line);

        UMLEdgeElement firthLine = edgeElements[3];

        if (firthLine.isTargetRight)
        {
            line.style.left = firthLine.left + firthLine.Width - lineSize + 2.5f;
            line.left = firthLine.left + firthLine.Width - lineSize + 2.5f;
        }
        else
        {
            line.style.left = firthLine.left;
            line.left = firthLine.left;
        }

        line.style.top = firthLine.top - (UMLView.nodeSpacing / 2) - 1.5f;

        line.style.width = lineSize;
        line.style.height = (UMLView.nodeSpacing / 2) + lineSize + 1.5f;

        line.top = firthLine.top - (UMLView.nodeSpacing / 2) - 1.5f;
        line.Width = lineSize;
        line.height = (UMLView.nodeSpacing / 2) + lineSize + 1.5f;
    }

    private UMLEdgeElement GetLine()
    {
        VisualTreeAsset edgeTreeAsset = EditorGUIUtility.Load(uiFile) as VisualTreeAsset;
        UMLEdgeElement line = edgeTreeAsset.CloneTree().Q<UMLEdgeElement>("line");

        return line;
    }

    private StyleTransformOrigin GetPivot(float x, float y)
    {
        TransformOrigin transformOrigin = new TransformOrigin(Length.Percent(x), Length.Percent(y), 0);
        StyleTransformOrigin styleTransformOrigin = new StyleTransformOrigin(transformOrigin);

        return styleTransformOrigin;
    }
}