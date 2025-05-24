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

    List<char>[] towers = [
        [' ', ' ', ' '], // Enemy 1
        [' ', ' ', ' '], // Player
        [' ', ' ', ' '], // Enemy 2
    ];
    int waterHeight = 0;

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
            Centered = true
        };
        gameInput.OnChange = () =>
        {
            if (!SequenceManager.Sequences[sequence].IsValid(gameInput.Value)) gameInput.Failed = true;
            else gameInput.Failed = false;
        };
        gameInput.OnSubmit = () =>
        {
            Raylib.PlaySound(CurrentSequence.IsValid(gameInput.Value) ? success : fail);
            for (int i = 0; i < 2; i++)
            {
                string answer = GetNPCAnswer();
                enemies[i].Config.Name = answer;
            }
            sequence = (sequence + 1) % SequenceManager.Sequences.Count;
            gameInput.Value = "";
        };

        controls = [
            new Label(() => State.T(CurrentSequence.Name), 400, 400, 30) {
                Centered = true,
            },
            new Label(() => State.T(CurrentSequence.Description), 400, 440, 20) {
                Centered = true,
            },
            new Button(() => State.T("Help"), 160, 340, 80, 50) {
                OnClick = () => {
                    Raylib.OpenURL($"https://oeis.org/{CurrentSequence.OEISID}");
                },
                SuppressSound = true
            },
            new Button(() => State.T("Submit"), 560, 340, 80, 50) {
                OnClick = gameInput.OnSubmit,
                SuppressSound = true
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

    string GetNPCAnswer()
    {
        int dUB = rpois(CurrentSequence.Expected);
        if (dUB < CurrentSequence.MinDigits)
            return Random.Shared.Next(0, (int)Math.Pow(10, CurrentSequence.Expected + 1)).ToString();
        return CurrentSequence.GenerateRandomValid(dUB);
    }

    /// <summary>
    /// Sample from the Poisson distribution with mean l.
    /// </summary>
    /// <param name="l">the mean</param>
    /// <returns>sample</returns>
    static int rpois(int l)
    {
        int k = 0;
        double p = Math.Exp(-l),
            s = p,
            u = Random.Shared.NextDouble();
        while (u > s)
        {
            k++;
            p = p * l / k;
            s += p;
        }
        return k;
    }

    public void Dispose() { }
}
