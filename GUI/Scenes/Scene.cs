namespace JobAtOEIS.GUI.Scenes;

internal interface Scene : IDisposable
{
    void Render();

    static (int, int, int, int) CenterToTL(int cx, int cy, int width, int height)
    {
        return (cx - width / 2, cy - height / 2, width, height);
    }
}
