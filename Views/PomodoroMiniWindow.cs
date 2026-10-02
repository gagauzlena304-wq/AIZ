using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using Windows.UI;
using ToDoListApp.Services;

namespace ToDoListApp.Views
{
    public class PomodoroMiniWindow : Window
    {
        private readonly Border _rootBorder;
        private readonly TextBlock _statusBadge;
        private readonly TextBlock _timeText;
        private readonly Button _playPauseBtn;
        private readonly FontIcon _playPauseIcon;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public PomodoroMiniWindow()
        {
            this.Title = "AIZ - Pomodoro";

            var appWindow = this.AppWindow;
            appWindow.Resize(new SizeInt32(250, 135));

            // Position at bottom-right of primary display
            try
            {
                var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                if (displayArea != null)
                {
                    int x = displayArea.WorkArea.X + displayArea.WorkArea.Width - 275;
                    int y = displayArea.WorkArea.Y + displayArea.WorkArea.Height - 155;
                    appWindow.Move(new PointInt32(x, y));
                }
            }
            catch { }

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = true;
                presenter.IsResizable = false;
                presenter.IsMinimizable = false;
                presenter.IsMaximizable = false;
                presenter.SetBorderAndTitleBar(false, false);
            }

            // Build layout
            _rootBorder = new Border
            {
                CornerRadius = new CornerRadius(0),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 10, 12, 10)
            };

            // Drag window on pointer pressed
            _rootBorder.PointerPressed += (s, e) =>
            {
                try
                {
                    IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                    ReleaseCapture();
                    SendMessage(hwnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
                }
                catch { }
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Top row: Status badge + Restore button + Close button
            var topBar = new Grid();
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _statusBadge = new TextBlock
            {
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(_statusBadge, 0);
            topBar.Children.Add(_statusBadge);

            var topActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };

            // Restore / Maximize button
            var restoreBtn = new Button
            {
                Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 4, 6, 4)
            };
            ToolTipService.SetToolTip(restoreBtn, "Open AIZ");
            restoreBtn.Content = new FontIcon { Glyph = "\uE740", FontSize = 12, Foreground = new SolidColorBrush(Colors.White) };
            restoreBtn.Click += (s, e) =>
            {
                RestoreMainWindow();
            };
            topActions.Children.Add(restoreBtn);

            // Close button
            var closeBtn = new Button
            {
                Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 4, 6, 4)
            };
            ToolTipService.SetToolTip(closeBtn, "Close");
            closeBtn.Content = new FontIcon { Glyph = "\uE711", FontSize = 12, Foreground = new SolidColorBrush(Colors.White) };
            closeBtn.Click += (s, e) =>
            {
                PomodoroService.Instance.HideMiniWindow();
            };
            topActions.Children.Add(closeBtn);

            Grid.SetColumn(topActions, 1);
            topBar.Children.Add(topActions);
            Grid.SetRow(topBar, 0);
            mainGrid.Children.Add(topBar);

            // Middle row: Timer text
            _timeText = new TextBlock
            {
                FontSize = 36,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(_timeText, 1);
            mainGrid.Children.Add(_timeText);

            // Bottom row: Controls
            var controlsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 10
            };

            _playPauseIcon = new FontIcon { Glyph = "\uE768", FontSize = 13, Foreground = new SolidColorBrush(Colors.White) };
            _playPauseBtn = new Button
            {
                Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                Content = _playPauseIcon
            };
            _playPauseBtn.Click += (s, e) =>
            {
                if (PomodoroService.Instance.IsRunning)
                {
                    PomodoroService.Instance.Pause();
                }
                else
                {
                    PomodoroService.Instance.Start();
                }
            };
            controlsPanel.Children.Add(_playPauseBtn);

            var stopBtn = new Button
            {
                Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                Content = new FontIcon { Glyph = "\uE71A", FontSize = 13, Foreground = new SolidColorBrush(Colors.White) }
            };
            stopBtn.Click += (s, e) =>
            {
                PomodoroService.Instance.Stop();
            };
            controlsPanel.Children.Add(stopBtn);

            var toggleModeBtn = new Button
            {
                Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                Content = new FontIcon { Glyph = "\uE895", FontSize = 13, Foreground = new SolidColorBrush(Colors.White) }
            };
            ToolTipService.SetToolTip(toggleModeBtn, "Switch Work / Break");
            toggleModeBtn.Click += (s, e) =>
            {
                PomodoroService.Instance.ToggleMode();
            };
            controlsPanel.Children.Add(toggleModeBtn);

            Grid.SetRow(controlsPanel, 2);
            mainGrid.Children.Add(controlsPanel);

            _rootBorder.Child = mainGrid;
            this.Content = _rootBorder;

            PomodoroService.Instance.Tick += OnTick;
            PomodoroService.Instance.StateChanged += OnStateChanged;

            UpdateVisuals();
        }

        private void RestoreMainWindow()
        {
            try
            {
                if (App.Current.MainWindow != null)
                {
                    if (App.Current.MainWindow.AppWindow.Presenter is OverlappedPresenter op)
                    {
                        op.Restore();
                    }
                    App.Current.MainWindow.Activate();
                }
                PomodoroService.Instance.HideMiniWindow();
            }
            catch { }
        }

        private void OnTick()
        {
            if (this.DispatcherQueue != null)
            {
                this.DispatcherQueue.TryEnqueue(UpdateVisuals);
            }
        }

        private void OnStateChanged()
        {
            if (this.DispatcherQueue != null)
            {
                this.DispatcherQueue.TryEnqueue(UpdateVisuals);
            }
        }

        private void UpdateVisuals()
        {
            var s = PomodoroService.Instance;
            _timeText.Text = s.TimeFormatted;

            bool isBreak = s.IsBreak;
            if (isBreak)
            {
                // Green theme for break
                _rootBorder.Background = new SolidColorBrush(Color.FromArgb(235, 30, 110, 40)); // #1E6E28
                _rootBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 102, 187, 106)); // #66BB6A
                _statusBadge.Text = "☕ BREAK TIME";
            }
            else
            {
                // Red theme for work
                _rootBorder.Background = new SolidColorBrush(Color.FromArgb(235, 185, 28, 28)); // #B91C1C
                _rootBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 239, 83, 80)); // #EF5350
                _statusBadge.Text = "🔥 WORK TIME";
            }

            _playPauseIcon.Glyph = s.IsRunning ? "\uE769" : "\uE768";
        }
    }
}
