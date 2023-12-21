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

        [HttpPost]
        public ActionResult<User> Post(User user)
        {
            try
            {
                user.Id = Guid.NewGuid(); // Eindeutige ID für den Benutzer.

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Users (Id, Username, Email) VALUES (@Id, @Username, @Email)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", user.Id);
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Email", user.Email);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return Ok(user); // Rückgabe des erstellten Benutzers mit Statuscode 200 (OK).
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Erstellen des Benutzers: " + ex.Message); // Rückgabe eines Fehlerstatuscodes mit einer Fehlermeldung.
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
        public ActionResult Delete(Guid id)
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
    }
}
