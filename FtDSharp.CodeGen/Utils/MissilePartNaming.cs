namespace FtDSharp.CodeGen.Utils;

public static class MissilePartNaming
{
    public static string FacadeClassName(string interfaceName) =>
        interfaceName.StartsWith('I') ? interfaceName[1..] : interfaceName;
}
