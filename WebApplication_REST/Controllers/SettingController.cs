using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using WebApplication_REST.Models;


public class SettingsController : ControllerBase
{
    private readonly string _connectionString;

    public SettingsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MyDbConnection");
    }

    [HttpGet]
    public ActionResult<Settings> GetSettings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
        {
            return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT * FROM Settings WHERE UserId = @UserId";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", currentUserId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new Settings
                        {
                            UserId = currentUserId,
                            Currency = reader["Currency"].ToString(),
                            ShowWarnings = (bool)reader["ShowWarnings"],
                            NotificationsEnabled = (bool)reader["NotificationsEnabled"]
                        });
                    }
                }
            }
        }
        return NotFound("Einstellungen nicht gefunden.");
    }

    [HttpPut]
    public IActionResult UpdateSettings([FromBody] Settings settings)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
        {
            return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var query = "UPDATE Settings SET Currency = @Currency, ShowWarnings = @ShowWarnings, NotificationsEnabled = @NotificationsEnabled WHERE UserId = @UserId";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", currentUserId);
                command.Parameters.AddWithValue("@Currency", settings.Currency);
                command.Parameters.AddWithValue("@ShowWarnings", settings.ShowWarnings);
                command.Parameters.AddWithValue("@NotificationsEnabled", settings.NotificationsEnabled);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Einstellungen aktualisiert.");
                }
            }
        }
        return NotFound("Einstellungen nicht gefunden.");
    }

    [HttpPost("create")]
    public IActionResult CreateDefaultSettings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
        {
            return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var query = "INSERT INTO Settings (UserId, Currency, ShowWarnings, NotificationsEnabled) VALUES (@UserId, 'EUR', 1, 0)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", currentUserId);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Standard-Einstellungen erstellt.");
                }
            }
        }
        return Problem("Fehler beim Erstellen der Standard-Einstellungen.");
    }


}

