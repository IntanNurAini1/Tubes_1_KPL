using System.Text.Json;

namespace Tubes_1_KPL.Model
{
    public class ReminderRule
    {
        public int DaysBefore { get; set; }
        public string Message { get; set; }
    }

    public class ReminderConfig
    {
        public List<ReminderRule> ReminderRules { get; set; }

        public static ReminderConfig LoadFromJson(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[DEBUG] File konfigurasi tidak ditemukan: {path}");
                return new ReminderConfig { ReminderRules = new List<ReminderRule>() };
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ReminderConfig>(json);
        }
    }
}
