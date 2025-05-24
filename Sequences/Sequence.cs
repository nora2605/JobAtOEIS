using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace JobAtOEIS.Sequences
{
    internal class SequenceManager
    {
        public static List<Sequence> Sequences { get; } = [];
        static SequenceManager() { }

        public static void Load()
        {
            string[] s = File.ReadAllLines(State.A("Assets/sequences.oeis"));
            foreach (var line in s)
            {
                // Line format:
                // <Digits|Largest>:<Name>:<Description>:<OEISID>:<Expected>:<Elements (Space Seperated)|Digits>
                string[] parts = line.Split(':');
                if (parts.Length < 6) continue;
                string type = parts[0].Trim();
                string name = parts[1].Trim();
                string description = parts[2].Trim();
                string oeisID = parts[3].Trim();
                int expected = int.Parse(parts[4].Trim());
                string[] elements = parts[5].Trim().Split(' ');
                if (type == "Digits")
                {
                    char[] digits = elements[0].ToCharArray();
                    Sequences.Add(new DigitSequence(name, description, oeisID, expected, digits));
                }
                else Sequences.Add(new IntegerSequence(name, description, oeisID, expected, elements));
            }
        }
    }

    internal interface Sequence
    {
        const int DEFAULT_MAX_DIGITS = 30;

        string Name { get; }
        string Description { get; }
        string OEISID { get; }
        int Expected { get; }
        int MinDigits { get; }
        int MaxDigits { get; }

        bool IsValid(string n);
        string GenerateRandomValid(int d);
    }

    /// <summary>
    /// Derives a Sequence from a list of elements.
    /// </summary>
    internal class IntegerSequence(string name, string description, string OEISID, int expected, string[] elements) : Sequence
    {
        public string[] Elements { get; } = elements;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string OEISID { get; } = OEISID;
        public int Expected { get; } = expected;
        public int MinDigits { get => Elements.Min(x => x.Length); }
        public int MaxDigits { get => Elements.Max(x => x.Length); }
        public bool IsValid(string n) => Elements.Contains(n);
        public string GenerateRandomValid(int d) {
            int r = Random.Shared.Next(Elements.Length);
            while (Elements[r].Length > d && d <= MinDigits)
                // requires sequence to be monotonically increasing, kind of
                r = Random.Shared.Next(r);
            return Elements[r];
        }
    }

    /// <summary>
    /// Derives a Sequence from a list of digits.
    /// </summary>
    internal class DigitSequence(string name, string description, string OEISID, int expected, char[] digits) : Sequence
    {
        public char[] Digits { get; } = digits;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string OEISID { get; } = OEISID;
        public int Expected { get; } = expected;
        public int MinDigits { get => 1; }
        public int MaxDigits { get => Digits.Length; }

        public bool IsValid(string n) => n.Length >= MinDigits && n.Length <= MaxDigits && n.Zip(Digits).All(x => x.First == x.Second);
        public string GenerateRandomValid(int d) => Digits[0..Math.Min(d, Digits.Length)].Aggregate("", (a, b) => a + b);
    }
}
