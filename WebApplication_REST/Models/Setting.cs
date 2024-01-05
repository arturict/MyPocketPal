namespace WebApplication_REST.Models
{
    public class Settings
    {
        public Guid UserId { get; set; }
        public string Currency { get; set; }
        public bool ShowWarnings { get; set; } = true;
        public bool NotificationsEnabled { get; set; } = false;

        // Weitere Einstellungen können hier hinzugefügt werden
    }
}
