using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Reflection;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using IList = System.Collections.IList;

public class GenerateButton : Button
{
    public new class UxmlFactory : UxmlFactory<GenerateButton, GenerateButton.UxmlTraits>
    {
    }

    private VisualElement root;
    private UMLView umlView;

    public List<string> filePath = new List<string>();
    private List<string> typeNames = new List<string>();
    private List<Type> scripts = new List<Type>();
    private List<ScriptInfo> scriptInfoList = new List<ScriptInfo>();

    public GenerateButton()
    {
        root = UMLGenerator.root;
        RegisterCallback<ClickEvent>(GenerateUML);
    }

    private void Init()
    {
        scripts.Clear();
        scriptInfoList.Clear();
        TextField consoleTextField = root.Q<TextField>(UMLGeneratorView.k_consoleTextFieldGPT);
        ConsoleView consoleView = root.Q<ConsoleView>(UMLGeneratorView.k_consoleView);
        consoleTextField.value = null;
        consoleView.HideConsoleView();
    }

    private void GenerateUML(ClickEvent evt)
    {
        Init();

        if (umlView == null)
            umlView = root.Q<UMLView>();


        foreach (var path in filePath)
        {
            if (path.Contains(".cs"))
            {
                string[] names = GetTypeNames(path);
                typeNames.AddRange(names);
            }
            else
            {
                string[] names = GetFolderTypeNames(path);
                typeNames.AddRange(names);
            }
        }

        for (int i = 0; i < filePath.Count; i++)
        {
            // 단일 스크립트
            if (filePath[i].Contains(".cs"))
            {
                string[] typeNames = GetTypeNames(filePath[i]);

                // 모든 타입 가져오기
                GetScriptTypes(typeNames);
            }
            // 폴더
            else
            {
                string[] typeNames = GetFolderTypeNames(filePath[i]);

                // 모든 타입 가져오기
                GetScriptTypes(typeNames);
            }
        }

        ViewNavigation.Instance.Push(UMLGeneratorView.k_umlViewPage);
        root.Q<UMLToolbarMenu>(UMLGeneratorView.k_toolbarMenu).text = "UML View";
        root.Q<UMLView>().SetMenu(true);
        ViewOptionDropDown viewOptionDropDown = root.Q<ViewOptionDropDown>(UMLGeneratorView.k_viewOptionDropDown);
        {
            viewOptionDropDown.SetValueWithoutNotify(ViewOption.All.ToString());
        }

        // UML 노드 생성
        EditorCoroutineUtility.StartCoroutine(umlView.PopulateView(scriptInfoList, ViewOption.All), this);
    }

    private string[] GetFolderTypeNames(string dirPath)
    {
        string[] scriptFilesPath = Directory.GetFiles(dirPath, "*.cs", SearchOption.AllDirectories);
        List<string> typeNames = new List<string>();

        for (int i = 0; i < scriptFilesPath.Length; i++)
        {
            string scriptText = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptFilesPath[i]).text;
            bool insideCommentBlock = false;
            string[] lines = scriptText.Split('\n');

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("/*"))
                {
                    insideCommentBlock = true;
                }

                if (insideCommentBlock && trimmedLine.EndsWith("*/"))
                {
                    insideCommentBlock = false;
                }

                if (insideCommentBlock || trimmedLine.StartsWith("//"))
                {
                    continue;
                }

                if (trimmedLine.Contains("class ") ||
                    trimmedLine.Contains("interface ") ||
                    trimmedLine.Contains("enum ") ||
                    trimmedLine.Contains("struct "))
                {
                    string[] words = trimmedLine.Split(' ');

                    int nameIndex = Array.FindIndex(words,
                        w => w == "class" || w == "interface" || w == "enum" || w == "struct");
                    if (nameIndex + 1 < words.Length)
                    {
                        string typeName = words[nameIndex + 1].TrimEnd(':').Trim();
                        typeName = typeName.Split(new[] { "//", "/*" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        typeName = typeName.Replace("<T>", "`1");
                        typeNames.Add(typeName);

                        // 디버깅
                        // Debug.Log($"Type: {words[nameIndex]}, Name: {typeName}");
                    }
                }
            }
        }

        return typeNames.ToArray();
    }

    private string[] GetTypeNames(string path)
    {
        List<string> typeNames = new List<string>();
        TextAsset scriptTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        string scriptText = scriptTextAsset.text;
        bool insideCommentBlock = false;
        string[] lines = scriptText.Split('\n');

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("/*"))
            {
                insideCommentBlock = true;
            }

            if (insideCommentBlock && trimmedLine.EndsWith("*/"))
            {
                insideCommentBlock = false;
            }

            if (insideCommentBlock || trimmedLine.StartsWith("//"))
            {
                continue;
            }

            if ((trimmedLine.Contains("class ") ||
                 trimmedLine.Contains("interface ") ||
                 trimmedLine.Contains("enum ") ||
                 trimmedLine.Contains("struct ")) && trimmedLine.Contains("\"") == false)
            {
                string[] words = trimmedLine.Split(' ');

                int nameIndex = Array.FindIndex(words,
                    w => w == "class" || w == "interface" || w == "enum" || w == "struct");
                if (nameIndex + 1 < words.Length)
                {
                    string typeName = words[nameIndex + 1].TrimEnd(':').Trim();
                    typeName = typeName.Split(new[] { "//", "/*" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    typeName = typeName.Replace("<T>", "`1");
                    typeNames.Add(typeName);

                    // 디버깅
                    // Debug.Log($"Type: {words[nameIndex]}, Name: {typeName}");
                }
            }
        }

        return typeNames.ToArray();
    }

    private void GetScriptTypes(string[] typeNames)
    {
        foreach (string typeName in typeNames)
        {
            if (!string.IsNullOrEmpty(typeName) && !string.IsNullOrWhiteSpace(typeName))
            {
                Type script = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(ass => ass.GetTypes())
                    .FirstOrDefault(t => t.Name == typeName);

                scripts.Add(script);

                ScriptInfo info = new ScriptInfo();
                bool isMonoScript = typeof(MonoBehaviour).IsAssignableFrom(script);
                info.scriptName = script.Name;
                info.isMonoScript = isMonoScript;
                info.scriptType = script;
                scriptInfoList.Add(info);
                // 디버깅
                // Debug.Log("scriptName: " + script.Name);
            }
        }


        // 함수와 변수 가져오기
        GetScriptsInfo();
    }

    private void GetScriptsInfo()
    {
        int scriptCount = scripts.Count;
        for (int i = 0; i < scriptCount; i++)
        {
            GetScriptInfo(scripts[i]);
        }
    }

    private void GetScriptInfo(Type script)
    {
        MemberInfo[] members = script.GetDeclaredMembers();
        members = members.OrderBy(member => member.DeclaringType.FullName).ToArray();
        ScriptInfo scriptInfo = scriptInfoList.Find(info => info.scriptName == script.Name);

        // 부모 참조 추가
        if (script.BaseType != null && script.BaseType != script)
        {
            string moduleName = script.BaseType.Module.Name;
            if (IsBuiltInDll(moduleName) == false)
            {
                ScriptInfo referringScriptInfo = new ScriptInfo();
                referringScriptInfo.scriptName = script.BaseType.Name;
                referringScriptInfo.isReferenced = true;
                referringScriptInfo.scriptType = script.BaseType;
                scriptInfo.referringScriptInfos.Add(referringScriptInfo);
                referringScriptInfo.referencedScriptInfos.Add(scriptInfo);
            }
        }
        
        // 자식 참조 추가
        var derivedTypes = script.Assembly
            .GetTypes()
            .Where(t => script.IsAssignableFrom(t) && t != script)
            .ToList();

        foreach (var derivedType in derivedTypes)
        {
            string moduleName = derivedType.BaseType.Module.Name;
            if (IsBuiltInDll(moduleName) == false)
            {
                ScriptInfo derivedScriptInfo = new ScriptInfo();
                derivedScriptInfo.scriptName = derivedType.Name;
                derivedScriptInfo.scriptType = derivedType; 
                derivedScriptInfo.isReferenced = true;
                derivedScriptInfo.referringScriptInfos.Add(scriptInfo);
                scriptInfo.referencedScriptInfos.Add(derivedScriptInfo);
                scriptInfoList.Add(derivedScriptInfo);
            }
        }

        foreach (MemberInfo member in members)
        {
            if (member.DeclaringType == typeof(System.Object))
                break;

            StringBuilder typeName = new StringBuilder();
            if (script.Namespace != null)
                typeName.Append(script.Namespace + ".");

            typeName.Append(script.Name + ",");
            typeName.Append(script.Assembly.FullName);

            // 상속받은 멤버
            Type scriptType = Type.GetType(typeName.ToString());
            if (member.DeclaringType != scriptType)
            {
                // 디버깅
                // Debug.Log($"원래 타입:{scriptType} 상속 받은 타입: {member.DeclaringType}" );
            }

            // 한정자 가져오기
            string accessModifier = GetAccessModifier(member);

            string type = member.GetType().Name;
            string memberName = member.Name;

            // 디버깅
            // Debug.Log($"{scripts[i].Name} {accessModifier} {type} {memberName}");


            // 각 멤버들의 참조 관계 가져오기
            GetReferenceInfo(member, scriptInfo);
        }
    }

    private string GetAccessModifier(MemberInfo member)
    {
        if (member is FieldInfo field)
        {
            switch (field.Attributes & FieldAttributes.FieldAccessMask)
            {
                case FieldAttributes.Public:
                    return "public";
                case FieldAttributes.Private:
                    return "private";
                case FieldAttributes.Family:
                    return "protected";
                case FieldAttributes.Assembly:
                    return "internal";
                case FieldAttributes.FamANDAssem:
                    return "private protected";
                case FieldAttributes.FamORAssem:
                    return "protected internal";
                default:
                    return "unknown";
            }
        }
        else if (member is MethodInfo method)
        {
            switch (method.Attributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.Public:
                    return "public";
                case MethodAttributes.Private:
                    return "private";
                case MethodAttributes.Family:
                    return "protected";
                case MethodAttributes.Assembly:
                    return "internal";
                case MethodAttributes.FamANDAssem:
                    return "private protected";
                case MethodAttributes.FamORAssem:
                    return "protected internal";
                default:
                    return "unknown";
            }
        }

        return "unknown";
    }

    private void GetReferenceInfo(MemberInfo memberInfo, ScriptInfo scriptInfo)
    {
        if (memberInfo is FieldInfo fieldInfo)
        {
        }
        else if (memberInfo is MethodInfo methodInfo)
        {
            // 디버깅
            // Debug.Log(methodInfo.DeclaringType.Module.Name);
            bool isBuiltInDll = IsBuiltInDll(methodInfo.DeclaringType.Module.Name);
            if (isBuiltInDll)
                return;

            IList<Instruction> instructions = null;
            try
            {
                instructions = methodInfo.GetInstructions();
            }
            catch
            {
                // Debug.Log("methodInfo.DeclaringType.Module.Name: " + methodInfo.DeclaringType.Module.Name);
                // Debug.Log("methodInfo.DeclaringType: " + methodInfo.DeclaringType);
                // Debug.Log("methodInfo.Name: " + methodInfo.Name);
            }

            if (instructions == null)
                return;

            foreach (var instruction in instructions)
            {
                if (instruction.Operand == null)
                    continue;

                if (instruction.Operand is MemberInfo info)
                {
                    Type declaringType = info.DeclaringType;
                    if (declaringType == null || declaringType.AssemblyQualifiedName.Contains("["))
                        continue;

                    // 디버깅
                    // Debug.Log(declaringType.Module.Name);
                    // Debug.Log($"Instruction: {instruction.OpCode} Operand Type: {declaringType}");

                    bool isContained =
                        scriptInfo.referringScriptInfos.Exists(x => x.scriptName == declaringType.Name);

                    bool isbuiltInDll = IsBuiltInDll(declaringType.Module.Name);
                    bool isLambdaMethod = IsLambdaMethod(declaringType.Name);
                    if (!isContained && !isbuiltInDll && !isLambdaMethod)
                    {
                        bool isSelectedScript = typeNames.Exists(name => name == declaringType.Name);
                        ScriptInfo childInfo = new ScriptInfo();
                        childInfo.scriptName = declaringType.Name;
                        childInfo.scriptType = declaringType;
                        childInfo.isReferenced = !isSelectedScript;

                        scriptInfo.referringScriptInfos.Add(childInfo);
                        childInfo.referringScriptInfos.Add(scriptInfo);

                        // 디버깅
                        // Debug.Log(declaringType.Name + "/" + scriptInfo.scriptName);

                        // 참조 Script면서, 유니티 폴더에 있는 Script
                        if (scriptInfoList.Exists(x => x.scriptName == childInfo.scriptName) == false)
                        {
                            scriptInfoList.Add(childInfo);
                            GetScriptInfo(declaringType);
                        }
                    }
                }
            }
        }
    }

    private bool IsBuiltInDll(string moduleName)
    {
        string[] builtInDll = new[] { "mscorlib", "UnityEngine", "UnityEditor", "System.Core", "Unity.", "System." };
        bool isBuiltInDll = builtInDll.Any(dllName => moduleName.Contains(dllName));

        return isBuiltInDll;
    }

    private bool IsLambdaMethod(string declaringName)
    {
        if (declaringName.Contains("<>c"))
            return true;

        else
            return false;
    }
}