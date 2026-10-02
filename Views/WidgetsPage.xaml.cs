using Microsoft.UI.Xaml.Controls;
using ToDoListApp.Services;

namespace ToDoListApp.Views
{
    public sealed partial class WidgetsPage : Page
    {
        public WidgetsPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) => ApplyLocalization();
            LocalizationService.Instance.PropertyChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            var loc = LocalizationService.Instance;
            if (TitleText != null) TitleText.Text = loc["TitleWidgets"];
            
            if (PomodoroWidgetToggle != null)
            {
                PomodoroWidgetToggle.Header = loc["EnablePomodoroWidget"];
                PomodoroWidgetToggle.OnContent = loc["ToggleOn"];
                PomodoroWidgetToggle.OffContent = loc["ToggleOff"];
            }

            if (CalendarWidgetToggle != null)
            {
                CalendarWidgetToggle.Header = loc["EnableCalendarWidget"];
                CalendarWidgetToggle.OnContent = loc["ToggleOn"];
                CalendarWidgetToggle.OffContent = loc["ToggleOff"];
            }
        }
    }
}
