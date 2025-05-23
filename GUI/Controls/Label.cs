using Raylib_cs;

namespace JobAtOEIS.GUI.Controls;

internal class Label(string text, int x, int y, int fontSize) : Control
{
    public string Text { get; set; } = text;
    private Func<string>? dynText;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int FontSize { get; set; } = fontSize;
    public Color Color { get; set; } = Color.Black;
    public bool Centered { get; set; } = false;
    public Label(Func<string> text, int x, int y, int fontSize) : this(text(), x, y, fontSize)
    {
        dynText = text;
    }
    public void Update()
    {
        if (dynText != null) Text = dynText.Invoke();
    }
    public void Render()
    {
        int t = Centered ? Raylib.MeasureText(Text, FontSize) : 0;
        Raylib.DrawText(Text, X - t / 2, Y, FontSize, Color);
    }

    public void Dispose()
    {

    }
}
