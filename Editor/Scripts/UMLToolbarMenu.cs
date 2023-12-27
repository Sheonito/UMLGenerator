using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UMLAutoGenerator;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Timeline.Actions;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

public class UMLToolbarMenu : ToolbarMenu
{
    public new class UxmlFactory : UxmlFactory<UMLToolbarMenu, ToolbarMenu.UxmlTraits>
    {
    }

    public VisualElement root;

    private VisualElement menuPage;
    private VisualElement umlViewPage;

    public UMLToolbarMenu()
    {
        root = UMLGenerator.root;
        menu.AppendAction("Menu", OnClickMenu);
        menu.AppendAction("UML View", OnClickUMLView);
        
        EditorCoroutineUtility.StartCoroutine(Init(), this);
    }

    private IEnumerator Init()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (menuPage == null || umlViewPage == null)
        {
            menuPage = root.Q<VisualElement>("menu-page");
            umlViewPage = root.Q<VisualElement>("umlView-page");
        }
        
        // ObjectField 추가 버튼
        Button addButton = root.Q<Button>("Button-add");
        addButton.RegisterCallback<ClickEvent>(AddObjectField);
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
        
        root.Q<UMLView>().SetMenu(true);
    }

    private void OnClickMenu(DropdownMenuAction action)
    {
        text = "Menu";
        ViewNavigation.Instance.Push(UMLGeneratorView.k_menuPage);
        
        root.Q<UMLView>().SetMenu(false);
    }

    private void AddObjectField(ClickEvent evt)
    {
        VisualElement currentField = root.Q<VisualElement>("FolderObjectField");
        int index = menuPage.IndexOf(currentField) + 1;

        FolderObjectField objectField = new FolderObjectField();
        objectField.style.width = Length.Percent(100);
        objectField.objectType = typeof(Object);

        menuPage.Insert(index, objectField);
    }
}