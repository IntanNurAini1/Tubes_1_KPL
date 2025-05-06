using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tubes_1_KPL.Controller;
using API.Model;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ModelTask = API.Model.Task;

namespace Tubes_1_KPL.Tests
{
    [TestClass]
    public class TaskCreatorTests
    {
        private const string BaseUrl = "http://localhost:5263/api/";

        [TestMethod]
        public async Task CreateTask_ShouldAddTask_WhenValidInput()
        {
            // Arrange
            string userId = "user123";
            var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            var taskCreator = new TaskCreator(userId, httpClient);

            string name = "Tugas UTS";
            string description = "Kerjakan soal nomor 1-5";
            int day = 20, year = 2025, hour = 10, minute = 30;
            string month = "April";

            // Act
            await taskCreator.CreateTaskAsync(name, description, day, month, year, hour, minute);

            // Assert
            var tasks = taskCreator.GetUserTasks();
            Assert.IsTrue(tasks.Exists(t =>
                t.Name == name &&
                t.Description == description &&
                t.UserId == userId &&
                t.Deadline.Day == day &&
                t.Deadline.Month == 4 // April
            ));
        }

        [TestMethod]
        public async Task CreateTask_ShouldNotAddTask_WhenInvalidDate()
        {
            // Arrange
            var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            var taskCreator = new TaskCreator("user123", httpClient);

            // Invalid: February 31 does not exist
            await taskCreator.CreateTaskAsync("Invalid Task", "Deskripsi", 31, "Februari", 2025, 10, 0);

            // Assert
            List<ModelTask> tasks = taskCreator.GetUserTasks();
            Assert.IsFalse(tasks.Exists(t => t.Name == "Invalid Task"));
        }

        [TestMethod]
        public void GetUserTasks_ShouldReturnEmptyList_WhenNoTasks()
        {
            // Arrange
            var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            var taskCreator = new TaskCreator("userABC", httpClient);

            // Act
            List<ModelTask> tasks = taskCreator.GetUserTasks();

            // Assert
            Assert.AreEqual(0, tasks.Count);
        }
    }
}
