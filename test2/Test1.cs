using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tubes_1_KPL.Controller;
using API.Model;
using System.Collections.Generic;
using ModelTask = API.Model.Task;

namespace Tubes_1_KPL.Tests
{
    [TestClass]
    public class TaskCreatorTests
    {
        [TestMethod]
        public void CreateTask_ShouldAddTask_WhenValidInput()
        {
            string userId = "user123";
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };
            var taskCreator = new TaskCreator(userId, httpClient);
            string name = "Tugas UTS";
            string description = "Kerjakan soal nomor 1-5";
            int day = 20, year = 2025, hour = 10, minute = 30;
            string month = "April";

            taskCreator.CreateTaskAsync(name, description, day, month, year, hour, minute);
            List<ModelTask> tasks = taskCreator.GetUserTasks();

            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(name, tasks[0].Name);
            Assert.AreEqual(description, tasks[0].Description);
            Assert.AreEqual(userId, tasks[0].UserId);
            Assert.AreEqual(day, tasks[0].Deadline.Day);
            Assert.AreEqual(4, tasks[0].Deadline.Month);
        }

        [TestMethod]
        public void CreateTask_ShouldNotAddTask_WhenInvalidDate()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };
            var taskCreator = new TaskCreator("user123", httpClient);
            taskCreator.CreateTaskAsync("Invalid Task", "Deskripsi", 31, "Februari", 2025, 10, 0);
            List<ModelTask> tasks = taskCreator.GetUserTasks();

            Assert.AreEqual(0, tasks.Count);
        }

        [TestMethod]
        public void GetUserTasks_ShouldReturnEmptyList_WhenNoTasks()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };
            var taskCreator = new TaskCreator("userABC", httpClient);
            List<ModelTask> tasks = taskCreator.GetUserTasks();

            Assert.AreEqual(0, tasks.Count);
        }
    }
}
