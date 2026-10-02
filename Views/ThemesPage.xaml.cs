using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ToDoListApp.Views
{
    public sealed partial class ThemesPage : Page
    {
        public ObservableCollection<ThemeOption> ThemeOptions { get; } = new();

        public ThemesPage()
        {
            this.InitializeComponent();
            LoadThemes();
            this.Loaded += (s, e) => ApplyLocalization();
            ToDoListApp.Services.LocalizationService.Instance.PropertyChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            var loc = ToDoListApp.Services.LocalizationService.Instance;
            if (TitleText != null) TitleText.Text = loc["TitleThemes"];
            if (UploadCustomPhotoText != null) UploadCustomPhotoText.Text = loc["BtnUploadPhoto"];
            if (ThemeOptions.Count >= 3)
            {
                ThemeOptions[0].Name = loc["ThemeSystemDefault"];
                ThemeOptions[1].Name = loc["ThemeLight"];
                ThemeOptions[2].Name = loc["ThemeDark"];
            }
        }

        private void LoadThemes()
        {
            ThemeOptions.Add(new ThemeOption
            {
                Name = "System Default",
                ElementTheme = ElementTheme.Default,
                WallpaperUrl = "",
                PreviewUrl = "https://images.unsplash.com/photo-1579546929518-9e396f3cc809?q=80&w=600&auto=format&fit=crop",
                OverlayBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(50, 0, 0, 0))
            });

            ThemeOptions.Add(new ThemeOption
            {
                Name = "Light Theme",
                ElementTheme = ElementTheme.Light,
                WallpaperUrl = "",
                PreviewUrl = "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?q=80&w=600&auto=format&fit=crop",
                OverlayBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(50, 255, 255, 255))
            });

            ThemeOptions.Add(new ThemeOption
            {
                Name = "Dark Theme",
                ElementTheme = ElementTheme.Dark,
                WallpaperUrl = "",
                PreviewUrl = "https://images.unsplash.com/photo-1550684848-fac1c5b4e853?q=80&w=600&auto=format&fit=crop",
                OverlayBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 0, 0, 0))
            });

            ThemeOptions.Add(new ThemeOption
            {
                Name = "Mountain Sunset",
                ElementTheme = ElementTheme.Dark,
                WallpaperUrl = "https://images.unsplash.com/photo-1506748686214-e9df14d4d9d0?q=80&w=1920&auto=format&fit=crop",
                PreviewUrl = "https://images.unsplash.com/photo-1506748686214-e9df14d4d9d0?q=80&w=600&auto=format&fit=crop"
            });

            ThemeOptions.Add(new ThemeOption
            {
                Name = "Ocean Breeze",
                ElementTheme = ElementTheme.Light,
                WallpaperUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?q=80&w=1920&auto=format&fit=crop",
                PreviewUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?q=80&w=600&auto=format&fit=crop"
            });
            
            ThemeOptions.Add(new ThemeOption
            {
                Name = "Deep Space",
                ElementTheme = ElementTheme.Dark,
                WallpaperUrl = "https://images.unsplash.com/photo-1462331940025-496dfbfc7564?q=80&w=1920&auto=format&fit=crop",
                PreviewUrl = "https://images.unsplash.com/photo-1462331940025-496dfbfc7564?q=80&w=600&auto=format&fit=crop"
            });
            ThemeOptions.Add(new ThemeOption
            {
                Name = "Neon Cyberpunk",
                ElementTheme = ElementTheme.Dark,
                WallpaperUrl = "https://images.unsplash.com/photo-1601042879364-f3947d3f9c16?q=80&w=1920&auto=format&fit=crop",
                PreviewUrl = "https://images.unsplash.com/photo-1601042879364-f3947d3f9c16?q=80&w=600&auto=format&fit=crop"
            });

            ThemeOptions.Add(new ThemeOption
            {
                Name = "Cozy Wood",
                ElementTheme = ElementTheme.Light,
                WallpaperUrl = "https://images.unsplash.com/photo-1517646287270-a5a9ca602e5c?q=80&w=1920&auto=format&fit=crop",
                PreviewUrl = "https://images.unsplash.com/photo-1517646287270-a5a9ca602e5c?q=80&w=600&auto=format&fit=crop"
            });
            
            ThemeOptions.Add(new ThemeOption
            {
                Name = "Minimalist Abstract",
                ElementTheme = ElementTheme.Light,
                WallpaperUrl = "https://images.unsplash.com/photo-1550684376-efcbd6e3f031?q=80&w=1920&auto=format&fit=crop",
                PreviewUrl = "https://images.unsplash.com/photo-1550684376-efcbd6e3f031?q=80&w=600&auto=format&fit=crop"
            });
        }

        private async void UploadCustomWallpaper_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var appDataFolder = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ToDoListApp", "Images");
                System.IO.Directory.CreateDirectory(appDataFolder);
                string uniqueName = System.Guid.NewGuid().ToString() + file.FileType;
                string destinationPath = System.IO.Path.Combine(appDataFolder, uniqueName);
                
                System.IO.File.Copy(file.Path, destinationPath, true);
                
                var customTheme = new ThemeOption
                {
                    Name = "Custom",
                    ElementTheme = App.Current.MainWindow.Content is FrameworkElement fe ? fe.RequestedTheme : ElementTheme.Default,
                    WallpaperUrl = destinationPath,
                    PreviewUrl = destinationPath
                };
                
                ThemeOptions.Insert(0, customTheme);
                
                // Select it immediately
                if (App.Current.MainWindow.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = customTheme.ElementTheme;
                }
                
                var appViewModel = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ViewModels.MainViewModel>(App.Current.Services);
                appViewModel.Settings.WallpaperPath = customTheme.WallpaperUrl;
                appViewModel.Settings.Theme = customTheme.ElementTheme == ElementTheme.Light ? "Light" : (customTheme.ElementTheme == ElementTheme.Dark ? "Dark" : "System");
                
                var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Services.IDataService>(App.Current.Services);
                dataService.SaveSettingsAsync(appViewModel.Settings).ConfigureAwait(false);
            }
        }

        private void OnThemeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ThemeOption selected)
            {
                // Apply ElementTheme to the Main Window
                if (App.Current.MainWindow.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = selected.ElementTheme;
                }

                // Apply Wallpaper via ViewModel
                var appViewModel = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ViewModels.MainViewModel>(App.Current.Services);
                appViewModel.Settings.WallpaperPath = string.IsNullOrEmpty(selected.WallpaperUrl) ? null : selected.WallpaperUrl;
                appViewModel.Settings.Theme = selected.ElementTheme == ElementTheme.Light ? "Light" : (selected.ElementTheme == ElementTheme.Dark ? "Dark" : "System");
                
                // Save settings using the DataService
                var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Services.IDataService>(App.Current.Services);
                dataService.SaveSettingsAsync(appViewModel.Settings).ConfigureAwait(false);
            }
        }
    }

    public class ThemeOption : System.ComponentModel.INotifyPropertyChanged
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
        public ElementTheme ElementTheme { get; set; }
        public string WallpaperUrl { get; set; } = string.Empty;
        public string PreviewUrl { get; set; } = string.Empty;
        public Brush OverlayBrush { get; set; } = new SolidColorBrush(Windows.UI.Color.FromArgb(0,0,0,0));

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}
