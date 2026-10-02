using Microsoft.UI.Xaml.Controls;
using ToDoListApp.Services;

namespace ToDoListApp.Views
{
    public sealed partial class DateTimePage : Page
    {
        public DateTimePage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) => ApplyLocalization();
            LocalizationService.Instance.PropertyChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            var loc = LocalizationService.Instance;
            if (TitleText != null) TitleText.Text = loc["TitleDateTime"];

            if (Format24Toggle != null)
            {
                Format24Toggle.Header = loc["Use24HourFormat"];
                Format24Toggle.OnContent = loc["ToggleOn"];
                Format24Toggle.OffContent = loc["ToggleOff"];
            }

            if (FirstDayCombo != null)
            {
                FirstDayCombo.Header = loc["FirstDayOfWeek"];
            }

            if (FirstDayMondayItem != null)
            {
                FirstDayMondayItem.Content = loc["DayMonday"];
            }

            if (FirstDaySundayItem != null)
            {
                FirstDaySundayItem.Content = loc["DaySunday"];
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
