using System;
using System.Collections.Generic;
using System.Text;

namespace JobAtOEIS.Config
{
    internal class CharacterConfig
    {
        public string Name { get; set; }
        public int Hair { get; set; }
        public int HairColor { get; set; }
        public int Headwear { get; set; }
        public int SkinColor { get; set; }
        public int Top { get; set; }
        public int TopColor { get; set; }
        public int Bottom { get; set; }
        public int BottomColor { get; set; }

        public CharacterConfig()
        {
            Name = "";
        }

        public static CharacterConfig? Load(string data)
        {
            var s = data.Split(':');
            if (s.Length != 2) return null;
            (string name, string config) = (s[0], s[1]);
            if (config.Length != 8) return null;
            return new CharacterConfig()
            {
                Name = name,
                Hair = Convert.ToInt32(config[0..1], 16),
                HairColor = Convert.ToInt32(config[1..2], 16),
                Headwear = Convert.ToInt32(config[2..3], 16),
                SkinColor = Convert.ToInt32(config[3..4], 16),
                Top = Convert.ToInt32(config[4..5], 16),
                TopColor = Convert.ToInt32(config[5..6], 16),
                Bottom = Convert.ToInt32(config[6..7], 16),
                BottomColor = Convert.ToInt32(config[7..8], 16)
            };
        }

        public string Serialize()
        {
            return $"{Name}:{Hair:X}{HairColor:X}{Headwear:X}{SkinColor:X}{Top:X}{TopColor:X}{Bottom:X}{BottomColor:X}";
        }
    }
}
