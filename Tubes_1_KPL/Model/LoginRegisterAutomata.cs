using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Tubes_1_KPL.Controller;

namespace Tubes_1_KPL.Model
{
    public class LoginRegisterAutomata
    {
        public enum State
        {
            LoggedOut,
            LoggedIn
        }

        private State _currentState;
        public State CurrentState => _currentState;

        private readonly LoginRegisterController _controller;
        private string? _currentUser;

        public LoginRegisterAutomata()
        {
            _currentState = State.LoggedOut;
            _controller = new LoginRegisterController();
        }

        public LoginRegisterAutomata(LoginRegisterController controller)
        {
            _currentState = State.LoggedOut;
            _controller = controller;
        }

        public async Task Register()
        {
            Contract.Requires(_currentState == State.LoggedOut);
            await _controller.RegisterAsync();
        }

        public async Task Register(string username, string password)
        {
            Contract.Requires(_currentState == State.LoggedOut);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Username/password tidak boleh kosong.");
                return;
            }

            await _controller.RegisterAsync(username, password);
        }

        public async Task<bool> TryLoginAsync(string username, string password)
        {
            Contract.Requires(_currentState == State.LoggedOut);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Username/password tidak boleh kosong.");
                return false;
            }

            var success = await _controller.TryLoginAsync(username, password);
            if (success)
            {
                _currentState = State.LoggedIn;
                _currentUser = username;
            }

            return success;
        }

        public async Task Logout()
        {
            Contract.Requires(_currentState == State.LoggedIn);

            if (_currentUser == null)
            {
                Console.WriteLine("Tidak ada user yang login.");
                return;
            }

            await _controller.LogoutAsync(_currentUser);
            _currentState = State.LoggedOut;
            _currentUser = null;
        }
    }
}
