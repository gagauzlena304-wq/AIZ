using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToDoListApp.Models
{
    public partial class UserSettings : ObservableObject
    {
        [ObservableProperty]
        private string? wallpaperPath = string.Empty;

        [ObservableProperty]
        private string theme = "System"; // Light, Dark, System

        [ObservableProperty]
        private string language = "English";

        [ObservableProperty]
        private int pomodoroDurationMinutes = 25;

        [ObservableProperty]
        private int breakDurationMinutes = 5;
        
        [ObservableProperty]
        private int totalTasksCompleted = 0;
        
        [ObservableProperty]
        private int totalPomodorosCompleted = 0;

        [ObservableProperty]
        private string userName = "Guest";

        [ObservableProperty]
        private string userEmail = "Not logged in";

        [ObservableProperty]
        private string userAvatarUrl = "ms-appx:///Assets/DefaultAvatar.png";
    }
}
