using JobAtOEIS.Config;
using Raylib_cs;
using System.Numerics;

namespace JobAtOEIS.GUI.Controls;

internal class Character(int x, int y, CharacterConfig c) : Control
{
    Texture2D atlas = Raylib.LoadTexture(State.A("Assets/character.png"));
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public CharacterConfig config { get; set; } = c;

    int blinkingTime = 0;

    public void Update()
    {
        blinkingTime = Math.Max(0, blinkingTime - 1);
        if (blinkingTime == 0)
        {
            int r = Raylib.GetRandomValue(0, 1000);
            if (r < 2)
            {
                blinkingTime = Raylib.GetRandomValue(10, 30);
            }
        }
    }

    public void Render()
    {
        Raylib.SetTextureFilter(atlas, TextureFilter.Point);
        
        // Head
        Raylib.DrawTexturePro(
            atlas,
            new Rectangle(spriteWidth * config.Headwear, spriteHeight * (int)(blinkingTime > 0 ? SpriteLocation.HeadBlinking : SpriteLocation.Head), spriteWidth, spriteHeight),
            new Rectangle(X, Y, 2 * spriteWidth, 2 * spriteHeight),
            new Vector2(),
            0f,
            skinColors[config.SkinColor]
        );
        // Hair
        Raylib.DrawTexturePro(
            atlas,
            new Rectangle(spriteWidth * config.Hair, spriteHeight * (int)SpriteLocation.Hair, spriteWidth, spriteHeight),
            new Rectangle(X, Y, 2 * spriteWidth, 2 * spriteHeight),
            new Vector2(),
            0f,
            hairColors[config.HairColor]
        );
        // Top
        Raylib.DrawTexturePro(
            atlas,
            new Rectangle(spriteWidth * config.Top, spriteHeight * (int)SpriteLocation.Top, spriteWidth, spriteHeight),
            new Rectangle(X, Y + 2 * spriteHeight, 2 * spriteWidth, 2 * spriteHeight),
            new Vector2(),
            0f,
            regularColors[config.TopColor]
        );
        // Bottom
        Raylib.DrawTexturePro(
            atlas,
            new Rectangle(spriteWidth * config.Bottom, spriteHeight * (int)SpriteLocation.Bottom, spriteWidth, spriteHeight),
            new Rectangle(X, Y + 4 * spriteHeight, 2 * spriteWidth, 2 * spriteHeight),
            new Vector2(),
            0f,
            regularColors[config.BottomColor]
        );
    }

    public void Dispose()
    {

    }

    static Color[] skinColors = [
        new Color(99, 74, 65),
        new Color(161, 110, 75),
        new Color(212, 170, 120),
        new Color(230, 188, 152),
        new Color(255, 231, 209),
    ];

    static Color[] hairColors = [
        Color.Black,
        Color.DarkBrown,
        Color.Brown,
        Color.Beige,
        Color.Gray,
        Color.LightGray,
        Color.Purple,
        Color.Pink,
        Color.Gold,
        Color.Blue,
        Color.DarkGreen,
        Color.Red,
        Color.RayWhite,
    ];

    static Color[] regularColors = [
        Color.DarkGray,
        Color.Gray,
        Color.RayWhite,
        Color.Brown,
        Color.Red,
        Color.Orange,
        Color.Yellow,
        Color.Green,
        Color.DarkGreen,
        Color.SkyBlue,
        Color.Blue,
        Color.Purple,
        Color.Violet,
        Color.Magenta,
        Color.Pink,
        Color.Black,
    ];
    const int spriteWidth = 32;
    const int spriteHeight = 32;
    enum SpriteLocation : int
    {
        Head = 0,
        HeadBlinking = 1,
        Hair = 2,
        Top = 3,
        Bottom = 4
    }
}
