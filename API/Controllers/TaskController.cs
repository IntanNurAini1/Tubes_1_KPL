using Microsoft.AspNetCore.Mvc;
using API.Model;
using System.Collections.Generic;
using System.Linq;
using Task = API.Model.Task;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/task")]
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

            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string folderPath = Path.Combine(projectRoot, "Data");
            string filePath = Path.Combine(folderPath, "task.json");

            try
            {
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                List<Task> fileTasks = new List<Task>();
                if (System.IO.File.Exists(filePath))
                {
                    string jsonData = System.IO.File.ReadAllText(filePath);
                    fileTasks = string.IsNullOrWhiteSpace(jsonData)
                        ? []
                        : JsonSerializer.Deserialize<List<Task>>(jsonData) ?? [];
                }

                fileTasks.Add(newTask);

                string updatedJsonData = JsonSerializer.Serialize(fileTasks, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, updatedJsonData);

                Console.WriteLine("Task berhasil disimpan ke file task.json di folder data.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gagal menyimpan task ke file task.json: {ex.Message}");
                return StatusCode(500, "Terjadi kesalahan saat menyimpan task ke file.");
            }

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);
        }

        // READ: Get all tasks
        [HttpGet]
        public IActionResult GetAllTasks()
        {
            return Ok(tasks);
        }

        //READ: Get a task by ID
        [HttpGet("{id}")]
        public IActionResult GetTaskById(string id)
        {
            var task = tasks.FirstOrDefault(t => t.UserId == id);
            if (task == null)
                return NotFound($"Task with ID {id} not found.");

            return Ok(task);
        }

        // UPDATE: Update a task by ID
        [HttpPut("{username}/{taskName}")]
        public IActionResult UpdateTask(string username, string taskName, [FromBody] Task updatedTask)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string folderPath = Path.Combine(projectRoot, "Data");
            string filePath = Path.Combine(folderPath, "task.json");

            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File task.json tidak ditemukan.");
                }

                string jsonData = System.IO.File.ReadAllText(filePath);
                List<Task> fileTasks = JsonSerializer.Deserialize<List<Task>>(jsonData) ?? [];

                var taskToUpdate = tasks.FirstOrDefault(t => t.UserId == username && t.Name == taskName);
                var taskToUpdateJson = fileTasks.FirstOrDefault(t => t.UserId == username && t.Name == taskName);
                if (taskToUpdate == null)
                {
                    return NotFound($"Task '{taskName}' untuk user '{username}' tidak ditemukan.");
                }

                taskToUpdate.Name = updatedTask.Name;
                taskToUpdate.Description = updatedTask.Description;
                taskToUpdate.Deadline = updatedTask.Deadline;
                taskToUpdate.Status = updatedTask.Status;

                taskToUpdateJson.Name = updatedTask.Name;
                taskToUpdateJson.Description = updatedTask.Description;
                taskToUpdateJson.Deadline = updatedTask.Deadline;
                taskToUpdateJson.Status = updatedTask.Status;

                string updatedJsonData = JsonSerializer.Serialize(taskToUpdateJson, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, updatedJsonData);

                Console.WriteLine($"Task '{taskName}' berhasil diperbarui di file task.json.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gagal memperbarui task di file task.json: {ex.Message}");
                return StatusCode(500, "Terjadi kesalahan saat memperbarui task di file.");
            }

            return NoContent();
        }

        // DELETE: Delete a task by ID
        [HttpDelete("{username}")]
        public IActionResult DeleteTask(string username, [FromQuery] string taskName)
        {
            // Path ke folder data dan file task.json
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string folderPath = Path.Combine(projectRoot, "Data");
            string filePath = Path.Combine(folderPath, "task.json");

            try
            {
                // Pastikan file task.json ada
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File task.json tidak ditemukan.");
                }

                // Baca data dari file task.json
                string jsonData = System.IO.File.ReadAllText(filePath);
                List<Task> fileTasks = JsonSerializer.Deserialize<List<Task>>(jsonData) ?? new List<Task>();

                // Cari task yang akan dihapus
                var taskToDelete = fileTasks.FirstOrDefault(t => t.UserId == username && t.Name == taskName);
                if (taskToDelete == null)
                {
                    return NotFound($"Task '{taskName}' untuk user '{username}' tidak ditemukan.");
                }

                // Hapus task dari daftar
                fileTasks.Remove(taskToDelete);

                // Tulis kembali data yang diperbarui ke file task.json
                string updatedJsonData = JsonSerializer.Serialize(fileTasks, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, updatedJsonData);

                Console.WriteLine($"Task '{taskName}' berhasil dihapus dari file task.json.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gagal menghapus task dari file task.json: {ex.Message}");
                return StatusCode(500, "Terjadi kesalahan saat menghapus task dari file.");
            }

            return NoContent();
        }
        // READ: Get ongoing tasks for a user
        [HttpGet("ongoing/{username}")]
        public IActionResult GetOngoingTasks(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("username is required.");

            var now = DateTime.Now;
            var ongoingTasks = tasks.Where(t => t.UserId == username && t.Status == Status.Incompleted).ToList();

            return Ok(ongoingTasks);
        }

        // READ: Get overdue tasks for a user
        [HttpGet("overdue/{username}")]
        public IActionResult GetOverdueTasks(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("username is required.");

            var now = DateTime.Now;
            var overdueTasks = tasks.Where(t => t.UserId == username && t.Status == Status.Overdue).ToList();

            return Ok(overdueTasks);
        }

        // READ: Get completed tasks for a user
        [HttpGet("completed/{username}")]
        public IActionResult GetCompletedTasks(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("username is required.");

            var completedTasks = tasks.Where(t => t.UserId == username && t.Status == Status.Completed).ToList();

            return Ok(completedTasks);
        }

    }
}
