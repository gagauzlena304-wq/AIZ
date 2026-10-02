using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ToDoListApp.Models;
using ToDoListApp.Views;

namespace ToDoListApp.Services
{
    public class PomodoroService
    {
        private static PomodoroService? _instance;
        public static PomodoroService Instance => _instance ??= new PomodoroService();

        private readonly DispatcherTimer _timer;
        private int _timeRemaining;
        private bool _isBreak;
        private bool _isRunning;
        private int _workDurationMinutes = 25;
        private int _breakDurationMinutes = 5;

        private PomodoroMiniWindow? _miniWindow;
        private bool _autoOpenedMini;

        public event Action? Tick;
        public event Action? StateChanged;

        public int TimeRemaining => _timeRemaining;
        public bool IsRunning => _isRunning;
        public bool IsBreak => _isBreak;
        public int WorkDurationMinutes => _workDurationMinutes;
        public int BreakDurationMinutes => _breakDurationMinutes;

        public string TimeFormatted
        {
            get
            {
                int m = _timeRemaining / 60;
                int s = _timeRemaining % 60;
                return $"{m:D2}:{s:D2}";
            }
        }

        public string ModeTitle => _isBreak ? "Break" : "Work";

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool MessageBeep(uint uType);

        public PomodoroService()
        {
            _timeRemaining = _workDurationMinutes * 60;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        public void InitializeFromSettings(UserSettings settings)
        {
            if (settings != null)
            {
                _workDurationMinutes = Math.Max(1, settings.PomodoroDurationMinutes);
                _breakDurationMinutes = Math.Max(1, settings.BreakDurationMinutes);
                if (!_isRunning)
                {
                    _timeRemaining = (_isBreak ? _breakDurationMinutes : _workDurationMinutes) * 60;
                    StateChanged?.Invoke();
                }
            }
        }

        public void Start()
        {
            _isRunning = true;
            _timer.Start();
            StateChanged?.Invoke();
        }

        public void Pause()
        {
            _isRunning = false;
            _timer.Stop();
            StateChanged?.Invoke();
        }

        public void Stop()
        {
            _isRunning = false;
            _timer.Stop();
            _isBreak = false;
            _timeRemaining = _workDurationMinutes * 60;
            StateChanged?.Invoke();
        }

        public void ToggleMode()
        {
            _isBreak = !_isBreak;
            _timeRemaining = (_isBreak ? _breakDurationMinutes : _workDurationMinutes) * 60;
            StateChanged?.Invoke();
        }

        public void SetDurations(int workMinutes, int breakMinutes)
        {
            _workDurationMinutes = Math.Max(1, workMinutes);
            _breakDurationMinutes = Math.Max(1, breakMinutes);
            if (!_isRunning)
            {
                _timeRemaining = (_isBreak ? _breakDurationMinutes : _workDurationMinutes) * 60;
            }
            StateChanged?.Invoke();
        }

        private void Timer_Tick(object? sender, object e)
        {
            if (_timeRemaining > 0)
            {
                _timeRemaining--;
                Tick?.Invoke();
            }
            else
            {
                // Play completion sound
                try { MessageBeep(0); } catch { }

                if (!_isBreak)
                {
                    // Finished work, switch to break!
                    _isBreak = true;
                    _timeRemaining = _breakDurationMinutes * 60;

                    // Increment stats
                    try
                    {
                        var vm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ViewModels.MainViewModel>(App.Current.Services);
                        vm.Settings.TotalPomodorosCompleted++;
                        var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IDataService>(App.Current.Services);
                        dataService.SaveSettingsAsync(vm.Settings).ConfigureAwait(false);
                    }
                    catch { }
                }
                else
                {
                    // Finished break, switch back to work
                    _isBreak = false;
                    _timeRemaining = _workDurationMinutes * 60;
                }
                StateChanged?.Invoke();
            }

            CheckWindowMinimizeState();
        }

        public void CheckWindowMinimizeState()
        {
            if (App.Current.MainWindow == null) return;
            try
            {
                IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.MainWindow);
                bool minimized = IsIconic(hwnd);

                if (minimized && _isRunning)
                {
                    if (_miniWindow == null)
                    {
                        ShowMiniWindow();
                        _autoOpenedMini = true;
                    }
                }
                else if (!minimized && _autoOpenedMini)
                {
                    HideMiniWindow();
                    _autoOpenedMini = false;
                }
            }
            catch { }
        }

        public void ShowMiniWindow()
        {
            if (App.Current.MainWindow?.DispatcherQueue is DispatcherQueue dq)
            {
                dq.TryEnqueue(() =>
                {
                    if (_miniWindow == null)
                    {
                        _miniWindow = new PomodoroMiniWindow();
                        _miniWindow.Closed += (s, e) => { _miniWindow = null; };
                    }
                    _miniWindow.Activate();
                });
            }
        }

        public void HideMiniWindow()
        {
            if (App.Current.MainWindow?.DispatcherQueue is DispatcherQueue dq)
            {
                dq.TryEnqueue(() =>
                {
                    if (_miniWindow != null)
                    {
                        try
                        {
                            _miniWindow.Close();
                        }
                        catch { }
                        _miniWindow = null;
                    }
                });
            }
        }
    }
}
