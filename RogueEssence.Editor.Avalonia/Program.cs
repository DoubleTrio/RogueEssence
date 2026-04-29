using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;

namespace RogueEssence.Dev
{
    public class Program
    {

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                File.WriteAllText("crash.log", e.ExceptionObject.ToString());
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                File.WriteAllText("crash_task.log", e.Exception.ToString());
                e.SetObserved();
            };
            Native.OS.SetupDataDir();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            var builder = AppBuilder.Configure<App>();
            builder.UsePlatformDetect();
            builder.LogToTrace();
            // builder.WithInterFont();
            builder.UseReactiveUI();
            Native.OS.SetupApp(builder);
            return builder;
        }
    }
}
