using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace RogueEssence.Dev.Native
{
    [SupportedOSPlatform("linux")]
    internal class Linux : OS.IBackend
    {
        public void SetupApp(AppBuilder builder)
        {
            builder.With(new X11PlatformOptions() { EnableIme = true });
        }

        public void SetupWindow(Window window)
        {
            if (OS.UseSystemWindowFrame)
            {
                window.WindowDecorations = WindowDecorations.BorderOnly;
                window.ExtendClientAreaToDecorationsHint = false;
            }
            else
            {
                window.WindowDecorations = WindowDecorations.None;
                window.ExtendClientAreaToDecorationsHint = true;
                window.Classes.Add("custom_window_frame");
            }
        }
    }
}
