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
                // <Digits|Largest>:<Name>:<Description>:<OEISID>:<Elements (Space Seperated)|Digits>
                string[] parts = line.Split(':');
                if (parts.Length < 5) continue;
                string type = parts[0].Trim();
                string name = parts[1].Trim();
                string description = parts[2].Trim();
                string oeisID = parts[3].Trim();
                string[] elements = parts[4].Trim().Split(' ');
                if (type == "Digits")
                {
                    char[] digits = elements[0].ToCharArray();
                    Sequences.Add(new DigitSequence(name, description, oeisID, digits));
                }
                else Sequences.Add(new IntegerSequence(name, description, oeisID, elements));
            }
        }
    }

    internal interface Sequence
    {
        const int DEFAULT_MAX_DIGITS = 30;

        string Name { get; }
        string Description { get; }
        string OEISID { get; }
        int MinDigits { get; }
        int MaxDigits { get; }

        bool IsValid(string n);
        string GenerateRandomValid();
    }

    /// <summary>
    /// Derives a Sequence from a list of elements.
    /// </summary>
    internal class IntegerSequence(string name, string description, string OEISID, string[] elements) : Sequence
    {
        public string[] Elements { get; } = elements;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string OEISID { get; } = OEISID;
        public int MinDigits { get => Elements.Min(x => x.Length); }
        public int MaxDigits { get => Elements.Max(x => x.Length); }
        public bool IsValid(string n) => Elements.Contains(n);
        public string GenerateRandomValid() => Elements[Random.Shared.Next(Elements.Length)];
    }

    /// <summary>
    /// Derives a Sequence from a list of digits.
    /// </summary>
    internal class DigitSequence(string name, string description, string OEISID, char[] digits) : Sequence
    {
        public char[] Digits { get; } = digits;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string OEISID { get; } = OEISID;
        public int MinDigits { get => 1; }
        public int MaxDigits { get => Digits.Length; }

        public bool IsValid(string n) => n.Length >= MinDigits && n.Length <= MaxDigits && n.Zip(Digits).All(x => x.First == x.Second);
        public string GenerateRandomValid() => Digits[0..Random.Shared.Next(MinDigits, MaxDigits)].Aggregate("", (a, b) => a + b);
    }
}
