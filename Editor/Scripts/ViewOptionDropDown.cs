using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UMLAutoGenerator;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum ViewOption
{
    All = 0,
    Selected = 1,
    Referring = 2,
    Referenced = 3,
    Mono = 4,
}

public class ViewOptionDropDown : DropdownField
{
    public new class UxmlFactory : UxmlFactory<ViewOptionDropDown, ViewOptionDropDown.UxmlTraits>
    {
    }

    private VisualElement root;

    public ViewOptionDropDown()
    {
        root = UMLGenerator.root;
        this.RegisterValueChangedCallback(OnChangedOption);
    }

    private void OnChangedOption(ChangeEvent<string> evt)
    {
        string previousValueValue = evt.previousValue;
        string newValue = evt.newValue;
        UMLView umlView = root.Q<UMLView>();

        ViewOption option = (ViewOption)Enum.Parse(typeof(ViewOption), newValue);
        umlView.PopulateView(option);
    }
}