using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using API.Model;
using ModelTask = API.Model.Task;
using ModelDeadline = API.Model.Deadline;

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
    }
}
