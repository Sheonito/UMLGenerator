using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


public enum UMLEdgeElementType
{
    Line,
    Direction
}

public class UMLEdgeElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<UMLEdgeElement, VisualElement.UxmlTraits>
    {
    }

    public float left;
    public float top;
    public float Width;
    public float height;
    public bool isTargetRight;
    public UMLEdgeElementType elementType;

    public void Select()
    {
        if (elementType == UMLEdgeElementType.Direction)
        {
            style.unityBackgroundImageTintColor = new Color(0.3840512f, 0.770284f, 0.8773585f);
        }
        else
        {
            style.backgroundColor = new Color(0.3840512f, 0.770284f, 0.8773585f);
        }
    }

    public void UnSelect()
    {
        if (elementType == UMLEdgeElementType.Direction)
        {
            style.unityBackgroundImageTintColor = new Color(0.6039216f, 0.6039216f, 0.6039216f);
        }
        else
        {
            style.backgroundColor = new Color(0.6039216f, 0.6039216f, 0.6039216f);
        }
    }
}