using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using API.Model; // pakai model dari API
using ModelTask = API.Model.Task;
using ModelDeadline = API.Model.Deadline;

namespace Tubes_1_KPL.Controller
{
    public class TaskCreator
    {
        private readonly Dictionary<string, int> _monthTable;
        private static Dictionary<string, List<ModelTask>> _userTasks = new(); // dibuat static
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

        private bool IsValidDate(int day, int month, int year)
        {
            return day <= DateTime.DaysInMonth(year, month);
        }
    }
}
