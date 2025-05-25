using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using JobAtOEIS.Sequences;
using Raylib_cs;
using System.Numerics;

namespace JobAtOEIS.GUI.Scenes;

internal class GameScene : Scene
{
    const int waterLevel = 448;

    List<Control> controls;
    List<Control> helpControls;
    List<Control> goSControls;
    Input gameInput;

    Music bgm;
    Sound fail;
    Sound success;

    SaveState save;

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
    int[] towerHeights = [3, 3, 3];
    bool[] pout = [false, false, false];
    int score = 0;

    Sequence[] sequences;
    Sequence CurrentSequence { get => sequences[sequence]; }
    int sequence = 0;

    float questionTimer = 10f;

    public GameScene(bool initialHelp)
    {
        showHelp = initialHelp;

        save = SaveState.Load();

        questionTimer = initialHelp ? 10f : 11f; // transition

        SequenceManager.Load();
        sequences = [.. SequenceManager.Sequences];
        Random.Shared.Shuffle(sequences);

        bgm = Raylib.LoadMusicStream(State.A("Assets/Pabl.o.qoa"));
        Raylib.SetMusicVolume(bgm, 0.5f);
        Raylib.PlayMusicStream(bgm);

        fail = Raylib.LoadSound(State.A("Assets/fail.wav"));
        success = Raylib.LoadSound(State.A("Assets/success.wav"));


        player = new Character(400 - 32, 100, save.Character);
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
            animSuspend = true;
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
                string answer = pout[2 * i] ? "" : GetNPCAnswer();
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

        goSControls = [
            new Button(() => State.T("Back to Menu"), 300, 275, 200, 50)
            {
                OnClick = () =>
                {
                    Raylib.PlaySound(success);
                    State.Transition(new Menu());
                },
                SuppressSound = true,
            },
            new Button(() => State.T("Play again"), 300, 215, 200, 50) {
                OnClick = () =>
                {
                    Raylib.PlaySound(success);
                    State.Transition(new GameScene(false));
                },
                SuppressSound = true,
            }
        ];
    }

    float phase = 0f;
    public void Render()
    {
        Raylib.UpdateMusicStream(bgm);

        // Sky
        Raylib.ClearBackground(new Color(0.8f, 0.9f, 1f));

        // Water Background
        phase += Raylib.GetFrameTime();
        if (phase >= 60.0f) phase -= 60.0f;
        Color sea = Color.Blue;
        Raylib.DrawRectangleRec(new Rectangle(0, waterLevel + 8 * MathF.Sin(MathF.PI * phase / 12), 800, 300), sea);

        DrawTowers();

        // Player and Enemies
        for (int i = 0; i < 3; i++)
        {
            Character c = i == 1 ? player : enemies[i / 2];
            if (!pout[i])
            {
                if (!animSuspend)
                    c.Y = waterLevel - towerHeights[i] * 20 - 64 * 3;
                c.Update();
                c.Render();
                int t = Raylib.MeasureText(c.Config.Name, 20);
                Raylib.DrawText(c.Config.Name, c.X + 32 - t / 2, c.Y - 25, 20, Color.Black);
            }
        }

        if (animSuspend)
            TowerAnim();
        else if (!gameOver && !showHelp)
            DrawTimer();

        if (fishEatTimer > 0f) FishEatAnim();

        // Water Foreground
        sea.A /= 2;
        Raylib.DrawRectangleRec(new Rectangle(0, waterLevel + 8 * MathF.Sin(MathF.PI * phase / 12), 800, 300), sea);

        // Game Controls
        gameInput.MaxLength = CurrentSequence.MaxDigits;
        Raylib.DrawRectangleRec(new Rectangle(50, 10, 700, 140), new Color(1f, 1f, 1f, .8f));
        Raylib.DrawRectangleLinesEx(new Rectangle(50, 10, 700, 140), 2, Color.Black);
        foreach (var control in controls)
        {
            if (!showHelp && !gameOver && !animSuspend) control.Update();
            control.Render();
        }

        if (gameOver) DrawGOScreen();

        if (showHelp) DrawHelp();
    }

    void DrawTowers()
    {
        foreach (var (ti, tower) in towers.Index())
        {
            foreach (var (i, c) in tower.Reverse<char>().Take(6 + towerHeights[ti]).Index())
            {
                Rectangle tile = new(200 + ti * 200 - 40, waterLevel + (i - towerHeights[ti] - towerHeightsAnimOffset) * 20, 80, 20);
                Raylib.DrawRectangleRec(tile, Color.RayWhite);
                tile.Height += 2;
                Raylib.DrawRectangleLinesEx(tile, 2f, Color.Black);
                int ct = Raylib.MeasureText(c.ToString(), 10);
                Raylib.DrawText(c.ToString(), (int)(tile.X + (tile.Width - ct) / 2), (int)(tile.Y + 2), 20, Color.Black);
            }
        }
    }

    void DrawGOScreen()
    {
        Raylib.DrawRectangleRec(new Rectangle(0, 0, 800, 600), new Color(0, 0, 0, 128));
        Raylib.DrawRectangle(100, 100, 600, 280, Color.White);
        Raylib.DrawRectangleLinesEx(new Rectangle(100, 100, 600, 280), 2, Color.Black);
        Raylib.DrawText(State.T(pout[1] ? "You lose!" : "You win!"), 400 - Raylib.MeasureText(State.T(pout[1] ? "You lose!" : "You win!"), 30) / 2, 110, 30, Color.Black);
        Raylib.DrawText($"{State.T("Score")}: {score}", 400 - Raylib.MeasureText($"{State.T("Score")}: {score}", 20) / 2, 150, 20, Color.Black);
        Raylib.DrawText($"{State.T("Highscore")}: {save.HighScore}", 400 - Raylib.MeasureText($"{State.T("Highscore")}: {save.HighScore}", 20) / 2, 180, 20, Color.Black);
        foreach (var control in goSControls)
        {
            control.Update();
            control.Render();
        }
    }

    void DrawTimer()
    {
        questionTimer -= Raylib.GetFrameTime();
        if (questionTimer <= 0f) gameInput.OnSubmit!.Invoke();

        Raylib.DrawRectangleRec(new Rectangle(360, 190, 80, 60), new Color(1f, 1f, 1f, 0.5f));
        Raylib.DrawRectangleLinesEx(new Rectangle(360, 190, 80, 60), 2, Color.Black);
        int tt = Raylib.MeasureText($"{questionTimer:0}", 60);
        Raylib.DrawText($"{questionTimer:0}", 400 - tt / 2, 190, 60, Color.Red);
    }

    bool showHelp = false;
    int helpPage = 0;
    void DrawHelp()
    {
        Raylib.DrawRectangle(0, 0, 800, 600, new Color(0, 0, 0, 128));
        Raylib.DrawRectangle(100, 50, 600, 380, Color.White);
        Raylib.DrawRectangleLinesEx(new Rectangle(100, 50, 600, 380), 2, Color.Black);
        foreach (var control in helpControls)
        {
            if (!gameOver) control.Update();
            control.Render();
        }
    }

    float fishEatTimer = 0f;
    void FishEatAnim()
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
                DrawFish(fish2Pos, MathF.PI - fishRot);
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
            Resume();
            if (pout[1] || (pout[0] && pout[2]))
                GameOver();
        }
    }

    float towerHeightsAnimOffset = 0f;
    int tileAnimOffset = 0;
    bool animSuspend = false;
    float animTimer = 0f;
    void TowerAnim()
    {
        if (animTimer < 0.2f * answers.Max(x => x.answer.Length))
        {
            int tile = (int)(animTimer / 0.2f);
            float p = (animTimer % 0.2f) / 0.2f;
            for (int i = 0; i < 3; i++)
            {
                float baseto = waterLevel - (towerHeights[i] + 1) * 20;
                float th = -20 + (baseto + 20) * p;
                if (answers[i].correct && tile < answers[i].answer.Length)
                {
                    Rectangle b = new(200 + i * 200 - 40, th, 80, 20);
                    Raylib.DrawRectangleRec(b, Color.RayWhite);
                    b.Height += 2;
                    Raylib.DrawRectangleLinesEx(b, 2f, Color.Black);
                    int ct = Raylib.MeasureText(answers[i].answer[tile].ToString(), 20);
                    Raylib.DrawText(answers[i].answer[tile].ToString(), (int)(b.X + (b.Width - ct) / 2), (int)(b.Y + 2), 20, Color.Black);

                    if (i == 1) player.Y = (int)(baseto + 56 * p * p - 76 * p) - 64 * 3;
                    else enemies[i / 2].Y = (int)(baseto + 56 * p * p - 76 * p) - 64 * 3;
                }
                else
                {
                    if (i == 1) player.Y = (int)(waterLevel - (towerHeights[i] + towerHeightsAnimOffset) * 20 - 64 * 3);
                    else enemies[i / 2].Y = (int)(waterLevel - (towerHeights[i] + towerHeightsAnimOffset) * 20 - 64 * 3);
                }
            }
            UpdateTower(tile);
        }
        else if (animTimer - 0.2f * answers.Max(x => x.answer.Length) <= 0.5f)
        {
            UpdateTower(answers.Max(x => x.answer.Length));
            float p = 2 * (animTimer - 0.2f * answers.Max(x => x.answer.Length));
            for (int i = 0; i < 3; i++)
            {
                if (i == 1) player.Y = (int)(waterLevel - (towerHeights[i] + towerHeightsAnimOffset) * 20 - 64 * 3);
                else enemies[i / 2].Y = (int)(waterLevel - (towerHeights[i] + towerHeightsAnimOffset) * 20 - 64 * 3);
                towerHeightsAnimOffset = -p * CurrentSequence.Expected;
            }
        }
        else
        {
            if (fishEatTimer == 0f)
            {
                for (int i = 0; i < 3; i++)
                {
                    towerHeights[i] -= CurrentSequence.Expected;
                }
                towerHeightsAnimOffset = 0f;
                UpdateTower(answers.Max(x => x.answer.Length));
                tileAnimOffset = 0;
                fishEatTimer = towerHeights.Index().Any(x => !pout[x.Index] && x.Item <= 0) ? 2f : 0f;
                if (fishEatTimer == 0f)
                    Resume();
            }
        }
        animTimer += Raylib.GetFrameTime();
    }

    void UpdateTower(int to)
    {
        for (int ti = tileAnimOffset; ti < to; ti++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (answers[i].correct && ti < answers[i].answer.Length)
                {
                    towers[i].Add(answers[i].answer[ti]);
                    towerHeights[i]++;
                }
            }
            tileAnimOffset = to;
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
            GameOver();
        else
        {
            questionTimer = 10f;
            sequence++;
        }
        score += answers[1].answer.Length;
        animSuspend = false;
    }

    bool gameOver = false;
    void GameOver()
    {
        gameOver = true;
        if (!pout[1]) save.HighScore = Math.Max(save.HighScore, score);
        save.Save();
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

    public void Dispose()
    {
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
