export enum LogEventLevel {
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Fatal
}

export async function getAvaloniaModule() {
    const global = globalThis as any;
    if (!global.AvaloniaModule) {
        const runtime = globalThis.getDotnetRuntime(0);
        const exports = await runtime?.getAssemblyExports("Avalonia.Browser.dll");
        global.AvaloniaModule = exports.Avalonia.Browser.Interop.AvaloniaModule;
    }

    return global.AvaloniaModule;
}

async function logMessageAsync(logLevel: LogEventLevel, message: string) {
    try {
        const module = await getAvaloniaModule();
        module.Log(logLevel, message);
    } catch {
        console.log("Avalonia.logMessage marshaling failed");
        console.log(message);
    }
}

///  Promises must be awaited, end with a call to .catch, end with a call to .then with a rejection handler or be explicitly marked as ignored with the `void` operator.
export function logMessage(logLevel: LogEventLevel, message: string): void {
    void logMessageAsync(logLevel, message);
}
