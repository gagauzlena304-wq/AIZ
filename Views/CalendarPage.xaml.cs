using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
    public sealed partial class CalendarPage : Page
    {
        public ObservableCollection<TaskItem> FilteredTasks { get; } = new();
        private MainViewModel _viewModel;
        private DateTimeOffset _selectedDate = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
        private DateTimeOffset _currentDisplayDate = DateTimeOffset.Now;
        private bool _isReordering = false;
        private readonly Dictionary<DateTime, CalendarViewDayItem> _activeDayItems = new();

        public CalendarPage()
        {
            this.InitializeComponent();
            _viewModel = App.Current.Services.GetRequiredService<MainViewModel>();

            this.Loaded += CalendarPage_Loaded;
            this.Unloaded += CalendarPage_Unloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplyLocalization();
            UpdateFilteredTasks();
            RefreshCalendarDensityColors();
        }

        private void ApplyLocalization()
        {
            var loc = LocalizationService.Instance;
            if (TitleText != null) TitleText.Text = loc["TitleCalendar"];
            if (AddTaskBtnText != null) AddTaskBtnText.Text = loc["BtnAddTask"];
            if (GoToTodayBtn != null) GoToTodayBtn.Content = loc["BtnToday"];
            if (PrevMonthBtn != null) ToolTipService.SetToolTip(PrevMonthBtn, loc["TooltipPrevMonth"]);
            if (NextMonthBtn != null) ToolTipService.SetToolTip(NextMonthBtn, loc["TooltipNextMonth"]);

            if (DayProgressTrackerHeader != null) DayProgressTrackerHeader.Text = loc["DayProgressTracker"];
            if (CelebrationText != null) CelebrationText.Text = loc["DayCompleteCelebration"];

            if (EmptyStateTitleText != null) EmptyStateTitleText.Text = loc["NoTasksDay"];
            if (EmptyStateSubtext != null) EmptyStateSubtext.Text = loc["NoTasksDaySubtext"];
            if (EmptyStateCreateTaskBtn != null) EmptyStateCreateTaskBtn.Content = loc["BtnCreateTask"];
        }

        private void CalendarPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalization();

            _viewModel.Tasks.CollectionChanged += Tasks_CollectionChanged;
            LocalizationService.Instance.PropertyChanged += Localizer_PropertyChanged;

            if (MainCalendar.SelectedDates.Count == 0)
            {
                MainCalendar.SelectedDates.Add(_selectedDate);
            }

            UpdateCalendarMonthHeader();
            UpdateFilteredTasks();
        }

        private void CalendarPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Tasks.CollectionChanged -= Tasks_CollectionChanged;
            LocalizationService.Instance.PropertyChanged -= Localizer_PropertyChanged;
        }

        private void Localizer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyLocalization();
            UpdateCalendarMonthHeader();
            UpdateDayProgressUI();
        }

        private void Tasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isReordering) return;
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move) return;
            UpdateFilteredTasks();
        }

        private void MainCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates.Count > 0)
            {
                var sel = sender.SelectedDates[0];
                _selectedDate = new DateTimeOffset(sel.Year, sel.Month, sel.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
                _currentDisplayDate = _selectedDate;
            }
            else
            {
                _selectedDate = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
            }

            UpdateCalendarMonthHeader();
            UpdateFilteredTasks();
        }

        private void MainCalendar_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            // Draw density stripes on days that have tasks (Green if all completed, Red if pending tasks exist)
            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(MainCalendar_CalendarViewDayItemChanging);
            }
            else if (args.Phase == 1)
            {
                if (args.Item != null)
                {
                    _activeDayItems[args.Item.Date.Date] = args.Item;
                    UpdateDayDensityColor(args.Item);
                }
            }
        }

        private void UpdateDayDensityColor(CalendarViewDayItem dayItem)
        {
            try
            {
                var itemDate = dayItem.Date;
                var dayTasks = _viewModel.Tasks.Where(t => 
                    t.Date.Year == itemDate.Year && 
                    t.Date.Month == itemDate.Month && 
                    t.Date.Day == itemDate.Day).ToList();
                if (dayTasks.Count == 0)
                {
                    dayItem.SetDensityColors(Array.Empty<Windows.UI.Color>());
                }
                else
                {
                    bool allCompleted = dayTasks.All(t => t.IsCompleted);
                    var color = allCompleted
                        ? Windows.UI.Color.FromArgb(255, 76, 175, 80)   // Green when all tasks completed (#4CAF50)
                        : Windows.UI.Color.FromArgb(255, 239, 83, 80);  // Red/coral when pending tasks exist (#EF5350)
                    dayItem.SetDensityColors(new[] { color });
                }
            }
            catch { }
        }

        private void RefreshCalendarDensityColors()
        {
            foreach (var kvp in _activeDayItems)
            {
                try
                {
                    UpdateDayDensityColor(kvp.Value);
                }
                catch { }
            }
        }

        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDisplayDate = _currentDisplayDate.AddMonths(-1);
            MainCalendar.SetDisplayDate(_currentDisplayDate);
            UpdateCalendarMonthHeader();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDisplayDate = _currentDisplayDate.AddMonths(1);
            MainCalendar.SetDisplayDate(_currentDisplayDate);
            UpdateCalendarMonthHeader();
        }

        private void GoToToday_Click(object sender, RoutedEventArgs e)
        {
            _selectedDate = new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
            _currentDisplayDate = _selectedDate;

            MainCalendar.SetDisplayDate(_currentDisplayDate);
            MainCalendar.SelectedDates.Clear();
            MainCalendar.SelectedDates.Add(_selectedDate);

            UpdateCalendarMonthHeader();
            UpdateFilteredTasks();
        }

        private void UpdateCalendarMonthHeader()
        {
            if (CalendarMonthText != null)
            {
                CalendarMonthText.Text = _currentDisplayDate.ToString("MMMM yyyy", LocalizationService.Instance.CurrentCulture);
            }
        }

        private void UpdateFilteredTasks()
        {
            FilteredTasks.Clear();
            var dayTasks = _viewModel.Tasks.Where(t => 
                t.Date.Year == _selectedDate.Year && 
                t.Date.Month == _selectedDate.Month && 
                t.Date.Day == _selectedDate.Day).OrderBy(t => t.IsCompleted);
            foreach (var t in dayTasks)
            {
                FilteredTasks.Add(t);
            }

            UpdateDayProgressUI();
            RefreshCalendarDensityColors();
        }

        private void UpdateDayProgressUI()
        {
            var loc = LocalizationService.Instance;
            var culture = loc.CurrentCulture;

            // Header titles
            if (SelectedDateTitleText != null)
            {
                if (_selectedDate.Date == DateTime.Today)
                {
                    SelectedDateTitleText.Text = $"{loc["DayToday"]}, {_selectedDate.ToString("MMMM d", culture)}";
                }
                else if (_selectedDate.Date == DateTime.Today.AddDays(1))
                {
                    SelectedDateTitleText.Text = $"{loc["DayTomorrow"]}, {_selectedDate.ToString("MMMM d", culture)}";
                }
                else if (_selectedDate.Date == DateTime.Today.AddDays(-1))
                {
                    SelectedDateTitleText.Text = $"{loc["DayYesterday"]}, {_selectedDate.ToString("MMMM d", culture)}";
                }
                else
                {
                    SelectedDateTitleText.Text = _selectedDate.ToString("dddd, MMMM d", culture);
                }
            }

            int count = FilteredTasks.Count;
            int completed = FilteredTasks.Count(t => t.IsCompleted);
            int remaining = count - completed;
            int percent = count > 0 ? (int)Math.Round((double)completed / count * 100) : 0;

            if (SelectedDateSubtext != null)
            {
                SelectedDateSubtext.Text = count == 0
                    ? loc["NoTasksDay"]
                    : loc["TasksScheduled"];
            }

            if (EmptyStatePanel != null && TasksListView != null)
            {
                EmptyStatePanel.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
                TasksListView.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            // Update Circular Progress Tracker for this day
            if (DayProgressRing != null)
            {
                DayProgressRing.Value = percent;
                if (count == 0)
                {
                    DayProgressRing.Foreground = (Brush)Application.Current.Resources["SystemControlForegroundBaseLowBrush"];
                    DayProgressPercentText.Text = "0%";
                    DayProgressDetailsText.Text = loc["NoTasksDay"];
                    DayProgressRemainingText.Text = loc["DayProgressRemainingNone"];
                    FlameBadge.Visibility = Visibility.Collapsed;
                    CelebrationBanner.Visibility = Visibility.Collapsed;
                }
                else if (percent == 100)
                {
                    DayProgressRing.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80)); // Vibrant Green
                    DayProgressPercentText.Text = "100%";
                    DayProgressDetailsText.Text = string.Format(loc["AllTasksCompleted"], count);
                    DayProgressRemainingText.Text = loc["DayCompleteCelebration"];
                    FlameBadge.Visibility = Visibility.Visible;
                    CelebrationBanner.Visibility = Visibility.Visible;

                    AnimateFlame();
                }
                else
                {
                    DayProgressRing.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 167, 38)); // Warm Orange
                    DayProgressPercentText.Text = $"{percent}%";
                    DayProgressDetailsText.Text = $"{completed} / {count} {loc["StatCompleted"]}";
                    DayProgressRemainingText.Text = string.Format(loc["DayProgressRemaining"], remaining);
                    FlameBadge.Visibility = Visibility.Collapsed;
                    CelebrationBanner.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AnimateFlame()
        {
            if (FlameScale == null) return;
            var sb = new Storyboard();
            var scaleXAnim = new DoubleAnimation
            {
                From = 0.7,
                To = 1.3,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 }
            };
            Storyboard.SetTarget(scaleXAnim, FlameScale);
            Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");

            var scaleYAnim = new DoubleAnimation
            {
                From = 0.7,
                To = 1.3,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2 }
            };
            Storyboard.SetTarget(scaleYAnim, FlameScale);
            Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");

            sb.Children.Add(scaleXAnim);
            sb.Children.Add(scaleYAnim);
            sb.Begin();
        }

        private void AddTaskForDate_Click(object sender, RoutedEventArgs e)
        {
            var draftTask = new TaskItem
            {
                Title = "",
                Date = new DateTimeOffset(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, 0, 0, 0, DateTimeOffset.Now.Offset)
            };
            Frame.Navigate(typeof(TaskDetailPage), draftTask);
        }

        private void OnTaskSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TaskItem task)
            {
                Frame.Navigate(typeof(TaskDetailPage), task);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TaskItem task)
            {
                _viewModel.DeleteTask(task);
                UpdateFilteredTasks();
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
                RefreshCalendarDensityColors();
                if (isCompleted)
                {
                    var dayTasks = _viewModel.Tasks.Where(t =>
                        t.Date.Year == _selectedDate.Year &&
                        t.Date.Month == _selectedDate.Month &&
                        t.Date.Day == _selectedDate.Day).ToList();
                    bool allDayDone = dayTasks.Count > 0 && dayTasks.All(t => t.IsCompleted);
                    bool allOverallDone = _viewModel.Tasks.Count > 0 && _viewModel.Tasks.All(t => t.IsCompleted);
                    bool allFilteredDone = FilteredTasks.Count > 0 && FilteredTasks.All(t => t.IsCompleted);

                    if (allDayDone || allOverallDone || allFilteredDone)
                    {
                        SoundService.PlayAllTasksComplete();
                    }
                    else
                    {
                        SoundService.PlayTaskComplete();
                    }
                }

                // Update progress tracker & header text right away
                UpdateDayProgressUI();

                // Allow the 260ms strikethrough animation to play cleanly before sliding the task
                await System.Threading.Tasks.Task.Delay(260);

                if (!FilteredTasks.Contains(task)) return;

                var movingCardGrid = ((cb.Parent as FrameworkElement)?.Parent as Grid) 
                    ?? GetCardGrid(TasksListView.ContainerFromItem(task) as ListViewItem);

                await AnimateCalendarTaskReorder(task, isCompleted, movingCardGrid);

                if (EmptyStatePanel != null && TasksListView != null)
                {
                    EmptyStatePanel.Visibility = FilteredTasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    TasksListView.Visibility = FilteredTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private async System.Threading.Tasks.Task AnimateCalendarTaskReorder(TaskItem task, bool isCompleted, Grid? movingGrid)
        {
            if (TasksListView == null || !FilteredTasks.Contains(task)) return;

            int fromIndex = FilteredTasks.IndexOf(task);
            if (fromIndex < 0) return;

            // Count other uncompleted tasks in FilteredTasks to find boundary
            int uncompletedCount = 0;
            for (int i = 0; i < FilteredTasks.Count; i++)
            {
                if (FilteredTasks[i] != task && !FilteredTasks[i].IsCompleted)
                {
                    uncompletedCount++;
                }
            }
            // Newly completed task moves directly under active tasks (top of completed section);
            // unchecked task moves to end of active tasks (right before completed section).
            int toIndex = uncompletedCount;

            if (fromIndex == toIndex || toIndex < 0 || toIndex >= FilteredTasks.Count)
            {
                await _viewModel.SaveTasksAsync();
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
                // Moving DOWN
                double distanceDown = 0;
                for (int i = fromIndex + 1; i <= toIndex; i++)
                {
                    var otherItem = FilteredTasks[i];
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
                // Moving UP
                double distanceUp = 0;
                for (int i = toIndex; i < fromIndex; i++)
                {
                    var otherItem = FilteredTasks[i];
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
                if (fromIndex >= 0 && fromIndex < FilteredTasks.Count &&
                    toIndex >= 0 && toIndex < FilteredTasks.Count &&
                    fromIndex != toIndex)
                {
                    FilteredTasks.Move(fromIndex, toIndex);
                }

                int vmFrom = _viewModel.Tasks.IndexOf(task);
                if (vmFrom >= 0)
                {
                    int vmUncompletedCount = 0;
                    for (int i = 0; i < _viewModel.Tasks.Count; i++)
                    {
                        if (_viewModel.Tasks[i] != task && !_viewModel.Tasks[i].IsCompleted)
                        {
                            vmUncompletedCount++;
                        }
                    }
                    int vmTo = vmUncompletedCount;

                    if (vmFrom != vmTo && vmTo >= 0 && vmTo < _viewModel.Tasks.Count)
                    {
                        _viewModel.Tasks.Move(vmFrom, vmTo);
                    }
                }
            }
            finally
            {
                _isReordering = false;
            }

            await _viewModel.SaveTasksAsync();
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
