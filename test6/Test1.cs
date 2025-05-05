using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tubes_1_KPL.Controller;
using Tubes_1_KPL.Model;
using TaskModel = API.Model.Task;

namespace test6
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void ShowReminders_ShouldPrintCorrectReminderMessages()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };
            var taskCreator = new TaskCreator("testuser", httpClient);

            var culture = new CultureInfo("id-ID");

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            taskCreator.CreateTaskAsync(
                "Tugas Hari Ini",
                "Deskripsi",
                today.Day,
                today.ToString("MMMM", culture),
                today.Year,
                9, 0
            );

            taskCreator.CreateTaskAsync(
                "Tugas Besok",
                "Deskripsi",
                tomorrow.Day,
                tomorrow.ToString("MMMM", culture),
                tomorrow.Year,
                10, 0
            );

            var reminderConfig = new ReminderConfig
            {
                ReminderRules = new List<ReminderRule>
                {
                    new ReminderRule { DaysBefore = 0, Message = "Hari ini" },
                    new ReminderRule { DaysBefore = 1, Message = "Besok" }
                }
            };

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            taskCreator.ShowReminders(reminderConfig);

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

            var output = stringWriter.ToString();
            Console.WriteLine("OUTPUT:\n" + output);

            Assert.IsTrue(output.Contains($"[Reminder] Tugas 'Tugas Hari Ini' akan jatuh tempo Hari ini"), "Reminder 'Hari ini' tidak ditemukan.");
            Assert.IsTrue(output.Contains($"[Reminder] Tugas 'Tugas Besok' akan jatuh tempo Besok"), "Reminder 'Besok' tidak ditemukan.");

        }
    }
}
