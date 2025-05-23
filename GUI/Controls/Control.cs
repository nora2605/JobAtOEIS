namespace JobAtOEIS.GUI.Controls;

internal interface Control : IDisposable
{
    void Update();
    void Render();
}
