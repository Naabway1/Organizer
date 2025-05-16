using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Organizer.Model;

public static class TaskStorage
{
    private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.json");

    public static List<TaskItem> LoadTasks()
    {
        if (!File.Exists(FilePath))
            return new List<TaskItem>();

        var json = File.ReadAllText(FilePath);
        return JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
    }

    public static void SaveTasks(List<TaskItem> tasks)
    {
        var json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }
}
