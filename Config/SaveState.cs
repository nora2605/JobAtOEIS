namespace JobAtOEIS.Config;

internal struct SaveState
{
    public CharacterConfig Character;
    public int[] HighScores;

    public SaveState()
    {
        Character = new();
        HighScores = [];
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
            HighScores = [..lines.Skip(1)
                .Select(line => int.TryParse(line, out int score) ? score : 0)]
        };
    }

    public void Save()
    {
        File.WriteAllText(
            State.A("Assets/save.conf"),
            $"{Character.Serialize()}\n{string.Join("\n", HighScores.Select(x => x.ToString()))}"
        );
    }
}
