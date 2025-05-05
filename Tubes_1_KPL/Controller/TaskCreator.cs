using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using API.Model;
using ModelTask = API.Model.Task;
using ModelDeadline = API.Model.Deadline;
using Tubes_1_KPL.Model;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using static System.Net.WebRequestMethods;
using System.Diagnostics.Eventing.Reader;

namespace Tubes_1_KPL.Controller
{
    public class TaskCreator
    {
        private readonly Dictionary<string, int> _monthTable;
        private static Dictionary<string, List<ModelTask>> _userTasks = new();
        private readonly string _loggedInUser;

        private readonly HttpClient _http;

        public TaskCreator(string loggedInUser, HttpClient httpClient)
        {
            Contract.Requires(httpClient != null, "HttpClient tidak boleh null.");
            Contract.Requires(!string.IsNullOrEmpty(loggedInUser), "LoggedInUser tidak boleh null atau kosong.");

            _http = httpClient;
            _loggedInUser = loggedInUser;

            _loggedInUser = loggedInUser;
            _monthTable = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                {"januari", 1}, {"februari", 2}, {"maret", 3}, {"april", 4},
                {"mei", 5}, {"juni", 6}, {"juli", 7}, {"agustus", 8},
                {"september", 9}, {"oktober", 10}, {"november", 11}, {"desember", 12}
            };

            if (!_userTasks.ContainsKey(_loggedInUser))
            {
                _userTasks[_loggedInUser] = new List<ModelTask>();
            }
        }

        public async System.Threading.Tasks.Task CreateTaskAsync(string name, string description, int day, string monthString, int year, int hour, int minute)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(!string.IsNullOrEmpty(description));
            Contract.Requires(day > 0 && day <= 31);
            Contract.Requires(!string.IsNullOrEmpty(monthString));
            Contract.Requires(year > 0);
            Contract.Requires(hour >= 0 && hour <= 23);
            Contract.Requires(minute >= 0 && minute <= 59);
            Contract.Requires(_monthTable.ContainsKey(monthString));

            if (_userTasks.TryGetValue(_loggedInUser, out var tasks))
            {
                int month = _monthTable[monthString];
                if (IsValidDate(day, month, year))
                {
                    var deadline = new ModelDeadline { Day = day, Month = month, Year = year, Hour = hour, Minute = minute };
                    var task = new ModelTask(name, description, deadline, _loggedInUser);
                    tasks.Add(task);

                    var jsonContent = JsonSerializer.Serialize(task);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var response = await _http.PostAsync("task", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Tugas berhasil dibuat di API.");
                    }
                    else
                    {
                        Console.WriteLine($"Gagal membuat tugas di API. Status: {response.StatusCode}");
                    }
                }
                else
                {
                    Console.WriteLine("Tanggal tidak valid.");
                }
            }
        }

        public List<ModelTask> GetUserTasks()
        {
            return _userTasks.TryGetValue(_loggedInUser, out var tasks) ? new List<ModelTask>(tasks) : new List<ModelTask>();
        }
            
        public async System.Threading.Tasks.Task EditTask(string oldTaskName, string newName, string newDescription, int newDay, string newMonthString, int newYear, int newHour, int newMinute)
        {
            Contract.Requires(!string.IsNullOrEmpty(oldTaskName), "Nama tugas lama harus diisi.");
            Contract.Requires(!string.IsNullOrEmpty(newName), "Nama tugas baru harus diisi.");
            Contract.Requires(!string.IsNullOrEmpty(newDescription), "Deskripsi tugas baru harus diisi.");
            Contract.Requires(newDay > 0 && newDay <= 31, "Tanggal tidak valid.");
            Contract.Requires(!string.IsNullOrEmpty(newMonthString), "Bulan harus diisi.");
            Contract.Requires(newYear > 0, "Tahun harus valid.");
            Contract.Requires(newHour >= 0 && newHour <= 23, "Jam harus di antara 0 dan 23.");
            Contract.Requires(newMinute >= 0 && newMinute <= 59, "Menit harus di antara 0 dan 59.");
            Contract.Requires(_monthTable.ContainsKey(newMonthString), "Bulan tidak valid.");

            if (_userTasks.TryGetValue(_loggedInUser, out var tasks))
            {
                var taskToEdit = tasks.FirstOrDefault(t => t.Name.Equals(oldTaskName, StringComparison.OrdinalIgnoreCase));
                if (taskToEdit != null)
                {
                    int newMonth = _monthTable[newMonthString];
                    if (IsValidDate(newDay, newMonth, newYear))
                    {
                        taskToEdit.Name = newName;
                        taskToEdit.Description = newDescription;
                        taskToEdit.Deadline = new ModelDeadline { Day = newDay, Month = newMonth, Year = newYear, Hour = newHour, Minute = newMinute };
                        var jsonContent = JsonSerializer.Serialize(taskToEdit);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        var response = await _http.PutAsync($"task/{_loggedInUser}", content);
                        Console.WriteLine($"Tugas '{oldTaskName}' berhasil diubah.");
                    }
                    else
                    {
                        Console.WriteLine("Tanggal baru tidak valid.");
                    }
                }
                else
                {
                    Console.WriteLine($"Tidak ditemukan tugas dengan nama '{oldTaskName}'.");
                }
            }
            else
            {
                Console.WriteLine("Tidak ada tugas yang ditemukan.");
            }
        }


        private bool IsValidDate(int day, int month, int year)
        {
            return day <= DateTime.DaysInMonth(year, month);
        }

        public void DeleteTask(string taskName)
        {
            Contract.Requires(!string.IsNullOrEmpty(taskName), "Nama tugas harus diisi.");

            if (_userTasks.TryGetValue(_loggedInUser, out var tasks))
            {
                var taskToDelete = tasks.FirstOrDefault(t => t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));
                if (taskToDelete != null)
                {
                    tasks.Remove(taskToDelete);
                }
            }
            else
            {
                Console.WriteLine("Tidak ada tugas yang ditemukan.");
            }
        }

        public class TaskAutomata
        {
            public enum State
            {
                Idle,
                TaskDelete,
                TaskDeleted
            }

            private State _currentState;
            private readonly TaskCreator _taskCreator;
            private readonly string _loggedInUser;

            public TaskAutomata(string loggedInUser, TaskCreator taskCreator)
            {
                _loggedInUser = loggedInUser;
                _taskCreator = taskCreator;
                _currentState = State.Idle;
            }

            public async System.Threading.Tasks.Task ExecuteDeleteTask(string taskName)
            {
                if (_currentState == State.Idle)
                {
                    _currentState = State.TaskDelete;
                    bool taskDeleted = await DeleteTask(taskName);

                    if (taskDeleted)
                    {
                        _currentState = State.TaskDeleted;
                        Console.WriteLine($"Tugas '{taskName}' berhasil dihapus.");
                    }
                    else
                    {
                        Console.WriteLine($"Tugas dengan nama '{taskName}' tidak ditemukan.");
                    }
                }
                else
                {
                    Console.WriteLine("Operasi tidak bisa dijalankan lebih dari sekali.");
                }
            }

            private async Task<bool> DeleteTask(string taskName)
            {
                if (string.IsNullOrEmpty(_loggedInUser))
                {
                    Console.WriteLine("Anda harus login terlebih dahulu.");
                    return false;
                }

                var tasks = _taskCreator.GetUserTasks();
                var taskToDelete = tasks.FirstOrDefault(t => t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));

                if (taskToDelete != null)
                {
                    var response = await _taskCreator._http.DeleteAsync($"task/{_loggedInUser}?taskName={Uri.EscapeDataString(taskName)}");

                    if (response.IsSuccessStatusCode)
                    {
                        _taskCreator.DeleteTask(taskName);
                        return true;
                    }
                }

                return false;
            }

            public State CurrentState => _currentState;
        }


        public async System.Threading.Tasks.Task MarkTaskAsCompleted(string taskName, string answer)
        {
            Contract.Requires(!string.IsNullOrEmpty(taskName), "Nama tugas tidak boleh kosong.");
            Contract.Requires(!string.IsNullOrEmpty(answer), "Jawaban tidak boleh kosong.");

            if (!_userTasks.TryGetValue(_loggedInUser, out var tasks))
            {
                Console.WriteLine("Pengguna tidak memiliki tugas.");
                return;
            }

            var task = tasks.FirstOrDefault(t => t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));
            if (task == null)
            {
                Console.WriteLine($"Tugas '{taskName}' tidak ditemukan.");
                return;
            }

            Dictionary<string, Status> answerTable = new(StringComparer.OrdinalIgnoreCase)
            {
                { "yes", Status.Completed },
                { "no", Status.Incompleted }
            };

            if (!answerTable.ContainsKey(answer))
            {
                Console.WriteLine("Input tidak valid. Harus 'yes' atau 'no'.");
                return;
            }

            Status newStatus = answerTable[answer];

            Console.WriteLine($"[DEBUG] Status lama: {task.Status}, Status baru: {newStatus}");

            switch (task.Status)
            {
                case Status.Incompleted:
                    if (newStatus == Status.Completed)
                    {
                        task.Status = Status.Completed;
                        var jsonContent = JsonSerializer.Serialize(task);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        var response = await _http.PutAsync($"task/{_loggedInUser}/{taskName}", content);
                        Console.WriteLine($"Tugas '{taskName}' berhasil ditandai selesai.");
                    }
                    else
                    {
                        Console.WriteLine($"Tugas '{taskName}' tetap dalam status belum selesai.");
                    }
                    break;

                case Status.Completed:
                    Console.WriteLine($"Tugas '{taskName}' sudah selesai sebelumnya.");
                    break;

                case Status.Overdue:
                    Console.WriteLine($"Tugas '{taskName}' sudah melewati batas waktu.");
                    break;

                default:
                    Console.WriteLine("Status tidak dikenali.");
                    break;
            }
        }

        public void ShowReminders(ReminderConfig config)
        {
            DateTime now = DateTime.Now;
            var userTasks = GetUserTasks();

            foreach (var task in userTasks)
            {
                Debug.Assert(task != null, "Task seharusnya tidak null.");
                Debug.Assert(!string.IsNullOrEmpty(task?.Name), "Task name seharusnya tidak kosong.");

                if (task == null || string.IsNullOrEmpty(task.Name)) continue;

                DateTime deadline = new DateTime(
                    task.Deadline.Year,
                    task.Deadline.Month,
                    task.Deadline.Day,
                    task.Deadline.Hour,
                    task.Deadline.Minute,
                    0
                );
                Debug.Assert(Enum.IsDefined(typeof(Status), task.Status), "Status task tidak valid.");

                if (task.Status == Status.Incompleted && now > deadline)
                {
                    task.Status = Status.Overdue;
                    continue;
                }

                if (task.Status != Status.Incompleted) continue;

                int daysDiff = (deadline.Date - now.Date).Days;

                foreach (var rule in config.ReminderRules)
                {
                    Debug.Assert(rule != null, "Reminder rule tidak boleh null.");
                    Debug.Assert(rule.Message != null, "Reminder message tidak boleh null.");

                    if (daysDiff == rule.DaysBefore)
                    {
                        Console.WriteLine($"[Reminder] Tugas '{task.Name}' akan jatuh tempo {rule.Message} pada {deadline}.");
                    }
                }
            }
        }

        public async Task<List<ModelTask>> GetOngoingTasksAsync()
        {
            return await GetTasksFromApiAsync($"task/ongoing/{_loggedInUser}");
        }

        public async Task<List<ModelTask>> GetOverdueTasksAsync()
        {
            return await GetTasksFromApiAsync($"task/overdue/{_loggedInUser}");
        }

        public async Task<List<ModelTask>> GetCompletedTasksAsync()
        {
            return await GetTasksFromApiAsync($"task/completed/{_loggedInUser}");
        }

        private async Task<List<ModelTask>> GetTasksFromApiAsync(string endpoint)
        {
            try
            {
                var response = await _http.GetAsync($"{endpoint}?userId={_loggedInUser}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ModelTask>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ModelTask>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<ModelTask>();
                } 
                else
                {
                    Console.WriteLine($"Gagal mengambil data dari API. Status: {response.StatusCode}");
                    return new List<ModelTask>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terjadi kesalahan saat menghubungi API: {ex.Message}");
                return new List<ModelTask>();
            }
        }
    }
}
