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

    static Dictionary<int, Color> skinColors = new()
    {
        { 0, new Color(99, 74, 65) },
        { 1, new Color(161, 110, 75) },
        { 2, new Color(212, 170, 120) },
        { 3, new Color(230, 188, 152) },
        { 4, new Color(255, 231, 209) },
    };

    static Dictionary<int, Color> hairColors = new()
    {
        { 0, Color.Black },
        { 1, Color.DarkBrown },
        { 2, Color.Brown },
        { 3, Color.Beige },
        { 4, Color.Gray },
        { 5, Color.LightGray },
        { 6, Color.Purple },
        { 7, Color.Pink },
        { 8, Color.Gold },
        { 9, Color.Blue },
        { 10, Color.DarkGreen }
    };

    static Dictionary<int, Color> regularColors = new()
    {
        { 0, Color.DarkGray },
        { 1, Color.Gray },
        { 2, Color.RayWhite },
        { 3, Color.Brown },
        { 4, Color.Red },
        { 5, Color.Orange },
        { 6, Color.Yellow },
        { 7, Color.Green },
        { 8, Color.DarkGreen },
        { 9, Color.SkyBlue },
        { 10, Color.Blue },
        { 11, Color.Purple },
        { 12, Color.Violet },
        { 13, Color.Magenta },
        { 14, Color.Pink },
        { 15, Color.Black },
    };
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
