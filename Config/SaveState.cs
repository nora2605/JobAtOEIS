using System;
using System.Collections.Generic;
using System.Text;

namespace JobAtOEIS.Config
{
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

            (CharacterConfig? c, int l) = CharacterConfig.Load(lines);
            if (c == null)
            {
                var current = new SaveState();
                current.Save();
                return current;
            }

            return new SaveState()
            {
                Character = c!,
                HighScores = [..lines.Skip(l)
                    .Select(line => int.TryParse(line, out int score) ? score : 0)]
            };
        }

        public void Save()
        {
            File.WriteAllLines(State.A("Assets/save.conf"), Character.Save().Concat(HighScores.Select(x => x.ToString())));
        }
    }
}
