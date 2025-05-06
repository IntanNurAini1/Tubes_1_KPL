using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tubes_1_KPL.Controller;
using Tubes_1_KPL.Model;
using ModelTask = API.Model.Task;
using System.Collections.Generic;

namespace Test3
{
    [TestClass]
    public sealed class TaskCreatorTests
    {
        private TaskCreator _taskCreator;

        [TestInitialize]
        public void Setup()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };
            _taskCreator = new TaskCreator("testuser", httpClient);
        }

        [TestMethod]
        public void TestCreateTask()
        {
            string name = "Tugas 1";
            string description = "Deskripsi tugas 1";
            int day = 5, month = 1, year = 2025, hour = 14, minute = 30;

            _taskCreator.CreateTaskAsync(name, description, day, "Januari", year, hour, minute);

            var tasks = _taskCreator.GetUserTasks();
            var createdTask = tasks.Find(t => t.Name == name); // mencari task dengan nama yang sama
            Assert.IsNotNull(createdTask, "Tugas tidak berhasil dibuat.");
            Assert.AreEqual(description, createdTask.Description, "Deskripsi tugas tidak sesuai.");
        }

        [TestMethod]
        public void TestEditTask()
        {
            string oldTaskName = "Tugas 1";
            string newTaskName = "Tugas 1 (Updated)";
            string newDescription = "Deskripsi tugas yang diperbarui";
            int newDay = 10, newMonth = 1, newYear = 2025, newHour = 16, newMinute = 45;

            _taskCreator.CreateTaskAsync(oldTaskName, "Deskripsi tugas 1", 5, "Januari", 2025, 14, 30);

            _taskCreator.EditTask(oldTaskName, newTaskName, newDescription, newDay, "Januari", newYear, newHour, newMinute);

            var tasks = _taskCreator.GetUserTasks();
            var editedTask = tasks.Find(t => t.Name == newTaskName);
            Assert.IsNotNull(editedTask, "Tugas yang diedit tidak ditemukan.");
            Assert.AreEqual(newDescription, editedTask.Description, "Deskripsi tugas yang diedit tidak sesuai.");
            Assert.AreEqual(newDay, editedTask.Deadline.Day, "Tanggal deadline tidak sesuai.");
            Assert.AreEqual(newHour, editedTask.Deadline.Hour, "Jam deadline tidak sesuai.");
        }
    }
}
