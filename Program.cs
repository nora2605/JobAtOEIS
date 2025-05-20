using Raylib_cs;
using System.IO;
using System.Reflection;

Raylib.InitWindow(800, 480, "Job At OEIS");
Raylib.InitAudioDevice();

var music = Raylib.LoadMusicStream("Assets/Marquez.qoa");
Raylib.PlayMusicStream(music);

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.UpdateMusicStream(music);
    Raylib.ClearBackground(Color.White);
    var t = Raylib.MeasureText("Hello, world!", 20);
    Raylib.DrawText("Hello, World!", 400 - t / 2, 230, 20, Color.Black);
    Raylib.EndDrawing();
}

Raylib.CloseAudioDevice();
Raylib.CloseWindow();