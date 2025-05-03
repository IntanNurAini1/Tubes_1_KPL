using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using API.Model;
using Tubes_1_KPL.Controller;
using ModelTask = API.Model.Task; 

namespace test5
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1_MarkTaskAsCompleted_ShouldUpdateStatusToCompleted()
        {
            string userId = "user123";
            var taskCreator = new TaskCreator(userId);

            taskCreator.CreateTask(
                name: "Tugas Uji",
                description: "Deskripsi uji",
                day: 10,
                monthString: "Mei",
                year: 2025,
                hour: 10,
                minute: 30
            );

            taskCreator.MarkTaskAsCompleted("Tugas Uji", "yes");

            List<ModelTask> userTasks = taskCreator.GetUserTasks();
            var task = userTasks.Find(t => t.Name == "Tugas Uji");

            Assert.IsNotNull(task, "Task tidak ditemukan.");
            Assert.AreEqual(Status.Completed, task.Status, "Status task seharusnya 'Completed'.");
        }
    }
}