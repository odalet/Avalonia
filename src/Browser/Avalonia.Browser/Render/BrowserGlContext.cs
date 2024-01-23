using System;
using System.Collections.Generic;
using Avalonia.OpenGL;

namespace Avalonia.Browser.Render;

internal class BrowserGlContext : IGlContext
{
    public BrowserGlContext()
    {
        GlInterface = null!;
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public object? TryGetFeature(Type featureType)
    {
        throw new NotImplementedException();
    }

    public bool IsLost { get; }
    public IDisposable EnsureCurrent()
    {
        throw new NotImplementedException();
    }

    public GlVersion Version { get; }
    public GlInterface GlInterface { get; }
    public int SampleCount { get; }
    public int StencilSize { get; }
    public IDisposable MakeCurrent()
    {
        throw new NotImplementedException();
    }

    public bool IsSharedWith(IGlContext context)
    {
        throw new NotImplementedException();
    }

    public bool CanCreateSharedContext { get; }
    public IGlContext? CreateSharedContext(IEnumerable<GlVersion>? preferredVersions = null)
    {
        throw new NotImplementedException();
    }
}
