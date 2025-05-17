using Organizer.Core;
using Organizer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Organizer.ViewModel
{
    public enum SortOption
    {
        ПоДате,
        ПоПриоритету
    }

    public class MainVM : ObservableObject
    {
        public IEnumerable<Category> Categories => Enum.GetValues(typeof(Category)).Cast<Category>();
        public IEnumerable<SortOption> SortOptions => Enum.GetValues(typeof(SortOption)).Cast<SortOption>();
        public IEnumerable<TaskPriority> Prioritys => Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>();

        private TimeSpan? _reminderTime;
        public TimeSpan? ReminderTime
        {
            get => _reminderTime;
            set
            {
                if (value != _reminderTime)
                {
                    _reminderTime = value;
                }
                OnPropertyChanged(nameof(ReminderTime));
            }
        }

        private bool _isEditButtonEnabled;
        public bool IsEditButtonEnabled
        {
            get => _isEditButtonEnabled;
            set
            {
                if (_isEditButtonEnabled != value)
                {
                    _isEditButtonEnabled = value;
                }
                OnPropertyChanged(nameof(IsEditButtonEnabled));
            }
        }

        private SortOption _selectedSortOption;
        public SortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged(nameof(SelectedSortOption));
                    SortTasks(); // Сортировка
                }
            }
        }

        private TaskItem _selectedTask;
        public TaskItem SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask != value)
                {
                    _selectedTask = value;
                }

                if (_selectedTask != null)
                {
                    IsEditButtonEnabled = true;
                }
                else
                {
                    IsEditButtonEnabled = false;
                }
                OnPropertyChanged(nameof(SelectedTask));
            }
        }

        private readonly Navigation _navigation;

        public ObservableCollection<TaskItem> AllTasks { get; set; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> SortedTasks { get; set; } = new ObservableCollection<TaskItem>();

        private TaskItem _newTask = new TaskItem() { DueDateTime = DateTime.Now };
        public TaskItem NewTask
        {
            get => _newTask;
            set
            {
                _newTask = value;
                OnPropertyChanged(nameof(NewTask));
            }
        }

        public ICommand AddNewTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand UpdateTaskCommand { get; }
        public ICommand NavigateToAddTask { get; }
        public ICommand NavigateToEditTask { get; }
        public ICommand DeleteCheckedTasks { get; }

        public MainVM(Navigation navigation)
        {
            _navigation = navigation;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            timer.Tick += (s, e) => CheckReminders();
            timer.Start();

            LoadTasks();
            SelectedSortOption = SortOption.ПоДате;

            DeleteCheckedTasks = new RelayCommand(_ => DeleteChecked());
            NavigateToEditTask = new RelayCommand(_ => _navigation.NavigateTo("EditTask", this));
            NavigateToAddTask = new RelayCommand(_ => _navigation.NavigateTo("AddNewTask", this));
            AddNewTaskCommand = new RelayCommand(_ => AddTask());
            DeleteTaskCommand = new RelayCommand(_ => DeleteTask(SelectedTask));
            UpdateTaskCommand = new RelayCommand(_ => UpdateTask(SelectedTask));
        }

        private DateTime UpdateFullReminderDateTime()
        {
            if (NewTask.DueDateTime.HasValue && ReminderTime.HasValue)
                return NewTask.DueDateTime.Value.Date + ReminderTime.Value;
            else
                return default;
        }

        private void DeleteChecked()
        {
            var result = MessageBox.Show("Удалить все выполненные задачи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var completedTasks = AllTasks.Where(t => t.IsCompleted).ToList();
                foreach (var task in completedTasks)
                {
                    AllTasks.Remove(task);
                }
                SaveTasks();
                SortTasks();
            }
            else
            {
                return;
            }
        }

        private void ShowToast(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }

        private void CheckReminders()
        {
            var now = DateTime.Now;
            foreach (var task in AllTasks.Where(t => !t.IsCompleted && t.ReminderDateTime.HasValue && t.IsNotifyed == false))
            {
                double difference = (task.ReminderDateTime.Value - now).TotalMinutes;
                if (difference < 1)
                {
                    ShowToast(task.Title, task.Description);
                    task.IsNotifyed = true;
                }
            }
        }

        private void LoadTasks()
        {
            AllTasks = new ObservableCollection<TaskItem>(TaskStorage.LoadTasks());
            SortTasks();
        }

        private void SaveTasks()
        {
            TaskStorage.SaveTasks(AllTasks.ToList());
        }

        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTask.Title) || NewTask.DueDateTime == default)
            {
                MessageBox.Show("Отсутствует дата и/или название задачи, они обязательны к заполнению!");
                return;
            }

            AllTasks.Add(new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = NewTask.Title,
                Description = NewTask.Description,
                DueDateTime = NewTask.DueDateTime,
                Priority = NewTask.Priority,
                Category = NewTask.Category,
                IsCompleted = false,
                ReminderDateTime = UpdateFullReminderDateTime()
            });

            SaveTasks();
            SortTasks();

            NewTask = new TaskItem()
            {
                DueDateTime = DateTime.Now,
                Priority = null,
                Category = null
            };

            _navigation.NavigateTo("MainPage", this);
        }

        private void SortTasks()
        {
            IEnumerable<TaskItem> sorted = SortedTasks;

            switch (SelectedSortOption)
            {
                case SortOption.ПоДате:
                    sorted = AllTasks.OrderBy(t => t.DueDateTime);
                    break;
                case SortOption.ПоПриоритету:
                    sorted = AllTasks.OrderByDescending(t => t.Priority);
                    break;

            }

            SortedTasks.Clear();
            foreach (var task in sorted)
                SortedTasks.Add(task);
        }

        private void DeleteTask(TaskItem task)
        {
            var result = MessageBox.Show("Удалить данную задачу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                AllTasks.Remove(task);
                SaveTasks();
                SortTasks();
                _navigation.NavigateTo("MainPage", this);
            }
            else
            {
                return;
            }
        }

        private void UpdateTask(TaskItem task)
        {
            if (task == null || string.IsNullOrWhiteSpace(SelectedTask.Title) || SelectedTask.DueDateTime == default)
            {
                MessageBox.Show("Запись не может быть отредактирована! Дата и/или название пусты, они обязательны к заполнению!");
                return;
            }

            if (task.IsCompleted == true)
            {
                task.IsCompleted = false;
            }

            SaveTasks();
            SortTasks();

            SelectedTask = null;

            _navigation.NavigateTo("MainPage", this);
        }
    }
}
