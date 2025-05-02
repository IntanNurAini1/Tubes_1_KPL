using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Tubes_1_KPL.Controller
{
    public class LoginRegisterController
    {
        private readonly HttpClient _http;

        public LoginRegisterController()
        {
            _http = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };
            Contract.Invariant(_http != null); 
        }

        public async Task RegisterAsync()
        {
            try
            {
                Console.Write("Username: ");
                var username = Console.ReadLine()?.Trim();
                Console.Write("Password: ");
                var password = Console.ReadLine()?.Trim();

                Contract.Requires(!string.IsNullOrEmpty(username), "Username tidak boleh kosong.");
                Contract.Requires(!string.IsNullOrEmpty(password), "Password tidak boleh kosong.");

                var user = new { Username = username, Password = password };
                Debug.WriteLine($"[DEBUG] Registering user: {username}");

                var response = await _http.PostAsJsonAsync("User/register", user);

                if (response == null)
                {
                    Console.WriteLine("Terjadi kesalahan saat mengirim request.");
                    return;
                }

                Console.WriteLine(response.IsSuccessStatusCode
                    ? "Registrasi berhasil."
                    : $"Registrasi gagal: {await response.Content.ReadAsStringAsync()}");

                Contract.Ensures(response != null, "Response tidak boleh null setelah register.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terjadi error saat registrasi: {ex.Message}");
                Debug.WriteLine($"[ERROR] RegisterAsync: {ex}");
            }
        }

        public async Task<bool> TryLoginAsync(string username, string password)
        {
            try
            {
                Contract.Requires(!string.IsNullOrEmpty(username), "Username tidak boleh kosong.");
                Contract.Requires(!string.IsNullOrEmpty(password), "Password tidak boleh kosong.");

                var user = new { Username = username, Password = password };
                Debug.WriteLine($"[DEBUG] Attempting login for user: {username}");

                var response = await _http.PostAsJsonAsync("User/login", user);

                if (response == null)
                {
                    Console.WriteLine("Terjadi kesalahan saat mengirim request login.");
                    return false;
                }

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Login berhasil: {username}");
                    return true;
                }
                else
                {
                    Console.WriteLine("Login gagal.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terjadi error saat login: {ex.Message}");
                Debug.WriteLine($"[ERROR] TryLoginAsync: {ex}");
                return false;
            }
        }

        public async Task LogoutAsync(string username)
        {
            try
            {
                Contract.Requires(!string.IsNullOrEmpty(username), "Username tidak boleh kosong.");
                Debug.WriteLine($"[DEBUG] Attempting logout for user: {username}");

                var response = await _http.PostAsync($"User/logout/{username}", null);

                if (response == null)
                {
                    Console.WriteLine("Terjadi kesalahan saat mengirim request logout.");
                    return;
                }

                Console.WriteLine(response.IsSuccessStatusCode
                    ? $"Logout berhasil: {username}"
                    : "Logout gagal.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terjadi error saat logout: {ex.Message}");
                Debug.WriteLine($"[ERROR] LogoutAsync: {ex}");
            }
        }
    }
}
