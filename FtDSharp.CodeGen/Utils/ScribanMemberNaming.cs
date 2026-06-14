using System.Reflection;
using System.Text;

namespace FtDSharp.CodeGen.Utils;

internal static class ScribanMemberNaming
{
    public static string ToSnakeCase(MemberInfo member) => ToSnakeCase(member.Name);

    public static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var builder = new StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    builder.Append('_');
                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
