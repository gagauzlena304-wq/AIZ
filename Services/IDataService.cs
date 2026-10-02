using System.Collections.Generic;
using System.Threading.Tasks;
using ToDoListApp.Models;

namespace ToDoListApp.Services
{
    public interface IDataService
    {
        void SetUserId(string userId);
        Task<List<TaskItem>> LoadTasksAsync();
        Task SaveTasksAsync(List<TaskItem> tasks);
        
        Task<UserSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(UserSettings settings);
        
        Task<List<string>> LoadAchievementsAsync();
        Task SaveAchievementsAsync(List<string> achievements);
    }
}
