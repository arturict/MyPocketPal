using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication_REST.Models;
using System.Security.Claims;

namespace WebApplication_REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly string _connectionString;

        public CategoryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
        }

        [HttpGet]
        public IEnumerable<Category> Get(bool? isIncome = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Enumerable.Empty<Category>();
            }

            List<Category> categories = new List<Category>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT Id, Name, IsIncome FROM Categories WHERE UserId = @UserId";
                if (isIncome.HasValue)
                {
                    sqlQuery += " AND IsIncome = @IsIncome";
                }

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    if (isIncome.HasValue)
                    {
                        command.Parameters.AddWithValue("@IsIncome", isIncome.Value);
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                IsIncome = Convert.ToBoolean(reader["IsIncome"]),
                                UserId = userId
                            });
                        }
                    }
                }

                connection.Close();
            }

            return categories;
        }





        [HttpPost]
        public ActionResult<Category> Post(Category category)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
                {
                    return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
                }

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Categories (Name, UserId, IsIncome) VALUES (@Name, @UserId, @IsIncome)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", category.Name);
                        command.Parameters.AddWithValue("@UserId", currentUserId);
                        command.Parameters.AddWithValue("@IsIncome", category.IsIncome);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Erstellen der Kategorie: " + ex.Message);
            }
        }


        [HttpGet("search/{categoryName}/{isIncome}")]
        public async Task<ActionResult<int>> SearchCategory(string categoryName, bool isIncome)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT Id FROM Categories WHERE Name = @Name AND UserId = @UserId AND IsIncome = @IsIncome";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", categoryName);
                    command.Parameters.AddWithValue("@UserId", currentUserId);
                    command.Parameters.AddWithValue("@IsIncome", isIncome);
                    var result = await command.ExecuteScalarAsync();

                    if (result != null)
                    {
                        return Ok((int)result);
                    }
                }
            }

            return NotFound("Kategorie nicht gefunden.");
        }



        [HttpPost("create")]
        public async Task<ActionResult> CreateCategory([FromBody] Category category)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
            }

            category.UserId = currentUserId;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "INSERT INTO Categories (Name, UserId, IsIncome) VALUES (@Name, @UserId, @IsIncome)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", category.Name);
                        command.Parameters.AddWithValue("@UserId", category.UserId);
                        command.Parameters.AddWithValue("@IsIncome", category.IsIncome);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok("Kategorie erstellt.");
            }
            catch (Exception ex)
            {
                // Hier wird der Fehler geloggt
                Console.WriteLine("Fehler beim Erstellen der Kategorie: " + ex.Message);
                return StatusCode(500, "Interner Serverfehler beim Erstellen der Kategorie.");
            }
        }






    }
}
