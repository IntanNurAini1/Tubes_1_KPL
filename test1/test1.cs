using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tubes_1_KPL.Model;
using Tubes_1_KPL.Controller;

namespace test1
{
    [TestClass]
    public class LoginRegisterAutomataTests
    {
        [TestMethod]
        public async Task Login_Berhasil()
        {
            var mockController = new MockLoginRegisterController();
            mockController.AddUser("user", "user123");
            var automata = new LoginRegisterAutomata(mockController);

            await automata.Login("user", "user123");

            Assert.AreEqual(LoginRegisterAutomata.State.LoggedIn, automata.CurrentState);
        }

        [TestMethod]
        public async Task Login_Gagal()
        {
            var mockController = new MockLoginRegisterController();
            mockController.AddUser("user", "user123");
            var automata = new LoginRegisterAutomata(mockController);

            await automata.Login("user", "wrongpassword");

            Assert.AreEqual(LoginRegisterAutomata.State.LoggedOut, automata.CurrentState);
        }


        private class MockLoginRegisterController : LoginRegisterController
        {
            private readonly Dictionary<string, string> _users = new();

            public void AddUser(string username, string password)
            {
                _users[username] = password;
            }

            public override Task<bool> TryLoginAsync(string username, string password)
            {
                bool success = _users.ContainsKey(username) && _users[username] == password;
                return Task.FromResult(success);
            }

            public override Task LogoutAsync(string username)
            {
                return Task.CompletedTask;
            }

            public override Task RegisterAsync()
            {
                return Task.CompletedTask;
            }

            public override Task RegisterAsync(string username, string password)
            {
                AddUser(username, password);
                return Task.CompletedTask;
            }
        }
    }
}