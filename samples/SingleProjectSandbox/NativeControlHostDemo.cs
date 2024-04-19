using System;
#if MACOS
using AppKit;
#endif
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Platform;

namespace SingleProjectSandbox;

public class NativeControlHostDemo : NativeControlHost
{
    private static bool duh;
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
#if MACOS
        if (!duh)
        {
            duh = true;
            NSApplication.Init();
        }

        // parent.Handle
        var edit = new TextField();
        edit.PlaceholderString = "type";
        return new MacOSViewHandle(edit);
#else
        return base.CreateNativeControlCore(parent);
#endif
    }
    
#if MACOS
    internal class TextField : NSTextField
    {
        public override void ViewWillMoveToSuperview(NSView? newSuperview)
        {
            base.ViewWillMoveToSuperview(newSuperview);
        }

        public override void ViewWillMoveToWindow(NSWindow? newWindow)
        {
            newWindow?.MakeKeyWindow();
            //newWindow?.MakeFirstResponder(this); // no effect
            base.ViewWillMoveToWindow(newWindow);
        }

        public override bool BecomeFirstResponder()
        {
            return base.BecomeFirstResponder();
        }

        public override bool AcceptsFirstResponder()
        {
            var def = base.AcceptsFirstResponder(); // always true
            return true;
        }

        public override bool AcceptsFirstMouse(NSEvent theEvent)
        {
            var def = base.AcceptsFirstMouse(theEvent); // always true
            return true;
        }

        public override void MouseDown(NSEvent theEvent)
        {
            //this.Editable = true;
            //this.Enabled = true;
            //this.Window.MakeFirstResponder(this); // nope
            base.MouseDown(theEvent);
        }

        public override void KeyDown(NSEvent theEvent)
        {
            base.KeyDown(theEvent); // never fired, even for working control
        }

        public override void KeyUp(NSEvent theEvent)
        {
            base.KeyUp(theEvent);
        }
    }
    
    internal class MacOSViewHandle : INativeControlHostDestroyableControlHandle
    {
        private NSView _view;

        public MacOSViewHandle(NSView view)
        {
            _view = view;
        }

        public IntPtr Handle => _view?.Handle ?? IntPtr.Zero;
        public string HandleDescriptor => "NSView";

        public void Destroy()
        {
            _view.Dispose();
            _view = null;
        }
    }
#endif
}
