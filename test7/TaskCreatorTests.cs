using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using Tubes_1_KPL.Controller;
using TaskModel = API.Model.Task;

namespace test7
{
    [TestClass]
    public sealed class TaskCreatorTests
    {
        private MockHttpMessageHandler _mockHttp;
        private HttpClient _mockClient;

        [TestInitialize]
        public void Setup()
        {
            // Setup MockHttpMessageHandler
            _mockHttp = new MockHttpMessageHandler();
            _mockClient = _mockHttp.ToHttpClient();
            _mockClient.BaseAddress = new Uri("http://localhost:5263/api/");
        }

        [TestMethod]
        public async Task CreateTaskAsync_ShouldSendPostRequestToApi()
        {
            // Arrange
            var taskCreator = new TaskCreator("testuser", _mockClient);
            var today = DateTime.Today;

            var task = new TaskModel(
                "Test Task",
                "Test Description",
                new API.Model.Deadline
                {
                    Day = today.Day,
                    Month = today.Month,
                    Year = today.Year,
                    Hour = 10,
                    Minute = 30
                },
                "testuser"
            );

            // Mock API response
            _mockHttp.When(HttpMethod.Post, "http://localhost:5263/api/task")
                     .Respond(HttpStatusCode.Created, "application/json", JsonSerializer.Serialize(task));

            // Act
            await taskCreator.CreateTaskAsync(
                "Test Task",
                "Test Description",
                today.Day,
                today.ToString("MMMM", new CultureInfo("id-ID")),
                today.Year,
                10,
                30
            );

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation(); // Ensure all expected calls were made
        }

        [TestMethod]
        public async Task EditTask_ShouldSendPutRequestToApi()
        {
            // Arrange
            var taskCreator = new TaskCreator("testuser", _mockClient);
            var today = DateTime.Today;

            var updatedTask = new TaskModel(
                "Updated Task",
                "Updated Description",
                new API.Model.Deadline
                {
                    Day = today.Day,
                    Month = today.Month,
                    Year = today.Year,
                    Hour = 12,
                    Minute = 45
                },
                "testuser"
            );

            // Mock API response
            _mockHttp.When(HttpMethod.Put, $"http://localhost:5263/api/task/testuser")
                     .Respond(HttpStatusCode.NoContent);

            // Act
            await taskCreator.EditTask(
                "Old Task",
                "Updated Task",
                "Updated Description",
                today.Day,
                today.ToString("MMMM", new CultureInfo("id-ID")),
                today.Year,
                12,
                45
            );

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation(); // Ensure all expected calls were made
        }

        [TestMethod]
        public async Task ExecuteDeleteTask_ShouldSendDeleteRequestToApi()
        {
            // Arrange
            var taskCreator = new TaskCreator("testuser", _mockClient);
            var taskAutomata = new TaskCreator.TaskAutomata("testuser", taskCreator);

            // Mock API response
            _mockHttp.When(HttpMethod.Delete, $"http://localhost:5263/api/task/testuser?taskName=Task%20To%20Delete")
                     .Respond(HttpStatusCode.NoContent);

            // Act
            await taskAutomata.ExecuteDeleteTask("Task To Delete");

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation(); // Ensure all expected calls were made
        }
    }
}
