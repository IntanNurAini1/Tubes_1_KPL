using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Tubes_1_KPL.Controller;

namespace Tubes_1_KPL.Model
{
    public class LoginRegisterAutomata
    {
        private enum State
        {
            LoggedOut,
            LoggedIn
        }

        private State _currentState;
        private readonly LoginRegisterController _controller;
        private string? _currentUser;

        public LoginRegisterAutomata()
        {
            _currentState = State.LoggedOut;
            _controller = new LoginRegisterController();
        }

        public async Task Register()
        {
            Contract.Requires(_currentState == State.LoggedOut, "Register hanya boleh saat LoggedOut");
            Debug.WriteLine($"[DEBUG] Current State: {_currentState}, Action: Register");
            await _controller.RegisterAsync();
        }

        public async Task Login()
        {
            Contract.Requires(_currentState == State.LoggedOut, "Login hanya boleh saat LoggedOut");
            Debug.WriteLine($"[DEBUG] Current State: {_currentState}, Action: Login");

            Console.Write("Username: ");
            var username = Console.ReadLine()?.Trim();
            Console.Write("Password: ");
            var password = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Username/password tidak boleh kosong.");
                return;
            }

            var success = await _controller.TryLoginAsync(username, password);
            if (success)
            {
                _currentState = State.LoggedIn;
                _currentUser = username;
            }
        }

        public async Task Logout()
        {
            Contract.Requires(_currentState == State.LoggedIn, "Logout hanya boleh saat LoggedIn");
            Debug.WriteLine($"[DEBUG] Current State: {_currentState}, Action: Logout");

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
