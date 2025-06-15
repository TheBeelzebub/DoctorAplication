using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#endif
#if MACCATALYST
using UIKit;
#endif
#if ANDROID
using Android.Graphics.Drawables;
using Microsoft.Maui.Handlers;
#endif
#if IOS
using UIKit;
using Microsoft.Maui.Handlers;
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
                        if (scene is UIKit.UIWindowScene windowScene)
                        {
                            var window = UIKit.UIApplication.SharedApplication.Windows.FirstOrDefault();
                            if (window != null)
                            {
                                var size = new CoreGraphics.CGSize(1000, 720);
                                windowScene.SizeRestrictions.MinimumSize = size;
                                windowScene.SizeRestrictions.MaximumSize = size;
                            }
                        }
                    });
                });
#endif
            });

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoGlow", (handler, view) =>
        {
#if WINDOWS
            if (handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
            {
                textBox.UseSystemFocusVisuals = false;
                textBox.FocusVisualPrimaryBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                textBox.FocusVisualSecondaryBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                textBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                textBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                textBox.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                var noBorderStyle = new Microsoft.UI.Xaml.Style(typeof(Microsoft.UI.Xaml.Controls.TextBox));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.BorderThicknessProperty, new Microsoft.UI.Xaml.Thickness(0)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.BorderBrushProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.FocusVisualPrimaryBrushProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.FocusVisualSecondaryBrushProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.UseSystemFocusVisualsProperty, false));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));

                textBox.Style = noBorderStyle;
            }
#endif
#if ANDROID
            if (handler.PlatformView is Android.Widget.EditText editText)
            {
                editText.Background = null;
            }
#endif
#if IOS || MACCATALYST
            if (handler.PlatformView is UIKit.UITextField textField)
            {
                textField.Layer.BorderWidth = 0;
                textField.Layer.ShadowOpacity = 0;
            }
#endif
        });

        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoGlow", (handler, view) =>
        {
#if WINDOWS
            if (handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
            {
                textBox.UseSystemFocusVisuals = false;
                textBox.FocusVisualPrimaryBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                textBox.FocusVisualSecondaryBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                textBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                textBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                textBox.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                var noBorderStyle = new Microsoft.UI.Xaml.Style(typeof(Microsoft.UI.Xaml.Controls.TextBox));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.BorderThicknessProperty, new Microsoft.UI.Xaml.Thickness(0)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.BorderBrushProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.FocusVisualPrimaryBrushProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.FocusVisualSecondaryBrushProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.UseSystemFocusVisualsProperty, false));
                noBorderStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty, new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)));

                textBox.Style = noBorderStyle;
            }
#endif
#if ANDROID
            if (handler.PlatformView is Android.Widget.EditText editText)
            {
                editText.Background = null;
            }
#endif
#if IOS || MACCATALYST
            if (handler.PlatformView is UIKit.UITextView textView)
            {
                textView.Layer.BorderWidth = 0;
                textView.Layer.ShadowOpacity = 0;
            }
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
