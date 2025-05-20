using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobAtOEIS.GUI
{
    internal class Slider(string text, int x, int y, int width, int height) : Control
    {
        public string Text { get; set; } = text;
        private Func<string>? dynText;
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int Width { get; set; } = width;
        public int Height { get; set; } = height;
        public Action<float>? OnChange { get; set; }

        public float Value { get; set; }
        public float Min { get; set; } = 0f;
        public float Max { get; set; } = 1f;

        private bool pressed = false;
        public Slider(Func<string> text, int x, int y, int width , int height) : this(text(), x, y, width, height) {
            dynText = text;
        }
        public void Update()
        {
            if (X < Raylib.GetMouseX() && Raylib.GetMouseX() < X + Width &&
                Y < Raylib.GetMouseY() && Raylib.GetMouseY() < Y + Height || pressed)
            {
                float d;
                if (Math.Abs(d = Raylib.GetMouseWheelMove()) > 0.0f)
                {
                    Value += d / 100.0f / (Max - Min);
                    if (Value > Max) Value = Max;
                    if (Value < Min) Value = Min;
                    OnChange?.Invoke(Value);
                }
                else if (Raylib.IsMouseButtonPressed(MouseButton.Left) || (pressed && Raylib.IsMouseButtonDown(MouseButton.Left)))
                {
                    pressed = true;
                    Value = (Raylib.GetMouseX() - X - 2) / (float)(Width - 4);
                    if (Value > Max) Value = Max;
                    if (Value < Min) Value = Min;
                    OnChange?.Invoke(Value);
                }
                if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                    pressed = false;
            }
            if (dynText != null) Text = dynText!.Invoke();
        }
        public void Render()
        {
            Raylib.DrawRectangleRec(new Rectangle(X, Y, Width, Height), pressed ? Color.Beige : Color.LightGray);
            Raylib.DrawRectangleRec(new Rectangle(X, Y, Value * Width, Height), Color.Gray);
            Raylib.DrawRectangleLinesEx(new Rectangle(X, Y, Width, Height), 2, Color.Black);
            int t = Raylib.MeasureText(Text, 20);
            Raylib.DrawText(Text, X + (Width - t) / 2, Y + (Height - 20) / 2, 20, Color.Black);
        }

        public void Dispose()
        {

        }
    }
}
