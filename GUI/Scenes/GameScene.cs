using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using JobAtOEIS.Sequences;
using Raylib_cs;

namespace JobAtOEIS.GUI.Scenes;

internal class GameScene : Scene
{
    List<Control> controls;

    Music bgm;
    Sound fail;
    Sound success;

    Character player;

    Character[] enemies;

    Input gameInput;

    Sequence CurrentSequence { get => SequenceManager.Sequences[sequence]; }
    int sequence = 0;

    public GameScene()
    {
        bgm = Raylib.LoadMusicStream(State.A("Assets/Pabl.o.qoa"));
        Raylib.SetMusicVolume(bgm, 0.5f);
        Raylib.PlayMusicStream(bgm);

        fail = Raylib.LoadSound(State.A("Assets/fail.wav"));
        success = Raylib.LoadSound(State.A("Assets/success.wav"));

        SequenceManager.Load();

        player = new Character(400 - 32, 100, SaveState.Load().Character);
        enemies = [
            new Character(200 - 32, 100, CharacterConfig.Random()),
            new Character(600 - 32, 100, CharacterConfig.Random()),
        ];

        gameInput = new Input(() => State.T("Try your best"), "", 250, 340, 300, 50)
        {
            LoseFocusOnSubmit = false,
        };
        gameInput.OnChange = () =>
        {
            if (!SequenceManager.Sequences[sequence].IsValid(gameInput.Value)) gameInput.Failed = true;
            else gameInput.Failed = false;
        };
        gameInput.OnSubmit = () =>
        {
            Raylib.PlaySound(gameInput.Failed ? fail : success);
            sequence = (sequence + 1) % SequenceManager.Sequences.Count;
            gameInput.Value = "";
        };

        controls = [
            //new Button(() => State.T("Shuffle"), 300, 400, 200, 50) {
            //    OnClick = () => {
            //        foreach (var enemy in enemies) {
            //            enemy.Config = CharacterConfig.Random();
            //        }
            //    }
            //}
            new Label(() => State.T(CurrentSequence.Name), 400, 400, 30) {
                Centered = true,
            },
            new Label(() => State.T(CurrentSequence.Description), 400, 440, 20) {
                Centered = true,
            },
            gameInput
        ];
    }

    public void Render()
    {
        gameInput.MaxLength = CurrentSequence.MaxDigits;

        Raylib.UpdateMusicStream(bgm);
        Raylib.ClearBackground(Color.White);

        player.Update();
        player.Render();
        int t = Raylib.MeasureText(player.Config.Name, 20);
        Raylib.DrawText(player.Config.Name, player.X + 32 - t / 2, player.Y - 25, 20, Color.Black);

        foreach (var enemy in enemies)
        {
            enemy.Update();
            enemy.Render();
            int et = Raylib.MeasureText(enemy.Config.Name, 20);
            Raylib.DrawText(enemy.Config.Name, enemy.X + 32 - et / 2, enemy.Y - 25, 20, Color.Black);
        }

        foreach (var control in controls)
        {
            control.Update();
            control.Render();
        }
    }
    public void Dispose() { }
}
