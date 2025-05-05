using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var mockController = new MockLoginRegisterController(true);
            var automata = new LoginRegisterAutomata(mockController);
            //await automata.Login("user", "pass");
            Assert.AreEqual(LoginRegisterAutomata.State.LoggedIn, automata.CurrentState);
        }

        [TestMethod]
        public async Task Login_Gagal()
        {
            var mockController = new MockLoginRegisterController(false);
            var automata = new LoginRegisterAutomata(mockController);
            //await automata.Login("user", "passe");
            Assert.AreEqual(LoginRegisterAutomata.State.LoggedOut, automata.CurrentState);
        }


        private class MockLoginRegisterController : LoginRegisterController
        {
            private readonly bool _loginResult;

            public MockLoginRegisterController(bool loginResult)
            {
                _loginResult = loginResult;
            }

            public override Task<bool> TryLoginAsync(string username, string password)
            {
                return Task.FromResult(_loginResult);
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
                return Task.CompletedTask;
            }
        }
    }
}