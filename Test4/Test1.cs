using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tubes_1_KPL.Controller;
using API.Model;
using System;
using System.Collections.Generic;
using static Tubes_1_KPL.Controller.TaskCreator;

namespace Test4
{

    namespace MyApp.Tests
    {
        [TestClass]
        public class TaskAutomataTests
        {
            private TaskCreator _taskCreator;
            private string _loggedInUser;

            [TestInitialize]
            public void TestInitialize()
            {
                _loggedInUser = "user1";
                var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };
                _taskCreator = new TaskCreator(_loggedInUser, httpClient);
                _taskCreator.CreateTaskAsync("Test Task", "Description of test task", 1, "Januari", 2025, 10, 30);
            }

            [TestMethod]
            public void Test_DeleteTask_Success()
            {
                var taskAutomata = new TaskAutomata(_loggedInUser, _taskCreator);

                taskAutomata.ExecuteDeleteTask("Test Task");

                var tasks = _taskCreator.GetUserTasks();
                Assert.AreEqual(0, tasks.Count, "Tugas tidak berhasil dihapus.");
            }

            [TestMethod]
            public void Test_DeleteTask_Failure_TaskNotFound()
            {
                var taskAutomata = new TaskAutomata(_loggedInUser, _taskCreator);

                taskAutomata.ExecuteDeleteTask("Nonexistent Task");

                var tasks = _taskCreator.GetUserTasks();
                Assert.AreEqual(1, tasks.Count, "Tugas tidak seharusnya dihapus.");
            }
        }
    }

}
