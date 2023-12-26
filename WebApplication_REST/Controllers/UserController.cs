using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication_REST.Models;

namespace WebApplication_REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;

        public UserController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
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
                                Id = Convert.ToInt32(reader["Id"].ToString()),
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


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "DELETE FROM Users WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Benutzer nicht gefunden");
                        }
                    }

                    connection.Close();
                }

                return NoContent(); // Erfolgreiche Löschung mit Statuscode 204 (NoContent).
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Löschen des Benutzers: " + ex.Message);
            }
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginModel loginModel)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT Id, Username, Email FROM Users WHERE Username = @Username AND Password = @Password";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", loginModel.username);
                        command.Parameters.AddWithValue("@Password", loginModel.password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                User user = new User
                                {
                                    Username = reader["Username"].ToString(),
                                    Email = reader["Email"].ToString()
                                };

                                return Ok(user); // Erfolgreicher Login
                            }
                        }
                    }
                }

                return Unauthorized(); // Login fehlgeschlagen
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Login: " + ex.Message);
            }
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterModel registerModel)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkUserCommand = new SqlCommand(checkUserQuery, connection))
                    {
                        checkUserCommand.Parameters.AddWithValue("@Username", registerModel.username);
                        int userExists = (int)checkUserCommand.ExecuteScalar();

                        if (userExists > 0)
                        {
                            return BadRequest("Benutzername bereits vergeben.");
                        }
                    }

                    string sqlQuery = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", registerModel.username);
                        command.Parameters.AddWithValue("@Email", registerModel.email);
                        command.Parameters.AddWithValue("@Password", registerModel.password); 

                        command.ExecuteNonQuery();
                    }

                    return Ok("Registrierung erfolgreich.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
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
