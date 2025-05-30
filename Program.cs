﻿using JobAtOEIS.Config;
using JobAtOEIS.GUI.Scenes;
using Raylib_cs;
using static State;

#if !DEBUG
Raylib.SetTraceLogLevel(TraceLogLevel.Error);
#endif

Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
Raylib.SetTargetFPS(60);

Raylib.InitWindow(V_WIDTH, V_HEIGHT, T("Job at the OEIS"));
Raylib.SetWindowIcon(Raylib.LoadImage(A("Assets/icon.png")));

Raylib.InitAudioDevice();
Raylib.SetMasterVolume(settings.Volume);

currentScene = new Menu();

while (!Raylib.WindowShouldClose() && !shouldClose)
{
    Raylib.BeginDrawing();

    currentScene.Render();

    if (timeUntilTransition > -totalTransitionTime)
    {
        timeUntilTransition -= Raylib.GetFrameTime();
        if (timeUntilTransition <= 0f && nextScene != null)
        {
            currentScene.Dispose();
            currentScene = nextScene;
            nextScene = null;
        }
        Raylib.DrawRectangle(0, 0, V_WIDTH, V_HEIGHT, new Color(0f, 0f, 0f, 1f - Math.Abs(timeUntilTransition / totalTransitionTime)));
        if (timeUntilTransition <= -totalTransitionTime)
            timeUntilTransition = totalTransitionTime = 0f;
    }

    Raylib.EndDrawing();
}
currentScene?.Dispose();
nextScene?.Dispose();

Raylib.CloseAudioDevice();
Raylib.CloseWindow();

static class State
{
    public const int V_WIDTH = 800;
    public const int V_HEIGHT = 480;

    public static Settings settings = Settings.Load();
    public static Translator translator = new();

    public static bool shouldClose = false;

    public static Scene? currentScene;
    public static Scene? nextScene;

    public static float timeUntilTransition = 0f;
    public static float totalTransitionTime = 0f;

    public static void Transition(Scene newScene, float time = 1f)
    {
        nextScene = newScene;
        timeUntilTransition = totalTransitionTime = time;
    }

    public static string T(string text) => translator.Translate(text, settings.Language);
    public static string A(string asset) => Persistent.GetPath(asset);
}

