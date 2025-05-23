using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobAtOEIS.GUI.Scenes;

internal class GameScene : Scene
{
    List<Control> controls;

    Music bgm;

    Character player;

    Character[] enemies;

    public GameScene()
    {
        bgm = Raylib.LoadMusicStream(State.A("Assets/Pabl.o.qoa"));
        Raylib.SetMusicVolume(bgm, 0.5f);
        Raylib.PlayMusicStream(bgm);

        player = new Character(400 - 32, 100, SaveState.Load().Character);
        enemies = [
            new Character(200 - 32, 100, CharacterConfig.Random()),
            new Character(600 - 32, 100, CharacterConfig.Random()),
        ];

        controls = [
            new Button(() => State.T("Shuffle"), 300, 400, 200, 50) {
                OnClick = () => {
                    foreach (var enemy in enemies) {
                        enemy.config = CharacterConfig.Random();
                    }               
                }
            }
        ];
    }

    public void Render()
    {
        Raylib.UpdateMusicStream(bgm);
        Raylib.ClearBackground(Color.White);

        player.Update();
        player.Render();
        int t = Raylib.MeasureText(player.config.Name, 20);
        Raylib.DrawText(player.config.Name, player.X + 32 - t / 2, player.Y - 25, 20, Color.Black);

        foreach (var enemy in enemies)
        {
            enemy.Update();
            enemy.Render();
            int et = Raylib.MeasureText(enemy.config.Name, 20);
            Raylib.DrawText(enemy.config.Name, enemy.X + 32 - et / 2, enemy.Y - 25, 20, Color.Black);
        }

        foreach (var control in controls)
        {
            control.Update();
            control.Render();
        }
    }
    public void Dispose() { }
}
