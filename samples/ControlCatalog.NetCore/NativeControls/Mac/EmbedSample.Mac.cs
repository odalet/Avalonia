using System;

using Avalonia.Platform;
using Avalonia.Threading;

using ControlCatalog.Pages;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.WebKit;

namespace ControlCatalog.NetCore;

public class EmbedSampleMac : INativeDemoControl
{
    public IPlatformHandle CreateControl(bool isSecond, IPlatformHandle parent, Func<IPlatformHandle> createDefault)
    {
        // Note: We are using MonoMac for example purposes
        // It shouldn't be used in production apps
        MacHelper.EnsureInitialized();

        var edit = new NSTextField();
        return new MacOSViewHandle(edit);
    }
}
