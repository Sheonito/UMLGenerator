using System.Collections;
using System.Collections.Generic;
using System.IO;
using UMLAutoGenerator;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveUMLButton : Button
{
    public new class UxmlFactory : UxmlFactory<SaveUMLButton, SaveUMLButton.UxmlTraits>
    {
    }

    private VisualElement root;

    public SaveUMLButton()
    {
        root = UMLGenerator.root;
        RegisterCallback<ClickEvent>(ScreenShot);
    }

    private void ScreenShot(ClickEvent evt)
    {
        EditorWindow Window = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.EditorWindow"));
        Window.Focus();

        int inspectorWidth = (int)Window.position.width;
        int inspectorHeight = (int)Window.position.height;

        Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(Window.position.position,
            inspectorWidth, inspectorHeight);

        Texture2D inspectorTexture = new Texture2D(inspectorWidth, inspectorHeight, TextureFormat.RGB24, false);
        inspectorTexture.SetPixels(pixels);
        byte[] bytes = inspectorTexture.EncodeToPNG();

        string path = EditorUtility.SaveFilePanel("Save UML", "Assets", "UML.png", "png");
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log("UML saved to: " + path);
        }
    }
}