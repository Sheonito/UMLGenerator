using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

public class UMLToolbarMenu : ToolbarMenu
{
    public new class UxmlFactory : UxmlFactory<UMLToolbarMenu, ToolbarMenu.UxmlTraits> { }
    public VisualElement root;

    private VisualElement menuPage;
    private VisualElement umlViewPage;
    
    public UMLToolbarMenu()
    {
        root = UMLGenerator.root;
        
        menu.AppendAction("Menu",OnClickMenu);
        menu.AppendAction("UML View",OnClickUMLView);
    }

    private void OnClickUMLView(DropdownMenuAction action)
    {
        text = "UML View";

        if (menuPage == null || umlViewPage == null)
        {
            menuPage = root.Q<VisualElement>("menu-page");
            umlViewPage = root.Q<VisualElement>("umlView-page");
        }

        ViewNavigation.Instance.Push(UMLGeneratorView.k_umlViewPage);
    }

    private void OnClickMenu(DropdownMenuAction action)
    {
        if (menuPage == null || umlViewPage == null)
        {
            menuPage = root.Q<VisualElement>("menu-page");
            umlViewPage = root.Q<VisualElement>("umlView-page");
        }

        text = "Menu";
        ViewNavigation.Instance.Push(UMLGeneratorView.k_menuPage);
        
        // ObjectField 추가 버튼
        Button addButton = root.Q<Button>("Button-add");
        addButton.RegisterCallback<ClickEvent>(AddObjectField);
        
        ViewOptionToggle allDependenciesToggle = root.Q<ViewOptionToggle>(UMLGeneratorView.k_allDependenciesToggle);
        allDependenciesToggle.style.display = DisplayStyle.None;
        
        SaveUMLButton saveUmlButton = root.Q<SaveUMLButton>();
        saveUmlButton.style.display = DisplayStyle.None;
    }

    private void AddObjectField(ClickEvent evt)
    {
        VisualElement currentField = root.Q<VisualElement>("FolderObjectField");
        int index = menuPage.IndexOf(currentField) + 1;
        
        ObjectField objectField = new ObjectField();
        objectField.style.width = Length.Percent(100);
        objectField.objectType = typeof(UnityEditor.DefaultAsset);    
        
        menuPage.Insert(index,objectField);
    }
}
