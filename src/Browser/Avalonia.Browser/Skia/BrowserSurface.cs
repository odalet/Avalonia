using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Avalonia.Browser.Interop;
using Avalonia.Browser.Render;
using Avalonia.Logging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace Avalonia.Browser.Skia;

internal abstract unsafe class BrowserSurface : IDisposable, IRenderTimer
{
    private readonly BrowserRenderingMode _renderingMode;
    private readonly GCHandle _handle;

    protected BrowserSurface(JSObject jsSurface, BrowserRenderingMode renderingMode)
    {
        _handle = GCHandle.Alloc(this, GCHandleType.Weak);
        _renderingMode = renderingMode;
        JsSurface = jsSurface;

        Scaling = 1;
        ClientSize = new Size(100, 100);
        RenderSize = new PixelSize(100, 100);
        
        Emscripten.RequestAnimationFrameLoop(&Frame, (IntPtr)_handle);
    }

    [UnmanagedCallersOnly]
    private static int Frame(double time, nint userData)
    {
        if (userData == 0
            || !(GCHandle.FromIntPtr(userData).Target is BrowserSurface surface))
        {
            return 0;
        }

        surface.Tick?.Invoke(TimeSpan.FromMilliseconds(time));
        return 1;
    }
    
    public bool IsOffscreen =>
        _renderingMode is BrowserRenderingMode.OffscreenSoftware2D or BrowserRenderingMode.OffscreenWebGL2;

    public bool IsWebGl => _renderingMode is BrowserRenderingMode.WebGL1 or BrowserRenderingMode.WebGL2
        or BrowserRenderingMode.OffscreenWebGL2;

    public JSObject JsSurface { get; private set; }
    public double Scaling { get; private set; }
    public Size ClientSize { get; private set; }

    public bool IsValid => RenderSize.Width > 0 && RenderSize.Height > 0 && Scaling > 0;

    public PixelSize RenderSize { get; private set; }

    public event Action<TimeSpan>? Tick;

    public bool RunsInBackground => IsOffscreen;

    public static BrowserSurface Create(JSObject container, PixelFormat pixelFormat,
        Action<BrowserSurface> onSizeChanged, Action<BrowserSurface> onScalingChanged)
    {
        var opts = AvaloniaLocator.Current.GetService<BrowserPlatformOptions>() ?? new BrowserPlatformOptions();
        if (opts.RenderingMode is null || !opts.RenderingMode.Any())
        {
            throw new InvalidOperationException($"{nameof(BrowserPlatformOptions)}.{nameof(BrowserPlatformOptions.RenderingMode)} must not be empty or null");
        }

        BrowserSurface? surface = null;
        foreach (var mode in opts.RenderingMode)
        {
            try
            {
                var (jsSurface, jsGlInfo) = CanvasHelper.CreateSurface(container, mode);
                surface = jsGlInfo != null
                    ? new BrowserGlSurface(jsSurface, jsGlInfo, pixelFormat, mode)
                    : new BrowserRasterSurface(jsSurface, pixelFormat, mode);
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.BrowserPlatform)?
                    .Log(null,
                        "Creation of BrowserSurface with mode {Mode} failed with an error:\r\n{Exception}",
                        mode, ex);
            }
        }

        if (surface is null)
        {
            throw new InvalidOperationException($"{nameof(BrowserPlatformOptions)}.{nameof(BrowserPlatformOptions.RenderingMode)} has a value of \"{string.Join(", ", opts.RenderingMode)}\", but no options were applied.");
        }

        CanvasHelper.RequestAnimationFrame(surface.JsSurface, () =>
        {
            if (surface.IsValid)
            {
                if (!surface.IsOffscreen)
                {
                    // TODO: do we need it?
                    Dispatcher.UIThread.RunJobs(DispatcherPriority.UiThreadRender);
                }

                //surface.Tick?.Invoke(_sw.Elapsed);   
            }
        });
        CanvasHelper.OnSizeChanged(surface.JsSurface, (pixelWidth, pixelHeight, dpr) =>
        {
            var oldScaling = surface.Scaling;
            var oldClientSize = surface.ClientSize;
            surface.RenderSize = new PixelSize(pixelWidth, pixelHeight);
            surface.ClientSize = surface.RenderSize.ToSize(dpr);
            surface.Scaling = dpr;
            if (oldClientSize != surface.ClientSize)
                onSizeChanged(surface);
            if (Math.Abs(oldScaling - dpr) > 0.0001)
                onScalingChanged(surface);
        });
        return surface;
    }

    public virtual void Dispose()
    {
        _handle.Free();
        CanvasHelper.Destroy(JsSurface);
        JsSurface.Dispose();
        JsSurface = null!;
    }
}
