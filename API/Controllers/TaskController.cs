using Microsoft.AspNetCore.Mvc;
using API.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Task = API.Model.Task;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private static List<Task> tasks = new List<Task>();

        private string GetFilePath()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string folderPath = Path.Combine(projectRoot, "Data");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, "task.json");
        }

        private void LoadTasksFromFile()
        {
            string filePath = GetFilePath();

            if (!System.IO.File.Exists(filePath))
            {
                tasks = new List<Task>();
                return;
            }

            string jsonData = System.IO.File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                tasks = new List<Task>();
                return;
            }

            try
            {
                tasks = JsonSerializer.Deserialize<List<Task>>(jsonData) ?? new List<Task>();
            }
            catch (JsonException)
            {
                try
                {
                    var singleTask = JsonSerializer.Deserialize<Task>(jsonData);
                    if (singleTask != null)
                    {
                        tasks = new List<Task> { singleTask };
                    }
                    else
                    {
                        tasks = new List<Task>();
                    }
                }
                catch
                {
                    tasks = new List<Task>();
                }
            }
        }

        private void SaveTasksToFile()
        {
            string filePath = GetFilePath();
            string updatedJsonData = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, updatedJsonData);
        }

        [HttpPost]
        public IActionResult CreateTask([FromBody] Task newTask)
        {
            if (newTask == null)
                return BadRequest("Task data is invalid.");

            LoadTasksFromFile();
            tasks.Add(newTask);
            SaveTasksToFile();

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);
        }

        [HttpGet]
        public IActionResult GetAllTasks()
        {
            LoadTasksFromFile();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public IActionResult GetTaskById(string id)
        {
            LoadTasksFromFile();
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return NotFound($"Task with ID {id} not found.");
            return Ok(task);
        }

        [HttpPut("{username}/{taskName}")]
        public IActionResult UpdateTask(string username, string taskName, [FromBody] Task updatedTask)
        {
            LoadTasksFromFile();
            var taskToUpdate = tasks.FirstOrDefault(t => t.UserId == username && t.Name == taskName);
            if (taskToUpdate == null)
                return NotFound($"Task '{taskName}' untuk user '{username}' tidak ditemukan.");

            taskToUpdate.Name = updatedTask.Name;
            taskToUpdate.Description = updatedTask.Description;
            taskToUpdate.Deadline = updatedTask.Deadline;
            taskToUpdate.Status = updatedTask.Status;

            SaveTasksToFile();
            return NoContent();
        }

        [HttpDelete("{username}")]
        public IActionResult DeleteTask(string username, [FromQuery] string taskName)
        {
            LoadTasksFromFile();
            var taskToDelete = tasks.FirstOrDefault(t => t.UserId == username && t.Name == taskName);
            if (taskToDelete == null)
                return NotFound($"Task '{taskName}' untuk user '{username}' tidak ditemukan.");

            tasks.Remove(taskToDelete);
            SaveTasksToFile();
            return NoContent();
        }

        [HttpGet("ongoing/{username}")]
        public IActionResult GetOngoingTasks(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Username is required.");

            LoadTasksFromFile();
            var ongoingTasks = tasks.Where(t
