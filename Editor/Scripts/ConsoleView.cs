using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.UIElements;

public class ConsoleView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<ConsoleView, ConsoleView.UxmlTraits>
    {
    }

    private VisualElement root;
    private bool isOnTextArea;
    public ConsoleView()
    {
        EditorCoroutineUtility.StartCoroutine(Init(), this);
    }

    private IEnumerator Init()
    {
        yield return new WaitForEndOfFrame();
        
        root = UMLGenerator.root;
        
        Button consoleButton = root.Q<Button>(UMLGeneratorView.k_buttonConsole);
        consoleButton.RegisterCallback<ClickEvent>(OnClickConsoleButton);
    }

    private void OnClickConsoleButton(ClickEvent evt)
    {
        if (!isOnTextArea)
        {
            ShowConsoleView();
        }
        else
        {
            HideConsoleView();
        }
    }

    public void ShowConsoleView()
    {
        isOnTextArea = true;
            
        Length heightLength = new Length(30, LengthUnit.Percent); 
        this.style.height = new StyleLength(heightLength);
        VisualElement textArea = root.Q(UMLGeneratorView.k_consoleTextArea);
        textArea.style.display = DisplayStyle.Flex;
    }

    public void HideConsoleView()
    {
        isOnTextArea = false;
            
        Length heightLength = new Length(20, LengthUnit.Pixel); 
        this.style.height = new StyleLength(heightLength);
        VisualElement textArea = root.Q(UMLGeneratorView.k_consoleTextArea);
        textArea.style.display = DisplayStyle.None;
    }
}
