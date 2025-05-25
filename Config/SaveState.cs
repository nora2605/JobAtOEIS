namespace JobAtOEIS.Config;

internal struct SaveState
{
    public CharacterConfig Character;
    public int HighScore;

    public SaveState()
    {
        Character = new();
        HighScore = 0;
    }

    public static SaveState Load()
    {
        if (!File.Exists(State.A("Assets/save.conf")))
        {
            var current = new SaveState();
            current.Save();
            return current;
        }
        string[] lines = File.ReadAllLines(State.A("Assets/save.conf"));

        CharacterConfig? c = CharacterConfig.Load(lines[0]);
        if (c == null)
        {
            var current = new SaveState();
            current.Save();
            return current;
        }

        return new SaveState()
        {
            Character = c!,
            HighScore = int.TryParse(lines[1], out int score) ? score : 0,
        };
    }

    public void Save()
    {
        File.WriteAllText(
            State.A("Assets/save.conf"),
            $"{Character.Serialize()}\n{string.Join("\n", HighScore.ToString())}"
        );
    }
}
