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
                else if (type == "Largest")
                    Sequences.Add(new IntegerSequence(name, description, oeisID, expected, elements));
                else if (type == "Prime")
                    Sequences.Add(new PrimeSequence(name, description, oeisID, expected));
                else if (type == "Square")
                    Sequences.Add(new SquareSequence(name, description, oeisID, expected));
                else if (type == "Triangle")
                    Sequences.Add(new TriangleSequence(name, description, oeisID, expected));
            }
        }
    }

    internal interface Sequence
    {
        const int DEFAULT_MAX_DIGITS = 30;

        /// <summary>
        /// Check if n = f(x) for integer x, where f is monotonically increasing and valid from 0 to n.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool SearchValid(BigInteger n, Func<BigInteger, BigInteger> f)
        {
            BigInteger l = 0, r = n;
            while (l <= r)
            {
                BigInteger m = (l + r) / 2;
                BigInteger x = f(m);
                if (x == n) return true;
                if (x < n) l = m + 1;
                else r = m - 1;
            }
            return false;
        }

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
        public string GenerateRandomValid(int d)
        {
            int r = Random.Shared.Next(Elements.Length);
            while (Elements[r].Length > d && d >= MinDigits)
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
        public string GenerateRandomValid(int d) => Digits[0..(d == 0 ? Random.Shared.Next(Digits.Length) : Math.Min(d, Digits.Length))].Aggregate("", (a, b) => a + b);
    }

    internal class PrimeSequence(string name, string description, string oeisid, int expected) : Sequence
    {
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string OEISID { get; } = oeisid;
        public int Expected { get; } = expected;
        public int MinDigits { get => 1; }
        public int MaxDigits { get => 15; }
        public bool IsValid(string n) => BigInteger.TryParse(n, out BigInteger num) && IsPrime(num);

        private bool IsPrime(BigInteger number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;
            for (BigInteger i = 3; i * i <= number; i += 2)
                if (number % i == 0) return false;
            return true;
        }
        public string GenerateRandomValid(int d)
        {
            BigInteger b = BigInteger.One;
            // honestly no idea if this is sufficiently safe against stalling
            do
            {
                if (b.ToString().Length <= d) b *= Random.Shared.Next(2, 10);
                b--;
                if (b <= 0) b = BigInteger.One;
                if (b.ToString().Length > d) b /= 100;
            } while (!IsPrime(b));
            return b.ToString();
        }
    }
    internal class SquareSequence(string name, string description, string oeisid, int expected) : Sequence
    {
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string OEISID { get; } = oeisid;
        public int Expected { get; } = expected;
        public int MinDigits { get => 1; }
        public int MaxDigits { get => 15; }
        public bool IsValid(string n) => BigInteger.TryParse(n, out BigInteger num) && IsSquare(num);

        private bool IsSquare(BigInteger number)
        {
            if (number < 0) return false;
            if (number == 0 || number == 1) return true;
            BigInteger l = 0, r = number;
            while (l <= r)
            {
                BigInteger m = (l + r) / 2;
                BigInteger s = m * m;
                if (s == number) return true;
                if (s < number) l = m + 1;
                else r = m - 1;
            }
            return false;
        }

        public string GenerateRandomValid(int d)
        {
            long n = Random.Shared.NextInt64(0, (long)BigInteger.Pow(10, d / 2));
            BigInteger square = (BigInteger)n * n;
            return square.ToString();
        }
    }

    internal class TriangleSequence(string name, string description, string oeisid, int expected) : Sequence
    {
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string OEISID { get; } = oeisid;
        public int Expected { get; } = expected;
        public int MinDigits { get => 1; }
        public int MaxDigits { get => 15; }
        public bool IsValid(string n) => BigInteger.TryParse(n, out BigInteger result) && IsTriangleNumber(result);

        private bool IsTriangleNumber(BigInteger number)
        {
            if (number <= 0) return false;
            if (number == 1) return true;
            return Sequence.SearchValid(number, n => n * (n + 1) / 2);
        }
        public string GenerateRandomValid(int d)
        {
            BigInteger n = Random.Shared.NextInt64(1, (long)BigInteger.Pow(10, d / 2));
            BigInteger triangleNumber = n * (n + 1) / 2;
            return triangleNumber.ToString();
        }
    }
}
