using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tubes_1_KPL.Controller;
using Tubes_1_KPL.Model;
using ModelTask = API.Model.Task;
using static Tubes_1_KPL.Controller.TaskCreator;

internal class Program
{
    private static string _loggedInUser = null;
    private static TaskCreator _taskCreator = null;

    static async Task Main()
    {
        var automata = new LoginRegisterAutomata();

        while (true)
        {
            Console.WriteLine("Pilih opsi:");
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Exit");
            Console.Write("Pilih: ");
            var choice = Console.ReadLine();
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5263/api/") };

            switch (choice)
            {
                case "1":
                    await automata.Register();
                    break;

                case "2":
                    Console.Write("Masukkan username: ");
                    var usernameLogin = Console.ReadLine();
                    Console.Write("Masukkan password: ");
                    var passwordLogin = Console.ReadLine();
                    bool loginSuccessful = await automata.TryLoginAsync(usernameLogin, passwordLogin);
                    if (loginSuccessful)
                    {
                        _loggedInUser = usernameLogin;
                        _taskCreator = new TaskCreator(_loggedInUser, httpClient);
                        Console.WriteLine($"Berhasil login sebagai: {_loggedInUser}");

                        while (_loggedInUser != null)
                        {
                            string configPath = "JSON/reminder_config.json";
                            ReminderConfig reminderConfig = ReminderConfig.LoadFromJson(configPath);
                            _taskCreator.ShowReminders(reminderConfig);

                            Console.WriteLine("\nPilih opsi:");
                            Console.WriteLine("1. Buat Tugas");
                            Console.WriteLine("2. Edit Tugas");
                            Console.WriteLine("3. Delete Tugas");
                            Console.WriteLine("4. Tandai Tugas Selesai");
                            Console.WriteLine("5. Tugas Ongoing");
                            Console.WriteLine("6. Tugas Deadline");
                            Console.WriteLine("7. Tugas Complete");
                            Console.WriteLine("8. Logout");
                            Console.Write("Pilih: ");
                            var taskChoice = Console.ReadLine();

                            switch (taskChoice)
                            {
                                case "1":
                                    Console.Write("Nama tugas: ");
                                    string name = Console.ReadLine();

                                    Console.Write("Deskripsi tugas: ");
                                    string description = Console.ReadLine();

                                    Console.Write("Tanggal (1-31): ");
                                    if (int.TryParse(Console.ReadLine(), out int day))
                                    {
                                        Console.Write("Bulan (misalnya, januari): ");
                                        string month = Console.ReadLine();
                                        Console.Write("Tahun: ");
                                        if (int.TryParse(Console.ReadLine(), out int year))
                                        {
                                            Console.Write("Jam (0-23): ");
                                            if (int.TryParse(Console.ReadLine(), out int hour))
                                            {
                                                Console.Write("Menit (0-59): ");
                                                if (int.TryParse(Console.ReadLine(), out int minute))
                                                {
                                                    await _taskCreator.CreateTaskAsync(name, description, day, month, year, hour, minute);
                                                }
                                                else { Console.WriteLine("Format menit tidak valid."); }
                                            }
                                            else { Console.WriteLine("Format jam tidak valid."); }
                                        }
                                        else { Console.WriteLine("Format tahun tidak valid."); }
                                    }
                                    else { Console.WriteLine("Format hari tidak valid."); }
                                    break;

                                case "2":
                                    Console.Write("Masukkan nama tugas yang ingin diubah: ");
                                    string oldTaskName = Console.ReadLine();

                                    Console.Write("Nama tugas baru: ");
                                    string newName = Console.ReadLine();

                                    Console.Write("Deskripsi tugas baru: ");
                                    string newDescription = Console.ReadLine();

                                    Console.Write("Tanggal baru (1-31): ");
                                    if (int.TryParse(Console.ReadLine(), out int newDay))
                                    {
                                        Console.Write("Bulan baru (misalnya, januari): ");
                                        string newMonth = Console.ReadLine();

                                        Console.Write("Tahun baru: ");
                                        if (int.TryParse(Console.ReadLine(), out int newYear))
                                        {
                                            Console.Write("Jam baru (0-23): ");
                                            if (int.TryParse(Console.ReadLine(), out int newHour))
                                            {
                                                Console.Write("Menit baru (0-59): ");
                                                if (int.TryParse(Console.ReadLine(), out int newMinute))
                                                {
                                                    await _taskCreator.EditTask(oldTaskName, newName, newDescription, newDay, newMonth, newYear, newHour, newMinute);
                                                }
                                                else { Console.WriteLine("Format menit tidak valid."); }
                                            }
                                            else { Console.WriteLine("Format jam tidak valid."); }
                                        }
                                        else { Console.WriteLine("Format tahun tidak valid."); }
                                    }
                                    else { Console.WriteLine("Format hari tidak valid."); }
                                    break;

                                case "3":
                                    Console.Write("Masukkan nama tugas yang ingin dihapus: ");
                                    string taskNameToDelete = Console.ReadLine();
                                    var taskAutomata = new TaskAutomata(_loggedInUser, _taskCreator);
                                    await taskAutomata.ExecuteDeleteTask(taskNameToDelete);
                                    break;

                                case "4":
                                    Console.WriteLine("\n=== Tandai Tugas Selesai ===");
                                    Console.Write("Masukkan nama tugas yang ingin ditandai selesai: ");
                                    string taskNameToComplete = Console.ReadLine() ?? "";

                                    Console.Write("Apakah tugas ini sudah selesai? (yes/no): ");
                                    string answer = Console.ReadLine() ?? "";
                                    await _taskCreator.MarkTaskAsCompleted(taskNameToComplete, answer);
                                    break;

                                case "5":
                                    Console.WriteLine("\n=== Tugas Ongoing ===");
                                    try
                                    {
                                        var ongoingTasks = await _taskCreator.GetOngoingTasksAsync();
                                        if (ongoingTasks.Count > 0)
                                        {
                                            Console.WriteLine($"Tugas ongoing untuk {_loggedInUser}:");
                                            foreach (var task in ongoingTasks)
                                            {
                                                Console.WriteLine(task.ToString());
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Tidak ada tugas ongoing.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Terjadi kesalahan: {ex.Message}");
                                    }
                                    break;

                                case "6":
                                    Console.WriteLine("\n=== Tugas Deadline ===");
                                    try
                                    {
                                        var overdueTasks = await _taskCreator.GetOverdueTasksAsync();
                                        if (overdueTasks.Count > 0)
                                        {
                                            Console.WriteLine($"Tugas deadline untuk {_loggedInUser}:");
                                            foreach (var task in overdueTasks)
                                            {
                                                Console.WriteLine(task.ToString());
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Tidak ada tugas yang sudah melewati deadline.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Terjadi kesalahan: {ex.Message}");
                                    }
                                    break;

                                case "7":
                                    Console.WriteLine("\n=== Tugas Complete ===");
                                    try
                                    {
                                        var completedTasks = await _taskCreator.GetCompletedTasksAsync();
                                        if (completedTasks.Count > 0)
                                        {
                                            Console.WriteLine($"Tugas complete untuk {_loggedInUser}:");
                                            foreach (var task in completedTasks)
                                            {
                                                Console.WriteLine(task.ToString());
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Tidak ada tugas yang sudah selesai.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Terjadi kesalahan: {ex.Message}");
                                    }
                                    break;

                                case "8":
                                    await automata.Logout();
                                    _loggedInUser = null;
                                    _taskCreator = null;
                                    Console.WriteLine("Berhasil logout.");
                                    break;

                                default:
                                    Console.WriteLine("Pilihan tidak valid.");
                                    break;
                            }

                            Console.WriteLine();
                        }
                    }
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine("Pilihan tidak valid.");
                    break;
            }

            Console.WriteLine();
        }
    }
}
