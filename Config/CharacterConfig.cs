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

        public static (CharacterConfig?, int) Load(string[] lines)
        {
            if (lines.Length < 9) return (null, 0);
            return (new CharacterConfig()
            {
                Name = lines[0],
                Hair = int.TryParse(lines[1], out int hair) ? hair : 0,
                HairColor = int.TryParse(lines[2], out int hairColor) ? hairColor : 0,
                Headwear = int.TryParse(lines[3], out int headwear) ? headwear : 0,
                SkinColor = int.TryParse(lines[3], out int skinColor) ? skinColor : 0,
                Top = int.TryParse(lines[4], out int top) ? top : 0,
                TopColor = int.TryParse(lines[5], out int topColor) ? topColor : 0,
                Bottom = int.TryParse(lines[6], out int bottom) ? bottom : 0,
                BottomColor = int.TryParse(lines[7], out int bottomColor) ? bottomColor : 0
            }, 9);
        }

        public string[] Save()
        {
            return [
                Name,
                Hair.ToString(),
                HairColor.ToString(),
                Headwear.ToString(),
                SkinColor.ToString(),
                Top.ToString(),
                TopColor.ToString(),
                Bottom.ToString(),
                BottomColor.ToString()
            ];
        }
    }
}
