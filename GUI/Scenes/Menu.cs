using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static State;

namespace JobAtOEIS.GUI.Scenes;

internal class Menu : Scene
{
    List<Control> menuControls;
    List<Control> settingsControls;

    List<(int, int)> bgNums;
    float phase;

    bool showSettings = false;

    Music bgm;
    Texture2D header;
    Sound success;

    public Menu()
    {
        success = Raylib.LoadSound(A("Assets/success.wav"));
        menuControls = [
            new Button(() => T("Play"), V_WIDTH / 2 - 100, V_HEIGHT / 2, 200, 50) {
                OnClick = () => {
                    SaveState s = SaveState.Load();
                    Raylib.PlaySound(success);
                    if (s.Character.Name == "") {
                        Transition(new CharacterCreator());
                    }
                    else Transition(new GameScene());
                },
                SuppressSound = true,
            },
            new Button(() => T("Settings"), V_WIDTH / 2 - 100, V_HEIGHT / 2 + 75, 200, 50) {
                OnClick = () => { showSettings = true; }
            },
            new Button(() => T("Exit"), V_WIDTH / 2 - 100, V_HEIGHT / 2 + 150, 200, 50) {
                OnClick = () => { shouldClose = true; }
            },
        ];

        settingsControls = [
            new Button(() => $"{T("Language")}: {settings.Language}", V_WIDTH / 2 - 100, V_HEIGHT / 2, 200, 50) {
                OnClick = () => {
                    settings.Language = translator.AvailableLanguages[
                        (Array.IndexOf(translator.AvailableLanguages, settings.Language) + 1) %
                        translator.AvailableLanguages.Length
                    ];
                }
                
            },
            new Slider(() => $"{T("Volume")}: {settings.Volume:0.00}", V_WIDTH / 2 - 100, V_HEIGHT / 2 + 75, 200, 50) {
                OnChange = (value) => {
                    settings.Volume = value;
                },
                Value = settings.Volume,
            },
            new Button(() => T("Save"), V_WIDTH / 2 - 100, V_HEIGHT / 2 + 150, 200, 50) {
                OnClick = () => {
                    settings.Save();
                    showSettings = false;
                }
            },
        ];

        Random r = new();

        bgNums = [.. Enumerable.Range(0, 60).Select(_ => (r.Next(0, 1000), r.Next(0, V_HEIGHT)))];
        phase = (float)r.NextDouble() * 1000;

        bgm = Raylib.LoadMusicStream(A("Assets/Marquez.qoa"));
        Raylib.SetMusicVolume(bgm, 0.5f);
        Raylib.PlayMusicStream(bgm);

        header = Raylib.LoadTexture(A("Assets/header.png"));
    }

    public void Render()
    {
        Raylib.UpdateMusicStream(bgm);
        Raylib.ClearBackground(Color.RayWhite);

        foreach (var (i, y) in bgNums)
            Raylib.DrawText($"{i}", (int)(phase * (i % 100 + 1)) % V_WIDTH, y, 10, Color.Red);

        int seaLevel = V_HEIGHT * 3 / 4 + (int)(30 * Math.Sin(phase / 2));
        // sea
        Raylib.DrawRectangle(0, seaLevel, V_WIDTH, V_HEIGHT / 2, Color.Blue);
        // boat
        float boatAngle = 4 * (float)Math.Cos(phase / 3);
        float boatR = boatAngle * MathF.PI / 180;
        int boatX = (int)(phase * 13 % (V_WIDTH + 100) - 50);
        Raylib.DrawCircleSector(
            new Vector2(boatX, seaLevel - 20),
            50.0f,
            boatAngle,
            180 + boatAngle,
            10,
            Color.DarkBrown
        );
        Raylib.DrawRectanglePro(
            new Rectangle(boatX, seaLevel - 20, 10, 120),
            new Vector2(5, 80),
            boatAngle,
            Color.DarkBrown
        );
        Raylib.DrawTriangle(
            new Vector2(boatX + (float)Math.Sin(boatR) * 80, seaLevel - 20 - 80 * (float)Math.Cos(boatR)),
            new Vector2(boatX, seaLevel - 20 - 10 * (float)Math.Cos(boatR)),
            new Vector2(boatX + 50 - (float)Math.Sin(boatR), seaLevel - 20 - 10 * (float)Math.Cos(boatR)),
            Color.Red
        );

        Raylib.DrawTexture(header, V_WIDTH / 2 - 150, 50, Color.White);

        if (!showSettings)
            foreach (var control in menuControls)
            {
                control.Update();
                control.Render();
            }
        else
            foreach (var control in settingsControls)
            {
                control.Update();
                control.Render();
            }

        phase += Raylib.GetFrameTime();
    }

    public void Dispose()
    {
        Raylib.UnloadMusicStream(bgm);
        Raylib.UnloadTexture(header);
        foreach (var control in menuControls)
            control.Dispose();
        foreach (var control in settingsControls)
            control.Dispose();
    }
}
