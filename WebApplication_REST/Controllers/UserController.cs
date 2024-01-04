using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication_REST.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace WebApplication_REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("session")]
        public IActionResult SessionTest()
        {
            // Speichern von Daten in der Session
            HttpContext.Session.SetString("SessionKey", "SessionValue");

            // Lesen von Daten aus der Session
            var sessionValue = HttpContext.Session.GetString("SessionKey");

            return Ok(new { message = $"Session-Wert: {sessionValue}" });
        }
        private readonly string _connectionString;
        private readonly PasswordHasher<IdentityUser> _hasher;


        public UserController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
            _hasher = new PasswordHasher<IdentityUser>();

        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Entfernen der Benutzeridentität und löschen aller Cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Session-Daten löschen
            HttpContext.Session.Clear(); // Löscht alle Session-Daten

            return Ok(new { message = "Logout erfolgreich." });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                User user = null;

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sqlQuery = "SELECT Id, Username, Email, Password FROM Users WHERE Username = @Username";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", loginModel.username);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var passwordHash = reader["Password"].ToString();
                                if (VerifyPasswordHash(loginModel.password, passwordHash))
                                {
                                    user = new User
                                    {
                                        Id = Guid.Parse(reader["Id"].ToString()),
                                        Username = reader["Username"].ToString(),
                                        Email = reader["Email"].ToString()
                                    };
                                }
                            }
                        }
                    }
                }

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Setzen Sie den Benutzerstatus in der Session.
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);

                    return Ok(new { message = "Login erfolgreich." });
                }

                return Unauthorized("Login fehlgeschlagen");
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Login: " + ex.Message);
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetUserStatus()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                return Ok(new { IsLoggedIn = true, UserId = userId, Username = username });
            }
            else
            {
                return Ok(new { IsLoggedIn = false });
            }
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel registerModel)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Überprüfen, ob der Benutzername bereits existiert
                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkUserCommand = new SqlCommand(checkUserQuery, connection))
                    {
                        checkUserCommand.Parameters.AddWithValue("@Username", registerModel.username);
                        int userExists = (int)await checkUserCommand.ExecuteScalarAsync();

                        if (userExists > 0)
                        {
                            return BadRequest("Benutzername bereits vergeben.");
                        }
                    }

                    // Benutzer registrieren
                    string sqlQuery = "INSERT INTO Users (Id, Username, Email, Password) VALUES (@Id, @Username, @Email, @PasswordHash)";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        var userId = Guid.NewGuid(); // Generiere eine neue GUID als Benutzer-ID
                        command.Parameters.AddWithValue("@Id", userId);
                        command.Parameters.AddWithValue("@Username", registerModel.username);
                        command.Parameters.AddWithValue("@Email", registerModel.email);
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword(registerModel.password));

                        await command.ExecuteNonQueryAsync();
                    }

                    return Ok("Registrierung erfolgreich.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
        }
        [HttpGet]
        public IEnumerable<User> Get()
        {
            List<User> users = new List<User>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM Users";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Id = Guid.Parse(reader["Id"].ToString()),
                                Username = reader["Username"].ToString(),
                                Email = reader["Email"].ToString()
                            };

                            users.Add(user);
                        }
                    }
                }

                connection.Close();
            }

            return users;
        }
        [HttpPut]
        public ActionResult<User> Put(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "UPDATE Users SET Username = @Username, Email = @Email WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", user.Id);
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Email", user.Email);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Benutzer nicht gefunden");
                        }
                    }

                    connection.Close();
                }

                return Ok(user); // Rückgabe des aktualisierten Benutzers mit Statuscode 200 (OK).
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Aktualisieren des Benutzers: " + ex.Message);
            }
        }
        [HttpPost]
        public ActionResult<User> Post(User user)
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Users ( Username, Email) VALUES ( @Username, @Email)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Email", user.Email);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Erstellen des Benutzers: " + ex.Message);
            }
        }
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            return _hasher.HashPassword(null, password);
        }

        // Hilfsfunktion, um das Passwort zu verifizieren
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            if (string.IsNullOrEmpty(storedHash))
                throw new ArgumentException("Stored hash cannot be null or empty.", nameof(storedHash));

            var result = _hasher.VerifyHashedPassword(null, storedHash, password);
            return result == PasswordVerificationResult.Success;
        }
        public class LoginModel
        {
            public string username { get; set; }
            public string password { get; set; }
        }
        public class RegisterModel
        {
            public string username { get; set; }
            public string email { get; set; }
            public string password { get; set; }

        }
    }
}
