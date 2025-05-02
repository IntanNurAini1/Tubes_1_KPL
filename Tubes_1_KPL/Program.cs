using System;
using System.Threading.Tasks;
using Tubes_1_KPL.Model;

internal class Program
{
    static async Task Main()
    {
        var automata = new LoginRegisterAutomata();

        while (true)
        {
            Console.WriteLine("Pilih opsi:");
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Logout");
            Console.WriteLine("4. Exit");
            Console.Write("Pilih: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await automata.Register();
                    break;
                case "2":
                    // Prompt user for username and password
                    Console.Write("Masukkan username: ");
                    var username = Console.ReadLine();
                    Console.Write("Masukkan password: ");
                    var password = Console.ReadLine();
                    await automata.Login(username, password);
                    break;
                case "3":
                    await automata.Logout();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Pilihan tidak valid.");
                    break;
            }

            Console.WriteLine();
        }
    }
}
