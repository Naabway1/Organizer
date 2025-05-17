using System;

namespace Organizer.Model
{
    public enum TaskPriority
    {
        Низкий,
        Средний,
        Высокий
    }

    public enum Category
    {
        Личное,
        Работа,
        Питомец,
        Учеба,
        Семья,
        Другое
    }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDateTime { get; set; }

        public Category? Category { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? ReminderDateTime { get; set; }

        public TaskPriority? Priority { get; set; }

        public bool IsNotifyed { get; set; } = false;
    }
}
