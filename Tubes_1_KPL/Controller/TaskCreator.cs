using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using API.Model;
using ModelTask = API.Model.Task;
using ModelDeadline = API.Model.Deadline;
using Tubes_1_KPL.Model;
using System.Diagnostics;

namespace Tubes_1_KPL.Controller
{
    public class TaskCreator
    {
        private readonly Dictionary<string, int> _monthTable;
        private static Dictionary<string, List<ModelTask>> _userTasks = new();
        private readonly string _loggedInUser;


        public TaskCreator(string loggedInUser)
        {
            Contract.Requires(!string.IsNullOrEmpty(loggedInUser));

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

        public void CreateTask(string name, string description, int day, string monthString, int year, int hour, int minute)
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

        public void EditTask(string oldTaskName, string newName, string newDescription, int newDay, string newMonthString, int newYear, int newHour, int newMinute)
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

            public void ExecuteDeleteTask(string taskName)
            {
                if (_currentState == State.Idle)
                {
                    _currentState = State.TaskDelete;
                    bool taskDeleted = DeleteTask(taskName);

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

            private bool DeleteTask(string taskName)
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

                    _taskCreator.DeleteTask(taskName);
                    return true;
                }

                return false;
            }

            public State CurrentState => _currentState;
        }
        public void MarkTaskAsCompleted(string taskName, string answer)
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
    }
}
