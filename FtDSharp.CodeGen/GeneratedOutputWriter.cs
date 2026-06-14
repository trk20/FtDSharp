namespace FtDSharp.CodeGen;

public static class GeneratedOutputWriter
{
    public static void CleanGeneratedFiles(string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (var file in Directory.GetFiles(directory, "*.g.cs"))
            File.Delete(file);
    }

    public static void Write(string directory, string fileName, string content)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, fileName), content);
    }
}
