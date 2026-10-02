using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ToDoListApp.ViewModels;

namespace ToDoListApp.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
            
            // Apply localization on load and language change
            ViewModel.Localizer.PropertyChanged += (s, e) => ApplyLocalization();
            Loaded += (s, e) => {
                ContentFrame.Navigate(typeof(TasksPage));
                ApplyLocalization();
            };
        }

        private void ApplyLocalization()
        {
            var loc = ViewModel.Localizer;
            if (NavMenuText != null) NavMenuText.Text = loc["Menu"];
            if (NavTasksText != null) NavTasksText.Text = loc["MenuTasks"];
            if (NavCalendarText != null) NavCalendarText.Text = loc["MenuCalendar"];
            if (NavProfileText != null) NavProfileText.Text = loc["MenuProfile"];
            if (NavThemesBtn != null) NavThemesBtn.Content = loc["MenuThemes"];
            if (NavNotifsBtn != null) NavNotifsBtn.Content = loc["MenuNotifications"];
            if (NavDateTimeBtn != null) NavDateTimeBtn.Content = loc["MenuDateTime"];
            if (NavLanguageBtn != null) NavLanguageBtn.Content = loc["MenuLanguage"];

            if (NavMenuBtn != null) ToolTipService.SetToolTip(NavMenuBtn, loc["Menu"]);
            if (NavTasksBtn != null) ToolTipService.SetToolTip(NavTasksBtn, loc["MenuTasks"]);
            if (NavCalendarBtn != null) ToolTipService.SetToolTip(NavCalendarBtn, loc["MenuCalendar"]);
            if (NavProfileBtn != null) ToolTipService.SetToolTip(NavProfileBtn, loc["MenuProfile"]);
        }

        private void NavTasks_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CurrentSourcePageType != typeof(TasksPage))
            {
                ContentFrame.Navigate(typeof(TasksPage));
            }
        }

        private void NavCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CurrentSourcePageType != typeof(CalendarPage))
            {
                ContentFrame.Navigate(typeof(CalendarPage));
            }
        }

        private void NavProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CurrentSourcePageType != typeof(ProfilePage))
            {
                ContentFrame.Navigate(typeof(ProfilePage));
            }
        }

        private void NavThemes_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ThemesPage));
            ViewModel.IsMenuOpen = false;
        }

        private void NavNotifications_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(NotificationsPage));
            ViewModel.IsMenuOpen = false;
        }

        private void NavDateTime_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(DateTimePage));
            ViewModel.IsMenuOpen = false;
        }

        private void NavLanguage_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(LanguagePage));
            ViewModel.IsMenuOpen = false;
        }
        
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavTasksBtn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            NavCalendarBtn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            NavProfileBtn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

            var activeBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0x44, 0xFF, 0xFF, 0xFF));

            if (e.SourcePageType == typeof(TasksPage) || e.SourcePageType == typeof(TaskDetailPage))
            {
                NavTasksBtn.Background = activeBrush;
            }
            else if (e.SourcePageType == typeof(CalendarPage))
            {
                NavCalendarBtn.Background = activeBrush;
            }
            else if (e.SourcePageType == typeof(ProfilePage))
            {
                NavProfileBtn.Background = activeBrush;
            }
        }
    }
}
