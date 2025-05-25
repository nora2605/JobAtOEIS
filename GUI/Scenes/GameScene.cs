using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using JobAtOEIS.Sequences;
using Raylib_cs;

namespace JobAtOEIS.GUI.Scenes;

internal class GameScene : Scene
{
    List<Control> controls;
    List<Control> helpControls;

    Music bgm;
    Sound fail;
    Sound success;

    Character player;

    Character[] enemies;

    List<char>[] towers = [
        ['3', '1', '4', '1', '5', '9', '2', '6'],
        ['3', '1', '4', '1', '5', '9', '2', '6'],
        ['3', '1', '4', '1', '5', '9', '2', '6'],
    ];
    // above the water
    int[] towerHeights = [2, 2, 2];
    const int waterLevel = 310;

    bool go = false;
    bool showHelp = false;
    int helpPage = 0;

    Input gameInput;
    Sequence[] sequences;
    Sequence CurrentSequence { get => sequences[sequence]; }
    int sequence = 0;

    float phase = 0f;

    public GameScene(bool initialHelp)
    {
        showHelp = initialHelp;

        SequenceManager.Load();
        sequences = [..SequenceManager.Sequences];
        Random.Shared.Shuffle(sequences);

        bgm = Raylib.LoadMusicStream(State.A("Assets/Pabl.o.qoa"));
        Raylib.SetMusicVolume(bgm, 0.5f);
        Raylib.PlayMusicStream(bgm);

        fail = Raylib.LoadSound(State.A("Assets/fail.wav"));
        success = Raylib.LoadSound(State.A("Assets/success.wav"));


        player = new Character(400 - 32, 100, SaveState.Load().Character);
        enemies = [
            new Character(200 - 32, 100, CharacterConfig.Random()),
            new Character(600 - 32, 100, CharacterConfig.Random()),
        ];

        gameInput = new Input(() => State.T("Answer"), "", 250, 340, 300, 50)
        {
            LoseFocusOnSubmit = false,
            Centered = true,
            IsNumeric = true,
        };
        gameInput.OnSubmit = () =>
        {
            if (!CurrentSequence.IsValid(gameInput.Value))
            {
                Raylib.PlaySound(fail);
                gameInput.Value = "";
            }
            else Raylib.PlaySound(success);
            towers[1].AddRange(gameInput.Value.ToCharArray());
            towerHeights[1] += gameInput.Value.Length - CurrentSequence.Expected;
            for (int i = 0; i < 2; i++)
            {
                string answer = GetNPCAnswer();
                if (CurrentSequence.IsValid(answer))
                {
                    towers[2 * i].AddRange(answer.ToCharArray());
                    towerHeights[2 * i] += answer.Length - CurrentSequence.Expected;
                }
            }
            if (sequence + 1 >= sequences.Length)
            {
                State.Transition(new Menu());
                go = true;
                return;
            }
            sequence++;
            gameInput.Value = "";
        };

        controls = [
            new Label(() => State.T(CurrentSequence.Name), 400, 400, 30) {
                Centered = true,
            },
            new Label(() => State.T(CurrentSequence.Description), 400, 440, 20) {
                Centered = true,
            },
            new Button(() => "OEIS", 160, 340, 80, 50) {
                OnClick = () => {
                    Raylib.OpenURL($"https://oeis.org/{CurrentSequence.OEISID}");
                },
                SuppressSound = true
            },
            new Button(() => State.T("Submit"), 560, 340, 80, 50) {
                OnClick = gameInput.OnSubmit,
                SuppressSound = true
            },
            gameInput,
            new Button(() => State.T("Help"), 10, 10, 100, 30) {
                OnClick = () => {
                    showHelp = true;
                    helpPage = 0;
                }
            },
        ];

        helpControls = [
            new Label(() => State.T("How to Play"), 400, 55, 30) { Centered = true },
            new Label(() => State.T($"HelpText{helpPage}"), 400, 120, 20) { Centered = true },
            new Button(() => State.T("Previous"), 110, 390, 100, 30) {
                OnClick = () => {
                    helpPage = Math.Max(0, helpPage - 1);
                },
            },
            new Button(() => State.T(helpPage == 2 ? "Close" : "Next"), 590, 390, 100, 30) {
                OnClick = () => {
                    if (helpPage < 2) helpPage++;
                    else showHelp = false;
                },
            },
        ];
    }

    public void Render()
    {
        gameInput.MaxLength = CurrentSequence.MaxDigits;
        phase += Raylib.GetFrameTime();
        if (phase >= 60.0f) phase -= 60.0f;

        Raylib.UpdateMusicStream(bgm);
        Raylib.ClearBackground(Color.White);
        Color sea = Color.Blue;
        Raylib.DrawRectangleRec(new Rectangle(0, waterLevel + 8 * MathF.Sin(MathF.PI * phase / 12), 800, 300), sea);

        foreach (var (ti, tower) in towers.Index())
        {
            foreach (var (i, c) in tower.Reverse<char>().TakeLast(6 + towerHeights[ti]).Index())
            {
                Rectangle tile = new Rectangle(200 + ti * 200 - 50, waterLevel + (i - towerHeights[ti]) * 30, 100, 30);
                Raylib.DrawRectangleRec(tile, Color.RayWhite);
                tile.Height = 32;
                Raylib.DrawRectangleLinesEx(tile, 2f, Color.Black);
                int ct = Raylib.MeasureText(c.ToString(), 20);
                Raylib.DrawText(c.ToString(), (int)(tile.X + (tile.Width - ct) / 2), (int)(tile.Y + 2), 20, Color.Black);
            }
            if (tower.Count > 6 + towerHeights[ti] && 6 + towerHeights[ti] > 0) tower.RemoveRange(0, tower.Count - 6 - towerHeights[ti]);
        }
        sea.A /= 2;
        Raylib.DrawRectangleRec(new Rectangle(0, waterLevel + 8 * MathF.Sin(MathF.PI * phase / 12), 800, 300), sea);

        player.Y = waterLevel - towerHeights[1] * 30 - 64 * 3;
        player.Update();
        player.Render();
        int t = Raylib.MeasureText(player.Config.Name, 20);
        Raylib.DrawText(player.Config.Name, player.X + 32 - t / 2, player.Y - 25, 20, Color.Black);

        foreach (var (ei, enemy) in enemies.Index())
        {
            enemy.Y = waterLevel - towerHeights[2 * ei] * 30 - 64 * 3;
            enemy.Update();
            enemy.Render();
            int et = Raylib.MeasureText(enemy.Config.Name, 20);
            Raylib.DrawText(enemy.Config.Name, enemy.X + 32 - et / 2, enemy.Y - 25, 20, Color.Black);
        }


        Raylib.DrawRectangleRec(new Rectangle(50, 330, 700, 300), new Color(1f, 1f, 1f, .8f));
        Raylib.DrawRectangleLinesEx(new Rectangle(50, 330, 700, 300), 2, Color.Black);
        foreach (var control in controls)
        {
            if (!showHelp && !go) control.Update();
            control.Render();
        }

        if (showHelp)
        {
            Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 128));
            Raylib.DrawRectangle(100, 50, 600, 380, Color.White);
            Raylib.DrawRectangleLinesEx(new Rectangle(100, 50, 600, 380), 2, Color.Black);
            foreach (var control in helpControls)
            {
                if (!go) control.Update();
                control.Render();
            }
        }
    }

    string GetNPCAnswer()
    {
        // NPCs should have no clue here
        if (CurrentSequence.Expected == 0)
        {
            if (Random.Shared.NextDouble() < 0.05)
                // This will be an actual random one (arbitrary amount of digits) in integer sequences
                return CurrentSequence.GenerateRandomValid(0);
            return Random.Shared.Next(0, 1000).ToString();
        }
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
