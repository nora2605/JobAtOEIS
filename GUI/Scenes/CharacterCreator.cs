using JobAtOEIS.Config;
using JobAtOEIS.GUI.Controls;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using static State;

namespace JobAtOEIS.GUI.Scenes;

internal class CharacterCreator : Scene
{
    List<Control> controls;
    Character character;

    public CharacterCreator()
    {
        character = new Character(58, 30, SaveState.Load().Character);
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
        ];
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.White);
        character.Update();
        character.Render();
        foreach (var control in controls)
        {
            control.Update();
            control.Render();
        }
    }
    public void Dispose()
    {
        character.Dispose();
        foreach (var control in controls)
        {
            control.Dispose();
        }
    }
}
