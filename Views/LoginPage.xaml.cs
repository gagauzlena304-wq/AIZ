using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ToDoListApp.Services;
using ToDoListApp.ViewModels;

namespace ToDoListApp.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }
        public MainViewModel MainViewModel { get; }

        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
            MainViewModel = App.Current.Services.GetRequiredService<MainViewModel>();

            ViewModel.OnLoginSuccess += () => 
            {
                Frame.Navigate(typeof(MainPage));
            };

            this.Loaded += (s, e) => ApplyLocalization();
            LocalizationService.Instance.PropertyChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            var loc = LocalizationService.Instance;
            if (TxtWelcomeBack != null) TxtWelcomeBack.Text = loc["LblWelcomeBack"];
            if (TxtCreateAccount != null) TxtCreateAccount.Text = loc["LblCreateAccount"];
            if (BtnLogIn != null) BtnLogIn.Content = loc["BtnLogIn"];
            if (BtnRegister != null) BtnRegister.Content = loc["BtnRegister"];
            if (TxtOr != null) TxtOr.Text = loc["LblOr"];
            if (TxtGoogleSignIn != null) TxtGoogleSignIn.Text = loc["BtnContinueGoogle"];
            if (TxtGuestLogin != null) TxtGuestLogin.Text = loc["BtnContinueGuest"];
            if (LinkSignUp != null) LinkSignUp.Content = loc["LinkNoAccount"];
            if (LinkSignIn != null) LinkSignIn.Content = loc["LinkHaveAccount"];
        }
    }
}
