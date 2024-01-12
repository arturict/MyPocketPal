using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using WebApplication_REST.Models;

namespace WebApplication_REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                            var settings = new Settings
                            {
                                UserId = currentUserId,
                                Currency = reader["Currency"].ToString(),
                                ShowWarnings = (bool)reader["ShowWarnings"],
                                NotificationsEnabled = (bool)reader["NotificationsEnabled"],
                                MonthlyBudget = reader["MonthlyBudget"] == DBNull.Value ? (decimal?)null : (decimal)reader["MonthlyBudget"]
                            };
                            return Ok(settings);
                        }
                    }
                }
            }
            return NotFound("Einstellungen nicht gefunden.");
        }




        [HttpGet("currencies")]
        public ActionResult<IEnumerable<string>> GetCurrencies()
        {
            List<string> currencies = new List<string>
    {
        "EUR - Euro (EUR)",
        "USD - US-Dollar (USD)",
        "CHF - Franken (CHF)",
        "AUD - Australische Dollar (AUD)",
        "BRL - Brasilianische Real (BRL)"
        
    };

            return currencies;
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
                var query = "UPDATE Settings SET Currency = @Currency, ShowWarnings = @ShowWarnings, " +
                            "NotificationsEnabled = @NotificationsEnabled, MonthlyBudget = @MonthlyBudget " +
                            "WHERE UserId = @UserId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", currentUserId);
                    command.Parameters.AddWithValue("@Currency", settings.Currency);
                    command.Parameters.AddWithValue("@ShowWarnings", settings.ShowWarnings);
                    command.Parameters.AddWithValue("@NotificationsEnabled", settings.NotificationsEnabled);
                    command.Parameters.AddWithValue("@MonthlyBudget", settings.MonthlyBudget);

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
                var checkQuery = "SELECT COUNT(*) FROM Settings WHERE UserId = @UserId";
                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserId", currentUserId);
                    var exists = (int)checkCommand.ExecuteScalar() > 0;
                    if (exists)
                    {
                        return Conflict("Einstellungen für diesen Benutzer existieren bereits.");
                    }
                }

                var insertQuery = "INSERT INTO Settings (UserId, Currency, ShowWarnings, NotificationsEnabled) VALUES (@UserId, 'CHF', 1, 0)";
                using (var insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@UserId", currentUserId);
                    int rowsAffected = insertCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Ok("Standard-Einstellungen erstellt.");
                    }
                }
            }
            return Problem("Fehler beim Erstellen der Standard-Einstellungen.");
        }

    }
}
