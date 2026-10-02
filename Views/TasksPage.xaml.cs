using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.ViewModels;

namespace ToDoListApp.Views
{
    public sealed partial class TasksPage : Page
    {
        public MainViewModel ViewModel { get; }
        public ObservableCollection<TaskItem> DisplayedTasks { get; } = new();
        private DateTimeOffset _selectedDay = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
        private bool _isReordering = false;

        public TasksPage()
        {
            ViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
            this.InitializeComponent();

            this.Loaded += TasksPage_Loaded;
            this.Unloaded += TasksPage_Unloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplyLocalization();
            RefreshView();
        }

        private void ApplyLocalization()
        {
            var loc = ViewModel.Localizer;
            if (TitleText != null) TitleText.Text = loc["TitleMyTasks"];
            // The selected-day label is updated in UpdateHeaderAndStats.
            if (AddTaskBtnText != null) AddTaskBtnText.Text = loc["BtnAddTask"];

            if (PrevDayBtn != null) ToolTipService.SetToolTip(PrevDayBtn, loc["TooltipPrevDay"]);
            if (NextDayBtn != null) ToolTipService.SetToolTip(NextDayBtn, loc["TooltipNextDay"]);

            if (EmptyStateTitleText != null) EmptyStateTitleText.Text = loc["NoTasksDay"];
            if (EmptyStateSubtext != null) EmptyStateSubtext.Text = loc["NoTasksDaySubtext"];
            if (EmptyStateCreateTaskBtn != null) EmptyStateCreateTaskBtn.Content = loc["BtnCreateTask"];

            if (DailyOverviewHeaderText != null) DailyOverviewHeaderText.Text = loc["DailyOverview"];
            if (ToDoHeaderText != null) ToDoHeaderText.Text = loc["StatToDo"];
            if (ToDoSubtext != null) ToDoSubtext.Text = loc["StatTasksToComplete"];
            if (CompletedHeaderText != null) CompletedHeaderText.Text = loc["StatCompleted"];
            if (CompletedSubtext != null) CompletedSubtext.Text = loc["StatTasksFinishedToday"];
            if (ProgressHeaderText != null) ProgressHeaderText.Text = loc["StatProgress"];

            if (FilterHeaderText != null) FilterHeaderText.Text = loc["ViewFilter"];
            if (FilterItemAll != null) FilterItemAll.Content = loc["FilterAll"];
            if (FilterItemPending != null) FilterItemPending.Content = loc["FilterPending"];
            if (FilterItemCompleted != null) FilterItemCompleted.Content = loc["FilterCompleted"];
        }

        private void TasksPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalization();
            ViewModel.Tasks.CollectionChanged += Tasks_CollectionChanged;
            ViewModel.Localizer.PropertyChanged += Localizer_PropertyChanged;

            RefreshView();
        }

        private void TasksPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Tasks.CollectionChanged -= Tasks_CollectionChanged;
            ViewModel.Localizer.PropertyChanged -= Localizer_PropertyChanged;
        }

        private void Localizer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyLocalization();
            RefreshView();
        }

        private void Tasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isReordering) return;
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move) return;
            RefreshView();
        }

        private void RefreshView()
        {
            if (ViewModel == null || TasksListView == null) return;

            UpdateHeaderAndStats();

            int filterMode = FilterComboBox?.SelectedIndex ?? 0;

            var dayTasks = ViewModel.Tasks.Where(t =>
                t.Date.Year == _selectedDay.Year &&
                t.Date.Month == _selectedDay.Month &&
                t.Date.Day == _selectedDay.Day);

            var result = filterMode switch
            {
                0 => dayTasks.OrderBy(t => t.IsCompleted), // All Tasks
                1 => dayTasks.Where(t => !t.IsCompleted),   // Pending Only
                2 => dayTasks.Where(t => t.IsCompleted),     // Completed Only
                _ => dayTasks.OrderBy(t => t.IsCompleted)
            };

            DisplayedTasks.Clear();
            foreach (var item in result)
            {
                DisplayedTasks.Add(item);
            }

            if (EmptyStatePanel != null && TasksListView != null)
            {
                EmptyStatePanel.Visibility = DisplayedTasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                TasksListView.Visibility = DisplayedTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateHeaderAndStats()
        {
            if (ViewModel == null) return;

            // 1. Calculate Statistics for the selected day
            var dayTasks = ViewModel.Tasks.Where(t =>
                t.Date.Year == _selectedDay.Year &&
                t.Date.Month == _selectedDay.Month &&
                t.Date.Day == _selectedDay.Day).ToList();

            int completedForDay = dayTasks.Count(t => t.IsCompleted);
            int pendingForDay = dayTasks.Count(t => !t.IsCompleted);
            int totalForDay = dayTasks.Count;

            var loc = ViewModel.Localizer;
            var culture = loc.CurrentCulture;
            var today = DateTime.Today;

            if (GoToTodayBtn != null)
            {
                GoToTodayBtn.Content = _selectedDay.Date == today
                    ? loc["DayToday"]
                    : _selectedDay.Date == today.AddDays(1)
                        ? loc["DayTomorrow"]
                        : _selectedDay.Date == today.AddDays(-1)
                            ? loc["DayYesterday"]
                            : _selectedDay.ToString(_selectedDay.Year == today.Year ? "d MMMM" : "d MMMM yyyy", culture);
                ToolTipService.SetToolTip(GoToTodayBtn, loc["BtnToday"]);
            }

            // 2. Day Header Formatting
            if (SelectedDayTitleText != null)
            {
                if (_selectedDay.Year == today.Year && _selectedDay.Month == today.Month && _selectedDay.Day == today.Day)
                {
                    SelectedDayTitleText.Text = $"{loc["DayToday"]}, {_selectedDay.ToString("MMMM d", culture)}";
                }
                else if (_selectedDay.Date == today.AddDays(1))
                {
                    SelectedDayTitleText.Text = $"{loc["DayTomorrow"]}, {_selectedDay.ToString("MMMM d", culture)}";
                }
                else if (_selectedDay.Date == today.AddDays(-1))
                {
                    SelectedDayTitleText.Text = $"{loc["DayYesterday"]}, {_selectedDay.ToString("MMMM d", culture)}";
                }
                else
                {
                    SelectedDayTitleText.Text = _selectedDay.ToString("dddd, MMMM d", culture);
                }
            }

            if (SelectedDaySummaryText != null)
            {
                SelectedDaySummaryText.Text = $"{pendingForDay} {loc["StatTasksToComplete"]} • {completedForDay} {loc["StatCompleted"]}";
            }

            // Stats in right card (Daily Overview for the selected day)
            int displayPending = pendingForDay;
            int displayCompleted = completedForDay;
            int displayTotal = totalForDay;
            int pct = displayTotal > 0 ? (int)Math.Round((double)displayCompleted / displayTotal * 100) : 0;

            if (StatCompletedCount != null) StatCompletedCount.Text = displayCompleted.ToString();
            if (StatPendingCount != null) StatPendingCount.Text = displayPending.ToString();

            var greenBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80));
            // Theme resources can expose either a Color or a Brush. Never cast
            // a Color directly to Brush: that made navigation to Tasks fail.
            var accentBrush = Application.Current.Resources["SystemAccentColor"] switch
            {
                Windows.UI.Color color => new SolidColorBrush(color),
                SolidColorBrush brush => brush,
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 215))
            };

            if (StatProgressBar != null)
            {
                StatProgressBar.Value = pct;
                StatProgressBar.Foreground = (pct == 100 && displayTotal > 0) ? greenBrush : accentBrush;
            }
            if (DayProgressBar != null)
            {
                DayProgressBar.Value = pct;
                DayProgressBar.Foreground = (pct == 100 && displayTotal > 0) ? greenBrush : accentBrush;
            }
            if (StatProgressPercent != null) StatProgressPercent.Text = $"{pct}%";

            if (StatMotivationalText != null)
            {
                if (displayTotal == 0)
                {
                    StatMotivationalText.Text = loc["MotivNoTasks"];
                }
                else if (pct == 100)
                {
                    StatMotivationalText.Text = loc["MotivAllDone"];
                }
                else if (pct >= 50)
                {
                    StatMotivationalText.Text = loc["MotivHalfway"];
                }
                else
                {
                    StatMotivationalText.Text = loc["MotivStart"];
                }
            }
        }

        private void PrevDay_Click(object sender, RoutedEventArgs e)
        {
            _selectedDay = _selectedDay.AddDays(-1);
            RefreshView();
        }

        private void NextDay_Click(object sender, RoutedEventArgs e)
        {
            _selectedDay = _selectedDay.AddDays(1);
            RefreshView();
        }

        private void GoToToday_Click(object sender, RoutedEventArgs e)
        {
            _selectedDay = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
            RefreshView();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshView();
        }

        private void OnTaskSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TaskItem task)
            {
                Frame.Navigate(typeof(TaskDetailPage), task);
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var draftTask = new TaskItem
            {
                Title = "",
                Date = new DateTimeOffset(_selectedDay.Year, _selectedDay.Month, _selectedDay.Day, 0, 0, 0, DateTimeOffset.Now.Offset)
            };
            Frame.Navigate(typeof(TaskDetailPage), draftTask);
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TaskItem task)
            {
                ViewModel.DeleteTask(task);
                RefreshView();
            }
        }

        private void TaskCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is TaskItem task)
            {
                ApplyStrikethrough(cb, task.IsCompleted, animate: false);
            }
        }

        private async void TaskCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is TaskItem task)
            {
                bool isCompleted = cb.IsChecked == true;
                task.IsCompleted = isCompleted;
                ApplyStrikethrough(cb, isCompleted, animate: true);
                int filterMode = FilterComboBox?.SelectedIndex ?? 0;

                if (isCompleted)
                {
                    var dayTasks = ViewModel.Tasks.Where(t =>
                        t.Date.Year == _selectedDay.Year &&
                        t.Date.Month == _selectedDay.Month &&
                        t.Date.Day == _selectedDay.Day).ToList();
                    bool allDayDone = dayTasks.Count > 0 && dayTasks.All(t => t.IsCompleted);
                    bool allOverallDone = ViewModel.Tasks.Count > 0 && ViewModel.Tasks.All(t => t.IsCompleted);
                    bool allDisplayedDone = DisplayedTasks.Count > 0 && DisplayedTasks.All(t => t.IsCompleted);

                    if (allOverallDone || allDisplayedDone || allDayDone)
                    {
                        SoundService.PlayAllTasksComplete();
                    }
                    else
                    {
                        SoundService.PlayTaskComplete();
                    }
                }

                // Update counts & progress right away
                UpdateHeaderAndStats();

                // Allow strikethrough animation (260ms) to finish cleanly before sliding the card
                await System.Threading.Tasks.Task.Delay(260);

                if (!DisplayedTasks.Contains(task)) return;

                var movingCardGrid = ((cb.Parent as FrameworkElement)?.Parent as Grid) 
                    ?? GetCardGrid(TasksListView.ContainerFromItem(task) as ListViewItem);

                if (filterMode == 1 && isCompleted)
                {
                    // In "Pending Only" filter (index 1), completed task fades out and is removed
                    if (movingCardGrid != null)
                    {
                        await AnimateFadeOut(movingCardGrid);
                    }
                    DisplayedTasks.Remove(task);
                    await ViewModel.SaveTasksAsync();
                }
                else if (filterMode == 2 && !isCompleted)
                {
                    // In "Completed Only" filter (index 2), uncompleted task fades out and is removed
                    if (movingCardGrid != null)
                    {
                        await AnimateFadeOut(movingCardGrid);
                    }
                    DisplayedTasks.Remove(task);
                    await ViewModel.SaveTasksAsync();
                }
                else
                {
                    await AnimateTaskReorder(task, isCompleted, movingCardGrid);
                }

                if (EmptyStatePanel != null && TasksListView != null)
                {
                    EmptyStatePanel.Visibility = DisplayedTasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    TasksListView.Visibility = DisplayedTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private async System.Threading.Tasks.Task AnimateFadeOut(FrameworkElement element)
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation
            {
                From = element.Opacity,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTarget(anim, element);
            Storyboard.SetTargetProperty(anim, "Opacity");
            sb.Children.Add(anim);

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            sb.Completed += (s, e) => tcs.TrySetResult(true);
            sb.Begin();
            await tcs.Task;
        }

        private async System.Threading.Tasks.Task AnimateTaskReorder(TaskItem task, bool isCompleted, Grid? movingGrid)
        {
            if (TasksListView == null || !DisplayedTasks.Contains(task)) return;

            int fromIndex = DisplayedTasks.IndexOf(task);
            if (fromIndex < 0) return;

            // Count other uncompleted tasks in DisplayedTasks to find boundary
            int uncompletedCount = 0;
            for (int i = 0; i < DisplayedTasks.Count; i++)
            {
                if (DisplayedTasks[i] != task && !DisplayedTasks[i].IsCompleted)
                {
                    uncompletedCount++;
                }
            }
            // Newly completed task moves directly under active tasks (top of completed section);
            // unchecked task moves to end of active tasks (right before completed section).
            int toIndex = uncompletedCount;

            if (fromIndex == toIndex || toIndex < 0 || toIndex >= DisplayedTasks.Count)
            {
                await ViewModel.SaveTasksAsync();
                return;
            }

            if (movingGrid == null)
            {
                movingGrid = GetCardGrid(TasksListView.ContainerFromItem(task) as ListViewItem);
            }

            double movingHeight = movingGrid != null && movingGrid.ActualHeight > 0 
                ? movingGrid.ActualHeight + 10.0 
                : 66.0;

            var storyboard = new Storyboard();
            var animatedTransforms = new System.Collections.Generic.List<TranslateTransform>();
            var duration = TimeSpan.FromMilliseconds(320);
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

            if (fromIndex < toIndex)
            {
                // Moving DOWN (e.g. marked completed)
                double distanceDown = 0;
                for (int i = fromIndex + 1; i <= toIndex; i++)
                {
                    var otherItem = DisplayedTasks[i];
                    var otherContainer = TasksListView.ContainerFromItem(otherItem) as ListViewItem;
                    var otherGrid = GetCardGrid(otherContainer);
                    double h = otherGrid != null && otherGrid.ActualHeight > 0 
                        ? otherGrid.ActualHeight + 10.0 
                        : 66.0;
                    distanceDown += h;

                    if (otherGrid != null)
                    {
                        var tt = GetOrCreateTranslate(otherGrid);
                        animatedTransforms.Add(tt);
                        var anim = new DoubleAnimation
                        {
                            From = 0,
                            To = -movingHeight,
                            Duration = duration,
                            EasingFunction = ease
                        };
                        Storyboard.SetTarget(anim, tt);
                        Storyboard.SetTargetProperty(anim, "Y");
                        storyboard.Children.Add(anim);
                    }
                }

                if (movingGrid != null)
                {
                    var tt = GetOrCreateTranslate(movingGrid);
                    animatedTransforms.Add(tt);
                    var anim = new DoubleAnimation
                    {
                        From = 0,
                        To = distanceDown,
                        Duration = duration,
                        EasingFunction = ease
                    };
                    Storyboard.SetTarget(anim, tt);
                    Storyboard.SetTargetProperty(anim, "Y");
                    storyboard.Children.Add(anim);
                }
            }
            else
            {
                // Moving UP (e.g. unmarked, going back to uncompleted)
                double distanceUp = 0;
                for (int i = toIndex; i < fromIndex; i++)
                {
                    var otherItem = DisplayedTasks[i];
                    var otherContainer = TasksListView.ContainerFromItem(otherItem) as ListViewItem;
                    var otherGrid = GetCardGrid(otherContainer);
                    double h = otherGrid != null && otherGrid.ActualHeight > 0 
                        ? otherGrid.ActualHeight + 10.0 
                        : 66.0;
                    distanceUp += h;

                    if (otherGrid != null)
                    {
                        var tt = GetOrCreateTranslate(otherGrid);
                        animatedTransforms.Add(tt);
                        var anim = new DoubleAnimation
                        {
                            From = 0,
                            To = movingHeight,
                            Duration = duration,
                            EasingFunction = ease
                        };
                        Storyboard.SetTarget(anim, tt);
                        Storyboard.SetTargetProperty(anim, "Y");
                        storyboard.Children.Add(anim);
                    }
                }

                if (movingGrid != null)
                {
                    var tt = GetOrCreateTranslate(movingGrid);
                    animatedTransforms.Add(tt);
                    var anim = new DoubleAnimation
                    {
                        From = 0,
                        To = -distanceUp,
                        Duration = duration,
                        EasingFunction = ease
                    };
                    Storyboard.SetTarget(anim, tt);
                    Storyboard.SetTargetProperty(anim, "Y");
                    storyboard.Children.Add(anim);
                }
            }

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            storyboard.Completed += (s, e) => tcs.TrySetResult(true);
            storyboard.Begin();
            await tcs.Task;

            foreach (var tt in animatedTransforms)
            {
                tt.Y = 0;
            }

            _isReordering = true;
            try
            {
                if (fromIndex >= 0 && fromIndex < DisplayedTasks.Count &&
                    toIndex >= 0 && toIndex < DisplayedTasks.Count &&
                    fromIndex != toIndex)
                {
                    DisplayedTasks.Move(fromIndex, toIndex);
                }

                int vmFrom = ViewModel.Tasks.IndexOf(task);
                if (vmFrom >= 0)
                {
                    int vmUncompletedCount = 0;
                    for (int i = 0; i < ViewModel.Tasks.Count; i++)
                    {
                        if (ViewModel.Tasks[i] != task && !ViewModel.Tasks[i].IsCompleted)
                        {
                            vmUncompletedCount++;
                        }
                    }
                    int vmTo = vmUncompletedCount;

                    if (vmFrom != vmTo && vmTo >= 0 && vmTo < ViewModel.Tasks.Count)
                    {
                        ViewModel.Tasks.Move(vmFrom, vmTo);
                    }
                }
            }
            finally
            {
                _isReordering = false;
            }

            await ViewModel.SaveTasksAsync();
        }

        private TranslateTransform GetOrCreateTranslate(UIElement element)
        {
            if (element.RenderTransform is TranslateTransform tt) return tt;
            var newTt = new TranslateTransform();
            element.RenderTransform = newTt;
            return newTt;
        }

        private Grid? GetCardGrid(ListViewItem? container)
        {
            if (container == null) return null;
            if (container.ContentTemplateRoot is Grid g) return g;
            return FindVisualChild<Grid>(container);
        }

        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;
                var sub = FindVisualChild<T>(child);
                if (sub != null) return sub;
            }
            return null;
        }

        private void SubtaskCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is SubtaskItem subtask)
            {
                ApplyStrikethrough(cb, subtask.IsCompleted, animate: false);
            }
        }

        private void SubtaskCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is SubtaskItem subtask)
            {
                bool isCompleted = cb.IsChecked == true;
                ApplyStrikethrough(cb, isCompleted, animate: true);
                if (isCompleted)
                {
                    SoundService.PlayTaskComplete();
                }
            }
        }

        private void TaskCard_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender is Grid cardGrid && args.NewValue is TaskItem task)
            {
                ApplyStrikethroughToCard(cardGrid, task.IsCompleted, animate: false);
            }
        }

        private void SubtaskRow_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender is Grid subtaskGrid && args.NewValue is SubtaskItem subtask)
            {
                ApplyStrikethroughToSubtask(subtaskGrid, subtask.IsCompleted, animate: false);
            }
        }

        private void ApplyStrikethroughToCard(Grid? cardGrid, bool isCompleted, bool animate)
        {
            if (cardGrid == null) return;
            if (cardGrid.Children.Count > 0 && cardGrid.Children[0] is Grid rowGrid && rowGrid.Children.Count > 1)
            {
                if (rowGrid.Children[1] is Grid titleContainer && titleContainer.Children.Count > 1)
                {
                    var textBlock = titleContainer.Children[0] as TextBlock;
                    var strikeBorder = titleContainer.Children[1] as Border;
                    if (textBlock != null && strikeBorder != null)
                    {
                        ApplyStrikethroughCore(textBlock, strikeBorder, isCompleted, animate);
                    }
                }
            }
        }

        private void ApplyStrikethroughToSubtask(Grid? subtaskGrid, bool isCompleted, bool animate)
        {
            if (subtaskGrid == null || subtaskGrid.Children.Count < 2) return;
            if (subtaskGrid.Children[1] is Grid titleContainer && titleContainer.Children.Count > 1)
            {
                var textBlock = titleContainer.Children[0] as TextBlock;
                var strikeBorder = titleContainer.Children[1] as Border;
                if (textBlock != null && strikeBorder != null)
                {
                    ApplyStrikethroughCore(textBlock, strikeBorder, isCompleted, animate);
                }
            }
        }

        private void ApplyStrikethrough(CheckBox cb, bool isCompleted, bool animate)
        {
            if (cb.Parent is not Grid rowGrid || rowGrid.Children.Count < 2) return;
            if (rowGrid.Children[1] is not Grid titleContainer || titleContainer.Children.Count < 2) return;
            var textBlock = titleContainer.Children[0] as TextBlock;
            var strikeBorder = titleContainer.Children[1] as Border;
            if (textBlock != null && strikeBorder != null)
            {
                ApplyStrikethroughCore(textBlock, strikeBorder, isCompleted, animate);
            }
        }

        private void ApplyStrikethroughCore(TextBlock textBlock, Border strikeBorder, bool isCompleted, bool animate)
        {
            if (strikeBorder.RenderTransform is not ScaleTransform scaleTransform)
            {
                scaleTransform = new ScaleTransform { ScaleX = isCompleted ? 1.0 : 0.0 };
                strikeBorder.RenderTransform = scaleTransform;
            }

            if (!animate)
            {
                scaleTransform.ScaleX = isCompleted ? 1.0 : 0.0;
                textBlock.Opacity = isCompleted ? 0.55 : 1.0;
                return;
            }

            var sb = new Storyboard();
            var scaleAnim = new DoubleAnimation
            {
                To = isCompleted ? 1.0 : 0.0,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new CubicEase { EasingMode = isCompleted ? EasingMode.EaseOut : EasingMode.EaseIn }
            };
            Storyboard.SetTarget(scaleAnim, scaleTransform);
            Storyboard.SetTargetProperty(scaleAnim, "ScaleX");
            sb.Children.Add(scaleAnim);

            var opacityAnim = new DoubleAnimation
            {
                To = isCompleted ? 0.55 : 1.0,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(opacityAnim, textBlock);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");
            sb.Children.Add(opacityAnim);

            sb.Begin();
        }
    }
}
