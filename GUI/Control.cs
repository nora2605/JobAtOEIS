using System;
using System.Collections.Generic;
using System.Text;

namespace JobAtOEIS.GUI
{
    internal interface Control : IDisposable
    {
        void Update();
        void Render();
    }
}
