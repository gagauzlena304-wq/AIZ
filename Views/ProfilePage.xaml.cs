using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ToDoListApp.ViewModels;

namespace ToDoListApp.Views
{
    public sealed partial class ProfilePage : Page
    {
        public MainViewModel MainViewModel { get; }
        private ToDoListApp.Services.LocalizationService _loc => ToDoListApp.Services.LocalizationService.Instance;

        public ProfilePage()
        {
            this.InitializeComponent();
            MainViewModel = App.Current.Services.GetService(typeof(MainViewModel)) as MainViewModel;

            this.Loaded += async (s, e) => 
            {
                await LoadAvatarAsync(MainViewModel.UserAvatarUrl);
                ApplyLocalization();
            };
            _loc.PropertyChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (TitleText != null) TitleText.Text = _loc["TitleProfile"];
            if (TxtTasksCompleted != null) TxtTasksCompleted.Text = _loc["LblTotalTasks"];
            if (TxtPomodoros != null) TxtPomodoros.Text = _loc["LblTotalPomodoros"];
            if (EditProfileBtn != null)
            {
                EditProfileBtn.Content = (NameEdit?.Visibility == Microsoft.UI.Xaml.Visibility.Visible)
                    ? _loc["BtnSaveProfile"]
                    : _loc["BtnEditProfile"];
            }
        }
        
        private async System.Threading.Tasks.Task LoadAvatarAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                bitmap.DecodePixelWidth = 256;
                bitmap.DecodePixelHeight = 256;

                if (url.StartsWith("ms-appdata:///local/"))
                {
                    string fileName = url.Replace("ms-appdata:///local/", "");
                    var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                    using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    await bitmap.SetSourceAsync(stream);
                }
                else if (System.IO.File.Exists(url))
                {
                    var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(url);
                    using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    await bitmap.SetSourceAsync(stream);
                }
                else
                {
                    AvatarBrush.ImageSource = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri(url));
                    return;
                }
                
                AvatarBrush.ImageSource = bitmap;
            }
            catch
            {
                AvatarBrush.ImageSource = null;
            }
        }

        private void EditProfile_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (NameEdit.Visibility == Microsoft.UI.Xaml.Visibility.Collapsed)
            {
                NameText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                NameEdit.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                ChangeAvatarBtn.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                EditProfileBtn.Content = _loc["BtnSaveProfile"];
            }
            else
            {
                NameText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                NameEdit.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                ChangeAvatarBtn.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                EditProfileBtn.Content = _loc["BtnEditProfile"];
                
                Bindings.Update();
                var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ToDoListApp.Services.IDataService>(App.Current.Services);
                dataService.SaveSettingsAsync(MainViewModel.Settings).ConfigureAwait(false);
            }
        }

        private async void ChangeAvatar_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

                MainViewModel.UserAvatarUrl = destinationPath;
                await LoadAvatarAsync(destinationPath);
                Bindings.Update();
                
                var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ToDoListApp.Services.IDataService>(App.Current.Services);
                dataService.SaveSettingsAsync(MainViewModel.Settings).ConfigureAwait(false);
            }
        }
    }
}
