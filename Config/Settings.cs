using Raylib_cs;

namespace JobAtOEIS.Config;

internal struct Settings
{
    public string Language
    {
        readonly get => language;
        set
        {
            language = value;
            Raylib.SetWindowTitle(State.translator.Translate("Job at the OEIS", value));
        }
    }
    private string language;


    public float Volume
    {
        readonly get => volume;
        set
        {
            volume = value;
            Raylib.SetMasterVolume(value);
        }
    }
    private float volume;

    /// <summary>
    /// Default values
    /// </summary>
    public Settings()
    {
        language = "en";
        volume = 1f;
    }

    public readonly void Save()
    {
        File.WriteAllText(State.A("Assets/settings.conf"), $"{Language}\n{Volume}");
    }

    public static Settings Load()
    {
        if (!File.Exists(State.A("Assets/settings.conf")))
        {
            var current = new Settings();
            current.Save();
            return current;
        }

        var lines = File.ReadAllLines(State.A("Assets/settings.conf"));
        return new Settings()
        {
            language = lines[0],
            volume = lines.Length >= 2 && float.TryParse(lines[1], out float volume) ? volume : 1f
        };
    }
}
