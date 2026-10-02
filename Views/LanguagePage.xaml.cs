using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ToDoListApp.ViewModels;
using ToDoListApp.Services;

namespace ToDoListApp.Views
{
    public sealed partial class LanguagePage : Page
    {
        public MainViewModel ViewModel { get; }

        public LanguagePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<MainViewModel>();

            this.Loaded += (s, e) => ApplyLocalization();
            ViewModel.Localizer.PropertyChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (TitleText != null) TitleText.Text = ViewModel.Localizer["TitleLanguage"];
            if (LanguageComboBox != null) LanguageComboBox.Header = ViewModel.Localizer["LblDisplayLanguage"];
        }

        private void LanguageComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var currentLang = ViewModel.Localizer.CurrentLanguage;
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag?.ToString() == currentLang)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                var newLang = item.Tag.ToString();
                if (newLang != null && ViewModel.Localizer.CurrentLanguage != newLang)
                {
                    ViewModel.Localizer.CurrentLanguage = newLang;
                    ViewModel.Settings.Language = newLang;
                    
                    var dataService = App.Current.Services.GetRequiredService<IDataService>();
                    dataService.SaveSettingsAsync(ViewModel.Settings).ConfigureAwait(false);
                }
            }
        }
    }
}
