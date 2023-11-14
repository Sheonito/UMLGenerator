using System.Collections;
using System.Collections.Generic;
using System.IO;
using UMLAutoGenerator;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewOptionToggle : Toggle
{
    public new class UxmlFactory : UxmlFactory<ViewOptionToggle, ViewOptionToggle.UxmlTraits>
    {
    }
    
    private VisualElement root;
    
    public ViewOptionToggle()
    {
        root = UMLGenerator.root;
        this.RegisterValueChangedCallback(OnChangedOption);
    }

    private void OnChangedOption(ChangeEvent<bool> evt)
    {
        bool previousValueValue = evt.previousValue;
        bool newValue = evt.newValue;
        UMLView umlView = root.Q<UMLView>();
        
        if (newValue == true)
        {
            umlView.ShowAllNodes();
        }
        else
        {
            umlView.ShowSelectedNodes();
        }
    }
}
