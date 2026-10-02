using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ToDoListApp.Models;
using ToDoListApp.ViewModels;
using ToDoListApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ToDoListApp.Views
{
    public sealed partial class TaskDetailPage : Page
    {
        private LocalizationService _loc => LocalizationService.Instance;

        public TaskItem Task { get; private set; }

        public TaskDetailPage()
        {
            this.InitializeComponent();
            var vm = App.Current.Services.GetRequiredService<MainViewModel>();
            PomodoroService.Instance.InitializeFromSettings(vm.Settings);

            NumWorkMin.Value = PomodoroService.Instance.WorkDurationMinutes;
            NumBreakMin.Value = PomodoroService.Instance.BreakDurationMinutes;

            PomodoroService.Instance.Tick += OnPomodoroTick;
            PomodoroService.Instance.StateChanged += OnPomodoroStateChanged;

            _loc.PropertyChanged += (s, e) => ApplyLocalization();
            UpdatePomodoroVisuals();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is TaskItem task)
            {
                Task = task;
                Bindings.Update();
                if (DatePickerCtrl != null)
                {
                    DatePickerCtrl.Date = task.Date;
                }
            }
            ApplyLocalization();
            UpdatePomodoroVisuals();
        }

        private void ApplyLocalization()
        {
            if (BtnBack != null) BtnBack.Content = _loc["BtnBack"];
            if (TxtSubtasks != null) TxtSubtasks.Text = _loc["LblSubtasks"];
            if (BtnAddSubtask != null) BtnAddSubtask.Content = _loc["BtnAddSubtask"];
            if (TxtSettings != null) TxtSettings.Text = _loc["LblSettings"];
            if (DatePickerCtrl != null) DatePickerCtrl.Header = _loc["LblDate"];
            if (TimePickerCtrl != null) TimePickerCtrl.Header = _loc["LblTime"];
            if (ReminderPickerCtrl != null) ReminderPickerCtrl.Header = _loc["LblReminder"];
            if (RepeatComboCtrl != null) RepeatComboCtrl.Header = _loc["LblRepeat"];
            if (RepeatItemNone != null) RepeatItemNone.Content = _loc["RepeatNone"];
            if (RepeatItemDaily != null) RepeatItemDaily.Content = _loc["RepeatDaily"];
            if (RepeatItemWeekly != null) RepeatItemWeekly.Content = _loc["RepeatWeekly"];
            if (RepeatItemMonthly != null) RepeatItemMonthly.Content = _loc["RepeatMonthly"];
            if (TxtNotes != null) TxtNotes.Text = _loc["LblNotes"];
            if (TxtPomodoro != null) TxtPomodoro.Text = _loc["LblPomodoro"];
            if (BtnStart != null) BtnStart.Content = _loc["BtnStart"];
            if (BtnPause != null) BtnPause.Content = _loc["BtnPause"];
            if (BtnStop != null) BtnStop.Content = _loc["BtnStop"];
            if (TxtAttachments != null) TxtAttachments.Text = _loc["LblAttachments"];
            if (BtnAddAttachment != null) BtnAddAttachment.Content = _loc["BtnAddAttachment"];

            if (TaskTitleBox != null) TaskTitleBox.PlaceholderText = _loc["PlaceholderTaskTitle"];
            if (TxtTimerSettings != null) TxtTimerSettings.Text = _loc["TimerSettings"];
            if (NumWorkMin != null) NumWorkMin.Header = _loc["WorkDurationMin"];
            if (NumBreakMin != null) NumBreakMin.Header = _loc["BreakDurationMin"];
            if (TxtPresets != null) TxtPresets.Text = _loc["Presets"];
            if (BtnSavePomodoro != null) BtnSavePomodoro.Content = _loc["SaveAndApply"];
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PomodoroService.Instance.Tick -= OnPomodoroTick;
            PomodoroService.Instance.StateChanged -= OnPomodoroStateChanged;

            if (Task != null && DatePickerCtrl != null)
            {
                var d = DatePickerCtrl.Date;
                Task.Date = new DateTimeOffset(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
            }

            var vm = App.Current.Services.GetRequiredService<MainViewModel>();
            if (Task != null && !vm.Tasks.Contains(Task))
            {
                if (!string.IsNullOrWhiteSpace(Task.Title))
                {
                    vm.AddNewTask(Task);
                }
            }
            vm.SaveTasksAsync().ConfigureAwait(false);
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void AddSubtask_Click(object sender, RoutedEventArgs e)
        {
            Task?.Subtasks.Add(new SubtaskItem { Title = "" });
        }

        private void DeleteSubtask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SubtaskItem subtask)
            {
                Task?.Subtasks.Remove(subtask);
            }
        }

        private void StartPomodoro_Click(object sender, RoutedEventArgs e)
        {
            PomodoroService.Instance.Start();
        }

        private void PausePomodoro_Click(object sender, RoutedEventArgs e)
        {
            PomodoroService.Instance.Pause();
        }

        private void StopPomodoro_Click(object sender, RoutedEventArgs e)
        {
            PomodoroService.Instance.Stop();
        }

        private void TogglePomodoroMode_Click(object sender, RoutedEventArgs e)
        {
            PomodoroService.Instance.ToggleMode();
        }

        private void FloatPomodoro_Click(object sender, RoutedEventArgs e)
        {
            PomodoroService.Instance.ShowMiniWindow();
        }

        private void Preset25_Click(object sender, RoutedEventArgs e)
        {
            NumWorkMin.Value = 25;
            NumBreakMin.Value = 5;
            SavePomodoroSettings();
        }

        private void Preset45_Click(object sender, RoutedEventArgs e)
        {
            NumWorkMin.Value = 45;
            NumBreakMin.Value = 10;
            SavePomodoroSettings();
        }

        private void Preset50_Click(object sender, RoutedEventArgs e)
        {
            NumWorkMin.Value = 50;
            NumBreakMin.Value = 10;
            SavePomodoroSettings();
        }

        private void SavePomodoroSettings_Click(object sender, RoutedEventArgs e)
        {
            SavePomodoroSettings();
            BtnPomodoroSettings.Flyout?.Hide();
        }

        private void SavePomodoroSettings()
        {
            int work = (int)Math.Max(1, NumWorkMin.Value);
            int brk = (int)Math.Max(1, NumBreakMin.Value);
            PomodoroService.Instance.SetDurations(work, brk);

            var vm = App.Current.Services.GetRequiredService<MainViewModel>();
            vm.Settings.PomodoroDurationMinutes = work;
            vm.Settings.BreakDurationMinutes = brk;
            var dataService = App.Current.Services.GetRequiredService<IDataService>();
            dataService.SaveSettingsAsync(vm.Settings).ConfigureAwait(false);
        }

        private void OnPomodoroTick()
        {
            this.DispatcherQueue?.TryEnqueue(UpdatePomodoroVisuals);
        }

        private void OnPomodoroStateChanged()
        {
            this.DispatcherQueue?.TryEnqueue(UpdatePomodoroVisuals);
        }

        private void UpdatePomodoroVisuals()
        {
            var s = PomodoroService.Instance;
            PomodoroTimeText.Text = s.TimeFormatted;

            if (s.IsBreak)
            {
                // Green for rest/break
                PomodoroCard.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(235, 30, 110, 40)); // #1E6E28
                PomodoroCard.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 102, 187, 106)); // #66BB6A
                PomodoroModeText.Text = _loc["PomodoroBreak"];
            }
            else
            {
                // Red for work time
                PomodoroCard.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(235, 185, 28, 28)); // #B91C1C
                PomodoroCard.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 83, 80)); // #EF5350
                PomodoroModeText.Text = _loc["PomodoroWork"];
            }

            BtnStart.IsEnabled = !s.IsRunning;
            BtnPause.IsEnabled = s.IsRunning;
        }

        private void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            Task?.Attachments.Add(new AttachmentItem { FileName = "New Attachment.txt" });
        }
    }
}
