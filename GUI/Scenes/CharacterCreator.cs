using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using Raylib_cs;
using System.Numerics;
using static JobAtOEIS.Config.CharacterConfig;
using static State;

namespace JobAtOEIS.GUI.Scenes;

internal class CharacterCreator : Scene
{
    List<Control> controls;
    Character character;
    Music bgm;
    Sound fail;
    Sound success;

    private Input codeInput;
    private Input nameInput;
    private Label hintLabel;

    private Texture2D header;

    private float phase = 0f;

    public CharacterCreator()
    {
        hintLabel = new Label("", 320, 230, 20);

        character = new Character(58, 30, SaveState.Load().Character);
        nameInput = new Input(() => T("Name"), character.Config.Name, 320, 60, 400, 30);
        nameInput.OnChange = () =>
        {
            if (string.IsNullOrWhiteSpace(nameInput.Value) || nameInput.Value.Contains(':'))
            {
                nameInput.Failed = true;
                return;
            }
            nameInput.Failed = false;
            hintLabel.Text = "";
            character.Config.Name = nameInput.Value;
        };
        nameInput.OnSubmit = () =>
        {
            if (string.IsNullOrWhiteSpace(nameInput.Value) || nameInput.Value.Contains(':'))
            {
                nameInput.Failed = true;
                Raylib.PlaySound(fail);
                return;
            }
            var s = SaveState.Load();
            s.Character = character.Config;
            s.Save();
            Raylib.PlaySound(success);
            Transition(new GameScene(true));
        };

        codeInput = new Input("", "", 320, 165, 400, 30) { ReadOnly = true, Centered = true };

        fail = Raylib.LoadSound(A("Assets/fail.wav"));
        bgm = Raylib.LoadMusicStream(A("Assets/Carl-os.qoa"));
        success = Raylib.LoadSound(A("Assets/success.wav"));
        header = Raylib.LoadTexture(A("Assets/header.png"));

        static int C(int x, int m) => (x + m) % m;

        controls = [
            new Button("<", 25, 20, 25, 25) {
                OnClick = () => character.Config.Hair = C(character.Config.Hair - 1, MAX_HAIR)
            }, new Button(">", 130, 20, 25, 25) {
                OnClick = () => character.Config.Hair = C(character.Config.Hair + 1, MAX_HAIR)
            }, new Button("<", 25, 50, 25, 25) {
                OnClick = () => character.Config.HairColor = C(character.Config.HairColor - 1, MAX_HAIR_COLOR)
            }, new Button(">", 130, 50, 25, 25) {
                OnClick = () => character.Config.HairColor = C(character.Config.HairColor + 1, MAX_HAIR_COLOR)
            }, new Button("<", 25, 80, 25, 25) {
                OnClick = () => character.Config.Headwear = C(character.Config.Headwear - 1, MAX_HEADWEAR)
            }, new Button(">", 130, 80, 25, 25) {
                OnClick = () => character.Config.Headwear = C(character.Config.Headwear + 1, MAX_HEADWEAR)
            }, new Button("<", 25, 110, 25, 25) {
                OnClick = () => character.Config.SkinColor = C(character.Config.SkinColor - 1, MAX_SKIN_COLOR)
            }, new Button(">", 130, 110, 25, 25) {
                OnClick = () => character.Config.SkinColor = C(character.Config.SkinColor + 1, MAX_SKIN_COLOR)
            }, new Button("<", 25, 140, 25, 25) {
                OnClick = () => character.Config.Top = C(character.Config.Top - 1, MAX_TOP)
            }, new Button(">", 130, 140, 25, 25) {
                OnClick = () => character.Config.Top = C(character.Config.Top + 1, MAX_TOP)
            }, new Button("<", 25, 170, 25, 25) {
                OnClick = () => character.Config.TopColor = C(character.Config.TopColor - 1, MAX_TOP_COLOR)
            }, new Button(">", 130, 170, 25, 25) {
                OnClick = () => character.Config.TopColor = C(character.Config.TopColor + 1, MAX_TOP_COLOR)
            }, new Button("<", 25, 200, 25, 25) {
                OnClick = () => character.Config.Bottom = C(character.Config.Bottom - 1, MAX_BOTTOM)
            }, new Button(">", 130, 200, 25, 25) {
                OnClick = () => character.Config.Bottom = C(character.Config.Bottom + 1, MAX_BOTTOM)
            }, new Button("<", 25, 230, 25, 25) {
                OnClick = () => character.Config.BottomColor = C(character.Config.BottomColor - 1, MAX_BOTTOM_COLOR)
            }, new Button(">", 130, 230, 25, 25) {
                OnClick = () => character.Config.BottomColor = C(character.Config.BottomColor + 1, MAX_BOTTOM_COLOR)
            },
            new Label(() => T("Create your Character"), 320, 25, 30),
            new Label(() => T("Hair Type"), 160, 25, 20),
            new Label(() => T("Hair Color"), 160, 55, 20),
            new Label(() => T("Face"), 160, 85, 20),
            new Label(() => T("Skin Color"), 160, 115, 20),
            new Label(() => T("Top"), 160, 145, 20),
            new Label(() => T("Top Color"), 160, 175, 20),
            new Label(() => T("Bottom"), 160, 205, 20),
            new Label(() => T("Bottom Color"), 160, 235, 20),
            nameInput,
            new Button(() => T("Save and Play"), 320, 100, 400, 30) {
                OnClick = nameInput.OnSubmit,
                SuppressSound = true
            },
            new Label(() => T("Your Character Code"), 320, 140, 20),
            codeInput,
            new Button(() => T("Copy Code"), 320, 200, 195, 30) {
                OnClick = () => {
                    Raylib.SetClipboardText(codeInput.Value);
                    Raylib.PlaySound(success);
                },
                SuppressSound = true
            },
            new Button(() => T("Paste Code"), 525, 200, 195, 30) {
                OnClick = () => {
                    var c = CharacterConfig.Load(Raylib.GetClipboardText_() ?? "");
                    if (c == null)
                    {
                        Raylib.PlaySound(fail);
                        hintLabel.Text = T("Invalid code format");
                        hintLabel.Color = Color.Red;
                        return;
                    }
                    character.Config = c;
                    nameInput.Value = c.Name;
                    Raylib.PlaySound(success);
                },
                SuppressSound = true
            },
            hintLabel
        ];
        Raylib.SetMusicVolume(bgm, 0.5f);
        Raylib.PlayMusicStream(bgm);
    }

    public void Render()
    {
        Raylib.UpdateMusicStream(bgm);

        phase += Raylib.GetFrameTime();
        if (phase >= 60) phase -= 60;

        float ascension = Math.Abs(30 - phase) / 30;
        Color sky = new(0.4f + 0.25f * ascension, 0.55f + 0.3f * ascension, 0.6f + 0.4f * ascension);

        Raylib.ClearBackground(sky);
        Raylib.DrawCircleV(
            new Vector2(700 - 20 * MathF.Sin(MathF.PI * phase / 30), 120 - 100 * ascension * ascension),
            50,
            Color.Yellow
        );

        Raylib.DrawRectangleRec(new Rectangle(0, 320, V_WIDTH, V_HEIGHT / 2), Color.Blue);
        if (5 <= phase && phase <= 15)
        {
            float t = (phase - 5) / 10;
            float t2 = (phase - 10) * (phase - 10) / 25;
            Raylib.DrawTriangle(
                new Vector2(270 + 200 * t, 290 + 70 * t2),
                new Vector2(270 + 200 * t, 310 + 70 * t2),
                new Vector2(300 + 200 * t, 300 + 70 * t2),
                Color.RayWhite
            );
            Raylib.DrawTriangleStrip([
                new Vector2(300 + 200 * t, 285 + 70 * t2),
                new Vector2(280 + 200 * t, 300 + 70 * t2),
                new Vector2(320 + 200 * t, 300 + 70 * t2),
                new Vector2(300 + 200 * t, 315 + 70 * t2),
            ], 4, Color.RayWhite);
            Raylib.DrawCircleV(new Vector2(310 + 200 * t, 300 + 70 * t2), 3f, Color.Black);
        }
        Raylib.DrawRectangleRec(new Rectangle(0, 330 - 20 * MathF.Sin(MathF.PI * phase / 6), V_WIDTH, V_HEIGHT / 2), Color.DarkBrown);
        Raylib.DrawTexturePro(header, new Rectangle(0, 0, 300, 150), new Rectangle(200, 340 - (int)(20 * MathF.Sin(MathF.PI * phase / 6)), 400, 100), new Vector2(0, 0), 0f, Color.Brown);

        character.Update();
        character.Render();


        if (nameInput.Failed)
        {
            hintLabel.Text = T("Name cannot be empty or contain ':'");
            hintLabel.Color = Color.Red;
        }
        codeInput.Value = character.Config.Serialize();

        foreach (var control in controls)
        {
            control.Update();
            control.Render();
        }
    }
    public void Dispose()
    {
        character.Dispose();
        Raylib.UnloadMusicStream(bgm);
        Raylib.UnloadSound(fail);
        Raylib.UnloadSound(success);
        foreach (var control in controls)
        {
            control.Dispose();
        }
    }
}
