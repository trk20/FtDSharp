using UnityEngine;

namespace FtDSharp.CodeGen.Utils;

public static class TypeNameHelper
{
    public static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(string)) return "string";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(long)) return "long";
        if (type == typeof(short)) return "short";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(void)) return "void";

        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef == typeof(Nullable<>))
                return GetFriendlyTypeName(type.GetGenericArguments()[0]) + "?";

            var baseName = type.Name.Split('`')[0];
            var args = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
            return $"{baseName}<{args}>";
        }

        if (type.Namespace?.StartsWith(nameof(UnityEngine)) == true)
            return type.Name;

        if (type.IsEnum)
            return type.FullName?.Replace("+", ".") ?? type.Name;

        return type.FullName?.Replace("+", ".") ?? type.Name;
    }

    public static string EscapeXml(string? text)
    {
        if (text == null) return "";
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;")
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }
}
