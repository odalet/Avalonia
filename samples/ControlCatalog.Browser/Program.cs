using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Web;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Rendering;
using Avalonia.Threading;
using ControlCatalog;
using ControlCatalog.Browser;

[assembly:SupportedOSPlatform("browser")]
#nullable enable

internal partial class Program
{
    // public static async Task Main(string[] args)
    // {
    //     Trace.Listeners.Add(new ConsoleTraceListener());
    //
    //     var options = ParseArgs(args) ?? new DemoBrowserPlatformOptions();
    //
    //     await BuildAvaloniaApp()
    //         .LogToTrace(options.LogLevel)
    //         .AfterSetup(_ =>
    //         {
    //             ControlCatalog.Pages.EmbedSample.Implementation = new EmbedSampleWeb();
    //         })
    //         .StartBrowserAppAsync("out", options);
    //
    //     Dispatcher.UIThread.Invoke(() =>
    //     {
    //         if (Application.Current!.ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
    //         {
    //             var topLevel = TopLevel.GetTopLevel(lifetime.MainView);
    //             topLevel!.RendererDiagnostics.DebugOverlays = options.DebugOverlays;
    //         }
    //     });
    // }

    private static AvaloniaView _avaloniaView1;
    private static AvaloniaView _avaloniaView2;
    public static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        var options = ParseArgs(args) ?? new DemoBrowserPlatformOptions();
        
        await BuildAvaloniaApp()
            .LogToTrace(options.LogLevel)
            .SetupBrowserAppAsync(options);

        _avaloniaView1 = new AvaloniaView("out1");
        _avaloniaView1.Content = new TextBlock { Text = "Hello" };

        _avaloniaView2 = new AvaloniaView("out2");
        _avaloniaView2.Content = new TextBlock { Text = "World" };

        Dispatcher.UIThread.Invoke(() =>
        {
            var topLevel = TopLevel.GetTopLevel(_avaloniaView1.Content);
            topLevel!.RendererDiagnostics.DebugOverlays = options.DebugOverlays;
        });
    }
    
    public static AppBuilder BuildAvaloniaApp()
           => AppBuilder.Configure<App>();

    private record DemoBrowserPlatformOptions : BrowserPlatformOptions
    {
        public RendererDebugOverlays DebugOverlays { get; set; }
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Warning;
    }

    private static DemoBrowserPlatformOptions? ParseArgs(string[] args)
    {
        try
        {
            if (args.Length == 0
                || !Uri.TryCreate(args[0], UriKind.Absolute, out var uri)
                || uri.Query.Length <= 1)
                return null;

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            var options = new DemoBrowserPlatformOptions();

            if (bool.TryParse(queryParams[nameof(options.PreferFileDialogPolyfill)], out var preferDialogsPolyfill))
            {
                options.PreferFileDialogPolyfill = preferDialogsPolyfill;
            }

            if (queryParams[nameof(options.RenderingMode)] is { } renderingModePairs)
            {
                options.RenderingMode = renderingModePairs
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(entry => Enum.Parse<BrowserRenderingMode>(entry, true))
                    .ToArray();
            }

            if (Enum.TryParse<RendererDebugOverlays>(queryParams[nameof(options.DebugOverlays)], true, out var debug))
            {
                options.DebugOverlays = debug;
            }

            if (Enum.TryParse<LogEventLevel>(queryParams[nameof(options.LogLevel)], true, out var logEventLevel))
            {
                options.LogLevel = logEventLevel;
            }

            Console.WriteLine("DemoBrowserPlatformOptions.PreferFileDialogPolyfill: " + options.PreferFileDialogPolyfill);
            Console.WriteLine("DemoBrowserPlatformOptions.RenderingMode: " + string.Join(";", options.RenderingMode));
            Console.WriteLine("DemoBrowserPlatformOptions.DebugOverlays: " + string.Join(";", options.DebugOverlays));
            Console.WriteLine("DemoBrowserPlatformOptions.LogLevel: " + string.Join(";", options.LogLevel));
            return options;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ParseArgs of DemoBrowserPlatformOptions failed: " + ex);
            return null;
        }
    }
}
