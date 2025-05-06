using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using API.Model;
using ModelTask = API.Model.Task;
using ModelDeadline = API.Model.Deadline;
using Tubes_1_KPL.Model;

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
            Contract.Requires(!string.IsNullOrEmpty(oldTaskName), "Nama tugas lama harus diisi.
