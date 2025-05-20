using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobAtOEIS.GUI
{
    internal class GameScene : Scene
    {
        public void Render()
        {
            Raylib.ClearBackground(Color.White);
        }
        public void Dispose() { }
    }
}
