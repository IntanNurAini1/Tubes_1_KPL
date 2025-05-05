using System.Text.Json;

namespace Tubes_1_KPL.Model
{
    public static class UserDataHelper
    {
        private static readonly string FilePath = Path.Combine("Data", "users.json");

        public static List<User> LoadUsers()
        {
            if (!File.Exists(FilePath))
                return new List<User>();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public static void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}