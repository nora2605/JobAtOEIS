using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using Raylib_cs;
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

    public CharacterCreator()
    {
        hintLabel = new Label("", 320, 230, 20);

        character = new Character(58, 30, SaveState.Load().Character);
        nameInput = new Input(() => T("Name"), character.config.Name, 320, 60, 400, 30);
        nameInput.OnChange = () =>
        {
            if (string.IsNullOrWhiteSpace(nameInput.Value) || nameInput.Value.Contains(':'))
            {
                nameInput.Failed = true;
                return;
            }
            nameInput.Failed = false;
            hintLabel.Text = "";
            character.config.Name = nameInput.Value;
        };

        codeInput = new Input("", "", 320, 165, 400, 30) { ReadOnly = true, Centered = true };

        fail = Raylib.LoadSound(A("Assets/fail.wav"));
        bgm = Raylib.LoadMusicStream(A("Assets/Carl-os.qoa"));
        success = Raylib.LoadSound(A("Assets/success.wav"));

        controls = [
            new Button("<", 25, 20, 25, 25) {
                OnClick = () => character.config.Hair = (character.config.Hair + 15) % 16
            },
            new Button(">", 130, 20, 25, 25) {
                OnClick = () => character.config.Hair = (character.config.Hair + 1) % 16
            },
            new Button("<", 25, 50, 25, 25) {
                OnClick = () => character.config.HairColor = (character.config.HairColor + 10) % 11
            },
            new Button(">", 130, 50, 25, 25) {
                OnClick = () => character.config.HairColor = (character.config.HairColor + 1) % 11
            },
            new Button("<", 25, 80, 25, 25) {
                OnClick = () => character.config.Headwear = (character.config.Headwear + 15) % 16
            },
            new Button(">", 130, 80, 25, 25) {
                OnClick = () => character.config.Headwear = (character.config.Headwear + 1) % 16
            },
            new Button("<", 25, 110, 25, 25) {
                OnClick = () => character.config.SkinColor = (character.config.SkinColor + 4) % 5
            },
            new Button(">", 130, 110, 25, 25) {
                OnClick = () => character.config.SkinColor = (character.config.SkinColor + 1) % 5
            },
            new Button("<", 25, 140, 25, 25) {
                OnClick = () => character.config.Top = (character.config.Top + 15) % 16
            },
            new Button(">", 130, 140, 25, 25) {
                OnClick = () => character.config.Top = (character.config.Top + 1) % 16
            },
            new Button("<", 25, 170, 25, 25) {
                OnClick = () => character.config.TopColor = (character.config.TopColor + 15) % 16
            },
            new Button(">", 130, 170, 25, 25) {
                OnClick = () => character.config.TopColor = (character.config.TopColor + 1) % 16
            },
            new Button("<", 25, 200, 25, 25) {
                OnClick = () => character.config.Bottom = (character.config.Bottom + 15) % 16
            },
            new Button(">", 130, 200, 25, 25) {
                OnClick = () => character.config.Bottom = (character.config.Bottom + 1) % 16
            },
            new Button("<", 25, 230, 25, 25) {
                OnClick = () => character.config.BottomColor = (character.config.BottomColor + 15) % 16
            },
            new Button(">", 130, 230, 25, 25) {
                OnClick = () => character.config.BottomColor = (character.config.BottomColor + 1) % 16
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
            new Button(() => T("Save"), 320, 100, 400, 30) {
                OnClick = () => {
                    if (string.IsNullOrWhiteSpace(nameInput.Value) || nameInput.Value.Contains(':'))
                    {
                        Raylib.PlaySound(fail);
                        return;
                    }
                    var s = SaveState.Load();
                    s.Character = character.config;
                    s.Save();
                    Raylib.PlaySound(success);
                    Transition(new GameScene());
                },
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
                    var c = CharacterConfig.Load(Raylib.GetClipboardText_());
                    if (c == null)
                    {
                        Raylib.PlaySound(fail);
                        hintLabel.Text = T("Invalid code format");
                        hintLabel.Color = Color.Red;
                        return;
                    }
                    character.config = c;
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

        Raylib.ClearBackground(Color.White);
        
        character.Update();
        character.Render();


        if (nameInput.Failed)
        {
            hintLabel.Text = T("Name cannot be empty or contain ':'");
            hintLabel.Color = Color.Red;
        }
        else
            codeInput.Value = character.config.Serialize();

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
