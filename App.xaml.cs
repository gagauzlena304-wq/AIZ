using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ToDoListApp.Services;
using ToDoListApp.ViewModels;

namespace ToDoListApp
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        
        public IServiceProvider Services { get; }

        public Window MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += (s, e) => 
            {
                try
                {
                    var logDirectory = System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                        "ToDoListApp");
                    System.IO.Directory.CreateDirectory(logDirectory);
                    System.IO.File.WriteAllText(System.IO.Path.Combine(logDirectory, "crash.txt"), e.Exception.ToString());
                }
                catch { }
                e.Handled = true;
            };
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDataService, JsonDataService>();
            services.AddSingleton<GoogleOAuthService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoginViewModel>();
            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MainWindow = new Window();
            MainWindow.Title = "AIZ";
            MainWindow.ExtendsContentIntoTitleBar = false;

            try
            {
                string iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    MainWindow.AppWindow.SetIcon(iconPath);
                }
            }
            catch { }

            if (MainWindow.AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }

            MainWindow.AppWindow.Changed += (s, args) =>
            {
                PomodoroService.Instance.CheckWindowMinimizeState();
            };

            if (MainWindow.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                MainWindow.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(ToDoListApp.Views.LoginPage), e.Arguments);
            MainWindow.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
