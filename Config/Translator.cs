namespace JobAtOEIS.Config;

internal class Translator
{
    private readonly Dictionary<string, Dictionary<string, string>> translations;

    public string[] AvailableLanguages => [.. translations.Keys];
    public Translator()
    {
        translations = Directory.GetFiles(State.A("Assets/"), "*.dict").Select(file => KeyValuePair.Create(file[^7..^5], File.ReadAllText(file)
            .Replace("\r", "")
            .Split('\n')
            .Select(line => (KeyValuePair<string, string>?)(string.IsNullOrEmpty(line) ? null : KeyValuePair.Create(
                line.Split("---")[0].Trim().ToLower(),
                string.Join("\n", line.Split("---")[1].Trim().Split("\\n"))
            )))
            .Where(kv => kv != null)
            .Select(kv => kv!.Value)
            .ToDictionary()
        )).ToDictionary();
    }

    public string Translate(string text, string language)
    {
        return translations.TryGetValue(language, out var a) ? a.TryGetValue(text.ToLower(), out string? value) ? value : text : text;
    }
}
