using System;
using System.Reflection;
using System.Text;
using Mono.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UMLAutoGenerator
{
    public class CodeReader
    {
        public static string GetTypeCode(Type type)
        {
            StringBuilder result = new StringBuilder();

            // 유니티 Object일 경우
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                string[] allPaths = AssetDatabase.GetAllAssetPaths();

                foreach (string path in allPaths)
                {
                    if (path.Contains(type.Name))
                    {
                        MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                        Type monoType = monoScript.GetClass();
                        if (monoType != null && monoType.FullName == type.FullName)
                        {
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(monoScript, out string guid, out long _);

                            if (guid != null)
                            {
                                string resultPath = AssetDatabase.GUIDToAssetPath(guid);
                                resultPath = resultPath.Substring(resultPath.IndexOf('/') + 1);
                                resultPath = "Assets/" + resultPath;
                                TextAsset scriptTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(resultPath);
                                result.Append(scriptTextAsset.text);

                                return result.ToString();
                            }
                        }
                    }
                }
            }
            // 그 외
            else
            {
                MemberInfo[] members = type.GetDeclaredMembers();

                foreach (MemberInfo memberInfo in members)
                {
                    if (memberInfo is FieldInfo fieldInfo)
                    {
                        result.Append(fieldInfo.Attributes);
                        result.Append(fieldInfo.GetAccessorType());
                        result.Append(fieldInfo.FieldType);
                        result.Append(fieldInfo.Name);
                    }
                }

                foreach (MemberInfo memberInfo in members)
                {
                    if (memberInfo is MethodInfo methodInfo)
                    {
                        var instructions = methodInfo.GetInstructions();
                        foreach (var instruction in instructions)
                        {
                            result.Append(instruction);
                        }
                    }
                }
            }

            return result.ToString();
        }
    }
}