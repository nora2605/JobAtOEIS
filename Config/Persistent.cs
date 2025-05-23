namespace JobAtOEIS.Config;

internal static class Persistent
{
    public static string GetPath(string asset)
    {
        string path = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", asset);
        if (!Directory.Exists(Path.GetDirectoryName(path)!))
            throw new DirectoryNotFoundException($"Your operating system doesn't like this path ----> {Path.GetDirectoryName(path)}");
        return path;
    }
}
