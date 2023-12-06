using System;
using System.Reflection;
using System.Text;
using Mono.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer.Internal;

namespace UMLAutoGenerator
{
    public class CodeReader
    {
        public static string GetILCode(Type type)
        {
            StringBuilder result = new StringBuilder();
            MemberInfo[] members = type.GetDeclaredMembers();

            foreach (MemberInfo memberInfo in members)
            {
                if (memberInfo is FieldInfo fieldInfo)
                {
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

            return result.ToString();
        }
    }
}