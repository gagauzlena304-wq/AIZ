using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToDoListApp.Models
{
    public partial class TaskItem : ObservableObject
    {
        [ObservableProperty]
        private string id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private DateTimeOffset date = DateTimeOffset.Now;

        [ObservableProperty]
        private TimeSpan time = TimeSpan.Zero;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private DateTimeOffset? reminder;

        [ObservableProperty]
        private string repeatSetting = "None";

        [ObservableProperty]
        private string notes = string.Empty;

        public ObservableCollection<SubtaskItem> Subtasks { get; } = new();
        public ObservableCollection<AttachmentItem> Attachments { get; } = new();
    }

    public partial class SubtaskItem : ObservableObject
    {
        [ObservableProperty]
        private string id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private bool isCompleted;
    }

    public partial class AttachmentItem : ObservableObject
    {
        [ObservableProperty]
        private string id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string fileName = string.Empty;

        [ObservableProperty]
        private string filePath = string.Empty;

        [ObservableProperty]
        private bool isPhoto;
    }
}
