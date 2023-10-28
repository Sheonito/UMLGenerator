using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class UMLGenerator : EditorWindow
{
    public static VisualElement root;
    private UMLView umlView;
    
    [MenuItem("Tools/UMLGenerator")]
    public static void ShowExample()
    {
        UMLGenerator window = GetWindow<UMLGenerator>();
        window.titleContent = new GUIContent("UMLGenerator");
        window.minSize = window.maxSize = new Vector2(1200 * 0.9f,600 * 0.9f);
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        root = rootVisualElement;
        
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.lucecita.sdk/Editor/Uxml/UMLGenerator.uxml");
        visualTree.CloneTree(root);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.lucecita.sdk/Editor/USS/UMLGenerator.uss");
        root.styleSheets.Add(styleSheet);

        umlView = root.Q<UMLView>();

        ViewNavigation viewNavigation = new ViewNavigation();
        var children = root.Children();
        viewNavigation.InitElements(children);
        viewNavigation.Push(UMLGeneratorView.k_menuPage);
    }

}