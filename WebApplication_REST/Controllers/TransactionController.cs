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
        private readonly string _connectionString;  

        public TransactionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
        }

        [HttpGet]
        public ActionResult<IEnumerable<TransactionGet>> Get()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }

            try
            {
                List<TransactionGet> transactions = new List<TransactionGet>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT t.*, c.Name as CategoryName FROM Transactions t LEFT JOIN Categories c ON t.CategoryId = c.Id WHERE t.UserId = @UserId";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TransactionGet transaction = new TransactionGet
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                                    IsIncome = reader.GetBoolean(reader.GetOrdinal("IsIncome")),
                                    UserId = userId
                                };

                                transactions.Add(transaction);
                            }
                        }
                    }
                }

                return transactions;
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler aufgetreten: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
        }

        [HttpGet("category/{categoryId}")]
        public ActionResult<IEnumerable<TransactionGet>> GetTransactionsByCategory(int categoryId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }

            try
            {
                List<TransactionGet> transactions = new List<TransactionGet>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT t.*, c.Name as CategoryName, c.IsIncome FROM Transactions t LEFT JOIN Categories c ON t.CategoryId = c.Id WHERE t.UserId = @UserId AND t.CategoryId = @CategoryId";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@CategoryId", categoryId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TransactionGet transaction = new TransactionGet
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                                    IsIncome = reader.GetBoolean(reader.GetOrdinal("IsIncome")), 
                                    UserId = userId
                                };

                                transactions.Add(transaction);
                            }


                        }
                    }
                }

                return transactions;
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
        }
        [HttpGet("transaction")]
        public ActionResult<IEnumerable<TransactionGet>> GetTransactions(bool? isIncome = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }

            try
            {
                List<TransactionGet> transactions = new List<TransactionGet>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT t.*, c.Name as CategoryName, c.IsIncome FROM Transactions t LEFT JOIN Categories c ON t.CategoryId = c.Id WHERE t.UserId = @UserId";
                    if (isIncome.HasValue)
                    {
                        sqlQuery += " AND c.IsIncome = @IsIncome";
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
                                TransactionGet transaction = new TransactionGet
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                                    IsIncome = reader.GetBoolean(reader.GetOrdinal("IsIncome")),
                                    UserId = userId
                                };

                                transactions.Add(transaction);
                            }
                        }
                    }
                }

                return transactions;
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
        }



        [HttpPost]
        public ActionResult<Transaction> Post(Transaction transaction)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
                {
                    return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
                }
                if (transaction.Date == null || transaction.Date == DateTime.MinValue)
                {
                    transaction.Date = DateTime.Now;
                }
                transaction.UserId = currentUserId;

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Transactions (Date, Amount, Description, CategoryId, UserId, IsIncome) VALUES (@Date, @Amount, @Description, @CategoryId, @UserId, @IsIncome)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Date", transaction.Date);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@Description", transaction.Description);
                        command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
                        command.Parameters.AddWithValue("@UserId", transaction.UserId);
                        command.Parameters.AddWithValue("@IsIncome", transaction.IsIncome);

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

                return Ok(transaction); 
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Aktualisieren der Transaktion: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "DELETE FROM Transactions WHERE Id = @Id AND UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@UserId", currentUserId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Transaktion mit der angegebenen ID für den aktuellen Benutzer nicht gefunden.");
                        }
                    }
                }

                return NoContent();
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler aufgetreten: " + ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Unbekannter Fehler beim Löschen der Transaktion: " + ex.Message);
            }
        }




    }
}
