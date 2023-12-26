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
    public class TransactionController : ControllerBase
    {
        private readonly string _connectionString; // Verbindungszeichenfolge zur Datenbank

        public TransactionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
        }

        [HttpGet("{userId}")]
        public IEnumerable<Transaction> Get(int userId)
        {
            List<Transaction> transactions = new List<Transaction>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM Transactions WHERE UserId = @UserId";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Transaction transaction = new Transaction
                            {
                                Id = Convert.ToInt32(reader["Id"].ToString()),
                                Date = DateTime.Parse(reader["Date"].ToString()),
                                Amount = decimal.Parse(reader["Amount"].ToString()),
                                Description = reader["Description"].ToString(),
                                CategoryId = Convert.ToInt32(reader["CategoryId"].ToString()),
                                UserId = Convert.ToInt32(reader["UserId"].ToString()) // Setzen Sie die UserId entsprechend
                            };

                            transactions.Add(transaction);
                        }
                    }
                }

                connection.Close();
            }

            return transactions;
        }


        [HttpPost]
        public ActionResult<Transaction> Post(Transaction transaction)
        {
            try
            {
                // Ermittle die UserId des aktuellen Benutzers aus dem Authentifizierungstoken
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
                }

                // Setze die UserId für die Transaktion
                transaction.UserId = currentUserId;

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Transactions (Date, Amount, Description, CategoryId, UserId) VALUES (@Date, @Amount, @Description, @CategoryId, @UserId)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Date", transaction.Date);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@Description", transaction.Description);
                        command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
                        command.Parameters.AddWithValue("@UserId", transaction.UserId); // Verwende die UserId

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Erstellen der Transaktion: " + ex.Message);
            }
        }

        [HttpPut]
        public ActionResult<Transaction> Put(Transaction transaction)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "UPDATE Transactions SET Date = @Date, Amount = @Amount, Description = @Description WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", transaction.Id);
                        command.Parameters.AddWithValue("@Date", transaction.Date);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@Description", transaction.Description);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Transaktion nicht gefunden");
                        }
                    }

                    connection.Close();
                }

                return Ok(transaction); // Rückgabe der aktualisierten Transaktion mit Statuscode 200 (OK).
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Aktualisieren der Transaktion: " + ex.Message);
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

                    string sqlQuery = "DELETE FROM Transactions WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Transaktion nicht gefunden");
                        }
                    }

                    connection.Close();
                }

                return NoContent(); // Erfolgreiche Löschung mit Statuscode 204 (NoContent).
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Löschen der Transaktion: " + ex.Message);
            }
        }
        [HttpGet("user/{userId}")]
        public ActionResult<IEnumerable<Transaction>> GetTransactionsByUserId(int userId)
        {
            List<Transaction> userTransactions = new List<Transaction>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT * FROM Transactions WHERE UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Transaction transaction = new Transaction
                                {
                                    Id = Convert.ToInt32(reader["Id"].ToString()),
                                    Date = DateTime.Parse(reader["Date"].ToString()),
                                    Amount = decimal.Parse(reader["Amount"].ToString()),
                                    Description = reader["Description"].ToString(),
                                };
                                userTransactions.Add(transaction);
                            }
                        }
                    }
                    connection.Close();
                }
                if (!userTransactions.Any())
                {
                    return NotFound("No transactions found for the provided user ID.");
                }
                return Ok(userTransactions);
            }
            catch (Exception ex)
            {
                return BadRequest("Error while retrieving transactions: " + ex.Message);
            }
        }

    }
}
