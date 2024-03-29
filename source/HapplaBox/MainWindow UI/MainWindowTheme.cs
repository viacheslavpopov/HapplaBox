﻿using HapplaBox.Base;
using HapplaBox.UI;
using HapplaBox.UI.WinApi;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HapplaBox
{
    public partial class MainWindow
    {
        private const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
        private Theme CurrentTheme = new();
        private DispatcherTimer themeUpdateTimer = new();


        private void UpdateTheme()
        {
            // update current theme using system variables
            CurrentTheme.Update();

            if (this.IsActive)
            {
                MainWindowTheme_Activated(this, null);
            }
            else
            {
                MainWindowTheme_Deactivated(this, null);
            }

            if (WindowState == WindowState.Maximized)
            {
                AppArea.BorderThickness = new Thickness(0);
            }
            else
            {
                AppArea.BorderThickness = CurrentTheme.BorderWeight;
            }


            UpdateWebviewTheme();
        }

        private void UpdateWebviewTheme()
        {
            if (!IsWebviewReady) return;

            // color accent
            var color = CurrentTheme.Accent.Color;
            var accentColorStr = $"{color.R} {color.G} {color.B}";

            Web2.CoreWebView2.ExecuteScriptAsync($"document.documentElement.style.setProperty('--colorAccent', '{accentColorStr}');");
        }

        private void MainWindowTheme_Activated(object sender, EventArgs e)
        {
            var bgColor = CurrentTheme.Background.Color;
            if (!AeroGlass.IsAeroGlassEnabled)
            {
                bgColor = Color.FromArgb(255,
                    CurrentTheme.Background.Color.R,
                    CurrentTheme.Background.Color.G,
                    CurrentTheme.Background.Color.B);
            }
            AnimateWindowBackground(CurrentTheme.BackgroundInactive.Color, bgColor);
            AnimateWindowFrame(WinTitleBar.Background, CurrentTheme.TitleBar);

            WinMainBorder.BorderBrush = CurrentTheme.Border;
            WinTitleText.Foreground =
              WinBtnMinimize.Foreground =
              WinBtnMaximize.Foreground =
              WinBtnRestore.Foreground =
              WinBtnClose.Foreground = CurrentTheme.TitleBarText;

            Web2.DefaultBackgroundColor = Helpers.FromColor(CurrentTheme.Background.Color);
        }

        private void MainWindowTheme_Deactivated(object sender, EventArgs e)
        {
            var bgColor = CurrentTheme.BackgroundInactive.Color;
            if (!AeroGlass.IsAeroGlassEnabled)
            {
                bgColor = Color.FromArgb(255,
                    CurrentTheme.BackgroundInactive.Color.R,
                    CurrentTheme.BackgroundInactive.Color.G,
                    CurrentTheme.BackgroundInactive.Color.B);
            }
            AnimateWindowBackground(CurrentTheme.Background.Color, bgColor);
            AnimateWindowFrame(WinTitleBar.Background, CurrentTheme.TitleBarInactive);

            WinMainBorder.BorderBrush = CurrentTheme.BorderInactive;
            WinTitleText.Foreground =
              WinBtnMinimize.Foreground =
              WinBtnMaximize.Foreground =
              WinBtnRestore.Foreground =
              WinBtnClose.Foreground = CurrentTheme.TitleBarTextInactive;

            Web2.DefaultBackgroundColor = Helpers.FromColor(CurrentTheme.BackgroundInactive.Color);
        }

        private void MainWindowTheme_Loaded(object sender, RoutedEventArgs e)
        {
            //AeroGlass.Apply(this);
            UpdateTheme();
            UpdateWindowChrome();

            themeUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
            themeUpdateTimer.Tick += ThemeUpdateTimer_Tick;

            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private void ThemeUpdateTimer_Tick(object sender, EventArgs e)
        {
            themeUpdateTimer.Stop();
            UpdateTheme();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DWMCOLORIZATIONCOLORCHANGED)
            {
                themeUpdateTimer.Stop();
                themeUpdateTimer.Start();
            }

            return IntPtr.Zero;
        }


        private void AnimateWindowBackground(Color from, Color to)
        {
            var ca = new ColorAnimation(from, to, new(TimeSpan.FromMilliseconds(200)));

            AppArea.Background = new SolidColorBrush(from);
            AppArea.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }


        private void AnimateWindowFrame(Brush from, Brush to)
        {
            if (from is null || to is null)
            {
                WinTitleBar.Background = to;
                AppArea.BorderBrush = to;
                return;
            }

            var fromBrush = from as SolidColorBrush;
            var toBrush = to as SolidColorBrush;

            var ca = new ColorAnimation(fromBrush.Color, toBrush.Color,
                new(TimeSpan.FromMilliseconds(200)));

            WinTitleBar.Background = toBrush;
            AppArea.BorderBrush = toBrush;

            WinTitleBar.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            AppArea.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }
    }
}
