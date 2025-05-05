using Microsoft.AspNetCore.Mvc;
using API.Model;
using System.Collections.Generic;
using System.Linq;
using Task = API.Model.Task;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private static List<Task> tasks = new List<Task>();

        // CREATE: Add a new task
        [HttpPost]
        public IActionResult CreateTask([FromBody] Task newTask)
        {
            if (newTask == null)
                return BadRequest("Task data is invalid.");

            tasks.Add(newTask);
            return CreatedAtAction(nameof(GetTaskByName), new { taskName = newTask.Name, userId = newTask.UserId }, newTask);
        }

        // READ: Get all tasks
        [HttpGet]
        public IActionResult GetAllTasks()
        {
            return Ok(tasks);
        }

        // ✅ READ: Get task by task name (and optional userId)
        [HttpGet("by-name")]
        public IActionResult GetTaskByName([FromQuery] string taskName, [FromQuery] string userId = null)
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return BadRequest("Task name is required.");

            var taskQuery = tasks.Where(t => t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(userId))
            {
                taskQuery = taskQuery.Where(t => t.UserId == userId);
            }

            var task = taskQuery.FirstOrDefault();

            if (task == null)
                return NotFound($"Task '{taskName}'{(userId != null ? $" for user '{userId}'" : "")} not found.");

            return Ok(task);
        }

        // ✅ UPDATE: Update task by name (and userId)
        [HttpPut("by-name")]
        public IActionResult UpdateTaskByName([FromQuery] string taskName, [FromQuery] string userId, [FromBody] Task updatedTask)
        {
            if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(userId))
                return BadRequest("Task name and userId are required.");

            var task = tasks.FirstOrDefault(t =>
                t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase) && t.UserId == userId);

            if (task == null)
                return NotFound($"Task '{taskName}' for user '{userId}' not found.");

            task.Name = updatedTask.Name;
            task.Description = updatedTask.Description;
            task.Deadline = updatedTask.Deadline;
            task.Status = updatedTask.Status;

            return NoContent();
        }

        // ✅ DELETE: Delete task by name and userId
        [HttpDelete("by-name")]
        public IActionResult DeleteTaskByName([FromQuery] string taskName, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(userId))
                return BadRequest("Task name and userId are required.");

            var task = tasks.FirstOrDefault(t =>
                t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase) && t.UserId == userId);

            if (task == null)
                return NotFound($"Task '{taskName}' for user '{userId}' not found.");

            tasks.Remove(task);
            return NoContent();
        }
    }
}
