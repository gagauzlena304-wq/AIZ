using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToDoListApp.Models;
using ToDoListApp.Services;

namespace ToDoListApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private UserSettings settings = new();

        public string UserName
        {
            get => Settings.UserName;
            set { Settings.UserName = value; OnPropertyChanged(); }
        }

        public string UserEmail
        {
            get => Settings.UserEmail;
            set { Settings.UserEmail = value; OnPropertyChanged(); }
        }

        public string UserAvatarUrl
        {
            get => Settings.UserAvatarUrl;
            set { Settings.UserAvatarUrl = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TaskItem> Tasks { get; } = new();

        [ObservableProperty]
        private bool isMenuOpen;

        public ToDoListApp.Services.LocalizationService Localizer => ToDoListApp.Services.LocalizationService.Instance;

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            ReloadDataAsync().ConfigureAwait(false);
        }

        public async Task ReloadDataAsync()
        {
            Tasks.Clear();
            Settings = await _dataService.LoadSettingsAsync();
            
            if (string.IsNullOrEmpty(Settings.Language))
                Settings.Language = "en"; // Default
            Localizer.CurrentLanguage = Settings.Language;
            
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(UserEmail));
            OnPropertyChanged(nameof(UserAvatarUrl));
            
            // Apply saved theme
            if (Microsoft.UI.Xaml.Application.Current is App app && app.MainWindow?.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
            {
                rootElement.DispatcherQueue.TryEnqueue(() =>
                {
                    if (Settings.Theme == "Light") rootElement.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
                    else if (Settings.Theme == "Dark") rootElement.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark;
                    else rootElement.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Default;
                });
            }

            var loadedTasks = (await _dataService.LoadTasksAsync()).OrderBy(t => t.IsCompleted).ToList();
            if (Microsoft.UI.Xaml.Application.Current is App app2 && app2.MainWindow?.DispatcherQueue is Microsoft.UI.Dispatching.DispatcherQueue dq)
            {
                dq.TryEnqueue(() =>
                {
                    foreach (var task in loadedTasks)
                    {
                        task.PropertyChanged += OnTaskPropertyChanged;
                        Tasks.Add(task);
                    }
                });
            }
            else
            {
                foreach (var task in loadedTasks)
                {
                    task.PropertyChanged += OnTaskPropertyChanged;
                    Tasks.Add(task);
                }
            }
        }

        private void OnTaskPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskItem.IsCompleted) && sender is TaskItem task)
            {
                if (task.IsCompleted)
                {
                    Settings.TotalTasksCompleted++;
                }
                else if (Settings.TotalTasksCompleted > 0)
                {
                    Settings.TotalTasksCompleted--;
                }
                _dataService.SaveSettingsAsync(Settings).ConfigureAwait(false);
            }
            // Always save tasks on any change
            _dataService.SaveTasksAsync(Tasks.ToList()).ConfigureAwait(false);
        }

        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        public async Task SaveTasksAsync()
        {
            await _dataService.SaveTasksAsync(Tasks.ToList());
        }
        
        internal void AddNewTask(TaskItem newTask)
        {
            newTask.PropertyChanged += OnTaskPropertyChanged;
            int firstCompleted = -1;
            for (int i = 0; i < Tasks.Count; i++)
            {
                if (Tasks[i].IsCompleted)
                {
                    firstCompleted = i;
                    break;
                }
            }
            if (firstCompleted >= 0)
            {
                Tasks.Insert(firstCompleted, newTask);
            }
            else
            {
                Tasks.Add(newTask);
            }
            _ = SaveTasksAsync();
        }

        internal void DeleteTask(TaskItem task)
        {
            if (Tasks.Contains(task))
            {
                task.PropertyChanged -= OnTaskPropertyChanged;
                if (task.IsCompleted && Settings.TotalTasksCompleted > 0)
                {
                    Settings.TotalTasksCompleted--;
                    _ = _dataService.SaveSettingsAsync(Settings);
                }
                Tasks.Remove(task);
                _ = SaveTasksAsync();
            }
        }
    }
}
