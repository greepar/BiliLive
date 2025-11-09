using Avalonia;
using System;

namespace BiliLive;

static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // .WithInterFont()
            .UseSkia()
            .With(new Win32PlatformOptions
            {
                RenderingMode =
                [
                    Win32RenderingMode.AngleEgl
                ]
            })
            .With(new X11PlatformOptions()
            {
                RenderingMode = 
                    [
                    X11RenderingMode.Egl
                    ]
            })
            .With(new AvaloniaNativePlatformOptions()
            {
                RenderingMode = 
                    [
                    AvaloniaNativeRenderingMode.OpenGl
                    ]
            })
            .LogToTrace();
}