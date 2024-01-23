using System.Runtime.InteropServices;

namespace Avalonia.Browser.Render;

internal static class Emscripten
{
    [DllImport("emscripten", EntryPoint = "emscripten_request_animation_frame_loop")]
    internal static extern unsafe void RequestAnimationFrameLoop(delegate* unmanaged<double, nint, int> f, nint userDataPtr);
}
