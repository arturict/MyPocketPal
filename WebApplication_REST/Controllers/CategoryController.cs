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
    public class CategoryController : ControllerBase
    {
        private readonly string _connectionString;

        public CategoryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
        }

        [HttpGet]
        public IEnumerable<Category> Get()
        {
            List<Category> categories = new List<Category>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM Categories";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Category category = new Category
                            {
                                Id = Convert.ToInt32(reader["Id"].ToString()),
                                Name = reader["Name"].ToString(),
                            };

                            categories.Add(category);
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
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Categories (Name) VALUES (@Name)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", category.Name);

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
        [HttpGet("{categoryName}")]
        public ActionResult<int?> GetCategoryIdByName(string categoryName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT Id FROM Categories WHERE Name = @CategoryName";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryName", categoryName);

                        var categoryId = command.ExecuteScalar() as int?;

                        return Ok(categoryId);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Abrufen der Kategorie-ID: " + ex.Message);
            }
        }



    }
}
