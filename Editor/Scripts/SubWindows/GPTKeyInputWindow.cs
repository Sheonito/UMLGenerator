using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UMLAutoGenerator;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UMLAutoGenerator
{
    public class GPTKeyInputWindow : EditorWindow
    {
        private static Action applyCallback;
        public static void ShowWindow(Action callback)
        {
 
            applyCallback = callback;
            GPTKeyInputWindow window = GetWindow<GPTKeyInputWindow>();
            window.titleContent = new GUIContent("GPTKeyInputWindow");
            window.minSize = window.maxSize = new Vector2(400,100);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.lucecita.sdk/Editor/Uxml/GPTKeyInputWindow.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            Button applyButton = root.Q<Button>(UMLGeneratorView.k_buttonGptApiApply);
            applyButton.RegisterCallback<ClickEvent>(Apply);
        }

        private async void Apply(ClickEvent evt)
        {
            TextField textField = rootVisualElement.Q<TextField>(UMLGeneratorView.k_textfieldGptApiKey);
            GPTStreamHandler.apiKey = textField.value;
            GPTStreamHandler.RequestData requestData = new GPTStreamHandler.RequestData();
            GPTStreamHandler.Message requestMessage = new GPTStreamHandler.Message() { role = "user", content = "테스트" };
            requestData.messages = new List<GPTStreamHandler.Message>() { requestMessage };
            CancellationTokenSource cts = new CancellationTokenSource();

            Label log = rootVisualElement.Q<Label>(UMLGeneratorView.k_labelGptApiKeyLog);            
            HttpStatusCode result = await GPTStreamHandler.CheckAPIStatus(requestData, cts.Token);
            if (result == HttpStatusCode.OK || result == HttpStatusCode.Created || result == HttpStatusCode.Accepted)
            {
                log.text = "Authorized successfully";
                PlayerPrefs.SetString("UMLGeneratorAPIKey",GPTStreamHandler.apiKey);
                applyCallback?.Invoke();
                await Task.Delay(500, cts.Token);
                Close();
            }
            else if (result == HttpStatusCode.Unauthorized)
            {
                log.text = result + ": Check if you input correct api key";
            }
            else
            {
                log.text = result.ToString();
            }
        }
    }
}



