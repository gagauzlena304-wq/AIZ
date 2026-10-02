using Microsoft.UI.Xaml.Controls;
using ToDoListApp.Services;

namespace ToDoListApp.Views
{
    public sealed partial class NotificationsPage : Page
    {
        public NotificationsPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) => ApplyLocalization();
            LocalizationService.Instance.PropertyChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            var loc = LocalizationService.Instance;
            if (TitleText != null) TitleText.Text = loc["TitleNotifications"];

            if (TaskRemindersToggle != null)
            {
                TaskRemindersToggle.Header = loc["TaskReminders"];
                TaskRemindersToggle.OnContent = loc["ToggleOn"];
                TaskRemindersToggle.OffContent = loc["ToggleOff"];
            }

            if (PomodoroAlertsToggle != null)
            {
                PomodoroAlertsToggle.Header = loc["PomodoroAlerts"];
                PomodoroAlertsToggle.OnContent = loc["ToggleOn"];
                PomodoroAlertsToggle.OffContent = loc["ToggleOff"];
            }
        }

        private void Toggle_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Grid grid && grid.Children.Count > 0 && grid.Children[0] is Microsoft.UI.Xaml.Controls.ToggleSwitch toggle)
            {
                toggle.IsOn = !toggle.IsOn;
            }
        }
    }
}
