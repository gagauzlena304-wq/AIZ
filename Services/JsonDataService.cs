using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ToDoListApp.Models;

namespace ToDoListApp.Services
{
    public class JsonDataService : IDataService
    {
        private readonly string _appDataFolder;
        private string _tasksFile = "";
        private string _settingsFile = "";
        private string _achievementsFile = "";

        public JsonDataService()
        {
            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToDoListApp");
            Directory.CreateDirectory(_appDataFolder);
            
            MigrateOldData();
            
            SetUserId("guest");
        }

        private void MigrateOldData()
        {
            string oldSettings = Path.Combine(_appDataFolder, "settings.json");
            if (File.Exists(oldSettings))
            {
                try
                {
                    var json = File.ReadAllText(oldSettings);
                    var settings = JsonSerializer.Deserialize<UserSettings>(json);
                    
                    string targetId = "guest";
                    if (settings != null && !string.IsNullOrWhiteSpace(settings.UserEmail))
                    {
                        targetId = settings.UserEmail;
                    }
                    
                    string safeId = string.Join("_", targetId.Split(Path.GetInvalidFileNameChars()));
                    string userFolder = Path.Combine(_appDataFolder, "users", safeId);
                    Directory.CreateDirectory(userFolder);
                    
                    File.Move(oldSettings, Path.Combine(userFolder, "settings.json"), true);
                    
                    string oldTasks = Path.Combine(_appDataFolder, "tasks.json");
                    if (File.Exists(oldTasks)) File.Move(oldTasks, Path.Combine(userFolder, "tasks.json"), true);
                    
                    string oldAchievements = Path.Combine(_appDataFolder, "achievements.json");
                    if (File.Exists(oldAchievements)) File.Move(oldAchievements, Path.Combine(userFolder, "achievements.json"), true);
                }
                catch { }
            }
        }

        public void SetUserId(string userId)
        {
            string safeId = string.Join("_", userId.Split(Path.GetInvalidFileNameChars()));
            string userFolder = Path.Combine(_appDataFolder, "users", safeId);
            Directory.CreateDirectory(userFolder);
            
            _tasksFile = Path.Combine(userFolder, "tasks.json");
            _settingsFile = Path.Combine(userFolder, "settings.json");
            _achievementsFile = Path.Combine(userFolder, "achievements.json");
        }

        public async Task<List<TaskItem>> LoadTasksAsync()
        {
            if (!File.Exists(_tasksFile)) return new List<TaskItem>();
            try
            {
                var json = await File.ReadAllTextAsync(_tasksFile);
                return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            catch
            {
                return new List<TaskItem>();
            }
        }

        public async Task SaveTasksAsync(List<TaskItem> tasks)
        {
            var json = JsonSerializer.Serialize(tasks);
            await File.WriteAllTextAsync(_tasksFile, json);
        }

        public async Task<UserSettings> LoadSettingsAsync()
        {
            if (!File.Exists(_settingsFile)) return new UserSettings();
            try
            {
                var json = await File.ReadAllTextAsync(_settingsFile);
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
            catch
            {
                return new UserSettings();
            }
        }

        public async Task SaveSettingsAsync(UserSettings settings)
        {
            var json = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(_settingsFile, json);
        }

        public async Task<List<string>> LoadAchievementsAsync()
        {
            if (!File.Exists(_achievementsFile)) return new List<string>();
            try
            {
                var json = await File.ReadAllTextAsync(_achievementsFile);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task SaveAchievementsAsync(List<string> achievements)
        {
            var json = JsonSerializer.Serialize(achievements);
            await File.WriteAllTextAsync(_achievementsFile, json);
        }
    }
}
