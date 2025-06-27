using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using System.Runtime.InteropServices;
using DoctorApp1.Services;
using DoctorApp1.Views;

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
                fonts.AddFont("Inter-VariableFont.ttf", "InterRegular");
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

                        const int GWL_WNDPROC = -4;
                        IntPtr originalWndProc = NativeMethods.GetWindowLongPtr(hwnd, GWL_WNDPROC);

                        NativeMethods.NewWndProcDelegate = (hWnd, msg, wParam, lParam) =>
                        {
                            const int WM_NCLBUTTONDBLCLK = 0x00A3;

                            if (msg == WM_NCLBUTTONDBLCLK)
                                return IntPtr.Zero;

                            return NativeMethods.CallWindowProc(originalWndProc, hWnd, msg, wParam, lParam);
                        };

                        IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(NativeMethods.NewWndProcDelegate);
                        NativeMethods.SetWindowLongPtr(hwnd, GWL_WNDPROC, newWndProcPtr);

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
                textField.BackgroundColor = UIKit.UIColor.Clear;
                textField.Layer.BackgroundColor = UIKit.UIColor.Clear.CGColor;
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
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<AppointmentNotificationService>();
        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<AppointmentNotificationService>();
        builder.Services.AddTransient<CalendarPage>();
        return builder.Build();
    }

    internal static class NativeMethods
    {
        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        public static WndProcDelegate NewWndProcDelegate;
    }
}
