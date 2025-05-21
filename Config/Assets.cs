namespace JobAtOEIS.Config;

public static class Assets
{
  public static string GetPath(string asset)
  {
#if WINDOWS
    return asset;
#else
    return Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "./", asset);
#endif
  }
}