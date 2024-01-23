using Avalonia.Platform;

namespace Avalonia.Browser.Render;

internal class BrowserPlatformGraphics : IPlatformGraphics
{
    public bool UsesSharedContext => true;
    public IPlatformGraphicsContext CreateContext()
    {
        throw new System.NotImplementedException();
    }

    public IPlatformGraphicsContext GetSharedContext()
    {
        throw new System.NotImplementedException();
    }
}
