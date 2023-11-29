using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class FolderObjectField : ObjectField
{
    private VisualElement root;
    
    public new class UxmlFactory : UxmlFactory<FolderObjectField, ObjectField.UxmlTraits>
    {
    }


    public FolderObjectField()
    {
        root = UMLGenerator.root;
        RegisterCallback<ChangeEvent<UnityEngine.Object>>(GetFolderPath);
    }

    private void GetFolderPath(ChangeEvent<Object> evt)
    {
        UnityEngine.Object selectedObject = evt.newValue as DefaultAsset;
        selectedObject = selectedObject == null ? evt.newValue as MonoScript : selectedObject;

        if (selectedObject != null)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(selectedObject, out string guid, out long _);

            if (guid != null)
            {
                // GUID -> Assets/ 로 시작하는 상대 경로 찾기
                string path = AssetDatabase.GUIDToAssetPath(guid);
                path = path.Substring(path.IndexOf('/') + 1);
                path = "Assets/" + path;

                GenerateButton generateButton = root.Q<GenerateButton>(UMLGeneratorView.k_buttonExecute);
                generateButton.filePath.Add(path);
            }
        }
        else
        {
            Debug.LogError("Select only script or folder");
        }
    }
}