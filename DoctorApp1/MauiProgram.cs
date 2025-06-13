using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

                // Remove shadow from all Buttons on Windows globally
                Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("NoShadow", (handler, view) =>
                {
                    if (handler.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
                    {
                        nativeButton.Shadow = null;
                    }
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

                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoGlow", (handler, view) =>
                {
#if WINDOWS
                    if (handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
                    {
                        textBox.UseSystemFocusVisuals = false;
                    }
#endif
                });
#endif
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}