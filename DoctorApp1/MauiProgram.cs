using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Microsoft.UI.Xaml;
#endif
#if MACCATALYST
using UIKit;
#endif

namespace DoctorApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                events.AddWindows(w =>
                {
                    w.OnWindowCreated(window =>
                    {
                        const int width = 1600;
                        const int height = 1000;

                        var mauiWinUIWindow = (Microsoft.UI.Xaml.Window)window;
                        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mauiWinUIWindow);
                        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                        var appWindow = AppWindow.GetFromWindowId(windowId);

                        if (appWindow is not null)
                        {
                            appWindow.Resize(new SizeInt32(width, height));
                            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);

                            // Prevent resizing
                            if (appWindow.Presenter is OverlappedPresenter presenter)
                            {
                                presenter.IsResizable = false;
                                presenter.IsMaximizable = false;
                                presenter.IsMinimizable = true;
                            }
                        }
                    });
                });
#endif
#if MACCATALYST
                events.AddiOS(w =>
                {
                    w.SceneWillConnect((scene, session, options) =>
                    {
                        if (scene is UIWindowScene windowScene)
                        {
                            var window = UIApplication.SharedApplication.Windows.FirstOrDefault();
                            if (window != null)
                            {
                                // Set constant size
                                var size = new CoreGraphics.CGSize(1000, 720);
                                windowScene.SizeRestrictions.MinimumSize = size;
                                windowScene.SizeRestrictions.MaximumSize = size;
                            }
                        }
                    });
                });
#endif
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}