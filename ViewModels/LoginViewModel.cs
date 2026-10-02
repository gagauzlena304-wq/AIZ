using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ToDoListApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private bool isRegistering = false;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private string errorMessage = "";

        public delegate void LoginSuccessHandler();
        public event LoginSuccessHandler? OnLoginSuccess;

        private readonly ToDoListApp.Services.GoogleOAuthService _oauthService;
        private readonly MainViewModel _mainViewModel;

        public LoginViewModel(ToDoListApp.Services.GoogleOAuthService oauthService, MainViewModel mainViewModel)
        {
            _oauthService = oauthService;
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private async Task GuestLoginAsync()
        {
            IsBusy = true;
            var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ToDoListApp.Services.IDataService>(App.Current.Services);
            dataService.SetUserId("guest");
            await _mainViewModel.ReloadDataAsync();
            
            // Set default guest info if empty
            if (string.IsNullOrEmpty(_mainViewModel.Settings.UserName))
            {
                _mainViewModel.Settings.UserName = "Guest";
                _mainViewModel.Settings.UserAvatarUrl = "ms-appx:///Assets/DefaultAvatar.png";
                await dataService.SaveSettingsAsync(_mainViewModel.Settings);
            }
            
            IsBusy = false;
            OnLoginSuccess?.Invoke();
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = "";
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter email and password.";
                return;
            }

            IsBusy = true;
            await Task.Delay(1000); // Simulate network

            var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ToDoListApp.Services.IDataService>(App.Current.Services);
            dataService.SetUserId(Email);
            await _mainViewModel.ReloadDataAsync();

            if (string.IsNullOrEmpty(_mainViewModel.Settings.UserName))
            {
                _mainViewModel.Settings.UserName = "Local User";
                _mainViewModel.Settings.UserEmail = Email;
                _mainViewModel.Settings.UserAvatarUrl = "ms-appx:///Assets/DefaultAvatar.png";
                await dataService.SaveSettingsAsync(_mainViewModel.Settings);
            }

            IsBusy = false;
            OnLoginSuccess?.Invoke();
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            ErrorMessage = "";
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please fill in all fields.";
                return;
            }

            IsBusy = true;
            await Task.Delay(1000); // Simulate network

            var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ToDoListApp.Services.IDataService>(App.Current.Services);
            dataService.SetUserId(Email);
            await _mainViewModel.ReloadDataAsync();

            _mainViewModel.Settings.UserName = Name;
            _mainViewModel.Settings.UserEmail = Email;
            _mainViewModel.Settings.UserAvatarUrl = "ms-appx:///Assets/DefaultAvatar.png";
            await dataService.SaveSettingsAsync(_mainViewModel.Settings);

            IsBusy = false;
            OnLoginSuccess?.Invoke();
        }

        [RelayCommand]
        private async Task GoogleSignInAsync()
        {
            ErrorMessage = "";
            IsBusy = true;
            
            try 
            {
                var userInfo = await _oauthService.AuthenticateAsync();
                
                if (Microsoft.UI.Xaml.Application.Current is App app && app.MainWindow?.DispatcherQueue is Microsoft.UI.Dispatching.DispatcherQueue dq)
                {
                    dq.TryEnqueue(async () =>
                    {
                        var dataService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ToDoListApp.Services.IDataService>(App.Current.Services);
                        dataService.SetUserId(userInfo.email);
                        await _mainViewModel.ReloadDataAsync();

                        _mainViewModel.Settings.UserName = userInfo.name;
                        _mainViewModel.Settings.UserEmail = userInfo.email;
                        if (!string.IsNullOrEmpty(userInfo.picture)) 
                        {
                            _mainViewModel.Settings.UserAvatarUrl = userInfo.picture;
                        }
                        await dataService.SaveSettingsAsync(_mainViewModel.Settings);

                        OnLoginSuccess?.Invoke();
                        IsBusy = false;
                    });
                }
            }
            catch (System.Exception ex)
            {
                if (Microsoft.UI.Xaml.Application.Current is App app && app.MainWindow?.DispatcherQueue is Microsoft.UI.Dispatching.DispatcherQueue dq)
                {
                    dq.TryEnqueue(() => 
                    {
                        ErrorMessage = "Google Sign-In failed: " + ex.Message;
                        IsBusy = false;
                    });
                }
            }
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsRegistering = !IsRegistering;
            ErrorMessage = "";
        }
    }
}
