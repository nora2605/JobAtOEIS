using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using JobAtOEIS.Sequences;
using Raylib_cs;
using System.Numerics;

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
        [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
        [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
        [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
    ];
    (bool correct, string answer)[] answers = [
        (false, ""),
        (false, ""),
        (false, ""),
    ];

    // above the water
    int[] towerHeights = [3, 3, 3];
    bool[] pout = [false, false, false];
    float towerHeightsAnimOffset = 0f;
    const int waterLevel = 448;

    bool go = false;
    bool csto = false;
    float animTimer = 0f;
    float fishEatTimer = 0f;

    float questionTimer = 10f;

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

        questionTimer = 11f; // transition

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

        gameInput = new Input(() => State.T("Answer"), "", 250, 90, 300, 50)
        {
            LoseFocusOnSubmit = false,
            Centered = true,
            IsNumeric = true,
        };
        gameInput.OnSubmit = () =>
        {
            csto = true;
            animTimer = 0f;
            bool pCorrect = CurrentSequence.IsValid(gameInput.Value);

            if (!pCorrect)
            {
                Raylib.PlaySound(fail);
                gameInput.Value = "";
            }
            else Raylib.PlaySound(success);

            answers[1] = (
                pCorrect,
                gameInput.Value
            );
            for (int i = 0; i < 2; i++)
            {
                if (pout[2 * i]) continue;
                string answer = GetNPCAnswer();
                answers[2 * i] = (CurrentSequence.IsValid(answer), answer);
            }
            gameInput.Value = "";
        };

        controls = [
            new Label(() => State.T(CurrentSequence.Name), 400, 20, 30) {
                Centered = true,
            },
            new Label(() => State.T(CurrentSequence.Description), 400, 60, 20) {
                Centered = true,
            },
            new Button(() => "OEIS", 160, 90, 80, 50) {
                OnClick = () => {
                    Raylib.OpenURL($"https://oeis.org/{CurrentSequence.OEISID}");
                },
                SuppressSound = true
            },
            new Button(() => State.T("Submit"), 560, 90, 80, 50) {
                OnClick = gameInput.OnSubmit,
                SuppressSound = true
            },
            gameInput,
            new Button(() => State.T("?"), 10, 10, 30, 30) {
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

        if (!go && !showHelp && !csto) questionTimer -= Raylib.GetFrameTime();
        if (!csto && questionTimer <= 0f) gameInput.OnSubmit!.Invoke();

        Raylib.UpdateMusicStream(bgm);

        Raylib.ClearBackground(Color.SkyBlue);
        
        Color sea = Color.Blue;
        Raylib.DrawRectangleRec(new Rectangle(0, waterLevel + 8 * MathF.Sin(MathF.PI * phase / 12), 800, 300), sea);

        foreach (var (ti, tower) in towers.Index())
        {
            foreach (var (i, c) in tower.Reverse<char>().TakeLast(6 + towerHeights[ti]).Index())
            {
                Rectangle tile = new(200 + ti * 200 - 40, waterLevel + (i - towerHeights[ti] - towerHeightsAnimOffset) * 20, 80, 20);
                Raylib.DrawRectangleRec(tile, Color.RayWhite);
                tile.Height += 2;
                Raylib.DrawRectangleLinesEx(tile, 2f, Color.Black);
                int ct = Raylib.MeasureText(c.ToString(), 10);
                Raylib.DrawText(c.ToString(), (int)(tile.X + (tile.Width - ct) / 2), (int)(tile.Y + 2), 20, Color.Black);
            }
            if (tower.Count > 6 + towerHeights[ti] && 6 + towerHeights[ti] > 0) tower.RemoveRange(0, tower.Count - 6 - towerHeights[ti]);
        }

        if (!pout[1])
        {
            if (fishEatTimer == 0f)
                player.Y = (int)(waterLevel - (towerHeights[1] + towerHeightsAnimOffset) * 20 - 64 * 3);
            player.Update();
            player.Render();
            int t = Raylib.MeasureText(player.Config.Name, 20);
            Raylib.DrawText(player.Config.Name, player.X + 32 - t / 2, player.Y - 25, 20, Color.Black);
        }

        foreach (var (ei, enemy) in enemies.Index())
        {
            if (!pout[2*ei])
            {
                if (fishEatTimer == 0f)
                    enemy.Y = (int)(waterLevel - (towerHeights[2 * ei] + towerHeightsAnimOffset) * 20 - 64 * 3);
                enemy.Update();
                enemy.Render();
                int et = Raylib.MeasureText(enemy.Config.Name, 20);
                Raylib.DrawText(enemy.Config.Name, enemy.X + 32 - et / 2, enemy.Y - 25, 20, Color.Black);
            }
        }

        if (csto)
        {
            if (animTimer <= 0.2f * answers.Max(x => x.answer.Length))
            {
                int tile = (int)(animTimer / 0.2f);
                float p = (animTimer % 0.2f) / 0.2f;
                for (int i = 0; i < 3; i++)
                {
                    float baseto = waterLevel - (towerHeights[i] + 1) * 20;
                    float th = -20 + (baseto + 20) * p;
                    if (answers[i].correct && tile < answers[i].answer.Length)
                    {
                        Rectangle b = new Rectangle(200 + i * 200 - 40, th, 80, 20);
                        Raylib.DrawRectangleRec(b, Color.RayWhite);
                        b.Height += 2;
                        Raylib.DrawRectangleLinesEx(b, 2f, Color.Black);
                        int ct = Raylib.MeasureText(answers[i].answer[tile].ToString(), 20);
                        Raylib.DrawText(answers[i].answer[tile].ToString(), (int)(b.X + (b.Width - ct) / 2), (int)(b.Y + 2), 20, Color.Black);
                        if (i == 1) player.Y = (int)(baseto + 56 * p * p - 76 * p);
                        else enemies[i / 2].Y = (int)(baseto + 56 * p * p - 76 * p);
                    }
                }
                if ((int)((animTimer + Raylib.GetFrameTime()) / 0.2f) > tile)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (answers[i].correct && tile < answers[i].answer.Length)
                        {
                            towers[i].Add(answers[i].answer[tile]);
                            towerHeights[i]++;
                        }
                    }
                }
            }
            else if (animTimer - 0.2f * answers.Max(x => x.answer.Length) <= 0.5f)
            {
                float p = 2 * (animTimer - 0.2f * answers.Max(x => x.answer.Length));
                for (int i = 0; i < 3; i++)
                {
                    towerHeightsAnimOffset = -p * CurrentSequence.Expected;
                }
            }
            else if (fishEatTimer == 0f)
            {
                for (int i = 0; i < 3; i++)
                {
                    towerHeights[i] -= CurrentSequence.Expected;
                    towerHeightsAnimOffset = 0f;
                }
                fishEatTimer = towerHeights.Index().Any(x => !pout[x.Index] && x.Item <= 0) ? 2f : 0f;
                if (fishEatTimer == 0f)
                    Resume();
            }
            animTimer += Raylib.GetFrameTime();
        }
        else if (!go && !showHelp)
        {
            Raylib.DrawRectangleRec(new Rectangle(360, 180, 80, 60), new Color(1f, 1f, 1f, 0.5f));
            Raylib.DrawRectangleLinesEx(new Rectangle(360, 180, 80, 60), 2, Color.Black);
            int tt = Raylib.MeasureText($"{questionTimer:0}", 60);
            Raylib.DrawText($"{questionTimer:0}", 400 - tt / 2, 180, 60, Color.Red);
        }

        if (fishEatTimer > 0f)
        {
            float p = 1f - fishEatTimer / 2f;
            float p2 = 4 * (p - 0.5f) * (p - 0.5f);
            for (int i = 0; i < 3; i++)
            {
                if (towerHeights[i] <= 0 && !pout[i])
                {
                    Vector2 fish1Pos = new(150 + 200 * i + 100 * p, waterLevel - 40 + (580 - waterLevel) * p2);
                    Vector2 fish2Pos = new(250 + 200 * i - 100 * p, waterLevel - 40 + (580 - waterLevel) * p2);
                    float fishRot = MathF.PI / 3f * (fishEatTimer - 1f);
                    DrawFish(fish1Pos, fishRot);
                    DrawFish(fish2Pos, MathF.PI-fishRot);
                    if (p >= 0.5f)
                    {
                        float pp = 2 * (p - 0.5f);
                        Character c = i == 1 ? player : enemies[i / 2];
                        c.Y -= (int)(30 * pp);
                    }
                }
            }

            fishEatTimer -= Raylib.GetFrameTime();
            if (fishEatTimer <= 0f)
            {
                fishEatTimer = 0f;
                for (int i = 0; i < 3; i++)
                {
                    if (towerHeights[i] <= 0)
                    {
                        pout[i] = true;
                    }
                }
                if (pout[1] || (pout[0] && pout[2])) go = true;
                Resume();
            }
        }

        sea.A /= 2;
        Raylib.DrawRectangleRec(new Rectangle(0, waterLevel + 8 * MathF.Sin(MathF.PI * phase / 12), 800, 300), sea);
        
        Raylib.DrawRectangleRec(new Rectangle(50, 10, 700, 140), new Color(1f, 1f, 1f, .8f));
        Raylib.DrawRectangleLinesEx(new Rectangle(50, 10, 700, 140), 2, Color.Black);
        foreach (var control in controls)
        {
            if (!showHelp && !go && !csto) control.Update();
            control.Render();
        }

        if (go)
        {
            Raylib.DrawRectangleRec(new Rectangle(0, 0, 800, 600), new Color(0, 0, 0, 128));
            Raylib.DrawRectangle(100, 100, 600, 280, Color.White);
            Raylib.DrawRectangleLinesEx(new Rectangle(100, 100, 600, 280), 2, Color.Black);
            Raylib.DrawText(State.T(pout[1] ? "You lose!" : "You win!"), 400 - Raylib.MeasureText(State.T(pout[1] ? "You lose!" : "You win!"), 30) / 2, 110, 30, Color.Black);
            var bt = new Button(() => State.T("Back to Menu"), 300, 215, 200, 50)
            {
                OnClick = () =>
                {
                    Raylib.PlaySound(success);
                    State.Transition(new Menu());
                },
                SuppressSound = true,
            };
            bt.Update();
            bt.Render();
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

    void DrawFish(Vector2 pos, float rot)
    {
        Vector2 h = new(MathF.Sin(rot), MathF.Cos(rot));
        Vector2 w = new(MathF.Cos(rot), -MathF.Sin(rot));
        Raylib.DrawTriangle(pos - 30 * w - 10 * h, pos - 30 * w + 10 * h, pos, Color.RayWhite);
        Raylib.DrawTriangleStrip([
            pos - 15 * h,
            pos - 20 * w,
            pos + 20 * w,
            pos + 15 * h,
        ], 4, Color.RayWhite);
        Raylib.DrawCircleV(pos + 5 * w, 3f, Color.Black);
    }

    void Resume()
    {
        if (sequence + 1 >= sequences.Length)
        {
            State.Transition(new Menu());
            go = true;
        }
        else
        {
            questionTimer = 10f;
            sequence++;
        }
        csto = false;
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

    public void Dispose() {
        Raylib.UnloadMusicStream(bgm);
        Raylib.UnloadSound(fail);
        Raylib.UnloadSound(success);
        player.Dispose();
        foreach (var enemy in enemies)
            enemy.Dispose();
        foreach (var control in controls)
            control.Dispose();
        foreach (var control in helpControls)
            control.Dispose();
    }
}
